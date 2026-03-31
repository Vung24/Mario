using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelect : MonoBehaviour
{
    private int index;
    private const string SelectedCharacterKey = "SelectedCharacterIndex";
    [SerializeField] private GameObject[] characters;
    [SerializeField] private GameObject[] characterPrefabs;
    [SerializeField] private TextMeshProUGUI characterName;
    public static GameObject selectedCharacter;
    public static int selectedCharacterIndex;
    // Start is called before the first frame update
    void Start()
    {
        index = 0;
        SelectCharacter(); 
    }
    public void OnStartBtnClick()
    {
        PlayerPrefs.SetInt(SelectedCharacterKey, selectedCharacterIndex);
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainScene");
    }
    public void OnPrewBtnClick()
    {
        if (index > 0)
        {
            index--;
        }
        SelectCharacter();
    }
    public void OnNextBtnClick()
    {
        if (index < characters.Length - 1)
        {
            index++;
        }
        SelectCharacter();
    }
    private void SelectCharacter()
    {
        for (int i = 0; i < characters.Length; i++)
        {
            if(i == index)
            {
                characters[i].GetComponent<SpriteRenderer>().color = Color.white;
                characters[i].GetComponent<Animator>().enabled = true;
                selectedCharacter = characterPrefabs[i];
                selectedCharacterIndex = i;
                characterName.text = characterPrefabs[i].name;
            }
            else
            {
                characters[i].GetComponent<SpriteRenderer>().color = Color.black;
                characters[i].GetComponent<Animator>().enabled = false;
            }
        }
    }

}
