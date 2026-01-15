using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Audio;


public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    public AudioMixer masterMixer;
    public AudioSource IntroSource;
    public AudioSource UISource;
    public AudioClip introM7Games;

    [Header("Capas musicales")]
    public List<AudioSource> layerSources;

    [Header("Intro del menú")]
    public List<int> initialActiveIndices;
    public int drumIndex = 0;
    public float drumMaxVolume = 0.8f;

    [Header("Patrón aleatorio")]
    public int minLayersPerLoop = 2;
    public int maxLayersPerLoop = 4;

    [Header("Config")]
    public float loopDuration = 8f;
    public float volumeIn = 0f;
    public float volumeOut = 0f;

    [Header("Sound FX")]
    [Header("- UI")]

    public float volumeUI = 0.6f;
    public List<AudioClip> buttonStandard;
    public List<AudioClip> buttonError;
    public List<AudioClip> buttonRun;
    public List<AudioClip> drawCard;
    private Coroutine loopRoutine;

    void Awake()
    {
        Instance = this;
        foreach (var src in layerSources)
        {
            src.time = 0f;
            src.volume = 0f;
            src.loop = false;
            src.Play();
        }
        UISource.volume = volumeUI;
    }
    void Start()
    {
        SetMasterVolume(GameSettings.MasterVolume);
        SetMusicVolume(GameSettings.MusicVolume);
        SetSFXVolume(GameSettings.SFXVolume);
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

    public void PlayIntro()
    {
        IntroSource.clip = introM7Games;
        IntroSource.Play();
    }
    public void StopIntro()
    {
        IntroSource.Stop();
    }

    public void PlayMenuLoop()
    {
        
        if (loopRoutine != null) StopCoroutine(loopRoutine);
        loopRoutine = StartCoroutine(ControlledIntroThenDynamicLoop());
    }

    public void PlayStandardClickSound()
    {
        UISource.clip = buttonStandard[Random.Range(0, buttonStandard.Count)];
        UISource.Play();

    }

    public void PlayErrorClickSound()
    {
        UISource.clip = buttonError[Random.Range(0, buttonError.Count)];
        UISource.Play();
    }

    public void PlayRunButton()
    {
        UISource.clip = buttonRun[Random.Range(0, buttonRun.Count)];
        UISource.Play();
    }
    public void PlayDrawCard()
    {
        UISource.clip = drawCard[Random.Range(0, drawCard.Count)];
        UISource.Play();
    }
    
    IEnumerator ControlledIntroThenDynamicLoop()
    {
        // Sincroniza todos los clips con volumen 0
        foreach (var src in layerSources)
        {
            src.time = 0f;
            src.volume = 0f;
            src.loop = false;
            src.Play();
        }

        // Activa las capas iniciales (menos batería)
        for (int i = 0; i < layerSources.Count; i++)
        {
            if (initialActiveIndices.Contains(i) && i != drumIndex)
            {
                layerSources[i].volume = volumeIn;
            }
        }

        // Batería con crescendo (hasta drumMaxVolume)
        LeanTween.delayedCall(layerSources[drumIndex].gameObject, loopDuration - 1.5f, () =>
        {
            LeanTween.value(layerSources[drumIndex].gameObject, 0f, drumMaxVolume, 1.5f)
                    .setEase(LeanTweenType.linear)
                    .setOnUpdate((float val) =>
                    {
                        layerSources[drumIndex].volume = val;
                    });
        });

        yield return new WaitForSeconds(loopDuration);

        // Reiniciar todos los clips para mantener sincronía
        foreach (var src in layerSources)
        {
            src.Stop();
            src.time = 0f;
            src.Play();
        }

        // 🔁 Bucle aleatorio continuo
        while (true)
        {
            List<int> activeIndices = GetRandomIndices(minLayersPerLoop, maxLayersPerLoop);

            for (int i = 0; i < layerSources.Count; i++)
            {
                if (i == drumIndex)
                {
                    // Batería siempre activa con volumen fijo
                    layerSources[i].volume = drumMaxVolume;
                }
                else
                {
                    layerSources[i].volume = activeIndices.Contains(i) ? volumeIn : volumeOut;
                }
            }

            yield return new WaitForSeconds(loopDuration);

            foreach (var src in layerSources)
            {
                src.Stop();
                src.time = 0f;
                src.Play();
            }
        }
    }

    List<int> GetRandomIndices(int min, int max)
    {
        List<int> indices = new List<int>();
        int total = layerSources.Count;
        int count = Random.Range(min, max + 1);

        List<int> pool = new List<int>();
        for (int i = 0; i < total; i++) pool.Add(i);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            indices.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return indices;
    }

    public void FadeOut(float duration)
    {
        foreach (var src in layerSources)
        {
            float startVolume = src.volume;
            LeanTween.value(src.gameObject, startVolume, 0f, duration)
                     .setEase(LeanTweenType.linear)
                     .setOnUpdate((float val) =>
                     {
                         src.volume = val;
                     });
        }
    }

    public void LoadLevelMusic(List<AudioClip> clips)
    {
        if (clips == null || clips.Count == 0)
        {
            Debug.LogError("No music clips provided for level!");
            return;
        }

        if (clips.Count != layerSources.Count)
        {
            Debug.LogError("Mismatch between clip count and music layers!");
            return;
        }

        // Asignar clips a capas y reiniciar
        for (int i = 0; i < layerSources.Count; i++)
        {
            layerSources[i].clip = clips[i];
            layerSources[i].time = 0f;
            layerSources[i].volume = 0f;            
            layerSources[i].Play();
        }

        // Ajustar duración del bucle a la duración de los clips
        loopDuration = clips[0].length;

        Debug.Log("Level music loaded. Loop duration set to " + loopDuration + " seconds.");
    }



}
