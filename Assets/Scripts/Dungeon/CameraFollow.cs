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

        public void TriggerScreenShake(float duration, float magnitude)
        {
            shakeDuration = duration;
            shakeMagnitude = magnitude;
        }
    }
}

