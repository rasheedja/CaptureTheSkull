using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullController : Photon.MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != this.tag)
        {
            other.GetComponent<PlayerController>().HoldSkull(this);
        }
    }

    [PunRPC]
    public void DisableSkull()
    {
        this.gameObject.SetActive(false);
    }

    [PunRPC]
    public void EnableSkull()
    {
        this.gameObject.SetActive(true);
    }
}
