using UnityEngine;
using Folklorium;

public class SpellManager : MonoBehaviour
{
    [SerializeField] private SpellSlot spellSlot1;
    [SerializeField] private SpellSlot spellSlot2;
    [SerializeField] private ManaManager manaManager;

    public void SetupSpells(CardData spell1, CardData spell2, bool isEnemySide)
    {
        if (spellSlot1 != null) spellSlot1.Configure(spell1, isEnemySide, manaManager);
        if (spellSlot2 != null) spellSlot2.Configure(spell2, isEnemySide, manaManager);
    }

    public SpellSlot[] GetSpellSlots()
    {
        return new SpellSlot[] { spellSlot1, spellSlot2 };
    }
}