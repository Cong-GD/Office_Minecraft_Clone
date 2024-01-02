using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

namespace Minecraft
{
    public class ConfirmPanel : GlobalReference<ConfirmPanel>
    {
        [SerializeField]
        private TextMeshProUGUI messageText;

        [SerializeField]
        private MinecraftButton confirmButton;

        [SerializeField]
        private MinecraftButton cancelButton;

        [SerializeField]
        private TextMeshProUGUI confirmButtonText;

        [SerializeField]
        private TextMeshProUGUI cancelButtonText;

        private Action _onConfirm;

        private Action _onCancel;

        private Action _onHide;

        protected override void Awake()
        {
            base.Awake();
            confirmButton.OnClick.AddListener(OnConfirmButtonClicked);
            cancelButton.OnClick.AddListener(OnCancelButtonClicked);
            gameObject.SetActive(false);
            GetComponent<Canvas>().enabled = true;
        }

        public void Show(string message, string confirmMessage = "Confirm", string cancelMessage = "Cancel", Action onConfirm = null, Action onCancel = null, Action onHide = null)
        {
            messageText.text = message;
            confirmButtonText.text = confirmMessage;
            cancelButtonText.text = cancelMessage;
            _onConfirm = onConfirm;
            _onCancel = onCancel;
            _onHide = onHide;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _onCancel = null;
            _onConfirm = null;
            gameObject.SetActive(false);
            _onHide?.Invoke();
            _onHide = null;
        }

        private void OnConfirmButtonClicked()
        {
            _onConfirm?.Invoke();
            Hide();
        }

        private void OnCancelButtonClicked()
        {
            _onCancel?.Invoke();
            Hide();
        }
    }
}
