using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("References")]
    public AircraftPhysics playerPhysics;
    public AircraftTargeting playerTargeting;
    public AircraftHealth playerHealth;

    [Header("Telemetry Displays")]
    public TextMeshProUGUI airspeedText;
    public TextMeshProUGUI altitudeText;
    public TextMeshProUGUI throttleText;
    public TextMeshProUGUI gForceText;
    public TextMeshProUGUI healthText;

    [Header("Combat Displays")]
    public RectTransform targetBox;
    public TextMeshProUGUI targetLockText;
    public Camera mainCamera;

    private void Update()
    {
        if (playerPhysics != null)
        {
            if (airspeedText != null) 
                airspeedText.text = $"SPD: {Mathf.RoundToInt(playerPhysics.Airspeed * 1.94384f)} KTS";
            if (altitudeText != null) 
                altitudeText.text = $"ALT: {Mathf.RoundToInt(playerPhysics.Altitude * 3.28084f)} FT";
            if (throttleText != null) 
                throttleText.text = $"THR: {Mathf.RoundToInt(playerPhysics.Throttle * 100f)}% {(playerPhysics.AfterburnerActive ? "[AB]" : "")}";
            if (gForceText != null) 
                gForceText.text = $"G: {playerPhysics.CurrentGForce:F1}";
        }

        if (playerHealth != null && healthText != null)
        {
            healthText.text = $"ARMOR: {Mathf.RoundToInt(playerHealth.CurrentHealth)}";
        }

        UpdateTargetBox();
    }

    private void UpdateTargetBox()
    {
        if (targetBox == null || playerTargeting == null) return;

        Transform target = playerTargeting.CurrentTarget;
        if (target == null || mainCamera == null)
        {
            targetBox.gameObject.SetActive(false);
            if (targetLockText != null) targetLockText.text = "";
            return;
        }

        Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position);
        if (screenPos.z > 0)
        {
            targetBox.gameObject.SetActive(true);
            targetBox.position = screenPos;

            if (targetLockText != null)
            {
                if (playerTargeting.IsLocked)
                    targetLockText.text = "LOCK";
                else if (playerTargeting.LockProgress > 0f)
                    targetLockText.text = $"TRACK {(int)(playerTargeting.LockProgress * 100)}%";
                else
                    targetLockText.text = "DETECTED";
            }
        }
        else
        {
            targetBox.gameObject.SetActive(false);
        }
    }
}
