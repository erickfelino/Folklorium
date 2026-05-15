using System;
using UnityEngine;

namespace Folklorium
{
    [Serializable]
    public struct BoardCardRemovedEventPayload
    {
        public CardCombat card;
        public BoardSlot slot;
    }

    [CreateAssetMenu(fileName = "BoardCardRemovedEventChannel", menuName = "Events/Board/Card Removed Channel")]
    public class BoardCardRemovedEventChannelSO : ScriptableObject
    {
        public event Action<BoardCardRemovedEventPayload> OnEventRaised;

        public void RaiseEvent(BoardCardRemovedEventPayload payload)
        {
            OnEventRaised?.Invoke(payload);
        }
    }
}