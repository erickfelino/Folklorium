using System;
using System.Collections.Generic;

// A classe pai. 
[Serializable]
public abstract class EffectData 
{ 
    // Por padrão, ninguém exclui a si mesmo.
    public virtual bool GetExcludeSelf() { return false; }
    public virtual bool RandomizeTarget() { return false; }
}

[Serializable]
public class DamageEffectData : EffectData
{
    public int damage;
    public bool excludeSelf;
    public bool randomTarget;

    public override bool GetExcludeSelf() { return excludeSelf; }
    public override bool RandomizeTarget() { return randomTarget; }
}

[Serializable]
public class BuffEffectData : EffectData
{
    public int attack;
    public int health;
    public bool excludeSelf;
    public bool randomTarget;

    public override bool GetExcludeSelf() { return excludeSelf; }
    public override bool RandomizeTarget() { return randomTarget; }
}

[Serializable]
public class HealEffectData : EffectData
{
    public int heal;
    public bool excludeSelf;
    public bool randomTarget;

    public override bool GetExcludeSelf() { return excludeSelf; }
    public override bool RandomizeTarget() { return randomTarget; }
}

[Serializable]
public class DrawCardEffectData : EffectData
{
    public int amount;
}

[Serializable]
public class SummonEffectData : EffectData
{
    public int attack;
    public int health;
    public int Quantity;
    public int boardSide;
}

[Serializable]
public class DestroyEffectData : EffectData
{
    public int quantityTargets;
    public bool randomTarget;

    public override bool RandomizeTarget() { return randomTarget; }
}

[Serializable]
public class SummonAuraEffectData : EffectData
{
    public int attackBonus = 1;
    public int healthBonus = 1;
    public bool affectOnlyOwnSide = true;
}

[Serializable]
public class AoEEffectData : EffectData
{
    public enum AoEMode
    {
        Damage,
        Heal,
        Buff,
        Destroy
    }

    public AoEMode mode = AoEMode.Damage;

    public int damageAmount = 1;
    public int healAmount = 1;
    public int buffAttack = 1;
    public int buffHealth = 1;

    public List<AoETargetType> targetGroups = new List<AoETargetType>();

    public bool excludeSelf;
    public override bool GetExcludeSelf() { return excludeSelf; }
}