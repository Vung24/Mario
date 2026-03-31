using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GamePauseManager : MonoBehaviour
{
    [SerializeField] private Button pauseButton, menuButton, restartButton, closeButton;
    [SerializeField] private GameObject pausePanel;

    // Start is called before the first frame update
    void Start()
    {
        pausePanel.transform.localScale = Vector3.zero;
        pausePanel.SetActive(false);
    }
    public void OpenPausePanel()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f; 
            pausePanel.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
        }
    }
    public void ClosePausePanel()
    {
        if (pausePanel != null)
        {
            Time.timeScale = 1f; 
            pausePanel.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack)
                .OnComplete(() => pausePanel.SetActive(false));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
