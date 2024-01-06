using NaughtyAttributes;
using ObjectPooling;
using UnityEngine;
using UnityEngine.UI;

public class StatDisplayer : PoolObject
{
    [SerializeField]
    private StatSpriteStogare statSpriteStogare;

    [SerializeField]
    private Image containerImage;

    [SerializeField]
    private Image statImage;

    [SerializeField]
    private StatPoint displaying = default;

    private void OnValidate()
    {
        try
        {
            DisplayStat(displaying);
        }
        catch
        {
        }
    }

    public void SetStat(StatPoint healthPoint)
    {
        if (healthPoint == displaying)
            return;

        displaying = healthPoint;
        DisplayStat(displaying);
    }

    public void DisplayStat(StatPoint statPoint)
    {
        containerImage.sprite = statSpriteStogare.GetContainer(statPoint.state);

        statImage.sprite = statPoint.state switch
        {
            StatPoint.State.Health => statPoint.amount switch
            {
                StatPoint.Amount.Full => statSpriteStogare.NormalFullHealth,
                StatPoint.Amount.Half => statSpriteStogare.NormalHalfHealth,
                _ => statSpriteStogare.Empty,
            },

            StatPoint.State.PoisonedHealth => statPoint.amount switch
            {
                StatPoint.Amount.Full => statSpriteStogare.PoisonedFullHealth,
                StatPoint.Amount.Half => statSpriteStogare.PoisonedHalfHealth,
                _ => statSpriteStogare.Empty,
            },

            StatPoint.State.AbsorbingHealth => statPoint.amount switch
            {
                StatPoint.Amount.Full => statSpriteStogare.AbsorbingFullHealth,
                StatPoint.Amount.Half => statSpriteStogare.AbsorbingHalfHealth,
                _ => statSpriteStogare.Empty,
            },

            StatPoint.State.Oxygen => statPoint.amount switch
            {
                StatPoint.Amount.Full => statSpriteStogare.OxygenFull,
                StatPoint.Amount.Half => statSpriteStogare.OxygenHalf,
                _ => statSpriteStogare.Empty,
            },

            StatPoint.State.Armor => statPoint.amount switch
            {
                StatPoint.Amount.Full => statSpriteStogare.ArmorFull,
                StatPoint.Amount.Half => statSpriteStogare.ArmorHalf,
                _ => statSpriteStogare.Empty,
            },

            StatPoint.State.Food => statPoint.amount switch
            {
                StatPoint.Amount.Full => statSpriteStogare.FoodFull,
                StatPoint.Amount.Half => statSpriteStogare.FoodHalf,
                _ => statSpriteStogare.Empty,
            },

            StatPoint.State.Saturation => statPoint.amount switch
            {
                StatPoint.Amount.Full => statSpriteStogare.FoodSaturatureFull,
                StatPoint.Amount.Half => statSpriteStogare.FoodSaturatureHalf,
                _ => statSpriteStogare.Empty,
            },
            _ => statSpriteStogare.Empty,
        };

        containerImage.color = containerImage.sprite == statSpriteStogare.Empty ? Color.clear : Color.white;
        statImage.color = statImage.sprite == statSpriteStogare.Empty ? Color.clear : Color.white;
    }
}
