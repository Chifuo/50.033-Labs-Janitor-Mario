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
    [SerializeField] private IntVariable gameScore;
    private void Awake()
    {
        if (scoreText != null)
            scoreLabel = scoreText.GetComponent<TMP_Text>();
        if (deliveriesText != null)
            deliveriesLabel = deliveriesText.GetComponent<TMP_Text>();
        if (finalscoreText  != null)
            finalscoreLabel = finalscoreText.GetComponent<TMP_Text>();
    }

    private void Start()
    {
        SetScore(gameScore.Value);
        SetDeliveries(gameScore.Deliveries);
    }

    public void SetScore(int score)
    {
        if (scoreLabel != null)
            scoreLabel.text = $"Score: {score}";
    }

    public void SetFinalScore()
    {
        if (finalscoreLabel != null)
        {
            finalscoreLabel.text = $"Score: {gameScore.Value}";
        }
    }

    public void SetDeliveries(int count)
    {
        if (deliveriesLabel != null)
            deliveriesLabel.text = $"Goomba Deliveries: {count}";
    }
}
