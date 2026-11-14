using UnityEngine;

/// <summary>
/// Đại diện cho 1 khẩu súng của Player trong Core Breach
/// </summary>
public class Weapon : MonoBehaviour
{
    [Header("Weapon Info")]
    [SerializeField] private string weaponName = "Laser Rifle";
    [SerializeField] private float damage = 15f;
    [SerializeField] private float fireRate = 5f;        // Số phát mỗi giây
    [SerializeField] private float heatPerShot = 10f;

    [Header("Prefabs & References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    private HeatSystem heatSystem;
    private float nextFireTime = 0f;

    public string WeaponName => weaponName;

    private void Awake()
    {
        heatSystem = GetComponentInParent<HeatSystem>();
        if (heatSystem == null) heatSystem = GetComponent<HeatSystem>();
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    /// <summary>
    /// Thử bắn theo hướng chỉ định
    /// </summary>
    public bool TryShoot(Vector2 direction)
    {
        if (Time.time < nextFireTime) return false;

        // Kiểm tra thanh nhiệt
        if (heatSystem != null && !heatSystem.CanShoot())
        {
            return false;
        }

        // Thực hiện bắn
        nextFireTime = Time.time + (1f / fireRate);
        SpawnBullet(direction);

        // Tích nhiệt
        if (heatSystem != null)
        {
            heatSystem.AddHeat(heatPerShot);
        }

        // Rung nhẹ màn hình khi bắn súng (Game Juice)
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.Shake(0.04f, 0.06f);
        }

        // Phát âm thanh bắn
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayShoot();
        }

        return true;
    }

    private void SpawnBullet(Vector2 direction)
    {
        if (bulletPrefab == null)
        {
            Bullet existing = FindObjectOfType<Bullet>();
            if (existing != null) bulletPrefab = existing.gameObject;
        }

        if (bulletPrefab == null)
        {
            Debug.LogWarning($"[Weapon] Chưa gán bulletPrefab cho vũ khí {weaponName}!");
            return;
        }

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Launch(direction, damage);
        }
    }
}
