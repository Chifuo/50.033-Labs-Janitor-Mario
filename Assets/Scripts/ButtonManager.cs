using System;
using UnityEngine;
using UnityEngine.Events;

public class ButtonManager : MonoBehaviour 
{
    public UnityEvent PauseGame = new UnityEvent();
    public UnityEvent UnpauseGame = new UnityEvent();

    public void NextSceneButtonClick()
    {
        GameManager.instance.GameNextScene();
    }
    public void RestartSceneButtonClick()
    {
        GameManager.instance.GameRestart();
    }
    public void HomeSceneButtonClick()
    {
        GameManager.instance.GameReturnHome();
    }
    public void MenuButtonClick()
    {
        Time.timeScale = 0f;
        PauseGame.Invoke();
    }
    public void ResumeButtonClick()
    {
        Time.timeScale = 1.0f;
        UnpauseGame.Invoke();
    }
}
