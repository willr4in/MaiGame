using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurfRush.Core
{
    /// <summary>
    /// Главный менеджер игры. Держит состояние, жизни, рассылает события.
    /// Один экземпляр на сцене, доступ через GameManager.Instance.
    ///
    /// Лайв-логика: игрок начинает с maxLives. Каждое столкновение снимает одну
    /// жизнь и активирует короткую неуязвимость, чтобы один продолжительный
    /// контакт с препятствием не съел все жизни сразу. Когда lives = 0 →
    /// триггерим GameOver.
    /// </summary>
    [DefaultExecutionOrder(-2000)]
    public class GameManager : MonoBehaviour
    {
        public enum State { Playing, GameOver }

        public static GameManager Instance { get; private set; }

        [Header("Жизни")]
        [SerializeField, Min(1)] private int maxLives = 3;
        [Tooltip("Сколько секунд игрок неуязвим после удара (i-frames).")]
        [SerializeField, Min(0f)] private float invulnerabilityDuration = 1.5f;

        public State CurrentState { get; private set; } = State.Playing;
        public int Lives { get; private set; }
        public int MaxLives => maxLives;
        public bool IsInvulnerable => Time.time < _invulnerableUntil;

        public event Action<State> OnStateChanged;
        public event Action<int> OnLivesChanged;
        public event Action OnDamaged; // снятие реальной жизни (для UI/FX)
        public event Action OnHit;     // любое касание препятствия, даже во время i-frames (для звука)

        private float _invulnerableUntil;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Lives = maxLives;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Снять одну жизнь. Если игрок неуязвим или уже GameOver — урон игнорируется,
        /// но событие OnHit всё равно срабатывает (для звука/FX касания).</summary>
        public void TakeDamage()
        {
            if (CurrentState != State.Playing) return;

            OnHit?.Invoke();

            if (IsInvulnerable) return;

            Lives--;
            _invulnerableUntil = Time.time + invulnerabilityDuration;
            OnDamaged?.Invoke();
            OnLivesChanged?.Invoke(Lives);

            if (Lives <= 0) TriggerGameOver();
        }

        public void TriggerGameOver()
        {
            if (CurrentState == State.GameOver) return;
            CurrentState = State.GameOver;
            // Останавливаем игровое время. UI работает на unscaledTime, поэтому реагирует.
            Time.timeScale = 0f;
            OnStateChanged?.Invoke(CurrentState);
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
