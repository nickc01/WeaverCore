using UnityEngine;

namespace WeaverCore.Inventory
{

    public abstract class ArrowElement : InventoryElement
    {
        public enum ArrowState
        {
            None,
            Left,
            Right
        }

        public abstract ArrowState ArrowType { get; }

        protected virtual void Awake()
        {
            if (Initialization.Environment == Enums.RunningState.Game)
            {
                if (TryGetComponent<SpriteRenderer>(out var mainRenderer))
                {
                    mainRenderer.enabled = false;
                }
            }
        }
    }
}