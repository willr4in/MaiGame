using System.Collections.Generic;
using UnityEngine;

namespace SurfRush.Spawn
{
    /// <summary>
    /// Простой пул препятствий: для каждого префаба свой стек неактивных
    /// инстансов. Get/Return вместо Instantiate/Destroy — нет аллокаций
    /// в горячем цикле спавнера.
    ///
    /// Не дженерик, не ScriptableObject — намеренно простой MonoBehaviour,
    /// потому что один спавнер = один пул, и он живёт ровно столько же.
    /// </summary>
    public class ObstaclePool : MonoBehaviour
    {
        private readonly Dictionary<Obstacle, Stack<Obstacle>> _stacks = new();

        public Obstacle Get(Obstacle prefab, Vector3 position, Quaternion rotation)
        {
            if (!_stacks.TryGetValue(prefab, out var stack))
            {
                stack = new Stack<Obstacle>();
                _stacks[prefab] = stack;
            }

            Obstacle inst;
            if (stack.Count > 0)
            {
                inst = stack.Pop();
                inst.transform.SetPositionAndRotation(position, rotation);
                inst.gameObject.SetActive(true);
            }
            else
            {
                inst = Instantiate(prefab, position, rotation, transform);
                inst.SourcePrefab = prefab;
                inst.OwnerPool = this;
            }
            return inst;
        }

        public void Return(Obstacle inst)
        {
            if (inst == null || inst.SourcePrefab == null)
            {
                if (inst != null) Destroy(inst.gameObject);
                return;
            }
            inst.gameObject.SetActive(false);
            if (!_stacks.TryGetValue(inst.SourcePrefab, out var stack))
            {
                stack = new Stack<Obstacle>();
                _stacks[inst.SourcePrefab] = stack;
            }
            stack.Push(inst);
        }
    }
}
