using UnityEngine;

namespace SurfRush.Core
{
    /// <summary>
    /// Простой аудио-менеджер для игровой сцены: фоновая музыка (Day/Evening),
    /// ambient-петля океана, one-shot звук удара по препятствию.
    /// Подписан на GameManager.OnDamaged и автоматически проигрывает hit-звук.
    /// </summary>
    [DefaultExecutionOrder(-1500)]
    public class AudioManager : MonoBehaviour
    {
        [Header("Музыка")]
        [SerializeField] private AudioClip musicDay;
        [SerializeField] private AudioClip musicEvening;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;

        [Header("Ambient (петля океана)")]
        [SerializeField] private AudioClip oceanLoop;
        [SerializeField, Range(0f, 1f)] private float ambientVolume = 0.6f;

        [Header("SFX")]
        [Tooltip("Один из этих звуков случайно играется при ударе.")]
        [SerializeField] private AudioClip[] hitSounds;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.9f;

        private AudioSource _music;
        private AudioSource _ambient;
        private AudioSource _sfx;

        private void Awake()
        {
            _music = gameObject.AddComponent<AudioSource>();
            _music.loop = true;
            _music.playOnAwake = false;
            _music.volume = musicVolume;

            _ambient = gameObject.AddComponent<AudioSource>();
            _ambient.loop = true;
            _ambient.playOnAwake = false;
            _ambient.volume = ambientVolume;

            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.loop = false;
            _sfx.playOnAwake = false;
            _sfx.volume = sfxVolume;
        }

        private void Start()
        {
            AudioClip music = GameSettings.TimeOfDay == TimeOfDay.Day ? musicDay : musicEvening;
            if (music == null) music = musicDay;
            if (music != null)
            {
                _music.clip = music;
                _music.Play();
            }

            if (oceanLoop != null)
            {
                _ambient.clip = oceanLoop;
                _ambient.Play();
            }

            if (GameManager.Instance != null)
                GameManager.Instance.OnHit += PlayHit;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnHit -= PlayHit;
        }

        private void PlayHit()
        {
            if (hitSounds == null || hitSounds.Length == 0) return;
            AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
            if (clip != null) _sfx.PlayOneShot(clip, sfxVolume);
        }
    }
}
