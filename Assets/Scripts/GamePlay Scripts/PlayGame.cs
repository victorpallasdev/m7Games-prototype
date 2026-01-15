using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene"); // Cambia "GameScene" al nombre de tu escena de juego
    }

    public void ExitGame()
    {
        Debug.Log("Exit Game");
        Application.Quit(); // Solo funciona en builds
    }
}