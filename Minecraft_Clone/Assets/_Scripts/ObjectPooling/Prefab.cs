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

        public void Init(int id, Action<int> returnAction, Action<int> destroyAction)
        {
            ID = id;
            Instance = GetComponent<IPoolObject>();
            Instance.OnReturn += () => returnAction.Invoke(ID);
            this.destroyAction = destroyAction;
        }

        private void OnDestroy()
        {
            destroyAction?.Invoke(ID);
        }
    }
}