using UnityEngine;

namespace DungeonEater.Dungeon
{
    public class CameraFollow : MonoBehaviour
    {
        public static CameraFollow Instance { get; private set; }

        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 8f;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

        [Header("Bounds (optional)")]
        [Tooltip("Giữ khung hình nằm trong vùng bản đồ, tránh lộ viền đen ngoài ảnh nền")]
        [SerializeField] private bool clampToBounds = false;
        [SerializeField] private Rect worldBounds = new Rect(-6f, -4.48f, 12f, 8.96f);

        [Header("Screen Shake")]
        private float shakeDuration = 0f;
        private float shakeMagnitude = 0f;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (target == null)
            {
                var player = FindObjectOfType<PlayerController>();
                if (player != null) target = player.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            if (clampToBounds) smoothedPosition = ClampToBounds(smoothedPosition);

            if (shakeDuration > 0f)
            {
                Vector3 shakeOffset = (Vector3)(Random.insideUnitCircle * shakeMagnitude);
                shakeOffset.z = 0;
                transform.position = smoothedPosition + shakeOffset;

                shakeDuration -= Time.deltaTime;
            }
            else
            {
                transform.position = smoothedPosition;
            }
        }

        private Vector3 ClampToBounds(Vector3 pos)
        {
            var cam = GetComponent<Camera>();
            if (cam == null || !cam.orthographic) return pos;

            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            pos.x = halfW * 2f >= worldBounds.width ? worldBounds.center.x : Mathf.Clamp(pos.x, worldBounds.xMin + halfW, worldBounds.xMax - halfW);
            pos.y = halfH * 2f >= worldBounds.height ? worldBounds.center.y : Mathf.Clamp(pos.y, worldBounds.yMin + halfH, worldBounds.yMax - halfH);
            return pos;
        }

        public void TriggerScreenShake(float duration, float magnitude)
        {
            shakeDuration = duration;
            shakeMagnitude = magnitude;
        }
    }
}

