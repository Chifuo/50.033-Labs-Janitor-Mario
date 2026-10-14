using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public UnityEvent gameStart = new UnityEvent();
    public UnityEvent gameOver = new UnityEvent();
    public UnityEvent gameRestart = new UnityEvent();
    public UnityEvent gamePaused = new UnityEvent();
    public UnityEvent gameNextScene = new UnityEvent();
    public UnityEvent<int> scoreChange = new UnityEvent<int>();
    public UnityEvent scoreIncreased = new UnityEvent();
    public UnityEvent<int> deliveryCountChange = new UnityEvent<int>();
    public UnityEvent goombaDeliveredTrash = new UnityEvent();
    public UnityEvent goombaShooed = new UnityEvent();
    public UnityEvent playerSpedup = new UnityEvent();
    public UnityEvent playerEndSpedup = new UnityEvent();
    [Min(1)] private int deliveriesToLose = 3;

    public bool IsGameOver { get; private set; }
    public int GoombaDeliveries { get; private set; }
    public int GoombaShooedNumber { get; private set; }

    [SerializeField] private IntVariable gameScore;
    public int Score => gameScore.Value;
    public int HighScore => gameScore.PreviousHighestValue;
    public float PlayerSpeed { get; private set; }
    public float PlayerMaxSpeed { get; private set; }
    public float PlayerDashSpeed { get; private set; }
    public bool PlayerIsSpedup {  get; private set; }
    //private float SpeedupTime = 5f;
    //private float SpeedupTimeRemaining;
    private static GameManager _instance;

    public static GameManager instance
    {
        get { return _instance; }
    }

    private static void InitialState()
    {
        instance.gameScore.Value = 0;
        instance.GoombaDeliveries = 0;
        instance.IsGameOver = false;
        instance.PlayerIsSpedup = false;
        Time.timeScale = 1f;
    }

    private static void PlayerNormalSpeed()
    {
        instance.PlayerSpeed = 100;
        instance.PlayerMaxSpeed = 200;
        instance.PlayerDashSpeed = 400f;
    }
    private static void PlayerSpedupSpeed()
    {
        instance.PlayerSpeed = 200;
        instance.PlayerMaxSpeed = 400;
        instance.PlayerDashSpeed = 400f;
    }
    private void Awake()
    {
        // check if the _instance is not this, means it's been set before, return
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        // otherwise this is the first time this instance is created
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    private void Start()
    {
        InitialState();
        gameStart.Invoke();
        SetScore(Score);
        deliveryCountChange.Invoke(GoombaDeliveries);
        PlayerNormalSpeed();
    }
    public void GameRestart()
    {
        InitialState();
        SetScore(Score);
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
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
        SceneManager.LoadSceneAsync("MainMenu");
    }
    public void IncreaseScore(int increment)
    {
        if (IsGameOver)
            return;
        gameScore.ApplyChange(increment);
        SetScore(Score);
        scoreIncreased.Invoke();
    }
    public void SetScore(int score)
    {
        scoreChange.Invoke(score);
    }

    public void ResetHighScore()
    {
        gameScore.ResetHighestValue();
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
        goombaDeliveredTrash.Invoke();
        if (GoombaDeliveries >= Mathf.Max(1, deliveriesToLose))
            GameOver();
    }

    public void RecordGoombaShooed()
    {
        if (IsGameOver) return;
        GoombaShooedNumber++;
        goombaShooed.Invoke();
    }

    public void RecordPlayerSpedup()
    {
        if (IsGameOver) return;
        PlayerIsSpedup = true;
        PlayerSpedupSpeed();
        playerSpedup.Invoke();
    }

    public void RecordPlayerEndSpeedup()
    {
        if (IsGameOver) return;
        PlayerIsSpedup = false;
        PlayerNormalSpeed();
        playerEndSpedup.Invoke();

    }
}
