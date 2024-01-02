using FMODUnity;
using Minecraft.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

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
        private Graphic subGraphic;

        [SerializeField]
        private EventReference clickSoundEvent;

        [Header("Colors")]
        [SerializeField]
        private Color normalColor = Color.white;

        [SerializeField]
        private Color highlightedColor = Color.white;

        [SerializeField]
        private Color disabledColor = Color.white;

        [SerializeField]
        private Color subGraphicNormalColor = Color.white;

        [SerializeField]
        private Color subGraphicHighlightedColor = Color.white;

        [SerializeField]
        private Color subGraphicDisabledColor = Color.white;

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
            subGraphic = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            ResetState();
        }

        private void OnDisable()
        {
            ResetState();
        }

        public void SetInteractable(bool value)
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
                    subGraphic.color = subGraphicHighlightedColor;
                    break;
                case ButtonState.Disabled:
                    buttonImage.color = disabledColor;
                    subGraphic.color = subGraphicDisabledColor;
                    break;
                case ButtonState.Normal:
                default:
                    buttonImage.color = normalColor;
                    subGraphic.color = subGraphicNormalColor;
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

            buttonImage.raycastTarget = interactable;
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
                AudioManager.PlayOneShot(clickSoundEvent);
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
