using UnityEngine;

public class SpellDamageAuraProvider : MonoBehaviour, ISpellDamageAuraProvider
{
    [SerializeField] private int spellDamageBonus = 4;
    public int SpellDamageBonus => spellDamageBonus;

    public void SetBonus(int bonus)
    {
        spellDamageBonus = bonus;
    }
}