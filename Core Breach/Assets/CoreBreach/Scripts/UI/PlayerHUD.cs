//using UnityEngine;
//using UnityEngine.UI;

///// <summary>
///// Giao diện HUD chiến đấu và Game Over trong Core Breach:
///// - Thanh HP (Máu) và Armor (Giáp)
///// - Thanh Overheat (Nhiệt độ súng) kèm hiệu ứng đổi màu / nhấp nháy khi bị khóa quá nhiệt
///// - Hiển thị số Scrap thu thập, Tầng Vault hiện tại (Depth), Điểm số (Score)
///// - Tâm ngắm Sci-Fi (Crosshair) bám chuột với cảnh báo Overheat
///// - Màn hình Game Over / Mission Failed với thống kê và nút Retry [Thử lại]
///// </summary>
//public class PlayerHUD : MonoBehaviour
//{
//    [Header("Health & Armor Bars")]
//    [SerializeField] private Slider healthSlider;
//    [SerializeField] private Slider armorSlider;
//    [SerializeField] private Text healthText;
//    [SerializeField] private Text armorText;

//    [Header("Heat / Overheat Bar")]
//    [SerializeField] private Slider heatSlider;
//    [SerializeField] private Image heatFillImage;
//    [SerializeField] private Text heatWarningText;
//    [SerializeField] private Color normalHeatColor = new Color(0.2f, 0.8f, 1f, 1f);
//    [SerializeField] private Color overheatedColor = new Color(1f, 0.2f, 0.1f, 1f);

//    [Header("Run Stats")]
//    [SerializeField] private Text scrapText;
//    [SerializeField] private Text depthText;
//    [SerializeField] private Text scoreText;

//    private PlayerHealth playerHealth;
//    private HeatSystem heatSystem;
//    private float ghostHpPct = 1f;
//    private float ghostArmorPct = 1f;

//    // =========================================================
//    // WAVE BANNER SYSTEM (SOUL KNIGHT SCREEN-SPACE BANNER)
//    // =========================================================
//    private static string bannerTitle = "";
//    private static string bannerSubtitle = "";
//    private static Color bannerColor = Color.cyan;
//    private static float bannerTimer = 0f;
//    private static float bannerMaxDuration = 2.0f;

//    public static void ShowWaveBanner(string title, string subtitle, Color color, float duration = 2.0f)
//    {
//        bannerTitle = title;
//        bannerSubtitle = subtitle;
//        bannerColor = color;
//        bannerTimer = duration;
//        bannerMaxDuration = duration;
//    }

//    private static float screenFlashAlpha = 0f;
//    private static Color screenFlashColor = Color.white;

//    public static void FlashScreen(Color color, float alpha = 0.85f)
//    {
//        screenFlashColor = color;
//        screenFlashAlpha = alpha;
//    }

//    // Texture trắng 1x1 dùng để vẽ hình học OnGUI mượt mà
//    private static Texture2D whitePixel;

//    private void Awake()
//    {
//        EnsureWhitePixel();
//    }

//    private void Start()
//    {
//        FindAndBindPlayer();
//        BindGameManager();
//    }

//    private void Update()
//    {
//        // Tự động tìm lại nếu player vừa mới spawn vào phòng
//        if (playerHealth == null || heatSystem == null)
//        {
//            FindAndBindPlayer();
//        }

//        // Cập nhật thanh trễ Ghost Bar (Chip Damage lag bar)
//        if (playerHealth != null && playerHealth.MaxHealth > 0)
//        {
//            float targetHpPct = playerHealth.CurrentHealth / playerHealth.MaxHealth;
//            if (ghostHpPct > targetHpPct) ghostHpPct = Mathf.MoveTowards(ghostHpPct, targetHpPct, Time.deltaTime * 0.45f);
//            else ghostHpPct = targetHpPct;

//            float targetArmPct = playerHealth.MaxArmor > 0 ? playerHealth.CurrentArmor / playerHealth.MaxArmor : 0f;
//            if (ghostArmorPct > targetArmPct) ghostArmorPct = Mathf.MoveTowards(ghostArmorPct, targetArmPct, Time.deltaTime * 0.45f);
//            else ghostArmorPct = targetArmPct;
//        }

//        // Cập nhật đếm ngược Wave Banner
//        if (bannerTimer > 0f)
//        {
//            bannerTimer -= Time.deltaTime;
//        }

//        // Nếu Game Over, cho phép bấm phím Space hoặc Enter để Restart ngay
//        bool isGameOver = (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.GameOver) ||
//                          (playerHealth != null && playerHealth.CurrentHealth <= 0f);

//<<<<<<< Updated upstream
//=======
//        // Bật/tắt tạm dừng khi nhấn ESC (khi chưa Game Over).
//        // Nếu scene đã có PauseMenu (Canvas) thì để PauseMenu xử lý, tránh 2 script cùng bắt ESC.
//        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver && FindFirstObjectByType<PauseMenu>() == null)
//        {
//            TogglePause();
//        }

//>>>>>>> Stashed changes
//        if (isGameOver)
//        {
//            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
//            {
//                if (GameManager.Instance != null)
//                {
//                    GameManager.Instance.RestartCurrentRun();
//                }
//            }
//        }
//    }

//    public void FindAndBindPlayer()
//    {
//        if (playerHealth == null)
//        {
//            playerHealth = FindFirstObjectByType<PlayerHealth>();
//            if (playerHealth != null)
//            {
//                playerHealth.OnHealthChanged += UpdateHealthBar;
//                playerHealth.OnArmorChanged += UpdateArmorBar;

//                UpdateHealthBar(playerHealth.CurrentHealth, 100f);
//                UpdateArmorBar(playerHealth.CurrentArmor, 20f);
//            }
//        }

//        if (heatSystem == null)
//        {
//            heatSystem = FindFirstObjectByType<HeatSystem>();
//            if (heatSystem != null)
//            {
//                heatSystem.OnHeatChanged += UpdateHeatBar;
//                UpdateHeatBar(heatSystem.CurrentHeat, heatSystem.MaxHeat, heatSystem.IsOverheated);
//            }
//        }
//    }

//    private void BindGameManager()
//    {
//        if (GameManager.Instance != null)
//        {
//            GameManager.Instance.OnScrapChanged += UpdateScrap;
//            GameManager.Instance.OnScoreChanged += UpdateScore;
//            GameManager.Instance.OnDepthChanged += UpdateDepth;

//            UpdateScrap(GameManager.Instance.CurrentScrap);
//            UpdateScore(GameManager.Instance.Score);
//            UpdateDepth(GameManager.Instance.CurrentDepth);
//        }
//    }

//    private void UpdateHealthBar(float current, float max)
//    {
//        if (healthSlider != null)
//        {
//            healthSlider.maxValue = max;
//            healthSlider.value = current;
//        }
//        if (healthText != null)
//        {
//            healthText.text = $"HP: {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
//        }
//    }

//    private void UpdateArmorBar(float current, float max)
//    {
//        if (armorSlider != null)
//        {
//            armorSlider.maxValue = max;
//            armorSlider.value = current;
//        }
//        if (armorText != null)
//        {
//            armorText.text = $"ARMOR: {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
//        }
//    }

//    private void UpdateHeatBar(float current, float max, bool isOverheated)
//    {
//        if (heatSlider != null)
//        {
//            heatSlider.maxValue = max;
//            heatSlider.value = current;
//        }

//        if (heatFillImage != null)
//        {
//            heatFillImage.color = isOverheated ? overheatedColor : normalHeatColor;
//        }

//        if (heatWarningText != null)
//        {
//            heatWarningText.gameObject.SetActive(isOverheated);
//            if (isOverheated)
//            {
//                heatWarningText.text = "⚠️ OVERHEAT! LÀM MÁT TRONG 2S...";
//            }
//        }
//    }

//    private void UpdateScrap(int scrap)
//    {
//        if (scrapText != null) scrapText.text = $"SCRAP: {scrap}";
//    }

//    private void UpdateScore(int score)
//    {
//        if (scoreText != null) scoreText.text = $"SCORE: {score}";
//    }

//    private void UpdateDepth(int depth)
//    {
//        if (depthText != null) depthText.text = $"DEPTH: B{depth}";
//    }

//    private static void EnsureWhitePixel()
//    {
//        if (whitePixel == null)
//        {
//            whitePixel = new Texture2D(1, 1);
//            whitePixel.SetPixel(0, 0, Color.white);
//            whitePixel.Apply();
//        }
//    }

//    private void DrawBar(Rect rect, float fillPercent, Color fillColor, Color bgColor, float ghostPercent = -1f, Color? ghostColor = null)
//    {
//        EnsureWhitePixel();

//        // Viền ngoài kim loại
//        GUI.color = new Color(0.18f, 0.24f, 0.36f, 0.95f);
//        GUI.DrawTexture(rect, whitePixel);

//        // Nền thanh bên trong
//        Rect innerBg = new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2);
//        GUI.color = bgColor;
//        GUI.DrawTexture(innerBg, whitePixel);

//        // Thanh Ghost Lag / Chip Damage (màu vàng cam hoặc trắng mờ chậm 0.3s)
//        if (ghostPercent > fillPercent + 0.005f)
//        {
//            float ghostWidth = (rect.width - 2) * Mathf.Clamp01(ghostPercent);
//            Rect ghostRect = new Rect(rect.x + 1, rect.y + 1, ghostWidth, rect.height - 2);
//            GUI.color = ghostColor ?? new Color(1f, 0.75f, 0.15f, 0.85f);
//            GUI.DrawTexture(ghostRect, whitePixel);
//        }

//        // Phần trăm thanh năng lượng phát sáng
//        if (fillPercent > 0.01f)
//        {
//            float fillWidth = (rect.width - 2) * Mathf.Clamp01(fillPercent);
//            Rect fillRect = new Rect(rect.x + 1, rect.y + 1, fillWidth, rect.height - 2);
//            GUI.color = fillColor;
//            GUI.DrawTexture(fillRect, whitePixel);

//            // Vạch sáng phản quang ở đỉnh thanh
//            GUI.color = new Color(1f, 1f, 1f, 0.35f);
//            Rect shineRect = new Rect(rect.x + 1, rect.y + 1, fillWidth, 2);
//            GUI.DrawTexture(shineRect, whitePixel);
//        }
//        GUI.color = Color.white;
//    }

//    private void OnGUI()
//    {
//        // Ẩn HUD nếu đang ở MainMenu, CharacterSelect hoặc Paused
//        if (GameManager.Instance != null && 
//            GameManager.Instance.CurrentState != GameState.Playing && 
//            GameManager.Instance.CurrentState != GameState.GameOver)
//        {
//            return;
//        }

//        // Ẩn HUD nếu đang mở Menu chọn nhân vật
//        if (CharacterSelectSystem.Instance != null && CharacterSelectSystem.Instance.IsSelectionOpen)
//        {
//            return;
//        }

//        // Flash màn hình dịch chuyển / cảnh báo
//        if (screenFlashAlpha > 0.01f)
//        {
//            EnsureWhitePixel();
//            Color c = screenFlashColor;
//            c.a = screenFlashAlpha;
//            GUI.color = c;
//            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whitePixel);
//            GUI.color = Color.white;
//            screenFlashAlpha = Mathf.MoveTowards(screenFlashAlpha, 0f, Time.deltaTime * 2.8f);
//        }

//        bool isGameOver = (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.GameOver) ||
//                          (playerHealth != null && playerHealth.CurrentHealth <= 0f);

//        // HUD Sci-Fi Cyberpunk hiển thị trực tiếp sắc nét
//        if (healthSlider == null)
//        {
//            float hp = playerHealth != null ? playerHealth.CurrentHealth : 100f;
//            float armor = playerHealth != null ? playerHealth.CurrentArmor : 20f;
//            float heat = heatSystem != null ? heatSystem.CurrentHeat : 0f;
//            bool over = heatSystem != null && heatSystem.IsOverheated;
//            int scrap = GameManager.Instance != null ? GameManager.Instance.CurrentScrap : 0;
//            int depth = GameManager.Instance != null ? GameManager.Instance.CurrentDepth : 1;
//            int score = GameManager.Instance != null ? GameManager.Instance.Score : 0;

//            WeaponController wc = playerHealth != null ? playerHealth.GetComponent<WeaponController>() : null;
//            string currentWeaponName = wc != null && wc.CurrentWeapon != null ? wc.CurrentWeapon.WeaponName : "Laser Rifle";
//            int slotIdx = wc != null ? wc.CurrentSlotIndex : 0;

//            PlayerMovement pm = playerHealth != null ? playerHealth.GetComponent<PlayerMovement>() : null;

//            EnsureWhitePixel();

//            // 1. KHUNG TRẠNG THÁI NHỎ GỌN (SOUL KNIGHT STYLE: CHỈ 3 THANH HP, GIÁP, NHIỆT)
//            Rect panelRect = new Rect(16, 16, 218, 76);

//            // Viền ngoài phát sáng Cyan / Đỏ khi quá nhiệt
//            GUI.color = over ? new Color(1f, 0.2f, 0.2f, 0.85f) : new Color(0f, 0.85f, 1f, 0.75f);
//            GUI.DrawTexture(new Rect(panelRect.x - 1, panelRect.y - 1, panelRect.width + 2, panelRect.height + 2), whitePixel);

//            // Nền tối mờ công nghệ cao
//            GUI.color = new Color(0.06f, 0.08f, 0.13f, 0.92f);
//            GUI.DrawTexture(panelRect, whitePixel);

//            GUIStyle barLabelStyle = new GUIStyle(GUI.skin.label)
//            {
//                fontSize = 10,
//                fontStyle = FontStyle.Bold,
//                alignment = TextAnchor.MiddleLeft,
//                normal = { textColor = new Color(0.9f, 0.95f, 1f, 1f) }
//            };
//            GUIStyle numStyle = new GUIStyle(GUI.skin.label)
//            {
//                fontSize = 10,
//                fontStyle = FontStyle.Bold,
//                alignment = TextAnchor.MiddleRight,
//                normal = { textColor = Color.white }
//            };

//            // THANH HP (Máu)
//            float maxHp = playerHealth != null ? playerHealth.MaxHealth : 100f;
//            float hpPct = maxHp > 0 ? hp / maxHp : 1f;
//            Color hpCol = hpPct > 0.5f ? new Color(0f, 0.9f, 0.45f) : (hpPct > 0.25f ? new Color(1f, 0.75f, 0.1f) : new Color(1f, 0.15f, 0.25f));
//            GUI.color = Color.white;
//            GUI.Label(new Rect(22, 20, 36, 15), "HP", barLabelStyle);
//            DrawBar(new Rect(58, 20, 96, 14), hpPct, hpCol, new Color(0.12f, 0.05f, 0.06f, 0.9f), ghostHpPct, new Color(1f, 0.8f, 0.2f, 0.85f));
//            GUI.Label(new Rect(156, 20, 72, 15), $"{Mathf.CeilToInt(hp)}/{Mathf.CeilToInt(maxHp)}", numStyle);

//            // THANH GIÁP (Armor)
//            float maxArm = playerHealth != null ? playerHealth.MaxArmor : 20f;
//            float armPct = maxArm > 0 ? armor / maxArm : 0f;
//            GUI.color = Color.white;
//            GUI.Label(new Rect(22, 38, 36, 15), "GIÁP", barLabelStyle);
//            DrawBar(new Rect(58, 38, 96, 14), armPct, new Color(0f, 0.75f, 1f), new Color(0.04f, 0.12f, 0.18f, 0.9f), ghostArmorPct, new Color(0.4f, 0.85f, 1f, 0.75f));
//            GUI.Label(new Rect(156, 38, 72, 15), $"{Mathf.CeilToInt(armor)}/{Mathf.CeilToInt(maxArm)}", numStyle);

//            // THANH NHIỆT (Overheat / Năng lượng)
//            Color heatCol = over ? (Mathf.Sin(Time.time * 12f) > 0 ? Color.red : Color.yellow) : (heat > 60f ? new Color(1f, 0.55f, 0.1f) : new Color(1f, 0.5f, 0.1f));
//            GUI.color = over ? Color.red : Color.white;
//            GUI.Label(new Rect(22, 56, 36, 15), over ? "QUÁ NHIỆT" : "NHIỆT", barLabelStyle);
//            DrawBar(new Rect(58, 56, 96, 14), heat / 100f, heatCol, new Color(0.15f, 0.08f, 0.04f, 0.9f));
//            GUI.Label(new Rect(156, 56, 72, 15), $"{Mathf.CeilToInt(heat)}%", numStyle);

//            // DÒNG THÔNG BÁO NHỎ GỌN PHÍA DƯỚI PANEL
//            GUI.color = new Color(1f, 0.85f, 0.2f, 0.9f);
//            GUIStyle miniStyle = new GUIStyle(GUI.skin.label)
//            {
//                fontSize = 10,
//                fontStyle = FontStyle.Bold,
//                normal = { textColor = new Color(1f, 0.85f, 0.2f, 0.9f) }
//            };
//            GUI.Label(new Rect(18, 96, 218, 16), $"💎 {scrap} SCRAP   •   TẦNG B{depth}", miniStyle);

//            GUI.color = Color.white;

//            // 2. HIỂN THỊ WAVE BANNER HOÀNH TRÁNG Ở GIỮA MÀN HÌNH (SOUL KNIGHT STYLE)
//            DrawWaveBanner();

//            // 3. CROSSHAIR SCI-FI QUANH CHUỘT
//            if (!isGameOver)
//            {
//                DrawCustomCrosshair(over);
//            }
//        }

//        // 3. Màn hình Game Over / Mission Failed khi Unit-7 bị tiêu diệt
//        if (isGameOver)
//        {
//            DrawGameOverScreen();
//        }
//    }

//    private void DrawWaveBanner()
//    {
//        if (bannerTimer <= 0f) return;

//        EnsureWhitePixel();
//        float alpha = 1f;
//        if (bannerTimer < 0.4f)
//        {
//            alpha = bannerTimer / 0.4f;
//        }
//        else if (bannerMaxDuration - bannerTimer < 0.25f)
//        {
//            alpha = (bannerMaxDuration - bannerTimer) / 0.25f;
//        }
//        alpha = Mathf.Clamp01(alpha);

//        float bannerW = 480f;
//        float bannerH = 54f;
//        float x = (Screen.width - bannerW) * 0.5f;
//        float y = 60f;
//        Rect bannerRect = new Rect(x, y, bannerW, bannerH);

//        // Nền đen mờ công nghệ cao
//        GUI.color = new Color(0.04f, 0.06f, 0.1f, alpha * 0.90f);
//        GUI.DrawTexture(bannerRect, whitePixel);

//        // Viền trên và dưới neon phát sáng
//        Color borderC = bannerColor;
//        borderC.a = alpha * 0.95f;
//        GUI.color = borderC;
//        GUI.DrawTexture(new Rect(bannerRect.x, bannerRect.y, bannerRect.width, 2.5f), whitePixel);
//        GUI.DrawTexture(new Rect(bannerRect.x, bannerRect.yMax - 2.5f, bannerRect.width, 2.5f), whitePixel);

//        // Chữ tiêu đề Wave Banner
//        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
//        {
//            alignment = TextAnchor.MiddleCenter,
//            fontStyle = FontStyle.Bold,
//            fontSize = 16,
//            normal = { textColor = new Color(bannerColor.r, bannerColor.g, bannerColor.b, alpha) }
//        };
//        GUI.Label(new Rect(bannerRect.x, bannerRect.y + 6, bannerRect.width, 22), bannerTitle, titleStyle);

//        // Chữ phụ đề
//        if (!string.IsNullOrEmpty(bannerSubtitle))
//        {
//            GUIStyle subStyle = new GUIStyle(GUI.skin.label)
//            {
//                alignment = TextAnchor.MiddleCenter,
//                fontStyle = FontStyle.Normal,
//                fontSize = 11,
//                normal = { textColor = new Color(0.85f, 0.92f, 1f, alpha * 0.9f) }
//            };
//            GUI.Label(new Rect(bannerRect.x, bannerRect.y + 28, bannerRect.width, 18), bannerSubtitle, subStyle);
//        }

//        GUI.color = Color.white;
//    }

//    private void DrawCustomCrosshair(bool isOverheated)
//    {
//        EnsureWhitePixel();
//        Vector2 mousePos = Event.current.mousePosition;
//        float size = 16f;
//        float thickness = 2f;
//        Color crosshairColor = isOverheated ? new Color(1f, 0.2f, 0.2f, 0.9f) : new Color(0.2f, 0.9f, 1f, 0.8f);

//        GUI.color = crosshairColor;
//        // Trục ngang trái/phải
//        GUI.DrawTexture(new Rect(mousePos.x - size - 4, mousePos.y - thickness / 2, size, thickness), whitePixel);
//        GUI.DrawTexture(new Rect(mousePos.x + 4, mousePos.y - thickness / 2, size, thickness), whitePixel);
//        // Trục dọc trên/dưới
//        GUI.DrawTexture(new Rect(mousePos.x - thickness / 2, mousePos.y - size - 4, thickness, size), whitePixel);
//        GUI.DrawTexture(new Rect(mousePos.x - thickness / 2, mousePos.y + 4, thickness, size), whitePixel);

//        // Chấm tâm
//        GUI.DrawTexture(new Rect(mousePos.x - 1, mousePos.y - 1, 2, 2), whitePixel);

//        if (isOverheated)
//        {
//            GUI.color = Color.red;
//            GUI.Label(new Rect(mousePos.x - 60, mousePos.y + 16, 120, 20), "⚠️ OVERHEAT!", new GUIStyle(GUI.skin.label)
//            {
//                alignment = TextAnchor.MiddleCenter,
//                fontStyle = FontStyle.Bold
//            });
//        }
//        GUI.color = Color.white;
//    }

//    private void DrawGameOverScreen()
//    {
//        EnsureWhitePixel();
//        float screenW = Screen.width;
//        float screenH = Screen.height;

//        // Phủ tối toàn màn hình
//        GUI.color = new Color(0.05f, 0.05f, 0.08f, 0.85f);
//        GUI.DrawTexture(new Rect(0, 0, screenW, screenH), whitePixel);
//        GUI.color = Color.white;

//        // Hộp thoại trung tâm
//        float boxW = 440;
//        float boxH = 320;
//        float startX = (screenW - boxW) / 2f;
//        float startY = (screenH - boxH) / 2f;

//        GUI.Box(new Rect(startX, startY, boxW, boxH), "");

//        // Tiêu đề
//        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
//        {
//            fontSize = 24,
//            fontStyle = FontStyle.Bold,
//            alignment = TextAnchor.MiddleCenter,
//            normal = { textColor = new Color(1f, 0.25f, 0.25f) }
//        };
//        GUI.Label(new Rect(startX, startY + 20, boxW, 35), "MISSION FAILED", titleStyle);

//        // Phụ đề
//        GUIStyle subStyle = new GUIStyle(GUI.skin.label)
//        {
//            fontSize = 13,
//            alignment = TextAnchor.MiddleCenter,
//            normal = { textColor = new Color(0.8f, 0.8f, 0.8f) }
//        };
//        int depth = GameManager.Instance != null ? GameManager.Instance.CurrentDepth : 1;
//        GUI.Label(new Rect(startX, startY + 55, boxW, 20), $"Unit-7 was neutralized in Vault Sector B{depth}", subStyle);

//        // Bảng kết quả chiến dịch
//        int finalScore = GameManager.Instance != null ? GameManager.Instance.Score : 0;
//        int highScore = PlayerPrefs.GetInt("CoreBreach_HighScore", finalScore);
//        int runScrap = GameManager.Instance != null ? GameManager.Instance.CurrentScrap : 0;
//        int totalScrap = PlayerPrefs.GetInt("CoreBreach_TotalScrap", runScrap);

//        GUIStyle statStyle = new GUIStyle(GUI.skin.label)
//        {
//            fontSize = 14,
//            normal = { textColor = Color.white }
//        };

//        float statY = startY + 95;
//        GUI.Label(new Rect(startX + 50, statY, 200, 25), "• Final Score:", statStyle);
//        GUI.Label(new Rect(startX + 250, statY, 150, 25), $"{finalScore} pts", statStyle);

//        statY += 28;
//        GUI.Label(new Rect(startX + 50, statY, 200, 25), "• Best High Score:", statStyle);
//        GUI.Label(new Rect(startX + 250, statY, 150, 25), $"{highScore} pts", statStyle);

//        statY += 28;
//        GUI.Label(new Rect(startX + 50, statY, 200, 25), "• Scrap Recovered:", statStyle);
//        GUI.Label(new Rect(startX + 250, statY, 150, 25), $"+{runScrap} scrap", statStyle);

//        statY += 28;
//        GUI.Label(new Rect(startX + 50, statY, 200, 25), "• Permanent Scrap Vault:", statStyle);
//        GUI.Label(new Rect(startX + 250, statY, 150, 25), $"{totalScrap} scrap", statStyle);

//        // Nút Thử Lại (Retry)
//        float btnW = 260;
//        float btnH = 45;
//        float btnX = startX + (boxW - btnW) / 2f;
//        float btnY = startY + 225;

//        if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "RETRY MISSION  [SPACE / ENTER]"))
//        {
//            if (GameManager.Instance != null)
//            {
//                GameManager.Instance.RestartCurrentRun();
//            }
//        }

//        GUIStyle tipStyle = new GUIStyle(GUI.skin.label)
//        {
//            fontSize = 11,
//            alignment = TextAnchor.MiddleCenter,
//            normal = { textColor = Color.gray }
//        };
//        GUI.Label(new Rect(startX, btnY + 50, boxW, 20), "Core Breach v1.0 • Rogue-lite Protocol", tipStyle);
//    }

//    private void OnDestroy()
//    {
//        if (playerHealth != null)
//        {
//            playerHealth.OnHealthChanged -= UpdateHealthBar;
//            playerHealth.OnArmorChanged -= UpdateArmorBar;
//        }
//        if (heatSystem != null)
//        {
//            heatSystem.OnHeatChanged -= UpdateHeatBar;
//        }
//        if (GameManager.Instance != null)
//        {
//            GameManager.Instance.OnScrapChanged -= UpdateScrap;
//            GameManager.Instance.OnScoreChanged -= UpdateScore;
//            GameManager.Instance.OnDepthChanged -= UpdateDepth;
//        }
//    }
//}
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
    private float ghostHpPct = 1f;
    private float ghostArmorPct = 1f;

    // =========================================================
    // WAVE BANNER SYSTEM (SOUL KNIGHT SCREEN-SPACE BANNER)
    // =========================================================
    private static string bannerTitle = "";
    private static string bannerSubtitle = "";
    private static Color bannerColor = Color.cyan;
    private static float bannerTimer = 0f;
    private static float bannerMaxDuration = 2.0f;

    public static void ShowWaveBanner(string title, string subtitle, Color color, float duration = 2.0f)
    {
        bannerTitle = title;
        bannerSubtitle = subtitle;
        bannerColor = color;
        bannerTimer = duration;
        bannerMaxDuration = duration;
    }

    private static float screenFlashAlpha = 0f;
    private static Color screenFlashColor = Color.white;

    public static void FlashScreen(Color color, float alpha = 0.85f)
    {
        screenFlashColor = color;
        screenFlashAlpha = alpha;
    }

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

        // Cập nhật thanh trễ Ghost Bar (Chip Damage lag bar)
        if (playerHealth != null && playerHealth.MaxHealth > 0)
        {
            float targetHpPct = playerHealth.CurrentHealth / playerHealth.MaxHealth;
            if (ghostHpPct > targetHpPct) ghostHpPct = Mathf.MoveTowards(ghostHpPct, targetHpPct, Time.deltaTime * 0.45f);
            else ghostHpPct = targetHpPct;

            float targetArmPct = playerHealth.MaxArmor > 0 ? playerHealth.CurrentArmor / playerHealth.MaxArmor : 0f;
            if (ghostArmorPct > targetArmPct) ghostArmorPct = Mathf.MoveTowards(ghostArmorPct, targetArmPct, Time.deltaTime * 0.45f);
            else ghostArmorPct = targetArmPct;
        }

        // Cập nhật đếm ngược Wave Banner
        if (bannerTimer > 0f)
        {
            bannerTimer -= Time.deltaTime;
        }

        // Nếu Game Over, cho phép bấm phím Space hoặc Enter để Restart ngay
        bool isGameOver = (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.GameOver) ||
                          (playerHealth != null && playerHealth.CurrentHealth <= 0f);

        if (isGameOver)
        {
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
            playerHealth = FindFirstObjectByType<PlayerHealth>();
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
            heatSystem = FindFirstObjectByType<HeatSystem>();
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

    private void DrawBar(Rect rect, float fillPercent, Color fillColor, Color bgColor, float ghostPercent = -1f, Color? ghostColor = null)
    {
        EnsureWhitePixel();

        // Viền ngoài kim loại
        GUI.color = new Color(0.18f, 0.24f, 0.36f, 0.95f);
        GUI.DrawTexture(rect, whitePixel);

        // Nền thanh bên trong
        Rect innerBg = new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2);
        GUI.color = bgColor;
        GUI.DrawTexture(innerBg, whitePixel);

        // Thanh Ghost Lag / Chip Damage (màu vàng cam hoặc trắng mờ chậm 0.3s)
        if (ghostPercent > fillPercent + 0.005f)
        {
            float ghostWidth = (rect.width - 2) * Mathf.Clamp01(ghostPercent);
            Rect ghostRect = new Rect(rect.x + 1, rect.y + 1, ghostWidth, rect.height - 2);
            GUI.color = ghostColor ?? new Color(1f, 0.75f, 0.15f, 0.85f);
            GUI.DrawTexture(ghostRect, whitePixel);
        }

        // Phần trăm thanh năng lượng phát sáng
        if (fillPercent > 0.01f)
        {
            float fillWidth = (rect.width - 2) * Mathf.Clamp01(fillPercent);
            Rect fillRect = new Rect(rect.x + 1, rect.y + 1, fillWidth, rect.height - 2);
            GUI.color = fillColor;
            GUI.DrawTexture(fillRect, whitePixel);

            // Vạch sáng phản quang ở đỉnh thanh
            GUI.color = new Color(1f, 1f, 1f, 0.35f);
            Rect shineRect = new Rect(rect.x + 1, rect.y + 1, fillWidth, 2);
            GUI.DrawTexture(shineRect, whitePixel);
        }
        GUI.color = Color.white;
    }

    private void OnGUI()
    {
        // Ẩn HUD nếu đang ở MainMenu, CharacterSelect hoặc Paused
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameState.Playing &&
            GameManager.Instance.CurrentState != GameState.GameOver)
        {
            return;
        }

        // Ẩn HUD nếu đang mở Menu chọn nhân vật
        if (CharacterSelectSystem.Instance != null && CharacterSelectSystem.Instance.IsSelectionOpen)
        {
            return;
        }

        // Flash màn hình dịch chuyển / cảnh báo
        if (screenFlashAlpha > 0.01f)
        {
            EnsureWhitePixel();
            Color c = screenFlashColor;
            c.a = screenFlashAlpha;
            GUI.color = c;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whitePixel);
            GUI.color = Color.white;
            screenFlashAlpha = Mathf.MoveTowards(screenFlashAlpha, 0f, Time.deltaTime * 2.8f);
        }

        bool isGameOver = (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.GameOver) ||
                          (playerHealth != null && playerHealth.CurrentHealth <= 0f);

        // HUD Sci-Fi Cyberpunk hiển thị trực tiếp sắc nét
        if (healthSlider == null)
        {
            float hp = playerHealth != null ? playerHealth.CurrentHealth : 100f;
            float armor = playerHealth != null ? playerHealth.CurrentArmor : 20f;
            float heat = heatSystem != null ? heatSystem.CurrentHeat : 0f;
            bool over = heatSystem != null && heatSystem.IsOverheated;
            int scrap = GameManager.Instance != null ? GameManager.Instance.CurrentScrap : 0;
            int depth = GameManager.Instance != null ? GameManager.Instance.CurrentDepth : 1;
            int score = GameManager.Instance != null ? GameManager.Instance.Score : 0;

            WeaponController wc = playerHealth != null ? playerHealth.GetComponent<WeaponController>() : null;
            string currentWeaponName = wc != null && wc.CurrentWeapon != null ? wc.CurrentWeapon.WeaponName : "Laser Rifle";
            int slotIdx = wc != null ? wc.CurrentSlotIndex : 0;

            PlayerMovement pm = playerHealth != null ? playerHealth.GetComponent<PlayerMovement>() : null;

            EnsureWhitePixel();

            // 1. KHUNG TRẠNG THÁI NHỎ GỌN (SOUL KNIGHT STYLE: CHỈ 3 THANH HP, GIÁP, NHIỆT)
            Rect panelRect = new Rect(16, 16, 218, 76);

            // Viền ngoài phát sáng Cyan / Đỏ khi quá nhiệt
            GUI.color = over ? new Color(1f, 0.2f, 0.2f, 0.85f) : new Color(0f, 0.85f, 1f, 0.75f);
            GUI.DrawTexture(new Rect(panelRect.x - 1, panelRect.y - 1, panelRect.width + 2, panelRect.height + 2), whitePixel);

            // Nền tối mờ công nghệ cao
            GUI.color = new Color(0.06f, 0.08f, 0.13f, 0.92f);
            GUI.DrawTexture(panelRect, whitePixel);

            GUIStyle barLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.9f, 0.95f, 1f, 1f) }
            };
            GUIStyle numStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = Color.white }
            };

            // THANH HP (Máu)
            float maxHp = playerHealth != null ? playerHealth.MaxHealth : 100f;
            float hpPct = maxHp > 0 ? hp / maxHp : 1f;
            Color hpCol = hpPct > 0.5f ? new Color(0f, 0.9f, 0.45f) : (hpPct > 0.25f ? new Color(1f, 0.75f, 0.1f) : new Color(1f, 0.15f, 0.25f));
            GUI.color = Color.white;
            GUI.Label(new Rect(22, 20, 36, 15), "HP", barLabelStyle);
            DrawBar(new Rect(58, 20, 96, 14), hpPct, hpCol, new Color(0.12f, 0.05f, 0.06f, 0.9f), ghostHpPct, new Color(1f, 0.8f, 0.2f, 0.85f));
            GUI.Label(new Rect(156, 20, 72, 15), $"{Mathf.CeilToInt(hp)}/{Mathf.CeilToInt(maxHp)}", numStyle);

            // THANH GIÁP (Armor)
            float maxArm = playerHealth != null ? playerHealth.MaxArmor : 20f;
            float armPct = maxArm > 0 ? armor / maxArm : 0f;
            GUI.color = Color.white;
            GUI.Label(new Rect(22, 38, 36, 15), "GIÁP", barLabelStyle);
            DrawBar(new Rect(58, 38, 96, 14), armPct, new Color(0f, 0.75f, 1f), new Color(0.04f, 0.12f, 0.18f, 0.9f), ghostArmorPct, new Color(0.4f, 0.85f, 1f, 0.75f));
            GUI.Label(new Rect(156, 38, 72, 15), $"{Mathf.CeilToInt(armor)}/{Mathf.CeilToInt(maxArm)}", numStyle);

            // THANH NHIỆT (Overheat / Năng lượng)
            Color heatCol = over ? (Mathf.Sin(Time.time * 12f) > 0 ? Color.red : Color.yellow) : (heat > 60f ? new Color(1f, 0.55f, 0.1f) : new Color(1f, 0.5f, 0.1f));
            GUI.color = over ? Color.red : Color.white;
            GUI.Label(new Rect(22, 56, 36, 15), over ? "QUÁ NHIỆT" : "NHIỆT", barLabelStyle);
            DrawBar(new Rect(58, 56, 96, 14), heat / 100f, heatCol, new Color(0.15f, 0.08f, 0.04f, 0.9f));
            GUI.Label(new Rect(156, 56, 72, 15), $"{Mathf.CeilToInt(heat)}%", numStyle);

            // DÒNG THÔNG BÁO NHỎ GỌN PHÍA DƯỚI PANEL
            GUI.color = new Color(1f, 0.85f, 0.2f, 0.9f);
            GUIStyle miniStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.85f, 0.2f, 0.9f) }
            };
            GUI.Label(new Rect(18, 96, 218, 16), $"💎 {scrap} SCRAP   •   TẦNG B{depth}", miniStyle);

            GUI.color = Color.white;

            // 2. HIỂN THỊ WAVE BANNER HOÀNH TRÁNG Ở GIỮA MÀN HÌNH (SOUL KNIGHT STYLE)
            DrawWaveBanner();

            // 3. CROSSHAIR SCI-FI QUANH CHUỘT
            if (!isGameOver)
            {
                DrawCustomCrosshair(over);
            }
        }

        // 3. Màn hình Game Over / Mission Failed khi Unit-7 bị tiêu diệt
        if (isGameOver)
        {
            DrawGameOverScreen();
        }
    }

    private void DrawWaveBanner()
    {
        if (bannerTimer <= 0f) return;

        EnsureWhitePixel();
        float alpha = 1f;
        if (bannerTimer < 0.4f)
        {
            alpha = bannerTimer / 0.4f;
        }
        else if (bannerMaxDuration - bannerTimer < 0.25f)
        {
            alpha = (bannerMaxDuration - bannerTimer) / 0.25f;
        }
        alpha = Mathf.Clamp01(alpha);

        float bannerW = 480f;
        float bannerH = 54f;
        float x = (Screen.width - bannerW) * 0.5f;
        float y = 60f;
        Rect bannerRect = new Rect(x, y, bannerW, bannerH);

        // Nền đen mờ công nghệ cao
        GUI.color = new Color(0.04f, 0.06f, 0.1f, alpha * 0.90f);
        GUI.DrawTexture(bannerRect, whitePixel);

        // Viền trên và dưới neon phát sáng
        Color borderC = bannerColor;
        borderC.a = alpha * 0.95f;
        GUI.color = borderC;
        GUI.DrawTexture(new Rect(bannerRect.x, bannerRect.y, bannerRect.width, 2.5f), whitePixel);
        GUI.DrawTexture(new Rect(bannerRect.x, bannerRect.yMax - 2.5f, bannerRect.width, 2.5f), whitePixel);

        // Chữ tiêu đề Wave Banner
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 16,
            normal = { textColor = new Color(bannerColor.r, bannerColor.g, bannerColor.b, alpha) }
        };
        GUI.Label(new Rect(bannerRect.x, bannerRect.y + 6, bannerRect.width, 22), bannerTitle, titleStyle);

        // Chữ phụ đề
        if (!string.IsNullOrEmpty(bannerSubtitle))
        {
            GUIStyle subStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Normal,
                fontSize = 11,
                normal = { textColor = new Color(0.85f, 0.92f, 1f, alpha * 0.9f) }
            };
            GUI.Label(new Rect(bannerRect.x, bannerRect.y + 28, bannerRect.width, 18), bannerSubtitle, subStyle);
        }

        GUI.color = Color.white;
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