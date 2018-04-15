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
}
