using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public GameObject primaryWeapon;
    public GameObject secondaryWeapon;

    private bool isHoldingSkull;
    private GameObject skull;
    private int maxHealth;
    private int health = 100;

    // Use this for initialization
    void Start () {
        primaryWeapon.SetActive(true);
        secondaryWeapon.SetActive(false);
        primaryWeapon.GetComponent<GunController>().UpdateAmmoCount();
        maxHealth = health;
        UIManager.Instance.UpdateHealth(health);
    }

    // Update is called once per frame
    void Update () {
		if ((Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) && !primaryWeapon.activeInHierarchy)
        {
            StartCoroutine(SwapWeapon(secondaryWeapon, primaryWeapon));
        }
        if ((Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) && !secondaryWeapon.activeInHierarchy)
        {
            StartCoroutine(SwapWeapon(primaryWeapon, secondaryWeapon));
        }
    }

    public bool IsHoldingSkull()
    {
        return isHoldingSkull;
    }

    public void HoldSkull(GameObject skull)
    {
        this.isHoldingSkull = true;
        this.skull = skull;
    }

    public void DropSkull()
    {
        this.isHoldingSkull = false;
        this.skull.SetActive(true);
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
