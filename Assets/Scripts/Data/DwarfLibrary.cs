using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Library", fileName = "CharacterLibrary")]
public class CharacterLibrary : ScriptableObject
{
    [Tooltip("Arrastra aquí todos los DwarfData de tu proyecto")]
    public List<CharacterData> allCharacters;

    // Diccionario interno para búsqueda rápida
    private Dictionary<string, CharacterData> lookup;

    /// <summary>
    /// Inicializa el diccionario. Llamar antes de usar GetById().
    /// </summary>
    public void Initialize()
    {
        lookup = allCharacters
            .Where(d => !string.IsNullOrEmpty(d.id))
            .ToDictionary(d => d.id);
    }

    /// <summary>
    /// Devuelve el DwarfData con ese ID, o null si no existe.
    /// </summary>
    public CharacterData GetById(string id)
    {
        if (lookup == null) Initialize();
        lookup.TryGetValue(id, out var data);
        return data;
    }
}
