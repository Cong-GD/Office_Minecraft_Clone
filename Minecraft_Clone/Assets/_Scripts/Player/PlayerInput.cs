using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public event Action OnFlyClick;
    public event Action OnMouseLeftClick;
    public event Action OnMouseRightClick;

    public Vector2 MouseInput { get; private set; }
    public Vector3 MovementVector { get; private set; }

    public bool IsJumping { get; private set; }

    public bool IsSprinting { get; private set; }


    private void Update()
    {
        GetMouseInput();
        GetMovementInput();
        GetJumpInput();
        GetFlyInput();
        GetSprintingInput();
    }

    private void GetFlyInput()
    {
        if(Input.GetKeyDown(KeyCode.V))
        {
            OnFlyClick?.Invoke();
        }
    }

    private void GetJumpInput()
    {
        IsJumping = Input.GetButton("Jump");
    }

    private void GetSprintingInput()
    {
        IsSprinting = Input.GetKey(KeyCode.LeftShift);
    }

    private void GetMovementInput()
    {
        MovementVector = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
    }

    private void GetMouseInput()
    {
        MouseInput = new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));
        if(Input.GetMouseButtonDown(0))
        {
            OnMouseLeftClick?.Invoke();
        }
        if (Input.GetMouseButtonDown(2))
        {
            OnMouseLeftClick?.Invoke();
        }
    }
}
