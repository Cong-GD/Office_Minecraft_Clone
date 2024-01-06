using FMODUnity;
using Minecraft;
using Minecraft.Audio;
using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    public int defaultAttackDamage = 1;
    public float defaultAttackSpeed = 4f;

    public float pushForce = 2f;

    [SerializeField]
    private float attackDelay = 0.2f;

    private float _lastAttackTime;
    private float _chargeTime;

    [SerializeField]
    private EventReference weakAttackSound;

    [SerializeField]
    private EventReference strongAttackSound;

    [SerializeField]
    private EventReference swepAttackSound;

    [SerializeField]
    private ProgressDisplayer chargeProgressDisplayer;

    private Coroutine _chargeAttackCoroutine;

    public void TryAttack(ItemSlot weaponSlot, GameObject target)
    {
        if(Time.time < _lastAttackTime + attackDelay)
            return;

        bool isWeakAttack = Time.time < _lastAttackTime + _chargeTime;
        _lastAttackTime = Time.time;

        if (target == null || !target.TryGetComponent(out Health health))
            return;

        Debug.Log($"isWeakAttack: {isWeakAttack}");

        int attackDamage = defaultAttackDamage;
        float attackSpeed = defaultAttackSpeed;
        if (!weaponSlot.IsEmpty())
        {
            attackDamage = 3;
            attackSpeed = 1f;
        }

        if(isWeakAttack)
        {
            attackDamage = 1;
        }

        if (target.TryGetComponent(out IPushAble pushAble))
        {
            Vector3 pushDirection = (target.transform.position - transform.position).With(y: 0).normalized;
            pushAble.Push(pushDirection.Add(y: 0.5f) * pushForce);
        }

        health.TakeDamage(attackDamage, DamegeType.Physic);
        _chargeTime = 1f / attackSpeed;

        if(isWeakAttack)
        {
            AudioManager.PlayOneShot(weakAttackSound, transform.position);
        }
        else
        {
            AudioManager.PlayOneShot(strongAttackSound, transform.position);
        }

        if(_chargeAttackCoroutine != null)
        {
            StopCoroutine(_chargeAttackCoroutine);
        }
        _chargeAttackCoroutine = StartCoroutine(ChargeAttackCoroutine());
    }


    private IEnumerator ChargeAttackCoroutine()
    {
        chargeProgressDisplayer.Enable();
        while (Time.time < _lastAttackTime + _chargeTime)
        {
            yield return Wait.ForEndOfFrame();
            if (Time.time < _lastAttackTime + _chargeTime)
            {
                chargeProgressDisplayer.SetValue((Time.time - _lastAttackTime) / _chargeTime);
            }
        }
        chargeProgressDisplayer.Disable();
    }
}
