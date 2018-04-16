using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/**
 * manage the view at the game over screen
 */
public class GameOverUIManager : Photon.MonoBehaviour
{
    public Text blueScoreText;
    public Text redScoreText;
    public Text winnerText;
    public Text yourKillsText;
    public Text yourDeathsText;
    public Text skullsYouCapturedText;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        UpdateBlueScore(PlayerPrefs.GetInt("blueScore"));
        UpdateRedScore(PlayerPrefs.GetInt("redScore"));
        UpdateYourKillsText(PlayerPrefs.GetInt("kills"));
        UpdateYourDeathsText(PlayerPrefs.GetInt("deaths"));
        UpdateSkullsYouCapturedText(PlayerPrefs.GetInt("skullCaptures"));

        string winner = PlayerPrefs.GetString("winner");

        if (winner == "blue")
        {
            UpdateWinnerText("Blue's Won!");
        }
        else if (winner == "red")
        {
            UpdateWinnerText("Red's Won!");
        }
        else
        {
            UpdateWinnerText("It's a Draw.");
        }

    }

    public void ButtonHandlerBackToMenu()
    {
        if (PhotonNetwork.connectedAndReady)
        {
            PhotonNetwork.LeaveRoom();
        }

        SceneManager.LoadSceneAsync(0);
    }

    public void UpdateBlueScore(int score)
    {
        blueScoreText.text = "Blue Score: " + score;
    }

    public void UpdateRedScore(int score)
    {
        redScoreText.text = "Red Score: " + score;
    }

    public void UpdateWinnerText(string winner)
    {
        winnerText.text = "Game Over: " + winner;
    }

    public void UpdateYourKillsText(int kills)
    {
        yourKillsText.text = "Your Kills: " + kills;
    }

    public void UpdateYourDeathsText(int deaths)
    {
        yourDeathsText.text = "Your Deaths: " + deaths;
    }

    public void UpdateSkullsYouCapturedText(int skullsCaptured)
    {
        skullsYouCapturedText.text = "Skulls You Captured: " + skullsCaptured;
    }
}
