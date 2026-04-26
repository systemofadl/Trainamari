using System.Collections.Generic;
using UnityEngine;
using Trainamari.Input;
using Trainamari.Core;

namespace Trainamari.Train
{
    /// <summary>
    /// Cheap free-roam train physics for the sandbox: momentum, acceleration from
    /// power notch, deceleration from brake notch, no track required. Drop on the
    /// cube alongside TrainInput. Replaces InputSanityTest for "feel" testing.
    /// </summary>
    public class SimpleTrainPhysics : MonoBehaviour
    {
        [SerializeField] private TrainInput input;

        [Header("Tuning (km/h converted to units/sec at runtime)")]
        [SerializeField] private float maxSpeedKmh = 120f;
        // Acceleration per unit throttle, in km/h per second (so P5 = +5 km/h/s gain).
        [SerializeField] private float accelKmhPerSec = 6f;
        // Service brake deceleration at B8 (full), in km/h per second.
        [SerializeField] private float brakeKmhPerSec = 18f;
        // Coasting friction with throttle = 0 and brake = 0.
        [SerializeField] private float coastFrictionKmhPerSec = 1.5f;
        // Emergency brake: very strong, can also kill power for a moment.
        [SerializeField] private float emergencyBrakeKmhPerSec = 35f;

        public float CurrentSpeedKmh { get; private set; }

        // Track which brake notches we've actually seen this session. Reaching
        // 3+ distinct values means contacts have warmed up enough to drive on.
        private readonly HashSet<int> seenBrakeNotches = new();
        private const int WarmupThreshold = 3;

        private void Reset()
        {
            input = GetComponent<TrainInput>();
        }

        private void Update()
        {
            if (input == null) return;

            // Track which brake notches the lever has visited this session.
            if (input.BrakeNotch >= 0) seenBrakeNotches.Add(input.BrakeNotch);

            // Convert "throttle" back into power vs brake intent so we can apply
            // proper momentum: positive throttle = power notch (accelerate),
            // negative throttle = service brake (decelerate, never reverse).
            // Run in Update (not FixedUpdate) so motion is smooth per-frame —
            // we have no Rigidbody so fixed timestep gives no benefit.
            float t = input.Throttle;          // -1..+1 from decoder
            float dt = Time.deltaTime;

            if (input.EmergencyBrake)
            {
                CurrentSpeedKmh = Mathf.MoveTowards(CurrentSpeedKmh, 0f, emergencyBrakeKmhPerSec * dt);
            }
            else if (t > 0.01f)
            {
                // Power notch — accelerate up to max speed.
                CurrentSpeedKmh = Mathf.MoveTowards(CurrentSpeedKmh, maxSpeedKmh, accelKmhPerSec * t * dt);
            }
            else if (t < -0.01f)
            {
                // Service brake — decelerate but never below 0.
                CurrentSpeedKmh = Mathf.MoveTowards(CurrentSpeedKmh, 0f, brakeKmhPerSec * (-t) * dt);
            }
            else
            {
                // Coast.
                CurrentSpeedKmh = Mathf.MoveTowards(CurrentSpeedKmh, 0f, coastFrictionKmhPerSec * dt);
            }

            // Move forward at current speed. km/h → m/s = / 3.6.
            float metersPerSecond = CurrentSpeedKmh / 3.6f;
            transform.position += transform.forward * metersPerSecond * dt;
        }

        private void OnGUI()
        {
            if (input == null) return;
            var style = new GUIStyle(GUI.skin.label) { fontSize = 16 };
            style.normal.textColor = Color.white;

            string buttons = "";
            for (int i = 0; i < 20; i++)
                if (UnityEngine.Input.GetKey($"joystick 1 button {i}")) buttons += i + " ";

            int powerBits = input.LastReadMask & 0x60100;
            int brakeBits = input.LastReadMask & 0x07800;

            GUILayout.BeginArea(new Rect(10, 10, 700, 240), GUI.skin.box);
            GUILayout.Label($"Controller: {input.ActiveControllerName}", style);
            GUILayout.Label($"Speed: {CurrentSpeedKmh:F1} km/h ({CurrentSpeedKmh / 3.6f:F1} m/s)", style);
            GUILayout.Label($"Throttle input: {input.Throttle:+0.00;-0.00;0.00}   Emergency: {input.EmergencyBrake}", style);
            GUILayout.Label($"Power: {NotchLabel(input.PowerNotch, "P", "N")}   (mask 0x{powerBits:X5})", style);
            GUILayout.Label($"Brake: {BrakeLabel(input.BrakeNotch)}   (mask 0x{brakeBits:X5})", style);
            GUILayout.Label($"Buttons: {(buttons.Length == 0 ? "(none)" : buttons)}", style);
            GUILayout.EndArea();

            // Warmup banner: prompts the user to sweep the brake until the contacts
            // bridge cleanly. 30-year-old copper pads need a few sweeps to settle.
            if (seenBrakeNotches.Count < WarmupThreshold)
            {
                var banner = new GUIStyle(GUI.skin.box) { fontSize = 24, alignment = TextAnchor.MiddleCenter };
                banner.normal.textColor = Color.yellow;
                float w = 700, h = 80;
                GUI.Box(new Rect((Screen.width - w) * 0.5f, 80, w, h),
                    $"SWEEP BRAKE LEVER B0 → EMG → B0 TO CALIBRATE\n({seenBrakeNotches.Count} / {WarmupThreshold} notches detected)",
                    banner);
            }
        }

        // Public hook so a future scene-loaded event can re-arm the warmup prompt.
        public void ResetWarmup() => seenBrakeNotches.Clear();

        private static string NotchLabel(int n, string prefix, string zero)
        {
            if (n < 0) return "(unknown)";
            if (n == 0) return zero;
            return prefix + n;
        }

        private static string BrakeLabel(int n)
        {
            if (n < 0) return "(unknown)";
            if (n == 10) return "B0/Free";
            if (n == 9) return "EMG";
            return "B" + n;
        }
    }
}
