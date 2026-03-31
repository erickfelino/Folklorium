using System;

// A classe pai. 
[Serializable]
public abstract class EffectData 
{ 
    // 👇 Função base: Por padrão, ninguém exclui a si mesmo.
    public virtual bool GetExcludeSelf() { return false; }
    public virtual bool RandomizeTarget() { return false; }
}

[Serializable]
public class DamageEffectData : EffectData
{
    public int damage;
    public bool excludeSelf; // Vai aparecer no Inspector!
    public bool randomTarget;

    public override bool GetExcludeSelf() { return excludeSelf; }
    public override bool RandomizeTarget() { return randomTarget; }
}

[Serializable]
public class BuffEffectData : EffectData
{
    public int attack;
    public int health;
    public bool excludeSelf; // Vai aparecer no Inspector!
    public bool randomTarget;

    public override bool GetExcludeSelf() { return excludeSelf; }
    public override bool RandomizeTarget() { return randomTarget; }
}

[Serializable]
public class HealEffectData : EffectData
{
    public int heal;
    public bool excludeSelf; // Vai aparecer no Inspector!
    public bool randomTarget;

    public override bool GetExcludeSelf() { return excludeSelf; }
    public override bool RandomizeTarget() { return randomTarget; }
}

[Serializable]
public class DrawCardEffectData : EffectData
{
    public int amount;
    // 👇 Sem variável aqui! O Inspector fica limpo e ele usa o 'false' da classe pai.
}

[Serializable]
public class SummonEffectData : EffectData
{
    public int attack;
    public int health;
    public int Quantity;
    public int boardSide;
    // Sem variável de excludeSelf aqui também.
}

[Serializable]
public class DestroyEffectData : EffectData
{
    public int quantityTargets;
    
    public bool randomTarget;
    public override bool RandomizeTarget() { return randomTarget; }
}