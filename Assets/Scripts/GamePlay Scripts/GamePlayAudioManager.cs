using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Audio;

public class GameplayAudioManager : MonoBehaviour
{
    public static GameplayAudioManager Instance;
    public LevelData levelData;
    public float volumeLevel = 1f;
    public AudioSource musicSource;
    public AudioClip shopMusic;
    public AudioSource buttonsSource;
    public AudioSource entitiesSource;
    public AudioSource cardsSource;
    public AudioMixer masterMixer;
    public SoundsFXManager soundsFXManager;
    private Coroutine musicLoopCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Initialize();
        StartCoroutine(PlayIntroAndStartCycle());
        SetMasterVolume(GameSettings.MasterVolume);
        SetMusicVolume(GameSettings.MusicVolume);
        SetSFXVolume(GameSettings.SFXVolume);
    }

    private void Initialize()
    {        
        musicSource.playOnAwake = false;
        musicSource.loop = false;
        musicSource.volume = volumeLevel;
    }

    public void SetMasterVolume(float volume)
    {
        masterMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }
    public void SetMusicVolume(float volume)
    {
        masterMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }
    public void SetSFXVolume(float volume)
    {
        masterMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }
   

    private IEnumerator PlayIntroAndStartCycle()
    {
        musicSource.clip = levelData.introClip;
        musicSource.Play();
        Debug.Log("🎬 Intro reproduciéndose...");
        // Preparamos los riffs del grupo 0 en sourcesA
        double introEndDSPTime = AudioSettings.dspTime + musicSource.clip.length;
        // Esperar hasta que DSP time alcance el final exacto de la intro
        while (AudioSettings.dspTime < introEndDSPTime - 0.05f)
        {
            yield return null; // frame a frame
        }

        Debug.Log("✅ Intro terminada. Iniciando bucle dinámico...");
        musicLoopCoroutine = StartCoroutine(LoopCycle());
    }


    private IEnumerator LoopCycle()
    {
        int currentClipIndex = 0;
        musicSource.volume = volumeLevel;
        musicSource.playOnAwake = false;
        musicSource.loop = false;

        // Reproducir el primer clip
        musicSource.clip = levelData.clips[currentClipIndex];
        musicSource.Play();

        yield return new WaitForSeconds(musicSource.clip.length);

        while (true)
        {
            // Elegir aleatoriamente otro clip distinto del actual
            List<int> posibles = Enumerable.Range(0, levelData.clips.Count).Where(i => i != currentClipIndex).ToList();
            int nextClipIndex = posibles[Random.Range(0, posibles.Count)];
            currentClipIndex = nextClipIndex;

            // Reproducir el nuevo clip
            musicSource.clip = levelData.clips[currentClipIndex];
            musicSource.Play();

            Debug.Log($"🎵 Sonando clip {currentClipIndex}");

            yield return new WaitForSeconds(musicSource.clip.length);
        }
    }
    private IEnumerator LoopShopCycle()
    {
        musicSource.clip = shopMusic;
        musicSource.Play();
        yield return new WaitForSeconds(musicSource.clip.length);
        while (true)
        {   
            musicSource.Play();
            yield return new WaitForSeconds(musicSource.clip.length);
        }

    }


    public void FadeOutInShopMusic()
    {
        float fadeDuration = 1f; // total: 1 segundo
        float half = fadeDuration / 2f;
        StopAllCoroutines(); // Detiene PlayIntroAndStartCycle si estaba activo
        LeanTween.cancel(musicSource.gameObject);
        // Fade Out
        LeanTween.value(musicSource.gameObject, 1f, 0f, half)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnUpdate((float t) =>
            {
                musicSource.volume = t;
            })
            .setOnComplete(() =>
            {

                musicLoopCoroutine = StartCoroutine(LoopShopCycle());

                // Fade In
                LeanTween.value(musicSource.gameObject, 0f, 1f, half)
                    .setEase(LeanTweenType.easeInOutSine)
                    .setOnUpdate((float t) =>
                    {
                        musicSource.volume = t;
                    });
            });
    }
        public void FadeOutInLevelMusic()
        {
            float fadeDuration = 1f; // total: 1 segundo
            float half = fadeDuration / 2f;

            // Fade Out
            LeanTween.value(musicSource.gameObject, 1f, 0f, half)
                .setEase(LeanTweenType.easeInOutSine)
                .setOnUpdate((float t) =>
                {
                    musicSource.volume = t;
                })
                .setOnComplete(() =>
                {
                    musicLoopCoroutine = StartCoroutine(LoopCycle());
                });
        }


 
}
