using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string FilePath;
    public static PlayerProgress progress;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitPath()
    {
        FilePath = Path.Combine(Application.persistentDataPath, "SaveProgress.json");
    }

    public static void Init()
    {
        progress = Load();
    }

    public static void Save(PlayerProgress progress)
    {
        string json = JsonUtility.ToJson(progress, true);
        File.WriteAllText(FilePath, json);
        Debug.Log($"[SaveSystem] Progreso guardado en: {FilePath}");
    }

    public static PlayerProgress Load()
    {
        PlayerProgress progress;

        if (!File.Exists(FilePath))
        {
            Debug.Log("[SaveSystem] No se encontró archivo de progreso. Creando uno nuevo con defaults.");
            // 1) Creamos un progreso con constructor que pone el dwarf '1' y golem '1'
            progress = new PlayerProgress();
            // 2) Guardamos inmediatamente para que exista de aquí en adelante
            Save(progress);
        }
        else
        {
            try
            {
                string json = File.ReadAllText(FilePath);
                progress = JsonUtility.FromJson<PlayerProgress>(json) ?? new PlayerProgress();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveSystem] Error cargando progreso: {e.Message}. Se crea uno nuevo con defaults.");
                progress = new PlayerProgress();
                Save(progress);
            }
        }

        return progress;
    }
}
