using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

namespace Minecraft
{
    public class ToolTip : GlobalReference<ToolTip>
    {
        [SerializeField]
        private TextMeshProUGUI textMesh;

        [SerializeField]
        private LayoutGroup layoutGroup;

        private float _timer;

        public void ShowToolTip(string text, Vector3 position, float duration = float.PositiveInfinity)
        {
            transform.position = position;
            textMesh.text = text;
            _timer = duration;
            gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        public void HideToolTip()
        {
            _timer = 0;
            gameObject.SetActive(false);
        }
        

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                HideToolTip();
            }
        }
    }
}
