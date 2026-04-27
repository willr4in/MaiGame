using System;
using UnityEngine;

namespace SurfRush.Ocean
{
    /// <summary>
    /// Параметры одной Gerstner-волны. Suммируя несколько таких волн с разными
    /// направлениями и длинами, мы получаем правдоподобный океан.
    /// </summary>
    [Serializable]
    public struct GerstnerWave
    {
        [Tooltip("Направление распространения волны на горизонтальной плоскости (XZ). Нормализуется автоматически.")]
        public Vector2 direction;

        [Tooltip("Длина волны в метрах (расстояние между гребнями).")]
        [Min(0.01f)] public float wavelength;

        [Tooltip("Высота гребня от среднего уровня воды, м.")]
        [Min(0f)] public float amplitude;

        [Range(0f, 1f)]
        [Tooltip("Steepness (Q) — насколько вершины собираются в гребень. 0 = синус, 1 = острый гребень. Сумма Q*A*k по всем волнам не должна превышать 1, иначе вершины будут пересекаться.")]
        public float steepness;

        [Tooltip("Множитель к фазовой скорости. Базовая скорость = sqrt(g/k) (deep-water gravity wave).")]
        [Min(0f)] public float speedMultiplier;
    }
}
