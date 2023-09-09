using System;
using UnityEngine;
using WeaverCore.Features;
using WeaverCore.Utilities;

namespace WeaverCore.Inventory
{
    public abstract class InventoryElement : MonoBehaviour
    {
        const float GIZMO_ARROW_ANGLE = 35f;

        [NonSerialized]
        InventoryPanel _mainPanel;

        public InventoryPanel MainPanel => _mainPanel ??= GetComponentInParent<InventoryPanel>();

        public enum MoveDirection
        {
            Up,
            Left,
            Down,
            Right
        }

        [HideInInspector]
        [SerializeField]
        Color gizmoColor = default;

        /// <summary>
        /// Can this inventory element be highlighted by the cursor?
        /// </summary>
        public virtual bool Highlightable => true;

        /// <summary>
        /// Can the player click on this inventory element?
        /// </summary>
        public virtual bool Selectable => false;


        /// <summary>
        /// Called when the player clicks on this element. <see cref="Selectable"/> must be set to true for this to trigger
        /// </summary>
        public virtual void OnClick()
        {

        }

        /// <summary>
        /// Called when the inventory element is highlighted
        /// </summary>
        public virtual void OnHighlight()
        {
            
        }

        /// <summary>
        /// Called when the inventory element is no longer highlighted
        /// </summary>
        public virtual void OnUnHighlight()
        {

        }

        /// <summary>
        /// Used to determine where the cursor should be located when highlighting this inventory element
        /// </summary>
        public virtual Vector3 CursorPos => transform.position;

        /// <summary>
        /// Used to determine the size of the cursor when highlighting this inventory element. By default, it uses the bounds of the Collider attached to this element
        /// </summary>
        public virtual Vector2 CursorSize => transform.GetComponent<Collider2D>().bounds.size;

        /// <summary>
        /// Used to determine the offset of the cursor when highlighting this inventory element. By default, it uses the offset of the Collider attached to this element
        /// </summary>
        public virtual Vector2 CursorOffset => transform.GetComponent<Collider2D>().offset;

        /// <summary>
        /// Called when the player wants to move to a nearby element. For example, if the player wants to move up in the UI, this function specifies which element the player will go to next
        /// </summary>
        /// <param name="move">The movement direction the player is going in</param>
        /// <returns>The next element to move to, or null if there is no element to move to</returns>
        public abstract InventoryElement NavigateTo(MoveDirection move);


        protected virtual void OnDrawGizmos()
        {
            for (int i = 0; i < 4; i++)
            {
                var dest = NavigateTo((MoveDirection)i);
                if (dest != null)
                {
                    GizmoDrawToObject(dest.transform.position);
                }
            }
        }

        void GizmoDrawToObject(Vector3 destination)
        {
            if (gizmoColor == default)
            {
                gizmoColor = Color.HSVToRGB(UnityEngine.Random.Range(0f,1f), 1f, 1f);
            }
            var vectToDest = (Vector2)(destination - transform.position).normalized;

            var angle = MathUtilities.CartesianToPolar(vectToDest).x;

            var upAngle = angle + 90f;

            var upVector = MathUtilities.PolarToCartesian(upAngle, 1f);

            var startOffset = (vectToDest * 0.25f) + (upVector * 0.25f);

            //Gizmos.color = Color.red;
            Gizmos.color = gizmoColor;

            var trueDest = destination - (Vector3)(vectToDest * 0.25f);

            Gizmos.DrawLine(transform.position + (Vector3)startOffset, trueDest);

            var firstAngle = upAngle + 90f - GIZMO_ARROW_ANGLE;
            var secondAngle = upAngle + 90f + GIZMO_ARROW_ANGLE;

            Gizmos.DrawLine(trueDest, trueDest + (Vector3)MathUtilities.PolarToCartesian(firstAngle, 0.25f));
            Gizmos.DrawLine(trueDest, trueDest + (Vector3)MathUtilities.PolarToCartesian(secondAngle, 0.25f));
        }
    }
}