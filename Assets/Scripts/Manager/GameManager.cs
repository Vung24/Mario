using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private const string SelectedCharacterKey = "SelectedCharacterIndex";

    [Header("Spawn")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject[] characterPrefabs;
    [SerializeField] public Transform startCheckpoint;
    [SerializeField] private Transform winCheckpoint;
    [Header("Camera")]
    [SerializeField] private CameraFollower cameraFollower;
    [Header("Game UI")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject gameOverPanel;
    // private bool isGameOver = false;
    private bool hasWon = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

    }

    // Start is called before the first frame update
    void Start()
    {
        winPanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void ResetLevelUIState()
    {
        hasWon = false;
        player = null;
        startCheckpoint = null;
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void GameOver()
    {
        if (hasWon)
        {
            return;
        }
        if (gameOverPanel != null)
        {
            StartCoroutine(ShowGameOver());
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }
    public void GameWin()
    {
        if (hasWon)
        {
            return;
        }

        hasWon = true;
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }
    private IEnumerator ShowGameOver()
    {
        yield return new WaitForSeconds(1f);
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
    public void Home()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Home");
    }
    public void SetStartCheckpoint(Transform checkpoint)
    {
        startCheckpoint = checkpoint;
    }
    public void SpawnPlayer()
    {
        if (startCheckpoint == null){
            return;
        }

        SpawnPlayerAt(startCheckpoint.position);
    }

    public void SpawnPlayerAt(Vector3 spawnPosition)
    {
        SpawnSelectedCharacter(spawnPosition);

        if (player == null)
        {
            return;
        }

        player.position = spawnPosition;

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }
    }

    private void SpawnSelectedCharacter(Vector3 spawnPosition)
    {
        int selectedIndex = PlayerPrefs.GetInt(SelectedCharacterKey, CharacterSelect.selectedCharacterIndex);
        if (selectedIndex < 0 || selectedIndex >= characterPrefabs.Length)
        {
            selectedIndex = 0;
        }

        GameObject selectedPrefab = characterPrefabs[selectedIndex];

        PlayerController[] existingPlayers = FindObjectsOfType<PlayerController>();
        foreach (PlayerController existingPlayer in existingPlayers)
        {
            Destroy(existingPlayer.gameObject);
        }
        player = null;
        
        GameObject spawnedPlayer = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

        player = spawnedPlayer.transform;
        
        if (cameraFollower != null)
        {
            cameraFollower.SetFollowTarget(player);
        }
    }
}
