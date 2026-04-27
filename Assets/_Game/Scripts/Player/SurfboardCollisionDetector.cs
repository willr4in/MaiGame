using SurfRush.Core;
using SurfRush.Spawn;
using UnityEngine;

namespace SurfRush.Player
{
    /// <summary>
    /// Слушает столкновения доски с препятствиями и снимает жизни.
    /// Логику i-frames (неуязвимости после удара) держит сам GameManager,
    /// поэтому здесь — просто детект и вызов TakeDamage().
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SurfboardCollisionDetector : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            // Был ли это Obstacle? Используем GetComponentInParent на случай,
            // если коллайдер сидит на дочернем объекте (визуальная модель).
            Obstacle obstacle = collision.collider.GetComponentInParent<Obstacle>();
            if (obstacle == null) return;

            if (GameManager.Instance != null)
                GameManager.Instance.TakeDamage();
        }
    }
}
