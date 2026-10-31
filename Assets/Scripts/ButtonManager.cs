using UnityEngine;
using UnityEngine.Events;

public class ButtonManager : MonoBehaviour 
{
    //public UnityEvent PauseGame = new UnityEvent();
    //public UnityEvent UnpauseGame = new UnityEvent();
    public UnityEvent onGameNextScene;
    public UnityEvent onGameReturnHome;
    public UnityEvent onGameRestart;
    public UnityEvent onGamePaused;
    public UnityEvent onGameUnpaused;
    public UnityEvent onHighscoreReset;

    public void NextSceneButtonClick()
    {
        onGameNextScene.Invoke();
    }
    public void RestartSceneButtonClick()
    {
        onGameRestart.Invoke();
    }
    public void HomeSceneButtonClick()
    {
        onGameReturnHome.Invoke();
    }
    public void ResetHighScoreButtonClick()
    {
        onHighscoreReset.Invoke();
    }
    public void MenuButtonClick()
    {
        onGamePaused.Invoke();
    }
    public void ResumeButtonClick()
    {
        onGameUnpaused.Invoke();
    }
}
