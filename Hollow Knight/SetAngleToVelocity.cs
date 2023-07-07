using System;
using UnityEngine;

public class SetAngleToVelocity : MonoBehaviour
{
    public Rigidbody2D rb;

    public float angleOffset;

    private void Update()
    {
        Vector2 velocity = rb.velocity;
        float z = Mathf.Atan2(velocity.y, velocity.x) * (180f / (float)Math.PI) + angleOffset;
        base.transform.localEulerAngles = new Vector3(0f, 0f, z);
    }
}
