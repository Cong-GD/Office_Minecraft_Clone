using FMOD.Studio;
using FMODUnity;
using NaughtyAttributes;
using UnityEngine;

namespace Minecraft
{
    [CreateAssetMenu(fileName = "New Food Item", menuName = "Minecraft/Item/Food Item")]
    public class FoodItem_SO : FunctionlessItem_SO, IUseableItem
    {
        [SerializeField]
        [BoxGroup("Food")]
        private int fillAmount;

        [SerializeField]
        [BoxGroup("Food")]
        private float eatTime;

        [SerializeField]
        [BoxGroup("Food")]
        private EventReference onEatSound;

        public object OnStartUse()
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(onEatSound);
            eventInstance.start();
            return eventInstance;
        }

        public bool Using(ref float holdedTime, ref object usingContext)
        {
            ItemSlot hand = InventorySystem.Instance.RightHand;
            if (hand.IsEmpty() || hand.RootItem != this)
            {
                return true;
            }

            holdedTime += Time.deltaTime;
            if (holdedTime >= eatTime)
            {
                hand.TakeAmount(1);
                PlayerController.Instance.AddFood(fillAmount);
                holdedTime = 0f;
            }
            return false;
        }

        public void OnEndUse(float holdedTime, object usingContext)
        {
            if (usingContext is EventInstance eventInstance)
            {
                if(eventInstance.isValid())
                {
                    eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    eventInstance.release();
                }
            }
            else
            {
                Debug.LogError("Using context is not a EventInstance");
            }
        }
    }
}