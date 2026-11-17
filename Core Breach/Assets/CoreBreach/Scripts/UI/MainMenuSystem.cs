using UnityEngine;

/// <summary>
/// Màn hình chính (Main Menu / Lobby) phong cách Soul Knight cho Core Breach:
/// - Tiêu đề Cyberpunk hoành tráng phát sáng Neon
/// - Thống kê thành tích vĩnh viễn (High Score, Permanent Scrap Vault)
/// - [START NEW RUN]: Bắt đầu màn chơi mới, chọn nhân vật
/// - [RESUME MISSION]: Tiếp tục trận đánh đang dang dở
/// - [SELECT OPERATOR]: Xem và lựa chọn 3 đặc vụ (Unit-7, Unit-0, Unit-9)
/// - [SETTINGS]: Bật/Tắt Âm thanh, Nhạc nền, Tia Laser ngắm bắn
/// - [QUIT GAME]: Thoát game (hoạt động cho cả Unity Editor và Standalone Build)
/// </summary>
public class MainMenuSystem : MonoBehaviour
{
    public static MainMenuSystem Instance { get; private set; }

    [Header("Settings Submenu")]
    private bool isSettingsOpen = false;

    private static Texture2D bgTex;
    private static Texture2D cardTex;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }
    }

    private void Start()
    {
        // Khi mở game lần đầu, đặt trạng thái là MainMenu
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.MainMenu)
        {
            Time.timeScale = 1f;
        }
    }

    private void Update()
    {
        // Phím tắt nhanh trong Main Menu
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.MainMenu)
        {
            if (isSettingsOpen)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) isSettingsOpen = false;
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                StartNewRun();
            }
        }
    }

    public void OpenMainMenu()
    {
        isSettingsOpen = false;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameState.MainMenu);
        }
    }

    public void StartNewRun()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewRun();
        }
        if (CharacterSelectSystem.Instance != null)
        {
            CharacterSelectSystem.Instance.OpenSelectionMenu();
        }
    }

    public void ResumeRun()
    {
        if (GameManager.Instance != null && GameManager.Instance.HasActiveRun)
        {
            //GameManager.Instance.ResumeGame();
            GameManager.Instance.ChangeState(GameState.Playing);
        }
    }

    public void OpenCharacterSelect()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameState.CharacterSelect);
        }
        if (CharacterSelectSystem.Instance != null)
        {
            CharacterSelectSystem.Instance.OpenSelectionMenu();
        }
    }

    private static void EnsureTextures()
    {
        if (bgTex == null)
        {
            bgTex = new Texture2D(1, 1);
            bgTex.SetPixel(0, 0, Color.white);
            bgTex.Apply();
        }
        if (cardTex == null)
        {
            cardTex = new Texture2D(1, 1);
            cardTex.SetPixel(0, 0, new Color(0.12f, 0.16f, 0.24f, 0.95f));
            cardTex.Apply();
        }
    }

    private void OnGUI()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.CurrentState == GameState.Victory)
        {
            GUIStyle victoryTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 42,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            victoryTitleStyle.normal.textColor = Color.cyan;

            GUI.Label(
                new Rect(0, Screen.height * 0.34f, Screen.width, 70),
                "CORE GUARDIAN DEFEATED",
                victoryTitleStyle
            );

            GUIStyle scoreStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter
            };

            GUI.Label(
                new Rect(0, Screen.height * 0.44f, Screen.width, 40),
                $"RUN COMPLETE | SCORE: {GameManager.Instance.Score}",
                scoreStyle
            );

            if (GUI.Button(
                new Rect(
                    Screen.width / 2f - 130,
                    Screen.height * 0.58f,
                    260,
                    50
                ),
                "RETURN TO MAIN MENU"))
            {
                Time.timeScale = 1f;
                GameManager.Instance.ChangeState(GameState.MainMenu);
            }

            return;
        }
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.MainMenu) return;

        EnsureTextures();
        float screenW = Screen.width;
        float screenH = Screen.height;

        // 1. NỀN KHÔNG GIAN CYBERPUNK (Deep Dark Sci-Fi Vignette)
        GUI.color = new Color(0.04f, 0.06f, 0.10f, 0.94f);
        GUI.DrawTexture(new Rect(0, 0, screenW, screenH), bgTex);
        GUI.color = Color.white;

        // 2. KHUNG NỘI DUNG CHÍNH (MAIN MENU CARD)
        float menuW = Mathf.Min(560f, screenW - 40f);
        float menuH = Mathf.Min(520f, screenH - 40f);
        float startX = (screenW - menuW) / 2f;
        float startY = (screenH - menuH) / 2f;

        // Viền phát sáng kim loại
        GUI.color = new Color(0.2f, 0.85f, 1f, 0.9f);
        GUI.DrawTexture(new Rect(startX - 2, startY - 2, menuW + 4, menuH + 4), bgTex);
        GUI.color = Color.white;

        GUI.DrawTexture(new Rect(startX, startY, menuW, menuH), cardTex);

        // 3. TIÊU ĐỀ LOGO GAME (SOUL KNIGHT STYLE CYBERPUNK LOGO)
        float pulse = (Mathf.Sin(Time.unscaledTime * 3f) + 1f) * 0.5f;
        Color titleGlow = Color.Lerp(new Color(0.1f, 0.95f, 1f), new Color(1f, 0.75f, 0.2f), pulse * 0.4f);

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 36,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = titleGlow }
        };
        GUI.Label(new Rect(startX, startY + 24, menuW, 46), "⚡ CORE BREACH ⚡", titleStyle);

        GUIStyle subTitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.7f, 0.85f, 1f, 0.9f) }
        };
        GUI.Label(new Rect(startX, startY + 70, menuW, 20), "ROGUE-LITE INFILTRATION PROTOCOL • SOUL KNIGHT COMBAT ENGINE", subTitleStyle);

        // 4. THỐNG KÊ THÀNH TÍCH (HIGH SCORE & PERMANENT SCRAP)
        int highScore = PlayerPrefs.GetInt("CoreBreach_HighScore", 0);
        int totalScrap = PlayerPrefs.GetInt("CoreBreach_TotalScrap", 0);

        Rect statBox = new Rect(startX + 30, startY + 100, menuW - 60, 48);
        GUI.color = new Color(0.08f, 0.11f, 0.17f, 0.9f);
        GUI.DrawTexture(statBox, bgTex);
        GUI.color = Color.white;

        GUIStyle statStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(1f, 0.85f, 0.2f) }
        };
        GUI.Label(statBox, $"🏆 KỶ LỤC: {highScore} ĐIỂM   |   💎 KHO SCRAP VĨNH VIỄN: {totalScrap} SCRAP", statStyle);

        // 5. NẾU ĐANG MỞ SETTINGS SUBMENU
        if (isSettingsOpen)
        {
            DrawSettingsMenu(startX, startY + 160, menuW, menuH - 180);
            return;
        }

        // 6. CÁC NÚT ĐIỀU HƯỚNG CHÍNH
        float btnW = 320;
        float btnH = 46;
        float btnX = startX + (menuW - btnW) / 2f;
        float curY = startY + 165;

        // NÚT 1: BẮT ĐẦU XÂM NHẬP MỚI
        GUI.color = new Color(0.2f, 1f, 0.5f);
        if (GUI.Button(new Rect(btnX, curY, btnW, btnH), "▶ BẮT ĐẦU XÂM NHẬP MỚI  [ENTER]"))
        {
            StartNewRun();
        }
        GUI.color = Color.white;

        // NÚT 2: TIẾP TỤC TRẬN ĐÁNH (RESUME)
        curY += 56;
        bool hasRun = GameManager.Instance != null && GameManager.Instance.HasActiveRun;
        GUI.enabled = hasRun;
        if (!hasRun) GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.5f);
        else GUI.color = new Color(0.2f, 0.9f, 1f);

        string resumeLabel = hasRun 
            ? $"⏩ TIẾP TỤC TRẬN ĐẤU (TẦNG B{GameManager.Instance.CurrentDepth})" 
            : "⏩ TIẾP TỤC (CHƯA CÓ TRẬN ĐANG CHƠI)";

        if (GUI.Button(new Rect(btnX, curY, btnW, btnH), resumeLabel))
        {
            ResumeRun();
        }
        GUI.enabled = true;
        GUI.color = Color.white;

        // NÚT 3: CHỌN ĐẶC VỤ (OPERATORS)
        curY += 56;
        if (GUI.Button(new Rect(btnX, curY, btnW, btnH), "◈ CHỌN ĐẶC VỤ CHIẾN ĐẤU (UNITS)"))
        {
            OpenCharacterSelect();
        }

        // NÚT 4: CÀI ĐẶT ÂM THANH & TÙY CHỌN
        curY += 56;
        if (GUI.Button(new Rect(btnX, curY, btnW, btnH), "⚙ CÀI ĐẶT & ÂM THANH (SETTINGS)"))
        {
            isSettingsOpen = true;
        }

        // NÚT 5: THOÁT GAME (QUIT GAME)
        curY += 56;
        GUI.color = new Color(1f, 0.45f, 0.45f);
        if (GUI.Button(new Rect(btnX, curY, btnW, btnH), "✖ THOÁT TRÒ CHƠI (QUIT GAME)"))
        {
            GameManager.QuitGame();
        }
        GUI.color = Color.white;

        // FOOTER
        GUIStyle footerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.5f, 0.6f, 0.7f) }
        };
        GUI.Label(new Rect(startX, startY + menuH - 30, menuW, 20), "Core Breach v1.2 • Unity 6 Engine • Top-Down Roguelite", footerStyle);
    }

    private void DrawSettingsMenu(float startX, float startY, float w, float h)
    {
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.cyan }
        };
        GUI.Label(new Rect(startX, startY + 10, w, 28), "⚙ THIẾT LẬP HỆ THỐNG", headerStyle);

        float btnW = 280;
        float btnH = 42;
        float btnX = startX + (w - btnW) / 2f;
        float curY = startY + 45;

        // 1. SFX Toggle
        bool sfx = AudioManager.Instance == null || AudioManager.Instance.IsSFXEnabled;
        if (GUI.Button(new Rect(btnX, curY, btnW, btnH), $"ÂM THANH HIỆU ỨNG (SFX): {(sfx ? "[BẬT]" : "[TẮT]")}"))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXEnabled(!sfx);
            }
        }

        // 2. Music Toggle
        curY += 50;
        bool music = AudioManager.Instance == null || AudioManager.Instance.IsMusicEnabled;
        if (GUI.Button(new Rect(btnX, curY, btnW, btnH), $"NHẠC NỀN (MUSIC): {(music ? "[BẬT]" : "[TẮT]")}"))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicEnabled(!music);
            }
        }

        // 3. Laser Sight Toggle
        curY += 50;
        PlayerAim aim = FindFirstObjectByType<PlayerAim>();
        bool laser = aim != null && aim.ShowLaserSight;
        if (GUI.Button(new Rect(btnX, curY, btnW, btnH), $"TIA LASER CHIẾN THUẬT: {(laser ? "[BẬT - PHÍM L]" : "[TẮT - PHÍM L]")}"))
        {
            if (aim != null)
            {
                aim.ShowLaserSight = !laser;
            }
        }

        // 4. Nút Quay Lại
        curY += 60;
        GUI.color = new Color(1f, 0.85f, 0.2f);
        if (GUI.Button(new Rect(btnX, curY, btnW, btnH), "◀ QUAY LẠI MENU CHÍNH  [ESC]"))
        {
            isSettingsOpen = false;
        }
        GUI.color = Color.white;
    }
}
