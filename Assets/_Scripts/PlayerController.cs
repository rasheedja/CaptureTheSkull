using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour {
    public GameObject primaryWeapon;
    public GameObject secondaryWeapon;
    public GameObject heldSkull;

    private GameObject currentWeapon;
    private bool isHoldingSkull;
    private SkullController skullController;
    private GameObject soldier; //The soldier model. This represents the players position to other players in the network, stores the player's health, and is also shown when the player dies.
    private SoldierController soldierController;

    void Start () {
        currentWeapon = primaryWeapon;
        secondaryWeapon.GetComponent<GunController>().DisableWeapon();
        heldSkull.SetActive(false);
        currentWeapon.GetComponent<GunController>().UpdateAmmoCount();

        Debug.Log("player controller started");
        if (this.tag == "Blue")
        {
            Debug.Log("spawning soldier");
            soldier = PhotonNetwork.Instantiate("Soldier_B", this.transform.position, this.transform.rotation, 0);
            Debug.Log("soldier spawned");
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

    void Update() {
        // First, check that we can still move
        if (this.gameObject.GetComponent<CharacterController>().enabled)
        {
            // Defaults: Left Mouse and Left Ctrl
            if (Input.GetButton("Fire1"))
            {
                currentWeapon.GetComponent<GunController>().Shoot(soldierController);
            }

            // TODO: Figure out customisable inputs
            if (Input.GetKeyDown(KeyCode.R))
            {
                currentWeapon.GetComponent<GunController>().Reload();
            }

            if ((Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) && !primaryWeapon.activeInHierarchy && !heldSkull.activeInHierarchy)
            {
                StartCoroutine(SwapWeapon(secondaryWeapon, primaryWeapon));
                soldierController.photonView.RPC("SelectPrimaryWeapon", PhotonTargets.AllBuffered);
            }

            if ((Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) && !secondaryWeapon.activeInHierarchy)
            {
                StartCoroutine(SwapWeapon(primaryWeapon, secondaryWeapon));
                soldierController.photonView.RPC("SelectSecondaryWeapon", PhotonTargets.AllBuffered);
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

        soldier.transform.position = new Vector3(this.transform.position.x, this.transform.position.y -0.95f, this.transform.position.z);
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
                SoldierController hitSoldierController = hitGameObject.GetComponent<SoldierController>();
                hitSoldierController.photonView.RPC("DecreaseHealth", PhotonTargets.All, new object[] { 10 });
                if (hitSoldierController.getIsDead())
                {
                    GameManager.Instance.IncrementKills();
                }
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
            StartCoroutine(SwapWeapon(primaryWeapon, secondaryWeapon));
        }

        skullController.photonView.RPC("DisableSkull", PhotonTargets.AllBuffered);
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
        }
        else
        {
            GameManager.Instance.photonView.RPC("IncrementRedScore", PhotonTargets.AllBuffered);
        }
        GameManager.Instance.IncrementSkullCaptures();
    }

    IEnumerator SwapWeapon(GameObject weaponFrom, GameObject weaponTo)
    {
        GunController weaponFromScript = weaponFrom.GetComponent<GunController>();
        GunController weaponToScript = weaponTo.GetComponent<GunController>();

        if (!weaponFromScript.IsBusy())
        {
            StartCoroutine(weaponFromScript.LowerWeapon());
            yield return new WaitForSeconds(weaponFromScript.swapTime);
            StartCoroutine(weaponToScript.RaiseWeapon());
            currentWeapon = weaponTo;
        }
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
        soldier.GetComponent<SoldierController>().SetOwner(null);
        soldier.GetComponent<SoldierController>().photonView.RPC("Despawn", PhotonTargets.All, new object[] { 10 });

        GameObject newSpawn = GameManager.Instance.GetRandomBlueSpawn();
        this.transform.DORotate(newSpawn.transform.rotation.eulerAngles, 2);
        this.transform.DOMove(newSpawn.transform.position, 2);  
        yield return new WaitForSeconds(3);

        soldier = PhotonNetwork.Instantiate("Soldier_B", this.transform.position, this.transform.rotation, 0);
        this.gameObject.GetComponent<CharacterController>().enabled = true;
        currentWeapon.SetActive(true);
        soldier.GetComponent<SoldierController>().SetOwner(this);
    }
}
