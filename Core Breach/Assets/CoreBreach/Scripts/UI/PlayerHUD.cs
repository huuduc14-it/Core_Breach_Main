using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Giao diện HUD chiến đấu và Game Over trong Core Breach:
/// - Thanh HP (Máu) và Armor (Giáp)
/// - Thanh Overheat (Nhiệt độ súng) kèm hiệu ứng đổi màu / nhấp nháy khi bị khóa quá nhiệt
/// - Hiển thị số Scrap thu thập, Tầng Vault hiện tại (Depth), Điểm số (Score)
/// - Tâm ngắm Sci-Fi (Crosshair) bám chuột với cảnh báo Overheat
/// - Màn hình Game Over / Mission Failed với thống kê và nút Retry [Thử lại]
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    [Header("Health & Armor Bars")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider armorSlider;
    [SerializeField] private Text healthText;
    [SerializeField] private Text armorText;

    [Header("Heat / Overheat Bar")]
    [SerializeField] private Slider heatSlider;
    [SerializeField] private Image heatFillImage;
    [SerializeField] private Text heatWarningText;
    [SerializeField] private Color normalHeatColor = new Color(0.2f, 0.8f, 1f, 1f);
    [SerializeField] private Color overheatedColor = new Color(1f, 0.2f, 0.1f, 1f);

    [Header("Run Stats")]
    [SerializeField] private Text scrapText;
    [SerializeField] private Text depthText;
    [SerializeField] private Text scoreText;

    private PlayerHealth playerHealth;
    private HeatSystem heatSystem;
    private bool isPaused = false;

    // Texture trắng 1x1 dùng để vẽ hình học OnGUI mượt mà
    private static Texture2D whitePixel;

    private void Awake()
    {
        EnsureWhitePixel();
    }

    private void Start()
    {
        FindAndBindPlayer();
        BindGameManager();
    }

    private void Update()
    {
        // Tự động tìm lại nếu player vừa mới spawn vào phòng
        if (playerHealth == null || heatSystem == null)
        {
            FindAndBindPlayer();
        }

        // Nếu Game Over, cho phép bấm phím Space hoặc Enter để Restart ngay
        bool isGameOver = (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.GameOver) ||
                          (playerHealth != null && playerHealth.CurrentHealth <= 0f);

        // Bật/tắt tạm dừng khi nhấn ESC (khi chưa Game Over)
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            TogglePause();
        }

        if (isGameOver)
        {
            if (isPaused) TogglePause(); // Tự hủy pause nếu Game Over

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RestartCurrentRun();
                }
            }
        }
    }

    public void FindAndBindPlayer()
    {
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealthBar;
                playerHealth.OnArmorChanged += UpdateArmorBar;

                UpdateHealthBar(playerHealth.CurrentHealth, 100f);
                UpdateArmorBar(playerHealth.CurrentArmor, 20f);
            }
        }

        if (heatSystem == null)
        {
            heatSystem = FindObjectOfType<HeatSystem>();
            if (heatSystem != null)
            {
                heatSystem.OnHeatChanged += UpdateHeatBar;
                UpdateHeatBar(heatSystem.CurrentHeat, heatSystem.MaxHeat, heatSystem.IsOverheated);
            }
        }
    }

    private void BindGameManager()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScrapChanged += UpdateScrap;
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnDepthChanged += UpdateDepth;

            UpdateScrap(GameManager.Instance.CurrentScrap);
            UpdateScore(GameManager.Instance.Score);
            UpdateDepth(GameManager.Instance.CurrentDepth);
        }
    }

    private void UpdateHealthBar(float current, float max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
        if (healthText != null)
        {
            healthText.text = $"HP: {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }
    }

    private void UpdateArmorBar(float current, float max)
    {
        if (armorSlider != null)
        {
            armorSlider.maxValue = max;
            armorSlider.value = current;
        }
        if (armorText != null)
        {
            armorText.text = $"ARMOR: {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }
    }

    private void UpdateHeatBar(float current, float max, bool isOverheated)
    {
        if (heatSlider != null)
        {
            heatSlider.maxValue = max;
            heatSlider.value = current;
        }

        if (heatFillImage != null)
        {
            heatFillImage.color = isOverheated ? overheatedColor : normalHeatColor;
        }

        if (heatWarningText != null)
        {
            heatWarningText.gameObject.SetActive(isOverheated);
            if (isOverheated)
            {
                heatWarningText.text = "⚠️ OVERHEAT! LÀM MÁT TRONG 2S...";
            }
        }
    }

    private void UpdateScrap(int scrap)
    {
        if (scrapText != null) scrapText.text = $"SCRAP: {scrap}";
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = $"SCORE: {score}";
    }

    private void UpdateDepth(int depth)
    {
        if (depthText != null) depthText.text = $"DEPTH: B{depth}";
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

    private void DrawBar(Rect rect, float fillPercent, Color fillColor, Color bgColor)
    {
        EnsureWhitePixel();
        // Nền thanh
        GUI.color = bgColor;
        GUI.DrawTexture(rect, whitePixel);

        // Phần trăm thanh
        if (fillPercent > 0f)
        {
            GUI.color = fillColor;
            Rect fillRect = new Rect(rect.x + 1, rect.y + 1, (rect.width - 2) * Mathf.Clamp01(fillPercent), rect.height - 2);
            GUI.DrawTexture(fillRect, whitePixel);
        }
        GUI.color = Color.white;
    }

    private void OnGUI()
    {
        bool isGameOver = (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.GameOver) ||
                          (playerHealth != null && playerHealth.CurrentHealth <= 0f);

        // Fallback giao diện trạng thái nếu chưa có Canvas Slider
        if (healthSlider == null)
        {
            float hp = playerHealth != null ? playerHealth.CurrentHealth : 100f;
            float armor = playerHealth != null ? playerHealth.CurrentArmor : 20f;
            float heat = heatSystem != null ? heatSystem.CurrentHeat : 0f;
            bool over = heatSystem != null && heatSystem.IsOverheated;
            int scrap = GameManager.Instance != null ? GameManager.Instance.CurrentScrap : 0;
            int depth = GameManager.Instance != null ? GameManager.Instance.CurrentDepth : 1;
            int score = GameManager.Instance != null ? GameManager.Instance.Score : 0;

            // 1. Khung trạng thái Unit-7 góc trên bên trái
            GUI.Box(new Rect(15, 15, 270, 135), "UNIT-7 STATUS [CORE BREACH]");

            // HP Bar
            GUI.Label(new Rect(25, 38, 90, 20), $"HP: {Mathf.CeilToInt(hp)}/100");
            DrawBar(new Rect(115, 41, 155, 14), hp / 100f, Color.red, new Color(0.2f, 0.05f, 0.05f, 0.9f));

            // Armor Bar
            GUI.Label(new Rect(25, 58, 90, 20), $"ARMOR: {Mathf.CeilToInt(armor)}/20");
            DrawBar(new Rect(115, 61, 155, 14), armor / 20f, Color.cyan, new Color(0.05f, 0.15f, 0.2f, 0.9f));

            // Heat Bar
            string heatLabel = over ? "OVERHEAT!" : $"HEAT: {Mathf.CeilToInt(heat)}%";
            GUI.color = over ? Color.red : Color.white;
            GUI.Label(new Rect(25, 78, 90, 20), heatLabel);
            GUI.color = Color.white;
            Color heatCol = over ? Color.red : new Color(1f, 0.55f, 0.1f);
            DrawBar(new Rect(115, 81, 155, 14), heat / 100f, heatCol, new Color(0.2f, 0.1f, 0.05f, 0.9f));

            // Stats
            GUI.color = Color.yellow;
            GUI.Label(new Rect(25, 102, 250, 20), $"SCRAP: {scrap}  |  DEPTH: B{depth}  |  PTS: {score}");
            GUI.color = Color.white;

            // 2. Tâm ngắm Sci-Fi quanh vị trí chuột (chỉ vẽ khi còn sống)
            if (!isGameOver)
            {
                DrawCustomCrosshair(over);
            }
        }

        // 3. Màn hình Tạm dừng (Pause) khi bấm ESC
        if (isPaused)
        {
            DrawPauseMenu();
            return;
        }

        // 4. Màn hình Game Over / Mission Failed khi Unit-7 bị tiêu diệt
        if (isGameOver)
        {
            DrawGameOverScreen();
        }
    }

    private void DrawCustomCrosshair(bool isOverheated)
    {
        EnsureWhitePixel();
        Vector2 mousePos = Event.current.mousePosition;
        float size = 16f;
        float thickness = 2f;
        Color crosshairColor = isOverheated ? new Color(1f, 0.2f, 0.2f, 0.9f) : new Color(0.2f, 0.9f, 1f, 0.8f);

        GUI.color = crosshairColor;
        // Trục ngang trái/phải
        GUI.DrawTexture(new Rect(mousePos.x - size - 4, mousePos.y - thickness / 2, size, thickness), whitePixel);
        GUI.DrawTexture(new Rect(mousePos.x + 4, mousePos.y - thickness / 2, size, thickness), whitePixel);
        // Trục dọc trên/dưới
        GUI.DrawTexture(new Rect(mousePos.x - thickness / 2, mousePos.y - size - 4, thickness, size), whitePixel);
        GUI.DrawTexture(new Rect(mousePos.x - thickness / 2, mousePos.y + 4, thickness, size), whitePixel);

        // Chấm tâm
        GUI.DrawTexture(new Rect(mousePos.x - 1, mousePos.y - 1, 2, 2), whitePixel);

        if (isOverheated)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(mousePos.x - 60, mousePos.y + 16, 120, 20), "⚠️ OVERHEAT!", new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            });
        }
        GUI.color = Color.white;
    }

    private void DrawGameOverScreen()
    {
        EnsureWhitePixel();
        float screenW = Screen.width;
        float screenH = Screen.height;

        // Phủ tối toàn màn hình
        GUI.color = new Color(0.05f, 0.05f, 0.08f, 0.85f);
        GUI.DrawTexture(new Rect(0, 0, screenW, screenH), whitePixel);
        GUI.color = Color.white;

        // Hộp thoại trung tâm
        float boxW = 440;
        float boxH = 320;
        float startX = (screenW - boxW) / 2f;
        float startY = (screenH - boxH) / 2f;

        GUI.Box(new Rect(startX, startY, boxW, boxH), "");

        // Tiêu đề
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(1f, 0.25f, 0.25f) }
        };
        GUI.Label(new Rect(startX, startY + 20, boxW, 35), "MISSION FAILED", titleStyle);

        // Phụ đề
        GUIStyle subStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.8f, 0.8f, 0.8f) }
        };
        int depth = GameManager.Instance != null ? GameManager.Instance.CurrentDepth : 1;
        GUI.Label(new Rect(startX, startY + 55, boxW, 20), $"Unit-7 was neutralized in Vault Sector B{depth}", subStyle);

        // Bảng kết quả chiến dịch
        int finalScore = GameManager.Instance != null ? GameManager.Instance.Score : 0;
        int highScore = PlayerPrefs.GetInt("CoreBreach_HighScore", finalScore);
        int runScrap = GameManager.Instance != null ? GameManager.Instance.CurrentScrap : 0;
        int totalScrap = PlayerPrefs.GetInt("CoreBreach_TotalScrap", runScrap);

        GUIStyle statStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            normal = { textColor = Color.white }
        };

        float statY = startY + 95;
        GUI.Label(new Rect(startX + 50, statY, 200, 25), "• Final Score:", statStyle);
        GUI.Label(new Rect(startX + 250, statY, 150, 25), $"{finalScore} pts", statStyle);

        statY += 28;
        GUI.Label(new Rect(startX + 50, statY, 200, 25), "• Best High Score:", statStyle);
        GUI.Label(new Rect(startX + 250, statY, 150, 25), $"{highScore} pts", statStyle);

        statY += 28;
        GUI.Label(new Rect(startX + 50, statY, 200, 25), "• Scrap Recovered:", statStyle);
        GUI.Label(new Rect(startX + 250, statY, 150, 25), $"+{runScrap} scrap", statStyle);

        statY += 28;
        GUI.Label(new Rect(startX + 50, statY, 200, 25), "• Permanent Scrap Vault:", statStyle);
        GUI.Label(new Rect(startX + 250, statY, 150, 25), $"{totalScrap} scrap", statStyle);

        // Nút Thử Lại (Retry)
        float btnW = 260;
        float btnH = 45;
        float btnX = startX + (boxW - btnW) / 2f;
        float btnY = startY + 225;

        if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "RETRY MISSION  [SPACE / ENTER]"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartCurrentRun();
            }
        }

        GUIStyle tipStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.gray }
        };
        GUI.Label(new Rect(startX, btnY + 50, boxW, 20), "Core Breach v1.0 • Rogue-lite Protocol", tipStyle);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }

    private void DrawPauseMenu()
    {
        EnsureWhitePixel();
        float screenW = Screen.width;
        float screenH = Screen.height;

        GUI.color = new Color(0.04f, 0.05f, 0.08f, 0.8f);
        GUI.DrawTexture(new Rect(0, 0, screenW, screenH), whitePixel);
        GUI.color = Color.white;

        float boxW = 340;
        float boxH = 240;
        float startX = (screenW - boxW) / 2f;
        float startY = (screenH - boxH) / 2f;

        GUI.Box(new Rect(startX, startY, boxW, boxH), "");

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.3f, 0.85f, 1f) }
        };
        GUI.Label(new Rect(startX, startY + 20, boxW, 30), "SYSTEM PAUSED", titleStyle);

        float btnW = 220;
        float btnH = 38;
        float btnX = startX + (boxW - btnW) / 2f;

        if (GUI.Button(new Rect(btnX, startY + 65, btnW, btnH), "RESUME MISSION  [ESC]"))
        {
            TogglePause();
        }

        if (GUI.Button(new Rect(btnX, startY + 115, btnW, btnH), "RESTART RUN"))
        {
            Time.timeScale = 1f;
            if (GameManager.Instance != null) GameManager.Instance.RestartCurrentRun();
        }

        if (GUI.Button(new Rect(btnX, startY + 165, btnW, btnH), "QUIT MISSION"))
        {
            Time.timeScale = 1f;
            Application.Quit();
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
            playerHealth.OnArmorChanged -= UpdateArmorBar;
        }
        if (heatSystem != null)
        {
            heatSystem.OnHeatChanged -= UpdateHeatBar;
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScrapChanged -= UpdateScrap;
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnDepthChanged -= UpdateDepth;
        }
    }
}
