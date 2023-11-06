using Minecraft;
using NaughtyAttributes;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Minecraft.Input;
using UnityEngine.Assertions.Must;

public class FirstPersonController : MonoBehaviour
{
    [SerializeField, Required] 
    private Transform orientation;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float sprintMultilier = 1.3f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float jumpCooldown = 0.1f;
    [SerializeField] private float airMultilier = 0.4f;
    [SerializeField] private float groundDrag = 5f;

    [Header("Ground check")]
    [SerializeField] private float groundOffset;
    [SerializeField] private float groundRadius;
    [SerializeField] private LayerMask groundMask;

    [ShowNonSerializedField]
    private Vector2 _moveInput;

    [ShowNonSerializedField]
    private float _moveSpeed;

    private Rigidbody _rb;
    private float _jumpAllowTime;
    private bool _isSprinting;

    [ShowNativeProperty]
    public bool IsGrounded { get; private set; }

    private void OnEnable()
    {
        MInput.Move.performed += ProcessMoveInput;
        MInput.Move.canceled += ProcessMoveInput;

        MInput.Jump.performed += ProcessJumpInput;
        MInput.Sprint.performed += ProcessSprintInput;
    }

    private void OnDisable()
    {
        MInput.Move.performed -= ProcessMoveInput;
        MInput.Move.canceled -= ProcessMoveInput;

        MInput.Jump.performed -= ProcessJumpInput;
        MInput.Sprint.performed -= ProcessSprintInput;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _moveSpeed = walkSpeed;
    }

    private void Update()
    {
        GroundCheck();
        ResetSprintState();
        ApplyGroundDrag();
        SpeedControl();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void ProcessSprintInput(InputAction.CallbackContext context)
    {
        _isSprinting = true;
    }

    private void ProcessMoveInput(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }
    private void ProcessJumpInput(InputAction.CallbackContext context)
    {
        if (!IsGrounded || Time.time < _jumpAllowTime)
            return;

        _jumpAllowTime = Time.time + jumpCooldown;
        Jump(1f);
    }

    private void Move()
    {
        var moveDirection = orientation.forward * _moveInput.y + orientation.right * _moveInput.x;
        float multilier = IsGrounded ? 10f : 10f * airMultilier;
        float sprintMul = _isSprinting ?  sprintMultilier : 1f;
        _moveSpeed = walkSpeed * sprintMul;
        _rb.AddForce(multilier * _moveSpeed * moveDirection, ForceMode.Force);
    }

    private void ResetSprintState()
    {
        if(_isSprinting && _moveInput == Vector2.zero)
        {
            _isSprinting = false;
        }
    }

    private void ApplyGroundDrag()
    {
        _rb.drag = IsGrounded ? groundDrag : 0f;
    }

    private void GroundCheck()
    {
        var spherePosition = new Vector3(transform.position.x, transform.position.y - groundOffset, transform.position.z);
        IsGrounded = Physics.CheckSphere(spherePosition, groundRadius, groundMask, QueryTriggerInteraction.Ignore);
    }

    private void Jump(float value)
    {
        _rb.velocity = _rb.velocity.X_Z(0);
        _rb.AddForce(jumpForce * value * transform.up, ForceMode.Impulse);
    }

    private void SpeedControl()
    {
        var flatVelocity = _rb.velocity.X_Z(0);
        _rb.velocity = Vector3.ClampMagnitude(flatVelocity, _moveSpeed).X_Z(_rb.velocity.y);
    }

    #if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = IsGrounded ? transparentGreen : transparentRed;
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - groundOffset, transform.position.z),
            groundRadius);
    }
    #endif
}
