using UnityEngine;

namespace SurfRush.Ocean
{
    /// <summary>
    /// Диагностический «щуп»: ставится в сцену, рисует gizmo-крестик на
    /// высоте воды под собой и нормаль. Если меш и щуп показывают разную
    /// высоту в одной точке — сэмплер рассинхронизирован с мешем.
    /// Используется для проверки Фазы 1.
    /// </summary>
    [ExecuteAlways]
    public class WaveProbe : MonoBehaviour
    {
        [SerializeField] private Color color = new Color(1f, 0.8f, 0.2f, 1f);
        [SerializeField, Min(0.1f)] private float crossSize = 0.5f;
        [SerializeField, Min(0.1f)] private float normalLength = 1.5f;

        private void OnDrawGizmos()
        {
            Vector3 p = transform.position;
            WaveField.Sample s = WaveField.SampleAt(new Vector3(p.x, 0f, p.z));

            Gizmos.color = color;
            Vector3 surface = s.position;
            Gizmos.DrawLine(surface + Vector3.left * crossSize,  surface + Vector3.right * crossSize);
            Gizmos.DrawLine(surface + Vector3.forward * crossSize, surface + Vector3.back * crossSize);
            Gizmos.DrawSphere(surface, crossSize * 0.2f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(surface, surface + s.normal * normalLength);

            // Тонкая линия от объекта вниз к поверхности — видно где «зонд» относительно воды.
            Gizmos.color = new Color(color.r, color.g, color.b, 0.3f);
            Gizmos.DrawLine(p, surface);
        }
    }
}
