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
    public float swapTime;
    // The reload and swap variables should use local transform values
    public float reloadXRotate;
    public float reloadYPosition;
    public float swapXRotate;
    public float swapYPosition;

    private bool isReloading = false;
    private bool isSwapping = false;
    private int currentAmmo;
    private Vector3 originalLocalPosition;
    private Vector3 originalLocalRotation;

    void Awake()
    {
        originalLocalPosition = this.transform.localPosition;
        originalLocalRotation = new Vector3(this.transform.localRotation.x, this.transform.localRotation.y, this.transform.localRotation.z);
    }

    void Start()
    {
        currentAmmo = magazineSize;
        UpdateAmmoCount();
    }

    void Update()
    {
        // Defaults: Left Mouse and Left Ctrl
        if (Input.GetButton("Fire1") && currentAmmo != 0 && !isReloading && !isSwapping)
        {
            StartCoroutine(Shoot());
        }

        // TODO: Figure out customisable inputs
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && !isSwapping && (currentAmmo != magazineSize) && reserveAmmo != 0)
        {
            StartCoroutine(Reload());
        }
    }

    // Update the ammo count in the UI
    public void UpdateAmmoCount()
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

    // Lower the weapon, used for swapping to a new weapon
    public IEnumerator LowerWeapon()
    {
        isSwapping = true;


        // The lower animation
        this.transform.DOLocalMoveY(swapYPosition, swapTime);
        this.transform.DOLocalRotate(new Vector3(swapXRotate, originalLocalRotation.y, originalLocalRotation.z), swapTime);
        yield return new WaitForSeconds(swapTime);

        isSwapping = false;
        this.gameObject.SetActive(false);
    }

    // Raise the weapon, used for swapping to a new weapon
    public IEnumerator RaiseWeapon()
    {
        isSwapping = true;

        // lower the gun so it is in the correct position when the animation starts
        this.transform.DOLocalMoveY(swapYPosition, 0);
        this.transform.DOLocalRotate(new Vector3(swapXRotate, originalLocalRotation.y, originalLocalRotation.z), 0);

        // Display the gun and update the ammo count
        this.gameObject.SetActive(true);
        UpdateAmmoCount();
        
        // Move weapon back so it's in the correct position when swapped to again
        this.transform.DOLocalMove(originalLocalPosition, swapTime);
        this.transform.DOLocalRotate(originalLocalRotation, swapTime);
        yield return new WaitForSeconds(swapTime);

        isSwapping = false;
    }

    /**
     * Used to just lower and disable the weapon without an animation. This is normally done when a player first spawns in
     * for their secondary weapons
     */
    public void DisableWeapon()
    {
        this.transform.DOLocalMoveY(swapYPosition, 0);
        this.transform.DOLocalRotate(new Vector3(swapXRotate, originalLocalRotation.y, originalLocalRotation.z), 0);
        this.gameObject.SetActive(false);
    }
}
