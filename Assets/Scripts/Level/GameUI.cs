using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public TextMeshProUGUI levelNumberText;
    public TextMeshProUGUI levelTitleText;
    public Image levelCircleImage;
    public GameObject pause;
    public GameObject pauseMenuPanel;
    public GameObject winPanel;
    public List<Button> pauseMenuButtons = new List<Button>();
    public RectTransform pauseArrowPointer;
    public float arrowXOffset = -50f;

    private bool isPaused = false;
    private int currentPauseIndex = 0;

    private void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isPaused || pauseMenuButtons == null || pauseMenuButtons.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            currentPauseIndex--;
            if (currentPauseIndex < 0) currentPauseIndex = pauseMenuButtons.Count - 1;
            UpdatePauseArrowPosition();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            currentPauseIndex++;
            if (currentPauseIndex >= pauseMenuButtons.Count) currentPauseIndex = 0;
            UpdatePauseArrowPosition();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
        {
            if (pauseMenuButtons[currentPauseIndex] != null && pauseMenuButtons[currentPauseIndex].interactable)
            {
                pauseMenuButtons[currentPauseIndex].onClick.Invoke();
            }
        }
    }

    public void SetupLevelHeader(LevelDataSO levelData)
    {
        if (levelData == null) return;

        int currentLevelIndex = PlayerPrefs.GetInt("SelectedLevel", 1);
        bool isActualTutorial = levelData.isTutorial && currentLevelIndex == 0;

        if (levelNumberText != null)
        {
            levelNumberText.text = isActualTutorial ? "TUTORIAL" : $"Level {currentLevelIndex}";
        }

        if (levelTitleText != null)
        {
            levelTitleText.text = isActualTutorial ? "" : levelData.levelName;
        }

        if (levelCircleImage != null)
        {
            levelCircleImage.color = levelData.levelThemeColor;
        }
    }

    public void TogglePause()
    {
        if (winPanel != null && winPanel.activeSelf) return;

        SFX.Instance?.PlayButtonClick();
        isPaused = !isPaused;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPaused);

            if (isPaused)
            {
                SFX.Instance?.PlayLevelSelect();
                currentPauseIndex = 0;
                UpdatePauseArrowPosition();
            }
        }

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        SFX.Instance?.PlayButtonClick();
        isPaused = false;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ShowWinUI()
    {
        if (pause != null) pause.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(true);

        SFX.Instance?.PlayLevelSelect();
        Time.timeScale = 0f;
    }

    private void UpdatePauseArrowPosition()
    {
        if (pauseArrowPointer == null || pauseMenuButtons.Count == 0) return;

        Button selectedButton = pauseMenuButtons[currentPauseIndex];
        if (selectedButton == null) return;

        RectTransform buttonRect = selectedButton.GetComponent<RectTransform>();

        Vector3 targetPosition = buttonRect.position;
        targetPosition.x += arrowXOffset;

        pauseArrowPointer.position = targetPosition;
    }
}