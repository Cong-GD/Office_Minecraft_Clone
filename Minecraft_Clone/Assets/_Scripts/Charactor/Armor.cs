using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace Minecraft
{

    public class Armor : MonoBehaviour, IDamagePreprocessor
    {
        [SerializeField]
        private int _baseArmorPoint;

        [SerializeField]
        private int _baseToughness;

        [field: SerializeField]
        public UnityEvent OnValueChanged { get; private set; } = new();

        public int MaxArmorPoint => 20;

        public int MaxToughness => 20;

        public int ArmorPoint { get; private set; }
        public int Toughness { get; private set; }

        private readonly List<ArmorSource> _sources = new();

        private void Start()
        {
            if(TryGetComponent(out Health health))
            {
                health.AddDamagePreprocessor(this);
            }
        }

        private void OnDestroy()
        {
            if(TryGetComponent(out Health health))
            {
                health.RemoveDamagePreprocessor(this);
            }
        }

        public void PreprocessDamage(ref int damage, ref DamegeType damegeType)
        {
            if (damegeType == DamegeType.Physic)
            {
                if(ArmorPoint <= 0)
                    return;

                float totaldamage = damage * 
                    (1f - math.min(20f, math.max(ArmorPoint / 5f, ArmorPoint - (4f * damage) / (Toughness + 8f))) 
                    / 25f);

                damage = (int)math.ceil(totaldamage);
            }
        }

        public void AddArmorSource(ArmorSource source)
        {

            if (source == null)
                return;

            _sources.Add(source);
            source.OnValueChanged += ValidateValue;
            ValidateValue();
        }

        public void RemoveArmorSource(ArmorSource source)
        {
            if(source == null)
                return;

            _sources.Remove(source);
            source.OnValueChanged -= ValidateValue;
            ValidateValue();
        }

        private void ValidateValue()
        {
            ArmorPoint = _baseArmorPoint;
            foreach (ArmorSource source in _sources)
            {
                ArmorPoint += source.ArmorPoint;
            }
            ArmorPoint = math.clamp(ArmorPoint, 0, MaxArmorPoint);

            Toughness = _baseToughness;
            foreach (ArmorSource source in _sources)
            {
                Toughness += source.Toughness;
            }
            Toughness = math.clamp(Toughness, 0, MaxToughness);
            OnValueChanged.Invoke();
        }
    }
}