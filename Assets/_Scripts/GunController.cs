using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GunController : MonoBehaviour {
    public AudioSource shootSound;
    public Animation shootAnim;
    public MeshRenderer shootFlash;
    public int magazineSize;
    public int reserveAmmo;
    public float reloadTime;
    // The reload variables should use local transform values
    public float reloadXRotate;
    public float reloadYPosition;

    private bool isReloading = false;
    private int currentAmmo;

    void Start()
    {
        currentAmmo = magazineSize;
        UpdateAmmoCount();
    }

    void Update()
    {
        // Defaults: Left Mouse and Left Ctrl
        if (Input.GetButton("Fire1") && currentAmmo != 0)
        {
            StartCoroutine(Shoot());
        }

        // TODO: Figure out customisable inputs
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && (currentAmmo != magazineSize) && reserveAmmo != 0)
        {
            StartCoroutine(Reload());
        }
    }

    // Update the ammo count in the UI
    private void UpdateAmmoCount()
    {
        UIManager.Instance.UpdateAmmo(currentAmmo, reserveAmmo);
    }

    // Play the shooting animation and deplete ammo from the gun
    IEnumerator Shoot()
    {
        if (!shootAnim.isPlaying)
        {
            shootSound.Play();
            shootAnim.Play();
            StartCoroutine(ShootFlash());
            UIManager.Instance.PhysicsRaycasts(transform.rotation);
            yield return new WaitForSeconds(shootAnim.clip.length);
            shootAnim.Stop();
            currentAmmo--;
            UpdateAmmoCount();
        }
    }

    // Animates the flash effect at the barrel of the gun
    IEnumerator ShootFlash()
    {
        shootFlash.enabled = true;
        yield return new WaitForSeconds(0.05f);
        shootFlash.enabled = false;
    }

    // Play the reload animation and then reload the gun
    IEnumerator Reload()
    {
        isReloading = true;

        // Save original transforms to move gun back to original position
        Vector3 originalLocalPosition = this.transform.localPosition;
        Vector3 originalLocalRotation = new Vector3(this.transform.localRotation.x, this.transform.localRotation.y, this.transform.localRotation.z);
        float halfReloadTime = reloadTime / 2;

        // The reload animation
        this.transform.DOLocalMoveY(reloadYPosition, halfReloadTime);
        this.transform.DOLocalRotate(new Vector3(reloadXRotate, originalLocalRotation.y, originalLocalRotation.z), halfReloadTime);
        yield return new WaitForSeconds(halfReloadTime);
        this.transform.DOLocalMove(originalLocalPosition, halfReloadTime);
        this.transform.DOLocalRotate(originalLocalRotation, halfReloadTime);
        yield return new WaitForSeconds(halfReloadTime);

        // Deplete reserve ammo and fill up current ammo;
        reserveAmmo -= magazineSize - currentAmmo;
        currentAmmo = magazineSize;
        UpdateAmmoCount();

        isReloading = false;
    }
}
