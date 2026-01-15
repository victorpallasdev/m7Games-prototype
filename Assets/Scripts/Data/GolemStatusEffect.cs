using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public static class GolemStatusEffectCatalog
{
    // Diccionario: nombre -> fábrica (value, duration) => instancia
    private static readonly Dictionary<string, Func<float, int, GolemStatusEffect>> effectDictionary
        = new Dictionary<string, Func<float, int, GolemStatusEffect>>(StringComparer.OrdinalIgnoreCase)
    {
        {
            "Wet", (value, duration) =>
            {
                GolemStatusEffect effect = new GolemStatusEffect
                {
                    StatusName    = "Wet",
                    TurnsRemaining = duration,
                    EffectType    = StatusEffectType.DefenseReduction,
                    EffectValue   = value,
                    // Al ganar el estado: baja resistencias a Electric e Ice
                    OnGet         = golem => golem?.ModifyWeaknesses(new List<Element>{ Element.Electric, Element.Ice }, value),
                    // Al quitar el estado: revierte el cambio
                    OnQuit        = golem => golem?.ModifyWeaknesses(new List<Element>{ Element.Electric, Element.Ice }, -value),
                    EffectIcon    = LoadEffectIcon("WetIcon")
                };
                return effect; // <- IMPORTANTE
            }
        },
    };

    public static GolemStatusEffect CreateEffect(string effectName, float effectValue, int duration)
    {
        if (effectDictionary.TryGetValue(effectName, out var factory))
            return factory(effectValue, duration);

        Debug.LogWarning($"Estado '{effectName}' no encontrado en GolemStatusEffectCatalog.");
        return null;
    }

    public static Sprite LoadEffectIcon(string iconName)
    {
        // Si usas Addressables: key = iconName
        AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(iconName);
        handle.WaitForCompletion();

        if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            return handle.Result;

        Debug.LogWarning($"No se encontró el ícono '{iconName}', usando icono por defecto.");
        return Resources.Load<Sprite>("Icons/DefaultIcon"); // Ajusta tu ruta de fallback
    }
}
public class GolemStatusEffect
{
    public string StatusName { get; set; }
    public int TurnsRemaining { get; set; }
    public StatusEffectType EffectType { get; set; }
    public float EffectValue { get; set; }
    public Action<GolemController> OnGet { get; set; }
    public Action<GolemController> OnTurn { get; set; }
    public Action<GolemController> OnTakeDamage { get; set; }
    public Action<GolemController> OnQuit { get; set; }
    public Sprite EffectIcon { get; set; }
    public bool IsExpired() => TurnsRemaining <= 0;
}
