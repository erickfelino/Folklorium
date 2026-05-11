using UnityEngine;

public interface IEffectSource
{
    bool IsEnemy { get; }
    Transform EffectTransform { get; }
    GameObject EffectGameObject { get; }
    string SourceName { get; }
}