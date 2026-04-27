using UnityEngine;

namespace SurfRush.Ocean
{
    /// <summary>
    /// Бесконечный океан из переиспользуемых чанков.
    ///
    /// Создаёт сетку (2*radius+1)² чанков вокруг target. Когда target уходит
    /// за границу центрального чанка, мы не двигаем target и не перегенерируем
    /// меши — просто сдвигаем XZ-позицию того кольца чанков, что осталось
    /// «позади», на противоположную сторону. Меш чанка статичен, а вершины
    /// каждый кадр сэмплят WaveField в мировых координатах, поэтому шов между
    /// соседями автоматически идеален.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public class OceanChunkManager : MonoBehaviour
    {
        [Header("Целеуказание")]
        [Tooltip("За кем следим. Если null — берётся Camera.main.")]
        [SerializeField] private Transform target;

        [Header("Сетка")]
        [Tooltip("Радиус сетки в чанках от центра. 1 = 3x3, 2 = 5x5.")]
        [SerializeField, Range(1, 4)] private int radius = 1;

        [Header("Префаб чанка")]
        [Tooltip("Должен иметь Mesh Filter, Mesh Renderer (с материалом) и OceanMeshChunk.")]
        [SerializeField] private OceanMeshChunk chunkPrefab;

        [Tooltip("Фактический размер одного чанка в метрах. Должен совпадать с настройкой sizeMeters в OceanMeshChunk префаба.")]
        [SerializeField, Min(1f)] private float chunkSize = 100f;

        private OceanMeshChunk[,] _chunks;          // [side, side]
        private Vector2Int _centerCoord;            // координата центрального чанка в «целочисленной» сетке
        private int _side;

        private Transform Target
        {
            get
            {
                if (target != null) return target;
                if (Camera.main != null) return Camera.main.transform;
                return null;
            }
        }

        private void Start()
        {
            if (chunkPrefab == null)
            {
                Debug.LogError("[OceanChunkManager] chunkPrefab не назначен.");
                enabled = false;
                return;
            }
            BuildGrid();
        }

        private void BuildGrid()
        {
            _side = radius * 2 + 1;
            _chunks = new OceanMeshChunk[_side, _side];

            Transform t = Target;
            Vector3 origin = t != null ? t.position : Vector3.zero;
            _centerCoord = WorldToCoord(origin);

            for (int z = 0; z < _side; z++)
            {
                for (int x = 0; x < _side; x++)
                {
                    Vector2Int coord = new Vector2Int(_centerCoord.x + (x - radius),
                                                      _centerCoord.y + (z - radius));
                    OceanMeshChunk c = Instantiate(chunkPrefab, transform);
                    c.name = $"OceanChunk_{coord.x}_{coord.y}";
                    c.transform.position = CoordToWorld(coord);
                    _chunks[x, z] = c;
                }
            }
        }

        private void LateUpdate()
        {
            Transform t = Target;
            if (t == null || _chunks == null) return;

            Vector2Int newCenter = WorldToCoord(t.position);
            if (newCenter == _centerCoord) return;

            // Простой алгоритм: сдвинуть массив чанков и переставить выпавшие
            // на противоположную сторону. Работает для любого смещения,
            // даже большого (например, телепорт).
            ShiftGrid(newCenter);
            _centerCoord = newCenter;
        }

        private void ShiftGrid(Vector2Int newCenter)
        {
            // Перебираем «логические» позиции массива и определяем, какому
            // мировому coord они должны соответствовать после сдвига.
            // Если найденный coord уже представлен каким-то чанком — переиспользуем.
            // Если нет — берём «лишний» и перемещаем туда.
            OceanMeshChunk[,] result = new OceanMeshChunk[_side, _side];
            bool[,] used = new bool[_side, _side];

            // Шаг 1: чанки, чьи мировые координаты остались внутри новой сетки —
            // на свои новые места.
            for (int z = 0; z < _side; z++)
            {
                for (int x = 0; x < _side; x++)
                {
                    OceanMeshChunk c = _chunks[x, z];
                    Vector2Int coord = WorldToCoord(c.transform.position);
                    int rx = coord.x - (newCenter.x - radius);
                    int rz = coord.y - (newCenter.y - radius);
                    if (rx >= 0 && rx < _side && rz >= 0 && rz < _side && result[rx, rz] == null)
                    {
                        result[rx, rz] = c;
                        used[x, z] = true;
                    }
                }
            }

            // Шаг 2: оставшиеся (выпавшие) чанки переезжают в пустые ячейки.
            for (int z = 0; z < _side; z++)
            {
                for (int x = 0; x < _side; x++)
                {
                    if (result[x, z] != null) continue;
                    OceanMeshChunk free = TakeUnused(used);
                    if (free == null)
                    {
                        Debug.LogError("[OceanChunkManager] Нет свободного чанка для переезда — алгоритм сломан.");
                        return;
                    }
                    Vector2Int coord = new Vector2Int(newCenter.x + (x - radius),
                                                      newCenter.y + (z - radius));
                    free.transform.position = CoordToWorld(coord);
                    free.name = $"OceanChunk_{coord.x}_{coord.y}";
                    result[x, z] = free;
                }
            }

            _chunks = result;
        }

        private OceanMeshChunk TakeUnused(bool[,] used)
        {
            for (int z = 0; z < _side; z++)
            {
                for (int x = 0; x < _side; x++)
                {
                    if (!used[x, z])
                    {
                        used[x, z] = true;
                        return _chunks[x, z];
                    }
                }
            }
            return null;
        }

        private Vector2Int WorldToCoord(Vector3 world)
        {
            return new Vector2Int(
                Mathf.FloorToInt((world.x + chunkSize * 0.5f) / chunkSize),
                Mathf.FloorToInt((world.z + chunkSize * 0.5f) / chunkSize));
        }

        private Vector3 CoordToWorld(Vector2Int coord)
        {
            return new Vector3(coord.x * chunkSize, 0f, coord.y * chunkSize);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.5f);
            int side = radius * 2 + 1;
            Transform t = Target;
            Vector2Int center = WorldToCoord(t != null ? t.position : Vector3.zero);
            for (int z = 0; z < side; z++)
            {
                for (int x = 0; x < side; x++)
                {
                    Vector2Int coord = new Vector2Int(center.x + (x - radius),
                                                      center.y + (z - radius));
                    Vector3 c = CoordToWorld(coord);
                    Gizmos.DrawWireCube(c, new Vector3(chunkSize, 0.1f, chunkSize));
                }
            }
        }
    }
}
