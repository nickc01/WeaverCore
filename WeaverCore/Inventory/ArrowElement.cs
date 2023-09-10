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
    }
}