using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A singleton used to manage the game score, win conditions, etc.
 */
public class GameManager : Photon.MonoBehaviour {

    public static GameManager Instance { get; private set; }

    public GameObject FPSController;
    public List<GameObject> blueSpawns;
    public List<GameObject> redSpawns;

    private int totalMatchTime = 120;
    private int blueCaptures = 0;
    private int redCaptures = 0;
    private int scoreToWin = 3;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        InstantiatePlayer(PlayerPrefs.GetString("team"));
    }

    void Update () {
        // Timer code based on Lab2/3
        float timeSinceGameStart = Time.timeSinceLevelLoad;
        float timeRemaining = totalMatchTime - timeSinceGameStart;
        FormatTime(timeRemaining);
    }

    public void InstantiatePlayer(string playerTeam)
    {
        GameObject player = FPSController;
        if (playerTeam == "Blue")
        {
            player.tag = "Blue";
            GameObject chosenSpawn = blueSpawns[Random.Range(0, blueSpawns.Count - 1)];
            Instantiate(FPSController, chosenSpawn.transform.position, chosenSpawn.transform.rotation);
        }
        else
        {
            player.tag = "Red";
            GameObject chosenSpawn = redSpawns[Random.Range(0, redSpawns.Count - 1)];
            Instantiate(FPSController, chosenSpawn.transform.position, chosenSpawn.transform.rotation);
        }
    }

    public void FormatTime(float timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        UIManager.Instance.UpdateTimer(string.Format("{0}:{1}", minutes.ToString("00"), seconds.ToString("00")));

        if (timeRemaining <= 0)
        {
            EndGame();
        }
    }

    [PunRPC]
    public void IncrementBlueScore()
    {
        blueCaptures++;
        if (blueCaptures >= scoreToWin) { EndGame(); }
        UIManager.Instance.UpdateBlueScore(blueCaptures);
    }

    [PunRPC]
    public void IncrementRedScore()
    {
        redCaptures++;
        if (redCaptures >= scoreToWin) { EndGame(); }
        UIManager.Instance.UpdateRedScore(redCaptures);
    }

    private void EndGame()
    {
        Debug.Log("game end");
    }
}
