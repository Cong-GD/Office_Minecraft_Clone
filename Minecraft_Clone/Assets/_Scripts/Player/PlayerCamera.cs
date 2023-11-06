using Cinemachine;
using Minecraft.Input;
using System;
using System.Collections.Generic;
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

    private Vector2 lookInput;
    private float _xRotation = 0;
    private float _yRotation = 0;
    private CinemachineBasicMultiChannelPerlin _camNoise;

    private void Start()
    {
        _camNoise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Update()
    {
        lookInput = MInput.Look.ReadValue<Vector2>();
        if (lookInput == Vector2.zero)
            return;

        var inputDelta = lookInput * mouseSensitive * Time.deltaTime;

        _xRotation += inputDelta.x;
        _yRotation += inputDelta.y;
        _yRotation = Mathf.Clamp(_yRotation, -90, 90);

        transform.rotation = Quaternion.Euler(_yRotation, _xRotation, 0);
        orientation.rotation = Quaternion.Euler(0, _xRotation, 0);
    }

    private void FixedUpdate()
    {
        float flatSpeed = playerBody.velocity.XZ().magnitude;
        _camNoise.m_FrequencyGain = MyMath.RemapValue(flatSpeed, 0, maxSpeed, minNoiseScale, maxNoiseScale);
    }


}
