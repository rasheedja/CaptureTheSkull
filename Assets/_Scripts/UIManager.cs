using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * A singleton used to update the game view
 */
public class UIManager : MonoBehaviour {
    public Text ammoText;
    public Text healthText;
    public Text timerText;
    public Text blueScoreText;
    public Text redScoreText;

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
}
