using Minecraft.Entity.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft
{
    public class FlashDamageFeedBack : MonoBehaviour
    {
        [SerializeField]
        private List<SingleBlockRenderer> meshDatas;

        [SerializeField]
        private float flashSpeed = 0.1f;

        [SerializeField]
        private float recoverSpeed = 0.1f;

        [SerializeField]
        private Color flashColor = Color.red;

        private Coroutine _flashCoroutine;
        private List<Material> materials = new();

        private void Awake()
        {
            foreach (SingleBlockRenderer meshData in meshDatas)
            {
                materials.Add(meshData.Material);
            }
        }

        public void Flash()
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }
            _flashCoroutine = StartCoroutine(FlashCoroutine());
        }

        private IEnumerator FlashCoroutine()
        {
            float time = 0;
            while (time < flashSpeed)
            {
                time += Time.deltaTime;
                foreach (Material material in materials)
                {
                    material.color = Color.Lerp(material.color, flashColor, time / flashSpeed);
                }
                yield return null;
            }
            time = 0;
            while (time < recoverSpeed)
            {
                time += Time.deltaTime;
                foreach (Material material in materials)
                {
                    material.color = Color.Lerp(material.color, Color.white, time / recoverSpeed);
                }
                yield return null;
            }

        }
    }
}
