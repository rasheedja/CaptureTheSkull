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

    [PunRPC]
    public override void HitRigidbody(Vector3 hitPoint)
    {
        GetComponent<Rigidbody>().AddForce(hitPoint, ForceMode.Impulse);
    }

    [PunRPC]
    public override void ExplodeRigidbody(float explosionForce, Vector3 explosionPosition, float explosionRadius)
    {
        GetComponent<Rigidbody>().AddExplosionForce(2500, explosionPosition, explosionRadius);
    }

    IEnumerator DeleteParticle(GameObject particle)
    {
        yield return new WaitForSeconds(particle.GetComponent<ParticleSystem>().main.duration);
        Destroy(particle);
    }
}
