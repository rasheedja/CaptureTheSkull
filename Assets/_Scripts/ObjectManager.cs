using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : ObjectManagerBase {
    public GameObject hitParticle;

    [PunRPC]
    public override void Hit(Vector3 hitPoint, Quaternion shooterRotation)
    {
        GameObject particle = Instantiate(hitParticle, hitPoint, transform.rotation);
        particle.transform.rotation = shooterRotation;
        particle.transform.Rotate(new Vector3(particle.transform.rotation.x, 90));
        StartCoroutine(DeleteParticle(particle));
    }

    IEnumerator DeleteParticle(GameObject particle)
    {
        yield return new WaitForSeconds(particle.GetComponent<ParticleSystem>().main.duration);
        Destroy(particle);
    }
}
