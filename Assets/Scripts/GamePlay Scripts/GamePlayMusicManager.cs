// GameplayMusicManager.cs (versión con doble lista de AudioSources para anacrusas)

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameplayMusicManager : MonoBehaviour
{
    public LevelData levelData;
    public float volumeLevel;
    private AudioSource introSource;



    private void Start()
    {
        Initialize();
        StartCoroutine(PlayIntroAndStartCycle());
    }

    private void Initialize()
    {

        introSource = gameObject.AddComponent<AudioSource>();
        introSource.playOnAwake = false;
        introSource.loop = false;
        introSource.volume = volumeLevel;
    }

    private IEnumerator PlayIntroAndStartCycle()
    {
        introSource.clip = levelData.introClip;
        introSource.Play();
        Debug.Log("🎬 Intro reproduciéndose...");
        // Preparamos los riffs del grupo 0 en sourcesA
        double introEndDSPTime = AudioSettings.dspTime + introSource.clip.length;
        // Esperar hasta que DSP time alcance el final exacto de la intro
        while (AudioSettings.dspTime < introEndDSPTime - 0.05f)
        {
            yield return null; // frame a frame
        }

        Debug.Log("✅ Intro terminada. Iniciando bucle dinámico...");
        StartCoroutine(LoopCycle());
    }
  

    private IEnumerator LoopCycle()
    {
        int currentClipIndex = 0;
        AudioSource loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.volume = volumeLevel;
        loopSource.playOnAwake = false;
        loopSource.loop = false;

        // Reproducir el primer clip
        loopSource.clip = levelData.clips[currentClipIndex];
        loopSource.Play();

        yield return new WaitForSeconds(loopSource.clip.length);

        while (true)
        {
            // Elegir aleatoriamente otro clip distinto del actual
            List<int> posibles = Enumerable.Range(0, levelData.clips.Count).Where(i => i != currentClipIndex).ToList();
            int nextClipIndex = posibles[Random.Range(0, posibles.Count)];
            currentClipIndex = nextClipIndex;

            // Reproducir el nuevo clip
            loopSource.clip = levelData.clips[currentClipIndex];
            loopSource.Play();

            Debug.Log($"🎵 Sonando clip {currentClipIndex}");

            yield return new WaitForSeconds(loopSource.clip.length);
        }
    }
 
}
