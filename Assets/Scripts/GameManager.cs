using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public UnityEvent gameStart;
    public UnityEvent gameOver;
    public UnityEvent gameRestart;
    public UnityEvent<int> scoreChange;

    private int score = 0;

    private void Start()
    {
        gameStart.Invoke();
    }
    public void GameRestart()
    {
        score = 0;
        SetScore(score);
        gameRestart.Invoke();
    }
    public void IncreaseScore(int increment)
    {
        score += increment;
        SetScore(score);
    }
    public void SetScore(int score)
    {
        scoreChange.Invoke(score);
    }
    public void GameOver()
    {
        gameOver.Invoke();
    }
}
