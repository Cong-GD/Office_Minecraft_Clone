using Minecraft;
using Minecraft.Input;
using NaughtyAttributes;
using System;
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

    [SerializeField]
    private CreavityCanvas creavityCanvas;

    [SerializeField]
    private UIRecipeDictionary recipeDictionary;

    [field: SerializeField]
    public ItemDragingSystem DraggingSystem { get; private set; }

    private bool _isInMenu;

    protected override void Awake()
    {
        base.Awake();
        Cursor.lockState = CursorLockMode.Locked;
        MInput.OpenInventory.performed += ProcessOpenInvenrotyInput;
        MInput.UI_Exit.performed += ProcessExitUIInput;
        MInput.Debugging.performed += ProcessDebuggingInput;
        MInput.OpenMenu.performed += ProcessOpenMenuInput;
    }

    private void OnDestroy()
    {
        MInput.OpenInventory.performed -= ProcessOpenInvenrotyInput;
        MInput.UI_Exit.performed -= ProcessExitUIInput;
        MInput.Debugging.performed -= ProcessDebuggingInput;
        MInput.OpenMenu.performed -= ProcessOpenMenuInput;
        Cursor.lockState = CursorLockMode.None;
    }
    private void ProcessOpenMenuInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            MInput.state = MInput.State.UI;
            Cursor.lockState = CursorLockMode.None;
            menuCanvas.enabled = true;
            _isInMenu = true;
            Time.timeScale = 0f;
        }
    }
    private void ProcessOpenInvenrotyInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OpenInventory();
        }
    }

    private void ProcessExitUIInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ExitUIMode();
        }
    }

    private void ProcessDebuggingInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            debuggingGameobject.SetActive(!debuggingGameobject.activeSelf);
        }
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
        recipeDictionary.enabled = false;
        creavityCanvas.enabled = false;
        DraggingSystem.gameObject.SetActive(false);
        inventory.SetState(UIInventory.State.None);
    }

    public void OpenCraftingTable()
    {
        EnterUIMode();
        recipeDictionary.enabled = false;
        creavityCanvas.enabled = false;
        inventory.SetState(UIInventory.State.FullCraftingTable);
    }

    public void OpenFurnace(Furnace blastFurnace)
    {
        EnterUIMode();
        recipeDictionary.enabled = false;
        creavityCanvas.enabled = false;
        inventory.SetFurnace(blastFurnace);
        inventory.SetState(UIInventory.State.BlastFurnace);
    }

    public void OpenRecipeDictionary()
    {
        EnterUIMode();
        creavityCanvas.enabled = false;
        inventory.SetState(UIInventory.State.None);
        recipeDictionary.enabled = true;
    }

    public void OpenCreavityCanvas()
    {
        EnterUIMode();
        recipeDictionary.enabled = false;
        inventory.SetState(UIInventory.State.None);
        creavityCanvas.enabled = true;
    }

    public void OpenInventory()
    {
        EnterUIMode();
        recipeDictionary.enabled = false;
        creavityCanvas.enabled = false;
        inventory.SetState(UIInventory.State.Inventory);
    }

    public void OpenStogare(Stogare storage)
    {
        EnterUIMode();
        recipeDictionary.enabled = false;
        creavityCanvas.enabled = false;
        inventory.SetStogare(storage);
        inventory.SetState(UIInventory.State.Stogare);
    }

    public void OnSaveAndQuitButtonClick()
    {
        ExitUIMode();
        GameManager.Instance.SaveAndReturnToMainMenu().Forget();
    }
}
