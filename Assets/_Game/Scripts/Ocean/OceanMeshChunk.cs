using UnityEngine;
using UnityEngine.Rendering;

namespace SurfRush.Ocean
{
    /// <summary>
    /// Один чанк океана: квадратная плоскость sizeMeters на sizeMeters,
    /// с (segments+1)² вершинами. Каждый кадр в LateUpdate сэмплирует
    /// WaveField для каждой вершины в МИРОВЫХ координатах — это даёт бесшовные
    /// швы между соседними чанками без дополнительной логики.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class OceanMeshChunk : MonoBehaviour
    {
        [Header("Размеры чанка")]
        [SerializeField, Min(1f)] private float sizeMeters = 100f;
        [SerializeField, Range(2, 254)] private int segments = 80;

        [Header("Производительность")]
        [Tooltip("Если true, нормали считаются по векторному произведению соседей (точнее визуально), иначе берутся из WaveField (быстрее).")]
        [SerializeField] private bool useFiniteDifferenceNormals = false;

        private MeshFilter _filter;
        private Mesh _mesh;

        // Базовая XZ сетка в локальных координатах (без деформации). Сохраняем,
        // чтобы каждый кадр не пересчитывать сетку, а только сэмплить волну.
        private Vector3[] _baseLocal;
        private Vector3[] _displaced;
        private Vector3[] _normals;
        private Vector2[] _uvs;
        private int[] _triangles;
        private int _verticesPerSide;

        // Worldspace XZ для каждой базовой вершины (обновляется при движении объекта).
        // Позволяет не дёргать transform.TransformPoint в горячем цикле.
        private Vector2[] _baseWorldXZ;
        private Vector3 _lastTransformPos;
        private Quaternion _lastTransformRot;

        public float SizeMeters => sizeMeters;

        private void Awake()
        {
            _filter = GetComponent<MeshFilter>();
            BuildMesh();
        }

        private void OnValidate()
        {
            if (Application.isPlaying && _filter != null)
                BuildMesh();
        }

        private void BuildMesh()
        {
            _verticesPerSide = segments + 1;
            int vCount = _verticesPerSide * _verticesPerSide;
            _baseLocal = new Vector3[vCount];
            _displaced = new Vector3[vCount];
            _normals = new Vector3[vCount];
            _uvs = new Vector2[vCount];
            _baseWorldXZ = new Vector2[vCount];

            float step = sizeMeters / segments;
            float half = sizeMeters * 0.5f;

            for (int z = 0; z < _verticesPerSide; z++)
            {
                for (int x = 0; x < _verticesPerSide; x++)
                {
                    int i = z * _verticesPerSide + x;
                    _baseLocal[i] = new Vector3(-half + x * step, 0f, -half + z * step);
                    _displaced[i] = _baseLocal[i];
                    _normals[i] = Vector3.up;
                    _uvs[i] = new Vector2(x / (float)segments, z / (float)segments);
                }
            }

            _triangles = new int[segments * segments * 6];
            int t = 0;
            for (int z = 0; z < segments; z++)
            {
                for (int x = 0; x < segments; x++)
                {
                    int v00 = z * _verticesPerSide + x;
                    int v10 = v00 + 1;
                    int v01 = v00 + _verticesPerSide;
                    int v11 = v01 + 1;
                    _triangles[t++] = v00; _triangles[t++] = v01; _triangles[t++] = v11;
                    _triangles[t++] = v00; _triangles[t++] = v11; _triangles[t++] = v10;
                }
            }

            if (_mesh == null)
            {
                _mesh = new Mesh
                {
                    name = "OceanChunkMesh",
                    indexFormat = vCount > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16
                };
                _mesh.MarkDynamic();
                _filter.sharedMesh = _mesh;
            }
            else
            {
                _mesh.Clear();
                _mesh.indexFormat = vCount > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            }
            _mesh.vertices = _displaced;
            _mesh.triangles = _triangles;
            _mesh.uv = _uvs;
            _mesh.normals = _normals;
            // Достаточно щедрые границы — чтобы culling не выкидывал чанк когда он
            // приподнят волной.
            _mesh.bounds = new Bounds(Vector3.zero, new Vector3(sizeMeters, 20f, sizeMeters));

            CacheBaseWorldXZ();
        }

        private void CacheBaseWorldXZ()
        {
            _lastTransformPos = transform.position;
            _lastTransformRot = transform.rotation;
            // Чанк всегда horizontal (ротация не учитывается); считаем как plain offset.
            float ox = _lastTransformPos.x;
            float oz = _lastTransformPos.z;
            for (int i = 0; i < _baseLocal.Length; i++)
            {
                _baseWorldXZ[i] = new Vector2(_baseLocal[i].x + ox, _baseLocal[i].z + oz);
            }
        }

        private void LateUpdate()
        {
            if (_baseLocal == null) return;

            if (transform.position != _lastTransformPos || transform.rotation != _lastTransformRot)
                CacheBaseWorldXZ();

            UpdateMeshVertices();
        }

        private void UpdateMeshVertices()
        {
            float baseX = transform.position.x;
            float baseZ = transform.position.z;

            // Этап 1: смещение вершин в локальные координаты.
            for (int i = 0; i < _baseLocal.Length; i++)
            {
                Vector2 wxz = _baseWorldXZ[i];
                WaveField.Sample s = WaveField.SampleAt(new Vector3(wxz.x, 0f, wxz.y));
                _displaced[i] = new Vector3(s.position.x - baseX, s.position.y, s.position.z - baseZ);

                if (!useFiniteDifferenceNormals)
                    _normals[i] = s.normal;
            }

            // Этап 2 (опционально): нормали из конечных разностей.
            if (useFiniteDifferenceNormals)
                ComputeFiniteDifferenceNormals();

            _mesh.vertices = _displaced;
            _mesh.normals = _normals;
            _mesh.RecalculateBounds();
        }

        private void ComputeFiniteDifferenceNormals()
        {
            int n = _verticesPerSide;
            for (int z = 0; z < n; z++)
            {
                for (int x = 0; x < n; x++)
                {
                    int i = z * n + x;
                    int ix1 = (x < n - 1) ? i + 1 : i;
                    int ix0 = (x > 0) ? i - 1 : i;
                    int iz1 = (z < n - 1) ? i + n : i;
                    int iz0 = (z > 0) ? i - n : i;
                    Vector3 dx = _displaced[ix1] - _displaced[ix0];
                    Vector3 dz = _displaced[iz1] - _displaced[iz0];
                    _normals[i] = Vector3.Cross(dz, dx).normalized;
                }
            }
        }
    }
}
