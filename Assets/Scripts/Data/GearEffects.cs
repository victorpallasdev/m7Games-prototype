using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representa un efecto asociado a una pieza de armadura.
/// Contiene distintos hooks (OnBuyGear, OnTurnStart, OnCardPlayed, OnTakeDamage, OnMakeDamage,
/// OnStatusEffect, OnGearExpires). Por defecto todos los Action están en null.
/// </summary>

[System.Serializable]
public class GearEffect
{
    /// <summary>
    /// Se invoca justo cuando el jugador compra o equipa esta pieza.
    /// </summary>
    public Action<PlayerCharacterController> OnEquip = null;

    /// <summary>
    /// Se invoca al comienzo de cada turno (antes de que el jugador actúe).
    /// </summary>
    public Action<PlayerCharacterController> OnTurnStart = null;

    /// <summary>
    /// Si esta función devuelve true para una carta, este efecto quiere ejecutarse.
    /// </summary>
    public Func<CardData, bool>   OnPlayedCardCondition = null;

    /// <summary>
    /// Callback que se ejecuta si la condición anterior fue true.
    /// </summary>
    public Action<PlayedCardsController> OnPlayedCard = null;

    /// <summary>
    /// Se invoca cuando el jugador recibe daño (puede ejecutarse antes o después de aplicar resistencias).
    /// </summary>
    public Action<PlayerCharacterController, int> OnTakeDamage = null;

    /// <summary>
    /// Se invoca cuando el jugador causa daño a un enemigo.
    /// </summary>
    public Action<PlayerCharacterController> OnMakeDamage = null;

    /// <summary>
    /// Se invoca cuando el jugador recibe un nuevo StatusEffect.
    /// </summary>
    public Func<StatusEffect, bool>   OnStatusCondition = null;
    public Action<PlayerCharacterController> OnStatusEffect = null;

    /// <summary>
    /// Se invoca justo cuando la pieza de armadura deja de estar equipada o expira.
    /// </summary>
    public Action<PlayerCharacterController> OnDisequip = null;

    // No hace falta método ApplyEffect ni CheckCondition, 
    // porque invocarás cada Action en el momento oportuno desde PlayerDwarfController.
}

/// <summary>
/// Contiene un diccionario que mapea el nombre de cada pieza de armadura
/// a la lista de GearEffect que debe aplicarse.
/// Ahora este catálogo genera primero los efectos “básicos” de armadura y resistencias
/// de forma automática (si >0), y luego añade los pasivos concretos que tengas definidos
/// en el diccionario solo para aquellos gearNames que tengan lógica especial.
/// </summary>
public static class GearEffectCatalog
{
    // 1) Diccionario de solo los **pasivos específicos** (llámalo “passiveDictionary” para distinguirlo).
    //    Clave = gearName (string), Valor = Func<GearData, List<GearEffect>> que monta los pasivos.
    private static readonly Dictionary<string, Func<GearData, List<GearEffect>>> passiveDictionary =
        new Dictionary<string, Func<GearData, List<GearEffect>>>
    {
        {
            "periscopeHelm_name",
            (gearData) =>
            {
                List<GearEffect> lista = new List<GearEffect>();
                GearEffect PeriscopeHelm = new GearEffect
                {
                    OnEquip = dwarf => dwarf.ModifyCriticalRate(10),
                    OnDisequip = dwarf => dwarf.ModifyCriticalRate(-10)
                };
                lista.Add(PeriscopeHelm);
                return lista;
            }
        },
        {
            "vinebreakerBoots_name",
            (gearData) =>
            {

                List<GearEffect> lista = new List<GearEffect>();
                GearEffect VinebreakerBoots = new GearEffect
                {
                    OnStatusCondition = status => status.StatusName == "Root",
                    OnStatusEffect = dwarf => dwarf.ModifyStatusEffectTurns("Root", 1)
                };
                lista.Add(VinebreakerBoots);
                return lista;
            }
        },

        {
            "voltaicFists_name",
            (gearData) =>
            {
                List<GearEffect> lista = new List<GearEffect>();
                GearEffect VoltaicFists = new GearEffect
                {
                    OnPlayedCardCondition = card => card.cardName == "sapphire_name" && UnityEngine.Random.value < 1f,
                    OnPlayedCard = playedCards => playedCards.InstantiateCardOnFirstPlace(CardDataLoader.LoadCardDataSync("Sapphire"), true)
                };
                lista.Add(VoltaicFists);
                return lista;
            }
        },

        {
            "echo-strider_greaves_name",
            (gearData) =>
            {
                List<GearEffect> lista = new List<GearEffect>();
                GearEffect EchoStriderGreaves = new GearEffect
                {
                    OnMakeDamage = dwarf => dwarf.BounceDmg(10f, 1),
                };
                lista.Add(EchoStriderGreaves);
                return lista;
            }
        },     

        // Aquí defines SOLO los gearNames cuyas pasivas no sean “armadura/resistencia” pura.
        };


    /// <summary>
    /// Genera los efectos “básicos” a partir de los stats de GearData:
    ///  • Si gearData.armor > 0 → genera un efecto que suma al equipar y resta al desequipar.
    ///  • Para cada campo de elementalResistance (Fire, Ice, Arcane, …) que sea > 0,
    ///    genera un efecto que suma esa resistencia al equipar y la resta al desequipar.
    /// </summary>
    private static List<GearEffect> GenerateStatEffects(GearData gearData)
    {
        var lista = new List<GearEffect>();
        Dictionary<Element, int> elementalDict = new Dictionary<Element, int>();

        // 1) Armadura
        if (gearData.armor > 0)
        {
            GearEffect armorEffect = new GearEffect
            {
                OnEquip = dwarf => dwarf.ModifyArmor(gearData.armor),
                OnDisequip = dwarf => dwarf.ModifyArmor(-gearData.armor)
            };
            lista.Add(armorEffect);
        }
        // Si en el futuro añades más campos en ElementalDamage, repites el mismo patrón aquí.
        // 2) Resistencias elementales automáticas
        foreach (ElementalPower element in gearData.elementalResistances)
        {
            elementalDict[element.element] = element.value;
        }

        if (elementalDict.Count > 0)
        {
            var elementalEffect = new GearEffect
            {
                OnEquip = dwarf =>
                {
                    dwarf.AddResistances(elementalDict);
                },
                OnDisequip = dwarf =>
                {
                    dwarf.RemoveResistances(elementalDict);
                }
            };
            lista.Add(elementalEffect);
        }

        return lista;
    }


    /// <summary>
    /// Método único para obtener **todos** los GearEffect que corresponden a esa GearData:
    ///   1) Primero genera los efectos de stat (armadura + resistencias) de forma automática. 
    ///   2) Luego, si en el dictionary existe una entrada para el gearName, añade también esos pasivos.
    /// </summary>
    public static List<GearEffect> CreateEffectsForGear(GearData gearData)
    {
        if (gearData == null)
        {
            Debug.LogWarning("GearData es null en CreateEffectsForGear.");
            return new List<GearEffect>();
        }

        // 1) Efectos automáticos de “stats”
        List<GearEffect> allEffects = GenerateStatEffects(gearData);

        // 2) Si existe pasivo específico en el diccionario, lo agregamos
        if (passiveDictionary.TryGetValue(gearData.gearName, out var constructor))
        {
            var passiveList = constructor(gearData);
            if (passiveList != null && passiveList.Count > 0)
                allEffects.AddRange(passiveList);
        }

        return allEffects;
    }
    

}

