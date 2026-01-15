using UnityEngine;

public class PlayerDwarfSoundController : MonoBehaviour
{
    [Header("Clips de sonido")]
    public AudioClip spendMoneySound;
    private AudioSource audioSource;


    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void playSound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
    

}
