using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AmmoManager : Photon.MonoBehaviour {

	// Use this for initialization
	void Start () {
        RotatePickup();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void RotatePickup()
    {
        // Rotate the item 360 Degrees. See: https://github.com/Demigiant/dotween/issues/179
        this.transform.DORotate(new Vector3(0, 360, 0), 5, RotateMode.FastBeyond360).SetRelative().SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != this.tag && (other.tag == "Blue" || other.tag == "Red"))
        {
            // If ammo replenished
            if (other.GetComponent<PlayerController>().ReplenishAmmo(this.tag))
            {
                photonView.RPC("HideItem", PhotonTargets.All, new object[] { 30 });
            }
        }
    }

    [PunRPC]
    private void HideItem(int seconds)
    {
        StartCoroutine(HideItemCR(seconds));
    }

    IEnumerator HideItemCR(int seconds)
    {
        foreach (MeshRenderer render in this.GetComponentsInChildren<MeshRenderer>())
        {
            render.enabled = false;
        }
        this.GetComponent<BoxCollider>().enabled = false;

        yield return new WaitForSeconds(seconds);

        foreach (MeshRenderer render in this.GetComponentsInChildren<MeshRenderer>())
        {
            render.enabled = true;
        }
        this.GetComponent<BoxCollider>().enabled = true;
    }
}
