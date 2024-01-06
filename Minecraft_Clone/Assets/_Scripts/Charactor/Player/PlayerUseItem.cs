using Minecraft.Input;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Minecraft
{
    public class PlayerUseItem : MonoBehaviour
    {
        private bool _isBreaking = false;
        private bool _isUsing = false;

        private void Awake()
        {
            MInput.Build.performed += ProcessBuildInput;
        }

        private void OnDestroy()
        {
            MInput.Build.performed -= ProcessBuildInput;
        }

        private void ProcessBuildInput(InputAction.CallbackContext context)
        {
            ItemSlot slot = InventorySystem.Instance.RightHand;
            if(slot.IsEmpty())
            {
                return;
            }

            if (slot.RootItem is IUseableItem item)
            {
                _isBreaking = true;
                StartCoroutine(UseItemCoroutine(item));
            }
        }

        private IEnumerator UseItemCoroutine(IUseableItem item)
        {
            yield return Wait.Until(() => _isUsing == false);
            _isUsing = true;
            object useContext = item.OnStartUse();
            float holdedTime = 0f;
            while (MInput.Build.IsPressed() || !_isBreaking)
            {
                if (item.Using(ref holdedTime, ref useContext))
                {
                    break;
                }
                yield return null;
            }
            item.OnEndUse(holdedTime, useContext);
            _isBreaking = false;
            _isUsing = false;
        }
    }
}