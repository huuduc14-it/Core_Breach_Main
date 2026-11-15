using System.Collections;
using UnityEngine;

public enum MeleeType
{
    CyberDaggers,   // Unit-7: Song đoản đao nano chém nhanh liên hoàn, cơ động cao
    CyberKatana,    // Unit-0: Đại kiếm quét cung rộng 150°, sát thương lớn, phản đạn địch
    GravityHammer   // Unit-9: Búa trọng lực nện đất 360° sóng chấn động, hất văng và làm choáng
}

/// <summary>
/// Quản lý vũ khí cận chiến đặc trưng cho từng nhân vật theo phong cách Soul Knight:
/// - Unit-7: Nano Energy Daggers (Song đoản đao chém liên hoàn cực nhanh)
/// - Unit-0: Cyber Muramasa Katana (Đại kiếm chém quét rộng, chém nổ đạn địch)
/// - Unit-9: Gravity Thunder Hammer (Búa xung kích nện đất chấn động 360°, choáng quái)
/// </summary>
public class MeleeWeapon : Weapon
{
    [Header("Melee Specifics")]
    [SerializeField] private MeleeType meleeType = MeleeType.CyberKatana;
    [SerializeField] private float slashRadius = 2.4f;
    [SerializeField] private float slashAngle = 140f;
    [SerializeField] private float knockbackForce = 7.0f;
    [SerializeField] private float meleeCooldown = 0.30f;
    [SerializeField] private float baseDamage = 35f;

    [Header("Visual Effects")]
    [SerializeField] private Sprite slashSprite;
    [SerializeField] private Color slashColor = new Color(0f, 0.95f, 1f, 1f);

    private float nextMeleeTime = 0f;
    private HeatSystem heatSystemRef;
    private Transform playerRoot;
    private Rigidbody2D playerRb;

    private static Sprite cachedFallbackSlashSprite;

    public MeleeType CurrentMeleeType => meleeType;

    public void ConfigureMelee(string name, MeleeType type, float dmg, float radius, float angle, float cooldown, Color fxColor)
    {
        Configure(name, dmg, 1f / cooldown, 10f, 1, 0f);
        meleeType = type;
        baseDamage = dmg;
        slashRadius = radius;
        slashAngle = angle;
        meleeCooldown = cooldown;
        slashColor = fxColor;
    }

    private void Awake()
    {
        heatSystemRef = GetComponentInParent<HeatSystem>();
        if (heatSystemRef == null) heatSystemRef = GetComponent<HeatSystem>();

        playerRoot = transform.root;
        if (playerRoot != null)
        {
            playerRb = playerRoot.GetComponent<Rigidbody2D>();
        }

        EnsureSlashSprite();
    }

    private void EnsureSlashSprite()
    {
        if (slashSprite != null) return;

        slashSprite = Resources.Load<Sprite>("Art/VFX/slash_arc");
        if (slashSprite != null) return;

        if (cachedFallbackSlashSprite == null)
        {
            int texSize = 128;
            Texture2D tex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
            Color clear = new Color(0, 0, 0, 0);
            for (int y = 0; y < texSize; y++)
            {
                for (int x = 0; x < texSize; x++)
                {
                    tex.SetPixel(x, y, clear);
                }
            }

            Vector2 center = new Vector2(texSize * 0.5f, texSize * 0.5f);
            for (int r = 26; r <= 60; r++)
            {
                for (int deg = -70; deg <= 70; deg++)
                {
                    float rad = deg * Mathf.Deg2Rad;
                    float distFromEdge = Mathf.Abs(r - 54);
                    if (distFromEdge > 12) continue;

                    float edgeFactor = 1f - (distFromEdge / 12f);
                    float angFactor = Mathf.Cos(deg * 90f / 70f * Mathf.Deg2Rad);
                    if (angFactor < 0) angFactor = 0;

                    float alpha = Mathf.Clamp01(Mathf.Pow(edgeFactor, 1.4f) * angFactor);
                    if (alpha <= 0.02f) continue;

                    Color pixelColor = distFromEdge <= 2 
                        ? Color.white 
                        : Color.Lerp(new Color(0f, 0.9f, 1f), Color.white, edgeFactor);
                    pixelColor.a = alpha;

                    int px = Mathf.RoundToInt(center.x + r * Mathf.Cos(rad));
                    int py = Mathf.RoundToInt(center.y + r * Mathf.Sin(rad));
                    if (px >= 0 && px < texSize && py >= 0 && py < texSize)
                    {
                        tex.SetPixel(px, py, pixelColor);
                    }
                }
            }
            tex.Apply();
            cachedFallbackSlashSprite = Sprite.Create(tex, new Rect(0, 0, texSize, texSize), new Vector2(0.35f, 0.5f), 64f);
        }

        slashSprite = cachedFallbackSlashSprite;
    }

    public override bool TryShoot(Vector2 direction)
    {
        if (Time.time < nextMeleeTime) return false;

        if (heatSystemRef != null && !heatSystemRef.CanShoot())
        {
            return false;
        }

        nextMeleeTime = Time.time + meleeCooldown;

        // Tùy theo loại vũ khí cận chiến mà thực hiện hành vi tương ứng
        if (meleeType == MeleeType.GravityHammer)
        {
            PerformHammerSlam();
        }
        else
        {
            PerformSlash(direction);
        }

        // Tích nhiệt nhẹ
        if (heatSystemRef != null)
        {
            heatSystemRef.AddHeat(meleeType == MeleeType.GravityHammer ? 15f : 10f);
        }

        return true;
    }

    // 1. CHÉM KIẾM HOẶC SONG ĐOẢN ĐAO (KATANA / DAGGERS)
    private void PerformSlash(Vector2 direction)
    {
        EnsureSlashSprite();

        // Kích hoạt hoạt ảnh vung tay và vung vũ khí cận chiến trên PlayerVisuals (Chỉ vung tay vũ khí, giữ chân vững dưới đất)
        PlayerVisuals visuals = GetComponentInParent<PlayerVisuals>();
        if (visuals == null) visuals = GetComponent<PlayerVisuals>();
        if (visuals == null) visuals = GetComponentInChildren<PlayerVisuals>();
        if (visuals != null)
        {
            visuals.TriggerMeleeSwing(meleeType, direction);
        }

        // Vệt chém phát sáng
        StartCoroutine(ShowSlashEffect(direction));

        // Rung nhẹ màn hình và âm thanh
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.Shake(0.06f, 0.08f);
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDash();
        }

        // Tia lửa chém không khí
        CombatFeedback.SpawnSparks((Vector2)transform.position + direction.normalized * 0.8f, direction, slashColor, 5);

        // Kiểm tra va chạm gây sát thương
        Vector2 slashOrigin = transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(slashOrigin, slashRadius);

        int hitCount = 0;
        foreach (Collider2D col in hits)
        {
            if (col.CompareTag("Enemy") || col.GetComponent<EnemyHealth>() != null)
            {
                Vector2 toTarget = ((Vector2)col.transform.position - slashOrigin).normalized;
                float angle = Vector2.Angle(direction, toTarget);

                if (angle <= slashAngle * 0.5f)
                {
                    EnemyHealth health = col.GetComponent<EnemyHealth>();
                    if (health != null)
                    {
                        health.TakeDamage(baseDamage);
                        hitCount++;
                        CombatFeedback.SpawnSparks(col.transform.position, toTarget, Color.yellow, 8);
                    }

                    Rigidbody2D enemyRb = col.GetComponent<Rigidbody2D>();
                    if (enemyRb != null)
                    {
                        enemyRb.linearVelocity = toTarget * knockbackForce;
                    }
                }
            }
            // Chém phá hủy đạn của địch (Parry)
            else if (col.GetComponent<Bullet>() != null)
            {
                Bullet bullet = col.GetComponent<Bullet>();
                if (bullet != null && !bullet.IsPlayerBullet)
                {
                    CombatFeedback.ShowNotice(col.transform.position, "PARRIED!", new Color(1f, 0.85f, 0.2f));
                    CombatFeedback.SpawnSparks(col.transform.position, -direction, Color.yellow, 8);
                    Destroy(col.gameObject);
                }
            }
        }

        if (hitCount > 0)
        {
            CombatFeedback.HitStop(0.06f);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayEnemyHit();
        }
    }

    // 2. NỆN BÚA TRỌNG LỰC 360 ĐỘ (GRAVITY HAMMER SLAM)
    private void PerformHammerSlam()
    {
        Vector2 origin = transform.position;

        // Kích hoạt hoạt ảnh giơ búa lên trời rồi nện mạnh xuống sàn
        PlayerVisuals visuals = GetComponentInParent<PlayerVisuals>();
        if (visuals == null) visuals = GetComponent<PlayerVisuals>();
        if (visuals == null) visuals = GetComponentInChildren<PlayerVisuals>();
        if (visuals != null)
        {
            visuals.TriggerMeleeSwing(MeleeType.GravityHammer, Vector2.down);
        }

        // Rung chấn màn hình cực mạnh
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.Shake(0.14f, 0.12f);
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossRoar();
        }

        // Bắn sóng xung kích chấn động hình tròn 360 độ
        CombatFeedback.SpawnSparks(origin, Vector2.up, slashColor, 18);
        CombatFeedback.ShowNotice(origin + Vector2.up * 0.6f, "THUNDER SLAM!", new Color(1f, 0.8f, 0.1f));

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, slashRadius * 1.3f);
        int hitCount = 0;

        foreach (Collider2D col in hits)
        {
            if (col.CompareTag("Enemy") || col.GetComponent<EnemyHealth>() != null)
            {
                EnemyHealth health = col.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.TakeDamage(baseDamage);
                    hitCount++;
                }

                Vector2 pushDir = ((Vector2)col.transform.position - origin).normalized;
                if (pushDir == Vector2.zero) pushDir = Random.insideUnitCircle.normalized;

                Rigidbody2D enemyRb = col.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    enemyRb.linearVelocity = pushDir * (knockbackForce * 1.6f); // Hất văng cực xa
                }

                CombatFeedback.SpawnSparks(col.transform.position, pushDir, Color.yellow, 10);
            }
            // Sóng chấn động phá hủy toàn bộ đạn xung quanh
            else if (col.GetComponent<Bullet>() != null)
            {
                Bullet bullet = col.GetComponent<Bullet>();
                if (bullet != null && !bullet.IsPlayerBullet)
                {
                    Destroy(col.gameObject);
                }
            }
        }

        if (hitCount > 0)
        {
            CombatFeedback.HitStop(0.08f);
        }
    }

    private IEnumerator ShowSlashEffect(Vector2 direction)
    {
        GameObject arc = new GameObject("Melee_SlashArc");
        arc.transform.position = transform.position + (Vector3)(direction.normalized * 0.95f);
        float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arc.transform.rotation = Quaternion.Euler(0f, 0f, rotZ);

        SpriteRenderer sr = arc.AddComponent<SpriteRenderer>();
        sr.sprite = slashSprite;
        sr.sortingOrder = 30;
        sr.color = slashColor;

        Vector3 startScale = meleeType == MeleeType.CyberDaggers ? new Vector3(0.8f, 0.5f, 1f) : new Vector3(1.0f, 0.7f, 1f);
        Vector3 targetScale = meleeType == MeleeType.CyberDaggers ? new Vector3(1.8f, 1.3f, 1f) : new Vector3(2.5f, 2.0f, 1f);

        float elapsed = 0f;
        float duration = meleeType == MeleeType.CyberDaggers ? 0.10f : 0.15f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (sr != null)
            {
                sr.color = new Color(slashColor.r, slashColor.g, slashColor.b, 1f - Mathf.Pow(t, 2f));
                arc.transform.localScale = Vector3.Lerp(startScale, targetScale, Mathf.Sin(t * Mathf.PI * 0.5f));
            }
            yield return null;
        }

        Destroy(arc);
    }
}
