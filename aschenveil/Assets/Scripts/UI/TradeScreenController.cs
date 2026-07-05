using System.Collections.Generic;
using Ashenveil.Core;
using Ashenveil.Inventory;
using Ashenveil.Trade;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ashenveil.UI
{
    /// <summary>
    /// Two-pane trading screen: the vendor's stock (buy) on the left, the player's
    /// sellable items on the right, with live prices from <see cref="TradeModel"/> and a
    /// German feedback line on failure. Referenced GDD section: Demo-Ablauf Phase 3.
    /// </summary>
    public sealed class TradeScreenController : MonoBehaviour
    {
        private GameObject _root;
        private TextMeshProUGUI _vendorGoldLabel;
        private TextMeshProUGUI _playerGoldLabel;
        private TextMeshProUGUI _feedbackLabel;
        private RectTransform _buyRoot;
        private RectTransform _sellRoot;
        private readonly List<Button> _buyButtons = new List<Button>();
        private readonly List<Button> _sellButtons = new List<Button>();

        private TradeModel _model;
        private InventoryModel _playerInventory;
        private IGamePauser _pauser;
        private IReadOnlyList<ItemDefinition> _vendorStock;

        private void Awake()
        {
            _pauser = new TimeScalePauser();
            BuildUi();
            SetVisible(false);
        }

        /// <summary>
        /// Whether the trade screen is open.
        /// </summary>
        public bool IsOpen => _root != null && _root.activeSelf;

        /// <summary>
        /// Opens a trading session.
        /// </summary>
        public void Open(TradeModel model, InventoryModel playerInventory, IReadOnlyList<ItemDefinition> vendorStock)
        {
            _model = model;
            _playerInventory = playerInventory;
            _vendorStock = vendorStock;
            SetVisible(true);
            _pauser.Pause();
            Refresh();
        }

        /// <summary>
        /// Closes the trading session.
        /// </summary>
        public void Close()
        {
            SetVisible(false);
            _pauser.Resume();
        }

        /// <summary>
        /// Overrides the pauser (tests / custom handling).
        /// </summary>
        public void SetPauser(IGamePauser pauser)
        {
            _pauser = pauser;
        }

        private void Buy(ItemDefinition item)
        {
            if (_model.ExecuteBuy(item, 1))
            {
                _feedbackLabel.text = $"Gekauft: {item.DisplayName}";
            }
            else if (_model.QuoteBuyPrice(item, 1) > _playerInventory.Gold)
            {
                _feedbackLabel.text = "Nicht genug Gold";
            }
            else if (!_playerInventory.CanAdd(item, 1))
            {
                _feedbackLabel.text = "Zu schwer";
            }
            else
            {
                _feedbackLabel.text = "Nicht verfügbar";
            }

            Refresh();
        }

        private void Sell(ItemDefinition item)
        {
            if (_model.ExecuteSell(item, 1))
            {
                _feedbackLabel.text = $"Verkauft: {item.DisplayName}";
            }
            else
            {
                _feedbackLabel.text = "Händler hat nicht genug Gold";
            }

            Refresh();
        }

        private void Refresh()
        {
            if (_model == null || _playerInventory == null)
            {
                return;
            }

            _vendorGoldLabel.text = $"Händler-Gold: {_model.VendorGold}";
            _playerGoldLabel.text = $"Dein Gold: {_playerInventory.Gold}";

            // Buy pane.
            for (int i = 0; i < _buyButtons.Count; i++)
            {
                bool active = _vendorStock != null && i < _vendorStock.Count;
                _buyButtons[i].gameObject.SetActive(active);
                if (active)
                {
                    ItemDefinition item = _vendorStock[i];
                    int price = _model.QuoteBuyPrice(item, 1);
                    int stock = _model.GetStockQuantity(item);
                    _buyButtons[i].GetComponentInChildren<TextMeshProUGUI>().text =
                        $"{item.DisplayName}  ({stock})  —  {price} G";
                }
            }

            // Sell pane.
            IReadOnlyList<InventoryStack> stacks = _playerInventory.Stacks;
            for (int i = 0; i < _sellButtons.Count; i++)
            {
                bool active = i < stacks.Count;
                _sellButtons[i].gameObject.SetActive(active);
                if (active)
                {
                    ItemDefinition item = stacks[i].Item;
                    int price = _model.QuoteSellPrice(item, 1);
                    _sellButtons[i].GetComponentInChildren<TextMeshProUGUI>().text =
                        $"{item.DisplayName}  x{stacks[i].Quantity}  —  {price} G";
                }
            }
        }

        private void SetVisible(bool visible)
        {
            if (_root != null)
            {
                _root.SetActive(visible);
            }
        }

        private void BuildUi()
        {
            Canvas canvas = UIFactory.CreateCanvas("Trade_Canvas", sortOrder: 55);
            canvas.transform.SetParent(transform, false);

            Image panel = UIFactory.CreatePanel("TradePanel", canvas.transform, UITheme.PanelBackground);
            UIFactory.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400f, 760f));
            _root = panel.gameObject;

            TextMeshProUGUI title = UIFactory.CreateLabel("Title", panel.transform, "Handel", UITheme.FontSizeTitle, UITheme.TextPrimary, TextAlignmentOptions.Center);
            UIFactory.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(1360f, 60f));

            _vendorGoldLabel = UIFactory.CreateLabel("VendorGold", panel.transform, string.Empty, UITheme.FontSizeBody, UITheme.TextMuted, TextAlignmentOptions.Left);
            UIFactory.Place(_vendorGoldLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -90f), new Vector2(600f, 30f));

            _playerGoldLabel = UIFactory.CreateLabel("PlayerGold", panel.transform, string.Empty, UITheme.FontSizeBody, UITheme.TextPrimary, TextAlignmentOptions.Right);
            UIFactory.Place(_playerGoldLabel.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -90f), new Vector2(600f, 30f));

            TextMeshProUGUI buyHeading = UIFactory.CreateLabel("BuyHeading", panel.transform, "Kaufen", UITheme.FontSizeHeading, UITheme.AetherFill, TextAlignmentOptions.Center);
            UIFactory.Place(buyHeading.rectTransform, new Vector2(0.25f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(640f, 40f));

            TextMeshProUGUI sellHeading = UIFactory.CreateLabel("SellHeading", panel.transform, "Verkaufen", UITheme.FontSizeHeading, UITheme.AetherFill, TextAlignmentOptions.Center);
            UIFactory.Place(sellHeading.rectTransform, new Vector2(0.75f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(640f, 40f));

            _buyRoot = UIFactory.CreateRect("BuyList", panel.transform);
            UIFactory.Place(_buyRoot, new Vector2(0.25f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -180f), new Vector2(640f, 480f));
            _sellRoot = UIFactory.CreateRect("SellList", panel.transform);
            UIFactory.Place(_sellRoot, new Vector2(0.75f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -180f), new Vector2(640f, 480f));

            for (int i = 0; i < 10; i++)
            {
                int idx = i;
                Button buy = UIFactory.CreateButton("Buy" + i, _buyRoot, string.Empty, () => { if (_vendorStock != null && idx < _vendorStock.Count) Buy(_vendorStock[idx]); });
                UIFactory.Place(buy.image.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -i * 46f), new Vector2(620f, 42f));
                _buyButtons.Add(buy);

                Button sell = UIFactory.CreateButton("Sell" + i, _sellRoot, string.Empty, () => { if (_playerInventory != null && idx < _playerInventory.Stacks.Count) Sell(_playerInventory.Stacks[idx].Item); });
                UIFactory.Place(sell.image.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -i * 46f), new Vector2(620f, 42f));
                _sellButtons.Add(sell);
            }

            _feedbackLabel = UIFactory.CreateLabel("Feedback", panel.transform, string.Empty, UITheme.FontSizeBody, UITheme.TextWarning, TextAlignmentOptions.Center);
            UIFactory.Place(_feedbackLabel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(1200f, 34f));

            Button close = UIFactory.CreateButton("Close", panel.transform, "Schließen (E)", Close);
            UIFactory.Place(close.image.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(300f, 44f));
        }
    }
}
