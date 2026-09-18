using UnityEngine;
using UnityEngine.UI;

public class RadarDisplay : MonoBehaviour
{
    public Transform playerTransform;
    public RectTransform radarPanel;
    public GameObject blipPrefab;
    public float radarRadiusMeters = 5000f;
    public float displayRadiusPixels = 100f;

    private void Update()
    {
        if (playerTransform == null || blipPrefab == null || radarPanel == null) return;

        // Clear previous frame blips
        foreach (Transform child in radarPanel)
        {
            Destroy(child.gameObject);
        }

        var hostiles = GameObject.FindGameObjectsWithTag("Hostile");
        foreach (var hostile in hostiles)
        {
            Vector3 offset = hostile.transform.position - playerTransform.position;
            if (offset.magnitude <= radarRadiusMeters)
            {
                // Rotate offset to match player heading
                Vector3 localOffset = Quaternion.Euler(0f, -playerTransform.eulerAngles.y, 0f) * offset;

                float normX = localOffset.x / radarRadiusMeters;
                float normY = localOffset.z / radarRadiusMeters;

                Vector2 blipPos = new Vector2(normX, normY) * displayRadiusPixels;

                GameObject blip = Instantiate(blipPrefab, radarPanel);
                RectTransform rt = blip.GetComponent<RectTransform>();
                rt.anchoredPosition = blipPos;
            }
        }
    }
}
