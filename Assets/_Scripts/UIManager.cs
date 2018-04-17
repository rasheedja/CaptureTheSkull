using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/**
 * A singleton used to update the game view
 */
public class UIManager : MonoBehaviour {
    public Text ammoText;
    public Text healthText;
    public Text timerText;
    public Text blueScoreText;
    public Text redScoreText;
    public Text killsText;
    public Text deathsText;
    public Text messageText;
    public Text grenadesText;

    public static UIManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateAmmo(int currentAmmo, int reserveAmmo)
    {
        ammoText.text = "Ammo: " + currentAmmo + "/" + reserveAmmo;
    }

    public void UpdateGrenades(int grenades)
    {
        grenadesText.text = "Grenades: " + grenades;
    }

    public void UpdateHealth(int health)
    {
        healthText.text = "Health: " + health;
    }

    /**
     * Update the timer. 
     * 
     * @param timeText: The time exactly how it should be displayed, e.g. 01:59
     */
    public void UpdateTimer(string timeText)
    {
        timerText.text = timeText;
    }

    public void UpdateBlueScore(int score)
    {
        blueScoreText.text = "Blue Score: " + score;
    }

    public void UpdateRedScore(int score)
    {
        redScoreText.text = "Red Score: " + score;
    }

    public void UpdateKills(int kills)
    {
        killsText.text = "Kills: " + kills;
    }

    public void UpdateDeaths(int deaths)
    {
        deathsText.text = "Deaths: " + deaths;
    }

    public void UpdateMessage(string message)
    {
        StartCoroutine(DisplayMessage(message));
    }

    IEnumerator DisplayMessage(string message)
    {
        messageText.DOFade(255, 0f);
        messageText.DOText(message, 1f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(3f);
        messageText.DOFade(0, 2f);
        yield return new WaitForSeconds(2f);
        messageText.text = "";
    }
}
