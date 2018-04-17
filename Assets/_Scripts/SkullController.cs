using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullController : Photon.MonoBehaviour {

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != this.tag && (other.tag == "Blue" || other.tag == "Red"))
        {
            other.GetComponent<PlayerController>().HoldSkull(this);
        }
    }

    [PunRPC]
    public void DisableSkull()
    {
        if (this.tag == "Blue")
        {
            UIManager.Instance.UpdateMessage("Blue Skull Stolen");
        }
        else
        {
            UIManager.Instance.UpdateMessage("Red Skull Stolen");
        }
        this.gameObject.SetActive(false);
    }

    [PunRPC]
    public void EnableSkull()
    {
        this.gameObject.SetActive(true);
    }
}
