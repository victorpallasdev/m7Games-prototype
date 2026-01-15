using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public string description;
    public string id;
    public int level;
    public int maxHealth;
    public int maxMagic;
    public int healthRegen; 
    public int magicRegen; 
    public int shield;
    public int lifeSteal;

    public Sprite characterSprite;
    public Sprite backPackSprite;
    public Color characterColor;
    public RuntimeAnimatorController animatorController;
    public RuntimeAnimatorController weaponAnimatorController;
    public List<CardData> deck;
}
