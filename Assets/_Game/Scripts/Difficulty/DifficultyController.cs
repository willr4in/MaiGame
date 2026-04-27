using SurfRush.Ocean;
using SurfRush.Spawn;
using UnityEngine;

namespace SurfRush.Difficulty
{
    /// <summary>
    /// Главный «крутильщик» сложности. Каждый кадр:
    /// 1. Считает playTime (время с момента включения).
    /// 2. Сэмплит каждую кривую профиля.
    /// 3. Применяет результаты к WaveField и ObstacleSpawner.
    ///
    /// При желании можно поставить ручку Time Scale для тестов
    /// (ускоренная прокрутка кривой без ожидания реальных секунд).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class DifficultyController : MonoBehaviour
    {
        [SerializeField] private DifficultyProfile profile;
        [SerializeField] private ObstacleSpawner spawner;

        [Tooltip("Множитель к реальному времени для отладки сложности. 1 = реальное время, 5 = в 5 раз быстрее проматывает кривые.")]
        [SerializeField, Min(0.1f)] private float timeScale = 1f;

        [Tooltip("Если true — playTime сбрасывается при старте сцены (новая попытка). Если false — копится между перезагрузками профиля.")]
        [SerializeField] private bool resetOnEnable = true;

        private float _playTime;

        public float PlayTime => _playTime;
        public DifficultyProfile Profile { get => profile; set => profile = value; }

        private void OnEnable()
        {
            if (resetOnEnable) _playTime = 0f;
        }

        private void Update()
        {
            if (profile == null) return;

            _playTime += Time.deltaTime * timeScale;

            float ampMul = profile.waveAmplitudeMultiplier.Evaluate(_playTime);
            float spdMul = profile.waveSpeedMultiplier.Evaluate(_playTime);
            WaveField.SetGlobalMultipliers(ampMul, spdMul);

            if (spawner != null)
            {
                spawner.SpawnInterval = profile.spawnInterval.Evaluate(_playTime);
                spawner.AmountPerWave = Mathf.RoundToInt(profile.amountPerWave.Evaluate(_playTime));
                spawner.CorridorHalfWidth = profile.corridorHalfWidth.Evaluate(_playTime);
            }
        }
    }
}
