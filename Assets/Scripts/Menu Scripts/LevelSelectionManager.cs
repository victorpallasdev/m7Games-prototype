using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelSelectionManager : MonoBehaviour
{
    [SerializeField] public LevelLibrary levelLibrary;
    [SerializeField] private List<LevelData> unlockedLevels = new List<LevelData>();

    private LevelData currentLevel;
    public TextMeshProUGUI selectedLevelNameText;
    public SpriteRenderer selectedLevelSprite;
    public CharacterSelectionManager characterSelectionManager;
    public Transform charsPanel;
    public Button selectButton;
    public Button backButton;
    public Button leftArrow;
    public Image leftArrowImage;
    private Material leftArrowImageMaterial;
    public Button rightArrow;
    public Image rightArrowImage;
    private Material rightArrowImageMaterial;
    public Transform levelSelectionPanel;
    public GolemSelectionManager golemSelectionManager;
    public MusicManager musicManager;
    private int currentIndex;

    void Awake()
    {
        // 2) Transfiere la lista de IDs al manejador
        LoadLevelsList(SaveSystem.progress.unlockedLevelIds);
    }
    public void StartSelection()
    {
        currentIndex = 0;
        LoadLevelIndex(currentIndex);
        leftArrowImageMaterial = leftArrowImage.material;
        leftArrowImage.material = new Material(leftArrowImageMaterial);
        rightArrowImageMaterial = rightArrowImage.material;
        rightArrowImage.material = new Material(rightArrowImageMaterial);
        CheckArrowButtons();
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(SelectLevelButton);
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

        if (currentIndex == unlockedLevels.Count - 1)
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
    private void LoadLevelIndex(int i)
    {
        currentLevel = unlockedLevels[i];
        selectedLevelNameText.text = currentLevel.levelName.ToUpper();
        selectedLevelSprite.sprite = currentLevel.levelSprite;
    }

    public void LoadLevelsList(List<string> ids)
    {
        levelLibrary.Initialize();

        unlockedLevels = new List<LevelData>();
        foreach (string id in ids)
        {
            LevelData level = levelLibrary.GetById(id);
            if (level != null) unlockedLevels.Add(level);
            else Debug.LogWarning($"No existe Dwarf con ID '{id}'");
        }
    }

    private void SelectLevelButton()
    {
        musicManager.PlayStandardClickSound();
        GameSession.Instance.SelectedLevel = currentLevel;
        //GameSession.Instance.levelMusic = currentLevel.musicClips;
        golemSelectionManager.gameObject.SetActive(true);
        golemSelectionManager.StartSelection();
        levelSelectionPanel.gameObject.SetActive(false);
    }

    private void GoBack()
    {
        musicManager.PlayErrorClickSound();
        charsPanel.gameObject.SetActive(true);
        SaveSystem.Load();
        characterSelectionManager.StartSelection();
        gameObject.SetActive(false);
    }
}
