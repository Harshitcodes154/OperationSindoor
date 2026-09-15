using UnityEngine;
using OperationSindoor.Core;

namespace OperationSindoor.Input
{
    /// <summary>
    /// Real-time diagnostic telemetry HUD for Milestone 1.
    /// Uses native OnGUI rendering so it works immediately out of the box with zero Canvas,
    /// Font, or EventSystem dependencies. Displays real-time control stick deflection,
    /// throttle percentage, trigger states, framerate, and keybindings.
    /// Toggle visibility in-game by pressing F1 or Tilde (~).
    /// </summary>
    [AddComponentMenu("Operation Sindoor/Debug/Telemetry Debug Display")]
    public class TelemetryDebugDisplay : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The FlightInputReader to read telemetry from. If left empty, will attempt GetComponent on this GameObject.")]
        [SerializeField] private FlightInputReader inputReader;

        [Header("Display Settings")]
        [SerializeField] private bool showTelemetry = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;

        // Performance counters
        private float frameCount;
        private float dtAccumulator;
        private float currentFps;
        private float currentFrameTimeMs;

        // Visual styles (cached)
        private GUIStyle headerStyle;
        private GUIStyle labelStyle;
        private GUIStyle valueStyle;
        private GUIStyle activeIndicatorStyle;
        private GUIStyle inactiveIndicatorStyle;
        private GUIStyle helpStyle;
        private Texture2D boxBackground;
        private Texture2D barBackground;
        private Texture2D barFill;
        private bool stylesInitialized = false;

        private void Awake()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<FlightInputReader>();
            }
        }

        private void Update()
        {
            // Toggle key
            if (UnityEngine.Input.GetKeyDown(toggleKey) || UnityEngine.Input.GetKeyDown(KeyCode.BackQuote))
            {
                showTelemetry = !showTelemetry;
            }

            // Calculate FPS
            frameCount++;
            dtAccumulator += Time.unscaledDeltaTime;
            if (dtAccumulator >= 0.5f)
            {
                currentFps = frameCount / dtAccumulator;
                currentFrameTimeMs = (dtAccumulator / frameCount) * 1000f;
                frameCount = 0f;
                dtAccumulator = 0f;
            }
        }

        private void InitializeStyles()
        {
            if (stylesInitialized) return;

            boxBackground = MakeColorTexture(new Color(0.04f, 0.08f, 0.12f, 0.88f));
            barBackground = MakeColorTexture(new Color(0.12f, 0.18f, 0.22f, 0.9f));
            barFill = MakeColorTexture(new Color(0.0f, 0.85f, 0.75f, 0.95f));

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            headerStyle.normal.textColor = new Color(0.1f, 0.95f, 0.85f);

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            labelStyle.normal.textColor = new Color(0.75f, 0.85f, 0.9f);

            valueStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight
            };
            valueStyle.normal.textColor = Color.white;

            activeIndicatorStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            activeIndicatorStyle.normal.background = MakeColorTexture(new Color(0.95f, 0.35f, 0.1f, 0.95f));
            activeIndicatorStyle.normal.textColor = Color.white;

            inactiveIndicatorStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 11,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };
            inactiveIndicatorStyle.normal.background = MakeColorTexture(new Color(0.15f, 0.2f, 0.25f, 0.6f));
            inactiveIndicatorStyle.normal.textColor = new Color(0.5f, 0.55f, 0.6f);

            helpStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Normal,
                wordWrap = true
            };
            helpStyle.normal.textColor = new Color(0.65f, 0.75f, 0.8f);

            stylesInitialized = true;
        }

        private Texture2D MakeColorTexture(Color col)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            return tex;
        }

        private void OnGUI()
        {
            if (!showTelemetry) return;
            InitializeStyles();

            FlightInputData input = inputReader != null ? inputReader.CurrentInput : FlightInputData.Zero;

            // Main Telemetry Panel
            Rect panelRect = new Rect(20, 20, 360, 480);
            GUI.DrawTexture(panelRect, boxBackground);

            // Border outline
            DrawRectBorder(panelRect, new Color(0.15f, 0.45f, 0.55f, 0.8f), 1);

            GUILayout.BeginArea(new Rect(panelRect.x + 14, panelRect.y + 12, panelRect.width - 28, panelRect.height - 24));

            // Header
            GUILayout.Label("OPERATION SINDOOR — TELEMETRY", headerStyle);
            GUILayout.Space(2);

            // Performance Line
            Color fpsColor = currentFps >= 55f ? Color.green : (currentFps >= 30f ? Color.yellow : Color.red);
            GUI.color = fpsColor;
            GUILayout.Label($"PERF: {currentFps:0.0} FPS  |  {currentFrameTimeMs:0.0} ms  |  (F1 to toggle)", labelStyle);
            GUI.color = Color.white;

            GUILayout.Space(8);

            // Axis Displays
            DrawAxisGauge("PITCH (W/S):", input.Pitch, -1f, 1f);
            DrawAxisGauge("ROLL  (A/D):", input.Roll, -1f, 1f);
            DrawAxisGauge("YAW   (Q/E):", input.Yaw, -1f, 1f);
            DrawAxisGauge("THROTTLE (Shift/Ctrl):", input.RawThrottle, 0f, 1f, isPercent: true);

            GUILayout.Space(10);
            GUILayout.Label("SYSTEM & WEAPON STATES", headerStyle);
            GUILayout.Space(4);

            // Indicators Grid
            GUILayout.BeginHorizontal();
            DrawIndicator("AIRBRAKE [Space]", input.Airbrake, new Color(0.9f, 0.2f, 0.2f));
            DrawIndicator("BURNER [Tab]", input.Afterburner, new Color(0.0f, 0.7f, 1.0f));
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            DrawIndicator("GUNS [LMB]", input.FirePrimary, new Color(1.0f, 0.8f, 0.1f));
            DrawIndicator("MISSILE [F/RMB]", input.FireMissile, new Color(1.0f, 0.3f, 0.1f));
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            DrawIndicator("TARGET LOCK [R]", input.TargetLock, new Color(0.2f, 0.9f, 0.3f));
            DrawIndicator("FLARES [X]", input.Countermeasures, new Color(1.0f, 0.5f, 0.0f));
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            DrawIndicator("CAM TOGGLE [C]", input.SwitchCamera, new Color(0.7f, 0.3f, 0.9f));
            DrawIndicator("PAUSE [Esc]", input.Pause, new Color(0.8f, 0.8f, 0.8f));
            GUILayout.EndHorizontal();

            GUILayout.Space(12);
            GUILayout.Label("CONTROLS REFERENCE", headerStyle);
            GUILayout.Label("• W/S : Pitch Down / Pitch Up\n• A/D : Bank Left / Bank Right\n• Q/E : Rudder Left / Rudder Right\n• Shift / Ctrl : Throttle Up / Down\n• Space : Airbrake  |  Tab : Afterburner\n• LMB / F : Fire Guns / Missile  |  R : Lock  |  C : Camera", helpStyle);

            GUILayout.EndArea();
        }

        private void DrawAxisGauge(string label, float value, float min, float max, bool isPercent = false)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, labelStyle, GUILayout.Width(170));
            string displayVal = isPercent ? $"{(value * 100f):0.0}%" : $"{value:+0.00;-0.00; 0.00}";
            GUILayout.Label(displayVal, valueStyle, GUILayout.Width(65));
            GUILayout.EndHorizontal();

            // Progress bar
            Rect barRect = GUILayoutUtility.GetRect(200, 10);
            GUI.DrawTexture(barRect, barBackground);

            float normalized;
            if (min < 0)
            {
                // Centered axis (-1 to +1)
                normalized = Mathf.InverseLerp(-1f, 1f, value);
                float midX = barRect.x + (barRect.width * 0.5f);
                float fillWidth = (normalized - 0.5f) * barRect.width;

                Rect fillRect = fillWidth >= 0
                    ? new Rect(midX, barRect.y, fillWidth, barRect.height)
                    : new Rect(midX + fillWidth, barRect.y, -fillWidth, barRect.height);

                GUI.DrawTexture(fillRect, barFill);
            }
            else
            {
                // Unipolar axis (0 to 1)
                normalized = Mathf.Clamp01(value);
                Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * normalized, barRect.height);
                GUI.DrawTexture(fillRect, barFill);
            }

            GUILayout.Space(4);
        }

        private void DrawIndicator(string text, bool isActive, Color activeColor)
        {
            GUIStyle style = isActive ? activeIndicatorStyle : inactiveIndicatorStyle;
            Color oldBg = GUI.backgroundColor;
            if (isActive) GUI.backgroundColor = activeColor;
            GUILayout.Box(text, style, GUILayout.Height(24), GUILayout.ExpandWidth(true));
            GUI.backgroundColor = oldBg;
        }

        private void DrawRectBorder(Rect r, Color color, int thickness)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y + r.height - thickness, r.width, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y, thickness, r.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - thickness, r.y, thickness, r.height), Texture2D.whiteTexture);
            GUI.color = old;
        }
    }
}
