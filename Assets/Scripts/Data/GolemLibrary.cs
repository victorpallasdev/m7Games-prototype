using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Golem Library", fileName = "GolemLibrary")]
public class GolemLibrary : ScriptableObject
{
    [Tooltip("Arrastra aquí todos los DwarfData de tu proyecto")]
    public List<GolemData> allGolems;

    // Diccionario interno para búsqueda rápida
    private Dictionary<string, GolemData> lookup;

    /// <summary>
    /// Inicializa el diccionario. Llamar antes de usar GetById().
    /// </summary>
    public void Initialize()
    {
        lookup = allGolems
            .Where(g => !string.IsNullOrEmpty(g.id))
            .ToDictionary(d => d.id);
    }

    /// <summary>
    /// Devuelve el GolemData con ese ID, o null si no existe.
    /// </summary>
    public GolemData GetById(string id)
    {
        if (lookup == null) Initialize();
        lookup.TryGetValue(id, out var data);
        return data;
    }
}
