using UnityEngine;

namespace DungeonEater.Core
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource bgmSource;

        [Header("SFX Clips")]
        public AudioClip swordSlashClip;
        public AudioClip swordHeavyClip;
        public AudioClip bowShootClip;
        public AudioClip hitClip;
        public AudioClip dashClip;
        public AudioClip explosionClip;
        public AudioClip pickupClip;
        public AudioClip coinClip;
        public AudioClip cookClip;
        public AudioClip serveSuccessClip;
        public AudioClip customerAngryClip;

        [Header("BGM Clips")]
        public AudioClip dungeonBgm;
        public AudioClip restaurantBgm;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.volume = 0.4f;
            }
        }

        public void PlaySfx(AudioClip clip, float volume = 1f)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
        }

        public void PlayBgm(AudioClip clip)
        {
            if (clip == null || bgmSource == null) return;
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.clip = clip;
            bgmSource.Play();
        }

        public void PlaySwordSlash() => PlaySfx(swordSlashClip);
        public void PlaySwordHeavy() => PlaySfx(swordHeavyClip, 1.2f);
        public void PlayBowShoot() => PlaySfx(bowShootClip);
        public void PlayHit() => PlaySfx(hitClip);
        public void PlayDash() => PlaySfx(dashClip, 0.8f);
        public void PlayExplosion() => PlaySfx(explosionClip);
        public void PlayPickup() => PlaySfx(pickupClip, 0.9f);
        public void PlayCoin() => PlaySfx(coinClip);
        public void PlayCook() => PlaySfx(cookClip);
        public void PlayServeSuccess() => PlaySfx(serveSuccessClip);
        public void PlayCustomerAngry() => PlaySfx(customerAngryClip);
    }
}

