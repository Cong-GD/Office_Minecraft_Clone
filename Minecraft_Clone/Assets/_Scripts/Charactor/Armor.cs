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

        private readonly List<IArmorSource> _sources = new();

        [field: SerializeField]
        public UnityEvent OnArmorChanged { get; private set; } = new();

        public int ArmorPoint { get; private set; }
        public int Toughness { get; private set; }

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

        private void OnValidate()
        {
            ArmorPoint = _baseArmorPoint;
            Toughness = _baseToughness;
        }

        public void PreprocessDamage(ref int damage, ref DamegeType damegeType)
        {
            if (damegeType == DamegeType.Physic && ArmorPoint > 0)
            {
                float totaldamage = damage * 
                    (1f - math.min(20f, math.max(ArmorPoint / 5f, ArmorPoint - (4f * damage) / (Toughness + 8f))) 
                    / 25f);

                damage = (int)math.ceil(totaldamage);
            }
        }

        public void AddArmorSource(IArmorSource source)
        {
            _sources.Add(source);
            ValidatePoint();
        }

        public void RemoveArmorSource(IArmorSource source)
        {
            _sources.Remove(source);
            ValidatePoint();
        }

        public void ValidatePoint()
        {
            ArmorPoint = _baseArmorPoint;
            Toughness = _baseToughness;
            foreach (var source in _sources)
            {
                ArmorPoint += source.ArmorPoint;
                Toughness += source.Toughness;
            }
            OnArmorChanged.Invoke();
        }
    }
}