using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public static class StatusEffectCatalog
{
    // Este diccionario simplemente mapea los nombres de los estados con sus metodos generadores
    private static Dictionary<string, Func<float, int, StatusEffect>> effectDictionary = new Dictionary<string, Func<float, int, StatusEffect>>
    {
        { "Burn", (value, duration) => new StatusEffect("Burn", duration, StatusEffectType.DamageOverTime, value, LoadEffectIcon("BurnIcon"), dwarf => dwarf.TakeDamage(0, new Dictionary<Element, int>{{Element.Fire , (int)value}}))},
        { "Poison", (value, duration) => new StatusEffect("Poison", duration, StatusEffectType.DamageOverTime, value, LoadEffectIcon("PoisonIcon"), dwarf => dwarf.TakeDamage(0, new Dictionary<Element, int>{{Element.Nature , (int)value}}))},
        //{ "Stun", (value, duration) => new StatusEffect("Aturdido", duration, StatusEffectType.Immobilize, value, dwarf => dwarf.SkipTurn()) },
        //{ "Blind", (value, duration) => new StatusEffect("Ceguera", duration, StatusEffectType.AccuracyReduction, value, dwarf => dwarf.ReduceAccuracy(value)) },
        { "Root", (value, duration) => new StatusEffect("Root", duration, StatusEffectType.Immobilize, value, LoadEffectIcon("RootIcon"), dwarf => dwarf.Immobilize()) },
    };

    public static StatusEffect CreateEffect(string effectName, float effectValue, int duration)
    {
        if(effectDictionary.ContainsKey(effectName))
        {
            return effectDictionary[effectName](effectValue, duration);
        }
        else
        {
            Debug.LogWarning($"Estado '{effectName}' no encontrado en StatusEffectCatalog.");
            return null;
        }
    }

    public static Sprite LoadEffectIcon(string iconName)
    {
        AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(iconName);
        handle.WaitForCompletion(); // 🔹 Espera hasta que la carga finalice completamente

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result; // ✅ Devuelve el sprite cargado correctamente
        }

        Debug.LogWarning($"No se encontró el ícono '{iconName}', usando icono por defecto.");
        return Resources.Load<Sprite>("Icons/DefaultIcon");
    }
}
public class StatusEffect
{
    public string StatusName { get; private set; } // Nombre del estado (ej. "Quemado", "Paralizado")
    public int TurnsRemaining { get; set; } // Duración en turnos
    public StatusEffectType EffectType { get; private set; } // Tipo de efecto (daño, inmovilización, etc.)
    public float EffectValue { get; private set; } // Magnitud del efecto (ej. daño por turno, % reducción)
    public Action<PlayerCharacterController> ApplyEffectAction; // Delegado que define qué hace el estado
    public Sprite EffectIcon { get; private set; }

    public StatusEffect(string name, int duration, StatusEffectType type, float value, Sprite icon, Action<PlayerCharacterController> effectAction)
    {
        StatusName = name;
        TurnsRemaining = duration;
        EffectType = type;
        EffectValue = value;
        ApplyEffectAction = effectAction;
        EffectIcon = icon;
    }

    public void ApplyEffect(PlayerCharacterController character)
    {
        Debug.Log("ENTRA EN EL APPLYEFFECT");
        ApplyEffectAction?.Invoke(character);
    }

    public bool IsExpired()
    {
        return TurnsRemaining <= 0;
    }
}

// Enum para definir los tipos de efectos
public enum StatusEffectType
{
    DamageOverTime,   // Ejemplo: Quemado, Veneno
    Immobilize,       // Ejemplo: Parálisis, Raíces
    AccuracyReduction, // Ejemplo: Ceguera
    AttackFailure,    // Ejemplo: Confusión (probabilidad de fallar)
    DefenseReduction, // Ejemplo: Debilidad (recibe más daño)
    SpeedReduction    // Ejemplo: Congelación (reduce turnos o velocidad)
}
