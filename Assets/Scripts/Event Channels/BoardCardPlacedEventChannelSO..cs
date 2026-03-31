using System;
using UnityEngine;

namespace Folklorium
{
    [Serializable]
    public struct BoardCardPlacedEventPayload
    {
        public CardCombat card;
        public BoardSlot slot;
        public BoardEntryType entryType;
    }

    [CreateAssetMenu(fileName = "BoardCardPlacedEventChannel", menuName = "Events/Board/Card Placed Channel")]
    public class BoardCardPlacedEventChannelSO : ScriptableObject
    {
        public event Action<BoardCardPlacedEventPayload> OnEventRaised;

        public void RaiseEvent(BoardCardPlacedEventPayload payload)
        {
            OnEventRaised?.Invoke(payload);
        }
    }
}