using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsController : MonoBehaviour
{
    public Button audioButton;
    public Slider masterVolumeSlider;
    public TextMeshProUGUI masterVolumeText;
    public Slider musicVolumeSlider;
    public TextMeshProUGUI musicVolumeText;
    public Slider sfxVolumeSlider;
    public TextMeshProUGUI sfxVolumeText;
    public Button languagesButton;
    public Button spanishButton;
    public Button englishButton;
    public LocalizationManager localizator;
    public Button backButton;
    public Button backFromLanguagesButton;
    public Button backFromAudioButton;
    public Transform startGamePanel;
    public Transform firstSettingsPanel;
    public Transform audioPanel;
    public Transform languagesPanel;
    public Transform settingsPanel;
    private MusicManager musicManager;

    void Start()
    {
        musicManager = MusicManager.Instance;

        audioButton.onClick.RemoveAllListeners();
        audioButton.onClick.AddListener(GoToAudio);
        languagesButton.onClick.RemoveAllListeners();
        languagesButton.onClick.AddListener(GoToLanguages);
        backFromLanguagesButton.onClick.RemoveAllListeners();
        backFromLanguagesButton.onClick.AddListener(GoBackFromLanguages);
        backFromAudioButton.onClick.RemoveAllListeners();
        backFromAudioButton.onClick.AddListener(GoBackFromAudio);
        spanishButton.onClick.RemoveAllListeners();
        spanishButton.onClick.AddListener(SetLanguageToSpanish);
        englishButton.onClick.RemoveAllListeners();
        englishButton.onClick.AddListener(SetLanguageToEnglish);
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(GoBack);

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

    private void GoToLanguages()
    {
        musicManager.PlayStandardClickSound();
        firstSettingsPanel.gameObject.SetActive(false);
        languagesPanel.gameObject.SetActive(true);
    }
    private void GoToAudio()
    {        
        musicManager.PlayStandardClickSound();
        firstSettingsPanel.gameObject.SetActive(false);
        audioPanel.gameObject.SetActive(true);
    }
    private void SetMasterVolume(float volume)
    {
        GameSettings.MasterVolume = volume;
        masterVolumeText.text = (volume * 100f).ToString("F0");
        musicManager.SetMasterVolume(volume);
    }
    private void SetMusicVolume(float volume)
    {
        GameSettings.MusicVolume = volume;
        musicVolumeText.text = (volume * 100f).ToString("F0");
        musicManager.SetMusicVolume(volume);
    }
    private void SetSFXVolume(float volume)
    {
        GameSettings.SFXVolume = volume;
        sfxVolumeText.text = (volume * 100f).ToString("F0");
        musicManager.SetSFXVolume(volume);
    }

    public void SetLanguageToSpanish()
    {
        musicManager.PlayStandardClickSound();
        GameSettings.Language = "es";
        localizator.LoadLanguage("es");
        localizator.UpdateAllLocalizedTexts();
    }

    public void SetLanguageToEnglish()
    {
        musicManager.PlayStandardClickSound();
        GameSettings.Language = "en";
        localizator.LoadLanguage("en");
        localizator.UpdateAllLocalizedTexts();
    }
    public void GoBackFromLanguages()
    {
        musicManager.PlayErrorClickSound();
        languagesPanel.gameObject.SetActive(false);
        firstSettingsPanel.gameObject.SetActive(true);        
    }
    public void GoBackFromAudio()
    {
        musicManager.PlayErrorClickSound();
        audioPanel.gameObject.SetActive(false);
        firstSettingsPanel.gameObject.SetActive(true);        
    }
    public void GoBack()
    {
        musicManager.PlayErrorClickSound();
        startGamePanel.gameObject.SetActive(true);
        settingsPanel.gameObject.SetActive(false);
    }
}
