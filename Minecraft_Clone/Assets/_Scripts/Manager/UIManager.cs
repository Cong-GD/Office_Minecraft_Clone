using System;
using Unity.VisualScripting;
using UnityEngine;

public class UIManager : GlobalReference<UIManager>
{
    [SerializeField] private PlayerInventory playerInventory;


    private bool _isActive;


    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (value == _isActive)
                return;

            _isActive = value;

            playerInventory.gameObject.SetActive(value);
        }
    }

}
