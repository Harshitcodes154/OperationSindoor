using UnityEngine;
using Unity.Cinemachine;

public class AircraftCamera : MonoBehaviour
{
    [Header("Target Tracking")]
    public Transform targetAircraft;
    public Vector3 chaseOffset = new Vector3(0f, 3.5f, -12f);
    public Vector3 cockpitOffset = new Vector3(0f, 1.2f, 0.5f);
    public float positionDamping = 10f;
    public float rotationDamping = 8f;

    [Header("Dynamic FOV")]
    public Camera targetCam;
    public float baseFOV = 60f;
    public float highSpeedFOV = 78f;
    public float fovTransitionSpeed = 4f;

    public enum CameraView { Chase, Cockpit }
    public CameraView currentView = CameraView.Chase;

    private AircraftPhysics targetPhysics;

    private void Start()
    {
        if (targetAircraft != null)
            targetPhysics = targetAircraft.GetComponent<AircraftPhysics>();
        if (targetCam == null)
            targetCam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (targetAircraft == null) return;

        Vector3 desiredPosition;
        Quaternion desiredRotation;

        if (currentView == CameraView.Cockpit)
        {
            desiredPosition = targetAircraft.TransformPoint(cockpitOffset);
            desiredRotation = targetAircraft.rotation;
        }
        else
        {
            desiredPosition = targetAircraft.TransformPoint(chaseOffset);
            desiredRotation = Quaternion.LookRotation(targetAircraft.position - transform.position, targetAircraft.up);
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * positionDamping);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Time.deltaTime * rotationDamping);

        // Adjust FOV with speed/afterburner
        if (targetCam != null && targetPhysics != null)
        {
            float targetFOV = targetPhysics.AfterburnerActive ? highSpeedFOV : baseFOV;
            targetCam.fieldOfView = Mathf.Lerp(targetCam.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
        }
    }

    public void ToggleView()
    {
        currentView = (currentView == CameraView.Chase) ? CameraView.Cockpit : CameraView.Chase;
    }
}
