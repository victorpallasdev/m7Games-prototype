using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GolemSelectionManager : MonoBehaviour
{
    [Header("Data Libraries")]
    [SerializeField] private GolemLibrary golemLibrary;

    [Header("UI References")]
    public TextMeshProUGUI selectedGolemNameText;
    public SpriteRenderer selectedGolemSprite;
    public Button selectButton;
    public Button leftArrow;
    public Image leftArrowImage;
    public Button rightArrow;
    public Image rightArrowImage;
    public Transform golemSelectionPanel;
    public Transform loadingPanel;
    public LoadingScreenManager loadingManager;
    public LevelSelectionManager levelSelectionManager;
    public Transform levelSelectionPanel;
    public Button backButton;
    public MusicManager musicManager;

    [HideInInspector] public List<GolemData> unlockedGolems = new List<GolemData>();
    private GolemData currentGolem;
    public static GolemData SelectedGolem { get; private set; }

    private Material leftArrowMaterial;
    private Material rightArrowMaterial;
    private int currentIndex;

    void Awake()
    {
        // Ensure the golem library is initialized (builds lookup)
        golemLibrary.Initialize();

        // Load the unlocked golems list for the selected level
        LoadGolemsForLevel(GameSession.Instance.SelectedLevel);

        // Hide panel until selection begins
        golemSelectionPanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// Call this once the player has chosen a level.
    /// </summary>
    public void StartSelection()
    {
        currentIndex = 0;
        PrepareArrowMaterials();
        LoadGolemIndex(currentIndex);
        CheckArrowButtons();
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnSelectGolem);
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(GoBack);
        golemSelectionPanel.gameObject.SetActive(true);
    }

    private void PrepareArrowMaterials()
    {
        leftArrowMaterial = leftArrowImage.material;
        leftArrowImage.material = new Material(leftArrowMaterial);

        rightArrowMaterial = rightArrowImage.material;
        rightArrowImage.material = new Material(rightArrowMaterial);
    }

    private void CheckArrowButtons()
    {
        bool atStart = currentIndex == 0;
        bool atEnd = currentIndex == unlockedGolems.Count - 1;

        SetArrowState(leftArrow, leftArrowImage, !atStart);
        SetArrowState(rightArrow, rightArrowImage, !atEnd);
    }

    private void SetArrowState(Button button, Image img, bool enabled)
    {
        button.interactable = enabled;
        var mat = img.material;
        if (!enabled)
        {
            mat.EnableKeyword("_ENABLESATURATION_ON");
            mat.SetFloat("_EnableSaturation", 0f);
        }
        else
        {
            mat.SetFloat("_EnableSaturation", 1f);
            mat.DisableKeyword("_ENABLESATURATION_ON");
        }
    }

    private void LoadGolemIndex(int index)
    {
        currentGolem = unlockedGolems[index];
        selectedGolemNameText.text = currentGolem.golemName.ToUpper();
        selectedGolemSprite.sprite = currentGolem.golemSprite;
        CheckArrowButtons();
    }

    /// <summary>
    /// Filters LevelData.golems by those unlocked in progress and populates unlockedGolems.
    /// </summary>
    private void LoadGolemsForLevel(LevelData level)
    {
        var progress = SaveSystem.progress;
        unlockedGolems = new List<GolemData>();

        foreach (var id in progress.unlockedGolemIds)
        {
            var golem = level.golems.FirstOrDefault(g => g.id == id);
            if (golem != null) unlockedGolems.Add(golem);
        }

        // If none unlocked yet for this level, unlock the first by default
        if (unlockedGolems.Count == 0 && level.golems.Count > 0)
        {
            var first = level.golems[0];
            unlockedGolems.Add(first);
            progress.unlockedGolemIds.Add(first.id);
            SaveSystem.Save(progress);
        }
    }

    // UI Callbacks
    public void OnLeftArrow() { if (currentIndex > 0) LoadGolemIndex(--currentIndex); }
    public void OnRightArrow() { if (currentIndex < unlockedGolems.Count - 1) LoadGolemIndex(++currentIndex); }

    private void OnSelectGolem()
    {
        musicManager.PlayRunButton();
        GameSession.Instance.SelectedGolem = currentGolem;
        loadingPanel.gameObject.SetActive(true);
        // Hide this panel and proceed to game start... 
        golemSelectionPanel.gameObject.SetActive(false);

        loadingManager.StartLoading("GamePlay");

    }

    private void GoBack()
    {
        musicManager.PlayErrorClickSound();
        levelSelectionPanel.gameObject.SetActive(true);
        levelSelectionManager.StartSelection();
        gameObject.SetActive(false);
    }
}
