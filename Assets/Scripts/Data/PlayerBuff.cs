using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class BuffCatalog
{
    // Mapea el nombre del buffo con un “constructor” que recibe valor y rondas
    private static readonly Dictionary<string, Func<int, int, PlayerBuff>> buffDictionary =
        new Dictionary<string, Func<int, int, PlayerBuff>>
    {
        { "healingTonic_name",
            (value, rounds) => new PlayerBuff(
                "healingTonic_name",
                rounds,
                BuffType.Regeneration,
                value,
                LoadBuffIcon("RegenerationIcon"),
                onturnstart: dwarf => dwarf.StartCoroutine(dwarf.TakeHeal(value))  // ✔️ se resuelve cada turno
            )
        },
        
        { "vitalityEmberMead_name",
            (value, rounds) => new PlayerBuff(
                "vitalityEmberMead_name",
                rounds,
                BuffType.DamageBoost,
                value,
                LoadBuffIcon("BoostDamageIcon"),
                onbuffspawn: dwarf => dwarf.ModifyAttack(value),                     // ✔️ se aplica al inicio de ronda
                onbuffexpires:   dwarf => dwarf.ModifyAttack(0)                            // ✔️ se revierte al final
            )
        },

        /*
         *  Añade aquí más buffos usando el mismo patrón.
         *  Pon “0” en 'value' si un buffo no necesita magnitud.
         */
    };

    /// <summary>
    /// Crea una instancia de PlayerBuff a partir de su nombre, valor y rondas.
    /// </summary>
    public static PlayerBuff CreateBuff(string buffName, int buffValue, int rounds)
    {
        if (buffDictionary.TryGetValue(buffName, out var factory))
            return factory(buffValue, rounds);

        Debug.LogWarning($"Buff '{buffName}' no encontrado en BuffCatalog.");
        return null;
    }

    /// <summary>
    /// Carga el icono desde Addressables (sincronamente, igual que en StatusEffectCatalog).
    /// </summary>
    private static Sprite LoadBuffIcon(string iconName)
    {
        AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(iconName);
        handle.WaitForCompletion();

        if (handle.Status == AsyncOperationStatus.Succeeded)
            return handle.Result;

        Debug.LogWarning($"No se encontró el icono '{iconName}', usando icono por defecto.");
        return Resources.Load<Sprite>("Icons/DefaultIcon");
    }
}

public class PlayerBuff
{
    public string BuffName { get; private set; }
    public int roundDuration { get; set; }
    public int roundRemaining { get; set; }
    public int buffValue;
    public BuffType BuffType { get; private set; }
    public Action<PlayerCharacterController> ApplyBuffAction;
    public Sprite BuffIcon { get; private set; }
    public Action<PlayerCharacterController> OnTurnStart { get; private set; }
    public Action<PlayerCharacterController> OnBuffSpawn { get; private set; }
    public Action<PlayerCharacterController> OnRoundEnd { get; private set; }
    public Action<PlayerCharacterController> OnBuffExpires { get; private set; }
    public PlayerBuff(string name, int rounds, BuffType type, int value, Sprite icon, Action<PlayerCharacterController> onturnstart = null, Action<PlayerCharacterController> onbuffspawn = null, Action<PlayerCharacterController> onroundend = null, Action<PlayerCharacterController> onbuffexpires = null)
    {
        BuffName = name;
        roundDuration = rounds;
        roundRemaining = roundDuration;
        BuffType = type;
        buffValue = value;
        BuffIcon = icon;
        OnTurnStart = onturnstart;
        OnBuffSpawn = onbuffspawn;
        OnRoundEnd = onroundend;
        OnBuffExpires = onbuffexpires;
    }
}
public enum BuffType
{
    Regeneration,
    DamageBoost,
    DefenseBoost,
    SpeedBoost,
    GoldBonus,
    Custom // comodín
}