using System;
using UnityEngine;

/// <summary>1
/// Quản lý máu và hủy diệt kẻ địch khi HP <= 0 trong Core Breach:
/// - Sentry Drone: HP 20, Scrap 2
/// - Turret Bot: HP 15, Scrap 3
/// - Core Guardian: HP 180, Scrap 20
/// Tự động cộng Scrap vào GameManager và phát âm thanh tương ứng khi bị hạ.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 20f;
    [SerializeField] private int scrapReward = 2;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    public event Action OnDeath;

    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor = Color.white;
    private Coroutine flashRoutine;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    public void SetMaxHealth(float hp, int scrap)
    {
        maxHealth = hp;
        CurrentHealth = hp;
        scrapReward = scrap;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        CurrentHealth -= damage;
        Debug.Log($"[EnemyHealth] {gameObject.name} nhận {damage} sát thương. HP: {CurrentHealth}/{maxHealth}");

        // Chớp trắng khi trúng đòn (Không cấp phát component runtime)
        TriggerFlash();

        // Hiển thị số sát thương nảy lên (Floating Combat Text)
        bool isCrit = damage >= 25f;
        CombatFeedback.ShowDamage(transform.position, damage, isCrit, false);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyHit();
        }

        VfxSpawner.Spawn(VfxType.BulletImpact, transform.position);

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    private void TriggerFlash()
    {
        if (spriteRenderer == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(DamageFlashRoutine());
    }

    private System.Collections.IEnumerator DamageFlashRoutine()
    {
        spriteRenderer.color = new Color(1f, 0.3f, 0.3f, 1f); // Đỏ chớp nhẹ hoặc trắng
        yield return new WaitForSeconds(0.08f);
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }

    [Header("Loot Drops")]
    [SerializeField] private GameObject scrapPrefab;

    public void SetScrapPrefab(GameObject prefab)
    {
        scrapPrefab = prefab;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Rung nhẹ màn hình (Game Juice tinh tế)
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.Shake(0.06f, 0.08f);
        }

        // Kích hoạt Hit-Stop & Kill Streak Combo
        CombatFeedback.RegisterEnemyKill(transform.position, scrapReward);

        OnDeath?.Invoke();
        Debug.Log($"[EnemyHealth] {gameObject.name} bị tiêu diệt! Rơi {scrapReward} Scrap.");

        // Rơi ra các mảnh kim loại ScrapItem để người chơi thu thập
        SpawnScrapDrops();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDeath();
        }

        VfxSpawner.Spawn(VfxType.EnemyExplosion, transform.position);
        VfxSpawner.Spawn(VfxType.MetalShards, transform.position);

        Destroy(gameObject);
    }

    private static Sprite cachedScrapSprite;

    private void EnsureScrapPrefab()
    {
        if (scrapPrefab == null)
        {
            scrapPrefab = Resources.Load<GameObject>("ScrapItem");
        }
    }

    private void SpawnScrapDrops()
    {
        EnsureScrapPrefab();
        int count = Mathf.Max(1, scrapReward);
        float healPerScrap = scrapReward >= 15 ? 15f : (scrapReward >= 3 ? 8f : 6f);

        for (int i = 0; i < count; i++)
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * 0.7f;
            Vector3 dropPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            GameObject scrapObj = null;
            if (scrapPrefab != null)
            {
                scrapObj = Instantiate(scrapPrefab, dropPos, Quaternion.identity);
            }
            else
            {
                // Fallback tạo ScrapItem trực tiếp nếu prefab chưa gán
                scrapObj = CreateRuntimeScrapItem(dropPos);
            }

            if (scrapObj != null)
            {
                ScrapItem item = scrapObj.GetComponent<ScrapItem>();
                if (item != null)
                {
                    item.SetValues(1, healPerScrap, 2f);
                }
            }
        }
    }

    private GameObject CreateRuntimeScrapItem(Vector3 position)
    {
        GameObject scrapObj = new GameObject("ScrapItem_Drop");
        scrapObj.transform.position = position;
        SpriteRenderer sr = scrapObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 8;
        
        if (cachedScrapSprite == null)
        {
            cachedScrapSprite = Resources.Load<Sprite>("scrap_gem");
        }
        if (cachedScrapSprite != null) sr.sprite = cachedScrapSprite;
        sr.color = new Color(0.2f, 1f, 0.4f, 1f); // Xanh ngọc hồi máu

        CircleCollider2D col = scrapObj.AddComponent<CircleCollider2D>();
        col.radius = 0.35f;
        col.isTrigger = true;

        scrapObj.AddComponent<ScrapItem>();
        return scrapObj;
    }
}
