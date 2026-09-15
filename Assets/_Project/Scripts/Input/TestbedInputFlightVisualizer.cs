using UnityEngine;
using OperationSindoor.Core;

namespace OperationSindoor.Input
{
    /// <summary>
    /// Lightweight kinematic visualizer for Milestone 1.
    /// Demonstrates how the decoupled IPlayerInputProvider drives an aircraft's
    /// rotation and velocity in real-time before the full Rigidbody aerodynamic
    /// flight model is wired up in Milestone 2.
    /// </summary>
    [AddComponentMenu("Operation Sindoor/Debug/Testbed Input Flight Visualizer")]
    public class TestbedInputFlightVisualizer : MonoBehaviour
    {
        [Header("Input Provider")]
        [Tooltip("Source of flight inputs. Auto-fetches FlightInputReader if left empty.")]
        [SerializeField] private FlightInputReader inputReader;

        [Header("Kinematic Movement Tuning")]
        [Tooltip("Maximum pitch rotation rate in degrees per second.")]
        [SerializeField] private float pitchRate = 60f;

        [Tooltip("Maximum roll rotation rate in degrees per second.")]
        [SerializeField] private float rollRate = 120f;

        [Tooltip("Maximum yaw rotation rate in degrees per second.")]
        [SerializeField] private float yawRate = 35f;

        [Tooltip("Base flight speed at 100% military power in meters per second.")]
        [SerializeField] private float cruiseSpeed = 150f;

        [Tooltip("Afterburner speed multiplier.")]
        [SerializeField] private float afterburnerMultiplier = 1.6f;

        [Tooltip("Airbrake drag reduction multiplier.")]
        [SerializeField] private float airbrakeMultiplier = 0.5f;

        [Header("Visual Model (Optional)")]
        [Tooltip("Optional child transform to bank/tilt visually.")]
        [SerializeField] private Transform visualModelTransform;

        private void Awake()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<FlightInputReader>();
            }

            // Create a procedural aircraft placeholder if no visual model is assigned
            if (visualModelTransform == null && transform.childCount == 0)
            {
                CreateProceduralJetMesh();
            }
        }

        private void Update()
        {
            if (inputReader == null) return;

            FlightInputData input = inputReader.CurrentInput;
            float dt = Time.deltaTime;

            // 1. Angular rotation from pitch, roll, yaw
            float pitch = input.Pitch * pitchRate * dt;
            float roll = -input.Roll * rollRate * dt; // A banks left (-z), D banks right (+z)
            float yaw = input.Yaw * yawRate * dt;

            transform.Rotate(pitch, yaw, roll, Space.Self);

            // 2. Forward speed from throttle
            float speed = input.RawThrottle * cruiseSpeed;
            if (input.Afterburner) speed *= afterburnerMultiplier;
            if (input.Airbrake) speed *= airbrakeMultiplier;

            transform.position += transform.forward * (speed * dt);
        }

        private void CreateProceduralJetMesh()
        {
            GameObject jetRoot = new GameObject("VisualModel_JetPlaceholder");
            jetRoot.transform.SetParent(transform, false);
            visualModelTransform = jetRoot.transform;

            // Fuselage
            GameObject fuselage = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            fuselage.name = "Fuselage";
            fuselage.transform.SetParent(jetRoot.transform, false);
            fuselage.transform.localScale = new Vector3(1.2f, 4f, 1.2f);
            fuselage.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            DestroyImmediate(fuselage.GetComponent<Collider>());

            // Nose cone
            GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            nose.name = "NoseCone";
            nose.transform.SetParent(jetRoot.transform, false);
            nose.transform.localPosition = new Vector3(0f, 0f, 4.2f);
            nose.transform.localScale = new Vector3(0.7f, 1.5f, 0.7f);
            nose.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            DestroyImmediate(nose.GetComponent<Collider>());

            // Main Delta Wings
            GameObject wings = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wings.name = "DeltaWings";
            wings.transform.SetParent(jetRoot.transform, false);
            wings.transform.localPosition = new Vector3(0f, 0f, -0.5f);
            wings.transform.localScale = new Vector3(8f, 0.12f, 3.5f);
            DestroyImmediate(wings.GetComponent<Collider>());

            // Vertical Stabilizer (Tail fin)
            GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tail.name = "VerticalTailFin";
            tail.transform.SetParent(jetRoot.transform, false);
            tail.transform.localPosition = new Vector3(0f, 1.2f, -2.8f);
            tail.transform.localScale = new Vector3(0.12f, 2.2f, 1.8f);
            tail.transform.localRotation = Quaternion.Euler(-18f, 0f, 0f);
            DestroyImmediate(tail.GetComponent<Collider>());

            // Dark military gray material
            Material jetMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            jetMat.name = "Mat_JetPlaceholder";
            jetMat.color = new Color(0.25f, 0.3f, 0.35f);
            jetMat.SetFloat("_Smoothness", 0.6f);

            fuselage.GetComponent<MeshRenderer>().sharedMaterial = jetMat;
            nose.GetComponent<MeshRenderer>().sharedMaterial = jetMat;
            wings.GetComponent<MeshRenderer>().sharedMaterial = jetMat;
            tail.GetComponent<MeshRenderer>().sharedMaterial = jetMat;
        }
    }
}
