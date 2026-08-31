using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public GameObject SettingsPanel;
    public GameObject quitPanel;
    public GameObject statPanel;
    public TextMeshProUGUI levelProgressText;
    public TextMeshProUGUI totalAttemptsText;

    private void Start()
    {
        if (SettingsPanel != null) SettingsPanel.SetActive(false);
        if (quitPanel != null) quitPanel.SetActive(false);
        if (statPanel != null) statPanel.SetActive(false);
    }

    public void Play()
    {
        SFX.Instance?.PlayStartOrQuit();

        if (PlayerPrefs.GetInt("HasCompletedTutorial", 0) == 0)
        {
            PlayerPrefs.SetInt("SelectedLevel", 0);
            PlayerPrefs.SetInt("HasCompletedTutorial", 1);
            PlayerPrefs.Save();
            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            SceneManager.LoadScene("LevelSelect");
        }
    }

    public void Settings()
    {
        SFX.Instance?.PlayButtonClick();
        if (SettingsPanel != null)
        {
            SettingsPanel.SetActive(true);
            SFX.Instance?.InitializeUI();

        }
    }

    public void SettingsClose()
    {
        SFX.Instance?.PlayButtonClick();
        SettingsPanel.SetActive(false);
    }

    public void OpenStats()
    {
        SFX.Instance?.PlayButtonClick();
        UpdateStatsDisplay();
        if (statPanel != null) statPanel.SetActive(true);
    }

    public void CloseStats()
    {
        SFX.Instance?.PlayButtonClick();
        if (statPanel != null) statPanel.SetActive(false);
    }

    public void UpdateStatsDisplay()
    {
        int totalAttempts = PlayerPrefs.GetInt("TotalAttempts", 0);
        int completedCount = 0;

        for (int i = 1; i <= 20; i++)
        {
            if (PlayerPrefs.GetInt($"Level_{i}_Completed", 0) == 1)
            {
                completedCount++;
            }
        }
        Color targetColor = (completedCount >= 20) ? Color.yellow : Color.white;

        if (levelProgressText != null)
        {
            levelProgressText.text = $"{completedCount}/20";
            levelProgressText.color = targetColor;
        }

        if (totalAttemptsText != null)
        {
            totalAttemptsText.text = totalAttempts.ToString();
            totalAttemptsText.color = targetColor;
        }
    }

    public void ResetAllData()
    {
        SFX.Instance?.PlayLoss();
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        UpdateStatsDisplay();
    }

    public void Quit()
    {
        SFX.Instance?.PlayStartOrQuit();

#if UNITY_WEBGL
        Screen.fullScreen = false;
        if (quitPanel != null) quitPanel.SetActive(true);
#else
        Application.Quit();
#endif
    }
}