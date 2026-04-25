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
        // Names of axes configured in Project Settings > Input Manager.
        // Add an axis named "DenshaLever" mapped to the joystick axis your adapter exposes
        // (use the JoystickCalibration scene to find which one).
        [SerializeField] private string denshaLeverAxisName = "DenshaLever";
        [SerializeField] private bool invertLever = false;
        [SerializeField] private int denshaHornButton = 0;
        [SerializeField] private int denshaDoorButton = 1;
        
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
        
        private void Start()
        {
            DetectControllers();
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
                    Debug.Log($"[TrainInput] Gamepad detected: {joystickNames[i]} (joystick {i})");
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
            // Densha de Go lever reads as a single axis
            // Center = neutral, up = accelerate, down = brake
            // The exact mapping depends on the USB adapter, but typically:
            //   Axis value ~0 = neutral
            //   Axis value >0 = accelerate (positions 1-5)
            //   Axis value <0 = brake (positions 1-8 + emergency)
            
            float leverValue = 0f;
            try
            {
                leverValue = UnityEngine.Input.GetAxis(denshaLeverAxisName);
            }
            catch (System.ArgumentException)
            {
                // Axis isn't configured in the Input Manager yet. Fall back to keyboard
                // and warn once so the user knows to set it up.
                if (!loggedMissingAxis)
                {
                    Debug.LogWarning($"[TrainInput] Input Manager axis '{denshaLeverAxisName}' is not configured. " +
                                     "Open Project Settings > Input Manager and add it, or use the JoystickCalibration scene.");
                    loggedMissingAxis = true;
                }
                Throttle = keyboardThrottle;
                return;
            }

            if (invertLever) leverValue = -leverValue;

            // Dead zone for neutral
            Throttle = (Mathf.Abs(leverValue) < 0.1f) ? 0f : leverValue;

            // Emergency brake: lever pulled all the way back OR keyboard fallback
            EmergencyBrake = leverValue <= -0.95f || UnityEngine.Input.GetKey(emergencyBrake);

            // Buttons on the Densha controller — these string forms ARE valid legacy Input names.
            Horn = UnityEngine.Input.GetKey($"joystick 1 button {denshaHornButton}");
            DoorClose = UnityEngine.Input.GetKey($"joystick 1 button {denshaDoorButton}");
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