using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WizardryConnect.Utility;

namespace WizardryConnect
{
    [CustomEditor(typeof(Wizardry8Unity))]
    public class Wizardry8UnityEditor : Editor
    {

        private Wizardry8Unity w8Unity { get { return target as Wizardry8Unity; } }

        private int SelectedPopUpIndex = 0;

        private const string showImportFoldout = "Wizardry8Unity_ShowImportFoldout";
        private static bool ShowImportFoldout
        {
            get { return EditorPrefs.GetBool(showImportFoldout, true); }
            set { EditorPrefs.SetBool(showImportFoldout, value); }
        }

        SerializedProperty Prop(string name)
        {
            return serializedObject.FindProperty(name);
        }

        public override void OnInspectorGUI()
        {
            // Update
            w8Unity.EditorUpdate();

            // Get properties
            var propWizardry8Path = Prop("Wizardry8Path");

            // Browse for Wizardry8 path
            EditorGUILayout.Space();
            GUILayoutHelper.Horizontal(() =>
            {
                EditorGUILayout.LabelField(new GUIContent("Wizardry8 Path", "The local Wizardry8 path used for development only."), GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                EditorGUILayout.SelectableLabel(w8Unity.Wizardry8Path, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                if (GUILayout.Button("Browse..."))
                {
                    string path = EditorUtility.OpenFolderPanel("Locate Wizardry8 Path", "", "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (!Wizardry8Unity.ValidateW8Path(path))
                        {
                            EditorUtility.DisplayDialog("Invalid Path", "The selected Wizardry8 path is invalid", "Close");
                        } else
                        {
                            w8Unity.Wizardry8Path = path;
                            propWizardry8Path.stringValue = path;
                            w8Unity.EditorResetW8Path();
                        }
                    }
                }
                if (GUILayout.Button("Clear"))
                {
                    w8Unity.EditorClearW8Path();
                    EditorUtility.SetDirty(target);
                }
            });

            // Prompt user to set Wizardry8 path
            if (string.IsNullOrEmpty(w8Unity.Wizardry8Path))
            {
                EditorGUILayout.HelpBox("Please set the Wizardry8 path of your Wizardry installation.", MessageType.Info);
                return;
            }

            // Display other GUI items
            //DisplayOptionsGUI();
            DisplayImporterGUI();

            // Save modified properties
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }


        private void DisplayImporterGUI()
        {
            // Hide importer GUI when not active in hierarchy
            if (!w8Unity.gameObject.activeInHierarchy)
                return;

            EditorGUILayout.Space();
            ShowImportFoldout = GUILayoutHelper.Foldout(ShowImportFoldout, new GUIContent("Importer"), () =>
            {
                GUILayoutHelper.Indent(() =>
                {
                    EditorGUILayout.Space();
                    var propModelName = Prop("Item3DImporter_ModelName");
                    var propModelID = Prop("Item3DImporter_ModelNameID");
                    EditorGUILayout.LabelField(new GUIContent("Item3D", "Enter name of model."));
                    GUILayoutHelper.Horizontal(() =>
                    {
                        // propModelName.stringValue = EditorGUILayout.TextField(propModelName.stringValue.Trim().ToUpper());
                        propModelID.intValue = EditorGUILayout.Popup(propModelID.intValue, w8Unity.ContentReader.GetItem3DList);
                        propModelName.stringValue = w8Unity.ContentReader.GetItem3DList[propModelID.intValue];
                        if (GUILayout.Button("Import"))
                        {
                            Debug.Log("Loading Asset: " + propModelName.stringValue);
                            GameObjectHelper.CreateWizardryMeshGameObject(propModelName.stringValue, null);
                        }
                    });

                    EditorGUILayout.Space();
                    
                });
            });
        }
    }

}
