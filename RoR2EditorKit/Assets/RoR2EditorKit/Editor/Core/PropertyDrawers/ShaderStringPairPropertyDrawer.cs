﻿using UnityEngine;
using UnityEditor;
using RoR2EditorKit.Settings;

namespace RoR2EditorKit.Core.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(MaterialEditorSettings.ShaderStringPair))]
    public class ShaderStringPairPropertyDrawer : ExtendedPropertyDrawer
    {
        protected override void DrawCustomDrawer()
        {
            Begin();
            var objRefProperty = property.FindPropertyRelative("shader");
            objRefProperty.objectReferenceValue = EditorGUI.ObjectField(rect, NicifyName(property.FindPropertyRelative("shaderName").stringValue), objRefProperty.objectReferenceValue, typeof(Shader), false);
            End();
        }
    }
}
