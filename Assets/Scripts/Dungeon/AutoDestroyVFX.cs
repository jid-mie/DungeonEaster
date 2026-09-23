using UnityEngine;

namespace DungeonEater.Dungeon
{
    /// <summary>
    /// Auto-destroying visual effect handler that plays animation frames or destroys itself after duration.
    /// </summary>
    public class AutoDestroyVFX : MonoBehaviour
    {
        [SerializeField] private float duration = 0.4f;
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float frameRate = 12f;

        private SpriteRenderer sr;
        private float timer = 0f;
        private int currentFrame = 0;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        public void Initialize(Sprite[] animFrames, float fps = 12f)
        {
            frames = animFrames;
            frameRate = fps;
            if (frames != null && frames.Length > 0)
            {
                duration = frames.Length / frameRate;
                if (sr != null) sr.sprite = frames[0];
            }
        }

        private void Start()
        {
            Destroy(gameObject, duration);
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0 || sr == null) return;

            timer += Time.deltaTime;
            currentFrame = Mathf.FloorToInt(timer * frameRate);

            if (currentFrame < frames.Length)
            {
                sr.sprite = frames[currentFrame];
            }
        }
    }
}

