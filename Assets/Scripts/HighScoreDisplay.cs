using TMPro;
using UnityEngine;

public class HighScoreDisplay : MonoBehaviour
{
    private TMP_Text label;
    [SerializeField] private IntVariable gameScore;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        label.text = $"Highscore: {gameScore.PreviousHighestValue}";
    }
}
