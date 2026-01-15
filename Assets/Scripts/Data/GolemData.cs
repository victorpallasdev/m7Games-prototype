using UnityEngine;
using System.Collections.Generic;



[CreateAssetMenu(fileName = "NewGolemData", menuName = "GolemData")]
public class GolemData : ScriptableObject
{
    [Header("Base Stats")]
    public string golemName; // Nombre del golem
    public string id;
    public int golemPower; // Daño físico básico
    public Sprite golemSprite; // Sprite base del golem
    public RuntimeAnimatorController animatorController; // Controlador de animaciones del golem
    public List<int> maxHealthbarvalues;

    [Header("Elemental Attributes")]
    public List<ElementalPower> elementalDamagesList = new List<ElementalPower>(); // Daños elementales del golem
    public List<ElementalPower> elementalResistances = new List<ElementalPower>(); // Resistencias del golem
    public List<ElementalPower> elementalWeaknesses = new List<ElementalPower>(); // Debilidades del golem

    [Header("Special Ability")]
    public string specialAbilityName; // Nombre de la habilidad especial
    public string specialAbilityMethodName; // Nombre exacto del metodo de la especial
    public string specialAbilityDescription; // Descripción de la habilidad especial
    public float specialAttackMultiplier = 2.0f; // Multiplicador del ataque especial
    public int specialAttackCooldown = 3; // Turnos necesarios para el ataque especial

    public static readonly List<Color> BarColors = new List<Color>
    {
        new Color(0.6f, 0.1f, 0.1f),   // Dark Red
        new Color(0.6f, 0.2f, 0.6f),   // Purple
        new Color(0.1f, 0.3f, 0.3f),   // Dark Teal
        new Color(0.1f, 0.4f, 0.1f),   // Dark Green
        new Color(0.1f, 0.1f, 0.6f),   // Dark Blue
        new Color(0.4f, 0.7f, 0.9f),   // Light Blue
        new Color(0.9f, 0.9f, 0.2f),   // Yellow
        new Color(0.8f, 0.7f, 0.2f),   // Mid Yellow
        new Color(0.9f, 0.5f, 0.2f),   // Mid Orange
        new Color(0.9f, 0.3f, 0.1f),   // Orange
        new Color(0.8f, 0.1f, 0.1f)    // Red
    };
    
}
