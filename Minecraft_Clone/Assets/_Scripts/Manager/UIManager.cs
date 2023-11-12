using Minecraft.Input;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : GlobalReference<UIManager>
{
    [SerializeField] private UIInventory inventory;
    [SerializeField] private GameObject debuggingGameobject;

    [field: SerializeField]
    public ItemDragingSystem DraggingSystem { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Cursor.lockState = CursorLockMode.Locked;
        MInput.OpenInventory.performed += OpenInventory;
        MInput.UI_Exit.performed += ProcessExitUIInput;
        MInput.Debugging.performed += ProcessDebuggingInput;
    }

    private void OnDestroy()
    {
        MInput.OpenInventory.performed -= OpenInventory;
        MInput.UI_Exit.performed -= ProcessExitUIInput;
        MInput.Debugging.performed -= ProcessDebuggingInput;
    }

    public void OpenCraftingTable()
    {
        EnterUIMode();
        inventory.SetState(UIInventory.State.FullCraftingTable);
    }

    public void OpenBlastFurnace(BlastFurnace blastFurnace)
    {
        EnterUIMode();
        inventory.SetFurnace(blastFurnace);
        inventory.SetState(UIInventory.State.BlastFurnace);
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

    private void ExitUIMode()
    {
        MInput.state = MInput.State.Gameplay;
        Cursor.lockState = CursorLockMode.Locked;
        DraggingSystem.gameObject.SetActive(false);
        inventory.SetState(UIInventory.State.None);
    }

    private void OpenInventory(InputAction.CallbackContext _)
    {
        EnterUIMode();
        inventory.SetState(UIInventory.State.Inventory);
    }
}
