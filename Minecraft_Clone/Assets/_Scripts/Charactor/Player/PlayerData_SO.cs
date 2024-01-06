using NaughtyAttributes;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Minecraft/Player Data")]
public class PlayerData_SO : ScriptableObject
{
    [field: Header("Movement")]
    [field: SerializeField]
    public float WalkSpeed { get; private set; } = 4.317f;

    [field: SerializeField]
    public float SprintMultilier { get; private set; } = 1.3f;

    [field: SerializeField]
    public float CrounchMultilier { get; private set; } = 0.3f;
    
    [field: SerializeField]
    public float JumpForce { get; private set; } = 9f;

    [field: SerializeField]
    public float JumpCooldown { get; private set; } = 0.1f;

    [field: SerializeField]
    public float WaterPushForce { get; private set; } = 20f;

    [field: SerializeField]
    public float SwinForce { get; private set; } = 15f;

    [field: SerializeField]
    public float DiveForce { get; private set; } = 10f;

    [field: SerializeField]
    public float HelpForceToLeaveWater { get; private set; } = 0.5f;

    [field: SerializeField]
    public float FlyingDragForce { get; private set; } = 5f;

    [field: SerializeField]
    public float FlySpeed { get; private set; } = 5f;

    [field: SerializeField]
    public float FlySprintSpeed { get; private set; } = 10f;

    [field: SerializeField]
    public float FlyUpForce { get; private set; } = 5f;

    [field: SerializeField]
    public float FlyDownForce { get; private set; } = 5f;

    [field: Header("Enviroment")]
    [field: SerializeField]
    public float AirMultilier { get; private set; } = 0.4f;

    [field: SerializeField]
    public float WaterMultilier { get; private set; } = 0.2f;

    [field: SerializeField]
    public float GroundDrag { get; private set; } = 5f;

    [field: SerializeField]
    public float AirDrag { get; private set; } = 1f;

    [field: SerializeField]
    public float WaterDrag { get; private set; } = 20f;

    [field: Header("Ground check")]

    [field: SerializeField]
    public float GroundRadius { get; private set; }

    [field: SerializeField]
    public LayerMask GroundLayer { get; private set; }

    [field: Header("Stats")]
    [field: SerializeField]
    public int MaxOxygen { get; private set; } = 20;

    [field: SerializeField]
    public float OxyConsumedTime { get; private set; } = 0.5f;

    [field: SerializeField]
    public int MaxFood { get; private set; } = 20;

    [field: SerializeField]
    public float FoodConsumedTime { get; private set; } = 0.1f;

    [field: SerializeField]
    public int DefaultMaxHealth { get; private set; } = 20;

    public Rigidbody PlayerBody
    {
        get
        {
            if(_playerBody == null)
            {
                _playerBody = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
            }

            return _playerBody;
        }
    }

    [NonSerialized]
    private Rigidbody _playerBody;

    [Header("Instance value")]
    [NonSerialized, ShowNonSerializedField]
    public float currentMoveSpeed;

    [NonSerialized, ShowNonSerializedField]
    public bool isSprinting;

    [NonSerialized, ShowNonSerializedField]
    public bool isCrounching;

    [NonSerialized, ShowNonSerializedField]
    public bool isGrounded;

    [NonSerialized, ShowNonSerializedField]
    public float allowJumpTime;

    [NonSerialized, ShowNonSerializedField]
    public bool isStepInWater;

    [NonSerialized, ShowNonSerializedField]
    public bool isBobyInWater;

    [NonSerialized, ShowNonSerializedField]
    public bool isHeadInWater;

    [NonSerialized, ShowNonSerializedField]
    public int currentOxygen;

    [NonSerialized, ShowNonSerializedField]
    public int currentFood;

    [NonSerialized, ShowNonSerializedField]
    public int maxHealth;

    [NonSerialized, ShowNonSerializedField]
    public int currentHealth;

    [NonSerialized, ShowNonSerializedField]
    public Vector3 checkPoint;

    [NonSerialized, ShowNonSerializedField]
    public bool isFlying;

    public void ClearTempData()
    {
        _playerBody = null;
        currentMoveSpeed = 0;
        isSprinting = false;
        isCrounching = false;
        isGrounded = false;
        allowJumpTime = 0;
        currentOxygen = MaxOxygen;
        currentFood = MaxFood;
        maxHealth = DefaultMaxHealth;
        currentHealth = maxHealth;
        isFlying = false;
        isStepInWater = false;
        isBobyInWater = false;
        isHeadInWater = false;
    }
}
