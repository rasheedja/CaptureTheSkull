using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Most of this code is from Lab 9
 */
public class RagdollController : MonoBehaviour
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
