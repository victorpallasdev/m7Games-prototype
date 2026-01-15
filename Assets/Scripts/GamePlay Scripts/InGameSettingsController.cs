using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameSettingsController : MonoBehaviour
{
    public Button settingsButton;
    public Button languagesButton;
    public Button audioButton;
    public Button spanishButton;
    public Button englishButton;
    private LocalizationManager localizator;
    public Button backButton;
    public Button backFromExitButton;
    public Button backFromLanguages;
    public Button backFromAudio;
    public Button goExitOptions;
    public Button exitMenuButton;
    public Button exitGameButton;
    public CombatManager combatManager;
    public Transform settingsPanel;
    public Transform mainButtonsContainer;
    public Transform exitButtonsContainer;
    public Slider masterVolumeSlider;
    public TextMeshProUGUI masterVolumeText;
    public Slider musicVolumeSlider;
    public TextMeshProUGUI musicVolumeText;
    public Slider sfxVolumeSlider;
    public TextMeshProUGUI sfxVolumeText;
    public GameplayAudioManager gameplayAudioManager;
    public SoundsFXManager soundsFXManager;
    public Transform languagesPanel;
    public Transform audioPanel;
    void Start()
    {
        spanishButton.onClick.RemoveAllListeners();
        spanishButton.onClick.AddListener(SetLanguageToSpanish);
        englishButton.onClick.RemoveAllListeners();
        englishButton.onClick.AddListener(SetLanguageToEnglish);
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(GoBack);
        settingsButton.onClick.RemoveAllListeners();
        settingsButton.onClick.AddListener(OpenSettings);
        goExitOptions.onClick.RemoveAllListeners();
        goExitOptions.onClick.AddListener(GoToExitOptions);
        backFromExitButton.onClick.RemoveAllListeners();
        backFromExitButton.onClick.AddListener(GoBackFromExitOptions);
        exitMenuButton.onClick.RemoveAllListeners();
        exitMenuButton.onClick.AddListener(ExitMenu);
        exitGameButton.onClick.RemoveAllListeners();
        exitGameButton.onClick.AddListener(ExitGame);
        backFromAudio.onClick.RemoveAllListeners();
        backFromAudio.onClick.AddListener(BackFromAudio);
        backFromLanguages.onClick.RemoveAllListeners();
        backFromLanguages.onClick.AddListener(BackFromLanguages);
        audioButton.onClick.RemoveAllListeners();
        audioButton.onClick.AddListener(GoToAudio);
        languagesButton.onClick.RemoveAllListeners();
        languagesButton.onClick.AddListener(GoToLanguages);        
        localizator = LocalizationManager.Instance;
        soundsFXManager = SoundsFXManager.Instance;
        
        masterVolumeSlider.value = GameSettings.MasterVolume;
        masterVolumeText.text =(GameSettings.MasterVolume * 100f).ToString("F0");
        masterVolumeSlider.onValueChanged.RemoveAllListeners();
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.value = GameSettings.MusicVolume;
        musicVolumeText.text = (GameSettings.MusicVolume * 100f).ToString("F0");
        musicVolumeSlider.onValueChanged.RemoveAllListeners();
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.value = GameSettings.SFXVolume;
        sfxVolumeText.text = (GameSettings.SFXVolume * 100f).ToString("F0");
        sfxVolumeSlider.onValueChanged.RemoveAllListeners();
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
    }
    public void OpenSettings()
    {
        soundsFXManager.PlayStandardClickSound();
        settingsPanel.gameObject.SetActive(true);
        combatManager.toggleUI();
    }

    private void GoToLanguages()
    {
        soundsFXManager.PlayStandardClickSound();
        mainButtonsContainer.gameObject.SetActive(false);
        languagesPanel.gameObject.SetActive(true);
    }
    private void BackFromLanguages()
    {
        soundsFXManager.PlayErrorClickSound();
        languagesPanel.gameObject.SetActive(false);
        mainButtonsContainer.gameObject.SetActive(true);        
    }
    private void GoToAudio()
    {
        soundsFXManager.PlayStandardClickSound();
        mainButtonsContainer.gameObject.SetActive(false);
        audioPanel.gameObject.SetActive(true);
    }

    private void BackFromAudio()
    {
        soundsFXManager.PlayErrorClickSound();
        audioPanel.gameObject.SetActive(false);
        mainButtonsContainer.gameObject.SetActive(true);        
    }
    private void SetMasterVolume(float volume)
    {
        GameSettings.MasterVolume = volume;
        masterVolumeText.text = (volume * 100f).ToString("F0");
        gameplayAudioManager.SetMasterVolume(volume);
    }
    private void SetMusicVolume(float volume)
    {
        GameSettings.MusicVolume = volume;
        musicVolumeText.text = (volume * 100f).ToString("F0");
        gameplayAudioManager.SetMusicVolume(volume);
    }
    private void SetSFXVolume(float volume)
    {
        GameSettings.SFXVolume = volume;
        sfxVolumeText.text = (volume * 100f).ToString("F0");
        gameplayAudioManager.SetSFXVolume(volume);
    }

    public void SetLanguageToSpanish()
    {
        soundsFXManager.PlayStandardClickSound();
        localizator.LoadLanguage("es");
        localizator.UpdateAllLocalizedTexts();
    }

    public void SetLanguageToEnglish()
    {
        soundsFXManager.PlayStandardClickSound();
        localizator.LoadLanguage("en");
        localizator.UpdateAllLocalizedTexts();
    }
    public void GoToExitOptions()
    {
        soundsFXManager.PlayStandardClickSound();
        mainButtonsContainer.gameObject.SetActive(false);
        exitButtonsContainer.gameObject.SetActive(true);
    }
    public void GoBackFromExitOptions()
    {
        soundsFXManager.PlayErrorClickSound();
        mainButtonsContainer.gameObject.SetActive(true);
        exitButtonsContainer.gameObject.SetActive(false);
    }
    public void ExitGame()
    {
        soundsFXManager.PlayErrorClickSound();
        #if UNITY_EDITOR
            // Para cerrar el modo Play en el Editor
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Para cerrar la aplicación compilada
            Application.Quit();
        #endif
    }
    public void ExitMenu()
    {
        soundsFXManager.PlayErrorClickSound();
        SceneManager.LoadScene("MainMenu");
    }
    public void GoBack()
    {
        soundsFXManager.PlayErrorClickSound();
        settingsPanel.gameObject.SetActive(false);
        combatManager.toggleUI();
    }
}
