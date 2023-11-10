using System;
using UnityEngine;

namespace ObjectPooling
{
    [RequireComponent(typeof(IPoolObject))]
    public class Prefab : MonoBehaviour
    {
        public IPoolObject Instance { get; private set; }

        public int ID { get; private set; }

        private Action<int> destroyAction;
        private Action<int> returnAction;

        public void Init(int id, Action<int> returnAction, Action<int> destroyAction)
        {
            ID = id;
            Instance = GetComponent<IPoolObject>();
            this.returnAction = returnAction;
            this.destroyAction = destroyAction;
            Instance.OnReturn += ReturnAction;
        }

        private void ReturnAction()
        {
            returnAction.Invoke(ID);
        }

        private void OnDestroy()
        {
            destroyAction?.Invoke(ID);
        }
    }
}