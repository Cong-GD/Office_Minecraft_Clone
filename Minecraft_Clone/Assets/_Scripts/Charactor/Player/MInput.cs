﻿using UnityEngine;
using UnityEngine.InputSystem;


namespace Minecraft.Input
{
    public static class MInput
    {
        public enum State
        {
            None,
            Gameplay,
            UI
        }

        private static State _state;

        public static State state
        {
            get => _state;
            set
            {
                _state = value;
                switch (_state)
                {
                    case State.None:
                        _gamePlayActions.Disable();
                        _uiActions.Disable();
                        break;
                    case State.Gameplay:
                        _gamePlayActions.Enable();
                        _uiActions.Disable();
                        break;
                    case State.UI:
                        _gamePlayActions.Disable();
                        _uiActions.Enable();
                        break;
                }
            }
        }

        public static PlayerInputActions InputActions { get; private set; }

        private static PlayerInputActions.GamePlayActions _gamePlayActions;
        private static PlayerInputActions.UIActions _uiActions;
        private static PlayerInputActions.GeneralActions _generalActions;

        public static InputAction Move { get; private set; }
        public static InputAction Jump { get; private set; }
        public static InputAction Crounch { get; private set; }
        public static InputAction Destroy { get; private set; }
        public static InputAction Build { get; private set; }
        public static InputAction Look { get; private set; }
        public static InputAction ScrollWheel { get; private set; }
        public static InputAction OpenInventory { get; private set; }
        public static InputAction Sprint { get; private set; }
        public static InputAction Throw { get; private set; }

        public static InputAction UI_LeftClick { get; private set; }
        public static InputAction UI_RightClick { get; private set; }
        public static InputAction UI_Exit { get; private set; }

        public static InputAction Pointer { get; private set; }
        public static InputAction Shift { get; private set; }

        public static InputAction Debugging { get; private set; }

        public static InputAction OpenMenu { get; private set; }

        public static InputAction Fly { get; private set; }

        public static Vector2 PointerPosition => Pointer.ReadValue<Vector2>();


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            InputActions = new PlayerInputActions();

            //Actions
            _gamePlayActions = InputActions.GamePlay;
            _uiActions = InputActions.UI;
            _generalActions = InputActions.General;

            //Gameplay
            Move = _gamePlayActions.Move;
            Jump = _gamePlayActions.Jump;
            Crounch = _gamePlayActions.Crounch;
            Destroy = _gamePlayActions.LeftMouse;
            Build = _gamePlayActions.RightMouse;
            Look = _gamePlayActions.Look;
            ScrollWheel = _gamePlayActions.ScrollWheel;
            OpenInventory = _gamePlayActions.OpenInventory;
            Sprint = _gamePlayActions.Sprint;
            Throw = _gamePlayActions.Throw;
            OpenMenu = _gamePlayActions.OpenMenu;
            Fly = _gamePlayActions.Fly;

            //UI
            UI_LeftClick = _uiActions.UI_LeftClick;
            UI_RightClick = _uiActions.UI_RightClick;
            UI_Exit = _uiActions.UI_Exit;

            //General
            Pointer = _generalActions.Pointer;
            Shift = _generalActions.Shift;
            Debugging = _generalActions.Debugging;

            _generalActions.Enable();

        }

    }
}