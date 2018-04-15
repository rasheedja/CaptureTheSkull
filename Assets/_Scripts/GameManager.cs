using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A singleton used to manage the game score, win conditions, etc.
 */
public class GameManager : MonoBehaviour {

    public static GameManager Instance { get; private set; }

    private int totalMatchTime = 120;
    private int blueCaptures = 0;
    private int redCaptures = 0;
    private int scoreToWin = 3;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

	void Update () {
        // Timer code based on Lab2/3
        float timeSinceGameStart = Time.timeSinceLevelLoad;
        float timeRemaining = totalMatchTime - timeSinceGameStart;

        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        UIManager.Instance.UpdateTimer(string.Format("{0}:{1}", minutes.ToString("00"), seconds.ToString("00")));

        if (timeRemaining > 0)
        {
            EndGame();
        }
    }

    public void IncrementBlueScore()
    {
        blueCaptures++;
        if (blueCaptures >= 3) { EndGame(); }
        UIManager.Instance.UpdateBlueScore(blueCaptures);
    }

    public void IncrementRedScore()
    {
        redCaptures++;
        if (redCaptures >= 3) { EndGame(); }
        UIManager.Instance.UpdateRedScore(redCaptures);
    }

    private void EndGame()
    {
        Debug.Log("game end");
    }
}
