using UnityEngine;

public class CollectCoin : MonoBehaviour 
{
    [Min(0f)] private float CoinLifespan = 10f;
    private float CoinCountdown;
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        CoinCountdown = CoinLifespan;
    }
    private void Update()
    {
        if (gameManager != null && gameManager.IsGameOver)
        {
            return;
        }
        if (gameManager != null && CoinCountdown <= 0f)
        {
            Destroy(gameObject);
        }
        Decay();
    }

    private void Decay()
    {
        CoinCountdown = Mathf.Max(0f, CoinCountdown - Time.deltaTime);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(gameObject);
        gameManager.IncreaseScore(10);
    }

}
