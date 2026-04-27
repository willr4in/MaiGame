using UnityEngine;

namespace SurfRush.Player
{
    /// <summary>
    /// Все настраиваемые параметры физики доски — в одном месте.
    /// Это даёт возможность держать несколько профилей (training board, pro board)
    /// и переключаться между ними.
    /// </summary>
    [CreateAssetMenu(fileName = "SurfboardConfig", menuName = "SurfRush/Surfboard Config", order = 1)]
    public class SurfboardConfig : ScriptableObject
    {
        [Header("Плавучесть")]
        [Tooltip("Сила выталкивания на единицу глубины погружения (Н/м). Чем больше — тем «жёстче» доска лежит на воде.")]
        public float buoyancyStrength = 35f;

        [Tooltip("Максимальная глубина (м), при которой плавучесть продолжает расти линейно. Глубже — насыщается.")]
        public float buoyancySaturationDepth = 0.6f;

        [Tooltip("Демпфирование вертикальной скорости в каждой точке плавучести. Гасит подпрыгивание.")]
        public float verticalDamping = 4f;

        [Header("Сила склона")]
        [Tooltip("Множитель к силе скатывания вдоль склона волны. 1 = чисто проекция гравитации; >1 — серфер «магнитится» к спуску.")]
        public float slopeAccelMultiplier = 1.6f;

        [Tooltip("Дополнительное сопротивление вдоль forward, в м/с²/(м/с). Аналог тяги воды.")]
        public float forwardDrag = 0.4f;

        [Tooltip("Сопротивление поперёк (sideways), в м/с²/(м/с). Чем больше — тем меньше доску «уносит» вбок.")]
        public float lateralDrag = 4.0f;

        [Header("Стабилизация ориентации")]
        [Tooltip("Скорость, с которой доска поворачивается, чтобы её Up совпал с нормалью воды (рад/с).")]
        public float alignTorqueStrength = 8f;

        [Tooltip("Дамп угловой скорости, чтобы доска не качалась. (1/с)")]
        public float angularDamping = 3f;
    }
}
