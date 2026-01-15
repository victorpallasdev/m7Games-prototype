using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Fusion Matrix", fileName = "FusionMatrix")]
public class FusionMatrix : ScriptableObject
{
    [Header("1) Agrega aquí tus CardData de gemas")]
    public List<CardData> gemCards = new List<CardData>();

    [Header("2) No toques esto: se genera automáticamente")]
    public List<FusionEntry> entries = new List<FusionEntry>();

    [System.Serializable]
    public class FusionEntry
    {
        public CardData gemA;
        public CardData gemB;
        public CardData result;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Cada vez que cambias gemCards en el Inspector, regeneramos entries
        SyncEntries();
    }

    private void SyncEntries()
    {
        int N = gemCards.Count;
        // Guardamos viejos resultados en un diccionario por clave "nombreA|nombreB"
        var old = new Dictionary<string, CardData>();
        foreach (var e in entries)
        {
            if (e.gemA != null && e.gemB != null)
            {
                string key = MakeKey(e.gemA.cardName, e.gemB.cardName);
                old[key] = e.result;
            }
        }

        // Construimos la nueva lista de entries con todas las combinaciones i≤j
        var next = new List<FusionEntry>();
        for (int i = 0; i < N; i++)
        {
            for (int j = i; j < N; j++)
            {
                var a = gemCards[i];
                var b = gemCards[j];
                if (a == null || b == null) continue;

                var e = new FusionEntry
                {
                    gemA   = a,
                    gemB   = b,
                    result = null
                };
                // Si ya existía un resultado, lo preservamos
                string key = MakeKey(a.cardName, b.cardName);
                if (old.TryGetValue(key, out var saved))
                    e.result = saved;

                next.Add(e);
            }
        }

        entries = next;
        EditorUtility.SetDirty(this);
    }

    private string MakeKey(string a, string b)
    {
        // Garantizamos que (A,B) y (B,A) mapeen a la misma clave
        return string.Compare(a, b) <= 0
            ? $"{a}|{b}"
            : $"{b}|{a}";
    }
#endif
}
