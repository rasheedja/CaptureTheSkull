using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Controls Soldiers over the network
 * Logic based on networking lab
 */
public class SoldierController : Photon.MonoBehaviour {
    // The actual models that are held by soldiers
    public GameObject primaryWeapon;
    public GameObject secondaryWeapon;
    public GameObject heldSkull;

    private Vector3 correctPlayerPos;
    private Quaternion correctPlayerRot;
    private Vector3 lastPosition;
    private PlayerController owner;
    private int maxHealth;
    private int health = 100;
    private bool isDead = false;
    private bool isHoldingSkull = false;

    private Animator animatorController;
    private int movingForwardHashId;
    private int movingBackwardHashId;
    private int movingLeftHashId;
    private int movingRightHashId;
    private int jumpHashId;


    void Awake()
    {
        correctPlayerPos = transform.position;
        correctPlayerRot = transform.rotation;

        animatorController = this.gameObject.GetComponent<Animator>();
        movingForwardHashId = Animator.StringToHash("movingForward");
        movingBackwardHashId = Animator.StringToHash("movingBackward");
        movingLeftHashId = Animator.StringToHash("movingLeft");
        movingRightHashId = Animator.StringToHash("movingRight");
        jumpHashId = Animator.StringToHash("jump");
    }

    void Start () {
        if (photonView.isMine) {
            // Hide the soldier from the owner
            foreach (SkinnedMeshRenderer renderer in this.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                renderer.enabled = false;
            }
            // Hide the guns from the owner
            foreach (MeshRenderer renderer in this.gameObject.GetComponentsInChildren<MeshRenderer>(true))
            {
                renderer.enabled = false;
            }
            // Disable colliders for the hidden soldier
            foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
            }

            maxHealth = health;
            UIManager.Instance.UpdateHealth(health);
        }
	}
	
	void Update () {
        // REMOVE ME: ONLY FOR TESTING
        if (Input.GetKeyDown(KeyCode.P))
        {
            DecreaseHealth(100);
        }
        if (!photonView.isMine)
        {
            transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * 10f);
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);

        }
        else
        {
            correctPlayerPos = (Vector3) stream.ReceiveNext();
            correctPlayerRot = (Quaternion) stream.ReceiveNext();
        }
    }

    public void SetOwner(PlayerController owner)
    {
        this.owner = owner;
    }

    public PlayerController GetOwner()
    {
        return owner;
    }

    [PunRPC]
    public void DecreaseHealth(int amount)
    {
        if (!isDead)
        {
            Debug.Log("decreasing health");
            health -= amount;
            if (health <= 0)
            {
                health = 0;
                photonView.RPC("Die", PhotonTargets.AllBuffered);
            }

            if (photonView.isMine)
            {
                UIManager.Instance.UpdateHealth(health);
            }
        }
    }

    [PunRPC]
    public void IncreaseHealth(int amount)
    {
        if (!isDead)
        {
            health += amount;
            if (health > maxHealth)
            {
                health = maxHealth;
            }

            if (photonView.isMine)
            {
                UIManager.Instance.UpdateHealth(health);
            }
        }
    }

    [PunRPC]
    public void Die()
    {
        if (!isDead)
        {
            // If it's mine, re-enable renderers and colliders
            if (photonView.isMine)
            {
                foreach (SkinnedMeshRenderer renderer in this.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    renderer.enabled = true;
                }
                foreach (MeshRenderer renderer in this.gameObject.GetComponentsInChildren<MeshRenderer>(true))
                {
                    // Render everything apart from the gun flash
                    if (renderer.gameObject.name != "FX_GunFlash")
                    {
                        renderer.enabled = true;
                    }
                }
                foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>(true))
                {
                    collider.enabled = true;
                }

                StartCoroutine(owner.Die());
            }

            isDead = true;
            this.gameObject.GetComponent<RagdollController>().EnableRagdoll();

            if (isHoldingSkull)
            {
                if (this.tag == "BlueSoldier")
                {
                    UIManager.Instance.UpdateMessage("Red Skull Recovered");
                }
                else
                {
                    UIManager.Instance.UpdateMessage("Blue Skull Recovered");
                }
            }
        }
    }

    [PunRPC]
    public void Despawn(int despawnTime)
    {
        owner = null;
        if (photonView.isMine)
        {
            StartCoroutine(DespawnCR(despawnTime));
        }
    }

    IEnumerator DespawnCR(int despawnTime)
    {
        yield return new WaitForSeconds(despawnTime);
        PhotonNetwork.Destroy(this.gameObject);
    }

    [PunRPC]
    public void SelectPrimaryWeapon()
    {
        primaryWeapon.SetActive(true);
        secondaryWeapon.SetActive(false);
    }

    [PunRPC]
    public void SelectSecondaryWeapon()
    {
        secondaryWeapon.SetActive(true);
        primaryWeapon.SetActive(false);
    }

    [PunRPC]
    public void ShowSkull()
    {
        heldSkull.SetActive(true);
        isHoldingSkull = true;
    }

    [PunRPC]
    public void HideSkull()
    {
        heldSkull.SetActive(false);
        isHoldingSkull = false;
    }

    [PunRPC]
    /**
     * Shoot the active weapon on the 3D model. This function will not check if we can or should shoot, it will just shoot.
     */
    public void ShootActiveWeapon()
    {
        if (!photonView.isMine)
        {
            if (primaryWeapon.GetActive())
            {
                StartCoroutine(ShootWeapon(primaryWeapon.GetComponent<GunController>()));
            }
            if (secondaryWeapon.GetActive())
            {
                StartCoroutine(ShootWeapon(secondaryWeapon.GetComponent<GunController>()));
            }
        }
    }

    IEnumerator ShootWeapon(GunController gunController)
    {
        gunController.shootAnim.Play();
        gunController.shootSound.Play();
        StartCoroutine(gunController.ShootFlash());
        yield return new WaitForSeconds(gunController.shootAnim.clip.length);
        gunController.shootAnim.Stop();
    }

    [PunRPC]
    public void MoveForward()
    {
        if (!photonView.isMine)
        {
            animatorController.SetBool(movingForwardHashId, true);
            animatorController.SetBool(movingBackwardHashId, false);
            animatorController.SetBool(movingLeftHashId, false);
            animatorController.SetBool(movingRightHashId, false);
        }
    }

    [PunRPC]
    public void MoveBackward()
    {
        if (!photonView.isMine)
        {
            animatorController.SetBool(movingForwardHashId, false);
            animatorController.SetBool(movingBackwardHashId, true);
            animatorController.SetBool(movingLeftHashId, false);
            animatorController.SetBool(movingRightHashId, false);
        }
    }

    [PunRPC]
    public void MoveLeft()
    {
        if (!photonView.isMine)
        {
            animatorController.SetBool(movingForwardHashId, false);
            animatorController.SetBool(movingBackwardHashId, false);
            animatorController.SetBool(movingLeftHashId, true);
            animatorController.SetBool(movingRightHashId, false);
        }
    }

    [PunRPC]
    public void MoveRight()
    {
        if (!photonView.isMine)
        {
            animatorController.SetBool(movingForwardHashId, false);
            animatorController.SetBool(movingBackwardHashId, false);
            animatorController.SetBool(movingLeftHashId, false);
            animatorController.SetBool(movingRightHashId, true);
        }
    }

    [PunRPC]
    public void Idle()
    {
        if (!photonView.isMine)
        {
            animatorController.SetBool(movingForwardHashId, false);
            animatorController.SetBool(movingBackwardHashId, false);
            animatorController.SetBool(movingLeftHashId, false);
            animatorController.SetBool(movingRightHashId, false);
        }
    }

    [PunRPC]
    public void Jump()
    {
        if (!photonView.isMine)
        {
            animatorController.SetTrigger(jumpHashId);
        }
    }

    public bool GetIsDead()
    {
        return isDead;
    }
}
