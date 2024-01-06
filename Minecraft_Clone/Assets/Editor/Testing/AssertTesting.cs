using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Assertions;

namespace Minecraft.Assets.Testing
{
    public class AssertTesting : MonoBehaviour
    {
        public bool condition = false;

        public GameObject go;

        [Button]
        public void Test()
        {
            Assert.IsTrue(condition, "Test fail");

            Debug.Log("Test");
        }

        [Button]
        public void Test3()
        {
            Debug.Assert(go != null, "game object is null", go);

            Debug.Log("Test");
        }
    }
}
