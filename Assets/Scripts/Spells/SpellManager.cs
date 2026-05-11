using UnityEngine;
using Folklorium;

public class SpellManager : MonoBehaviour
{
    public static SpellManager Instance { get; private set; }

    [SerializeField] private SpellSlot spellSlot1;
    [SerializeField] private SpellSlot spellSlot2;
    [SerializeField] private ManaManager manaManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetupSpells(CardData spell1, CardData spell2, bool isEnemySide)
    {
        if (spellSlot1 != null) spellSlot1.Configure(spell1, isEnemySide, manaManager);
        if (spellSlot2 != null) spellSlot2.Configure(spell2, isEnemySide, manaManager);
    }
}