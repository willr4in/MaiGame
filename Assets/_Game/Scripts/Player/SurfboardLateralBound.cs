using UnityEngine;

namespace SurfRush.Player
{
    /// <summary>
    /// Мягкое ограничение бокового движения доски.
    ///
    /// Если |position.x| > halfWidth, прикладываем возвращающую силу к Rigidbody
    /// пропорционально превышению. Это даёт «течение» — игрок ощущает сопротивление
    /// при попытке уплыть слишком далеко вбок, но не натыкается на резкую стену.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SurfboardLateralBound : MonoBehaviour
    {
        [Tooltip("Полуширина коридора. Должна совпадать с corridorHalfWidth у спавнера, чтобы препятствия и игрок жили в одном поле.")]
        [SerializeField] private float halfWidth = 25f;

        [Tooltip("Ускорение, прикладываемое на каждый метр превышения границы (м/с²/м).")]
        [SerializeField] private float restoreAcceleration = 6f;

        [Tooltip("Демпфирование боковой скорости, когда игрок за границей (1/с).")]
        [SerializeField] private float overflowLateralDamp = 2f;

        private Rigidbody _rb;

        public float HalfWidth { get => halfWidth; set => halfWidth = Mathf.Max(1f, value); }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            float x = transform.position.x;
            float overflow = 0f;
            if (x > halfWidth) overflow = x - halfWidth;
            else if (x < -halfWidth) overflow = x + halfWidth; // отрицательное

            if (Mathf.Approximately(overflow, 0f)) return;

            // Возвращающая сила: тянет к 0 пропорционально превышению.
            Vector3 restore = new Vector3(-Mathf.Sign(overflow) * restoreAcceleration * Mathf.Abs(overflow), 0f, 0f);
            _rb.AddForce(restore, ForceMode.Acceleration);

            // Дополнительное демпфирование скорости по X, чтобы не «вылететь» инерцией.
            Vector3 v = _rb.linearVelocity;
            _rb.AddForce(new Vector3(-v.x * overflowLateralDamp, 0f, 0f), ForceMode.Acceleration);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0.4f, 0.6f);
            Vector3 c = transform.position;
            float l = 80f;
            Gizmos.DrawLine(new Vector3(-halfWidth, c.y, c.z - l), new Vector3(-halfWidth, c.y, c.z + l));
            Gizmos.DrawLine(new Vector3( halfWidth, c.y, c.z - l), new Vector3( halfWidth, c.y, c.z + l));
        }
    }
}
