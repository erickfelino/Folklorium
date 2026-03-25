using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Folklorium
{
    // A "Entrada de Efeito": Liga o Comportamento (SO) aos Dados (EffectData)
    [System.Serializable]
    public class EffectEntry
    {
        [Tooltip("O ScriptableObject com a lógica do efeito")]
        public CardEffect effectSO; 
        
        [Tooltip("Quando este efeito deve ser ativado?")]
        public EffectTriggerType trigger; 
        
        // 👇 O SEGREDO DO POLIMORFISMO NA UNITY 👇
        // Isso fará a Unity desenhar a classe específica (DamageData, BuffData, etc) no Inspector
        [SerializeReference] 
        public EffectData parameters; 
    }

    // ==========================================
    // DADOS DA CARTA
    // ==========================================

    [CreateAssetMenu(fileName = "New Card", menuName = "Card")]
    public class CardData : ScriptableObject
    {
        public string cardName;
        [TextArea]
        public string cardBody;
        public CardType cardColorGroup;
        public CardRole cardRole;
        public int mana;
        public int attack;
        public int life;
        public Sprite art;

        [Header("Efeitos Dinâmicos")]
        public List<EffectEntry> effects;

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