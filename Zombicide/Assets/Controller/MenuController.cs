using Persistence;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject mainMenuCanvas;
    public GameObject hostGameCanvas;

    public TMP_Dropdown mapDropdown;
    public Image mapImage;
    public TMP_Text mapObjectives;
    public TMP_Text mapDifficulty;
    public TMP_Text mapRules;

    public TMP_Dropdown characterDropdown;
    public Image characterImage;

    public TMP_InputField playerCountInput;
    private List<MapData> maps;
    private List<CharacterData> characters;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowHostGame()
    {
        mainMenuCanvas.SetActive(false);
        hostGameCanvas.SetActive(true);

        maps = FileManager.Instance.LoadMaps();
        characters = FileManager.Instance.LoadCharacters();

        PopulateMapDropdown();
        PopulateCharacterDropdown();
    }
    public void ShowMainMenu()
    {
        mainMenuCanvas.SetActive(true);
        hostGameCanvas.SetActive(false);
    }
    public void Quit()
    {
        Application.Quit();
    }

    void PopulateMapDropdown()
    {
        mapDropdown.ClearOptions();
        List<string> mapNames = new List<string>();

        foreach (var map in maps)
        {
            mapNames.Add(map.name);
        }

        mapDropdown.AddOptions(mapNames);
        mapDropdown.onValueChanged.AddListener(delegate { UpdateMapDetails(mapDropdown.value); });

        UpdateMapDetails(0); // Alapértelmezett kiválasztott pálya
    }

    void UpdateMapDetails(int index)
    {
        mapImage.sprite = Resources.Load<Sprite>("Maps/" + maps[index].image);
        mapObjectives.text = maps[index].objectives;
        mapDifficulty.text = maps[index].difficulty;
        mapRules.text = maps[index].rules;
    }

    void PopulateCharacterDropdown()
    {
        characterDropdown.ClearOptions();
        List<string> characterNames = new List<string>();

        foreach (var character in characters)
        {
            characterNames.Add(character.name);
        }

        characterDropdown.AddOptions(characterNames);
        characterDropdown.onValueChanged.AddListener(delegate { UpdateCharacterDetails(characterDropdown.value); });

        UpdateCharacterDetails(0); // Alapértelmezett kiválasztott karakter
    }

    void UpdateCharacterDetails(int index)
    {
        characterImage.sprite = Resources.Load<Sprite>("Characters/" + characters[index].image);
    }

    public void OnOkButtonPressed()
    {
        int playerCount = int.Parse(playerCountInput.text);
        string selectedMap = maps[mapDropdown.value].name;
        string selectedCharacter = characters[characterDropdown.value].name;

        Debug.Log($"Host létrehozva! Pálya: {selectedMap}, Játékosok száma: {playerCount}, Karakter: {selectedCharacter}");
        // Itt fogjuk majd elindítani a szervert a következõ lépésben!
    }

    public void OnCancelButtonPressed()
    {
        mainMenuCanvas.SetActive(true);
        hostGameCanvas.SetActive(false);
    }
}
