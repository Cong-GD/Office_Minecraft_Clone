using Minecraft.Input;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minecraft
{
    public class CreavityCanvas : MonoBehaviour
    {
        [SerializeField]
        private Canvas myCanvas;

        [SerializeField]
        private UIItemSlot[] toolBarSlots;

        [SerializeField]
        private CreavityUIItemSlot slotPrefab;

        [SerializeField]
        private Transform slotParent;

        private List<CreavityUIItemSlot> _uiSlots = new List<CreavityUIItemSlot>();

        private ItemSlot _cachedSlot = new ItemSlot();

        private void OnEnable()
        {
            myCanvas.enabled = true;
        }

        private void OnDisable()
        {
            myCanvas.enabled = false;
        }

        private void Awake()
        {
            InventorySystem inventorySystem = InventorySystem.Instance;

            for (int i = 0; i < toolBarSlots.Length && i < inventorySystem.toolBarItems.Length; i++)
            {
                toolBarSlots[i].SetSlot(inventorySystem.toolBarItems[i]);
            }
            EnsureNumberOfSlot(ItemUtilities.AllItems.Length);
            DisplayItems("");
        }

        public void DisplayItems(string searchPattern)
        {
            int count = 0;
            foreach (BaseItem_SO item in ItemUtilities.AllItems)
            {
                if (item.Name.Contains(searchPattern, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    _uiSlots[count].SetItem(item);
                    count++;
                }
            }
            for (int i = count; i < _uiSlots.Count; i++)
            {
                _uiSlots[i].SetItem(null);
            }
        }

        public void OnItemSlotClick(PointerEventData eventData, CreavityUIItemSlot slot)
        {
            if (slot.Item == null || eventData.button == PointerEventData.InputButton.Right)
            {
                UIManager.Instance.DraggingSystem.DraggingSlot.SetItem(ItemPacked.Empty);
                return;
            }

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                int amount = 1;
                if (MInput.Shift.IsPressed())
                {
                    amount = slot.Item.MaxStack;
                }
                _cachedSlot.SetItem(slot.Item, amount);
                _cachedSlot.TryTransferTo(UIManager.Instance.DraggingSystem.DraggingSlot, amount);
            }
        }

        private void EnsureNumberOfSlot(int count)
        {
            int totalRow = count / 8 + 1;
            totalRow = Mathf.Max(totalRow, 6);
            int totalSlot = totalRow * 8;
            while (_uiSlots.Count < totalSlot)
            {
                CreavityUIItemSlot slot = Instantiate(slotPrefab, slotParent);
                _uiSlots.Add(slot);
                slot.OnClick.AddListener(OnItemSlotClick);
            }

            while (_uiSlots.Count > totalSlot)
            {
                int last = _uiSlots.Count - 1;
                CreavityUIItemSlot slot = _uiSlots[last];
                _uiSlots.RemoveAt(last);
                Destroy(slot.gameObject);
            }
        }

    }
}
