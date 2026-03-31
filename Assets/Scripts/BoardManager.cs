using System;
using System.Collections.Generic;
using UnityEngine;

namespace Folklorium
{
    public enum BoardEntryType
    {
        PlayedFromHand,
        Summoned,
        Generated
    }

    public class BoardManager : MonoBehaviour
    {
        public static BoardManager Instance { get; private set; }

        [Header("Slots")]
        [SerializeField] private bool autoDiscoverSlots = true;
        [SerializeField] private List<BoardSlot> registeredSlots = new List<BoardSlot>();

        [Header("Event Channels")]
        [SerializeField] private BoardCardPlacedEventChannelSO boardCardPlacedChannel;
        [SerializeField] private BoardCardRemovedEventChannelSO boardCardRemovedChannel;

        private readonly Dictionary<CardCombat, BoardSlot> cardToSlot = new Dictionary<CardCombat, BoardSlot>();
        private readonly Dictionary<BoardSlot, CardCombat> slotToCard = new Dictionary<BoardSlot, CardCombat>();

        public event Action<CardCombat, BoardSlot, BoardEntryType> OnCardPlaced;
        public event Action<CardCombat, BoardSlot> OnCardRemoved;

        public BoardCardPlacedEventChannelSO CardPlacedChannel => boardCardPlacedChannel;
        public BoardCardRemovedEventChannelSO CardRemovedChannel => boardCardRemovedChannel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            RefreshSlots();
        }

        private void RefreshSlots()
        {
            if (!autoDiscoverSlots && registeredSlots.Count > 0)
                return;

            registeredSlots.Clear();

#if UNITY_2022_2_OR_NEWER
            registeredSlots.AddRange(FindObjectsByType<BoardSlot>(FindObjectsSortMode.None));
#else
            registeredSlots.AddRange(FindObjectsOfType<BoardSlot>());
#endif
        }

        public bool TryGetFreeSlot(CardData cardData, bool cardIsEnemy, out BoardSlot slot)
        {
            slot = null;
            RefreshSlots();

            if (cardData == null)
                return false;

            foreach (BoardSlot candidate in registeredSlots)
            {
                if (candidate == null) continue;

                if (candidate.CanAccept(cardData, cardIsEnemy))
                {
                    slot = candidate;
                    return true;
                }
            }

            return false;
        }

        public void HighlightValidSlots(CardData cardData, bool cardIsEnemy, bool active, Color color)
        {
            RefreshSlots();

            foreach (BoardSlot slot in registeredSlots)
            {
                if (slot == null) continue;

                bool shouldHighlight = active &&
                                       cardData != null &&
                                       slot.CanAccept(cardData, cardIsEnemy);

                slot.SetHighlight(shouldHighlight, color);
            }
        }

        public void ClearHighlights()
        {
            RefreshSlots();

            foreach (BoardSlot slot in registeredSlots)
            {
                if (slot == null) continue;

                slot.SetHighlight(false, Color.white);
            }
        }

        public bool TryPlaceCard(CardCombat card, BoardSlot slot, BoardEntryType entryType = BoardEntryType.PlayedFromHand)
        {
            if (card == null || slot == null)
                return false;

            if (cardToSlot.ContainsKey(card))
                return false;

            CardDisplay display = card.GetComponent<CardDisplay>();
            if (display == null || display.cardData == null)
                return false;

            if (!slot.CanAccept(display.cardData, card.isEnemy))
                return false;

            if (slotToCard.ContainsKey(slot))
                return false;

            slot.SetOccupant(card);
            cardToSlot[card] = slot;
            slotToCard[slot] = card;

            return true;
        }

        public void NotifyCardPlaced(CardCombat card, BoardSlot slot, BoardEntryType entryType = BoardEntryType.PlayedFromHand)
        {
            if (card == null || slot == null) return;

            OnCardPlaced?.Invoke(card, slot, entryType);

            if (boardCardPlacedChannel != null)
            {
                boardCardPlacedChannel.RaiseEvent(new BoardCardPlacedEventPayload
                {
                    card = card,
                    slot = slot,
                    entryType = entryType
                });
            }
        }

        public void ReleaseCard(CardCombat card)
        {
            if (card == null)
                return;

            if (!cardToSlot.TryGetValue(card, out BoardSlot slot))
                return;

            cardToSlot.Remove(card);

            if (slot != null)
            {
                if (slotToCard.TryGetValue(slot, out CardCombat occupied) && occupied == card)
                {
                    slotToCard.Remove(slot);
                }

                slot.ClearOccupant(card);
                OnCardRemoved?.Invoke(card, slot);

                if (boardCardRemovedChannel != null)
                {
                    boardCardRemovedChannel.RaiseEvent(new BoardCardRemovedEventPayload
                    {
                        card = card,
                        slot = slot
                    });
                }
            }
        }

        public BoardSlot GetSlotOf(CardCombat card)
        {
            if (card == null)
                return null;

            return cardToSlot.TryGetValue(card, out BoardSlot slot) ? slot : null;
        }

        public int CountCards(bool isEnemySide, CardData.CardRole? role = null)
        {
            int count = 0;

            foreach (var pair in cardToSlot)
            {
                CardCombat card = pair.Key;
                if (card == null || card.isEnemy != isEnemySide)
                    continue;

                CardDisplay display = card.GetComponent<CardDisplay>();
                if (display == null || display.cardData == null)
                    continue;

                if (role.HasValue && display.cardData.cardRole != role.Value)
                    continue;

                count++;
            }

            return count;
        }

        public List<CardCombat> GetAllCardsOnBoard()
        {
            List<CardCombat> cards = new List<CardCombat>();

            foreach (var pair in cardToSlot)
            {
                if (pair.Key != null)
                {
                    cards.Add(pair.Key);
                }
            }

            return cards;
        }
    }
}