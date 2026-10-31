using UnityEngine;
using UnityEngine.SceneManagement;

public class StageProgression : MonoBehaviour
{
    [SerializeField] private int scoreThreshold = 10;
    [SerializeField] private string nextScene = "Stage-2";
    [SerializeField] private IntVariable gameScore;
    private bool loading;

    private void Update()
    {
        CheckScore(gameScore.Value);
    }

    private void CheckScore(int score)
    {
        if (loading || gameScore.GameOver || score < scoreThreshold)
            return;

        loading = true;
        SceneManager.LoadSceneAsync(nextScene);
    }
}
