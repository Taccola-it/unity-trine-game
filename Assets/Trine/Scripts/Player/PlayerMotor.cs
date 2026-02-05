using UnityEngine;

namespace Trine.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("Refs")]
        public PlayerInputRouter input;
        public ThirdPersonCameraRig cameraRig;
        public Stamina stamina;

        [Header("Movement Speeds (Valheim-like)")]
        public float forwardSpeed = 5.2f;
        public float strafeSpeed = 4.6f;
        public float backSpeed = 3.8f;

        public float sprintMultiplier = 1.45f;
        public float crouchMultiplier = 0.6f;

        [Header("Acceleration")]
        public float accel = 18f;
        public float decel = 22f;
        public float airControl = 0.55f;

        [Header("Jump / Gravity")]
        public float jumpHeight = 1.25f;
        public float gravity = -22f;
        public float groundSnap = 2.5f;

        [Header("Roll (Dodge)")]
        public float rollDuration = 0.35f;
        public float rollSpeed = 9.0f;

        private CharacterController _cc;
        private Vector3 _vel;      // включает Y
        private Vector3 _planar;   // XZ текуща€ скорость
        private float _rollUntil;
        private Vector3 _rollDir;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (stamina == null) stamina = GetComponent<Stamina>();
        }

        private void Update()
        {
            bool grounded = _cc.isGrounded;

            // Ground snap
            if (grounded && _vel.y < 0f)
                _vel.y = -groundSnap;

            // Roll
            if (input != null && input.RollPressedThisFrame && Time.time >= _rollUntil)
            {
                Vector3 wish = GetWishDir();
                if (wish.sqrMagnitude < 0.001f) wish = transform.forward;

                if (stamina == null || stamina.TrySpend(stamina.rollCost))
                {
                    _rollDir = wish.normalized;
                    _rollUntil = Time.time + rollDuration;
                }
            }

            // Jump
            if (input != null && input.JumpPressedThisFrame && grounded && Time.time >= _rollUntil)
            {
                if (stamina == null || stamina.TrySpend(stamina.jumpCost))
                {
                    _vel.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }

            // Planar move
            Vector3 desiredPlanar;

            if (Time.time < _rollUntil)
            {
                desiredPlanar = _rollDir * rollSpeed;
            }
            else
            {
                desiredPlanar = ComputeDesiredMove();
            }

            float control = grounded ? 1f : airControl;
            float a = (desiredPlanar.sqrMagnitude > _planar.sqrMagnitude) ? accel : decel;
            _planar = Vector3.MoveTowards(_planar, desiredPlanar, a * control * Time.deltaTime);

            // Sprint stamina drain
            if (stamina != null && input != null && input.SprintHeld && desiredPlanar.sqrMagnitude > 0.1f && grounded && Time.time >= _rollUntil)
            {
                stamina.SpendContinuous(stamina.sprintPerSec);
                if (stamina.Current <= 0.01f)
                {
                    // если устал Ч убираем спринт эффект (Valheim-like)
                    _planar = Vector3.ClampMagnitude(_planar, forwardSpeed);
                }
            }

            // Gravity
            _vel.y += gravity * Time.deltaTime;

            // Apply movement
            Vector3 motion = _planar + Vector3.up * _vel.y;
            _cc.Move(motion * Time.deltaTime);

            // Rotate character to camera yaw when moving (Valheim feel)
            if (Time.time >= _rollUntil)
            {
                Vector3 face = _planar;
                face.y = 0f;
                if (face.sqrMagnitude > 0.0005f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(face.normalized, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 18f * Time.deltaTime);
                }
            }
        }

        private Vector3 ComputeDesiredMove()
        {
            Vector3 wish = GetWishDir();
            if (wish.sqrMagnitude < 0.0001f) return Vector3.zero;

            // базовые скорости по направлению
            float localZ = Vector3.Dot(wish, transform.forward);
            float localX = Vector3.Dot(wish, transform.right);

            float spdZ = localZ >= 0f ? forwardSpeed : backSpeed;
            float spdX = strafeSpeed;

            float speed =
                Mathf.Abs(localZ) >= Mathf.Abs(localX)
                    ? spdZ
                    : spdX;

            // sprint / crouch
            if (input != null)
            {
                bool grounded = _cc.isGrounded;

                if (input.CrouchHeld)
                    speed *= crouchMultiplier;

                if (input.SprintHeld && grounded && (stamina == null || stamina.Current > 0.01f))
                    speed *= sprintMultiplier;
            }

            return wish.normalized * speed;
        }

        private Vector3 GetWishDir()
        {
            if (input == null || cameraRig == null) return Vector3.zero;

            Vector2 m = input.Move;
            if (m.sqrMagnitude < 0.0001f) return Vector3.zero;

            // камера-относительно (yaw)
            float yaw = cameraRig.Yaw;
            Quaternion rot = Quaternion.Euler(0f, yaw, 0f);

            Vector3 dir = rot * new Vector3(m.x, 0f, m.y);
            return dir;
        }
    }
}
