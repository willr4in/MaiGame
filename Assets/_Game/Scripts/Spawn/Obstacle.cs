using SurfRush.Ocean;
using UnityEngine;

namespace SurfRush.Spawn
{
    /// <summary>
    /// Препятствие на воде. Опционально «привязано» к высоте воды (плавает),
    /// но не реагирует на физику — это статичная декорация с коллайдером.
    /// Возвращается в пул через ObstaclePool, когда отъезжает далеко за игрока.
    /// </summary>
    [DisallowMultipleComponent]
    public class Obstacle : MonoBehaviour
    {
        [Tooltip("Если true — каждый кадр Y подгоняется под высоту воды + verticalOffset (риф колышется).")]
        [SerializeField] private bool floatOnWater = true;

        [Tooltip("Сдвиг по Y относительно поверхности воды. Положительный — над водой, отрицательный — частично под водой.")]
        [SerializeField] private float verticalOffset = -0.4f;

        [Tooltip("Источник, который заспавнил препятствие — нужно для возврата в пул. Заполняется автоматически.")]
        public ObstaclePool OwnerPool { get; set; }

        [Tooltip("Префаб-источник — нужно пулу, чтобы знать в какой стек возвращать. Заполняется автоматически.")]
        public Obstacle SourcePrefab { get; set; }

        private void LateUpdate()
        {
            if (!floatOnWater) return;
            float wy = WaveField.SampleHeight(transform.position.x, transform.position.z);
            Vector3 p = transform.position;
            p.y = wy + verticalOffset;
            transform.position = p;
        }
    }
}
