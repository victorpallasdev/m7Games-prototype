#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
using UnityEngine;
using System.Collections.Generic;

public enum GearType { Helmet, Chestplate, Gloves, Leggings, Boots}

[CreateAssetMenu(fileName = "NewGear", menuName = "GearData")]
public class GearData : ScriptableObject
{
    [Header("Main Info")]
    public string gearName; // Nombre del equipo
    public Sprite gearIcon; // Ícono para mostrar en la tienda
    public GearType gearType; // Tipo de equipo (Casco, Pechera, Botas, etc.)
    public int goldCost;

    [Header("Stats")]
    public int armor; // Armadura que otorga
    public List<ElementalPower> elementalResistances; // Resistencia a un elemento (ejemplo: Fuego, Hielo)
    public List<GearEffect> gearEffects;
    [TextArea] public string description; // Descripción para la tienda  
    
    [Header("Auto-generated")]
    [SerializeField] private BuyableItemData linkedBuyableItem;

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (linkedBuyableItem == null && !string.IsNullOrEmpty(name))
        {
            string folder = "Assets/Prefabs/BuyableItemsData/Gear";
            string buyablePath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{name}_Buyable.asset");

            BuyableItemData newBuyable = CreateInstance<BuyableItemData>();
            newBuyable.gearData = this;
            newBuyable.itemType = BuyableItemType.Gear;         

            AssetDatabase.CreateAsset(newBuyable, buyablePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            linkedBuyableItem = newBuyable;
            EditorUtility.SetDirty(this);

            Debug.Log($"[Auto] BuyableItemData creado para: {gearName} en {buyablePath}");
        }
    }
    #endif
}


