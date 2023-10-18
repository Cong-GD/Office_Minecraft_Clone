using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform rootTransform;

    [SerializeField] private PlayerInput input;

    [Header("Settings")]
    public float mouseSensitive;

    private float currentRotationY = 0;

    private void Update()
    {
        var mouseInputDelta = input.MouseInput * Time.deltaTime * mouseSensitive;

        rootTransform.Rotate(Vector3.up, mouseInputDelta.x);

        currentRotationY = Mathf.Clamp(currentRotationY + mouseInputDelta.y, -90, 90);
        transform.localRotation = Quaternion.Euler(currentRotationY, 0, 0);
    }
}
