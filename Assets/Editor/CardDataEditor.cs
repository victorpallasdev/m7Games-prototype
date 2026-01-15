using UnityEditor;

[CustomEditor(typeof(CardData))]
public class CardDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CardData card = (CardData)target;

        // Siempre visibles
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cardName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cardImage"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cardType"));        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cardAbility"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cardLevel"));
        CardType type = card.cardType;

        if (type == CardType.Gem)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defensiveMode"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("offensiveMode"));
        }
        else
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cardDescription"));
        }      

        // Mostrar duration y power solo si es DefensiveGem o Weapon
        if (type == CardType.DefensiveGem || type == CardType.Weapon || type == CardType.Gadget)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("duration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("power"));
        }

        // Mostrar cardLevel, lifeSteal y upgradedCard para Gem, DefensiveGem, Weapon y Support
        if (type == CardType.Gem || type == CardType.DefensiveGem || type == CardType.Weapon || type == CardType.Support)
        {
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("upgradedCard"));        
        }
        if (type == CardType.Gem ||type == CardType.Weapon)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("lifeSteal"));
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("deffenseText"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("attackText"));

        if (type == CardType.Weapon)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
        }

        if (type == CardType.Gadget)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("summonableData"));
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultTextType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("elementalDamagesList"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("elementalResistancesList"), true);

        serializedObject.ApplyModifiedProperties();
    }
}
