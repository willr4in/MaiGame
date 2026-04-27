using UnityEngine;

namespace SurfRush.Player
{
    /// <summary>
    /// Временная камера для Фазы 3: следит за target с заданным мировым
    /// смещением, не наследуя его наклон/масштаб. В Фазе 4 заменим на Cinemachine.
    /// </summary>
    public class SimpleFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 5f, -8f);
        [SerializeField] private float positionSmooth = 6f;
        [SerializeField] private float lookSmooth = 5f;
        [SerializeField] private float lookAheadY = 1f;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPos = target.position + worldOffset;
            transform.position = Vector3.Lerp(transform.position, desiredPos,
                                              1f - Mathf.Exp(-positionSmooth * Time.deltaTime));

            Vector3 lookTarget = target.position + Vector3.up * lookAheadY;
            Quaternion desiredRot = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot,
                                                  1f - Mathf.Exp(-lookSmooth * Time.deltaTime));
        }
    }
}
