using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MenuController : MonoBehaviour
{
    public GameObject mainMenuCanvas;
    public GameObject helpMenuCanvas;
    public Text totalKillsText;
    public Text skullsCapturedText;
    public Text totalDeathsText;

    void Start()
    {
        totalKillsText.text = "Total Kills: " + PlayerPrefs.GetInt("totalKills", 0);
        totalDeathsText.text = "Total Deaths: " + PlayerPrefs.GetInt("totalDeaths", 0);
        skullsCapturedText.text = "Skulls Captured: " + PlayerPrefs.GetInt("totalSkullCaptures", 0);
    }

    public void ButtonHandlerStartGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void ButtonHandlerHelp()
    {
        mainMenuCanvas.SetActive(false);
        helpMenuCanvas.SetActive(true);
    }

    public void ButtonHandlerBack()
    {
        mainMenuCanvas.SetActive(true);
        helpMenuCanvas.SetActive(false);
    }

    public void ButtonHandlerQuitGame()
    {
        Application.Quit();
    }
}
