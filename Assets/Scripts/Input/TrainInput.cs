using UnityEngine;
using System;

namespace Trainamari.Input
{
    /// <summary>
    /// Unified input handler for train controls.
    /// Supports Densha de Go lever controller via DirectInput, keyboard, and gamepad.
    /// 
    /// Densha de Go controller (PS1 via USB adapter) maps as DirectInput device:
    /// - Y-axis: Brake/Accelerate lever positions
    /// - Buttons: Horn, Door Close, etc.
    /// 
    /// We abstract all inputs to a common interface so the TrainController
    /// doesn't care what device is being used.
    /// </summary>
    public class TrainInput : MonoBehaviour
    {
        [Header("Keyboard Controls")]
        [SerializeField] private KeyCode throttleUp = KeyCode.W;
        [SerializeField] private KeyCode throttleDown = KeyCode.S;
        [SerializeField] private KeyCode emergencyBrake = KeyCode.Space;
        [SerializeField] private KeyCode horn = KeyCode.A;
        [SerializeField] private KeyCode doorClose = KeyCode.D;
        
        [Header("Gamepad Controls")]
        [SerializeField] private string gamepadThrottleAxis = "Vertical";
        [SerializeField] private string gamepadHornButton = "joystick button 0";
        [SerializeField] private string gamepadDoorButton = "joystick button 1";
        
        [Header("Densha de Go Controller")]
        [SerializeField] private bool detectDenshaController = true;
        [SerializeField] private string denshaDeviceName = "Densha"; // partial match on joystick name
        [SerializeField] private bool forceDenshaMode = false;       // treat ANY joystick as Densha
        [SerializeField] private bool invertLever = false;
        [SerializeField] private int denshaHornButton = 8;           // Select on standard PS layout
        [SerializeField] private int denshaDoorButton = 9;           // Start on standard PS layout

        [Header("Densha Two-Handle Decoder (Taito SLPH-00051 via Bliss-Box)")]
        // Two-handle PS1 Densha calibrated 2026-04-26.
        // Master controller (power, 6 positions) uses bits 8, 17, 18.
        // Brake handle (B0..B8, EMG) uses bits 11, 12, 13, 14.
        // Captured masks live in the static lookup tables below.
        [SerializeField] private bool useTwoHandleDecoder = true;
        [SerializeField] private float notchStabilityWindow = 0.08f; // seconds the mask must hold before we trust it
        [SerializeField] private bool stickyEmergency = true;        // EMG persists through the FREE click past it

        // Decoded notch state (exposed for HUD / debugging)
        public int PowerNotch { get; private set; } = -1;   // 0..5, or -1 if pattern unrecognised
        public int BrakeNotch { get; private set; } = -1;   // 0..8, 9=EMG, 10=Free, -1 if unrecognised
        public int LastReadMask { get; private set; }       // raw button bitmask, for debugging
        
        // Output state
        public float Throttle { get; private set; }     // -1 (full brake) to 1 (full accelerate)
        public bool EmergencyBrake { get; private set; }
        public bool Horn { get; private set; }
        public bool DoorClose { get; private set; }
        public bool HornDown { get; private set; }       // true only on frame pressed
        public bool DoorCloseDown { get; private set; }  // true only on frame pressed
        
        // Controller detection
        public bool DenshaControllerConnected { get; private set; }
        public string ActiveControllerName { get; private set; } = "Keyboard";
        
        // Internal
        private float keyboardThrottle = 0f;
        private float keyboardThrottleSpeed = 2f; // how fast keyboard ramps throttle
        private bool prevHorn = false;
        private bool prevDoorClose = false;
        private bool loggedMissingAxis = false;
        private int lastPowerSlice = -1;
        private int lastBrakeSlice = -1;
        private float powerStableSince = 0f;
        private float brakeStableSince = 0f;
        
        private void Start()
        {
            // Force the editor (and built players) to keep polling while defocused.
            // Without this, alt-tabbing out of Unity drops joystick state — when
            // focus returns the brake mask reads 0x0000 (all released) for a few
            // frames before the actual lever bits come back through.
            Application.runInBackground = true;
            DetectControllers();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
            {
                // Joystick names list can change across focus events on macOS.
                // Re-enumerate so DenshaControllerConnected stays accurate.
                DetectControllers();
                // Reset stability tracking so the next stable mask gets accepted
                // promptly instead of waiting on the previous "0 buttons" reading.
                lastBrakeSlice = -1;
                lastPowerSlice = -1;
                brakeStableSince = Time.time;
                powerStableSince = Time.time;
            }
        }
        
        private void Update()
        {
            ReadKeyboardInput();
            ReadGamepadInput();
            
            if (DenshaControllerConnected)
            {
                ReadDenshaInput();
            }
            
            // Edge detection for button presses
            HornDown = Horn && !prevHorn;
            DoorCloseDown = DoorClose && !prevDoorClose;
            prevHorn = Horn;
            prevDoorClose = DoorClose;
        }
        
        private void DetectControllers()
        {
            string[] joystickNames = UnityEngine.Input.GetJoystickNames();
            DenshaControllerConnected = false;
            
            for (int i = 0; i < joystickNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(joystickNames[i]) && 
                    joystickNames[i].ToLower().Contains(denshaDeviceName.ToLower()))
                {
                    DenshaControllerConnected = true;
                    ActiveControllerName = joystickNames[i];
                    Debug.Log($"[TrainInput] Densha de Go controller detected: {joystickNames[i]} (joystick {i})");
                    return;
                }
            }
            
            // Check for any joystick as fallback gamepad
            for (int i = 0; i < joystickNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(joystickNames[i]))
                {
                    ActiveControllerName = joystickNames[i];
                    if (forceDenshaMode)
                    {
                        DenshaControllerConnected = true;
                        Debug.Log($"[TrainInput] forceDenshaMode: treating {joystickNames[i]} as Densha (joystick {i})");
                    }
                    else
                    {
                        Debug.Log($"[TrainInput] Gamepad detected: {joystickNames[i]} (joystick {i})");
                    }
                    return;
                }
            }

            Debug.Log("[TrainInput] No controller detected, using keyboard input");
        }
        
        private void ReadKeyboardInput()
        {
            // Keyboard throttle: hold to ramp up/down
            if (UnityEngine.Input.GetKey(throttleUp))
                keyboardThrottle = Mathf.Min(1f, keyboardThrottle + keyboardThrottleSpeed * Time.deltaTime);
            else if (UnityEngine.Input.GetKey(throttleDown))
                keyboardThrottle = Mathf.Max(-1f, keyboardThrottle - keyboardThrottleSpeed * Time.deltaTime);
            else
                keyboardThrottle = Mathf.MoveTowards(keyboardThrottle, 0f, keyboardThrottleSpeed * 0.5f * Time.deltaTime);
            
            if (!DenshaControllerConnected)
            {
                Throttle = keyboardThrottle;
                EmergencyBrake = UnityEngine.Input.GetKey(emergencyBrake);
                Horn = UnityEngine.Input.GetKey(horn);
                DoorClose = UnityEngine.Input.GetKey(doorClose);
            }
        }
        
        private void ReadGamepadInput()
        {
            if (DenshaControllerConnected) return;
            
            // Standard gamepad fallback
            float gamepadAxis = UnityEngine.Input.GetAxis(gamepadThrottleAxis);
            if (Mathf.Abs(gamepadAxis) > 0.1f)
            {
                Throttle = gamepadAxis;
                ActiveControllerName = "Gamepad";
            }
        }
        
        private void ReadDenshaInput()
        {
            // PS1 two-handle Densha de Go (SLPH-00051) encodes both handles as button
            // combinations. Read all 20 legacy buttons once, decode each handle from
            // its own bit slice independently.
            int mask = ReadButtonMask();
            LastReadMask = mask;

            // Stability filtering: only update the decoded notch when the SAME mask
            // has been observed for at least notchStabilityWindow seconds. This kills
            // the 0x7800 "all-bits-bridged" transit pattern that flashes between
            // every detent, plus any other in-flight noise. Hysteresis on the public
            // PowerNotch/BrakeNotch values means real detents stick.
            int powerSlice = mask & POWER_MASK;
            int brakeSlice = mask & BRAKE_MASK;
            if (powerSlice != lastPowerSlice) { lastPowerSlice = powerSlice; powerStableSince = Time.time; }
            if (brakeSlice != lastBrakeSlice) { lastBrakeSlice = brakeSlice; brakeStableSince = Time.time; }

            if (useTwoHandleDecoder && Time.time - powerStableSince >= notchStabilityWindow)
            {
                int newPower = DecodePowerNotch(mask);
                if (newPower >= 0) PowerNotch = newPower;
            }
            if (useTwoHandleDecoder && Time.time - brakeStableSince >= notchStabilityWindow)
            {
                int newBrake = DecodeBrakeNotch(mask);
                if (newBrake >= 0)
                {
                    // 0x0000 ambiguity resolution: mechanical EMG and FREE both produce
                    // mask 0x0000 on this hardware. Use history — if we just left a
                    // braking notch (B1..B8), the lever crossed into EMG territory.
                    // If stickyEmergency is on, also keep EMG when the user clicks
                    // past it onto FREE (real trains don't release EMG by overshoot).
                    int emgUpper = stickyEmergency ? 9 : 8;
                    if (brakeSlice == 0 && BrakeNotch >= 1 && BrakeNotch <= emgUpper)
                        BrakeNotch = 9; // Emergency
                    else
                        BrakeNotch = newBrake;
                }
                else
                {
                    // Unknown brake mask. Two distinct cases:
                    //   - 0x7800 = "all 4 bits bridged" transit pattern that appears
                    //     between every pair of detents. Ignore.
                    //   - Anything else with brake bits set = an intermediate position
                    //     in the smooth zone PAST B8, on the way to EMG (e.g. 0x1000,
                    //     0x1800, 0x2000, 0x2800, 0x0800). Only happens when we just
                    //     left B8, so treat as EMG without waiting to reach 0x0000.
                    if (brakeSlice != 0 && brakeSlice != 0x7800 && BrakeNotch == 8)
                        BrakeNotch = 9; // Emergency (lever parked between B8 and EMG)
                }
            }

            // Brake takes priority over power (true even on the real train).
            // BrakeNotch values: 0..8 = service brake, 9 = Emergency, 10 = Free (released).
            float t = 0f;
            bool emg = false;
            if (BrakeNotch >= 1 && BrakeNotch <= 8)
            {
                t = -(BrakeNotch / 8f);
            }
            else if (BrakeNotch == 9) // Emergency
            {
                t = -1f;
                emg = true;
            }
            else if (PowerNotch >= 1 && PowerNotch <= 5)
            {
                t = PowerNotch / 5f;
            }
            // BrakeNotch 0 (no brake), 10 (Free), or unrecognised: leave throttle at power value (or 0).

            if (invertLever) t = -t;
            Throttle = t;
            EmergencyBrake = emg || UnityEngine.Input.GetKey(emergencyBrake);

            Horn = UnityEngine.Input.GetKey($"joystick 1 button {denshaHornButton}");
            DoorClose = UnityEngine.Input.GetKey($"joystick 1 button {denshaDoorButton}");
        }

        // Bitmask of all buttons we care about for power decoding (bits 8, 17, 18).
        private const int POWER_MASK = (1 << 8) | (1 << 17) | (1 << 18);
        // Bitmask of all brake bits (11, 12, 13, 14).
        private const int BRAKE_MASK = (1 << 11) | (1 << 12) | (1 << 13) | (1 << 14);

        /// <summary>Read joystick 1 buttons 0..19 into a single bitmask.</summary>
        private int ReadButtonMask()
        {
            int m = 0;
            for (int i = 0; i < 20; i++)
                if (UnityEngine.Input.GetKey($"joystick 1 button {i}")) m |= 1 << i;
            return m;
        }

        /// <summary>Decode the master controller. Returns 0=N, 1..5=P1..P5, -1=unknown.</summary>
        private int DecodePowerNotch(int rawMask)
        {
            switch (rawMask & POWER_MASK)
            {
                case 0x60000: return 0; // bits 17, 18  → N
                case 0x40100: return 1; // bits 8, 18   → P1
                case 0x40000: return 2; // bit 18       → P2
                case 0x20100: return 3; // bits 8, 17   → P3
                case 0x20000: return 4; // bit 17       → P4
                case 0x00100: return 5; // bit 8        → P5
                default:      return -1;
            }
        }

        /// <summary>Decode the brake handle. Returns 1..8=B1..B8, 10=Free/B0/EMG, -1=unknown.</summary>
        private int DecodeBrakeNotch(int rawMask)
        {
            // SLPH-00051 brake encoding via Bliss-Box (recalibrated 2026-04-26 from
            // settled return-sweep values; original capture was off-by-one).
            // Note: 0x7800 (all 4 brake bits) appears constantly between detents as
            // contacts bridge in flight — NOT a real notch. Excluded from the table;
            // stability filtering in ReadDenshaInput drops it.
            switch (rawMask & BRAKE_MASK)
            {
                case 0x7000: return 10; // 12 13 14    — B0 (released, no force)
                case 0x5800: return 1;  // 11 12    14 — B1
                case 0x5000: return 2;  //    12    14 — B2
                case 0x6800: return 3;  // 11    13 14 — B3
                case 0x6000: return 4;  //       13 14 — B4
                case 0x4800: return 5;  // 11       14 — B5
                case 0x4000: return 6;  //          14 — B6
                case 0x3800: return 7;  // 11 12 13    — B7
                case 0x3000: return 8;  //    12 13    — B8 (max service brake)
                // 0x0000 = lever past EMG OR in free zone OR not engaged.
                // 0x7800 = momentary all-bits-bridged transit between detents.
                // Both are filtered out by stability check in caller.
                case 0x0000: return 10; // — Free / EMG (indistinguishable on this hw)
                default:     return -1; // mid-transition, ignore
            }
        }
        
        /// <summary>
        /// Force re-detect controllers (call if controller is hot-plugged).
        /// </summary>
        public void RedetectControllers()
        {
            DetectControllers();
        }
    }
}