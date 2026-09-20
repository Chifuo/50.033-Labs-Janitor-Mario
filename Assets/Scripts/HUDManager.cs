using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public GameObject scoreText;
    public GameObject deliveriesText;

    private TMP_Text scoreLabel;
    private TMP_Text deliveriesLabel;

    private void Awake()
    {
        if (scoreText != null)
            scoreLabel = scoreText.GetComponent<TMP_Text>();
        if (deliveriesText != null)
            deliveriesLabel = deliveriesText.GetComponent<TMP_Text>();
    }

    public void SetScore(int score)
    {
        if (scoreLabel != null)
            scoreLabel.text = $"Score: {score}";
    }

    public void SetDeliveries(int count)
    {
        if (deliveriesLabel != null)
            deliveriesLabel.text = $"Goomba Deliveries: {count}";
    }
}
