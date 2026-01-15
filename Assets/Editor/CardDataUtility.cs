#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CardDataUtility
{
    /// <summary>
    /// Busca todas las CardData en Assets/CardsData cuyo type sea Gem o DefensiveGem.
    /// </summary>
    public static List<CardData> LoadGemsCardData()
    {
        var gems = new List<CardData>();
        // 1) Buscar en Assets/CardsData todos los assets de tipo CardData
        string[] guids = AssetDatabase.FindAssets("t:CardData", new[] { "Assets/CardsData" });

        foreach (var guid in guids)
        {
            // 2) Convertir GUID a ruta y cargar el asset
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var cd = AssetDatabase.LoadAssetAtPath<CardData>(path);
            if (cd == null) 
                continue;

            // 3) Filtrar por tu enum (o string) de tipo
            //    Asumimos que CardData tiene un campo público "type" de tipo CardType
            if (cd.cardType == CardType.Gem || cd.cardType == CardType.DefensiveGem)
            {
                gems.Add(cd);
            }
        }
        return gems;
    }
}
#endif
