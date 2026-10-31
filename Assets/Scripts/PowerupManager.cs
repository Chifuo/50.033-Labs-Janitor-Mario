using UnityEngine;
using UnityEngine.Events;

public class PowerupManager : MonoBehaviour 
{
    public GameEvent<int> onScoreIncreased;
    public UnityEvent onPlayerSpeedup;
    [SerializeField] private IntVariable gameScore;
    public enum PowerupType { Coin, Speedup }
    public PowerupType PowerupPrefab;
    [Min(0f)] private float PowerupLifespan = 10f;
    private float PowerupCountdown;

    private void Awake()
    {
        PowerupCountdown = PowerupLifespan;
    }
    private void Update()
    {
        if (gameScore.GameOver)
            return;
        if (PowerupCountdown <= 0f)
        {
            Destroy(gameObject);
        }
        Decay();
    }
    private void Decay()
    {
        PowerupCountdown = Mathf.Max(0f, PowerupCountdown - Time.deltaTime);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PowerupEffect();
            Destroy(gameObject);
        }
    }

    private void PowerupEffect()
    {
        switch (PowerupPrefab)
        {
            case PowerupType.Coin:
                CoinEffect();
                break;
            case PowerupType.Speedup:
                SpeedupEffect();
                break;
        } 

    }

    private void CoinEffect()
    {
        onScoreIncreased.Raise(10);
    }

    private void SpeedupEffect()
    {
        onPlayerSpeedup.Invoke();
    }

}
