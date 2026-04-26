using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Trainamari.Input
{
    /// <summary>
    /// Snapshot-style calibration recorder. Drop on a GameObject in a scene,
    /// remove other input scripts (or disable InputSanityTest), press Play.
    ///
    /// Workflow:
    ///   ← / →  : select which named position you're calibrating
    ///   SPACE  : capture the currently-pressed buttons into the selected slot
    ///   BACKSP : clear the selected slot
    ///   D      : dump a generated C# switch statement to the Console
    ///
    /// Hold the lever steady at the named position before pressing SPACE.
    /// The capture takes the EXACT current button bitmask, so let the lever
    /// settle and any flicker stop.
    /// </summary>
    public class CalibrationRecorder : MonoBehaviour
    {
        // The set of positions a one-handle Densha has. Edit if your controller differs.
        private static readonly string[] Positions = new[]
        {
            "N", "P1", "P2", "P3", "P4", "P5",
            "B0", "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8",
            "EMG", "FREE"
        };

        // Unity's legacy Input only knows button names 0..19. Anything higher throws.
        private const int MAX_LEGACY_BUTTON = 20;
        [SerializeField, Range(1, MAX_LEGACY_BUTTON)] private int buttonsToScan = MAX_LEGACY_BUTTON;

        private readonly Dictionary<string, int> captures = new(); // name -> bitmask
        private int selectedIndex = 0;
        private GUIStyle style;

        // Stability tracking: only capture when the bitmask has been unchanged for
        // at least stabilityWindow seconds. Prevents catching mid-flicker garbage.
        [SerializeField] private float stabilityWindow = 0.3f;
        private int lastObservedMask = -1;
        private float stableSince = 0f;

        private int CurrentBitmask()
        {
            int mask = 0;
            int limit = Mathf.Min(buttonsToScan, MAX_LEGACY_BUTTON);
            for (int i = 0; i < limit; i++)
                if (UnityEngine.Input.GetKey($"joystick 1 button {i}")) mask |= 1 << i;
            return mask;
        }

        private void Update()
        {
            // Track how long the live bitmask has been stable.
            int liveMask = CurrentBitmask();
            if (liveMask != lastObservedMask)
            {
                lastObservedMask = liveMask;
                stableSince = Time.time;
            }
            float stableFor = Time.time - stableSince;

            if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow))
                selectedIndex = (selectedIndex + 1) % Positions.Length;
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow))
                selectedIndex = (selectedIndex - 1 + Positions.Length) % Positions.Length;

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                if (stableFor < stabilityWindow)
                {
                    Debug.LogWarning($"[Calib] Lever still moving (stable for {stableFor:F2}s, need {stabilityWindow:F2}s). Hold steady and try again.");
                }
                else
                {
                    captures[Positions[selectedIndex]] = liveMask;
                    Debug.Log($"[Calib] Captured {Positions[selectedIndex]} = {DescribeMask(liveMask)}");
                    selectedIndex = (selectedIndex + 1) % Positions.Length;
                }
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Backspace))
            {
                captures.Remove(Positions[selectedIndex]);
                Debug.Log($"[Calib] Cleared {Positions[selectedIndex]}");
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.D))
                DumpCode();
        }

        private static string DescribeMask(int mask)
        {
            if (mask == 0) return "(none)";
            var sb = new StringBuilder();
            for (int i = 0; i < 32; i++)
                if ((mask & (1 << i)) != 0) sb.Append(i).Append(' ');
            return sb.ToString().TrimEnd();
        }

        private void DumpCode()
        {
            var sb = new StringBuilder();
            sb.AppendLine("// === Captured button bitmasks ===");
            foreach (var p in Positions)
            {
                if (captures.TryGetValue(p, out var m))
                    sb.AppendLine($"//   {p,-4} = 0x{m:X4}  ({DescribeMask(m)})");
                else
                    sb.AppendLine($"//   {p,-4} = (not captured)");
            }
            sb.AppendLine();
            sb.AppendLine("// Paste into TrainInput.ReadDenshaInput() (one-handle variant):");
            sb.AppendLine("int mask = 0;");
            sb.AppendLine("for (int i = 0; i < 24; i++)");
            sb.AppendLine("    if (UnityEngine.Input.GetKey($\"joystick 1 button {i}\")) mask |= 1 << i;");
            sb.AppendLine("switch (mask)");
            sb.AppendLine("{");
            // Power notches → throttle
            EmitCase(sb, "N", "Throttle = 0f; break;");
            for (int i = 1; i <= 5; i++)
                EmitCase(sb, "P" + i, $"Throttle = {i / 5f}f; break;");
            // Brake notches → negative throttle
            for (int i = 0; i <= 8; i++)
                EmitCase(sb, "B" + i, $"Throttle = {-i / 8f}f; break;");
            EmitCase(sb, "EMG", "Throttle = -1f; EmergencyBrake = true; break;");
            EmitCase(sb, "FREE", "Throttle = 0f; break;");
            sb.AppendLine("    default: break; // unrecognised — keep last value");
            sb.AppendLine("}");
            Debug.Log(sb.ToString());
        }

        private void EmitCase(StringBuilder sb, string posName, string body)
        {
            if (!captures.TryGetValue(posName, out var m)) return;
            sb.AppendLine($"    case 0x{m:X4}: /* {posName} */ {body}");
        }

        private void OnGUI()
        {
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label) { fontSize = 14, richText = true };
                style.normal.textColor = Color.white;
            }

            int liveMask = CurrentBitmask();
            float stableFor = Time.time - stableSince;
            bool ready = stableFor >= stabilityWindow;
            string readyTag = ready ? "<color=#00ff66>READY</color>" : $"<color=#ff8800>settling… {stableFor:F2}s</color>";

            GUILayout.BeginArea(new Rect(10, 10, 700, Screen.height - 20), GUI.skin.box);
            GUILayout.Label("<b>DENSHA CALIBRATION RECORDER</b>", style);
            GUILayout.Label($"Live buttons: {DescribeMask(liveMask)}    (mask 0x{liveMask:X4})    {readyTag}", style);
            GUILayout.Label("← →  select   SPACE  capture (only when READY)   BACKSPACE  clear   D  dump code", style);
            GUILayout.Space(8);

            for (int i = 0; i < Positions.Length; i++)
            {
                string p = Positions[i];
                string captured = captures.TryGetValue(p, out var m) ? DescribeMask(m) : "<color=#888>(empty)</color>";
                string marker = (i == selectedIndex) ? "<color=#ffd000>></color>" : " ";
                GUILayout.Label($"{marker} <b>{p,-4}</b>  {captured}", style);
            }

            GUILayout.EndArea();
        }
    }
}
