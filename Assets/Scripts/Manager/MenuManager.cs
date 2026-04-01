using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Button PlayButton;
    [SerializeField] private Button ExitButton;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void PlayButtonClicked()
    {
        SceneManager.LoadScene("CharacterSelection");
    }

    public void ExitButtonClicked()
    {
        Application.Quit();
    }
}
