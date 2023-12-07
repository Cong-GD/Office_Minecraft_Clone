using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Minecraft
{
    public enum DamegeType
    {
        Physic,
        Fire,
        Poison,
        Explosion,
    }

    public interface IDamagePreprocessor
    {
        int PreprocessDamage(int damage, DamegeType damegeType);
    }

    public class Health : MonoBehaviour
    {
        [field: SerializeField]
        public UnityEvent OnDeath { get; private set; }

        [field: SerializeField]
        public UnityEvent<int, DamegeType> OnDamaged { get; private set; }

        private List<IDamagePreprocessor> _damagePreprocessors = new List<IDamagePreprocessor>();

        private int _maxHealth;

        private int _currentHealth;

        public int MaxHealth
        {
            get
            {
                return _maxHealth;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("MaxHealth", "MaxHealth must be greater than 0");
                }

                _maxHealth = value;
                _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
            }
        }

        public int CurrentHealth
        {
            get
            {
                return _currentHealth;
            }
            set
            {
                _currentHealth = Mathf.Clamp(value, 0, _maxHealth);
            }
        }

        public bool IsAlive => _currentHealth > 0;

        public void AddDamagePreprocessor(IDamagePreprocessor preprocessor)
        {
            _damagePreprocessors.Add(preprocessor);
        }

        public void RemoveDamagePreprocessor(IDamagePreprocessor preprocessor)
        {
            _damagePreprocessors.Remove(preprocessor);
        }

        public void TakeDamage(int damage, DamegeType damegeType)
        {
            if (!IsAlive)
            {
                return;
            }

            foreach (var preprocessor in _damagePreprocessors)
            {
                damage = preprocessor.PreprocessDamage(damage, damegeType);
            }

            CurrentHealth -= damage;

            if (!IsAlive)
            {
                OnDeath.Invoke();
            }
            else
            {
                OnDamaged.Invoke(damage, damegeType);
            }
        }

        public void Heal(int amount)
        {
            if (!IsAlive)
            {
                return;
            }

            CurrentHealth += amount;
        }
    }
}