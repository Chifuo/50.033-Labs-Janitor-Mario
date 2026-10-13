using System;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

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
    [Min(1)] public int deliveriesToLose = 3;

    public bool IsGameOver { get; private set; }
    public int GoombaDeliveries { get; private set; }
    public int GoombaShooedNumber { get; private set; }

    public int score { get; private set; }
    private static GameManager _instance;

    public static GameManager instance
    {
        get { return _instance; }
    }

    private static void InitialState()
    {
        instance.score = 0;
        instance.GoombaDeliveries = 0;
        instance.IsGameOver = false;
        Time.timeScale = 1f;
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
        SetScore(score);
        deliveryCountChange.Invoke(GoombaDeliveries);
    }
    public void GameRestart()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        InitialState();
    }
    public void GameNextScene()
    {
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
        score += increment;
        SetScore(score);
        scoreIncreased.Invoke();
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
}
