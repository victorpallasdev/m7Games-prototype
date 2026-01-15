using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewFieldEntityData", menuName = "FieldEntityData")]
public class FieldEntityData : ScriptableObject
{
    [Header("Main Info")]
    public string entityName;
    public Sprite entitySprite;
    public int hitPoints;
    public float weight;
    public List<CardData> CardsToDrop;
    public List<ConsumableData> consumablesToDrop;

}
