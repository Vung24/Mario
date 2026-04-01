using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    public GameObject[] levelPrefabs;
    public int currentLevelIndex = 0;
    private GameObject currentLevel;

    private Temp_LevelController curLevel;

    [SerializeField] private CameraFollower cameraFollower;

    public Temp_LevelController[] levelControllers;


    [Header("Player Settings")]
    public GameObject player;

    void Start()
    {
        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelPrefabs.Length)
        {
            return;
        }
        if (currentLevel != null)
        {
            Destroy(currentLevel);
        }

        // currentLevel = Instantiate(levelPrefabs[levelIndex], new Vector3() , Quaternion.identity);

        curLevel = Instantiate(levelControllers[levelIndex], new Vector3(), Quaternion.identity);

        //Transform spawnPoint = currentLevel.transform.Find("Checkpoint/Start");

        Transform spawnPoint = curLevel.startPos;

        GameManager.Instance.startCheckpoint = spawnPoint;
        GameManager.Instance.SpawnPlayer();
        
        // player = spawnedPlayer.transform;

        // if (cameraFollower != null)
        // {
        //     cameraFollower.SetFollowTarget(player.transform);
        // }

        /*if (spawnPoint != null && player != null)
        {
            player.transform.position = spawnPoint.position;
        }
        else if (player != null)
        {
            player.transform.position = Vector3.zero; 
        }*/
    }

    public void NextLevel()
    {
        currentLevelIndex++;

        if (currentLevelIndex < levelPrefabs.Length)
        {
            LoadLevel(currentLevelIndex);
        }
    }
}