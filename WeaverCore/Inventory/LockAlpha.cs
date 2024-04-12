using System;
using UnityEngine;

namespace WeaverCore.Inventory
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class LockAlpha : MonoBehaviour
    {
        [field: SerializeField]
        public float LockedAlphaValue { get; private set; } = 0f;

        [NonSerialized]
        SpriteRenderer _renderer;

        void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        void LateUpdate()
        {
            var temp = _renderer.color;
            temp.a = LockedAlphaValue;
            _renderer.color = temp;
        }

        void Reset()
        {
            if (gameObject.TryGetComponent<SpriteRenderer>(out var r))
            {
                LockedAlphaValue = r.color.a;
            }
        }
    }
}