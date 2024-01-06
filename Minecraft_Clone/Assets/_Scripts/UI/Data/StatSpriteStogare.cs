using UnityEngine;

[CreateAssetMenu(fileName = "StatSpriteStogare", menuName = "Minecraft/UI/Stat Sprite Stogare")]
public class StatSpriteStogare : ScriptableObject
{
    [field: Header("Health Display")]
    [field: SerializeField]
    public Sprite HealthContainer { get; private set; }

    [field: SerializeField]
    public Sprite NormalFullHealth { get; private set; }

    [field: SerializeField]
    public Sprite NormalHalfHealth { get; private set; }

    [field: SerializeField]
    public Sprite PoisonedFullHealth { get; private set; }

    [field: SerializeField]
    public Sprite PoisonedHalfHealth { get; private set; }

    [field: SerializeField]
    public Sprite AbsorbingFullHealth { get; private set; }

    [field: SerializeField]
    public Sprite AbsorbingHalfHealth { get; private set; }


    [field: Header("Oxygen Display")]
    [field: SerializeField]
    public Sprite OxygenContainer { get; private set; }

    [field: SerializeField]
    public Sprite OxygenFull { get; private set; }

    [field: SerializeField]
    public Sprite OxygenHalf { get; private set; }


    [field: Header("Armor Display")]
    [field: SerializeField]
    public Sprite ArmorContainer { get; private set; }

    [field: SerializeField]
    public Sprite ArmorFull { get; private set; }

    [field: SerializeField]
    public Sprite ArmorHalf { get; private set; }


    [field: Header("Food Display")]
    [field: SerializeField]
    public Sprite FoodContainer { get; private set; }

    [field: SerializeField]
    public Sprite FoodFull { get; private set; }

    [field: SerializeField]
    public Sprite FoodHalf { get; private set; }

    [field: SerializeField]
    public Sprite FoodSaturatureFull { get; private set; }

    [field: SerializeField]
    public Sprite FoodSaturatureHalf { get; private set; }

    public Sprite Empty => null;


    public Sprite GetContainer(StatPoint.State state)
    {
        return state switch
        {
            StatPoint.State.Health => HealthContainer,
            StatPoint.State.PoisonedHealth => HealthContainer,
            StatPoint.State.AbsorbingHealth => HealthContainer,
            StatPoint.State.Oxygen => OxygenContainer,
            StatPoint.State.Armor => ArmorContainer,
            StatPoint.State.Food => FoodContainer,
            StatPoint.State.Saturation => FoodContainer,
            _ => Empty,
        };
    }
}