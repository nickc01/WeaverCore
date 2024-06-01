using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{

    public class ConstantlyRotate : MonoBehaviour
    {
        [SerializeField]
        Vector2 rotationRange = new Vector2(-20f, 20f);

        float rotationAmount;

        private void Awake()
        {
            rotationAmount = rotationRange.RandomInRange();
        }

        private void Update()
        {
            transform.rotation *= Quaternion.Euler(0f, 0f, rotationAmount * Time.deltaTime);
        }
    }
}
