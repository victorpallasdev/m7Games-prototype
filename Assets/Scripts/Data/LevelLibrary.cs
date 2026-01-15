using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Level Library", fileName = "LevelLibrary")]
public class LevelLibrary : ScriptableObject
{
    [Tooltip("Arrastra aquí todos tus LevelData")]
    public List<LevelData> allLevels;
    private Dictionary<string,LevelData> lookup;

    public void Initialize()
    {
        lookup = allLevels
            .Where(l => !string.IsNullOrEmpty(l.levelId))
            .ToDictionary(l => l.levelId);
    }

    public LevelData GetById(string id)
    {
        if (lookup == null) Initialize();
        lookup.TryGetValue(id, out var lvl);
        return lvl;
    }
}
