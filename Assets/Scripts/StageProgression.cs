using UnityEngine;
using UnityEngine.SceneManagement;

public class StageProgression : MonoBehaviour
{
    [SerializeField] private int scoreThreshold = 10;
    [SerializeField] private string nextScene = "Stage-2";
    private GameManager manager;
    private bool loading;

    private void Start()
    {
        manager = GameManager.instance;
        manager.scoreChange.AddListener(CheckScore);
        CheckScore(manager.Score);
    }

    private void CheckScore(int score)
    {
        if (loading || manager.IsGameOver || score < scoreThreshold)
            return;

        loading = true;
        SceneManager.LoadSceneAsync(nextScene);
    }

    private void OnDestroy()
    {
        if (manager != null)
            manager.scoreChange.RemoveListener(CheckScore);
    }
}
