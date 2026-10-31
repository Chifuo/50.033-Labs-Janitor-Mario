using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public UnityEvent onGameStart;
    public UnityEvent onGameOver;
    public GameEvent<int> onScoreChange;
    public GameEvent<int> onDeliveryCountChange;
    [Min(1)] private int deliveriesToLose = 3;

    [SerializeField] private IntVariable gameScore;
    [SerializeField] private JanitorMario.FSM.JanitorRankProgress rankProgress;
    [SerializeField] private JanitorMario.FSM.BuffProgress buffProgress;
    public JanitorMario.FSM.JanitorRankProgress RankProgress => rankProgress;
    public JanitorMario.FSM.BuffProgress BuffProgress => buffProgress;
    public bool IsGameOver => gameScore.GameOver;

    private void InitialState()
    {
        rankProgress.ResetForNewRun();
        buffProgress.ResetForNewRun();
        gameScore.ApplyDefaultValues();
        Time.timeScale = 1f;
    }

    private void Awake()
    {
        InitialState();
    }
    private void Start()
    {
        onGameStart.Invoke();
        InitialState();
        SetScore(gameScore.Value);
        onDeliveryCountChange.Raise(gameScore.Deliveries);
    }
    public void GameRestart()
    {
        InitialState();
        SetScore(gameScore.Value);
        SceneManager.LoadSceneAsync("Stage-1");
    }
    public void GameNextScene()
    {
        // Leaving the menu starts a new run; changing stages keeps the score.
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            InitialState();
            SceneManager.LoadSceneAsync("LoadingScene");
            return;
        }
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void GameReturnHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync("MainMenu");
    }
    public void IncreaseScore(int increment)
    {
        if (IsGameOver)
            return;
        gameScore.ApplyChange("Value",increment);
        rankProgress.RecordScore(gameScore.Value);
        SetScore(gameScore.Value);
    }
    public void SetScore(int score)
    {
        onScoreChange.Raise(score);
    }

    public void ResetHighScore()
    {
        gameScore.ResetHighestValue();
    }
    public void GameOver()
    {
        onGameOver.Invoke();
        rankProgress.MarkGameOver();
        gameScore.ApplyGameOver();
        
        Debug.Log("Final Score:" + gameScore.Value);
    }

    public void GamePaused()
    {
        Time.timeScale = 0f;
    }

    public void GameResumed()
    {
        Time.timeScale = 1.0f;
    }

    public void RecordGoombaDelivery()
    {
        if (IsGameOver)
            return;

        gameScore.ApplyChange("Deliveries", 1);
        rankProgress.RecordDeliveries(gameScore.Deliveries);
        onDeliveryCountChange.Raise(gameScore.Deliveries);
        if (gameScore.Deliveries >= Mathf.Max(1, deliveriesToLose))
            GameOver();
    }

    public void RecordGoombaShooed()
    {
        if (IsGameOver) 
            return;
        gameScore.ApplyChange("Shooed", 1);
    }

    public void RecordPlayerSpedup()
    {
        if (IsGameOver) 
            return;
        buffProgress.RequestSpeedUp();
    }
}
