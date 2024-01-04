using UnityEngine;

namespace WeaverCore.Inventory
{
    /// <summary>
    /// Represents an the left and right arrows in an inventory pane. These are used to move to different panes
    /// </summary>
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