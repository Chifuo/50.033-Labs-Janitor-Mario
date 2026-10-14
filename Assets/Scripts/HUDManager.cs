using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public GameObject scoreText;
    public GameObject deliveriesText;
    public GameObject finalscoreText;

    private TMP_Text scoreLabel;
    private TMP_Text deliveriesLabel;
    private TMP_Text finalscoreLabel;

    private void Awake()
    {
        if (scoreText != null)
            scoreLabel = scoreText.GetComponent<TMP_Text>();
        if (deliveriesText != null)
            deliveriesLabel = deliveriesText.GetComponent<TMP_Text>();
        if (finalscoreText  != null)
            finalscoreLabel = finalscoreText.GetComponent<TMP_Text>();
        SetScore(GameManager.instance.Score);
        SetDeliveries(GameManager.instance.GoombaDeliveries);

        GameManager.instance.scoreChange.AddListener(SetScore);
        GameManager.instance.deliveryCountChange.AddListener(SetDeliveries);
        GameManager.instance.gameOver.AddListener(SetFinalScore);
    }

    public void SetScore(int score)
    {
        if (scoreLabel != null)
            scoreLabel.text = $"Score: {score}";
    }

    public void SetFinalScore()
    {
        if(finalscoreLabel != null)
            finalscoreLabel.text = $"Score: {GameManager.instance.Score}";
    }

    public void SetDeliveries(int count)
    {
        if (deliveriesLabel != null)
            deliveriesLabel.text = $"Goomba Deliveries: {count}";
    }
}
