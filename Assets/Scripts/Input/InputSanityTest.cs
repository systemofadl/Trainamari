using UnityEngine;

namespace Trainamari.Input
{
    /// <summary>
    /// Tiny end-to-end sanity check: reads TrainInput.Throttle and moves this
    /// GameObject forward/backward. Drop on a Cube, put a TrainInput on the same
    /// (or another) GameObject, hit Play. W speeds up, S slows / reverses, Space
    /// is emergency brake. Proves the input pipeline works without needing a
    /// track spline, scoring system, or any of the rest of the game wired up.
    /// </summary>
    public class InputSanityTest : MonoBehaviour
    {
        [SerializeField] private TrainInput input;
        [SerializeField] private float maxSpeed = 3f; // slow enough to watch
        [SerializeField] private bool clampToOrigin = false;

        private GUIStyle bigStyle;

        private void Reset()
        {
            input = GetComponent<TrainInput>();
        }

        private void Update()
        {
            if (input == null) return;
            float v = input.EmergencyBrake ? 0f : input.Throttle * maxSpeed;
            transform.position += transform.forward * v * Time.deltaTime;
            if (clampToOrigin) transform.position = Vector3.zero; // for input debugging without losing the cube
        }

        private void OnGUI()
        {
            if (bigStyle == null)
            {
                bigStyle = new GUIStyle(GUI.skin.label) { fontSize = 16 };
                bigStyle.normal.textColor = Color.white;
            }

            if (input == null) return;

            string buttons = "";
            for (int i = 0; i < 20; i++)
                if (UnityEngine.Input.GetKey($"joystick 1 button {i}")) buttons += i + " ";

            int powerBits = input.LastReadMask & 0x60100;
            int brakeBits = input.LastReadMask & 0x07800;

            GUILayout.BeginArea(new Rect(10, 10, 700, 240), GUI.skin.box);
            GUILayout.Label($"Controller: {input.ActiveControllerName}", bigStyle);
            GUILayout.Label($"Throttle: {input.Throttle:+0.00;-0.00;0.00}   Emergency: {input.EmergencyBrake}", bigStyle);
            GUILayout.Label($"PowerNotch: {Notch(input.PowerNotch, "P")}   (mask 0x{powerBits:X5})", bigStyle);
            GUILayout.Label($"BrakeNotch: {BrakeLabel(input.BrakeNotch)}   (mask 0x{brakeBits:X5})", bigStyle);
            GUILayout.Label($"Buttons pressed: {(buttons.Length == 0 ? "(none)" : buttons)}", bigStyle);
            GUILayout.Label($"Position: {transform.position.x:F1}, {transform.position.z:F1}   Speed: {input.Throttle * maxSpeed:F2} u/s", bigStyle);
            GUILayout.EndArea();
        }

        private static string Notch(int n, string prefix)
        {
            if (n < 0) return "(unknown)";
            if (n == 0) return "N";
            return prefix + n;
        }

        private static string BrakeLabel(int n)
        {
            if (n < 0) return "(unknown)";
            if (n == 0) return "B0";
            if (n == 9) return "EMG";
            if (n == 10) return "Free";
            return "B" + n;
        }
    }
}
