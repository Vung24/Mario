using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private const string SelectedCharacterKey = "SelectedCharacterIndex";

    [Header("Spawn")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject[] characterPrefabs;
    [SerializeField] private Transform startCheckpoint;
    [SerializeField] private Transform winCheckpoint;
    [SerializeField] private CameraFollower cameraFollower;
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
        SpawnPlayer();
    }
    public void GameOver()
    {
        if (hasWon)
        {
            return;
        }

        Debug.Log("Game Over!");
    }

    public void GameWin()
    {
        // if (hasWon)
        // {
        //     return;
        // }

        // hasWon = true;
        Debug.Log("You Win!");
    }

    public void SetStartCheckpoint(Transform checkpoint)
    {
        startCheckpoint = checkpoint;
    }
    private void PlayerPosition()
    {
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            player = playerController.transform;
        }
    }
    private void SpawnPlayer()
    {
        if (startCheckpoint == null)
            return;

        SpawnSelectedCharacter();
        PlayerPosition();
        player.position = startCheckpoint.position;

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }
    }

    private void SpawnSelectedCharacter()
    {
        PlayerPosition();

        if (characterPrefabs == null || characterPrefabs.Length == 0)
        {
            return;
        }

        int selectedIndex = PlayerPrefs.GetInt(SelectedCharacterKey, CharacterSelect.selectedCharacterIndex);
        if (selectedIndex < 0 || selectedIndex >= characterPrefabs.Length)
        {
            selectedIndex = 0;
        }

        GameObject selectedPrefab = CharacterSelect.selectedCharacter != null
            ? CharacterSelect.selectedCharacter
            : characterPrefabs[selectedIndex];

        if (selectedPrefab == null)
        {
            return;
        }

        if (player != null)
        {
            Destroy(player.gameObject);
        }

        GameObject spawnedPlayer = Instantiate(selectedPrefab, startCheckpoint.position, Quaternion.identity);
        player = spawnedPlayer.transform;

        if (cameraFollower != null)
        {
            cameraFollower.SetFollowTarget(player);
        }
    }
}
