using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private const string CurrentLevelKey = "CurrentLevelIndex";

    [Header("Level Settings")]
    public GameObject[] levelPrefabs;
    public int currentLevelIndex = 0;
    private GameObject currentLevel;

    private Temp_LevelController curLevel;

    public Temp_LevelController[] levelControllers;

    void Start()
    {
        int availableLevelCount = GetAvailableLevelCount();
        if (availableLevelCount > 0)
        {
            currentLevelIndex = Mathf.Clamp(
                PlayerPrefs.GetInt(CurrentLevelKey, currentLevelIndex),
                0,
                availableLevelCount - 1
            );
        }

        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int levelIndex)
    {
        int availableLevelCount = GetAvailableLevelCount();
        if (levelIndex < 0 || levelIndex >= availableLevelCount)
        {
            return;
        }

        currentLevelIndex = levelIndex;
        PlayerPrefs.SetInt(CurrentLevelKey, currentLevelIndex);
        PlayerPrefs.Save();

        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.ResetLevelUIState();
        }

        if (currentLevel != null)
        {
            Destroy(currentLevel);
        }

        if (!InstantiateLevel(levelIndex))
        {
            return;
        }

        if (curLevel?.startPos == null || gameManager == null)
        {
            return;
        }

        gameManager.startCheckpoint = curLevel.startPos;
        gameManager.SpawnPlayer();
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        int nextLevelIndex = currentLevelIndex + 1;

        if (nextLevelIndex < GetAvailableLevelCount())
        {
            LoadLevel(nextLevelIndex);
        }
    }

    private int GetAvailableLevelCount()
    {
        if (levelControllers != null && levelControllers.Length > 0) return levelControllers.Length;
        return levelPrefabs?.Length ?? 0;
    }

    private bool InstantiateLevel(int levelIndex)
    {
        if (levelControllers != null && levelControllers.Length > levelIndex)
        {
            curLevel = Instantiate(levelControllers[levelIndex], Vector3.zero, Quaternion.identity);
            currentLevel = curLevel.gameObject;
            return true;
        }

        if (levelPrefabs != null && levelPrefabs.Length > levelIndex)
        {
            currentLevel = Instantiate(levelPrefabs[levelIndex], Vector3.zero, Quaternion.identity);
            curLevel = currentLevel.GetComponent<Temp_LevelController>();
            return currentLevel != null;
        }

        curLevel = null;
        currentLevel = null;
        return false;
    }
}