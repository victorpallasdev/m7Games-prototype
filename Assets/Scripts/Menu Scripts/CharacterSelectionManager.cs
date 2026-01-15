using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterSelectionManager : MonoBehaviour
{
    [SerializeField] public CharacterLibrary characterLibrary;
    [SerializeField] private List<CharacterData> unlockedCharacters = new List<CharacterData>();
    private CharacterData currentChar;
    public Transform charSelectionPanel;
    public Transform startMenuPanel;
    public Transform startMenuButtonsContainer;
    public LevelSelectionManager levelSelectionManager;
    public DisplayDeckManager displayDeckManager;
    public TextMeshProUGUI selectedCharNameText;
    public TextMeshProUGUI selectedCharDescriptionText;
    public SpriteRenderer selectedCharSprite;
    public Animator animator;
    public Button selectButton;
    public Button backButton;
    public Button leftArrow;
    public Image leftArrowImage;
    public MusicManager musicManager;
    private Material leftArrowImageMaterial;
    public Button rightArrow;
    public Image rightArrowImage;
    private Material rightArrowImageMaterial;
    private int currentIndex;


    void Awake()
    {
        // 1) Carga o crea el JSON con defaults
        SaveSystem.Init();
        // 2) Transfiere la lista de IDs al manejador
        LoadDwarfsList(SaveSystem.progress.unlockedCharactersIds);
        
    }
    public void StartSelection()
    {
        currentIndex = 0;
        LoadDwarfIndex(currentIndex);
        leftArrowImageMaterial = leftArrowImage.material;
        leftArrowImage.material = new Material(leftArrowImageMaterial);
        rightArrowImageMaterial = rightArrowImage.material;
        rightArrowImage.material = new Material(rightArrowImageMaterial);
        CheckArrowButtons();
        rightArrow.onClick.RemoveAllListeners();
        rightArrow.onClick.AddListener(NextCharacter);
        leftArrow.onClick.RemoveAllListeners();
        leftArrow.onClick.AddListener(PreviousCharacter);
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(SelectCharacterButton);
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(GoBack);
    }
    private void CheckArrowButtons()
    {
        if (currentIndex == 0)
        {
            ImageToBW(leftArrowImage);
            leftArrow.interactable = false;
        }
        else
        {
            ImageToColor(leftArrowImage);
            leftArrow.interactable = true;
        }

        if (currentIndex == unlockedCharacters.Count - 1)
        {
            ImageToBW(rightArrowImage);
            rightArrow.interactable = false;
        }
        else
        {
            ImageToColor(rightArrowImage);
            rightArrow.interactable = true;
        }
    }

    private void ImageToBW(Image arrow)
    {
        Material mat = arrow.material;
        mat.EnableKeyword("_ENABLESATURATION_ON");
        mat.SetFloat("_EnableSaturation", 0f);
    }
    private void ImageToColor(Image arrow)
    {
        Material mat = arrow.material;
        mat.SetFloat("_EnableSaturation", 1f);
        mat.DisableKeyword("_ENABLESATURATION_ON");
    }
    private void NextCharacter()
    {
        musicManager.PlayStandardClickSound();
        currentIndex++;
        LoadDwarfIndex(currentIndex);
        CheckArrowButtons();
    }
    private void PreviousCharacter()
    {
        musicManager.PlayStandardClickSound();
        Debug.Log("Detectado left arrow");
        currentIndex--;
        LoadDwarfIndex(currentIndex);
        CheckArrowButtons();
    }
    private void LoadDwarfIndex(int i)
    {
        currentChar = unlockedCharacters[i];
        selectedCharNameText.text = currentChar.characterName.ToUpper();
        selectedCharDescriptionText.text = currentChar.description;
        selectedCharSprite.sprite = currentChar.characterSprite;
        animator.runtimeAnimatorController = currentChar.animatorController;
        displayDeckManager.Initialize(currentChar.deck);
    }
    public void LoadDwarfsList(List<string> characterIds)
    {
        characterLibrary.Initialize();

        unlockedCharacters = new List<CharacterData>();
        foreach (string id in characterIds)
        {
            CharacterData character = characterLibrary.GetById(id);
            if (character != null) unlockedCharacters.Add(character);
            else Debug.LogWarning($"No existe Dwarf con ID '{id}'");
        }
    }

    private void SelectCharacterButton()
    {
        musicManager.PlayStandardClickSound();
        GameSession.Instance.SelectedChar = currentChar;
        levelSelectionManager.gameObject.SetActive(true);
        levelSelectionManager.StartSelection();
        charSelectionPanel.gameObject.SetActive(false);
    }

    private void GoBack()
    {
        musicManager.PlayErrorClickSound();
        startMenuPanel.gameObject.SetActive(true);
        startMenuButtonsContainer.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

}
