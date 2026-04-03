using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class LevelSelection : MonoBehaviour
{
    private const string CurrentLevelKey = "CurrentLevelIndex";

    [SerializeField] private string gameplaySceneName = "MainScene";
    [SerializeField] private Transform levelButtonsRoot;
    private bool autoBindLevelButtons = true;

    private void Start()
    {
        if (autoBindLevelButtons)
        {
            BindLevelButtons();
        }
    }

    public void SelectLevel(int levelIndex)
    {
        if (levelIndex < 0)
        {
            return;
        }

        PlayerPrefs.SetInt(CurrentLevelKey, levelIndex);
        PlayerPrefs.Save();
        SceneManager.LoadScene(gameplaySceneName);
    }

    private void BindLevelButtons()
    {
        Transform root = levelButtonsRoot != null ? levelButtonsRoot : transform;
        Button[] buttons = root.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            int levelIndex = GetLevelIndexFromButtonLabel(button);
            if (levelIndex < 0)
            {
                continue;
            }

            int capturedLevelIndex = levelIndex;
            button.onClick.AddListener(() => SelectLevel(capturedLevelIndex));
        }
    }

    private int GetLevelIndexFromButtonLabel(Button button)
    {
        if (button == null)
        {
            return -1;
        }

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label == null || string.IsNullOrWhiteSpace(label.text))
        {
            return -1;
        }

        Match match = Regex.Match(label.text, "\\d+");
        if (!match.Success)
        {
            return -1;
        }

        if (!int.TryParse(match.Value, out int levelNumber) || levelNumber <= 0)
        {
            return -1;
        }

        return levelNumber - 1;
    }
}
