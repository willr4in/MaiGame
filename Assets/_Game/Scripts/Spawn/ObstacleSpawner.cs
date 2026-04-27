using System.Collections.Generic;
using UnityEngine;

namespace SurfRush.Spawn
{
    /// <summary>
    /// Процедурный спавнер препятствий впереди игрока в заданном «коридоре».
    ///
    /// Концепция:
    /// — Берём ось «вдоль трека» — мировой +Z (наша конвенция forward).
    ///   Препятствия спавнятся на Z игрока + spawnAheadDistance.
    /// — Активные препятствия хранятся в списке. Каждые spawnInterval секунд
    ///   спавним пачку случайных X в коридоре [-corridorHalfWidth, +corridorHalfWidth]
    ///   с проверкой на минимальное расстояние между соседями (Poisson-light).
    /// — Те, что отъехали за игрока на despawnBehindDistance, возвращаются в пул.
    ///
    /// В Фазе 6 DifficultyController подкрутит spawnInterval и amountPerWave
    /// от времени игры.
    /// </summary>
    public class ObstacleSpawner : MonoBehaviour
    {
        [Header("Цель и геометрия трека")]
        [SerializeField] private Transform target;
        [Tooltip("Полуширина коридора, где могут появляться препятствия (по X).")]
        [SerializeField] private float corridorHalfWidth = 12f;
        [Tooltip("На каком расстоянии впереди игрока (по Z) появляется новая волна препятствий.")]
        [SerializeField] private float spawnAheadDistance = 60f;
        [Tooltip("На сколько метров позади игрока препятствие возвращается в пул.")]
        [SerializeField] private float despawnBehindDistance = 25f;

        [Header("Темп спавна")]
        [Tooltip("Интервал в секундах между волнами спавна.")]
        [SerializeField, Min(0.05f)] private float spawnInterval = 1.5f;
        [Tooltip("Сколько препятствий пытаемся заспавнить в каждой волне.")]
        [SerializeField, Min(1)] private int amountPerWave = 2;
        [Tooltip("Минимальное расстояние между двумя препятствиями (м). Меньше — отбрасываем кандидата.")]
        [SerializeField, Min(0.1f)] private float minSeparation = 4f;
        [Tooltip("Сколько раз пытаемся подобрать валидный X на одного кандидата (Poisson rejection).")]
        [SerializeField, Min(1)] private int rejectionAttempts = 6;

        [Header("Префабы")]
        [Tooltip("Тематический набор. Если задан — используется он; иначе fallback на массив prefabs.")]
        [SerializeField] private ObstacleTheme theme;
        [SerializeField] private Obstacle[] prefabs;
        [SerializeField] private ObstaclePool pool;

        public ObstacleTheme Theme { get => theme; set => theme = value; }

        [Header("Прелоад")]
        [Tooltip("При старте сразу заспавнить препятствия впереди до этого расстояния.")]
        [SerializeField] private float initialSpawnDistance = 80f;

        private readonly List<Obstacle> _active = new();
        private float _nextSpawnTime;

        // Публичный API для DifficultyController: можно крутить параметры в рантайме.
        public float SpawnInterval { get => spawnInterval; set => spawnInterval = Mathf.Max(0.05f, value); }
        public int AmountPerWave { get => amountPerWave; set => amountPerWave = Mathf.Max(1, value); }
        public float CorridorHalfWidth { get => corridorHalfWidth; set => corridorHalfWidth = Mathf.Max(1f, value); }

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
            if (pool == null) pool = GetComponent<ObstaclePool>();
            if (pool == null)
            {
                Debug.LogError("[ObstacleSpawner] ObstaclePool не найден.");
                enabled = false;
                return;
            }
            bool hasTheme = theme != null && theme.Count > 0;
            bool hasPrefabs = prefabs != null && prefabs.Length > 0;
            if (!hasTheme && !hasPrefabs)
            {
                Debug.LogError("[ObstacleSpawner] не задана ни тема, ни массив prefabs.");
                enabled = false;
                return;
            }

            // Прелоад: рассыпаем препятствия от позиции цели до initialSpawnDistance впереди.
            Transform t = Target;
            if (t != null)
            {
                float startZ = t.position.z + 8f; // не у самой доски
                float endZ = t.position.z + initialSpawnDistance;
                int waves = Mathf.Max(1, Mathf.CeilToInt((endZ - startZ) / 8f));
                float dz = (endZ - startZ) / waves;
                for (int i = 0; i < waves; i++)
                {
                    float z = startZ + i * dz;
                    SpawnWaveAtZ(z);
                }
            }

            _nextSpawnTime = Time.time + spawnInterval;
        }

        private void Update()
        {
            Transform t = Target;
            if (t == null) return;

            if (Time.time >= _nextSpawnTime)
            {
                float spawnZ = t.position.z + spawnAheadDistance;
                SpawnWaveAtZ(spawnZ);
                _nextSpawnTime = Time.time + spawnInterval;
            }

            // Despawn — за игроком (по Z).
            float despawnZ = t.position.z - despawnBehindDistance;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Obstacle o = _active[i];
                if (o == null) { _active.RemoveAt(i); continue; }
                if (o.transform.position.z < despawnZ)
                {
                    pool.Return(o);
                    _active.RemoveAt(i);
                }
            }
        }

        private void SpawnWaveAtZ(float baseZ)
        {
            for (int i = 0; i < amountPerWave; i++)
            {
                if (TryPickFreeX(baseZ, out float x))
                {
                    Obstacle prefab = PickPrefab();
                    if (prefab == null) continue;
                    Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    Obstacle inst = pool.Get(prefab, new Vector3(x, 0f, baseZ), rot);
                    _active.Add(inst);
                }
            }
        }

        private Obstacle PickPrefab()
        {
            if (theme != null && theme.Count > 0) return theme.PickRandom();
            if (prefabs != null && prefabs.Length > 0)
                return prefabs[Random.Range(0, prefabs.Length)];
            return null;
        }

        private bool TryPickFreeX(float z, out float x)
        {
            for (int attempt = 0; attempt < rejectionAttempts; attempt++)
            {
                float candidateX = Random.Range(-corridorHalfWidth, corridorHalfWidth);
                if (IsFar(candidateX, z))
                {
                    x = candidateX;
                    return true;
                }
            }
            x = 0f;
            return false;
        }

        private bool IsFar(float x, float z)
        {
            float minSep2 = minSeparation * minSeparation;
            for (int i = 0; i < _active.Count; i++)
            {
                Vector3 p = _active[i].transform.position;
                float dx = p.x - x;
                float dz = p.z - z;
                if (dx * dx + dz * dz < minSep2) return false;
            }
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Transform t = Target;
            if (t == null) return;
            Vector3 c = t.position;

            Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.7f);
            Vector3 a = new Vector3(-corridorHalfWidth, 0, c.z + spawnAheadDistance);
            Vector3 b = new Vector3( corridorHalfWidth, 0, c.z + spawnAheadDistance);
            Vector3 d = new Vector3( corridorHalfWidth, 0, c.z - despawnBehindDistance);
            Vector3 e = new Vector3(-corridorHalfWidth, 0, c.z - despawnBehindDistance);
            Gizmos.DrawLine(a, b); Gizmos.DrawLine(b, d);
            Gizmos.DrawLine(d, e); Gizmos.DrawLine(e, a);
        }
    }
}
