using System;
using UnityEditor;
using UnityEngine;
using Folklorium;

[CustomPropertyDrawer(typeof(EffectEntry))]
public class EffectEntryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty effectProp = property.FindPropertyRelative("effectSO");
        SerializedProperty triggerProp = property.FindPropertyRelative("trigger");
        SerializedProperty paramProp = property.FindPropertyRelative("parameters");

        float line = EditorGUIUtility.singleLineHeight;
        float space = EditorGUIUtility.standardVerticalSpacing;
        float y = position.y;

        // Effect SO
        Rect effectRect = new Rect(position.x, y, position.width, line);
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(effectRect, effectProp, new GUIContent("Effect SO"));
        bool effectChanged = EditorGUI.EndChangeCheck();
        y += line + space;

        // Trigger
        Rect triggerRect = new Rect(position.x, y, position.width, line);
        EditorGUI.PropertyField(triggerRect, triggerProp, new GUIContent("Trigger"));
        y += line + space;

        CardEffect effect = effectProp.objectReferenceValue as CardEffect;

        // Se trocou o effect, recria os parameters
        if (effectChanged && effect != null)
        {
            paramProp.managedReferenceValue = Activator.CreateInstance(effect.GetDataType());
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
        }

        if (effect == null)
        {
            EditorGUI.EndProperty();
            return;
        }

        if (paramProp.managedReferenceValue == null)
        {
            Rect buttonRect = new Rect(position.x, y, position.width, line);
            if (GUI.Button(buttonRect, "Create Parameters"))
            {
                paramProp.managedReferenceValue = Activator.CreateInstance(effect.GetDataType());
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            }

            EditorGUI.EndProperty();
            return;
        }

        y += DrawParametersBlock(new Rect(position.x, y, position.width, line), paramProp, effect) + space;

        EditorGUI.EndProperty();
    }

    private float DrawParametersBlock(Rect position, SerializedProperty paramProp, CardEffect effect)
    {
        float y = position.y;
        float line = EditorGUIUtility.singleLineHeight;
        float space = EditorGUIUtility.standardVerticalSpacing;

        SerializedProperty timingProp = paramProp.FindPropertyRelative("timing");

        SerializedProperty child = paramProp.Copy();
        SerializedProperty end = paramProp.GetEndProperty();

        bool enterChildren = true;
        if (!child.NextVisible(enterChildren))
            return 0f;

        do
        {
            if (SerializedProperty.EqualContents(child, end))
                break;

            // Esconde os filhos de timing aqui, porque ele será desenhado em bloco próprio
            if (IsTimingDescendant(child, timingProp))
                continue;

            // Exclude Self só faz sentido se o effect puder mirar em si mesmo
            if (child.name == "excludeSelf" && !CanTargetSelf(effect))
                continue;

            // Random Target só faz sentido se o effect de fato pede alvo
            if (child.name == "randomTarget" && !effect.requiresTarget)
                continue;

            if (child.name == "timing")
            {
                Rect timingLabelRect = new Rect(position.x, y, position.width, line);
                EditorGUI.LabelField(timingLabelRect, "Timing");
                y += line + space;

                y += DrawTimingFields(new Rect(position.x, y, position.width, line), timingProp) + space;
                continue;
            }

            float h = EditorGUI.GetPropertyHeight(child, true);
            Rect childRect = new Rect(position.x, y, position.width, h);
            EditorGUI.PropertyField(childRect, child, true);
            y += h + space;

        } while (child.NextVisible(false));

        return y - position.y;
    }

    private float DrawTimingFields(Rect position, SerializedProperty timingProp)
    {
        if (timingProp == null)
            return 0f;

        float y = position.y;
        float line = EditorGUIUtility.singleLineHeight;
        float space = EditorGUIUtility.standardVerticalSpacing;

        SerializedProperty resolutionTiming = timingProp.FindPropertyRelative("resolutionTiming");
        SerializedProperty resolutionScope = timingProp.FindPropertyRelative("resolutionScope");
        SerializedProperty delayTurns = timingProp.FindPropertyRelative("delayTurns");
        SerializedProperty lifetimeMode = timingProp.FindPropertyRelative("lifetimeMode");
        SerializedProperty lifetimeScope = timingProp.FindPropertyRelative("lifetimeScope");
        SerializedProperty durationTurns = timingProp.FindPropertyRelative("durationTurns");

        // Resolution Timing
        Rect rtRect = new Rect(position.x, y, position.width, line);
        EditorGUI.PropertyField(rtRect, resolutionTiming, new GUIContent("Resolution Timing"));
        y += line + space;

        // Só aparece se for delayed
        if (resolutionTiming != null &&
            resolutionTiming.enumValueIndex == (int)EffectResolutionTiming.Delayed)
        {
            Rect rsRect = new Rect(position.x, y, position.width, line);
            EditorGUI.PropertyField(rsRect, resolutionScope, new GUIContent("Resolution Scope"));
            y += line + space;

            Rect delayRect = new Rect(position.x, y, position.width, line);
            EditorGUI.PropertyField(delayRect, delayTurns, new GUIContent("Delay Turns"));
            y += line + space;
        }

        // Lifetime Mode
        Rect lmRect = new Rect(position.x, y, position.width, line);
        EditorGUI.PropertyField(lmRect, lifetimeMode, new GUIContent("Lifetime Mode"));
        y += line + space;

        // Só aparece se for temporary
        if (lifetimeMode != null &&
            lifetimeMode.enumValueIndex == (int)EffectLifetimeMode.Temporary)
        {
            Rect lsRect = new Rect(position.x, y, position.width, line);
            EditorGUI.PropertyField(lsRect, lifetimeScope, new GUIContent("Lifetime Scope"));
            y += line + space;

            Rect durRect = new Rect(position.x, y, position.width, line);
            EditorGUI.PropertyField(durRect, durationTurns, new GUIContent("Duration Turns"));
            y += line + space;
        }

        return y - position.y;
    }

    private bool IsTimingDescendant(SerializedProperty child, SerializedProperty timingProp)
    {
        if (child == null || timingProp == null)
            return false;

        return child.propertyPath.StartsWith(timingProp.propertyPath + ".", StringComparison.Ordinal);
    }

    private bool CanTargetSelf(CardEffect effect)
    {
        if (effect == null)
            return false;

        return effect.validTargets == ValidTargetType.AllyCard ||
               effect.validTargets == ValidTargetType.AnyCard ||
               effect.validTargets == ValidTargetType.AnyCharacter;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        float space = EditorGUIUtility.standardVerticalSpacing;

        float height = 0f;

        // Effect SO + Trigger
        height += (line + space) * 2;

        SerializedProperty effectProp = property.FindPropertyRelative("effectSO");
        SerializedProperty paramProp = property.FindPropertyRelative("parameters");

        CardEffect effect = effectProp.objectReferenceValue as CardEffect;

        if (effect == null)
            return height;

        if (paramProp.managedReferenceValue == null)
        {
            height += line;
            return height;
        }

        height += GetParametersBlockHeight(paramProp, effect);
        return height;
    }

    private float GetParametersBlockHeight(SerializedProperty paramProp, CardEffect effect)
    {
        float line = EditorGUIUtility.singleLineHeight;
        float space = EditorGUIUtility.standardVerticalSpacing;
        float height = 0f;

        SerializedProperty timingProp = paramProp.FindPropertyRelative("timing");

        SerializedProperty child = paramProp.Copy();
        SerializedProperty end = paramProp.GetEndProperty();

        bool enterChildren = true;
        if (!child.NextVisible(enterChildren))
            return 0f;

        do
        {
            if (SerializedProperty.EqualContents(child, end))
                break;

            if (IsTimingDescendant(child, timingProp))
                continue;

            if (child.name == "excludeSelf" && !CanTargetSelf(effect))
                continue;

            if (child.name == "randomTarget" && !effect.requiresTarget)
                continue;

            if (child.name == "timing")
            {
                height += line + space; // label Timing
                height += GetTimingFieldsHeight(timingProp);
                continue;
            }

            height += EditorGUI.GetPropertyHeight(child, true) + space;

        } while (child.NextVisible(false));

        return height;
    }

    private float GetTimingFieldsHeight(SerializedProperty timingProp)
    {
        if (timingProp == null)
            return 0f;

        float line = EditorGUIUtility.singleLineHeight;
        float space = EditorGUIUtility.standardVerticalSpacing;
        float height = 0f;

        SerializedProperty resolutionTiming = timingProp.FindPropertyRelative("resolutionTiming");
        SerializedProperty lifetimeMode = timingProp.FindPropertyRelative("lifetimeMode");

        // Resolution Timing
        height += line + space;

        if (resolutionTiming != null &&
            resolutionTiming.enumValueIndex == (int)EffectResolutionTiming.Delayed)
        {
            height += line + space; // Resolution Scope
            height += line + space; // Delay Turns
        }

        // Lifetime Mode
        height += line + space;

        if (lifetimeMode != null &&
            lifetimeMode.enumValueIndex == (int)EffectLifetimeMode.Temporary)
        {
            height += line + space; // Lifetime Scope
            height += line + space; // Duration Turns
        }

        return height;
    }
}