using Cinemachine;
using DG.Tweening;
using Minecraft;
using Minecraft.Input;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField]
    private PlayerData_SO playerData;

    [SerializeField]
    private Transform camFollowTarget;

    [SerializeField] 
    private Transform orientation;

    [SerializeField] 
    private Rigidbody playerBody;

    [SerializeField] 
    private CinemachineVirtualCamera vcam;

    [Header("Settings")]
    public Vector2 mouseSensitive;
    public float minNoiseScale = 1f;
    public float maxNoiseScale = 5f;
    public float maxSpeed = 7f;

    [SerializeField] 
    private float upLimit;

    [SerializeField] 
    private float downLimit;

    [SerializeField]
    private float shakeDuration = 0.3f;

    [SerializeField]
    private float shakeStrength = 2f;

    public float XRotation
    {
        get => _xRotation;
        set
        {
            _xRotation =  value;
            orientation.localRotation = Quaternion.Euler(0, _xRotation, 0);
        }
    }

    public float YRotation
    {
        get => _yRotation;
        set
        {
            _yRotation = Mathf.Clamp(value, -upLimit, downLimit);
            transform.localRotation = Quaternion.Euler(_yRotation, 0, 0);
        }
    }

    private Vector2 _lookInput;
    private float _xRotation = 0;
    private float _yRotation = 0;
    private Tweener shakingTweener;
    private CinemachineBasicMultiChannelPerlin _camNoise;

    private void Start()
    {
        _camNoise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {
        _lookInput = MInput.Look.ReadValue<Vector2>();
        if (_lookInput == Vector2.zero)
            return;

        Vector2 inputDelta = Time.deltaTime * _lookInput * mouseSensitive;

        _xRotation += inputDelta.x;
        _yRotation += inputDelta.y;
        _yRotation = Mathf.Clamp(_yRotation, -upLimit, downLimit);

        orientation.localRotation = Quaternion.Euler(0, _xRotation, 0);
        transform.localRotation = Quaternion.Euler(_yRotation, 0, 0);
    }

    private void FixedUpdate()
    {
        if(playerData.isFlying)
        {
            _camNoise.m_FrequencyGain = 1;
            return;
        }

        float flatSpeed = playerBody.velocity.XZ().magnitude;
        _camNoise.m_FrequencyGain = math.remap(0, maxSpeed, minNoiseScale, maxNoiseScale, flatSpeed);
    }

    [Button("Shake")]
    public void ShakeCammera()
    {
        shakingTweener?.Complete();
        shakingTweener = camFollowTarget.DOShakeRotation(shakeDuration, shakeStrength);
    }

}
