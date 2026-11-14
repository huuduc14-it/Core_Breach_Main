using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý menu tạm dừng (Pause Menu) trong Core Breach:
/// - Nhấn phím ESC để mở/đóng tạm dừng
/// - Cung cấp các chức năng: Tiếp tục (Resume), Chơi lại (Restart), Về Menu chính
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenuPanel;

    private bool isPaused = false;

    public bool IsPaused => isPaused;

    private void Start()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Nhấn phím ESC để bật/tắt pause theo Game Concept
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
        Time.timeScale = 0f; // Đóng băng thời gian
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        Time.timeScale = 1f; // Tiếp tục thời gian bình thường
    }

    public void RestartRun()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartCurrentRun();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        // Nếu có scene MainMenu thì load, nếu không thì reload run
        if (Application.CanStreamedLevelBeLoaded("MainMenu"))
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            Debug.Log("[PauseMenu] Thoát game / Chưa có Scene MainMenu, reload scene hiện tại.");
            SceneManager.LoadScene(0);
        }
    }

    private void OnGUI()
    {
        if (isPaused && pauseMenuPanel == null)
        {
            float screenW = Screen.width;
            float screenH = Screen.height;
            float boxW = 320;
            float boxH = 230;
            float startX = (screenW - boxW) / 2f;
            float startY = (screenH - boxH) / 2f;

            GUI.Box(new Rect(startX, startY, boxW, boxH), "SYSTEM PAUSED [ESC]");

            float btnW = 220;
            float btnH = 38;
            float btnX = startX + (boxW - btnW) / 2f;

            if (GUI.Button(new Rect(btnX, startY + 45, btnW, btnH), "RESUME MISSION  [ESC]"))
            {
                ResumeGame();
            }
            if (GUI.Button(new Rect(btnX, startY + 95, btnW, btnH), "RESTART RUN"))
            {
                RestartRun();
            }
            if (GUI.Button(new Rect(btnX, startY + 145, btnW, btnH), "QUIT"))
            {
                QuitToMainMenu();
            }
        }
    }
}
