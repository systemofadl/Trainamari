using UnityEngine;

namespace Trainamari.Train
{
    /// <summary>
    /// Dead-simple follow camera. Drop on Main Camera, set Target to the cube.
    /// </summary>
    public class ChaseCam : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -10f);
        [SerializeField] private float smoothing = 5f;

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 desired = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desired, smoothing * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * 0.5f);
        }
    }
}
