using SurfRush.Ocean;
using UnityEngine;

namespace SurfRush.Player
{
    /// <summary>
    /// Главный «sauce» серфинга: проецирует гравитацию на касательную плоскость
    /// волны под доской и прикладывает результат как ускоряющую силу. Так,
    /// съезжая со склона гребня, доска ускоряется, а заехав на встречный склон —
    /// тормозит. Дополнительно стабилизирует ориентацию доски по нормали воды.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SurfboardSlopePropulsion : MonoBehaviour
    {
        [SerializeField] private SurfboardConfig config;

        [Tooltip("Точка под доской, в которой берём нормаль воды (обычно центр).")]
        [SerializeField] private Vector3 sampleAnchorLocal = Vector3.zero;

        private Rigidbody _rb;

        public SurfboardConfig Config { get => config; set => config = value; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (config == null) return;

            Vector3 worldAnchor = transform.TransformPoint(sampleAnchorLocal);
            WaveField.Sample s = WaveField.SampleAt(new Vector3(worldAnchor.x, 0f, worldAnchor.z));

            // Если якорь высоко над водой — ничего не делаем (доска в полёте).
            float depth = s.height - worldAnchor.y;
            if (depth < -0.5f) return;

            Vector3 normal = s.normal;

            // Сила скатывания: проекция g на касательную к воде.
            // Если вода плоская — нет горизонтальной компоненты, силы нет.
            Vector3 gravity = Physics.gravity;
            Vector3 slopeForce = Vector3.ProjectOnPlane(gravity, normal) * config.slopeAccelMultiplier;
            _rb.AddForce(slopeForce, ForceMode.Acceleration);

            // Сопротивление воды: разделяем скорость на forward/lateral составляющие
            // и тормозим их разными коэффициентами.
            Vector3 vel = _rb.linearVelocity;
            // Игнорируем вертикальную компоненту здесь — ею занимается buoyancy damping.
            Vector3 horizVel = new Vector3(vel.x, 0f, vel.z);
            Vector3 forward = transform.forward; forward.y = 0f;
            float fwLen = forward.sqrMagnitude;
            if (fwLen < 1e-4f) return;
            forward /= Mathf.Sqrt(fwLen);
            Vector3 right = new Vector3(forward.z, 0f, -forward.x); // 90° вправо в XZ

            float vForward = Vector3.Dot(horizVel, forward);
            float vLateral = Vector3.Dot(horizVel, right);

            Vector3 dragAccel = -forward * (vForward * config.forwardDrag)
                              - right   * (vLateral * config.lateralDrag);
            _rb.AddForce(dragAccel, ForceMode.Acceleration);

            // Мягкая стабилизация ориентации по нормали воды.
            // Берём желаемую ротацию: up = нормаль, forward = текущий forward, спроецированный на касательную.
            Vector3 desiredForward = Vector3.ProjectOnPlane(transform.forward, normal);
            if (desiredForward.sqrMagnitude < 1e-4f) return;
            Quaternion desired = Quaternion.LookRotation(desiredForward.normalized, normal);
            Quaternion delta = desired * Quaternion.Inverse(transform.rotation);
            delta.ToAngleAxis(out float angleDeg, out Vector3 axis);
            if (angleDeg > 180f) angleDeg -= 360f;
            if (axis.sqrMagnitude > 1e-6f)
            {
                Vector3 angularAccel = axis.normalized * (angleDeg * Mathf.Deg2Rad * config.alignTorqueStrength);
                // Демпфирование угловой скорости.
                angularAccel -= _rb.angularVelocity * config.angularDamping;
                _rb.AddTorque(angularAccel, ForceMode.Acceleration);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 wp = transform.TransformPoint(sampleAnchorLocal);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(wp, 0.1f);
            if (Application.isPlaying)
            {
                WaveField.Sample s = WaveField.SampleAt(new Vector3(wp.x, 0f, wp.z));
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(s.position, s.position + s.normal * 1.5f);
            }
        }
    }
}
