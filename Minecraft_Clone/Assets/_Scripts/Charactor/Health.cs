using FMODUnity;
using Minecraft.Audio;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace Minecraft
{

    public class Health : MonoBehaviour
    {
        [SerializeField, Min(0)]
        private int maxHealth = 20;

        [field: SerializeField]
        public UnityEvent<DamegeType> OnDeath { get; private set; }

        [field: SerializeField]
        public UnityEvent<int, DamegeType> OnDamaged { get; private set; }

        [field: SerializeField]
        public UnityEvent OnValueChanged { get; private set; }

        [SerializeField]
        private EventReference OnHurtSoundEvent;

        [SerializeField]
        private EventReference OnDeathSoundEvent;

        public int MaxHealth
        {
            get
            {
                return maxHealth;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("MaxHealth", "MaxHealth must be greater than 0");
                }

                maxHealth = value;
                _currentHealth = Mathf.Min(_currentHealth, maxHealth);
                OnValueChanged.Invoke();
            }
        }

        public int CurrentHealth
        {
            get => _currentHealth;
            set
            {
                SetCurrentHealth(value);
                OnValueChanged.Invoke();
            }
        }


        public int AbsorptionAmount => _absorptionAmount;

        public int AbsorptionAmountRemaining => _absorptionAmountRemaining;
        public bool IsAlive => _currentHealth > 0;


        [SerializeField, ProgressBar("Health", "maxHealth", EColor.Red)]
        private int _currentHealth;

        private readonly List<IDamagePreprocessor> _damagePreprocessors = new List<IDamagePreprocessor>();
       
        private int _absorptionAmount;

        [SerializeField, ProgressBar("Absorption", "_absorptionAmount", EColor.Yellow)]
        private int _absorptionAmountRemaining;

        private void OnValidate()
        {
            _currentHealth = maxHealth;
        }

        [Button]
        private void TakeDamage()
        {
            TakeDamage(1, DamegeType.Physic);
        }

        public void AddDamagePreprocessor(IDamagePreprocessor preprocessor)
        {
            _damagePreprocessors.Add(preprocessor);
        }

        public void RemoveDamagePreprocessor(IDamagePreprocessor preprocessor)
        {
            _damagePreprocessors.Remove(preprocessor);
        }

        public void TakeDamage(BaseItem_SO weapon)
        {
            if(weapon == null)
            {
                TakeDamage(1, DamegeType.Physic);
            }
            else
            {
                TakeDamage(weapon.AttackDamage, DamegeType.Physic);
            }
        }

        public void TakeDamage(int damage, DamegeType damegeType)
        {
            if (!IsAlive)
            {
                return;
            }

            foreach (IDamagePreprocessor preprocessor in _damagePreprocessors)
            {
                preprocessor.PreprocessDamage(ref damage, ref damegeType);
            }

            if (_absorptionAmountRemaining > 0)
            {
                int absorbed = math.min(_absorptionAmountRemaining, damage);
                _absorptionAmountRemaining -= absorbed;
                damage -= absorbed;
            }

            SetCurrentHealth(_currentHealth - damage);

            if (!IsAlive)
            {
                OnDeath.Invoke(damegeType);
                AudioManager.PlayOneShot(OnDeathSoundEvent, transform.position);
            }
            else
            {
                OnDamaged.Invoke(damage, damegeType);
                AudioManager.PlayOneShot(OnHurtSoundEvent, transform.position);
            }
            OnValueChanged.Invoke();
        }

        public void Heal(int amount)
        {
            if (!IsAlive)
            {
                return;
            }
            int overHeal = math.max(0, _currentHealth + amount - maxHealth);
            _absorptionAmountRemaining = math.min(_absorptionAmountRemaining + overHeal, _absorptionAmount);
            SetCurrentHealth(_currentHealth + amount);
            OnValueChanged.Invoke();
        }

        public void FullHeal()
        {
            if (!IsAlive)
            {
                return;
            }

            _absorptionAmountRemaining = _absorptionAmount;
            SetCurrentHealth(maxHealth);
            OnValueChanged.Invoke();
        }

        public void Kill()
        {
            if (!IsAlive)
            {
                return;
            }

            _absorptionAmountRemaining = 0;
            _absorptionAmount = 0;
            SetCurrentHealth(0);
            OnDeath.Invoke(DamegeType.InstantDeath);
            OnValueChanged.Invoke();
        }

        public void Revive()
        {
            if (IsAlive)
            {
                return;
            }

            SetCurrentHealth(1);
            OnValueChanged.Invoke();
        }

        public void AddAbsorption(int amount)
        {
            _absorptionAmount += amount;
            _absorptionAmountRemaining += amount;
            OnValueChanged.Invoke();
        }

        public void ClearAbsorption()
        {
            _absorptionAmount = 0;
            _absorptionAmountRemaining = 0;
            OnValueChanged.Invoke();
        }

        private void SetCurrentHealth(int health)
        {
            _currentHealth = Mathf.Clamp(health, 0, maxHealth);
        }
    }
}
