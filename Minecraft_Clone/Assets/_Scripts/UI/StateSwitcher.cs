using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Minecraft
{
    public class StateSwitcher : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI text;

        public UnityEvent<string> OnStateChanged;

        public string switcherName;

        public string[] stateNames =
        {
            "ON", "OFF"
        };

        [SerializeField]
        private int stateIndex;

        private void OnValidate()
        {
            SetState(stateIndex);
        }

        public void SwitchState()
        {
            SetState(stateIndex + 1);
        }

        public void SetState(int index)
        {
            index = Mathf.Clamp(index, 0, stateNames.Length);
            if(stateNames.Length == 0)
            {
                text.text = $"{switcherName}: None";
                return;
            }
            stateIndex = index % stateNames.Length;
            text.text = $"{switcherName}: {stateNames[stateIndex]}";
            OnStateChanged.Invoke(stateNames[stateIndex]);
        }

    }
}
