using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private BulletPool bulletPool;

    [Header("Stats")]
    [SerializeField] private float range;
    [SerializeField] private float damage;
    [SerializeField] private float fireRate;
    [SerializeField] private int magazineSize;
    [SerializeField] private float reloadTime;
    [SerializeField] private LayerMask hitMask;

    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject impactEffectPrefab;

    private int currentAmmo;
    private float nextFireTime = 0f;
    private bool isReloading = false;

    private void Start()
    {
        currentAmmo = magazineSize;
    }

    private void Update()
    {
    }

    public void TryFire()
    {
        if (isReloading || Time.time < nextFireTime || currentAmmo <= 0)
        {
            if (currentAmmo <= 0 && !isReloading)
            {
                Debug.Log("Out of ammo. Reloading...");
                StartReload();
            }
            return;
        }

        Fire();
        nextFireTime = Time.time + 1f / fireRate;
    }

    public void StartReload()
    {
        if (!isReloading && currentAmmo < magazineSize)
        {
            isReloading = true;
            Debug.Log("Reloading...");
            Invoke(nameof(FinishReload), reloadTime);
        }
    }

    private void FinishReload()
    {
        currentAmmo = magazineSize;
        isReloading = false;
        Debug.Log("Reload complete.");
    }

    private void Fire()
    {
        currentAmmo--;
        Debug.Log("Fired! Ammo left: " + currentAmmo);

        Vector3 targetPoint = playerCamera.transform.position + playerCamera.transform.forward * range;

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit camHit, range, hitMask))
        {
            targetPoint = camHit.point;
        }

        Vector3 direction = (targetPoint - firePoint.position).normalized;

        if (Physics.Raycast(firePoint.position, direction, out RaycastHit hit, range, hitMask))
        {
            Debug.Log("Hit: " + hit.collider.name);
            if (impactEffectPrefab)
                Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }

        if (muzzleFlash)
            muzzleFlash.Play();

        GameObject bulletObj = bulletPool.GetBullet();
        bulletObj.transform.position = firePoint.position;
        bulletObj.transform.rotation = Quaternion.LookRotation(direction);
        bulletObj.transform.Rotate(90f, 0f, 0f);

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
            bullet.Initialize(direction);
    }
}