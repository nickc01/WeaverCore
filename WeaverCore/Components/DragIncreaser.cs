using System.Collections;
using UnityEngine;

namespace WeaverCore.Components
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class DragIncreaser : MonoBehaviour
    {
        [SerializeField]
        float increaseDragDelay = 1f;

        [SerializeField]
        float increaseDragRate = 10f;

        [SerializeField]
        float maxDrag = 100f;

        [SerializeField]
        bool resetDragOnDisable = true;

        [SerializeField]
        float resetValue = 0f;

        Rigidbody2D _rb;

        public Rigidbody2D RB
        {
            get
            {
                if (_rb == null)
                {
                    _rb = GetComponent<Rigidbody2D>();
                }
                return _rb;
            }
        }

        private void OnEnable() 
        {
            StartCoroutine(DragRoutine());
        }

        private void OnDisable() 
        {
            StopAllCoroutines();
            if (resetDragOnDisable)
            {
                RB.drag = resetValue;
            }
        }

        IEnumerator DragRoutine()
        {
            yield return new WaitForSeconds(increaseDragDelay);
            while (RB.drag < maxDrag)
            {
                RB.drag += increaseDragRate * Time.deltaTime;
                yield return null;
            }
            RB.drag = maxDrag;
        }
    }
}
