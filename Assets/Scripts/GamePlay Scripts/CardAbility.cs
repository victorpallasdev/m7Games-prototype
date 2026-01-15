using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Collections;
using UnityEngine.UI;

[Serializable]
public class TemporaryModifier
{
    public Element attackElement; // Nombre del elemento ("Fire", "Ice", etc.)
    public Element resistanceElement; // Nombre del elemento ("Fire", "Ice", etc.)
    public int lifeSteal; 
    public int damage;         // Daño que aplica este modificador
    public int remainingTurns; // Turnos restantes para este modificador  
}

public class CardAbility : MonoBehaviour
{
    private static Dictionary<Element, int> accumulatedElementalPowers;
    
    void Awake()
    {
        accumulatedElementalPowers = new Dictionary<Element, int>
        {
            { Element.Fire, 0 },
            { Element.Ice, 0 },
            { Element.Electric, 0 },
            { Element.Water, 0 },
            { Element.Nature, 0 },
            { Element.Earth, 0 }
        };
    }

    void Update()
    {
        
    }

    public void GiveWeapon(CardData cardData, List<TemporaryModifier> temporaryOffensiveModifiers, List<TemporaryModifier> temporaryDefensiveModifiers)
    {
        PlayerCharacterController dwarfController = PlayerCharacterController.Instance;
        PlayedCardsController playedCardsController = PlayedCardsController.Instance;
        CardController cardController = GetComponent<CardController>();
        Dictionary<Element, int> elementalDamages = cardData.GetElementalDamages();
        foreach (var element in elementalDamages)
        {
            TemporaryModifier modifier = new TemporaryModifier
            {
                attackElement = element.Key, 
                damage = element.Value,
                remainingTurns = cardData.duration,
            };
            playedCardsController.AddTemporaryOffensiveModifier(modifier, transform.position);
        }
        // Recorre toda la lista de temporaryModifiers y agrega a nuestro diccionario de efectos acumulados justo el daño del modificador en el elemento correspondiente
        foreach (var modifier in temporaryOffensiveModifiers)
        {            
            accumulatedElementalPowers[modifier.attackElement] += modifier.damage;          
        }
        // Se llama a la funcion GivePickaxe del dwarf pasandole el poder, la duracion del pico, el diccionario de efectos acumulados y el nombre del pico
        dwarfController.GiveWeapon(cardData.power, cardData.duration, accumulatedElementalPowers, cardData.cardName, cardData.icon);
        if (cardData.lifeSteal > 0)
        {
            playedCardsController.AddTemporaryOffensiveModifier(new TemporaryModifier {lifeSteal = cardData.lifeSteal}, transform.position);
        }        
        dwarfController.IncreaseLifeSteal(playedCardsController.GetLifeStealModifier(), cardData.duration);
        // Reseteo los dos diccionarios porque representa que se gastan los efectos en esta habilidad
        playedCardsController.ClearTemporaryModifiers();
        accumulatedElementalPowers = new Dictionary<Element, int>
        {
            { Element.Fire, 0 },
            { Element.Ice, 0 },
            { Element.Electric, 0 },
            { Element.Water, 0 },
            { Element.Nature, 0 },
            { Element.Earth, 0 }
        };
        cardController.IsProcesed = true;       
    }

    public void BeerHeal(CardData cardData, List<TemporaryModifier> temporaryOffensiveModifiers, List<TemporaryModifier> temporaryDefensiveModifiers)
    {
        PlayerCharacterController dwarf = PlayerCharacterController.Instance;
        CardController cardController = GetComponent<CardController>();
        // Cura el 50% de la vida máxima
        int curacion = dwarf.maxHealth / 2;
        Debug.Log($"Se va a curar = {curacion}");
        StartCoroutine(dwarf.TakeHeal(curacion));
        cardController.IsProcesed = true;
    }
    public void GrapesHeal(CardData cardData, List<TemporaryModifier> temporaryOffensiveModifiers, List<TemporaryModifier> temporaryDefensiveModifiers)
    {
        PlayerCharacterController dwarf = PlayerCharacterController.Instance;
        CardController cardController = GetComponent<CardController>();
        // Cura el power de la carta
        int curacion = cardData.power;
        Debug.Log($"Se va a curar = {curacion}");
        StartCoroutine(dwarf.TakeHeal(curacion));
        cardController.IsProcesed = true;  
    }

    public void CitrineShield(CardData cardData, List<TemporaryModifier> temporaryOffensiveModifiers, List<TemporaryModifier> temporaryDefensiveModifiers)
    {
        PlayerCharacterController dwarfController = PlayerCharacterController.Instance;
        PlayedCardsController playedCardsController = PlayedCardsController.Instance;
        CardController cardController = GetComponent<CardController>();
        foreach (var modifier in temporaryOffensiveModifiers)
        {            
            accumulatedElementalPowers[modifier.attackElement] += modifier.damage;            
        }
        foreach (var modifier in temporaryDefensiveModifiers)
        {            
            accumulatedElementalPowers[modifier.resistanceElement] += modifier.damage;
        }
        // Una vez obtenidos los efectos acumulados, crea el escudo
        dwarfController.CreateShield(cardData.duration, cardData.power, accumulatedElementalPowers);
        // Reseteo el diccionario porque representa que se gastan los efectos en esta habilidad
        playedCardsController.ClearTemporaryModifiers();
        accumulatedElementalPowers = new Dictionary<Element, int>
        {
            { Element.Fire, 0 },
            { Element.Ice, 0 },
            { Element.Electric, 0 },
            { Element.Water, 0 },
            { Element.Nature, 0 },
            { Element.Earth, 0 }
        };
        cardController.IsProcesed = true;
    }
    public void NormalGemAbility(CardData cardData, List<TemporaryModifier> temporaryOffensiveModifiers, List<TemporaryModifier> temporaryDefensiveModifiers)
    {
        PlayerCharacterController dwarfController = PlayerCharacterController.Instance;
        PlayedCardsController playedCardsController = PlayedCardsController.Instance;
        CardController cardController = GetComponent<CardController>();
        Dictionary<Element, int> elementalDamages = cardData.GetElementalDamages();
        Dictionary<Element, int> elementalResistances = cardData.GetElementalResistances();

        bool mode = playedCardsController.GetCardEffectType(playedCardsController.GetNextCardsList(transform));
        
        if (!mode)
        {
            if (cardData.lifeSteal > 0)
            {
                TemporaryModifier lifeStealModifier = new TemporaryModifier { lifeSteal = cardData.lifeSteal };
                playedCardsController.AddTemporaryOffensiveModifier(lifeStealModifier, transform.position);
                Debug.Log($"{cardData.cardName} effect applied: increased dwarf lifeSteal to = {cardData.lifeSteal}%");
            }

            foreach (var element in elementalDamages)
            {
                TemporaryModifier modifier = new TemporaryModifier
                {
                    attackElement = element.Key,
                    damage = element.Value,
                    remainingTurns = cardData.duration,
                };
                playedCardsController.AddTemporaryOffensiveModifier(modifier, transform.position);
            }
            Debug.Log($"{cardData.cardName} effect applied: {string.Join(", ", elementalDamages.Select(e => $"{e.Key}: {e.Value}"))}");                     
        }
        else
        {
            foreach (var element in elementalResistances)
            {
                TemporaryModifier modifier = new TemporaryModifier
                {
                    resistanceElement = element.Key,
                    damage = element.Value,
                    remainingTurns = cardData.duration,
                };
                playedCardsController.AddTemporaryDefensiveModifier(modifier, transform.position);
            }
            Debug.Log($"{cardData.cardName} effect applied: {string.Join(", ", elementalResistances.Select(e => $"{e.Key}: {e.Value}"))}");
        }         
    }

    public void TNTEffect(CardData cardData, List<TemporaryModifier> temporaryOffensiveModifiers, List<TemporaryModifier> temporaryDefensiveModifiers)
    {
        GolemController golemController = GolemController.Instance;
        PlayerCharacterController playerDwarfController = PlayerCharacterController.Instance;
        CardController cardController = GetComponent<CardController>();
        CombatManager combatManager = CombatManager.Instance;
        List<Transform> entities = combatManager.GetFieldEntities();
        GameObject middlePanel = GameObject.Find("Middle Panel");
        Transform middlePanelTransform = middlePanel.transform;

        // Coordenadas desde/hasta para lanzar la TNT
        Vector2 from = playerDwarfController.transform.localPosition;
        Vector2 to = golemController.transform.localPosition;

        LoadPrefab("ProjectilePrefab", (projectileGO) =>
        {
            if (projectileGO == null) return;

            LoadSprite("TNTSprite", (sprite) =>
            {
                if (sprite == null)
                {
                    Destroy(projectileGO);
                    return;
                }

                // Inicializar
                ProjectileEntityController projectileController = projectileGO.GetComponent<ProjectileEntityController>();
                projectileController.Initialize(sprite);

                // Posicionar en el canvas (o donde toque)
                projectileGO.transform.SetParent(middlePanelTransform, false);
                // Efecto flicker de luz
                projectileController.StartTNTFlicker();
                // Animar proyectil con parábola
                SoundsFXManager.Instance.PlayTNTSound();
                projectileController.ParabolicLaunch(from, to, 1.5f, 500f, 360f, () =>
                {
                    
                    // Impacto: daño a los entities del campo
                    if (entities != null && entities.Count > 0)
                    {
                        foreach (Transform entity in entities)
                        {

                            if (entity != null)
                            {
                                Debug.Log("Hay algun entity NULL");
                                FieldEntityController entityController = entity.GetComponent<FieldEntityController>();
                                entityController.TakeHit();
                            }
                        }
                    }

                    // Daño elemental al golem
                    Dictionary<Element, int> elementalPower = cardData.GetElementalDamages();
                    StartCoroutine(golemController.TakeDamage(0, elementalPower, null, () =>
                    {
                        cardController.IsProcesed = true;
                    }));

                    Destroy(projectileGO);
                });
            });
        });
    }
    public void WaterBottleEffect(CardData cardData, List<TemporaryModifier> temporaryOffensiveModifiers, List<TemporaryModifier> temporaryDefensiveModifiers)
    {
        GolemController golemController = GolemController.Instance;
        PlayerCharacterController playerDwarfController = PlayerCharacterController.Instance;
        CardController cardController = GetComponent<CardController>();
        GameObject middlePanel = GameObject.Find("Middle Panel");
        SummonableUnitData unitData = cardData.summonableData;
        Transform middlePanelTransform = middlePanel.transform;

        Vector2 from = playerDwarfController.transform.localPosition;
        Vector2 to = golemController.transform.localPosition;

        LoadPrefab("ProjectilePrefab", (projectileGO) =>
        {
            if (projectileGO == null) return;

            LoadSprite("WaterBottleSprite", (sprite) =>
            {
                if (sprite == null)
                {
                    Destroy(projectileGO);
                    return;
                }

                // Inicializar
                ProjectileEntityController projectileController = projectileGO.GetComponent<ProjectileEntityController>();
                projectileController.Initialize(sprite);

                // Posicionar en el canvas (o donde toque)
                projectileGO.transform.SetParent(middlePanelTransform, false);
                // Animar proyectil con parábola
                projectileController.ParabolicLaunch(from, to, 1f, 500f, 360f, () =>
                {
                    GolemStatusEffect effect = GolemStatusEffectCatalog.CreateEffect("Wet", 100f, 4);
                    golemController.ApplyStatusEffect(effect);
                    Destroy(projectileGO);
                    cardController.IsProcesed = true;
                });
            });
        });       
    }
    public void TurretEffect(CardData cardData, List<TemporaryModifier> temporaryOffensiveModifiers, List<TemporaryModifier> temporaryDefensiveModifiers)
    {
        GolemController golemController = GolemController.Instance;
        PlayerCharacterController playerDwarfController = PlayerCharacterController.Instance;
        CardController cardController = GetComponent<CardController>();
        GameObject middlePanel = GameObject.Find("Middle Panel");
        SummonableUnitData unitData = cardData.summonableData;
        Transform middlePanelTransform = middlePanel.transform;

        LoadPrefab("SummonableUnit", (unit) =>
        {
            if (unit == null) return;

            // Espaciado fijo entre torretas
            float spacing = 45f; // ajusta según tamaño de la torreta
            int existingUnits = playerDwarfController.GetSummonableUnits().Count;

            // Calcula la posición para esta torreta, centrando todas alrededor de la base
            Vector3 basePosition = unitData.targetPosition;
            float offset = (existingUnits - (existingUnits / 2f)) * spacing;
            Vector3 finalPosition = basePosition + new Vector3(offset, 0, 0);

            // Asignar posición y padre
            unit.transform.localPosition = finalPosition;
            unit.transform.SetParent(middlePanelTransform);

            SummonableUnitController unitController = unit.GetComponent<SummonableUnitController>();
            unitController.Initialize(unitData);
            cardController.IsProcesed = true;
        });
    }







     public void LoadPrefab(string address, Action<GameObject> onLoaded)
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(address);
        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject instance = Instantiate(op.Result);
                onLoaded?.Invoke(instance);
            }
            else
            {
                Debug.LogError($"Error al cargar prefab '{address}'");
                onLoaded?.Invoke(null);
            }
        };
    }

    public void LoadSprite(string address, Action<Sprite> onLoaded)
    {
        var handle = Addressables.LoadAssetAsync<Sprite>(address);
        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                onLoaded?.Invoke(op.Result);
            }
            else
            {
                Debug.LogError($"Error al cargar sprite '{address}'");
                onLoaded?.Invoke(null);
            }
        };
    }
    

}
