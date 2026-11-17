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
    [SerializeField] private int projectileCount = 1;    // Số tia đạn mỗi lần bắn
    [SerializeField] private float spreadAngle = 0f;     // Độ tán xạ đạn (độ)
    [SerializeField] private Sprite weaponSprite;

    [Header("Prefabs & References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    private HeatSystem heatSystem;
    private float nextFireTime = 0f;

    public string WeaponName => weaponName;
    public float Damage => damage;
    public int ProjectileCount => projectileCount;
    public Sprite WeaponSprite { get => weaponSprite; set => weaponSprite = value; }

    public void Configure(string name, float dmg, float rate, float heat, int projCount = 1, float spread = 0f, Sprite spr = null)
    {
        weaponName = name;
        damage = dmg;
        fireRate = rate;
        heatPerShot = heat;
        projectileCount = projCount;
        spreadAngle = spread;
        if (spr != null) weaponSprite = spr;
    }

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
    /// Thử bắn / tấn công theo hướng chỉ định
    /// </summary>
    public virtual bool TryShoot(Vector2 direction)
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
            Bullet existing = FindFirstObjectByType<Bullet>();
            if (existing != null) bulletPrefab = existing.gameObject;
        }

        if (bulletPrefab == null)
        {
            Debug.LogWarning($"[Weapon] Chưa gán bulletPrefab cho vũ khí {weaponName}!");
            return;
        }

        if (projectileCount <= 1)
        {
            SpawnSingleBullet(direction, damage);
        }
        else
        {
            float startAngle = -spreadAngle * 0.5f;
            float step = spreadAngle / (projectileCount - 1);
            for (int i = 0; i < projectileCount; i++)
            {
                float currentAngle = startAngle + (step * i);
                Vector2 spreadDir = Quaternion.Euler(0, 0, currentAngle) * direction;
                SpawnSingleBullet(spreadDir, damage);
            }
        }
    }

    private void SpawnSingleBullet(Vector2 direction, float dmg)
    {
        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Launch(direction, dmg);
        }
    }
}
