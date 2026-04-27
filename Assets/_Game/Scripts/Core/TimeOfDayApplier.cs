using SurfRush.Spawn;
using UnityEngine;

namespace SurfRush.Core
{
    /// <summary>
    /// При запуске сцены читает GameSettings.TimeOfDay и применяет
    /// соответствующий TimeOfDayPreset: меняет skybox, параметры
    /// Directional Light и тему препятствий у спавнера.
    /// Запускается в Awake — до Start спавнера, чтобы тот при старте
    /// уже видел нужную тему.
    /// </summary>
    [DefaultExecutionOrder(-3000)]
    public class TimeOfDayApplier : MonoBehaviour
    {
        [SerializeField] private TimeOfDayPreset dayPreset;
        [SerializeField] private TimeOfDayPreset eveningPreset;
        [SerializeField] private Light sun;
        [SerializeField] private ObstacleSpawner spawner;

        private void Awake()
        {
            TimeOfDayPreset preset = GameSettings.TimeOfDay == TimeOfDay.Day
                ? dayPreset
                : eveningPreset;
            if (preset == null)
            {
                Debug.LogWarning("[TimeOfDayApplier] Пресет не назначен.");
                return;
            }
            Apply(preset);
        }

        private void Apply(TimeOfDayPreset p)
        {
            if (p.skyboxMaterial != null) RenderSettings.skybox = p.skyboxMaterial;

            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Euler(p.sunRotation);
                sun.color = p.sunColor;
                sun.intensity = p.sunIntensity;
            }

            if (spawner != null && p.obstacleTheme != null)
                spawner.Theme = p.obstacleTheme;

            DynamicGI.UpdateEnvironment();
        }
    }
}
