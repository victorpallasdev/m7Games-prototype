#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FusionMatrix))]
public class FusionMatrixEditor : Editor
{
    SerializedProperty pGemCards;
    SerializedProperty pEntries;
    Vector2 scroll;

    private void OnEnable()
    {
        pGemCards = serializedObject.FindProperty("gemCards");
        pEntries  = serializedObject.FindProperty("entries");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 1) Lista de gemCards
        EditorGUILayout.LabelField("1) Gemas disponibles", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(pGemCards, true);
        EditorGUILayout.Space();

        // 2) Tabla de combinaciones
        EditorGUILayout.LabelField("2) Define el resultado de cada fusión", EditorStyles.boldLabel);
        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(200));

        for (int i = 0; i < pEntries.arraySize; i++)
        {
            var entry = pEntries.GetArrayElementAtIndex(i);
            var pA = entry.FindPropertyRelative("gemA");
            var pB = entry.FindPropertyRelative("gemB");
            var pr = entry.FindPropertyRelative("result");

            EditorGUILayout.BeginHorizontal();
            // Mostramos los nombres pero deshabilitados (no editables)
            GUI.enabled = false;
            EditorGUILayout.ObjectField(pA, GUIContent.none, GUILayout.Width(100));
            GUILayout.Label("+", GUILayout.Width(10));
            EditorGUILayout.ObjectField(pB, GUIContent.none, GUILayout.Width(100));
            GUI.enabled = true;

            GUILayout.Label("=", GUILayout.Width(10));
            // Aquí sí puedes asignar el result
            EditorGUILayout.PropertyField(pr, GUIContent.none, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
