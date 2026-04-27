using UnityEngine;

namespace SurfRush.Ocean
{
    /// <summary>
    /// Назначает активный WaveProfile в WaveField. Один такой компонент на сцене.
    /// В будущем сюда же подключим DifficultyController, который будет крутить
    /// глобальные множители амплитуды/скорости.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class OceanController : MonoBehaviour
    {
        [SerializeField] private WaveProfile activeProfile;
        [SerializeField, Min(0f)] private float globalAmplitude = 1f;
        [SerializeField, Min(0f)] private float globalSpeed = 1f;

        public WaveProfile ActiveProfile => activeProfile;

        private void OnEnable()
        {
            WaveField.SetActiveProfile(activeProfile);
            WaveField.SetGlobalMultipliers(globalAmplitude, globalSpeed);
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                WaveField.SetActiveProfile(activeProfile);
                WaveField.SetGlobalMultipliers(globalAmplitude, globalSpeed);
            }
        }
    }
}
