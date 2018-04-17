using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * code based onLab 9
 */
public class RagdollController : Photon.MonoBehaviour
{
    void Awake()
    {
        foreach (Rigidbody component in this.GetComponentsInChildren<Rigidbody>())
        {
            component.isKinematic = true;
        }
    }

    public void EnableRagdoll()
    {
        this.GetComponent<Animator>().enabled = false;
        foreach (Rigidbody component in this.GetComponentsInChildren<Rigidbody>())
        {
            component.isKinematic = false;
        }
    }
}
