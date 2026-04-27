using UnityEngine;

namespace SurfRush.Ocean
{
    /// <summary>
    /// Единый сэмплер высоты и нормали океана. Меш океана и физика доски
    /// должны спрашивать высоту воды ТОЛЬКО здесь, чтобы не было рассинхрона
    /// между тем, что игрок видит, и тем, по чему он плывёт.
    ///
    /// Активный профиль и время задаются один раз за кадр (OceanBootstrap),
    /// после чего любое количество вызовов Sample() возвращают согласованный
    /// результат.
    /// </summary>
    public static class WaveField
    {
        public struct Sample
        {
            /// <summary>Смещённая позиция точки воды в мировых координатах (с учётом горизонтального схождения к гребню).</summary>
            public Vector3 position;
            /// <summary>Высота воды в этой точке (Y компонента position, для удобства).</summary>
            public float height;
            /// <summary>Нормаль поверхности воды (нормализованная).</summary>
            public Vector3 normal;
        }

        private static WaveProfile s_profile;
        private static float s_amplitudeMultiplier = 1f;
        private static float s_speedMultiplier = 1f;
        private static double s_overrideTime;
        private static bool s_useOverrideTime;

        public static WaveProfile ActiveProfile => s_profile;

        public static void SetActiveProfile(WaveProfile profile)
        {
            s_profile = profile;
        }

        /// <summary>Тестам/реплеям можно зафиксировать время. В обычной игре не вызывать —
        /// WaveField сам читает Time.timeAsDouble, что автоматически согласует Update и FixedUpdate.</summary>
        public static void SetTimeOverride(double time)
        {
            s_overrideTime = time;
            s_useOverrideTime = true;
        }

        public static void ClearTimeOverride() => s_useOverrideTime = false;

        public static void SetGlobalMultipliers(float amplitude, float speed)
        {
            s_amplitudeMultiplier = Mathf.Max(0f, amplitude);
            s_speedMultiplier = Mathf.Max(0f, speed);
        }

        private static double CurrentTime => s_useOverrideTime ? s_overrideTime : Time.timeAsDouble;

        /// <summary>Сэмплирует воду в плоских мировых координатах X/Z. Y во входе игнорируется.</summary>
        public static Sample SampleAt(Vector3 worldPos)
        {
            Sample s = new Sample
            {
                position = worldPos,
                height = 0f,
                normal = Vector3.up
            };

            if (s_profile == null || s_profile.waves == null || s_profile.waves.Length == 0)
                return s;

            float dx = 0f, dy = 0f, dz = 0f;
            float nx = 0f, nz = 0f;
            float nyDeficit = 0f; // 1 - nyDeficit = ny

            float ampMul = s_amplitudeMultiplier;
            float spdMul = s_speedMultiplier;
            float time = (float)CurrentTime;

            for (int i = 0; i < s_profile.waves.Length; i++)
            {
                GerstnerWave w = s_profile.waves[i];
                if (w.amplitude <= 0f || w.wavelength <= 0f)
                    continue;

                Vector2 dir = w.direction;
                float dirLen = dir.magnitude;
                if (dirLen < 1e-5f) continue;
                dir /= dirLen;

                float amplitude = w.amplitude * ampMul;
                float k = 2f * Mathf.PI / w.wavelength;                     // wavenumber
                float omega = Mathf.Sqrt(WaveProfile.Gravity * k) * w.speedMultiplier * spdMul;
                float phase = k * (dir.x * worldPos.x + dir.y * worldPos.z) - omega * time;

                float c = Mathf.Cos(phase);
                float sn = Mathf.Sin(phase);
                float qa = w.steepness * amplitude;
                float wa = omega * amplitude;

                // displacement
                dx += qa * dir.x * c;
                dz += qa * dir.y * c;
                dy += amplitude * sn;

                // normal accumulation (GPU Gems 1, ch.1 formulation)
                nx += dir.x * wa * c;
                nz += dir.y * wa * c;
                nyDeficit += w.steepness * wa * sn;
            }

            s.position = new Vector3(worldPos.x + dx, dy, worldPos.z + dz);
            s.height = dy;
            Vector3 n = new Vector3(-nx, 1f - nyDeficit, -nz);
            float nLen = n.magnitude;
            s.normal = nLen > 1e-5f ? n / nLen : Vector3.up;
            return s;
        }

        /// <summary>Быстрая версия — только высота. Используй для физики, где не нужны
        /// горизонтальные смещения и нормаль.</summary>
        public static float SampleHeight(float worldX, float worldZ)
        {
            if (s_profile == null || s_profile.waves == null) return 0f;

            float dy = 0f;
            float ampMul = s_amplitudeMultiplier;
            float spdMul = s_speedMultiplier;
            float time = (float)CurrentTime;

            for (int i = 0; i < s_profile.waves.Length; i++)
            {
                GerstnerWave w = s_profile.waves[i];
                if (w.amplitude <= 0f || w.wavelength <= 0f) continue;

                Vector2 dir = w.direction;
                float dirLen = dir.magnitude;
                if (dirLen < 1e-5f) continue;
                dir /= dirLen;

                float amplitude = w.amplitude * ampMul;
                float k = 2f * Mathf.PI / w.wavelength;
                float omega = Mathf.Sqrt(WaveProfile.Gravity * k) * w.speedMultiplier * spdMul;
                float phase = k * (dir.x * worldX + dir.y * worldZ) - omega * time;
                dy += amplitude * Mathf.Sin(phase);
            }
            return dy;
        }
    }
}
