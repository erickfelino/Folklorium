using UnityEngine;

namespace Folklorium
{
    [DisallowMultipleComponent]
    public class BoardSlot : MonoBehaviour
    {
        [Header("Slot Rules")]
        [SerializeField] private CardData.CardRole acceptedRole = CardData.CardRole.Soldier;
        [SerializeField] private bool isEnemySide = false;

        public CardCombat Occupant { get; private set; }
        public bool IsFree => Occupant == null;

        public CardData.CardRole AcceptedRole => acceptedRole;
        public bool IsEnemySide => isEnemySide;
    
        public bool CanAccept(CardData cardData, bool cardIsEnemy)
        {
            if (cardData == null) return false;
            if (!IsFree) return false;
            if (cardData.cardRole != acceptedRole) return false;
            return cardIsEnemy == isEnemySide;
        }

        public void SetOccupant(CardCombat card)
        {
            Occupant = card;
        }

        public void ClearOccupant(CardCombat card)
        {
            if (Occupant == card)
            {
                Occupant = null;
            }
        }
    }
}