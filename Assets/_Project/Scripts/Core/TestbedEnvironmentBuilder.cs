using UnityEngine;

namespace OperationSindoor.Core
{
    /// <summary>
    /// Utility builder that proceduralizes a reference flight testbed in the scene.
    /// Creates a 10km grid ground plane, altitude reference pylons, realistic aerial lighting,
    /// and a geometric supersonic delta-wing aircraft mock to allow immediate flight inspection.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("Operation Sindoor/Core/Testbed Environment Builder")]
    public class TestbedEnvironmentBuilder : MonoBehaviour
    {
        [Header("Testbed Configuration")]
        [Tooltip("When enabled, builds the visual testbed elements on Start if they don't already exist.")]
        [SerializeField] private bool autoBuildOnStart = true;

        [Header("Environment Dimensions")]
        [SerializeField] private float groundSize = 10000f; // 10 km
        [SerializeField] private int pylonCount = 8;
        [SerializeField] private float pylonRadius = 1500f;

        private void Start()
        {
            if (autoBuildOnStart && !HasBuiltEnvironment())
            {
                BuildEnvironment();
            }
        }

        public bool HasBuiltEnvironment()
        {
            return transform.Find("Testbed_Ground") != null;
        }

        [ContextMenu("Build Testbed Environment")]
        public void BuildEnvironment()
        {
            // 1. Directional Sun Light
            Light sun = FindFirstObjectByType<Light>();
            if (sun == null)
            {
                GameObject sunGo = new GameObject("Sun_DirectionalLight");
                sunGo.transform.SetParent(transform);
                sunGo.transform.rotation = Quaternion.Euler(42f, -35f, 0f);
                sun = sunGo.AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.color = new Color(1.0f, 0.96f, 0.88f);
                sun.intensity = 1.3f;
                sun.shadows = LightShadows.Soft;
            }

            // 2. Ground Plane
            Transform existingGround = transform.Find("Testbed_Ground");
            if (existingGround == null)
            {
                GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "Testbed_Ground";
                ground.transform.SetParent(transform);
                ground.transform.position = Vector3.zero;
                ground.transform.localScale = new Vector3(groundSize / 10f, 1f, groundSize / 10f);

                var renderer = ground.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                    mat.name = "Mat_TestbedGrid";
                    mat.color = new Color(0.18f, 0.22f, 0.26f);
                    mat.SetFloat("_Smoothness", 0.1f);
                    renderer.sharedMaterial = mat;
                }
            }

            // 3. Altitude / Spatial Reference Pylons
            Transform existingPylons = transform.Find("Testbed_Pylons");
            if (existingPylons == null)
            {
                GameObject pylonGroup = new GameObject("Testbed_Pylons");
                pylonGroup.transform.SetParent(transform);

                for (int i = 0; i < pylonCount; i++)
                {
                    float angle = i * (360f / pylonCount) * Mathf.Deg2Rad;
                    Vector3 pos = new Vector3(Mathf.Cos(angle) * pylonRadius, 250f, Mathf.Sin(angle) * pylonRadius);

                    GameObject pylon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    pylon.name = $"Pylon_Marker_{i + 1}";
                    pylon.transform.SetParent(pylonGroup.transform);
                    pylon.transform.position = pos;
                    pylon.transform.localScale = new Vector3(15f, 250f, 15f);

                    var pylonRenderer = pylon.GetComponent<MeshRenderer>();
                    if (pylonRenderer != null)
                    {
                        Material pylonMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                        pylonMat.color = (i % 2 == 0) ? new Color(0.9f, 0.3f, 0.1f) : new Color(0.9f, 0.8f, 0.1f);
                        pylonRenderer.sharedMaterial = pylonMat;
                    }
                }
            }

            Debug.Log("[TestbedEnvironmentBuilder] Environment built successfully (10km terrain reference & altitude markers).");
        }

        [ContextMenu("Clear Testbed Environment")]
        public void ClearEnvironment()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
            Debug.Log("[TestbedEnvironmentBuilder] Testbed cleared.");
        }
    }
}
