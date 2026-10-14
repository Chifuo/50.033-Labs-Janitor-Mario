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
    private GameManager manager;

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
        manager = GameManager.instance;
        manager.scoreChange.AddListener(SetScore);
        manager.deliveryCountChange.AddListener(SetDeliveries);
        manager.gameOver.AddListener(SetFinalScore);
        SetScore(manager.Score);
        SetDeliveries(manager.GoombaDeliveries);
    }

    private void OnDestroy()
    {
        if (manager == null)
            return;
        manager.scoreChange.RemoveListener(SetScore);
        manager.deliveryCountChange.RemoveListener(SetDeliveries);
        manager.gameOver.RemoveListener(SetFinalScore);
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
