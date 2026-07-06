using System.Collections.Generic;
using Ashenveil.Core;
using Ashenveil.Dialog;
using Ashenveil.Inventory;
using Ashenveil.Player;
using Ashenveil.Quests;
using Ashenveil.Trade;
using Ashenveil.UI;
using UnityEngine;

namespace Ashenveil.Flow
{
    /// <summary>
    /// Runtime bridge that connects world interactables to their UI screens and owns the
    /// shared <see cref="QuestLogModel"/>. It builds dialog runners when the player talks,
    /// opens the trade screen for vendors, and delivers quest rewards into the player's
    /// inventory. Wired by the scene builder. Referenced GDD section: Kernsysteme.
    /// </summary>
    public sealed class GameServices : MonoBehaviour, IRewardSink
    {
        [Header("Player")]
        [SerializeField] private PlayerInventory _playerInventory;

        [Header("UI")]
        [SerializeField] private DialogScreenController _dialogScreen;
        [SerializeField] private TradeScreenController _tradeScreen;
        [SerializeField] private JournalScreenController _journalScreen;

        [Header("Content")]
        [SerializeField] private List<QuestDefinition> _quests = new List<QuestDefinition>();
        [SerializeField] private List<DialogSpeaker> _speakers = new List<DialogSpeaker>();
        [SerializeField] private List<VendorController> _vendors = new List<VendorController>();

        private QuestLogModel _questLog;

        /// <summary>
        /// The shared quest log.
        /// </summary>
        public QuestLogModel QuestLog => _questLog;

        private void Awake()
        {
            _questLog = new QuestLogModel(_quests, this);
            _journalScreen?.Bind(_questLog, _quests);
        }

        private void OnEnable()
        {
            foreach (DialogSpeaker speaker in _speakers)
            {
                if (speaker != null)
                {
                    speaker.Talked += OnTalked;
                }
            }

            foreach (VendorController vendor in _vendors)
            {
                if (vendor != null)
                {
                    VendorController captured = vendor;
                    captured.SessionStarted += session => OnVendorSession(captured, session);
                }
            }
        }

        private void OnDisable()
        {
            foreach (DialogSpeaker speaker in _speakers)
            {
                if (speaker != null)
                {
                    speaker.Talked -= OnTalked;
                }
            }

            // Vendor sessions are subscribed via capturing lambdas on a persistent
            // service object; they need no explicit teardown for the demo's lifetime.
        }

        private void OnTalked(DialogSpeaker speaker)
        {
            if (speaker.DialogGraph == null || _dialogScreen == null)
            {
                return;
            }

            var runner = new DialogRunnerModel(speaker.DialogGraph, _questLog, _questLog);
            _dialogScreen.Open(runner);
        }

        private void OnVendorSession(VendorController vendor, TradeModel session)
        {
            if (_tradeScreen == null || _playerInventory == null)
            {
                return;
            }

            var stock = new List<ItemDefinition>();
            if (vendor.Definition != null)
            {
                foreach (VendorDefinition.StockEntry entry in vendor.Definition.Stock)
                {
                    if (entry.Item != null)
                    {
                        stock.Add(entry.Item);
                    }
                }
            }

            _tradeScreen.Open(session, _playerInventory.Model, stock);
        }

        /// <inheritdoc />
        public void GrantGold(int amount)
        {
            _playerInventory?.Model.EarnGold(amount);
        }

        /// <inheritdoc />
        public void GrantItem(ItemDefinition item, int amount)
        {
            if (item != null)
            {
                _playerInventory?.Model.Add(item, amount);
            }
        }
    }
}
