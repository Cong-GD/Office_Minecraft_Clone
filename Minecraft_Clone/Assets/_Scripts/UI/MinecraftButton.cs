using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minecraft
{
    public class MinecraftButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        public enum ButtonState
        {
            Normal,
            Highlighted,
            Disabled
        }

        [Header("References")]
        [SerializeField]
        private Image buttonImage;

        [SerializeField]
        private TextMeshProUGUI buttonText;

        [Header("Colors")]
        [SerializeField]
        private Color normalColor = Color.white;

        [SerializeField]
        private Color highlightedColor = Color.white;

        [SerializeField]
        private Color disabledColor = Color.white;

        [SerializeField]
        private Color textNormalColor = Color.white;

        [SerializeField]
        private Color textHighlightedColor = Color.white;

        [SerializeField]
        private Color textDisabledColor = Color.white;

        [Header("Sprites")]
        [SerializeField]
        private Sprite normalSprite;

        [SerializeField]
        private Sprite disabledSprite;

        [Header("Control")]
        [SerializeField]
        private bool interactable = true;

        [field: SerializeField]
        public UnityEvent OnClick { get; private set; } = new UnityEvent();

        public bool Interactable
        {
            get => interactable;
            set
            {
                if (interactable == value)
                    return;

                SetInteractable(value);
            }
        }

        private void OnValidate()
        {
            ResetState();
        }

        private void Reset()
        {
            buttonImage = GetComponent<Image>();
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            ResetState();
        }

        private void OnDisable()
        {
            ResetState();
        }

        private void SetInteractable(bool value)
        {
            interactable = value;
            ResetState();
        }

        private void SetState(ButtonState state)
        {
            if (!interactable)
                state = ButtonState.Disabled;

            switch (state)
            {
                case ButtonState.Highlighted:
                    buttonImage.color = highlightedColor;
                    buttonText.color = textHighlightedColor;
                    break;
                case ButtonState.Disabled:
                    buttonImage.color = disabledColor;
                    buttonText.color = textDisabledColor;
                    break;
                case ButtonState.Normal:
                default:
                    buttonImage.color = normalColor;
                    buttonText.color = textNormalColor;
                    break;
            }

        }

        private void ResetState()
        {
            if (interactable)
            {
                buttonImage.sprite = normalSprite;
                SetState(ButtonState.Normal);
            }
            else
            {
                buttonImage.sprite = disabledSprite;
                SetState(ButtonState.Disabled);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(interactable)
            {
                SetState(ButtonState.Highlighted);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(interactable)
            {
                OnClick.Invoke();
                ResetState();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(interactable)
            {
                SetState(ButtonState.Normal);
            } 
        }
    }
}
