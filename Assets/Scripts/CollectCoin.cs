using UnityEngine;

public class CollectCoin : MonoBehaviour 
{
    [Min(0f)] private float CoinLifespan = 10f;
    private float CoinCountdown;

    private void Awake()
    {
        CoinCountdown = CoinLifespan;
    }
    private void Update()
    {
        if (GameManager.instance != null && GameManager.instance.IsGameOver)
        {
            return;
        }
        if (GameManager.instance != null && CoinCountdown <= 0f)
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
        GameManager.instance.IncreaseScore(10);
    }

}
