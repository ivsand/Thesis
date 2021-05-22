using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SetupInitialConnections))]
public class SetupInitialConnectionsEditor : Editor
{
    SetupInitialConnections _setup;

    SerializedProperty _fromNodes;
    SerializedProperty _toNodes;

    void OnEnable()
    {
        _setup = (SetupInitialConnections)target;
        _fromNodes = serializedObject.FindProperty("fromNodes");
        _toNodes = serializedObject.FindProperty("toNodes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        int size = _setup.size;
        _setup.size = EditorGUILayout.IntSlider("# of Connections", size, 0, 50);

        int fromCount = _setup.fromNodes.Count;
        int toCount = _setup.toNodes.Count;

        EditorGUILayout.LabelField("Correct Connections:");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        if (_setup.fromNodes != null)
        {
            for (int i = 0; i < fromCount; i++)
            {
                EditorGUILayout.PropertyField(_fromNodes.GetArrayElementAtIndex(i), GUIContent.none);
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        if (_setup.fromNodes != null)
        {
            for (int i = 0; i < fromCount; i++)
            {
                EditorGUILayout.LabelField("——(" + i + ")——>", GUILayout.MaxWidth(80));
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        if (_setup.toNodes != null)
        {
            for (int i = 0; i < toCount; i++)
            {
                EditorGUILayout.PropertyField(_toNodes.GetArrayElementAtIndex(i), GUIContent.none);
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        if (fromCount < size)
        {
            for (int i = fromCount - 1; i < size; i++)
            {
                _setup.fromNodes.Add(null);
            }
        }
        else if (fromCount > size)
        {
            for (int i = fromCount - 1; i >= size; i--)
            {
                _setup.fromNodes.RemoveAt(i);
            }
        }
        if (toCount < size)
        {
            for (int i = toCount - 1; i < size; i++)
            {
                _setup.toNodes.Add(null);
            }
        }
        else if (toCount > size)
        {
            for (int i = toCount - 1; i >= size; i--)
            {
                _setup.toNodes.RemoveAt(i);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
