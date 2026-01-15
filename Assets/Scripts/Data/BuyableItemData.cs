using UnityEngine;
using System.Collections.Generic;

public enum BuyableItemType
{
    GemPack,
    Upgrade,
    Buff,
    Mead,
    Healing,
    Bonfire,
    WeaponBox,
    GadgetBox,
    ConsumableCard,
    Vouche,
    Gear
}


[CreateAssetMenu(fileName = "NewBuyableItem", menuName = "BuyableItemData")]
public class BuyableItemData : ScriptableObject
{
    public string id;
    public string itemName;
    public Sprite itemSprite;
    public int cost;
    public BuyableItemType itemType;
    public string description;

    [Header("Gem Packs")]
    public int gemAmount;
    public List<CardData> possibleGems;
    public Dictionary<CardType, float> rarityChances;

    [Header("Weapon Stand")]
    public int weaponAmount;
    public List<CardData> possibleWeapons;

    [Header("Gadget Chest")]
    public int gadgetAmount;
    public List<CardData> possibleGadgets;

    [Header("Consumable Card")]
    public ConsumableData consumableData;

    [Header("Buff")]
    public int buffValue;
    public int roundsDuration;

    [Header("Healing")]
    public int healingAmount;

    [Header("Vouche")]
    public bool fusionatorVouche;

    [Header("Gear")]
    public GearData gearData;

    
}
    
