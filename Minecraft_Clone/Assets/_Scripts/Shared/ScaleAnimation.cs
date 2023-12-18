using NaughtyAttributes;
using Unity.Mathematics;
using UnityEngine;

namespace Minecraft
{
    public class ScaleAnimation : MonoBehaviour
    {
        public enum Axis
        {
            X = 1,
            Y = 2,
            Z = 4
        }

        [EnumFlags]
        public Axis axis;

        public float speed = 1f;

        public float amplitude = 1f;

        public EasingFunction.Ease ease = EasingFunction.Ease.EaseInSine;

        private Vector3 _default;

        private void Start()
        {
            _default = transform.localScale;
        }

        void Update()
        {
            float time = Time.time * speed;
            float value = math.remap(-1f, 1f, 0f, 1f, math.sin(time));
            value = EasingFunction.GetEaseFunction(ease)(0f, 1f, value);

            Vector3 scale = _default;

            if(HasAxis(Axis.X))
                scale.x += amplitude * value;

            if(HasAxis(Axis.Y))
                scale.y += amplitude * value;

            if(HasAxis(Axis.Z))
                scale.z += amplitude * value;

            transform.localScale = scale;
        }

        private bool HasAxis(Axis axis)
        {
            return (this.axis & axis) == axis;
        }
    }
}
