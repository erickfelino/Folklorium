using UnityEngine;

namespace Folklorium
{
    [CreateAssetMenu(fileName = "New Card", menuName = "Card")]
    public class Card : ScriptableObject
    {
        public string cardName;
        public string cardBody;
        public CardType cardColorGroup;
        public CardRole cardRole;
        public int mana;
        public int attack;
        public int life;
        public Sprite art;

        public enum CardType
        {
            Red,
            Blue,
            Yellow,
            Green,
            Neutral
        }

        public enum CardRole // Categorias de criatura
        {
            Soldier,
            Hero,
            Commander
        }

    }
}
