using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    public GameObject page1;
    public GameObject page2;
    public GameObject prevButton;
    public GameObject nextButton;

    public Button[] levelButtons;

    public GameObject star;
    private string sampleScene = "SampleScene";
    private string mainMenuScene = "MainMenu";

    public Button level20Button;
    public Image level20LockIcon;
    public GameObject lockedPopupPanel;
    public GameObject gameCompletionPanel;

    private void Start()
    {
        PlayerPrefs.Save();

        if (lockedPopupPanel != null)
            lockedPopupPanel.SetActive(false);

        CheckGameCompletion();

        SetupLevel20();
        RefreshAllStars();
        ShowPage1();
    }

    private void CheckGameCompletion()
    {
        bool isLevel20Done = Progress.IsLevelCompleted(20) || PlayerPrefs.GetInt("Level_20_Completed", 0) == 1;
        bool hasShownPopup = PlayerPrefs.GetInt("HasSeenCompletion", 0) == 1;

        if (isLevel20Done && !hasShownPopup)
        {
            if (gameCompletionPanel != null)
            {
                gameCompletionPanel.SetActive(true);
            }

            PlayerPrefs.SetInt("HasSeenCompletion", 1);
            PlayerPrefs.Save();
        }
        else
        {
            if (gameCompletionPanel != null)
            {
                gameCompletionPanel.SetActive(false);
            }
        }
    }

    public void CloseGameCompletion()
    {
        SFX.Instance?.PlayButtonClick();
        if (gameCompletionPanel != null)
        {
            gameCompletionPanel.SetActive(false);
        }
    }

    public void ShowPage1()
    {
        SFX.Instance?.PlayButtonClick();

        if (page1 != null) page1.SetActive(true);
        if (page2 != null) page2.SetActive(false);

        if (prevButton != null) prevButton.SetActive(false);
        if (nextButton != null) nextButton.SetActive(true);

        if (level20LockIcon != null)
            level20LockIcon.gameObject.SetActive(false);

        RefreshAllStars();
    }

    public void ShowPage2()
    {
        SFX.Instance?.PlayButtonClick();

        if (page1 != null) page1.SetActive(false);
        if (page2 != null) page2.SetActive(true);

        if (prevButton != null) prevButton.SetActive(true);
        if (nextButton != null) nextButton.SetActive(false);

        if (level20LockIcon != null)
            level20LockIcon.gameObject.SetActive(!AllLevelsComplete());

        RefreshAllStars();
    }

    private void SetupLevel20()
    {
        if (level20Button != null)
        {
            level20Button.onClick.RemoveAllListeners();

            if (AllLevelsComplete())
            {
                level20Button.onClick.AddListener(() => SelectLevel(20));
            }
            else
            {
                level20Button.onClick.AddListener(ShowLockedPopup);
            }
        }
    }

    private bool AllLevelsComplete()
    {
        for (int i = 1; i <= 19; i++)
        {
            if (Progress.IsLevelCompleted(i) == false && PlayerPrefs.GetInt($"Level_{i}_Completed", 0) == 0)
            {
                return false;
            }
        }
        return true;
    }

    public void SelectLevel(int levelIndex)
    {
        if (levelIndex == 20 && !AllLevelsComplete())
        {
            ShowLockedPopup();
            return;
        }

        SFX.Instance?.PlayLevelSelect();
        PlayerPrefs.SetInt("SelectedLevel", levelIndex);
        PlayerPrefs.Save();
        SceneManager.LoadScene(sampleScene);
    }

    private void RefreshAllStars()
    {
        if (star == null || levelButtons == null) return;

        for (int i = 0; i < levelButtons.Length; i++)
        {
            Button btn = levelButtons[i];
            if (btn == null) continue;

            int boardLevelIndex = i + 1;

            bool isCompleted = Progress.IsLevelCompleted(boardLevelIndex) ||
                               PlayerPrefs.GetInt($"Level_{boardLevelIndex}_Completed", 0) == 1;

            Transform existingStar = btn.transform.Find("Star");

            if (isCompleted)
            {
                if (existingStar == null)
                {
                    GameObject newStar = Instantiate(star, btn.transform);
                    newStar.name = "Star";

                    RectTransform rect = newStar.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchorMin = new Vector2(1, 1);
                        rect.anchorMax = new Vector2(1, 1);
                        rect.pivot = new Vector2(1, 1);
                        rect.anchoredPosition = new Vector2(10f, 10f);
                    }
                }

                if (btn.transform.Find("Star") != null)
                {
                    btn.transform.Find("Star").gameObject.SetActive(true);
                }
            }
            else if (existingStar != null)
            {
                existingStar.gameObject.SetActive(false);
            }
        }
    }

    public void ShowLockedPopup()
    {
        SFX.Instance?.PlayLoss();
        if (lockedPopupPanel != null)
            lockedPopupPanel.SetActive(true);
    }

    public void HideLockedPopup()
    {
        SFX.Instance?.PlayButtonClick();
        if (lockedPopupPanel != null)
            lockedPopupPanel.SetActive(false);
    }

    public void GoToMainMenu()
    {
        SFX.Instance?.PlayButtonClick();
        SceneManager.LoadScene(mainMenuScene);
    }
}