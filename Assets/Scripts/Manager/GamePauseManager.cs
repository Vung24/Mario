using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GamePauseManager : MonoBehaviour
{
    [SerializeField] private Button pauseButton, menuButton, restartButton, closeButton, soundButton;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Image soundToggleImage;
    [SerializeField] private Sprite spriteSoundOn;
    [SerializeField] private Sprite spriteSoundOff;
    private bool localSoundOn = true;

    void Awake()
    {
        if (soundToggleImage == null && soundButton != null)
        {
            soundToggleImage = soundButton.image;
        }
    }

    void Start()
    {
        pausePanel.transform.localScale = Vector3.zero;
        pausePanel.SetActive(false);

        if (AudioManager.Instance != null)
        {
            localSoundOn = AudioManager.Instance.IsSoundOn();
        }

        UpdateSoundIcon();
    }

    void OnEnable()
    {
        UpdateSoundIcon();
    }
    public void OpenPausePanel()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            pausePanel.transform.localScale = Vector3.one;  
            Time.timeScale = 0f;
        }

    }
    public void ClosePausePanel()
    {
        if (pausePanel != null)
        {
            pausePanel.transform.localScale = Vector3.zero;
            pausePanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void OnHomeButtonPressed()
    {
        Time.timeScale = 1f;  
        SceneManager.LoadScene("Home");
    }

    public void OnRestartButtonPressed()
    {
        Time.timeScale = 1f;  
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void ToggleSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleSound();
            localSoundOn = AudioManager.Instance.IsSoundOn();
        }
        else
        {
            localSoundOn = !localSoundOn;
        }

        UpdateSoundIcon();
    }

    public void UpdateSoundIcon()
    {
        if (soundToggleImage == null)
        {
            return;
        }

        bool isSoundOn = AudioManager.Instance != null ? AudioManager.Instance.IsSoundOn() : localSoundOn;
        soundToggleImage.sprite = isSoundOn ? spriteSoundOn : spriteSoundOff;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            OpenPausePanel();
        }
    }
}

