using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{

    public class ConstantlyRotate : MonoBehaviour
    {
        [SerializeField]
        Vector2 rotationRange = new Vector2(-20f, 20f);

        [SerializeField]
        float minRotationSpeed = 0f;

        float rotationAmount;
        
        public float RotationSpeed
        {
            get => rotationAmount;
            set => rotationAmount = value;
        }

        private void Awake()
        {
            rotationAmount = rotationRange.RandomInRange();

            if (Mathf.Abs(rotationAmount) < minRotationSpeed)
            {
                if (rotationAmount >= 0)
                {
                    rotationAmount = minRotationSpeed;
                }
                else
                {
                    rotationAmount = -minRotationSpeed;
                }

            }
        }

        private void Update()
        {
            transform.rotation *= Quaternion.Euler(0f, 0f, rotationAmount * Time.deltaTime);
        }
    }
}
