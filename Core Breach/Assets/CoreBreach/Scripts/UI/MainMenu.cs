using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main Menu. Nút Play gọi PlayGame(), nút Quit gọi QuitGame().
/// Scene Menu và scene game phải có trong File > Build Profiles (Build Settings).
/// Có thể tạo nhanh bằng menu: Core Breach > Member B > 2. Create Main Menu Scene.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "CoreBreach_Main";
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text totalScrapText;

    // Trùng với khóa PlayerPrefs của GameManager
    private const string PREF_HIGH_SCORE = "CoreBreach_HighScore";
    private const string PREF_TOTAL_SCRAP = "CoreBreach_TotalScrap";

    private void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (highScoreText != null)
            highScoreText.text = "HIGH SCORE: " + PlayerPrefs.GetInt(PREF_HIGH_SCORE, 0);

        if (totalScrapText != null)
            totalScrapText.text = "TOTAL SCRAP: " + PlayerPrefs.GetInt(PREF_TOTAL_SCRAP, 0);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
