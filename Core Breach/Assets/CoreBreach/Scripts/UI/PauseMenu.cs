//using System.Diagnostics;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using static System.Net.Mime.MediaTypeNames;

//<<<<<<< Updated upstream
///// <summary>
///// Quản lý menu tạm dừng (Pause Menu) trong Core Breach chuẩn phong cách Soul Knight:
//=======
///// <summary>1
///// Quản lý menu tạm dừng (Pause Menu) trong Core Breach:
//>>>>>>> Stashed changes
///// - Nhấn phím ESC để mở/đóng tạm dừng
///// - Cung cấp 4 chức năng: Tiếp tục (Resume), Chơi lại (Restart), Về Menu chính (Main Menu), Thoát Game (Desktop)
///// </summary>
//public class PauseMenu : MonoBehaviour
//{
//    public static PauseMenu Instance { get; private set; }

//    [Header("UI Panels")]
//    [SerializeField] private GameObject pauseMenuPanel;

//    private static Texture2D dimTex;

//    private void Awake()
//    {
//        if (Instance == null) Instance = this;
//        else if (Instance != this) { Destroy(gameObject); return; }
//    }

//    private void Start()
//    {
//        if (pauseMenuPanel != null)
//        {
//            pauseMenuPanel.SetActive(false);
//        }
//    }

//    private void Update()
//    {
//<<<<<<< Updated upstream
//        // Nhấn phím ESC để bật/tắt pause khi đang chơi hoặc đang pause
//=======
//        // Không cho pause khi đã Game Over (màn hình Game Over do PlayerHUD hiển thị)
//        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.GameOver)
//        {
//            if (isPaused) ResumeGame();
//            return;
//        }

//        // Nhấn phím ESC để bật/tắt pause theo Game Concept
//>>>>>>> Stashed changes
//        if (Input.GetKeyDown(KeyCode.Escape))
//        {
//            if (GameManager.Instance == null) return;

//            if (GameManager.Instance.CurrentState == GameState.Playing)
//            {
//                PauseGame();
//            }
//            else if (GameManager.Instance.CurrentState == GameState.Paused)
//            {
//                ResumeGame();
//            }
//        }
//    }

//    public void PauseGame()
//    {
//        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
//        if (GameManager.Instance != null)
//        {
//            GameManager.Instance.PauseGame();
//        }
//        else
//        {
//            Time.timeScale = 0f;
//        }
//<<<<<<< Updated upstream
//=======
//        Time.timeScale = 0f; // Đóng băng thời gian
//        AudioListener.pause = true;
//>>>>>>> Stashed changes
//    }

//    public void ResumeGame()
//    {
//        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
//        if (GameManager.Instance != null)
//        {
//            GameManager.Instance.ResumeGame();
//        }
//        else
//        {
//            Time.timeScale = 1f;
//        }
//<<<<<<< Updated upstream
//=======
//        Time.timeScale = 1f; // Tiếp tục thời gian bình thường
//        AudioListener.pause = false;
//>>>>>>> Stashed changes
//    }

//    public void RestartRun()
//    {
//        Time.timeScale = 1f;
//<<<<<<< Updated upstream
//        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
//=======
//        AudioListener.pause = false;
//>>>>>>> Stashed changes
//        if (GameManager.Instance != null)
//        {
//            GameManager.Instance.RestartCurrentRun();
//        }
//        else
//        {
//            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
//        }
//    }

//    public void ReturnToMainMenu()
//    {
//        Time.timeScale = 1f;
//<<<<<<< Updated upstream
//        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
//        if (GameManager.Instance != null)
//=======
//        AudioListener.pause = false;
//        // Nếu có scene MainMenu thì load, nếu không thì reload run
//        if (UnityEngine.Application.CanStreamedLevelBeLoaded("MainMenu"))
//>>>>>>> Stashed changes
//        {
//            GameManager.Instance.ReturnToMainMenu();
//        }
//    }

//    public void QuitGameToDesktop()
//    {
//        Time.timeScale = 1f;
//        GameManager.QuitGame();
//    }

//    private static void EnsureDimTex()
//    {
//        if (dimTex == null)
//        {
//<<<<<<< Updated upstream
//            dimTex = new Texture2D(1, 1);
//            dimTex.SetPixel(0, 0, Color.white);
//            dimTex.Apply();
//=======
//            UnityEngine.Debug.Log("[PauseMenu] Thoát game / Chưa có Scene MainMenu, reload scene hiện tại.");
//            SceneManager.LoadScene(0);
//>>>>>>> Stashed changes
//        }
//    }

//    private void OnGUI()
//    {
//        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Paused) return;

//        EnsureDimTex();
//        float screenW = Screen.width;
//        float screenH = Screen.height;

//        // Phủ mờ màn hình nền
//        GUI.color = new Color(0.03f, 0.05f, 0.09f, 0.85f);
//        GUI.DrawTexture(new Rect(0, 0, screenW, screenH), dimTex);
//        GUI.color = Color.white;

//        float boxW = 360;
//        float boxH = 310;
//        float startX = (screenW - boxW) / 2f;
//        float startY = (screenH - boxH) / 2f;

//        GUI.Box(new Rect(startX, startY, boxW, boxH), "");

//        // Tiêu đề
//        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
//        {
//            fontSize = 22,
//            fontStyle = FontStyle.Bold,
//            alignment = TextAnchor.MiddleCenter,
//            normal = { textColor = new Color(0.2f, 0.9f, 1f) }
//        };
//        GUI.Label(new Rect(startX, startY + 18, boxW, 30), "SYSTEM PAUSED", titleStyle);

//        GUIStyle subStyle = new GUIStyle(GUI.skin.label)
//        {
//            fontSize = 12,
//            alignment = TextAnchor.MiddleCenter,
//            normal = { textColor = new Color(0.7f, 0.8f, 0.9f) }
//        };
//        int depth = GameManager.Instance != null ? GameManager.Instance.CurrentDepth : 1;
//        GUI.Label(new Rect(startX, startY + 48, boxW, 20), $"VAULT PROTOCOL SUSPENDED • SECTOR B{depth}", subStyle);

//        float btnW = 260;
//        float btnH = 40;
//        float btnX = startX + (boxW - btnW) / 2f;
//        float curY = startY + 80;

//        // 1. TIẾP TỤC
//        if (GUI.Button(new Rect(btnX, curY, btnW, btnH), "▶ RESUME MISSION  [ESC]"))
//        {
//            ResumeGame();
//        }

//        // 2. CHƠI LẠI
//        curY += 50;
//        if (GUI.Button(new Rect(btnX, curY, btnW, btnH), "↻ RESTART RUN"))
//        {
//            RestartRun();
//        }

//        // 3. VỀ MENU CHÍNH
//        curY += 50;
//        if (GUI.Button(new Rect(btnX, curY, btnW, btnH), "⌂ RETURN TO MAIN MENU"))
//        {
//            ReturnToMainMenu();
//        }

//        // 4. THOÁT GAME RA DESKTOP
//        curY += 50;
//        GUI.color = new Color(1f, 0.6f, 0.6f);
//        if (GUI.Button(new Rect(btnX, curY, btnW, btnH), "✖ QUIT TO DESKTOP"))
//        {
//            QuitGameToDesktop();
//        }
//        GUI.color = Color.white;
//    }
//}
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý menu tạm dừng (Pause Menu) trong Core Breach chuẩn phong cách Soul Knight:
/// - Nhấn phím ESC để mở/đóng tạm dừng
/// - Cung cấp 4 chức năng: Tiếp tục (Resume), Chơi lại (Restart), Về Menu chính (Main Menu), Thoát Game (Desktop)
/// </summary>
public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenuPanel;

    private static Texture2D dimTex;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }
    }

    private void Start()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Không cho pause khi đã Game Over (màn hình Game Over do PlayerHUD hiển thị)
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.GameOver)
        {
            return;
        }

        // Nhấn phím ESC để bật/tắt pause khi đang chơi hoặc đang pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance == null) return;

            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (GameManager.Instance.CurrentState == GameState.Paused)
            {
                ResumeGame();
            }
        }
    }

    public void PauseGame()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
        else
        {
            Time.timeScale = 0f;
        }
        AudioListener.pause = true;
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
        else
        {
            Time.timeScale = 1f;
        }
        AudioListener.pause = false;
    }

    public void RestartRun()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartCurrentRun();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
    }

    public void QuitGameToDesktop()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        GameManager.QuitGame();
    }

    private static void EnsureDimTex()
    {
        if (dimTex == null)
        {
            dimTex = new Texture2D(1, 1);
            dimTex.SetPixel(0, 0, Color.white);
            dimTex.Apply();
        }
    }

    private void OnGUI()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Paused) return;
        if (pauseMenuPanel != null) return;
        EnsureDimTex();
        float screenW = Screen.width;
        float screenH = Screen.height;

        // Phủ mờ màn hình nền
        GUI.color = new Color(0.03f, 0.05f, 0.09f, 0.85f);
        GUI.DrawTexture(new Rect(0, 0, screenW, screenH), dimTex);
        GUI.color = Color.white;

        float boxW = 360;
        float boxH = 310;
        float startX = (screenW - boxW) / 2f;
        float startY = (screenH - boxH) / 2f;

        GUI.Box(new Rect(startX, startY, boxW, boxH), "");

        // Tiêu đề
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.2f, 0.9f, 1f) }
        };
        GUI.Label(new Rect(startX, startY + 18, boxW, 30), "SYSTEM PAUSED", titleStyle);

        GUIStyle subStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.7f, 0.8f, 0.9f) }
        };
        int depth = GameManager.Instance != null ? GameManager.Instance.CurrentDepth : 1;
        GUI.Label(new Rect(startX, startY + 48, boxW, 20), $"VAULT PROTOCOL SUSPENDED • SECTOR B{depth}", subStyle);

        float btnW = 260;
        float btnH = 40;
        float btnX = startX + (boxW - btnW) / 2f;
        float curY = startY + 80;

        // 1. TIẾP TỤC
        if (GUI.Button(new Rect(btnX, curY, btnW, btnH), "▶ RESUME MISSION  [ESC]"))
        {
            ResumeGame();
        }

        // 2. CHƠI LẠI
        curY += 50;
        if (GUI.Button(new Rect(btnX, curY, btnW, btnH), "↻ RESTART RUN"))
        {
            RestartRun();
        }

        // 3. VỀ MENU CHÍNH
        curY += 50;
        if (GUI.Button(new Rect(btnX, curY, btnW, btnH), "⌂ RETURN TO MAIN MENU"))
        {
            ReturnToMainMenu();
        }

        // 4. THOÁT GAME RA DESKTOP
        curY += 50;
        GUI.color = new Color(1f, 0.6f, 0.6f);
        if (GUI.Button(new Rect(btnX, curY, btnW, btnH), "✖ QUIT TO DESKTOP"))
        {
            QuitGameToDesktop();
        }
        GUI.color = Color.white;
    }
}