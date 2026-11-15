using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hệ thống Combat Feel & Game Juice đỉnh cao cho Core Breach:
/// 1. Floating Combat Text (Số sát thương nảy lên màu sắc neon theo phong cách Hades / Enter the Gungeon)
/// 2. Hit-Stop (Micro freeze-frame 2-3 frame khi đánh trúng tạo cảm giác chém/bắn có lực)
/// 3. Kill Streak & Combo Multiplier (Dopamine loop khi xả đạn liên hoàn)
/// 4. Impact Sparks (Tia lửa điện công nghệ cao nảy ra khi va chạm)
/// 5. Low HP Vignette & Overheat Pulse (Hiệu ứng cảnh báo màn hình)
/// </summary>
public class CombatFeedback : MonoBehaviour
{
    public static CombatFeedback Instance { get; private set; }

    // =========================================================
    // FLOATING COMBAT TEXT
    // =========================================================

    private class FloatingText
    {
        public Vector3 worldPosition;
        public Vector2 velocity;
        public string text;
        public Color color;
        public float scale;
        public float lifeTime;
        public float maxLifeTime;
        public bool isCrit;
    }

    private readonly List<FloatingText> activeTexts = new List<FloatingText>(64);
    private readonly List<SparkParticle> activeSparks = new List<SparkParticle>(128);

    private class SparkParticle
    {
        public Vector3 position;
        public Vector2 velocity;
        public Color color;
        public float lifeTime;
        public float maxLifeTime;
        public float size;
    }

    // Texture trắng 1x1 dùng để vẽ hình học và hiệu ứng
    private static Texture2D whitePixel;

    // =========================================================
    // HIT-STOP
    // =========================================================

    private Coroutine hitStopRoutine;

    // =========================================================
    // COMBO / KILL STREAK SYSTEM
    // =========================================================

    [Header("Combo System")]
    [SerializeField] private float comboWindow = 3.2f;
    private int currentCombo = 0;
    private float comboTimer = 0f;
    private float overdriveTimer = 0f;
    public bool IsOverdriveActive => overdriveTimer > 0f;
    public int CurrentCombo => currentCombo;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        EnsureWhitePixel();
    }

    private static void EnsureWhitePixel()
    {
        if (whitePixel == null)
        {
            whitePixel = new Texture2D(1, 1);
            whitePixel.SetPixel(0, 0, Color.white);
            whitePixel.Apply();
        }
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;

        // 1. Cập nhật Floating Combat Text
        for (int i = activeTexts.Count - 1; i >= 0; i--)
        {
            var item = activeTexts[i];
            item.lifeTime -= dt;
            if (item.lifeTime <= 0f)
            {
                activeTexts.RemoveAt(i);
                continue;
            }

            // Bay lên và chậm dần
            item.worldPosition += (Vector3)item.velocity * dt;
            item.velocity *= Mathf.Pow(0.85f, dt * 60f);
        }

        // 2. Cập nhật Tia lửa Spark Particles
        for (int i = activeSparks.Count - 1; i >= 0; i--)
        {
            var sp = activeSparks[i];
            sp.lifeTime -= dt;
            if (sp.lifeTime <= 0f)
            {
                activeSparks.RemoveAt(i);
                continue;
            }

            sp.position += (Vector3)sp.velocity * dt;
            sp.velocity *= Mathf.Pow(0.88f, dt * 60f);
        }

        // 3. Cập nhật Combo Streak
        if (comboTimer > 0f)
        {
            comboTimer -= dt;
            if (comboTimer <= 0f)
            {
                currentCombo = 0;
            }
        }

        // 4. Cập nhật Overdrive Buff
        if (overdriveTimer > 0f)
        {
            overdriveTimer -= dt;
        }
    }

    // =========================================================
    // PUBLIC STATIC API
    // =========================================================

    /// <summary>
    /// Hiển thị sát thương nảy lên tại vị trí mục tiêu
    /// </summary>
    public static void ShowDamage(Vector3 worldPos, float damage, bool isCrit = false, bool isPlayerDamage = false)
    {
        EnsureInstance();
        if (Instance == null) return;

        Color textColor;
        string textStr = Mathf.CeilToInt(damage).ToString();

        if (isPlayerDamage)
        {
            textColor = new Color(1f, 0.25f, 0.25f, 1f); // Đỏ khi Player dính đòn
            textStr = "-" + textStr;
        }
        else if (isCrit)
        {
            textColor = new Color(1f, 0.85f, 0.1f, 1f); // Vàng Gold cho đòn chém Crit hoặc Scatter
            textStr = "CRIT " + textStr + "!";
        }
        else
        {
            textColor = new Color(0f, 0.95f, 1f, 1f); // Cyan Neon cho đạn bắn trúng quái
        }

        Instance.SpawnFloatingText(worldPos, textStr, textColor, isCrit ? 1.4f : 1.0f, isCrit);
    }

    /// <summary>
    /// Hiển thị thông điệp văn bản chiến thuật (DEFLECTED, +HP, HACKED)
    /// </summary>
    public static void ShowNotice(Vector3 worldPos, string message, Color color)
    {
        EnsureInstance();
        if (Instance == null) return;
        Instance.SpawnFloatingText(worldPos, message, color, 1.25f, true);
    }

    /// <summary>
    /// Kích hoạt Hit-Stop (Micro-freeze 2-3 frame tạo cảm giác đòn đánh lực)
    /// </summary>
    public static void HitStop(float duration = 0.045f)
    {
        EnsureInstance();
        if (Instance != null)
        {
            Instance.DoHitStop(duration);
        }
    }

    /// <summary>
    /// Ghi nhận 1 mạng hạ gục để kích hoạt Kill Streak / Combo
    /// </summary>
    public static void RegisterEnemyKill(Vector3 enemyPos, int rewardScrap)
    {
        EnsureInstance();
        if (Instance == null) return;

        Instance.currentCombo++;
        Instance.comboTimer = Instance.comboWindow;

        // Hit-stop nhẹ khi tiêu diệt quái (0.04s)
        Instance.DoHitStop(0.04f);

        // Bắn tia lửa nổ
        Instance.SpawnSparksInternal(enemyPos, UnityEngine.Random.insideUnitCircle, new Color(1f, 0.8f, 0.2f), 8);

        // Chuỗi hạ gục
        if (Instance.currentCombo == 2)
        {
            Instance.SpawnFloatingText(enemyPos + Vector3.up * 0.8f, "x2 DOUBLE KILL!", new Color(0f, 1f, 0.8f), 1.3f, true);
        }
        else if (Instance.currentCombo == 4)
        {
            Instance.SpawnFloatingText(enemyPos + Vector3.up * 0.8f, "x4 MEGA STREAK!", new Color(1f, 0.7f, 0.1f), 1.5f, true);
        }
        else if (Instance.currentCombo >= 6)
        {
            Instance.overdriveTimer = 4.0f; // Kích hoạt 4 giây tăng tốc & hồi nhiệt
            Instance.SpawnFloatingText(enemyPos + Vector3.up * 0.8f, "⚡ CORE OVERDRIVE!", new Color(1f, 0.2f, 0.4f), 1.7f, true);
        }
    }

    /// <summary>
    /// Bắn các tia lửa điện công nghệ cao khi đạn va chạm
    /// </summary>
    public static void SpawnSparks(Vector3 worldPos, Vector2 impactDir, Color color, int count = 5)
    {
        EnsureInstance();
        if (Instance != null)
        {
            Instance.SpawnSparksInternal(worldPos, impactDir, color, count);
        }
    }

    // =========================================================
    // INTERNAL IMPLEMENTATION
    // =========================================================

    private static void EnsureInstance()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("CombatFeedback_Manager");
            Instance = go.AddComponent<CombatFeedback>();
            DontDestroyOnLoad(go);
        }
    }

    private void SpawnFloatingText(Vector3 worldPos, string text, Color color, float scale, bool isCrit)
    {
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * 0.2f;
        Vector2 initialVel = new Vector2(UnityEngine.Random.Range(-0.4f, 0.4f), UnityEngine.Random.Range(1.8f, 2.5f));
        float life = isCrit ? 1.2f : 0.8f;

        activeTexts.Add(new FloatingText
        {
            worldPosition = worldPos + (Vector3)randomOffset,
            velocity = initialVel,
            text = text,
            color = color,
            scale = scale,
            lifeTime = life,
            maxLifeTime = life,
            isCrit = isCrit
        });
    }

    private void SpawnSparksInternal(Vector3 worldPos, Vector2 impactDir, Color color, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 randomDir = (impactDir + UnityEngine.Random.insideUnitCircle * 1.2f).normalized;
            float speed = UnityEngine.Random.Range(2.5f, 6.0f);
            activeSparks.Add(new SparkParticle
            {
                position = worldPos,
                velocity = randomDir * speed,
                color = color,
                lifeTime = UnityEngine.Random.Range(0.2f, 0.4f),
                maxLifeTime = 0.4f,
                size = UnityEngine.Random.Range(3f, 6f)
            });
        }
    }

    private void DoHitStop(float duration)
    {
        if (hitStopRoutine != null) StopCoroutine(hitStopRoutine);
        hitStopRoutine = StartCoroutine(HitStopRoutine(duration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        float prevScale = Time.timeScale;
        Time.timeScale = 0.05f; // Chậm lại 95% trong tích tắc
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = prevScale > 0.05f ? prevScale : 1f;
        hitStopRoutine = null;
    }

    // =========================================================
    // RENDER ONGUI (SỐ NẢY SẮC NÉT & HIỆU ỨNG TIA LỬA)
    // =========================================================

    private void OnGUI()
    {
        EnsureWhitePixel();
        Camera cam = Camera.main;
        if (cam == null) return;

        // 1. Vẽ các tia lửa Spark Particles
        for (int i = 0; i < activeSparks.Count; i++)
        {
            var sp = activeSparks[i];
            Vector3 screenPt = cam.WorldToScreenPoint(sp.position);
            if (screenPt.z <= 0) continue;

            float alpha = sp.lifeTime / sp.maxLifeTime;
            Color col = sp.color;
            col.a *= alpha;
            GUI.color = col;

            float y = Screen.height - screenPt.y;
            GUI.DrawTexture(new Rect(screenPt.x - sp.size * 0.5f, y - sp.size * 0.5f, sp.size, sp.size), whitePixel);
        }

        // 2. Vẽ Floating Combat Text
        GUIStyle textStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        for (int i = 0; i < activeTexts.Count; i++)
        {
            var item = activeTexts[i];
            Vector3 screenPt = cam.WorldToScreenPoint(item.worldPosition);
            if (screenPt.z <= 0) continue;

            float t = item.lifeTime / item.maxLifeTime; // 1 -> 0
            float alpha = Mathf.Clamp01(t * 1.5f);
            
            // Pop bounce scale
            float scaleProgress = 1f - t;
            float curScale = item.scale * (scaleProgress < 0.2f ? Mathf.Lerp(1.4f, 1.0f, scaleProgress / 0.2f) : 1.0f);

            textStyle.fontSize = Mathf.RoundToInt(15 * curScale);

            float x = screenPt.x;
            float y = Screen.height - screenPt.y;
            float textWidth = textStyle.CalcSize(new GUIContent(item.text)).x + 28f;
            float w = Mathf.Max(180f * curScale, textWidth);
            float h = 32f * curScale;

            Rect rect = new Rect(x - w * 0.5f, y - h * 0.5f, w, h);

            // Shadow bóng đổ đen để nổi bật trên mọi nền
            GUI.color = new Color(0f, 0f, 0f, alpha * 0.9f);
            GUI.Label(new Rect(rect.x + 1.5f, rect.y + 1.5f, rect.width, rect.height), item.text, textStyle);

            // Chữ Neon phát quang
            Color col = item.color;
            col.a = alpha;
            GUI.color = col;
            GUI.Label(rect, item.text, textStyle);
        }

        // 3. Hiển thị Huy hiệu Combo Streak ở góc trên giữa màn hình khi có combo
        if (currentCombo >= 2)
        {
            float comboAlpha = Mathf.Clamp01(comboTimer / 0.5f);
            GUI.color = new Color(0.05f, 0.08f, 0.12f, comboAlpha * 0.85f);
            Rect badgeRect = new Rect(Screen.width * 0.5f - 85, 14, 170, 32);
            GUI.DrawTexture(badgeRect, whitePixel);

            // Viền phát sáng
            Color borderCol = overdriveTimer > 0f ? Color.red : new Color(0f, 0.9f, 1f, 1f);
            borderCol.a *= comboAlpha;
            GUI.color = borderCol;
            GUI.DrawTexture(new Rect(badgeRect.x, badgeRect.y, badgeRect.width, 2), whitePixel);
            GUI.DrawTexture(new Rect(badgeRect.x, badgeRect.yMax - 2, badgeRect.width, 2), whitePixel);

            GUIStyle comboStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };
            string comboLabel = overdriveTimer > 0f ? $"⚡ OVERDRIVE (x{currentCombo})" : $"COMBO x{currentCombo}";
            GUI.Label(badgeRect, comboLabel, comboStyle);
        }

        GUI.color = Color.white;
    }
}
