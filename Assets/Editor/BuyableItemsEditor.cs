using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BuyableItemData))]
public class BuyableItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        BuyableItemData item = (BuyableItemData)target;

        // Siempre visibles
        EditorGUILayout.PropertyField(serializedObject.FindProperty("id"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemSprite"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("cost"));        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("itemType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));

        BuyableItemType type = item.itemType;

        if (type == BuyableItemType.GemPack)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gemAmount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("possibleGems"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("rarityChances"));
        }
            
        
        if (type == BuyableItemType.WeaponBox)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("weaponAmount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("possibleWeapons"));
        }

        if (type == BuyableItemType.GadgetBox)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gadgetAmount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("possibleGadgets"));
        }

        if (type == BuyableItemType.ConsumableCard)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("consumableData"));
        }

        if (type == BuyableItemType.Buff)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("buffValue"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("roundsDuration"));
        }

        if (type == BuyableItemType.Healing)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("healingAmount"));
        }

        if (type == BuyableItemType.Vouche)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fusionatorVouche"));
        }

        if (type == BuyableItemType.Gear)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gearData"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
