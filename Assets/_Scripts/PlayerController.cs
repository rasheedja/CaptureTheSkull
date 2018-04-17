using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {
    public GameObject primaryWeapon;
    public GameObject secondaryWeapon;
    public GameObject heldSkull;

    private Image crosshair;
    private GameObject currentWeapon;
    private bool isHoldingSkull;
    private SkullController skullController;
    private GameObject soldier; //The soldier model. This represents the players position to other players in the network, stores the player's health, and is also shown when the player dies.
    private SoldierController soldierController;
    private int grenadeAmmo;
    private int grenadeMax;

    void Awake()
    {
        crosshair = GameObject.Find("Crosshair").GetComponent<Image>();
    }

    void Start () {
        currentWeapon = primaryWeapon;
        secondaryWeapon.GetComponent<GunController>().DisableWeapon();
        heldSkull.SetActive(false);
        isHoldingSkull = false;
        currentWeapon.GetComponent<GunController>().UpdateAmmoCount();
        grenadeAmmo = 3;
        grenadeMax = grenadeAmmo;

        if (this.tag == "Blue")
        {
            soldier = PhotonNetwork.Instantiate("Soldier_B", this.transform.position, this.transform.rotation, 0);
            soldierController = soldier.GetComponent<SoldierController>();
            soldierController.SetOwner(this);
        }
        else
        {
            soldier = PhotonNetwork.Instantiate("Soldier_R", this.transform.position, this.transform.rotation, 0);
            soldierController = soldier.GetComponent<SoldierController>();
            soldierController.SetOwner(this);
        }
    }

    void Update()
    {
        // First, check soldier controller is not null. It's only null when you're dead
        if (soldierController)
        {
            // Then, check that we can still move
            if (this.gameObject.GetComponent<CharacterController>().enabled)
            {
                // Defaults: Left Mouse and Left Ctrl
                if (Input.GetButton("Fire1"))
                {
                    currentWeapon.GetComponent<GunController>().Shoot(soldierController);
                }

                // Defaults: Right Mouse and Left Alt
                if (Input.GetButton("Fire2"))
                {
                    this.GetComponentInChildren<Camera>().DOFieldOfView(45, 0.5f);
                }
                else
                {
                    this.GetComponentInChildren<Camera>().DOFieldOfView(60, 0.5f);
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    currentWeapon.GetComponent<GunController>().Reload();
                }

                if ((Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) && !primaryWeapon.activeInHierarchy && !heldSkull.activeInHierarchy)
                {
                    if (SwapWeapon(secondaryWeapon, primaryWeapon))
                    {
                        soldierController.photonView.RPC("SelectPrimaryWeapon", PhotonTargets.AllBuffered);
                    }
                }

                if ((Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) && !secondaryWeapon.activeInHierarchy)
                {
                    if (SwapWeapon(primaryWeapon, secondaryWeapon))
                    {
                        soldierController.photonView.RPC("SelectSecondaryWeapon", PhotonTargets.AllBuffered);
                    }
                }

                if (Input.GetKeyDown(KeyCode.G) && !isHoldingSkull && grenadeAmmo != 0)
                {
                    ThrowGrenade();
                }

                // If Left Shift held down with some key
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        if (isHoldingSkull) { DropSkull(); }
                        PhotonNetwork.LeaveRoom();
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = true;
                        SceneManager.LoadScene(0);
                    }
                }

                // Tell the soldier model the direction we are moving in, determing the animation played in the soldier model.
                // The order of the conditions determine the animation played if multiple buttons are pressed. For example,
                // if the player holds down forward and right, the forward animation will play.
                float verticalInput = Input.GetAxis("Vertical");
                float horizontalInput = Input.GetAxis("Horizontal");
                if (verticalInput > 0)
                {
                    soldierController.photonView.RPC("MoveForward", PhotonTargets.All);
                }
                else if (verticalInput < 0)
                {
                    soldierController.photonView.RPC("MoveBackward", PhotonTargets.All);

                }
                else if (horizontalInput > 0)
                {
                    soldierController.photonView.RPC("MoveRight", PhotonTargets.All);

                }
                else if (horizontalInput < 0)
                {
                    soldierController.photonView.RPC("MoveLeft", PhotonTargets.All);

                }
                else
                {
                    soldierController.photonView.RPC("Idle", PhotonTargets.All);
                }

                if (Input.GetButtonDown("Jump"))
                {
                    soldierController.photonView.RPC("Jump", PhotonTargets.All);
                }
            }
            soldier.transform.position = new Vector3(this.transform.position.x, this.transform.position.y - 0.95f, this.transform.position.z);
            soldier.transform.rotation = this.transform.rotation;
        }

        // Fire the raycast to determine crosshair colour
        PhysicsCrosshairRaycast();
    }

    public bool ReplenishAmmo(string gunType)
    {
        if (gunType == "Primary")
        {
            return primaryWeapon.GetComponent<GunController>().FillAmmo();
        }
        else if (gunType == "Secondary")
        {
            return secondaryWeapon.GetComponent<GunController>().FillAmmo();
        }
        else if (gunType == "Grenade")
        {
            if (grenadeAmmo == grenadeMax) { return false; }
            grenadeAmmo = grenadeMax;
            UIManager.Instance.UpdateGrenades(grenadeAmmo);
            return true;
        }
        return false;
    }

    public void ThrowGrenade()
    {
        GameObject grenade = PhotonNetwork.Instantiate("M67_Grenade", new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), transform.rotation, 0);
        Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
        grenadeRb.AddForce(Camera.main.transform.forward * 20, ForceMode.Impulse);
        grenadeAmmo--;
        UIManager.Instance.UpdateGrenades(grenadeAmmo);
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
            hit.transform.GetComponent<ObjectManagerBase>().photonView.RPC("Hit", PhotonTargets.All, new object[] { hit.point, shooterRotation });

            GameObject hitGameObject = hit.transform.root.gameObject;
            if (hitGameObject.tag == "BlueSoldier" || hitGameObject.tag == "RedSoldier") // i.e. another player
            {
                SoldierController hitSoldierController = hitGameObject.GetComponent<SoldierController>();
                if (!hitSoldierController.GetIsDead()) // Decrease health if not dead
                {
                    hitSoldierController.photonView.RPC("DecreaseHealth", PhotonTargets.AllBuffered, new object[] { 10 });
                    if (hitSoldierController.GetIsDead()) // If they died, increment your kills
                    {
                        GameManager.Instance.IncrementKills();
                    }
                }
                else
                {
                    // Make the ragdoll move in the direction it was shot at
                    hit.transform.GetComponent<ObjectManagerBase>().photonView.RPC("HitRigidbody", PhotonTargets.All, new object[] { hit.point });
                }
            }
            else if (hitGameObject.tag == "Grenade")
            {
                hit.transform.GetComponent<ObjectManagerBase>().photonView.RPC("HitRigidbody", PhotonTargets.All, new object[] { hit.point });
            }
        }
    }

    /**
     * This raycast is called every frame and is used to determine the colour of the crosshair
     */
    private void PhysicsCrosshairRaycast()
    {
        Vector3 centreOfScreen = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        Ray centreOfScreenRay = Camera.main.ScreenPointToRay(centreOfScreen);
        RaycastHit hit;

        if (Physics.Raycast(centreOfScreenRay, out hit))
        {
            GameObject hitGameObject = hit.transform.root.gameObject;
            if (hitGameObject.tag == "BlueSoldier")
            {
                // Only change colours for alive players
                if (!hitGameObject.GetComponent<SoldierController>().GetIsDead())
                {
                    if (this.tag == "Blue")
                    {
                        crosshair.color = new Color(0f, 255f, 0f);
                    }
                    else if (this.tag == "Red")
                    {
                        crosshair.color = new Color(255f, 0f, 0f);

                    }
                }
                else
                {
                    crosshair.color = new Color(255f, 255f, 255f);
                }
            }
            else if (hitGameObject.tag == "RedSoldier")
            {
                // Only change colours for alive players
                if (!hitGameObject.GetComponent<SoldierController>().GetIsDead())
                {
                    if (this.tag == "Blue")
                    {
                        crosshair.color = new Color(255f, 0f, 0f);
                    }
                    else if (this.tag == "Red")
                    {
                        crosshair.color = new Color(0f, 255f, 0f);
                    }
                }
                else
                {
                    crosshair.color = new Color(255f, 255f, 255f);
                }
            }
            else
            {
                crosshair.color = new Color(255f, 255f, 255f);
            }
        }
    }
    public bool IsHoldingSkull()
    {
        return isHoldingSkull;
    }

    public void HoldSkull(SkullController skullController)
    {
        isHoldingSkull = true;
        heldSkull.SetActive(true);
        soldierController.photonView.RPC("ShowSkull", PhotonTargets.AllBuffered);
        this.skullController = skullController;

        // When holding a skull, you can only use your pistol
        if (!secondaryWeapon.activeInHierarchy)
        {
            // Ensure the weapon is swapped if we grab the skull
            if (SwapWeapon(primaryWeapon, secondaryWeapon, true))
            {
                soldierController.photonView.RPC("SelectSecondaryWeapon", PhotonTargets.AllBuffered);
            }
        }

        skullController.DisableSkull();
        skullController.photonView.RPC("DisableSkull", PhotonTargets.AllBuffered);
        skullController.photonView.RPC("DisableSkullMessage", PhotonTargets.All);
    }

    public void DropSkull()
    {
        isHoldingSkull = false;
        heldSkull.SetActive(false);
        soldierController.photonView.RPC("HideSkull", PhotonTargets.AllBuffered);
        skullController.photonView.RPC("EnableSkull", PhotonTargets.AllBuffered);
    }

    public void IncrementScore()
    {
        if (this.tag == "Blue")
        {
            GameManager.Instance.photonView.RPC("IncrementBlueScore", PhotonTargets.AllBuffered);
            GameManager.Instance.photonView.RPC("BlueScoreMessage", PhotonTargets.All);
        }
        else
        {
            GameManager.Instance.photonView.RPC("IncrementRedScore", PhotonTargets.AllBuffered);
            GameManager.Instance.photonView.RPC("RedScoreMessage", PhotonTargets.All);
        }
        GameManager.Instance.IncrementSkullCaptures();
    }

    public bool SwapWeapon(GameObject weaponFrom, GameObject weaponTo, bool forceSwap = false)
    {
        if (!weaponFrom.GetComponent<GunController>().IsBusy() || forceSwap)
        {
            StartCoroutine(SwapWeaponCR(weaponFrom, weaponTo));
            return true; // We swapped the weapon
        }
        return false; // We didn't swap the weapon
    }

    IEnumerator SwapWeaponCR(GameObject weaponFrom, GameObject weaponTo)
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
        GameManager.Instance.IncrementDeaths();
        this.gameObject.GetComponent<CharacterController>().enabled = false;
        currentWeapon.SetActive(false);
        if (isHoldingSkull) { DropSkull(); }
        string team = this.gameObject.tag;
        soldierController.photonView.RPC("Despawn", PhotonTargets.AllBuffered, new object[] { 30 });
        soldierController = null;
        GameObject newSpawn;
        string soldierPrefab;

        if (this.tag == "Blue")
        {
            newSpawn = GameManager.Instance.GetRandomBlueSpawn();
            soldierPrefab = "Soldier_B";
        }
        else
        {
            newSpawn = GameManager.Instance.GetRandomRedSpawn();
            soldierPrefab = "Soldier_R";
        }
        yield return new WaitForSeconds(3);
        this.transform.DORotate(newSpawn.transform.rotation.eulerAngles, 2);
        this.transform.DOMove(newSpawn.transform.position, 2);
        yield return new WaitForSeconds(2);
        soldier = PhotonNetwork.Instantiate(soldierPrefab, this.transform.position, this.transform.rotation, 0);
        soldierController = soldier.GetComponent<SoldierController>();
        ReplenishAmmo("Grenade");
        this.gameObject.GetComponent<CharacterController>().enabled = true;
        currentWeapon.SetActive(true);
        soldier.GetComponent<SoldierController>().SetOwner(this);
    }

    public SoldierController GetSoldierController()
    {
        return soldierController;
    }

    void OnApplicationQuit()
    {
        if (isHoldingSkull) { DropSkull(); }
    }
}
