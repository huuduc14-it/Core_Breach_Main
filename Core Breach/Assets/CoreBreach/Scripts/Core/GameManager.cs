using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý vòng lặp chính của Core Breach: Điểm số, Scrap, Tầng hầm (Depth), Game Over
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Run Stats")]
    [SerializeField] private int currentScrap = 0;
    [SerializeField] private int currentDepth = 1;
    [SerializeField] private int score = 0;

    public int CurrentScrap => currentScrap;
    public int CurrentDepth => currentDepth;
    public int Score => score;

    public GameState CurrentState { get; private set; } = GameState.MainMenu;
    public bool HasActiveRun { get; private set; } = false;

    public event Action<GameState> OnStateChanged;
    public event Action<int> OnScrapChanged;
    public event Action<int> OnScoreChanged;
    public event Action<int> OnDepthChanged;
    public event Action OnGameOver;

    private const string PREF_HIGH_SCORE = "CoreBreach_HighScore";
    private const string PREF_TOTAL_SCRAP = "CoreBreach_TotalScrap";

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;
        GameState oldState = CurrentState;
        CurrentState = newState;

        if (newState == GameState.Playing)
        {
            HasActiveRun = true;
            Time.timeScale = 1f;
        }
        else if (newState == GameState.Paused)
        {
            Time.timeScale = 0f;
        }
        else if (newState == GameState.MainMenu || newState == GameState.CharacterSelect)
        {
            Time.timeScale = 1f;
        }
        else if (newState == GameState.Victory)
        {
            HasActiveRun = false;
            Time.timeScale = 0f;
        }

        OnStateChanged?.Invoke(newState);
        Debug.Log($"[GameManager] Chuyển trạng thái: {oldState} -> {newState}");
    }

    public void SetState(GameState newState)
    {
        ChangeState(newState);
    }

    public void PauseGame()
    {
        if (CurrentState == GameState.Playing)
        {
            ChangeState(GameState.Paused);
        }
    }

    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            ChangeState(GameState.Playing);
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        ChangeState(GameState.MainMenu);
    }

    public void StartNewRun()
    {
        currentScrap = 0;
        currentDepth = 1;
        score = 0;
        HasActiveRun = false;
        Time.timeScale = 1f;
        OnScrapChanged?.Invoke(currentScrap);
        OnDepthChanged?.Invoke(currentDepth);
        OnScoreChanged?.Invoke(score);
        ChangeState(GameState.CharacterSelect);
    }

    /// <summary>
    /// Thoát game an toàn hỗ trợ cả Unity Editor và Standalone Build
    /// </summary>
    public static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Đảm bảo luôn có MainMenuSystem và PauseMenu hoạt động đồng bộ
        if (GetComponent<MainMenuSystem>() == null) gameObject.AddComponent<MainMenuSystem>();
        if (GetComponent<PauseMenu>() == null) gameObject.AddComponent<PauseMenu>();
    }

    private void Start()
    {
        // Lắng nghe sự kiện chết của Player
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath += TriggerGameOver;
        }
    }

    public void AddScrap(int amount)
    {
        if (amount <= 0) return;
        currentScrap += amount;
        AddScore(amount * 10);
        OnScrapChanged?.Invoke(currentScrap);
    }

    public void AddScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);
    }

    public void AdvanceDepth()
    {
        currentDepth++;
        AddScore(100);
        OnDepthChanged?.Invoke(currentDepth);
        Debug.Log($"[GameManager] Tiến vào Tầng {currentDepth} của Vault!");
    }

    public void TriggerGameOver()
    {
        if (CurrentState == GameState.GameOver) return;
        HasActiveRun = false;
        ChangeState(GameState.GameOver);

        // Lưu điểm kỷ lục và tích luỹ Scrap vĩnh viễn (Meta-progression)
        int bestScore = PlayerPrefs.GetInt(PREF_HIGH_SCORE, 0);
        if (score > bestScore)
        {
            PlayerPrefs.SetInt(PREF_HIGH_SCORE, score);
            Debug.Log($"[GameManager] KỶ LỤC MỚI: {score} điểm!");
        }

        int totalSavedScrap = PlayerPrefs.GetInt(PREF_TOTAL_SCRAP, 0) + currentScrap;
        PlayerPrefs.SetInt(PREF_TOTAL_SCRAP, totalSavedScrap);
        PlayerPrefs.Save();

        Debug.Log($"[GameManager] GAME OVER! Tổng Scrap vĩnh viễn: {totalSavedScrap}");
        OnGameOver?.Invoke();
    }

    public void RestartCurrentRun()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void CompleteRun()
    {
        if (CurrentState == GameState.Victory)
            return;

        HasActiveRun = false;
        ChangeState(GameState.Victory);
    }
}

