using UnityEngine;

public static class Progress
{
    private const string LEVEL_COMPLETED_KEY = "Level_Completed_";

    public static void MarkLevelCompleted(int levelIndex)
    {
        PlayerPrefs.SetInt(LEVEL_COMPLETED_KEY + levelIndex, 1);
        PlayerPrefs.Save();
    }

    public static bool IsLevelCompleted(int levelIndex)
    {
        return PlayerPrefs.GetInt(LEVEL_COMPLETED_KEY + levelIndex, 0) == 1;
    }

    public static bool AreAllBaseLevelsCompleted()
    {
        for (int i = 0; i < 19; i++)
        {
            if (!IsLevelCompleted(i)) return false;
        }
        return true;
    }

    public static void ResetAllProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}