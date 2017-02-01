using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WizardryConnect
{
    public static class GUILayoutHelper
    {
        public delegate void VoidDelegate();

        public static bool Foldout(bool toggle, GUIContent label, VoidDelegate callback)
        {
            var rect = GUILayoutUtility.GetRect(new GUIContent("\t" + label.text), GUIStyle.none);
            bool result = EditorGUI.Foldout(rect, toggle, label, true);
            if (result)
                callback();
            return result;
        }

        public static void EnableGroup(bool enabled, VoidDelegate callback)
        {
            EditorGUI.BeginDisabledGroup(!enabled);
            callback();
            EditorGUI.EndDisabledGroup();
        }

        public static void Indent(VoidDelegate callback)
        {
            EditorGUI.indentLevel++;
            callback();
            EditorGUI.indentLevel--;
        }

        public static void Indent(int levels, VoidDelegate callback)
        {
            EditorGUI.indentLevel += levels;
            callback();
            EditorGUI.indentLevel -= levels;
        }

        public static void Horizontal(VoidDelegate callback)
        {
            EditorGUILayout.BeginHorizontal();
            callback();
            EditorGUILayout.EndHorizontal();
        }

        public static void Vertical(VoidDelegate callback)
        {
            EditorGUILayout.BeginVertical();
            callback();
            EditorGUILayout.EndVertical();
        }

        public static Vector2 ScrollView(Vector2 scrollPosition, VoidDelegate callback)
        {
            var newScrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            callback();
            EditorGUILayout.EndScrollView();
            return newScrollPosition;
        }
    }
}
