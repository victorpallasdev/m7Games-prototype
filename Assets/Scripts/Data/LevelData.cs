using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level Data", fileName = "LevelData")]
public class LevelData : ScriptableObject
{
    [Header("Identidad del nivel")]
    public string levelId;
    public string levelName;
    public Sprite levelSprite;
    public string firstGolemId;

    [Header("Golems de este nivel")]
    public List<GolemData> golems;

    [Header("Música")]
    public AudioClip introClip;
    public List<AudioClip> clips;
    public int bpm;
}

