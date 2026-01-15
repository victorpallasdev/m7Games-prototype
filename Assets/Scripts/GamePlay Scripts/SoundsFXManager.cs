using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class SoundsFXManager : MonoBehaviour
{

    public static SoundsFXManager Instance;

    [Header("SOURCES")]
    public AudioSource buttonsAudio;
    public List<AudioSource> entitiesAudio;
    public List<AudioSource> characterAudio;
    public AudioSource cardsAudio;

    [Header("CLIPS")]
    [Header("Cards")]
    public List<AudioClip> cardSound;
    public List<AudioClip> burningCardSound;
    public List<AudioClip> hoverSound;
    [Header("Money")]
    public List<AudioClip> moneySound;
    public List<AudioClip> lotOfMoneySound;
    public List<AudioClip> aLittleBitMoneySound;
    public List<AudioClip> spentMoneySound;
    public List<AudioClip> aLittleBitRunesSound;
    public List<AudioClip> runesSound;
    [Header("Buttons")]
    public List<AudioClip> buttonStandard;
    public List<AudioClip> buttonError;
    public List<AudioClip> openGemPackSound;
    [Header("Effects")]
    public AudioClip shotSound;
    public List<AudioClip> tntSound;
    public List<AudioClip> loadShieldSound;
    public List<AudioClip> downloadShieldSound;

    [Header("Player")]
    public List<AudioClip> playerSteps;
    public List<AudioClip> weaponSpawn;
    public List<AudioClip> takeDmg;

    [Header("Entities")]
    public List<AudioClip> golemSteps;
    public List<AudioClip> golemSounds;
    public List<AudioClip> golemTakeDmg;

    void Awake()
    {
        Instance = this;
    }

    public void PlayCardSound()
    {
        AudioClip selectedClip = cardSound[Random.Range(0, cardSound.Count)];
        if (!cardsAudio.isPlaying)
        {
            cardsAudio.clip = selectedClip;
            cardsAudio.Play();
        }
        else
        {
            cardsAudio.Stop();
            cardsAudio.clip = selectedClip;
            cardsAudio.Play();
        }
    }
    public void PlayBurningCardSound()
    {
        AudioClip selectedClip = burningCardSound[Random.Range(0, burningCardSound.Count)];
        if (!cardsAudio.isPlaying)
        {
            cardsAudio.clip = selectedClip;
            cardsAudio.Play();
        }
        else
        {
            cardsAudio.Stop();
            cardsAudio.clip = selectedClip;
            cardsAudio.Play();
        }
    }

    public void PlayHoverCardSound()
    {
        AudioClip selectedClip = hoverSound[Random.Range(0, hoverSound.Count)];
        if (!cardsAudio.isPlaying)
        {
            cardsAudio.clip = selectedClip;
            cardsAudio.Play();
        }
        else
        {
            cardsAudio.Stop();
            cardsAudio.clip = selectedClip;
            cardsAudio.Play();
        }
    }   

    public void PlayShotSound()
    {
        EntitiesPlayClip(shotSound);
    }
    public void PlayMoneySound()
    {
        AudioClip selectedClip = moneySound[Random.Range(0, moneySound.Count)];
        if (!buttonsAudio.isPlaying)
        {
            buttonsAudio.clip = selectedClip;
            buttonsAudio.Play();
        }
        else
        {
            buttonsAudio.Stop();
            buttonsAudio.clip = selectedClip;
            buttonsAudio.Play();
        }
    }
    public void PlayLotOfMoneySound()
    {
        AudioClip selectedClip = lotOfMoneySound[Random.Range(0, lotOfMoneySound.Count)];
        if (!buttonsAudio.isPlaying)
        {
            buttonsAudio.clip = selectedClip;
            buttonsAudio.Play();
        }
        else
        {
            buttonsAudio.Stop();
            buttonsAudio.clip = selectedClip;
            buttonsAudio.Play();
        }
    }
    public void PlayAlittleBitMoney()
    {
        AudioClip selectedClip = aLittleBitMoneySound[Random.Range(0, aLittleBitMoneySound.Count)];
        buttonsAudio.clip = selectedClip;
        buttonsAudio.Play();
    }

    public void PlaySpentMoneySound()
    {
        AudioClip selectedClip = spentMoneySound[Random.Range(0, spentMoneySound.Count)];
        buttonsAudio.clip = selectedClip;
        buttonsAudio.Play();
    }
    public void PlayAlittleBitRunes()
    {
        AudioClip selectedClip = aLittleBitRunesSound[Random.Range(0, aLittleBitRunesSound.Count)];
        buttonsAudio.clip = selectedClip;
        buttonsAudio.Play();
    }

    public void PlayRunesSound()
    {
        AudioClip selectedClip = runesSound[Random.Range(0, runesSound.Count)];
        buttonsAudio.clip = selectedClip;
        buttonsAudio.Play();       
    }
    public void PlayStandardClickSound()
    {
        buttonsAudio.clip = buttonStandard[Random.Range(0, buttonStandard.Count)];
        buttonsAudio.Play();
    }
    public void PlayErrorClickSound()
    {
        buttonsAudio.clip = buttonError[Random.Range(0, buttonError.Count)];
        buttonsAudio.Play();
    }
    public void PlayPlayerStep()
    {
        CharacterPlayClip(playerSteps[Random.Range(0, playerSteps.Count)]);
    }
    public void PlayWeaponSpawnSound()
    {
        CharacterPlayClip(weaponSpawn[Random.Range(0, weaponSpawn.Count)]);
    }

    public void PlayTNTSound()
    {
        EntitiesPlayClip(tntSound[0]);

    }
    public void PlayGolemStep()
    {
        EntitiesPlayClip(golemSteps[Random.Range(0, golemSteps.Count)]);
    }
    public void PlayGolemSound()
    {
        EntitiesPlayClip(golemSounds[Random.Range(0, golemSounds.Count)]);
    }
    public void PlayLoadShieldSound(bool mode)
    {
        if (mode)
        {
            CharacterPlayClip(loadShieldSound[0]);
        }
        else
        {
            CharacterPlayClip(downloadShieldSound[0]);
        }
    }
    public void PlayPlayerTakeDmgSound()
    {
        CharacterPlayClip(takeDmg[Random.Range(0, takeDmg.Count)]);
    }
    public void PlayGolemTakeDmgSound()
    {
        EntitiesPlayClip(golemTakeDmg[Random.Range(0, golemTakeDmg.Count)]);
    }
    public void PlayOpenPackSound()
    {
        buttonsAudio.clip = openGemPackSound[Random.Range(0, openGemPackSound.Count)];
        buttonsAudio.Play();
    }

    private void CharacterPlayClip(AudioClip audioclip)
    {
        foreach (var source in characterAudio)
        {
            if (source.isPlaying)
            {
                continue;
            }
            else
            {
                source.clip = audioclip;
                source.Play();
                return;
            }
        }

    }

    private void EntitiesPlayClip(AudioClip audioclip)
    {
        foreach (var source in entitiesAudio)
        {
            if (source.isPlaying)
            {
                continue;
            }
            else
            {
                source.clip = audioclip;
                source.Play();
                return;
            }
        }

    }
    
    private void CopyAudioSourceSettings(AudioSource from, AudioSource to)
    {
        to.volume = from.volume;
        to.pitch = from.pitch;
        to.spatialBlend = from.spatialBlend;
        to.loop = false;
        to.playOnAwake = false;
        to.outputAudioMixerGroup = from.outputAudioMixerGroup;
        to.priority = from.priority;
        to.dopplerLevel = from.dopplerLevel;
        to.spread = from.spread;
        to.rolloffMode = from.rolloffMode;
        to.minDistance = from.minDistance;
        to.maxDistance = from.maxDistance;
        // Puedes añadir más si los necesitas
    }
}
