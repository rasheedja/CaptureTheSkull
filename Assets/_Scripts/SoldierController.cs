using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Controls Soldiers over the network
 * Logic based on networking lab
 */
public class SoldierController : Photon.MonoBehaviour {
    private Vector3 correctPlayerPos;
    private Quaternion correctPlayerRot;
    private PlayerController owner;
    private int maxHealth;
    private int health = 100;

    void Awake()
    {
        correctPlayerPos = transform.position;
        correctPlayerRot = transform.rotation;
    }

    void Start () {
		if (photonView.isMine) {
            // Hide the soldier from the owner
            foreach (SkinnedMeshRenderer renderer in this.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                renderer.enabled = false;
            }
            // Disable colliders for the hidden soldier
            foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }

            maxHealth = health;
            UIManager.Instance.UpdateHealth(health);
        }
	}
	
	void Update () {
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

    [PunRPC]
    public void DecreaseHealth(int amount)
    {
        Debug.Log("decreasing health");
        health -= amount;
        if (health <= 0)
        {
            health = 0;
            photonView.RPC("Die", PhotonTargets.All);
        }

        if (photonView.isMine)
        {
            Debug.Log("mine");
            UIManager.Instance.UpdateHealth(health);
        }
    }

    [PunRPC]
    public void IncreaseHealth(int amount)
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

    [PunRPC]
    public void Die()
    {
        // If it's mine, re-enable renderers and colliders
        if (photonView.isMine)
        {
            foreach (SkinnedMeshRenderer renderer in this.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                renderer.enabled = true;
            }
            foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>())
            {
                collider.enabled = true;
            }

            StartCoroutine(owner.Die());
        }

        this.gameObject.GetComponent<RagdollController>().EnableRagdoll();
    }

    [PunRPC]
    public void Despawn(int despawnTime)
    {
        StartCoroutine(DespawnCR(despawnTime));
    }

    IEnumerator DespawnCR(int despawnTime)
    {
        yield return new WaitForSeconds(despawnTime);
        Destroy(this.gameObject);
    }
}
