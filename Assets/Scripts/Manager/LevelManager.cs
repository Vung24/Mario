using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    private const string CurrentLevelKey = "CurrentLevelIndex";

    [Header("Level Settings")]
    public GameObject[] levelPrefabs;
    public int currentLevelIndex = 0;
    private GameObject currentLevel;

    private Temp_LevelController curLevel;
    private Coroutine loadLevelRoutine;

    public Temp_LevelController[] levelControllers;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
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

        if (loadLevelRoutine != null)
        {
            StopCoroutine(loadLevelRoutine);
            loadLevelRoutine = null;
        }

        loadLevelRoutine = StartCoroutine(LoadLevelRoutine(levelIndex));
    }

    private System.Collections.IEnumerator LoadLevelRoutine(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        PlayerPrefs.SetInt(CurrentLevelKey, currentLevelIndex);
        PlayerPrefs.Save();

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("GameManager instance is missing.");
            yield break;
        }

        gameManager.ResetLevelUIState();

        if (currentLevel != null)
        {
            Destroy(currentLevel);
            currentLevel = null;
            curLevel = null;

            yield return null;
        }

        if (!InstantiateLevel(levelIndex))
        {
            Debug.LogError($"Failed to instantiate level {levelIndex}");
            yield break;
        }

        yield return null;

        Transform resolvedStartPos = curLevel != null ? curLevel.ResolveStartPos() : null;
        
        // Fallback: if controller didn't resolve, search the whole level
        if (resolvedStartPos == null && currentLevel != null)
        {
            Transform[] allTransforms = currentLevel.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allTransforms)
            {
                if (t.name == "Start" || t.name == "CheckPoint" || t.name == "Checkpoint")
                {
                    resolvedStartPos = t;
                    break;
                }
            }
        }
        
        if (resolvedStartPos == null)
        {
            if (currentLevel != null)
            {
                resolvedStartPos = currentLevel.transform;
            }
            else
            {
                yield break;
            }
        }

        gameManager.SetStartCheckpoint(resolvedStartPos);
        gameManager.SpawnPlayerAt(resolvedStartPos.position);
        loadLevelRoutine = null;
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