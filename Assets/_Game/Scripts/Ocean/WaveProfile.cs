using UnityEngine;

namespace SurfRush.Ocean
{
    /// <summary>
    /// Набор Gerstner-волн, описывающих текущее состояние океана.
    /// DifficultyController со временем подкручивает amplitudeMultiplier и speedMultiplier
    /// в WaveField, чтобы волны становились выше и быстрее.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveProfile", menuName = "SurfRush/Wave Profile", order = 0)]
    public class WaveProfile : ScriptableObject
    {
        public const float Gravity = 9.81f;

        [Tooltip("Список независимых волн. Складываются в WaveField.")]
        public GerstnerWave[] waves = new GerstnerWave[]
        {
            new GerstnerWave { direction = new Vector2(1f, 0.2f), wavelength = 28f,  amplitude = 0.55f, steepness = 0.55f, speedMultiplier = 1f },
            new GerstnerWave { direction = new Vector2(0.7f, 0.7f), wavelength = 14f, amplitude = 0.30f, steepness = 0.45f, speedMultiplier = 1f },
            new GerstnerWave { direction = new Vector2(0.3f, -1f), wavelength = 8f,  amplitude = 0.18f, steepness = 0.40f, speedMultiplier = 1.1f },
            new GerstnerWave { direction = new Vector2(-0.4f, 0.6f),wavelength = 4f,  amplitude = 0.08f, steepness = 0.35f, speedMultiplier = 1.2f },
        };
    }
}
