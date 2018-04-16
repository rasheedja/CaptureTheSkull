using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public GameObject primaryWeapon;
    public GameObject secondaryWeapon;
    public GameObject heldSkull;

    private GameObject currentWeapon;
    private bool isHoldingSkull;
    private SkullController skullController;
    private GameObject soldier; //The soldier model. This represents the players position to other players in the network, stores the player's health, and is also shown when the player dies.

    void Start () {
        currentWeapon = primaryWeapon;
        secondaryWeapon.GetComponent<GunController>().DisableWeapon();
        heldSkull.SetActive(false);
        secondaryWeapon.GetComponent<GunController>().UpdateAmmoCount();

        if (this.tag == "Blue")
        {
            soldier = PhotonNetwork.Instantiate("Soldier_B", this.transform.position, this.transform.rotation, 0);
            soldier.GetComponent<SoldierController>().SetOwner(this);
        }
        else
        {
            soldier = PhotonNetwork.Instantiate("Soldier_R", this.transform.position, this.transform.rotation, 0);
            soldier.GetComponent<SoldierController>().SetOwner(this);
        }
    }

    void Update () {
        // Defaults: Left Mouse and Left Ctrl
        if (Input.GetButton("Fire1"))
        {
            currentWeapon.GetComponent<GunController>().Shoot();
        }

        // TODO: Figure out customisable inputs
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentWeapon.GetComponent<GunController>().Reload();
        }

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

    /**
     * This should be called when the gun is shot. This will determine what hit the gun and call the appropriate reaction
     * function for the object that was hit. The logic for this function is based on the raycasting code from Lab 4.
     */
    public static void PhysicsRaycasts(Quaternion shooterRotation)
    {
        Vector3 centreOfScreen = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        Ray centreOfScreenRay = Camera.main.ScreenPointToRay(centreOfScreen);
        RaycastHit hit;

        if (Physics.Raycast(centreOfScreenRay, out hit))
        {
            Debug.Log("Raycast hit: " + hit.transform.name);
            Debug.Log(shooterRotation);
            //hit.transform.GetComponent<ObjectManagerBase>().Hit(hit, shooterRotation);
            Debug.Log(hit.transform.GetComponent<ObjectManagerBase>().photonView);
            hit.transform.GetComponent<ObjectManagerBase>().photonView.RPC("Hit", PhotonTargets.All, new object[] { hit.point, shooterRotation });

            GameObject hitGameObject = hit.transform.root.gameObject;
            Debug.Log(hitGameObject.tag);
            Debug.Log(hitGameObject);
            if (hitGameObject.tag == "BlueSoldier" || hitGameObject.tag == "RedSoldier")
            {
                hitGameObject.GetComponent<SoldierController>().photonView.RPC("DecreaseHealth", PhotonTargets.All, new object[] { 10 });
            }
        }
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
        currentWeapon = weaponTo;
    }

    /**
     * Drop the camera, disable player controls, and wait 10 seconds before respawning.
     */
    public IEnumerator Die()
    {
        this.gameObject.GetComponent<CharacterController>().enabled = false;
        this.currentWeapon.SetActive(false);
        string team = this.gameObject.tag;
        yield return new WaitForSeconds(2);

        this.soldier.GetComponent<SoldierController>().SetOwner(null);
        this.soldier.GetComponent<SoldierController>().photonView.RPC("Despawn", PhotonTargets.All, new object[] { 10 });

        GameObject newSpawn = GameManager.Instance.GetRandomBlueSpawn();
        this.transform.position = newSpawn.transform.position;
        this.transform.rotation = newSpawn.transform.rotation;
        soldier = PhotonNetwork.Instantiate("Soldier_B", this.transform.position, this.transform.rotation, 0);
        this.gameObject.GetComponent<CharacterController>().enabled = true;
        this.currentWeapon.SetActive(true);
        soldier.GetComponent<SoldierController>().SetOwner(this);
    }
}
