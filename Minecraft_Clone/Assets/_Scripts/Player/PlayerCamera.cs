using Cinemachine;
using Minecraft.Input;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform orientation;
    [SerializeField] private Rigidbody playerBody;
    [SerializeField] private CinemachineVirtualCamera vcam;

    [Header("Settings")]
    public Vector2 mouseSensitive;
    public float minNoiseScale = 1f;
    public float maxNoiseScale = 5f;
    public float maxSpeed = 7f;
    [SerializeField] private float upLimit;
    [SerializeField] private float downLimit;

    private Vector2 _lookInput;
    private float _xRotation = 0;
    private float _yRotation = 0;
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

        var inputDelta = _lookInput * mouseSensitive * Time.deltaTime;

        _xRotation += inputDelta.x;
        _yRotation += inputDelta.y;
        _yRotation = Mathf.Clamp(_yRotation, -upLimit, downLimit);

        transform.localRotation = Quaternion.Euler(_yRotation, 0, 0);
        orientation.localRotation = Quaternion.Euler(0, _xRotation, 0);
    }

    private void FixedUpdate()
    {
        float flatSpeed = playerBody.velocity.XZ().magnitude;
        _camNoise.m_FrequencyGain = MyMath.RemapValue(flatSpeed, 0, maxSpeed, minNoiseScale, maxNoiseScale);
    }


}
