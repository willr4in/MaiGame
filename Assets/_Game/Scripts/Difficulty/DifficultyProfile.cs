using UnityEngine;

namespace SurfRush.Difficulty
{
    /// <summary>
    /// Профиль сложности: набор кривых, по которым DifficultyController
    /// со временем модулирует мир. Все кривые принимают на вход playTime в
    /// секундах и возвращают значение конкретной характеристики.
    ///
    /// Edit в Inspector: двойной клик по кривой → редактор Animation Curve.
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyProfile", menuName = "SurfRush/Difficulty Profile", order = 2)]
    public class DifficultyProfile : ScriptableObject
    {
        [Header("Океан")]
        [Tooltip("Глобальный множитель амплитуды волн от времени (с). Стартует обычно 1, доходит до 2-3.")]
        public AnimationCurve waveAmplitudeMultiplier = AnimationCurve.Linear(0f, 1f, 120f, 2.5f);

        [Tooltip("Глобальный множитель скорости волн от времени (с). Стартует 1, плавно растёт.")]
        public AnimationCurve waveSpeedMultiplier = AnimationCurve.Linear(0f, 1f, 120f, 1.5f);

        [Header("Спавн препятствий")]
        [Tooltip("Интервал спавна (с). Стартует медленно (~2с), к концу — быстро (~0.4с).")]
        public AnimationCurve spawnInterval = AnimationCurve.Linear(0f, 2f, 120f, 0.5f);

        [Tooltip("Количество препятствий в одной волне спавна. Стартует ~2, доходит до 6.")]
        public AnimationCurve amountPerWave = AnimationCurve.Linear(0f, 2f, 120f, 6f);

        [Tooltip("Полуширина коридора (м). Можно держать константной или плавно расширять.")]
        public AnimationCurve corridorHalfWidth = AnimationCurve.Constant(0f, 120f, 25f);
    }
}
