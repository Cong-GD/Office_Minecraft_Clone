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

    [field: Header("Enviroment")]
    [field: SerializeField]
    public float AirMultilier { get; private set; } = 0.4f;

    [field: SerializeField]
    public float GroundDrag { get; private set; } = 5f;

    [field: SerializeField]
    public float AirDrag { get; private set; } = 1f;

    [field: SerializeField]
    public float WaterDrag { get; private set; } = 20f;

    [field: SerializeField]
    public Vector3 BodyOffset { get; private set; }

    [field: Header("Ground check")]
    [field: SerializeField]
    public float GroundOffset { get; private set; }

    [field: SerializeField]
    public float GroundRadius { get; private set; }

    [field: SerializeField]
    public LayerMask GroundLayer { get; private set; }


    public Rigidbody PlayerBody
    {
        get
        {
            if(_playerBody == null)
                _playerBody = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();

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

    public void ClearTempData()
    {
        _playerBody = null;
        currentMoveSpeed = 0;
        isSprinting = false;
        isCrounching = false;
        isGrounded = false;
        allowJumpTime = 0;
    }
}
