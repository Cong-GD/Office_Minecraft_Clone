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
            EventInstance instance = RuntimeManager.CreateInstance(onEatSound);
            instance.start();
            return instance;
        }

        public bool Using(ref float holdedTime, ref object useContext)
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
                return true;
            }
            return false;
        }

        public void OnEndUse(float holdedTime, object useContext)
        {
            if (useContext is EventInstance instance)
            {
                instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                instance.release();
            }
            else
            {
                Debug.LogError("Use context is not a EventInstance");
            }
        }
    }
}