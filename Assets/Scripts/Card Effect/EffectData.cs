using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum EffectResolutionTiming
{
    Instant,
    Delayed
}

[Serializable]
public enum EffectTurnScope
{
    OwnerTurns,
    GlobalTurns
}

[Serializable]
public enum EffectLifetimeMode
{
    Permanent,
    Temporary
}

[Serializable]
public class EffectTimingData
{
    [Tooltip("Instant = resolve agora. Delayed = esperar x turnos.")]
    public EffectResolutionTiming resolutionTiming = EffectResolutionTiming.Instant;

    [Tooltip("OwnerTurns = conta só os turnos do dono da fonte. GlobalTurns = conta qualquer troca de turno.")]
    public EffectTurnScope resolutionScope = EffectTurnScope.OwnerTurns;

    [Min(0)]
    public int delayTurns = 0;

    [Tooltip("Permanent = o efeito aplicado não expira. Temporary = dura x turnos.")]
    public EffectLifetimeMode lifetimeMode = EffectLifetimeMode.Permanent;

    [Tooltip("OwnerTurns = expira nos turnos do dono. GlobalTurns = expira em turnos globais.")]
    public EffectTurnScope lifetimeScope = EffectTurnScope.OwnerTurns;

    [Min(1)]
    public int durationTurns = 1;
}
// A classe pai. 
[Serializable]
public abstract class EffectData 
{ 
    public EffectTimingData timing = new EffectTimingData();
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
    public int attackBonus;
    public int healthBonus;
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

    public int damageAmount;
    public int healAmount;
    public int buffAttack;
    public int buffHealth;

    public List<AoETargetType> targetGroups = new List<AoETargetType>();

    public bool excludeSelf;
    public override bool GetExcludeSelf() { return excludeSelf; }
}

[Serializable]
public class ManaEffectData : EffectData
{
    public int manaDelta;
}

[Serializable]
public class SpellDamageAuraEffectData : EffectData
{
    public int bonusDamage;
}

[Serializable]
public class FreeSpellCastEffectData : EffectData
{
    
}

[Serializable]
public class ReactivateSpentSpellEffectData : EffectData
{
    
}

[Serializable]
public class FreezeEffectData : EffectData
{
    [Min(1)]
    public int freezeTurns;
}