using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManagerBase : MonoBehaviour {

    public virtual void Hit(Vector3 hitPoint, Quaternion shooterRotation) { }
}
