using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManagerBase : Photon.MonoBehaviour {

    [PunRPC]
    public virtual void Hit(Vector3 hitPoint, Quaternion shooterRotation) { }

    [PunRPC]
    public virtual void HitRigidbody(Vector3 hitPoint) { }
}
