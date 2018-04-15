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

    /**
     * This should be called when the gun is shot. This will determine what hit the gun and call the appropriate reaction
     * function for the object that was hit. The logic for this function is based on the raycasting code from Lab 4.
     */
    public void PhysicsRaycasts(Quaternion shooterRotation)
    {
        Vector3 centreOfScreen = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        Ray centreOfScreenRay = Camera.main.ScreenPointToRay(centreOfScreen);
        RaycastHit hit;

        if (Physics.Raycast(centreOfScreenRay, out hit))
        {
            Debug.Log("Raycast hit: " + hit.transform.name);
            hit.transform.GetComponent<ObjectManagerBase>().Hit(hit.point, shooterRotation);
        }
        else
        {

        }

    }
}
