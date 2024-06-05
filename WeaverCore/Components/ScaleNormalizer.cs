using UnityEngine;

namespace WeaverCore.Components
{
    public class ScaleNormalizer : MonoBehaviour 
    {
        void Awake() => NormalizeScale();
        void FixedUpdate() => NormalizeScale();
        void LateUpdate() => NormalizeScale();

        void NormalizeScale()
        {
            var oldParent = transform.parent;
            transform.SetParent(null);

            transform.localScale = Vector3.one;

            transform.SetParent(oldParent);
        }
    }

}