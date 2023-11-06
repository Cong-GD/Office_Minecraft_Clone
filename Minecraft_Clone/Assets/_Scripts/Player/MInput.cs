using UnityEngine;
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
                        _gamePlayInput.Disable();
                        _uiInput.Disable();
                        break;
                    case State.Gameplay:
                        _gamePlayInput.Enable();
                        _uiInput.Disable();
                        break;
                    case State.UI:
                        _gamePlayInput.Disable();
                        _uiInput.Enable();
                        break;
                }
            }
        }

        public static PlayerInputActions InputActions { get; private set; }

        private static PlayerInputActions.GamePlayActions _gamePlayInput;

        private static PlayerInputActions.UIActions _uiInput;

        public static InputAction Move { get; private set; }
        public static InputAction Jump { get; private set; }
        public static InputAction Crounch { get; private set; }
        public static InputAction LeftMouse { get; private set; }
        public static InputAction RightMouse { get; private set; }
        public static InputAction Look { get; private set; }
        public static InputAction ScrollWheel { get; private set; }
        public static InputAction OpenInventory { get; private set; }
        public static InputAction Sprint { get; private set; }

        public static InputAction UI_LeftClick { get; private set; }
        public static InputAction UI_RightClick { get; private set; }
        public static InputAction UI_Exit { get; private set; }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            InputActions = new PlayerInputActions();

            _gamePlayInput = InputActions.GamePlay;
            _uiInput = InputActions.UI;

            Move = _gamePlayInput.Move;
            Jump = _gamePlayInput.Jump;
            Crounch = _gamePlayInput.Crounch;
            LeftMouse = _gamePlayInput.LeftMouse;
            RightMouse = _gamePlayInput.RightMouse;
            Look = _gamePlayInput.Look;
            ScrollWheel = _gamePlayInput.ScrollWheel;
            OpenInventory = _gamePlayInput.OpenInventory;
            Sprint = _gamePlayInput.Sprint;

            UI_LeftClick = _uiInput.UI_LeftClick;
            UI_RightClick = _uiInput.UI_RightClick;
            UI_Exit = _uiInput.UI_Exit;

            state = State.Gameplay;
        }

    }
}