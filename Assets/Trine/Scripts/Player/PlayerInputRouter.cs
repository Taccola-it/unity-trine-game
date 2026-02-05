using UnityEngine;
using UnityEngine.InputSystem;

namespace Trine.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputRouter : MonoBehaviour
    {
        [Header("Action Names (must match InputActions)")]
        public string actionMove = "Move";
        public string actionLook = "Look";
        public string actionJump = "Jump";
        public string actionSprint = "Sprint";
        public string actionCrouch = "Crouch";
        public string actionRoll = "Roll";
        public string actionInteract = "Interact";
        public string actionZoom = "Zoom";

        private PlayerInput _pi;

        private InputAction _move, _look, _jump, _sprint, _crouch, _roll, _interact, _zoom;

        public Vector2 Move => _move != null ? _move.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 Look => _look != null ? _look.ReadValue<Vector2>() : Vector2.zero;

        public bool SprintHeld => _sprint != null && _sprint.IsPressed();
        public bool CrouchHeld => _crouch != null && _crouch.IsPressed();

        public bool JumpPressedThisFrame => _jump != null && _jump.WasPressedThisFrame();
        public bool RollPressedThisFrame => _roll != null && _roll.WasPressedThisFrame();
        public bool InteractPressedThisFrame => _interact != null && _interact.WasPressedThisFrame();

        public float ZoomDelta => _zoom != null ? _zoom.ReadValue<float>() : 0f;

        private void Awake()
        {
            _pi = GetComponent<PlayerInput>();
            Bind();
        }

        private void OnEnable()
        {
            if (_pi != null && _pi.actions != null)
                _pi.actions.Enable();
        }

        private void OnDisable()
        {
            if (_pi != null && _pi.actions != null)
                _pi.actions.Disable();
        }

        private void Bind()
        {
            var a = _pi.actions;
            if (a == null)
            {
                Debug.LogError("[PlayerInputRouter] PlayerInput.actions is null. Assign InputActions asset on PlayerInput.");
                return;
            }

            _move = a.FindAction(actionMove, true);
            _look = a.FindAction(actionLook, true);
            _jump = a.FindAction(actionJump, true);
            _sprint = a.FindAction(actionSprint, true);
            _crouch = a.FindAction(actionCrouch, true);
            _roll = a.FindAction(actionRoll, true);
            _interact = a.FindAction(actionInteract, true);
            _zoom = a.FindAction(actionZoom, true);
        }
    }
}
