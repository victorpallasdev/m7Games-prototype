using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Gear Library", fileName = "GearLibrary")]
public class GearLibrary : ScriptableObject
{
    [Tooltip("Arrastra aquí todos los DwarfData de tu proyecto")]
    public List<BuyableItemData> allBuyableGear;

    // Diccionario interno para búsqueda rápida
    private Dictionary<string, BuyableItemData> lookup;

    /// <summary>
    /// Inicializa el diccionario. Llamar antes de usar GetById().
    /// </summary>
    public void Initialize()
    {
        lookup = allBuyableGear
            .Where(d => !string.IsNullOrEmpty(d.id))
            .ToDictionary(d => d.id);
    }

    /// <summary>
    /// Devuelve el DwarfData con ese ID, o null si no existe.
    /// </summary>
    public BuyableItemData GetById(string id)
    {
        if (lookup == null) Initialize();
        lookup.TryGetValue(id, out var data);
        return data;
    }
}