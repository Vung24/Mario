using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button PlayButton;
    [SerializeField] private Button ExitButton;
    [SerializeField] private TextMeshProUGUI playButtonText;
    private int currentLevel;
    public void PlayButtonClicked()
    {
        SceneManager.LoadScene("CharacterSelection");
    }

    public void ExitButtonClicked()
    {
        Application.Quit();
    }
    public void Start()
    {
        UpdateUI();
    }
    private void UpdateUI()
    {

        currentLevel = PlayerPrefs.GetInt("CurrentLevelIndex", 0);
        playButtonText.text = $"Play Level {currentLevel + 1}";
    }
}
