using UnityEngine;

namespace Trainamari.Input
{
    /// <summary>
    /// Debug overlay for discovering how the Densha de Go controller (or any USB
    /// joystick adapter) maps to Unity's legacy Input system.
    ///
    /// Drop this on any GameObject in a scene, press Play, and wiggle the lever /
    /// press buttons. The on-screen readout shows:
    ///   - Detected joystick names
    ///   - Live values for joystick axes 1..10 (via Input Manager axis names you'll add)
    ///   - Which buttons (0..19) are currently pressed on joystick 1
    ///
    /// To populate the axis readouts you must add axes named "Joy1Axis1" .. "Joy1Axis10"
    /// in Project Settings > Input Manager. See README for the recipe.
    /// </summary>
    public class JoystickCalibration : MonoBehaviour
    {
        [SerializeField] private int axesToProbe = 10;
        [SerializeField] private int buttonsToProbe = 20;
        [SerializeField] private string axisNamePrefix = "Joy1Axis";

        private GUIStyle style;

        private void OnGUI()
        {
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label) { fontSize = 16, richText = true };
                style.normal.textColor = Color.white;
            }

            GUILayout.BeginArea(new Rect(20, 20, 700, Screen.height - 40), GUI.skin.box);
            GUILayout.Label("<b>JOYSTICK CALIBRATION</b>", style);

            string[] joys = UnityEngine.Input.GetJoystickNames();
            if (joys.Length == 0)
            {
                GUILayout.Label("No joysticks detected. Plug controller in and press Play again.", style);
            }
            else
            {
                for (int i = 0; i < joys.Length; i++)
                {
                    GUILayout.Label($"Joystick {i + 1}: \"{joys[i]}\"", style);
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>Axes (move the lever):</b>", style);
            for (int i = 1; i <= axesToProbe; i++)
            {
                string axisName = axisNamePrefix + i;
                float v = 0f;
                bool ok = true;
                try { v = UnityEngine.Input.GetAxis(axisName); }
                catch (System.ArgumentException) { ok = false; }

                if (!ok)
                    GUILayout.Label($"  {axisName}: <color=#888>not configured in Input Manager</color>", style);
                else
                {
                    string bar = MakeBar(v);
                    string color = Mathf.Abs(v) > 0.05f ? "#ffd000" : "#888";
                    GUILayout.Label($"  {axisName}: <color={color}>{v,+6:F2}</color>  {bar}", style);
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>Buttons currently pressed (joystick 1):</b>", style);
            string pressed = "";
            for (int i = 0; i < buttonsToProbe; i++)
            {
                if (UnityEngine.Input.GetKey($"joystick 1 button {i}"))
                    pressed += i + " ";
            }
            GUILayout.Label(string.IsNullOrEmpty(pressed) ? "  (none)" : "  " + pressed, style);

            GUILayout.EndArea();
        }

        private static string MakeBar(float v)
        {
            const int width = 20;
            int center = width / 2;
            int pos = Mathf.Clamp(center + Mathf.RoundToInt(v * center), 0, width);
            var s = new System.Text.StringBuilder("[");
            for (int i = 0; i < width; i++) s.Append(i == pos ? '#' : (i == center ? '|' : '-'));
            s.Append(']');
            return s.ToString();
        }
    }
}
