using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

/*
 * A singleton used to manage the game score, win conditions, etc.
 */
public class GameManager : Photon.MonoBehaviour {

    public static GameManager Instance { get; private set; }

    public int totalMatchTime;
    public int scoreToWin;
    public GameObject FPSController;
    public List<GameObject> blueSpawns;
    public List<GameObject> redSpawns;

    private int blueCaptures = 0;
    private int redCaptures = 0;

    // Track player stats for game over screen
    private int kills = 0;
    private int deaths = 0;
    private int skullsCaptured = 0;

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
            GameObject chosenSpawn = GetRandomBlueSpawn();
            Instantiate(player, chosenSpawn.transform.position, chosenSpawn.transform.rotation);
        }
        else
        {
            player.tag = "Red";
            GameObject chosenSpawn = GetRandomRedSpawn();
            Instantiate(player, chosenSpawn.transform.position, chosenSpawn.transform.rotation);
        }
    }

    public void FormatTime(float timeRemaining)
    {
        // Timer code based on my Lab2/3 implementation
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        UIManager.Instance.UpdateTimer(string.Format("{0}:{1}", minutes.ToString("00"), seconds.ToString("00")));

        if (timeRemaining <= 0)
        {
            EndGame();
        }
    }

    /**
     * Get a random spawn location for the blue team
     */
    public GameObject GetRandomBlueSpawn()
    {
        return blueSpawns[Random.Range(0, blueSpawns.Count - 1)];
    }

    /**
    * Get a random spawn location for the red team
    */
    public GameObject GetRandomRedSpawn()
    {
        return redSpawns[Random.Range(0, blueSpawns.Count - 1)];
    }

    [PunRPC]
    public void IncrementBlueScore()
    {
        blueCaptures++;
        if (blueCaptures >= scoreToWin) { EndGame(); }
        UIManager.Instance.UpdateBlueScore(blueCaptures);
        UIManager.Instance.UpdateMessage("Red Skull Captured");
    }

    [PunRPC]
    public void IncrementRedScore()
    {
        redCaptures++;
        if (redCaptures >= scoreToWin) { EndGame(); }
        UIManager.Instance.UpdateRedScore(redCaptures);
        UIManager.Instance.UpdateMessage("Blue Skull Captured");
    }

    public void IncrementKills()
    {
        kills++;
        UIManager.Instance.UpdateKills(kills);
    }

    public void IncrementDeaths()
    {
        deaths++;
        UIManager.Instance.UpdateDeaths(deaths);
    }

    public void IncrementSkullCaptures()
    {
        skullsCaptured++;
    }

    /**
     * Save game over stats in player prefs and then load the game over scene
     */
    private void EndGame()
    {
        PlayerPrefs.SetInt("blueScore", blueCaptures);
        PlayerPrefs.SetInt("redScore", redCaptures);

        if (blueCaptures == scoreToWin)
        {
            PlayerPrefs.SetString("winner", "blue");
        }
        else if (redCaptures == scoreToWin)
        {
            PlayerPrefs.SetString("winner", "red");
        }
        else if (blueCaptures > redCaptures)
        {
            PlayerPrefs.SetString("winner", "blue");
        }
        else if (redCaptures > blueCaptures)
        {
            PlayerPrefs.SetString("winner", "red");
        }
        else
        {
            PlayerPrefs.SetString("winner", "noone");
        }

        PlayerPrefs.SetInt("kills", kills);
        PlayerPrefs.SetInt("deaths", deaths);
        PlayerPrefs.SetInt("skullCaptures", skullsCaptured);

        int totalKills = PlayerPrefs.GetInt("totalKills", 0) + kills;
        int totalDeaths = PlayerPrefs.GetInt("totalDeaths", 0) + deaths;
        int totalSkullCaptures = PlayerPrefs.GetInt("totalSkullCaptures", 0) + skullsCaptured;

        PlayerPrefs.SetInt("totalKills", totalKills);
        PlayerPrefs.SetInt("totalDeaths", totalDeaths);
        PlayerPrefs.SetInt("totalSkullCaptures", totalSkullCaptures);

        SceneManager.LoadScene(2);
    }
}
