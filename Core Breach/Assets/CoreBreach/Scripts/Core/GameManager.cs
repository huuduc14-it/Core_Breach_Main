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

    public GameState CurrentState { get; private set; } = GameState.Playing;

    public event Action<int> OnScrapChanged;
    public event Action<int> OnScoreChanged;
    public event Action<int> OnDepthChanged;
    public event Action OnGameOver;

    private const string PREF_HIGH_SCORE = "CoreBreach_HighScore";
    private const string PREF_TOTAL_SCRAP = "CoreBreach_TotalScrap";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Lắng nghe sự kiện chết của Player
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
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
        CurrentState = GameState.GameOver;

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
}

