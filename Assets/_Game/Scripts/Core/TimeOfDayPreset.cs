using SurfRush.Spawn;
using UnityEngine;

namespace SurfRush.Core
{
    /// <summary>
    /// Полный визуально-игровой пресет для одного «времени суток»:
    /// skybox, параметры солнца, тема препятствий.
    /// Один ScriptableObject на каждый режим (Day, Evening).
    /// </summary>
    [CreateAssetMenu(fileName = "TimeOfDayPreset", menuName = "SurfRush/Time Of Day Preset")]
    public class TimeOfDayPreset : ScriptableObject
    {
        [Header("Skybox")]
        public Material skyboxMaterial;

        [Header("Солнце (Directional Light)")]
        public Vector3 sunRotation = new Vector3(50f, -30f, 0f);
        [ColorUsage(false, false)]
        public Color sunColor = Color.white;
        [Min(0f)] public float sunIntensity = 1.5f;

        [Header("Препятствия")]
        public ObstacleTheme obstacleTheme;
    }
}
