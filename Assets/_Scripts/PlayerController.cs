using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public GameObject primaryWeapon;
    public GameObject secondaryWeapon;
    public GameObject heldSkull;

    private bool isHoldingSkull;
    private SkullController skullController;
    private int maxHealth;
    private int health = 100;
    private GameObject soldier; //The soldier model. This represents the players position to other players in the network and is also shown when the player dies.

    void Start () {
        secondaryWeapon.GetComponent<GunController>().DisableWeapon();
        heldSkull.SetActive(false);
        secondaryWeapon.GetComponent<GunController>().UpdateAmmoCount();
        maxHealth = health;
        UIManager.Instance.UpdateHealth(health);

        if (this.tag == "Blue")
        {
            soldier = PhotonNetwork.Instantiate("Soldier_B", this.transform.position, this.transform.rotation, 0);
        }
        else
        {
            soldier = PhotonNetwork.Instantiate("Soldier_R", this.transform.position, this.transform.rotation, 0);
        }
    }

    void Update () {
		if ((Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) && !primaryWeapon.activeInHierarchy && !heldSkull.activeInHierarchy)
        {
            StartCoroutine(SwapWeapon(secondaryWeapon, primaryWeapon));
        }
        if ((Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) && !secondaryWeapon.activeInHierarchy)
        {
            StartCoroutine(SwapWeapon(primaryWeapon, secondaryWeapon));
        }

        soldier.transform.position = new Vector3(this.transform.position.x, this.transform.position.y - 0.4f, this.transform.position.z);
        soldier.transform.rotation = this.transform.rotation;
    }

    public bool IsHoldingSkull()
    {
        return isHoldingSkull;
    }

    public void HoldSkull(SkullController skullController)
    {
        this.isHoldingSkull = true;
        heldSkull.SetActive(true);
        this.skullController = skullController;
        
        // When holding a skull, you can only use your pistol
        if (!secondaryWeapon.activeInHierarchy)
        {
            StartCoroutine(SwapWeapon(primaryWeapon, secondaryWeapon));
        }

        skullController.photonView.RPC("DisableSkull", PhotonTargets.AllBuffered);
    }

    public void DropSkull()
    {
        this.isHoldingSkull = false;
        heldSkull.SetActive(false);

        skullController.photonView.RPC("EnableSkull", PhotonTargets.AllBuffered);

        if (this.tag == "Blue")
        {
            GameManager.Instance.photonView.RPC("IncrementBlueScore", PhotonTargets.AllBuffered);
        }
        else
        {
            GameManager.Instance.photonView.RPC("IncrementRedScore", PhotonTargets.AllBuffered);
        }
    }

    IEnumerator SwapWeapon(GameObject weaponFrom, GameObject weaponTo)
    {
        GunController weaponFromScript = weaponFrom.GetComponent<GunController>();
        GunController weaponToScript = weaponTo.GetComponent<GunController>();

        StartCoroutine(weaponFromScript.LowerWeapon());
        yield return new WaitForSeconds(weaponFromScript.swapTime);
        StartCoroutine(weaponToScript.RaiseWeapon());
    }

    public void DecreaseHealth(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            health = 0;
            Die();
        }

        UIManager.Instance.UpdateHealth(health);
    }

    public void IncreaseHealth(int amount)
    {
        health += amount;
        if (health > maxHealth)
        {
            health = maxHealth;
        }

        UIManager.Instance.UpdateHealth(health);
    }

    public void Die()
    {
        Debug.Log("bang bang...");
    }
}
