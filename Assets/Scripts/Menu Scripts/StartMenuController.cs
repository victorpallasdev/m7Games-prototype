using UnityEngine;
using UnityEngine.UI;


public class StartMenuController : MonoBehaviour
{
    public Button playButton;
    public Button settingsButton;
    public Button exitButton;
    public Transform charsPanel;
    public Transform settingsPanel;
    public Transform startGamePanel;
    public CharacterSelectionManager characterSelectionManager;
    public MusicManager musicManager;
   
    

    private void Start()
    {
        musicManager = MusicManager.Instance;
        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(GoToCharSelection);
        settingsButton.onClick.RemoveAllListeners();
        settingsButton.onClick.AddListener(GoToSettings);
        exitButton.onClick.RemoveAllListeners();
        exitButton.onClick.AddListener(ExitGame);
    }
    private void GoToCharSelection()
    {
        musicManager.PlayStandardClickSound();
        GameSession.firstTimeMenu = false;
        charsPanel.gameObject.SetActive(true);
        SaveSystem.Load();
        characterSelectionManager.StartSelection();
        startGamePanel.gameObject.SetActive(false);
    }
    private void GoToSettings()
    {
        musicManager.PlayStandardClickSound();
        settingsPanel.gameObject.SetActive(true);
        startGamePanel.gameObject.SetActive(false);
    }

    public void ExitGame()
    {
        musicManager.PlayErrorClickSound();
        #if UNITY_EDITOR
        // Para cerrar el modo Play en el Editor
        UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Para cerrar la aplicación compilada
            Application.Quit();
        #endif
    }
}
