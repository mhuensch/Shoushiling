using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;

namespace TRS.CaptureTool.Extras
{
    /*
     * Reference:
     * https://github.com/Unity-Technologies/UnityCsReference
     * https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/EditorGUI.cs
     */

    public static class CustomEditorGUI
    {
        public static void PropertyField(Rect position, SerializedProperty property, bool includeChildren = false)
        {
            PropertyField(position, property, new GUIContent(property.displayName), includeChildren);
        }

        public static void PropertyField(Rect position, SerializedProperty property, GUIContent label, bool includeChildren = false)
        {
            PropertyField(position, property, label, EditorGUI.indentLevel, includeChildren);
        }

        public static void PropertyField(Rect position, SerializedProperty property, GUIContent label, int indentLevel, bool includeChildren = false)
        {
            if (includeChildren || property.propertyType == SerializedPropertyType.Generic)
            {
                property.isExpanded = GUILayout.Toggle(property.isExpanded, label, "foldout");
                if (includeChildren && property.isExpanded)
                {
                    int originalIndentLevel = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = indentLevel;

                    ++indentLevel;
                    if (property.isArray)
                        property.arraySize = EditorGUI.DelayedIntField(position, new GUIContent("Size"), property.arraySize);
                    foreach (SerializedProperty childProperty in property)
                        PropertyField(position, childProperty, new GUIContent(childProperty.displayName), indentLevel, false);// includeChildren && childProperty.hasVisibleChildren);
                    EditorGUI.indentLevel = originalIndentLevel;
                }

                return;
            }

            MethodInfo method = null;
            switch (property.propertyType)
            {
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = EditorGUI.CurveField(position, label, property.animationCurveValue);
                    break;
                case SerializedPropertyType.ArraySize:
                    property.intValue = EditorGUI.DelayedIntField(position, label, property.intValue);
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = EditorGUI.Toggle(position, label, property.boolValue);
                    break;
                case SerializedPropertyType.Bounds:
                    property.boundsValue = EditorGUI.BoundsField(position, label, property.boundsValue);
                    break;
                case SerializedPropertyType.Character:
                    string newValue = EditorGUI.TextField(position, label, new string(new char[] { (char)property.intValue }));
                    property.intValue = newValue.Length > 0 ? newValue[0] : '\0';
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = EditorGUI.ColorField(position, label, property.colorValue);
                    break;
                case SerializedPropertyType.Enum:
                    GUIContent[] displayNames = property.enumDisplayNames.Select(name => new GUIContent(name)).ToArray();
                    property.enumValueIndex = EditorGUI.Popup(position, label, property.enumValueIndex, displayNames);
                    break;
#if UNITY_2017_OR_NEWER
                case SerializedPropertyType.ExposedReference:
                    property.exposedReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, ReflectionExtensions.GetType(property), true);
                    break
#endif
                case SerializedPropertyType.Float:
                    property.floatValue = EditorGUI.FloatField(position, label, property.floatValue);
                    break;
                case SerializedPropertyType.Integer:
                    property.intValue = EditorGUI.IntField(position, label, property.intValue);
                    break;
                case SerializedPropertyType.LayerMask:
                    method = typeof(EditorGUI).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).First(t => t.Name == "LayerMaskField");
                    method.Invoke(null, new object[] { position, property, label });
                    break;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, ReflectionExtensions.GetType(property), true);
                    break;
                case SerializedPropertyType.Quaternion:
                    Quaternion quaternion = property.quaternionValue;
                    Vector4 quaternionValues = new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
                    quaternionValues = EditorGUI.Vector4Field(position, label, quaternionValues);
                    property.quaternionValue = new Quaternion(quaternionValues.x, quaternionValues.y, quaternionValues.z, quaternionValues.w);
                    break;
                case SerializedPropertyType.Rect:
                    property.rectValue = EditorGUI.RectField(position, label, property.rectValue);
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = EditorGUI.TextField(position, label, property.stringValue);
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = EditorGUI.Vector2Field(position, label, property.vector2Value);
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = EditorGUI.Vector3Field(position, label, property.vector3Value);
                    break;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = EditorGUI.Vector4Field(position, label, property.vector4Value);
                    break;

#if UNITY_2017_2_OR_NEWER
                case SerializedPropertyType.BoundsInt:
                    property.boundsIntValue = EditorGUI.BoundsIntField(position, label, property.boundsIntValue);
                    break;
                case SerializedPropertyType.RectInt:
                    property.rectIntValue = EditorGUI.RectIntField(position, label, property.rectIntValue);
                    break;
                case SerializedPropertyType.Vector2Int:
                    property.vector2IntValue = EditorGUI.Vector2IntField(position, label, property.vector2IntValue);
                    break;
                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue = EditorGUI.Vector3IntField(position, label, property.vector3IntValue);
                    break;
#endif
                case SerializedPropertyType.Gradient:
                    method = typeof(EditorGUI).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).First(t => t.Name == "DefaultPropertyField");
                    method.Invoke(null, new object[] { position, property, label });

                    //property.objectReferenceValue = EditorGUI.GradientField(position, label, SafeGradientValue(property)) as Object;

                    //method = typeof(EditorGUI).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).First(t => t.Name == "GradientField");
                    //method.Invoke(null, new object[] { position, property });
                    break;
                default:
                    method = typeof(EditorGUI).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).First(t => t.Name == "DefaultPropertyField");
                    method.Invoke(null, new object[] { position, property, label });

                    //Debug.LogError("SerializedPropertyType: " + property.propertyType + " not handled");
                    break;
            }
        }
    }
}