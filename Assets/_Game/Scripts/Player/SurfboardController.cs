using SurfRush.Ocean;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SurfRush.Player
{
    /// <summary>
    /// Управление доской: рулёжка yaw-моментом + короткий буст «pump».
    /// Работает только когда доска касается воды (один из якорей под водой),
    /// чтобы в полёте серфер не вращался волшебным образом.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SurfboardController : MonoBehaviour
    {
        [Header("Ввод")]
        [Tooltip("Action типа Value/Axis (1D), от -1 до 1.")]
        [SerializeField] private InputActionReference steerAction;
        [Tooltip("Action типа Button.")]
        [SerializeField] private InputActionReference pumpAction;

        [Header("Параметры")]
        [Tooltip("Угловое ускорение, прикладываемое к доске вокруг мировой оси Y, на единицу ввода Steer.")]
        [SerializeField] private float steerTorque = 6f;

        [Tooltip("Импульс ускорения вперёд при нажатии Pump (м/с² × длительность).")]
        [SerializeField] private float pumpAcceleration = 12f;

        [Tooltip("Длительность одного pump-импульса в секундах.")]
        [SerializeField] private float pumpDuration = 0.35f;

        [Tooltip("Cooldown между нажатиями pump.")]
        [SerializeField] private float pumpCooldown = 0.6f;

        private Rigidbody _rb;
        private float _pumpTimer;
        private float _pumpCooldownTimer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            if (steerAction != null) steerAction.action.Enable();
            if (pumpAction != null)
            {
                pumpAction.action.Enable();
                pumpAction.action.performed += OnPumpPerformed;
            }
        }

        private void OnDisable()
        {
            if (steerAction != null) steerAction.action.Disable();
            if (pumpAction != null)
            {
                pumpAction.action.performed -= OnPumpPerformed;
                pumpAction.action.Disable();
            }
        }

        private void OnPumpPerformed(InputAction.CallbackContext _)
        {
            if (_pumpCooldownTimer > 0f) return;
            if (!IsOnWater()) return;
            _pumpTimer = pumpDuration;
            _pumpCooldownTimer = pumpCooldown;
        }

        private bool IsOnWater()
        {
            // Простой тест: центр доски не выше чем 0.5м над текущей высотой воды.
            float waterY = WaveField.SampleHeight(transform.position.x, transform.position.z);
            return transform.position.y - waterY < 0.5f;
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            if (_pumpCooldownTimer > 0f) _pumpCooldownTimer -= dt;

            if (!IsOnWater())
            {
                _pumpTimer = 0f;
                return;
            }

            // Steer → yaw torque вокруг мировой оси Y. Не используем locale up,
            // потому что доска может быть наклонена вбок и тогда «yaw» в её
            // локалке выглядит странно для игрока.
            float steer = steerAction != null ? steerAction.action.ReadValue<float>() : 0f;
            if (Mathf.Abs(steer) > 0.01f)
            {
                _rb.AddTorque(Vector3.up * (steer * steerTorque), ForceMode.Acceleration);
            }

            // Pump → forward acceleration пока таймер активен.
            if (_pumpTimer > 0f)
            {
                _pumpTimer -= dt;
                Vector3 fwd = transform.forward;
                fwd.y = 0f;
                if (fwd.sqrMagnitude > 1e-4f)
                {
                    fwd.Normalize();
                    _rb.AddForce(fwd * pumpAcceleration, ForceMode.Acceleration);
                }
            }
        }
    }
}
