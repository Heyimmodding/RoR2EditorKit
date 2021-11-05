﻿using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Core.Windows
{
    /// <summary>
    /// Derive editor windows from this class.
    /// <para>Based on the video "Easy Editor Windows in Unity with Serialized Properties" by "Game Dev Guide"</para>
    /// <para>https://www.youtube.com/watch?v=c_3DXBrH-Is</para>
    /// </summary>
    public class ExtendedEditorWindow : EditorWindow
    {
        protected SerializedObject mainSerializedObject;
        protected SerializedProperty mainCurrentProperty;

        private string mainSelectedPropertyPath;
        protected SerializedProperty mainSelectedProperty;

        public static void OpenEditorWindow<T>(Object unityObject, string windowName) where T : ExtendedEditorWindow
        {
            T window = GetWindow<T>(windowName);
            if (unityObject != null)
                window.mainSerializedObject = new SerializedObject(unityObject);

            window.OnWindowOpened();
        }
        protected virtual void OnWindowOpened() { }
        protected void DrawProperties(SerializedProperty property, bool drawChildren)
        {
            string lastPropPath = string.Empty;
            foreach (SerializedProperty prop in property)
            {
                if (prop.isArray && prop.propertyType == SerializedPropertyType.Generic)
                {
                    EditorGUILayout.BeginHorizontal();
                    prop.isExpanded = EditorGUILayout.Foldout(prop.isExpanded, prop.displayName);
                    EditorGUILayout.EndHorizontal();

                    if (prop.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        DrawProperties(prop, drawChildren);
                        EditorGUI.indentLevel--;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(lastPropPath) && prop.propertyPath.Contains(lastPropPath))
                    {
                        continue;
                    }
                    lastPropPath = prop.propertyPath;
                    EditorGUILayout.PropertyField(prop, drawChildren);
                }
            }
        }

        #region Button Sidebars
        protected bool DrawButtonSidebar(SerializedProperty property)
        {
            bool pressed = false;

            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            foreach (SerializedProperty prop in property)
            {
                if (GUILayout.Button(prop.displayName))
                {
                    mainSelectedPropertyPath = prop.propertyPath;
                    GUI.FocusControl(null);
                    pressed = true;
                }
            }
            if (!string.IsNullOrEmpty(mainSelectedPropertyPath))
            {
                mainSelectedProperty = mainSerializedObject.FindProperty(mainSelectedPropertyPath);
            }
            return pressed;
        }

        protected (Vector2, bool) DrawScrollableButtonSidebar(SerializedProperty property, Vector2 scrollPosition)
        {
            bool pressed = false;
            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(300));

            if (property.arraySize != 0)
            {
                foreach (SerializedProperty prop in property)
                {
                    if (GUILayout.Button(prop.displayName))
                    {
                        mainSelectedPropertyPath = prop.propertyPath;
                        GUI.FocusControl(null);
                        pressed = true;
                    }
                }
                if (!string.IsNullOrEmpty(mainSelectedPropertyPath))
                {
                    mainSelectedProperty = mainSerializedObject.FindProperty(mainSelectedPropertyPath);
                }

            }
            else
            {
                EditorGUILayout.LabelField($"Increase {property.name}'s Size.");
            }
            EditorGUILayout.EndScrollView();
            return (scrollPosition, pressed);
        }

        protected bool DrawButtonSidebar(SerializedProperty property, string buttonName)
        {
            bool pressed = false;

            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            foreach (SerializedProperty prop in property)
            {
                var p = prop.FindPropertyRelative(buttonName);
                if (p != null && p.objectReferenceValue)
                {
                    if (p.objectReferenceValue && GUILayout.Button(p.objectReferenceValue.name))
                    {
                        mainSelectedPropertyPath = prop.propertyPath;
                        GUI.FocusControl(null);
                        pressed = true;
                    }
                }
                else if (GUILayout.Button(prop.displayName))
                {
                    mainSelectedPropertyPath = prop.propertyPath;
                    GUI.FocusControl(null);
                    pressed = true;
                }
            }
            if (!string.IsNullOrEmpty(mainSelectedPropertyPath))
            {
                mainSelectedProperty = mainSerializedObject.FindProperty(mainSelectedPropertyPath);
            }
            return pressed;
        }

        protected (Vector2, bool) DrawScrollableButtonSidebar(SerializedProperty property, Vector2 scrollPosition, string buttonName)
        {
            bool pressed = false;

            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(300));

            if (property.arraySize != 0)
            {
                foreach (SerializedProperty prop in property)
                {
                    var p = prop.FindPropertyRelative(buttonName);
                    if (p != null && p.objectReferenceValue)
                    {
                        if (p.objectReferenceValue && GUILayout.Button(p.objectReferenceValue.name))
                        {
                            mainSelectedPropertyPath = prop.propertyPath;
                            GUI.FocusControl(null);
                            pressed = true;
                        }
                    }
                    else if (GUILayout.Button(prop.displayName))
                    {
                        mainSelectedPropertyPath = prop.propertyPath;
                        GUI.FocusControl(null);
                        pressed = true;
                    }
                }
                if (!string.IsNullOrEmpty(mainSelectedPropertyPath))
                {
                    mainSelectedProperty = mainSerializedObject.FindProperty(mainSelectedPropertyPath);
                }

            }
            else
            {
                EditorGUILayout.LabelField($"Increase {property.name}'s Size.");
            }
            EditorGUILayout.EndScrollView();
            return (scrollPosition, pressed);
        }

        protected bool DrawButtonSidebar(SerializedProperty property, ref string selectedPropPath, ref SerializedProperty selectedProperty)
        {
            bool pressed = false;

            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            foreach (SerializedProperty prop in property)
            {
                if (GUILayout.Button(prop.displayName))
                {
                    selectedPropPath = prop.propertyPath;
                    GUI.FocusControl(null);
                    pressed = true;
                }
            }
            if (selectedPropPath != string.Empty)
            {
                selectedProperty = mainSerializedObject.FindProperty(selectedPropPath);
            }
            return pressed;
        }

        protected (Vector2, bool) DrawScrollableButtonSidebar(SerializedProperty property, Vector2 scrollPosition, ref string selectedPropPath, ref SerializedProperty selectedProperty)
        {
            bool pressed = false;

            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(300));

            foreach (SerializedProperty prop in property)
            {
                if (GUILayout.Button(prop.displayName))
                {
                    selectedPropPath = prop.propertyPath;
                    GUI.FocusControl(null);
                    pressed = true;
                }
            }
            if (!string.IsNullOrEmpty(selectedPropPath))
            {
                selectedProperty = mainSerializedObject.FindProperty(selectedPropPath);
            }
            EditorGUILayout.EndScrollView();
            return (scrollPosition, pressed);
        }

        protected bool DrawButtonSidebar(SerializedProperty property, string propertyNameForButton, ref string selectedPropPath, ref SerializedProperty selectedProperty)
        {
            bool pressed = false;

            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            foreach (SerializedProperty prop in property)
            {
                var p = prop.FindPropertyRelative(propertyNameForButton);
                if (p != null && p.objectReferenceValue)
                {
                    if (p.objectReferenceValue && GUILayout.Button(p.objectReferenceValue.name))
                    {
                        selectedPropPath = prop.propertyPath;
                        GUI.FocusControl(null);
                        pressed = true;
                    }
                }
                else if (GUILayout.Button(prop.displayName))
                {
                    selectedPropPath = prop.propertyPath;
                    GUI.FocusControl(null);
                    pressed = true;
                }
            }
            if (selectedPropPath != string.Empty)
            {
                selectedProperty = mainSerializedObject.FindProperty(selectedPropPath);
            }
            return pressed;
        }

        protected (Vector2, bool) DrawScrollableButtonSidebar(SerializedProperty property, Vector2 scrollPosition, string propertyNameForButton, ref string selectedPropPath, ref SerializedProperty selectedProperty)
        {
            bool pressed = false;

            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(300));

            foreach (SerializedProperty prop in property)
            {
                var p = prop.FindPropertyRelative(propertyNameForButton);
                if (p != null && p.objectReferenceValue)
                {
                    if (p.objectReferenceValue && GUILayout.Button(p.objectReferenceValue.name))
                    {
                        selectedPropPath = prop.propertyPath;
                        GUI.FocusControl(null);
                        pressed = true;
                    }
                }
                else if (GUILayout.Button(prop.displayName))
                {
                    selectedPropPath = prop.propertyPath;
                    GUI.FocusControl(null);
                    pressed = true;
                }
            }
            if (!string.IsNullOrEmpty(selectedPropPath))
            {
                selectedProperty = mainSerializedObject.FindProperty(selectedPropPath);
            }
            EditorGUILayout.EndScrollView();
            return (scrollPosition, pressed);
        }
        #endregion

        #region Value Sidebars
        protected void DrawValueSidebar(SerializedProperty property)
        {
            property.arraySize = EditorGUILayout.DelayedIntField($"Array Size", property.arraySize);

            foreach (SerializedProperty prop in property)
            {
                DrawField(prop, true);
            }
        }
        #endregion

        #region Button Creation
        protected bool SimpleButton(string buttonName, params GUILayoutOption[] options)
        {
            return GUILayout.Button(buttonName, options);
        }

        protected bool SwitchButton(string buttonName, ref bool switchingBool, params GUILayoutOption[] options)
        {
            var button = GUILayout.Button(buttonName, options);
            if (button)
                switchingBool = !switchingBool;

            return button;
        }
        #endregion
        protected void DrawField(string propName, bool relative)
        {
            if (relative && mainCurrentProperty != null)
            {
                EditorGUILayout.PropertyField(mainCurrentProperty.FindPropertyRelative(propName), true);
            }
            else if (mainSerializedObject != null)
            {
                EditorGUILayout.PropertyField(mainSerializedObject.FindProperty(propName), true);
            }
        }

        protected void DrawField(SerializedProperty property, bool includeChildren, string label = null)
        {
            if (label != null)
                EditorGUILayout.PropertyField(property, new GUIContent(label), includeChildren);
            else
                EditorGUILayout.PropertyField(property, includeChildren);
        }

        protected void DrawField(string propName, bool relative, SerializedProperty currentProp, SerializedObject serializedObj)
        {
            if (relative && currentProp != null)
            {
                EditorGUILayout.PropertyField(currentProp.FindPropertyRelative(propName), true);
            }
            else if (mainSerializedObject != null)
            {
                EditorGUILayout.PropertyField(serializedObj.FindProperty(propName), true);
            }
        }
        
        protected void DrawField(string propName, SerializedObject serializedObject)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(propName));
        }

        protected void ApplyChanges()
        {
            mainSerializedObject.ApplyModifiedProperties();
        }
    }
}