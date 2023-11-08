using Minecraft.Input;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UIInventory playerInventory;


    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        MInput.OpenInventory.performed += OpenInventory;
        MInput.UI_Exit.performed += ExitUI;
    }

    private void OnDestroy()
    {
        MInput.OpenInventory.performed -= OpenInventory;
        MInput.UI_Exit.performed -= ExitUI;
    }

    private void ExitUI(InputAction.CallbackContext obj)
    {
        MInput.state = MInput.State.Gameplay;
        Cursor.lockState = CursorLockMode.Locked;
        playerInventory.gameObject.SetActive(false);
    }

    private void OpenInventory(InputAction.CallbackContext obj)
    {
        MInput.state = MInput.State.UI;
        Cursor.lockState = CursorLockMode.None;
        playerInventory.gameObject.SetActive(true);
    }
}
