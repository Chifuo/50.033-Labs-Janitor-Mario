using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public UnityEvent gameStart = new UnityEvent();
    public UnityEvent gameOver = new UnityEvent();
    public UnityEvent gameRestart = new UnityEvent();
    public UnityEvent<int> scoreChange = new UnityEvent<int>();
    public UnityEvent<int> deliveryCountChange = new UnityEvent<int>();
    [Min(1)] public int deliveriesToLose = 3;

    public bool IsGameOver { get; private set; }
    public int GoombaDeliveries { get; private set; }

    private int score = 0;

    private void Start()
    {
        gameStart.Invoke();
        SetScore(score);
        deliveryCountChange.Invoke(GoombaDeliveries);
    }
    public void GameRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void IncreaseScore(int increment)
    {
        if (IsGameOver)
            return;
        score += increment;
        SetScore(score);
    }
    public void SetScore(int score)
    {
        scoreChange.Invoke(score);
    }
    public void GameOver()
    {
        if (IsGameOver)
            return;
        IsGameOver = true;
        gameOver.Invoke();
    }

    public void RecordGoombaDelivery()
    {
        if (IsGameOver)
            return;

        GoombaDeliveries++;
        deliveryCountChange.Invoke(GoombaDeliveries);
        if (GoombaDeliveries >= Mathf.Max(1, deliveriesToLose))
            GameOver();
    }
}
