using Minecraft;
using Minecraft.Input;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : GlobalReference<UIManager>
{
    [SerializeField] 
    private UIInventory inventory;

    [SerializeField] 
    private GameObject debuggingGameobject;

    [SerializeField] 
    private Canvas menuCanvas;

    [SerializeField] 
    private SettingUI settingsUI;

    [field: SerializeField]
    public ItemDragingSystem DraggingSystem { get; private set; }

    private bool _isInMenu;

    protected override void Awake()
    {
        base.Awake();
        Cursor.lockState = CursorLockMode.Locked;
        MInput.OpenInventory.performed += OpenInventory;
        MInput.UI_Exit.performed += ProcessExitUIInput;
        MInput.Debugging.performed += ProcessDebuggingInput;
        MInput.OpenMenu.performed += ProcessOpenMenuInput;
    }

    private void OnDestroy()
    {
        MInput.OpenInventory.performed -= OpenInventory;
        MInput.UI_Exit.performed -= ProcessExitUIInput;
        MInput.Debugging.performed -= ProcessDebuggingInput;
        MInput.OpenMenu.performed -= ProcessOpenMenuInput;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OpenCraftingTable()
    {
        EnterUIMode();
        inventory.SetState(UIInventory.State.FullCraftingTable);
    }

    public void OpenFurnace(Furnace blastFurnace)
    {
        EnterUIMode();
        inventory.SetFurnace(blastFurnace);
        inventory.SetState(UIInventory.State.BlastFurnace);
    }

    private void ProcessOpenMenuInput(InputAction.CallbackContext context)
    {
        MInput.state = MInput.State.UI;
        Cursor.lockState = CursorLockMode.None;
        menuCanvas.enabled = true;
        _isInMenu = true;
        Time.timeScale = 0f;
    }


    private void ProcessExitUIInput(InputAction.CallbackContext _)
    {
        ExitUIMode();
    }

    private void ProcessDebuggingInput(InputAction.CallbackContext obj)
    {
        debuggingGameobject.SetActive(!debuggingGameobject.activeSelf);
    }

    private void EnterUIMode()
    {
        MInput.state = MInput.State.UI;
        Cursor.lockState = CursorLockMode.None;
        DraggingSystem.gameObject.SetActive(true);
    }

    public void ExitUIMode()
    {
        MInput.state = MInput.State.Gameplay;
        Cursor.lockState = CursorLockMode.Locked;
        if (_isInMenu)
        {
            menuCanvas.enabled = false;
            _isInMenu = false;
            Time.timeScale = 1f;
            settingsUI.Close();
            return;
        }
        DraggingSystem.gameObject.SetActive(false);
        inventory.SetState(UIInventory.State.None);
    }

    public void OnSaveAndQuitButtonClick()
    {
        ExitUIMode();
        GameManager.Instance.SaveAndReturnToMainMenu().Forget();
    }

    private void OpenInventory(InputAction.CallbackContext _)
    {
        EnterUIMode();
        inventory.SetState(UIInventory.State.Inventory);
    }
}
