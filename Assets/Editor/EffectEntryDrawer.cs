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
            paramProp.managedReferenceValue = System.Activator.CreateInstance(effect.GetDataType());
            property.serializedObject.ApplyModifiedProperties();
        }

        // LÓGICA DO PARÂMETRO
        if (effect != null)
        {
            if (paramProp.managedReferenceValue == null)
            {
                if (GUI.Button(position, "Create Parameters"))
                {
                    paramProp.managedReferenceValue = System.Activator.CreateInstance(effect.GetDataType());
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                // 👇 DESCOBRE SE A TAG PERMITE SELF-TARGET
                bool canTargetSelf = false;
                var vt = effect.validTargets;
                if (vt == ValidTargetType.AllyCard || 
                    vt == ValidTargetType.AllAllyCards || 
                    vt == ValidTargetType.AnyCard || 
                    vt == ValidTargetType.AnyCharacter)
                {
                    canTargetSelf = true;
                }

                // 👇 DESENHA OS PARÂMETROS UM POR UM PARA FILTRAR O EXCLUDE SELF
                SerializedProperty child = paramProp.Copy();
                SerializedProperty end = paramProp.GetEndProperty();

                if (child.NextVisible(true))
                {
                    do
                    {
                        if (SerializedProperty.EqualContents(child, end)) break;

                        // A MÁGICA VISUAL AQUI: Pula a variável se for impossível dar auto-alvo!
                        if (child.name == "excludeSelf" && !canTargetSelf) continue;

                        position.height = EditorGUI.GetPropertyHeight(child, true);
                        EditorGUI.PropertyField(position, child, true);
                        position.y += position.height + 2;

                    } while (child.NextVisible(false));
                }
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        float space = 2f;
        float height = 0;

        // Effect SO + Trigger
        height += (line + space) * 2;

        var effectProp = property.FindPropertyRelative("effectSO");
        var paramProp = property.FindPropertyRelative("parameters");

        CardEffect effect = effectProp.objectReferenceValue as CardEffect;
        bool canTargetSelf = false;

        if (effect != null)
        {
            var vt = effect.validTargets;
            if (vt == ValidTargetType.AllyCard || 
                vt == ValidTargetType.AllAllyCards || 
                vt == ValidTargetType.AnyCard || 
                vt == ValidTargetType.AnyCharacter)
            {
                canTargetSelf = true;
            }
        }

        if (paramProp.managedReferenceValue != null)
        {
            // 👇 CALCULA A ALTURA CONSIDERANDO SE ESCONDEU A VARIÁVEL OU NÃO
            SerializedProperty child = paramProp.Copy();
            SerializedProperty end = paramProp.GetEndProperty();

            if (child.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(child, end)) break;
                    
                    if (child.name == "excludeSelf" && !canTargetSelf) continue;

                    height += EditorGUI.GetPropertyHeight(child, true) + space;

                } while (child.NextVisible(false));
            }
        }
        else if (effect != null)
        {
            height += line; // Botão de Create Parameters
        }

        return height;
    }
}