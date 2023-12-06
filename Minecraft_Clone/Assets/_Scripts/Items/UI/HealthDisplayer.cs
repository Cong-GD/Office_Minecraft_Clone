using ObjectPooling;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplayer : PoolObject
{
    [SerializeField]
    private Sprite normalFullHealth;

    [SerializeField]
    private Sprite normalHalfHealth;

    [SerializeField]
    private Sprite poisonedFullHealth;

    [SerializeField]
    private Sprite poisonedHalfHealth;

    [SerializeField]
    private Sprite absorbingFullHealth;

    [SerializeField]
    private Sprite absorbingHalfHealth;

    [SerializeField] 
    private Sprite empty;

    [SerializeField]
    private Image healthImage;

    private HealthPoint displaying = default;

    public bool DisplayHealth(HealthPoint healthPoint)
    {
        if (healthPoint == displaying)
            return false;

        displaying = healthPoint;

        healthImage.sprite = displaying switch
        {
            { amount: HealthPoint.Amount.Half, state: HealthPoint.State.Normal} => normalHalfHealth,
            { amount: HealthPoint.Amount.Full, state: HealthPoint.State.Normal} => normalFullHealth,
            { amount: HealthPoint.Amount.Half, state: HealthPoint.State.Poisoned} => poisonedHalfHealth,
            { amount: HealthPoint.Amount.Full, state: HealthPoint.State.Poisoned} => poisonedFullHealth,
            { amount: HealthPoint.Amount.Full, state: HealthPoint.State.Absorbing} => absorbingFullHealth,
            { amount: HealthPoint.Amount.Half, state: HealthPoint.State.Absorbing} => absorbingHalfHealth,
            _ => empty,
        };
        return true;
    }
}
