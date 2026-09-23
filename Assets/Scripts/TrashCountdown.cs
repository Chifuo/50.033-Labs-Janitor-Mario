using TMPro;
using UnityEngine;

public class TrashCountdown : MonoBehaviour
{
    public CollectTrash trash;
    public TMP_Text countdownText;
    public SpriteRenderer gear;

    private Quaternion gearRotation;

    private void Awake()
    {
        if (gear != null)
            gearRotation = gear.transform.localRotation;
    }

    private void LateUpdate()
    {
        bool visible = trash != null && trash.isActiveAndEnabled && trash.CanPlayerCollect;
        if (countdownText != null)
        {
            countdownText.enabled = visible;
            if (visible)
                countdownText.text = $"{trash.RemainingTime:0.0}s";
        }

        if (gear != null)
        {
            gear.enabled = visible;
            float progress = visible && trash.collectionDuration > 0f
                ? 1f - Mathf.Clamp01(trash.RemainingTime / trash.collectionDuration)
                : 0f;
            gear.transform.localRotation = gearRotation * Quaternion.Euler(0f, 0f, -360f * progress);
        }
    }

    private void OnDisable()
    {
        if (countdownText != null)
            countdownText.enabled = false;
        if (gear != null)
        {
            gear.enabled = false;
            gear.transform.localRotation = gearRotation;
        }
    }
}
