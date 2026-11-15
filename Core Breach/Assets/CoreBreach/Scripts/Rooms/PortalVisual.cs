using System.Collections;
using UnityEngine;

/// <summary>
/// Hiệu ứng thị giác và tương tác cho Cổng Dịch Chuyển (Vault Portal) chuẩn phong cách Soul Knight:
/// - Đảm bảo Sorting Order = 8 (Nổi hoàn toàn trên mặt sàn gạch kim loại Sorting Order = 5)
/// - Tự động tạo sprite vòng xoáy cổng năng lượng Sci-Fi rực rỡ nếu chưa có sprite
/// - Xoay tròn liên tục trong không gian 2D tạo hiệu ứng xoáy ma pháp công nghệ cao
/// - Nhịp thở phát quang (Pulsing Glow & Scale)
/// - Bắn các hạt bụi năng lượng Cyan/Magenta lên trời
/// - Hiển thị chỉ dẫn chữ phát sáng nổi trên đầu cổng: [▼ CỔNG VAULT - BƯỚC VÀO ĐỂ QUA TẦNG]
/// </summary>
public class PortalVisual : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float rotationSpeed = -130f;
    [SerializeField] private float pulseFrequency = 3.5f;
    [SerializeField] private float pulseAmplitude = 0.12f;
    [SerializeField] private Color portalColor = new Color(0f, 0.95f, 1f, 1f);

    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale = new Vector3(2.2f, 2.2f, 1f);
    private float particleTimer = 0f;
    private bool isSpawning = false;

    public bool IsWarping { get; set; } = false;

    private static Sprite cachedPortalSprite;
    private static Texture2D whitePixel;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // QUAN TRỌNG: Thiết lập Sorting Order = 8 để luôn nổi trên nền sàn gạch (Sorting Order = 5)
        spriteRenderer.sortingOrder = 8;
        spriteRenderer.color = portalColor;

        // Nạp sprite cổng xoáy rực rỡ chuẩn Soul Knight
        if (cachedPortalSprite == null)
        {
            cachedPortalSprite = Resources.Load<Sprite>("Objects/sci_portal_exit");
            if (cachedPortalSprite == null)
            {
                cachedPortalSprite = GenerateVibrantPortalSprite();
            }
        }
        spriteRenderer.sprite = cachedPortalSprite;

        transform.localScale = baseScale;

        // Bán kính va chạm thu gọn chuẩn xác (0.35f ~ 0.77m world size), yêu cầu bước hẳn vào cổng mới kích hoạt
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        col.isTrigger = true;
        col.radius = 0.35f;

        // Đảm bảo có RoomExit
        if (GetComponent<RoomExit>() == null)
        {
            gameObject.AddComponent<RoomExit>();
        }

        EnsureWhitePixel();
    }

    private void OnEnable()
    {
        InitializeComponents();

        // Khi cổng kích hoạt mở ra: Hiệu ứng phóng to bùng nổ từ tâm
        StartCoroutine(AppearPopRoutine());
        
        // Bắn pháo hoa năng lượng xung quanh
        if (CombatFeedback.Instance != null)
        {
            CombatFeedback.SpawnSparks(transform.position, Vector2.up, portalColor, 20);
        }
    }

    private IEnumerator AppearPopRoutine()
    {
        isSpawning = true;
        transform.localScale = Vector3.zero;
        float elapsed = 0f;
        float duration = 0.35f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Overshoot bounce
            float scaleFactor = Mathf.Sin(t * Mathf.PI * 0.5f) * 1.15f;
            transform.localScale = baseScale * scaleFactor;
            yield return null;
        }

        transform.localScale = baseScale;
        isSpawning = false;
    }

    private void Update()
    {
        // Luôn đảm bảo sorting order cao hơn sàn gạch (5)
        if (spriteRenderer != null && spriteRenderer.sortingOrder < 8)
        {
            spriteRenderer.sortingOrder = 8;
        }

        float curRotSpeed = IsWarping ? rotationSpeed * 3f : rotationSpeed;
        float curPulseFreq = IsWarping ? pulseFrequency * 2.5f : pulseFrequency;
        float curPulseAmp = IsWarping ? pulseAmplitude * 1.8f : pulseAmplitude;

        // 1. Xoay tròn liên tục
        transform.Rotate(0f, 0f, curRotSpeed * Time.deltaTime);

        // 2. Nhịp thở phát sáng (khi không trong trạng thái bắt đầu spawn)
        if (!isSpawning)
        {
            float pulse = Mathf.Sin(Time.time * curPulseFreq) * curPulseAmp;
            transform.localScale = baseScale * (1f + pulse);
        }

        if (spriteRenderer != null)
        {
            float alpha = IsWarping ? 1.0f : (0.88f + 0.12f * Mathf.Sin(Time.time * 5f));
            Color c = IsWarping ? Color.white : portalColor;
            c.a = alpha;
            spriteRenderer.color = c;
        }

        // 3. Định kỳ phát ra tia hạt bụi năng lượng
        particleTimer += Time.deltaTime;
        float sparkInterval = IsWarping ? 0.04f : 0.14f;
        if (particleTimer >= sparkInterval)
        {
            particleTimer = 0f;
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector3 sparkPos = transform.position + (Vector3)randomDir * Random.Range(0.15f, 0.85f);
            Color sparkCol = Random.value > 0.4f ? portalColor : new Color(0.9f, 0.3f, 1f, 0.9f);
            if (IsWarping) sparkCol = Color.white;
            CombatFeedback.SpawnSparks(sparkPos, randomDir, sparkCol, IsWarping ? 5 : 3);
        }
    }

    private void OnGUI()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Vẽ nhãn chỉ dẫn phát quang phía trên cổng
        Vector3 screenPt = cam.WorldToScreenPoint(transform.position + Vector3.up * 1.4f);
        if (screenPt.z <= 0) return;

        float x = screenPt.x;
        float y = Screen.height - screenPt.y;

        float pulseAlpha = 0.85f + 0.15f * Mathf.Sin(Time.time * 6f);
        float w = 260f;
        float h = 28f;
        Rect rect = new Rect(x - w * 0.5f, y - h * 0.5f, w, h);

        // Nền tối nhỏ
        EnsureWhitePixel();
        GUI.color = new Color(0.04f, 0.08f, 0.14f, pulseAlpha * 0.90f);
        GUI.DrawTexture(rect, whitePixel);

        // Viền Cyan phát sáng
        GUI.color = new Color(0f, 0.95f, 1f, pulseAlpha * 0.9f);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2f), whitePixel);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), whitePixel);

        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 12,
            normal = { textColor = new Color(0.2f, 1f, 1f, pulseAlpha) }
        };

        GUI.Label(rect, "▼ CỔNG VAULT [BƯỚC VÀO QUA TẦNG]", style);
        GUI.color = Color.white;
    }

    /// <summary>
    /// Sinh sprite cổng dịch chuyển Sci-Fi nhiều lớp phát sáng rực rỡ chuẩn Pixel Art
    /// </summary>
    private static Sprite GenerateVibrantPortalSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Vector2 center = new Vector2(31.5f, 31.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist > 31f)
                {
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
                    continue;
                }

                // Góc xoáy Angle
                float angle = Mathf.Atan2(y - center.y, x - center.x);
                float spiral = Mathf.Sin(dist * 0.45f - angle * 4f);

                // 1. Vành ngoài năng lượng (Outer Ring 26 - 31)
                if (dist >= 26f && dist <= 31f)
                {
                    if (spiral > 0.1f) tex.SetPixel(x, y, new Color(0f, 0.9f, 1f, 0.95f)); // Vạch Cyan
                    else tex.SetPixel(x, y, new Color(0.1f, 0.3f, 0.7f, 0.85f)); // Viền đậm
                }
                // 2. Vành xoáy thứ hai (Vortex Ring 16 - 25)
                else if (dist >= 16f && dist < 26f)
                {
                    float t = (dist - 16f) / 10f;
                    if (spiral > -0.2f)
                    {
                        Color c = Color.Lerp(new Color(0.1f, 0.95f, 1f, 1f), new Color(0.7f, 0.2f, 1f, 1f), t);
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        tex.SetPixel(x, y, new Color(0.05f, 0.12f, 0.28f, 0.9f));
                    }
                }
                // 3. Vòng tròn sáng bên trong (Inner Glow 8 - 15)
                else if (dist >= 8f && dist < 16f)
                {
                    Color c = Color.Lerp(Color.white, new Color(0f, 0.95f, 1f, 1f), (dist - 8f) / 8f);
                    tex.SetPixel(x, y, c);
                }
                // 4. Lõi Singularity siêu sáng ở tâm (Core 0 - 7)
                else
                {
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, 1f));
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
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
}
