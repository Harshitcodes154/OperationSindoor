#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using OperationSindoor.Core;
using OperationSindoor.Input;

namespace OperationSindoor.Editor
{
    /// <summary>
    /// Custom Editor tool to automatically scaffold the Milestone 1 Testbed Scene in 1 click.
    /// Accessible via the Unity menu bar: Operation Sindoor -> Setup Milestone 1 Testbed Scene
    /// </summary>
    public static class TestbedSceneSetupMenu
    {
        private const string ScenePath = "Assets/_Project/Scenes/Testbed/Sandbox_Flight_M1.unity";

        [MenuItem("Operation Sindoor/Setup Milestone 1 Testbed Scene", false, 1)]
        public static void SetupTestbedScene()
        {
            // 1. Create a clean new scene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Setup Environment Builder
            GameObject envGo = new GameObject("Environment_Testbed");
            var envBuilder = envGo.AddComponent<TestbedEnvironmentBuilder>();
            envBuilder.BuildEnvironment();

            // 3. Setup Aircraft Testbed Root
            GameObject aircraftGo = new GameObject("Aircraft_M1_Testbed");
            aircraftGo.transform.position = new Vector3(0f, 100f, 0f);

            var inputReader = aircraftGo.AddComponent<FlightInputReader>();
            var telemetry = aircraftGo.AddComponent<TelemetryDebugDisplay>();
            var visualizer = aircraftGo.AddComponent<TestbedInputFlightVisualizer>();

            // Try to link the InputActionAsset if it exists
            string assetPath = "Assets/_Project/Settings/Input/FlightControls.inputactions";
            var actionAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(assetPath);
            if (actionAsset != null)
            {
                var serializedReader = new SerializedObject(inputReader);
                var prop = serializedReader.FindProperty("inputActionsAsset");
                if (prop != null)
                {
                    prop.objectReferenceValue = actionAsset;
                    serializedReader.ApplyModifiedProperties();
                }
            }

            // 4. Setup Camera
            GameObject cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var cam = cameraGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 15000f;
            cameraGo.AddComponent<AudioListener>();

            // Parent camera behind aircraft for basic chase preview
            cameraGo.transform.SetParent(aircraftGo.transform);
            cameraGo.transform.localPosition = new Vector3(0f, 4f, -16f);
            cameraGo.transform.localRotation = Quaternion.Euler(7f, 0f, 0f);

            // 5. Ensure directory exists and save scene
            string dir = System.IO.Path.GetDirectoryName(ScenePath);
            if (!System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            EditorSceneManager.SaveScene(newScene, ScenePath);
            AssetDatabase.Refresh();

            Selection.activeGameObject = aircraftGo;

            Debug.Log($"<color=#00ffc8><b>[Operation Sindoor]</b></color> Milestone 1 Testbed Scene created and saved to: <i>{ScenePath}</i>");
            Debug.Log("<color=#00ffc8><b>[Ready for Play Mode]</b></color> Press <b>Play</b> in the Unity Editor to test flight inputs (W/A/S/D, Q/E, Shift/Ctrl, Space, Tab) with real-time telemetry HUD!");
        }
    }
}
#endif
