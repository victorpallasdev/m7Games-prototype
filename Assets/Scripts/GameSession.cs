using System.Collections.Generic;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }
    public static bool firstTimeMenu { get; set; }
    [SerializeField] public CharacterData SelectedChar { get; set; }
    public LevelData SelectedLevel { get; set; }
    public List<AudioClip> levelMusic { get; set; }
    public GolemData SelectedGolem { get; set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        firstTimeMenu = true;
    }

}
