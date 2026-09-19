using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class HUDManager : MonoBehaviour 
{
    private Vector3[] scoreTextPosition =
    {
        new Vector3(-699,444,0),
        new Vector3 (0,0,0),
    };

    public GameObject scoreText;

    public void GameStart()
    {
        scoreText.transform.localPosition = scoreTextPosition[0];
    }

    public void SetScore(int score)
    {
        scoreText.GetComponent<TextMeshProUGUI>().text = "Score: " + score.ToString();
    }

}
