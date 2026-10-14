using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    [SerializeField] private float delay = 3f;
    private Coroutine loading;
    private bool leaving;

    private void Start()
    {
        loading = StartCoroutine(LoadStage());
    }

    private IEnumerator LoadStage()
    {
        yield return new WaitForSecondsRealtime(delay);
        leaving = true;
        yield return SceneManager.LoadSceneAsync("Stage-1");
    }

    public void BackToMenu()
    {
        // Once async loading starts, it cannot be cancelled by stopping the coroutine.
        if (leaving)
            return;

        leaving = true;
        StopCoroutine(loading);
        SceneManager.LoadSceneAsync("MainMenu");
    }

    private void OnDisable()
    {
        if (loading != null)
            StopCoroutine(loading);
    }
}
