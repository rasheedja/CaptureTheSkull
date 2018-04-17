using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// See: https://docs.unity3d.com/Manual/PartSysExplosion.html for a guide on particle explosions
public class GrenadeManager : ObjectManagerBase {
    public ParticleSystem explosion;
    public AudioSource explosionSound;

    void Start()
    {
        ArmGrenade(5);
    }

    [PunRPC]
    public override void HitRigidbody(Vector3 hitPoint)
    {
        GetComponent<Rigidbody>().AddForce(-hitPoint, ForceMode.Impulse);
    }

    public void ArmGrenade(int timer)
    {
        Invoke("Explode", timer);
        StartCoroutine(DestroyAfterParticles(timer));
    }

    public void Explode()
    {
        explosionSound.Play();
        explosion.Play();

        // See: https://docs.unity3d.com/ScriptReference/Physics.OverlapSphere.html for calculating explosion damage
        // See: https://forum.unity.com/threads/detect-if-raycast-hit-the-specific-object.497252/ for raycasting logic
        // I used the debug rays to see the radius of the explosion and whether the explosion would ignore players behind a wall or not
        float explosionRadius = 5f;
        Collider[] hitColliders = Physics.OverlapSphere(this.transform.position, explosionRadius);
        foreach (Collider hitCollider in hitColliders)
        {
            Debug.DrawRay(transform.position, (hitCollider.transform.position - transform.position), Color.yellow, 5f);
            RaycastHit hitRaycast;
            if (Physics.Raycast(transform.position, (hitCollider.transform.position - transform.position), out hitRaycast))
            {
                if (hitCollider == hitRaycast.collider)
                {
                    Debug.DrawRay(transform.position, (hitCollider.transform.position - transform.position), Color.red, 5f);
                    PlayerController playerController = hitCollider.transform.root.GetComponent<PlayerController>();
                    if (playerController != null)
                    {
                        Debug.DrawRay(transform.position, (hitCollider.transform.position - transform.position), Color.blue, 5f);
                        SoldierController soldierController = playerController.GetSoldierController();

                        playerController.gameObject.GetComponent<ImpactReceiver>().AddImpact(hitCollider.transform.position - transform.position, 100);
                        soldierController.DecreaseHealth(50);
                    }
                    else if (hitCollider.transform.root.tag == "BlueSoldier" || hitCollider.transform.root.tag == "RedSoldier")
                    {
                        Debug.DrawRay(transform.position, (hitCollider.transform.position - transform.position), Color.green, 5f);
                        hitCollider.GetComponent<ObjectManagerBase>().photonView.RPC("ExplodeRigidbody", PhotonTargets.All, new object[] { 5000f, transform.position, explosionRadius });
                    }
                }
            }
        }
    }

    IEnumerator DestroyAfterParticles(int timer)
    {
        yield return new WaitForSeconds(timer + 1f);
        PhotonNetwork.Destroy(this.gameObject);
    }
}
