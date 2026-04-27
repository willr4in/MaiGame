using SurfRush.Ocean;
using UnityEngine;

namespace SurfRush.Player
{
    /// <summary>
    /// Плавучесть «по точкам». На доске задаётся 4 (или больше) точек-якорей.
    /// Каждый FixedUpdate для каждой точки сэмплируется высота воды и
    /// прикладывается вертикальная сила пропорционально глубине погружения.
    ///
    /// Так как каждая точка чувствует свою высоту воды, доска естественно
    /// принимает наклон склона — не нужно отдельно «крутить» её ротацию.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SurfboardBuoyancy : MonoBehaviour
    {
        [SerializeField] private SurfboardConfig config;

        [Tooltip("Локальные позиции якорей плавучести (обычно 4 угла доски). Если пусто — берётся центр объекта.")]
        [SerializeField] private Vector3[] anchorsLocal;

        private Rigidbody _rb;

        public SurfboardConfig Config { get => config; set => config = value; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            if (anchorsLocal == null || anchorsLocal.Length == 0)
            {
                anchorsLocal = new[] { Vector3.zero };
            }
        }

        private void FixedUpdate()
        {
            if (config == null) return;

            // Вес распределяется поровну по якорям, чтобы суммарная плавучесть
            // не зависела от их количества.
            float perAnchorScale = 1f / anchorsLocal.Length;

            for (int i = 0; i < anchorsLocal.Length; i++)
            {
                Vector3 worldPos = transform.TransformPoint(anchorsLocal[i]);
                float waterY = WaveField.SampleHeight(worldPos.x, worldPos.z);
                float depth = waterY - worldPos.y;

                if (depth <= 0f)
                    continue; // якорь над водой — никакой плавучести

                float effectiveDepth = Mathf.Min(depth, config.buoyancySaturationDepth);
                float buoyForce = config.buoyancyStrength * effectiveDepth * perAnchorScale;

                Vector3 force = Vector3.up * buoyForce;

                // Демпфирование вертикальной скорости в этой точке.
                Vector3 pointVel = _rb.GetPointVelocity(worldPos);
                force += Vector3.up * (-pointVel.y * config.verticalDamping * perAnchorScale);

                _rb.AddForceAtPosition(force, worldPos, ForceMode.Acceleration);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (anchorsLocal == null) return;
            Gizmos.color = new Color(0.2f, 1f, 0.5f, 0.9f);
            for (int i = 0; i < anchorsLocal.Length; i++)
            {
                Vector3 wp = transform.TransformPoint(anchorsLocal[i]);
                Gizmos.DrawSphere(wp, 0.08f);

                // Нарисуем вертикаль до текущей высоты воды.
                if (Application.isPlaying)
                {
                    float wy = WaveField.SampleHeight(wp.x, wp.z);
                    Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.6f);
                    Gizmos.DrawLine(wp, new Vector3(wp.x, wy, wp.z));
                    Gizmos.color = new Color(0.2f, 1f, 0.5f, 0.9f);
                }
            }
        }
    }
}
