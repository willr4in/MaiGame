using UnityEngine;

namespace SurfRush.Spawn
{
    /// <summary>
    /// Набор препятствий под определённую тему (Day, Evening, и т.д.).
    /// ScriptableObject, чтобы можно было держать несколько наборов и
    /// переключать их одной ссылкой при смене режима TimeOfDay.
    ///
    /// Каждая запись имеет вес — относительная вероятность выпадения этого
    /// префаба. Веса не обязаны нормализоваться: спавнер сам поделит на сумму.
    /// </summary>
    [CreateAssetMenu(fileName = "ObstacleTheme", menuName = "SurfRush/Obstacle Theme")]
    public class ObstacleTheme : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public Obstacle prefab;
            [Min(0f)] public float weight;
        }

        [SerializeField] private Entry[] entries;

        public int Count => entries == null ? 0 : entries.Length;

        /// <summary>Сумма всех весов; 0 если набор пуст.</summary>
        public float TotalWeight
        {
            get
            {
                if (entries == null) return 0f;
                float sum = 0f;
                for (int i = 0; i < entries.Length; i++) sum += Mathf.Max(0f, entries[i].weight);
                return sum;
            }
        }

        /// <summary>Случайный префаб с учётом весов. Возвращает null если пуст.</summary>
        public Obstacle PickRandom()
        {
            if (entries == null || entries.Length == 0) return null;
            float total = TotalWeight;
            if (total <= 0f) return entries[0].prefab; // fallback: все веса нулевые

            float r = Random.value * total;
            float acc = 0f;
            for (int i = 0; i < entries.Length; i++)
            {
                acc += Mathf.Max(0f, entries[i].weight);
                if (r <= acc) return entries[i].prefab;
            }
            return entries[entries.Length - 1].prefab;
        }
    }
}
