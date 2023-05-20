using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RescaleToParent : MonoBehaviour
{
    /*private void LateUpdate()
    {
        if (transform.localScale != Vector3.one)
        {
            transform.localRotation = Quaternion.identity;
            var scaleChange = transform.localScale;
            var posChange = new Vector3(transform.localPosition.x * scaleChange.x, transform.localPosition.y * scaleChange.y, transform.localPosition.z * scaleChange.z);

            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;
            transform.parent.localScale = new Vector3(transform.parent.localScale.x * scaleChange.x, transform.parent.localScale.y * scaleChange.y, transform.parent.localScale.z * scaleChange.z);
            transform.parent.localPosition += posChange;
        }
    }*/
}
