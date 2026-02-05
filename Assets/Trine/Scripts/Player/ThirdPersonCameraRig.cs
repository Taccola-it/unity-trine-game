using UnityEngine;

namespace Trine.Player
{
    public class ThirdPersonCameraRig : MonoBehaviour
    {
        [Header("Refs")]
        public Transform target;              // Лучше: PlayerPivot (на уровне плеч)
        public Transform cameraPivot;         // Empty (можно внутри CameraRig)
        public Camera cam;
        public PlayerInputRouter input;

        [Header("Pivot / Shoulder (Valheim-like)")]
        public Vector3 pivotOffset = new Vector3(0f, 0f, 0f);          // если target = PlayerPivot, оставь 0
        public Vector3 shoulderOffset = new Vector3(0.35f, 0f, 0f);    // лёгкое смещение в бок

        [Header("Rotation")]
        public float yawSpeed = 240f;
        public float pitchSpeed = 200f;

        [Tooltip("Вверх (смотреть в небо). Обычно отрицательное.")]
        public float minPitch = -55f;

        [Tooltip("Вниз (смотреть под ноги).")]
        public float maxPitch = 55f;

        [Tooltip("Инвертировать Y мыши, если ощущения 'наоборот'.")]
        public bool invertLookY = false;

        [Header("Zoom")]
        public float distance = 4.5f;
        public float minDistance = 2.0f;
        public float maxDistance = 8.0f;
        public float zoomSpeed = 2.0f;

        [Header("Collision")]
        public LayerMask collisionMask = ~0;
        public float sphereRadius = 0.18f;
        public float collisionPadding = 0.15f;

        [Header("Stability")]
        [Tooltip("Минимальная дистанция при коллизии, чтобы не 'проваливаться' в pivot.")]
        public float hardMinDistance = 1.2f;

        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        private void Reset()
        {
            cam = Camera.main;
        }

        private void LateUpdate()
        {
            if (target == null || cameraPivot == null || cam == null)
                return;

            // Лочим курсор для тестов (позже сделаем unlock на Esc/Inventory)
            if (Application.isFocused)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            Vector2 look = input != null ? input.Look : Vector2.zero;
            float zoom = input != null ? input.ZoomDelta : 0f;

            // Zoom
            distance = Mathf.Clamp(distance - zoom * zoomSpeed * Time.deltaTime, minDistance, maxDistance);

            // Rotation
            Yaw += look.x * yawSpeed * Time.deltaTime;

            float lookY = invertLookY ? -look.y : look.y;
            Pitch -= lookY * pitchSpeed * Time.deltaTime;
            Pitch = Mathf.Clamp(Pitch, minPitch, maxPitch);

            Quaternion rot = Quaternion.Euler(Pitch, Yaw, 0f);

            // Pivot position (ВАЖНО: цель + оффсет)
            Vector3 pivotPos = target.position + pivotOffset;
            cameraPivot.position = pivotPos;

            // "Shoulder" offset (чтобы не строго по центру)
            Vector3 shoulder = rot * shoulderOffset;

            // Origin для коллизии: pivot + плечо
            Vector3 origin = pivotPos + shoulder;

            // Desired position
            Vector3 desired = origin + rot * (Vector3.back * distance);

            // Collision: не даём камере уходить слишком близко и "проваливаться"
            float finalDist = distance;

            Vector3 dir = desired - origin;
            float len = dir.magnitude;

            if (len > 0.001f)
            {
                dir /= len;

                if (Physics.SphereCast(origin, sphereRadius, dir, out RaycastHit hit, len, collisionMask, QueryTriggerInteraction.Ignore))
                {
                    float hitDist = Mathf.Max(0f, hit.distance - collisionPadding);

                    // зажимаем дистанцию
                    float minD = Mathf.Max(minDistance, hardMinDistance);
                    finalDist = Mathf.Clamp(hitDist, minD, distance);
                }
            }

            Vector3 finalPos = origin + (rot * Vector3.back) * finalDist;

            cam.transform.position = finalPos;
            cam.transform.rotation = rot;
        }
    }
}
