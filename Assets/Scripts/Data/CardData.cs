using UnityEngine;
using System.Collections.Generic;

public enum CardType
{
    Gem,
    DefensiveGem,
    Weapon,
    Support,
    Gadget
}
public enum Element
{
    Fire,
    Electric,
    Ice,
    Water,
    Nature,
    Earth
}

[System.Serializable]
public class ElementalPower
{
    public Element element;
    public int value; // Valor del daño
}

[CreateAssetMenu(fileName = "NewCardData", menuName = "CardData")]
[System.Serializable]
public class CardData : ScriptableObject
{
    public string cardName; // Nombre de la carta
    public Sprite cardImage; // Imagen de la carta
    public CardType cardType; // Tipo de carta
    public string cardDescription; // Descripción de la carta
    public string defensiveMode;
    public string offensiveMode;
    public string cardAbility;
    public int duration;
    public int power; // Daño físico base
    public int cardLevel;
    public int lifeSteal;
    public string attackText;
    public string deffenseText;
    public bool defaultTextType;
    public Sprite icon;
    public CardData upgradedCard;
    public SummonableUnitData summonableData;
    [SerializeField]public List<ElementalPower> elementalDamagesList = new List<ElementalPower>();
    [SerializeField]public List<ElementalPower> elementalResistancesList = new List<ElementalPower>();
    private Dictionary<Element, int> elementalDamages;
    private Dictionary<Element, int> elementalResistances;


    private void OnEnable()
    {
        elementalDamages = new Dictionary<Element, int>();
        elementalResistances = new Dictionary<Element, int>();
        foreach (var damage in elementalDamagesList)
        {
            elementalDamages[damage.element] = damage.value;
        }
         foreach (var resistance in elementalResistancesList)
        {
            elementalResistances[resistance.element] = resistance.value;
        }
    }

    // Metodo get del diccionario
    public Dictionary<Element, int> GetElementalDamages()
    {
        return elementalDamages;
    }
    public Dictionary<Element, int> GetElementalResistances()
    {
        return elementalResistances;
    }
}


