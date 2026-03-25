using UnityEditor;
using UnityEngine;
using Folklorium;

[CustomPropertyDrawer(typeof(EffectEntry))]
public class EffectEntryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var effectProp = property.FindPropertyRelative("effectSO");
        var triggerProp = property.FindPropertyRelative("trigger");
        var paramProp = property.FindPropertyRelative("parameters");

        float line = EditorGUIUtility.singleLineHeight;

        // EFFECT FIELD
        position.height = line;
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(position, effectProp);

        bool effectChanged = EditorGUI.EndChangeCheck();

        position.y += line + 2;
        EditorGUI.PropertyField(position, triggerProp);

        position.y += line + 2;

        CardEffect effect = effectProp.objectReferenceValue as CardEffect;

        // ⭐ SE TROCOU O EFFECT → recria parâmetros
        if (effectChanged && effect != null)
        {
            paramProp.managedReferenceValue =
                System.Activator.CreateInstance(effect.GetDataType());

            property.serializedObject.ApplyModifiedProperties();
        }

        // BOTÃO OU DRAW
        if (effect != null)
        {
            if (paramProp.managedReferenceValue == null)
            {
                if (GUI.Button(position, "Create Parameters"))
                {
                    paramProp.managedReferenceValue =
                        System.Activator.CreateInstance(effect.GetDataType());

                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                EditorGUI.PropertyField(position, paramProp, true);
            }
        }

        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        float space = 2f;

        float height = 0;

        // Effect SO
        height += line + space;

        // Trigger
        height += line + space;

        var paramProp = property.FindPropertyRelative("parameters");

        if (paramProp.managedReferenceValue != null)
        {
            height += EditorGUI.GetPropertyHeight(paramProp, true);
        }
        else
        {
            height += line;
        }

        return height;
    }
}