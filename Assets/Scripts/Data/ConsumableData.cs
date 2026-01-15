#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
using System.Collections.Generic;
using UnityEngine;
// Este tipo hará de “sub-lista” y Unity sí lo mostrará
[System.Serializable]
public class CardTypeGroup
{
    [Tooltip("Elige uno de estos tipos para satisfacer este grupo (OR interno)")]
    public List<CardType> allowedTypes = new List<CardType>();
}

[CreateAssetMenu(fileName = "NewConsumableData", menuName = "ConsumableData")]
[System.Serializable]
public class ConsumableData : ScriptableObject
{
    public string consumableName;        // Nombre del consumible
    // public int cost;           // Costo en monedas
    public string description; // Descripción del consumible
    public string methodName;  // Nombre del método que ejecutará el efecto
    public Sprite cardImage;
    public int cost;
    public List<CardTypeGroup> requiredGroups;

     [Header("Auto-generated")]
    [SerializeField] private BuyableItemData linkedBuyableItem;

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (linkedBuyableItem == null && !string.IsNullOrEmpty(name))
        {
            string folder = "Assets/Prefabs/BuyableItemsData/Consumables";
            string buyablePath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{name}_Buyable.asset");

            BuyableItemData newBuyable = CreateInstance<BuyableItemData>();
            newBuyable.consumableData = this;
            newBuyable.itemType = BuyableItemType.ConsumableCard;         

            AssetDatabase.CreateAsset(newBuyable, buyablePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            linkedBuyableItem = newBuyable;
            EditorUtility.SetDirty(this);

            Debug.Log($"[Auto] BuyableItemData creado para: {consumableName} en {buyablePath}");
        }
    }
    #endif

}


