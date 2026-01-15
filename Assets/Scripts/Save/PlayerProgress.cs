using System;
using System.Collections.Generic;

public class LevelProgress
{
    public string levelId;              // Ej: "Forest"
    public List<string> unlockedGolemIds = new List<string>();
}


[Serializable]
public class PlayerProgress
{
    // Lista de IDs (strings) de los PNJs desbloqueados
    public List<string> unlockedCharactersIds = new List<string>();
    public List<string> unlockedLevelIds = new List<string>();
    public List<string> unlockedGolemIds = new List<string>();

    // (Opcional) otros datos de progreso, por ejemplo:
    // public int lastLevelReached;
    // public float totalGold;
    // Constructor
    public PlayerProgress()
    {
        // Desbloqueos por defecto al crear uno nuevo
        unlockedCharactersIds.Add("1");
        unlockedCharactersIds.Add("2");
        unlockedGolemIds.Add("1");
        unlockedLevelIds.Add("1");
    }
}
