using System;
using UnityEngine;

namespace SurfRush.Core
{
    /// <summary>
    /// Очки начисляются за пройденную дистанцию вдоль +Z (трекаем Transform игрока).
    /// Стоишь — счёт стоит. Едешь быстрее — очки идут быстрее.
    /// Множитель растёт со временем без удара, при OnDamaged сбрасывается до minMultiplier.
    /// Лучший результат сохраняется в PlayerPrefs.
    /// </summary>
    public class ScoreSystem : MonoBehaviour
    {
        private const string BestScoreKey = "SurfRush.BestScore";

        [Header("Что трекаем")]
        [SerializeField] private Transform tracked;

        [Header("Начисление")]
        [SerializeField] private float pointsPerMeter = 5f;

        [Header("Множитель")]
        [SerializeField] private float minMultiplier = 1f;
        [SerializeField] private float maxMultiplier = 5f;
        [SerializeField] private float multiplierGrowthPerSecond = 0.05f;

        public static ScoreSystem Instance { get; private set; }

        public float CurrentScore { get; private set; }
        public int CurrentScoreInt => Mathf.FloorToInt(CurrentScore);
        public int BestScore { get; private set; }
        public float Multiplier { get; private set; } = 1f;

        public event Action<int> OnScoreChanged;
        public event Action<int> OnBestScoreChanged;

        private float _lastTrackedZ;
        private bool _hasBaseline;
        private GameManager _gm;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
            Multiplier = minMultiplier;
        }

        private void Start()
        {
            _gm = GameManager.Instance;
            if (_gm != null) _gm.OnDamaged += OnDamaged;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_gm != null) _gm.OnDamaged -= OnDamaged;
        }

        private void Update()
        {
            if (_gm != null && _gm.CurrentState != GameManager.State.Playing) return;
            if (tracked == null) return;

            float z = tracked.position.z;
            if (!_hasBaseline)
            {
                _lastTrackedZ = z;
                _hasBaseline = true;
                return;
            }

            float dz = z - _lastTrackedZ;
            _lastTrackedZ = z;

            Multiplier = Mathf.Min(maxMultiplier, Multiplier + multiplierGrowthPerSecond * Time.deltaTime);

            if (dz > 0f)
            {
                int prev = CurrentScoreInt;
                CurrentScore += dz * pointsPerMeter * Multiplier;
                int now = CurrentScoreInt;
                if (now != prev) OnScoreChanged?.Invoke(now);

                if (CurrentScoreInt > BestScore)
                {
                    BestScore = CurrentScoreInt;
                    PlayerPrefs.SetInt(BestScoreKey, BestScore);
                    OnBestScoreChanged?.Invoke(BestScore);
                }
            }
        }

        private void OnDamaged()
        {
            Multiplier = minMultiplier;
        }
    }
}
