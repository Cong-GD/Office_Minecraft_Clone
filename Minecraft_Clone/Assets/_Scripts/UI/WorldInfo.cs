using Minecraft.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minecraft
{
    public class WorldInfo : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField]
        private Sprite defautIcon;

        [SerializeField]
        private Image iconImage;

        [SerializeField]
        private TextMeshProUGUI worldNameText;

        [SerializeField]
        private TextMeshProUGUI creationTimeText;

        [SerializeField]
        private TextMeshProUGUI gameModeText;

        [SerializeField]
        private Image border;

        private Sprite _sprite;

        public WorldMetaData WorldData { get; private set; }

        [field: SerializeField]
        public UnityEvent<WorldInfo> OnClickEvent { get; private set; } = new UnityEvent<WorldInfo>();

        private void OnDestroy()
        {
            Destroy(_sprite);
        }

        public void SetWorldInfo(WorldMetaData worldMetaData)
        {
            WorldData = worldMetaData;
            RefreshUI();
        }

        public Sprite ToSprite(Texture2D texture)
        {
            if(texture == null)
            {
                return defautIcon;
            }
            Destroy(_sprite);
            _sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            return _sprite;
        }

        public void RefreshUI()
        {
            if(WorldData == null)
            {
                return;
            }

            iconImage.sprite = ToSprite(WorldData.icon);
            worldNameText.text = WorldData.name;
            creationTimeText.text = $"{WorldData.name} ({WorldData.creationTime:dd/MM/yyyy - hh:mm tt})";
            gameModeText.text = $"{WorldData.gameMode} Mode, Version 1.12.1";
        }

        public void SetSelected(bool selected)
        {
            border.enabled = selected;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClickEvent.Invoke(this);
        }
    }
}
