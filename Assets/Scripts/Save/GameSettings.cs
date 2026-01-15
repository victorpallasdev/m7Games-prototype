using UnityEngine;

public static class GameSettings
{
    // Valores por defecto
    private const float DEFAULT_MASTER = 0.75f;
    private const float DEFAULT_MUSIC = 1f;
    private const float DEFAULT_SFX = 1f;

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat("MasterVolume", DEFAULT_MASTER);
        set
        {
            PlayerPrefs.SetFloat("MasterVolume", value);
            PlayerPrefs.Save();
        }
    }
    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat("MusicVolume", DEFAULT_MUSIC);
        set
        {
            PlayerPrefs.SetFloat("MusicVolume", value);
            PlayerPrefs.Save();
        }
    }

    public static float SFXVolume
    {
        get => PlayerPrefs.GetFloat("SFXVolume", DEFAULT_SFX);
        set
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
            PlayerPrefs.Save();
        }
    }

    public static string Language
    {
        get => PlayerPrefs.GetString("Language", "en");
        set
        {
            PlayerPrefs.SetString("Language", value);
            PlayerPrefs.Save();
        }
    }
}
