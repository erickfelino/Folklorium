using System;

// A classe pai. Ela é vazia, serve só de "crachá" para a Unity aceitar na lista.
[Serializable]
public abstract class EffectData 
{ 
}

// --- ABAIXO FICAM AS CLASSES FILHAS (Crie uma para cada tipo de efeito que precisar) ---

[Serializable]
public class DamageEffectData : EffectData
{
    public int damage;
}

[Serializable]
public class BuffEffectData : EffectData
{
    public int attack;
    public int health;
}

[Serializable]
public class DrawCardEffectData : EffectData
{
    public int amount;
}

[Serializable]
public class HealEffectData : EffectData
{
    public int heal;
}