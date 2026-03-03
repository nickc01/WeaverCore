using UnityEngine;

namespace WeaverCore.Utilities
{
    public static class RigidbodyUtilities
    {
        public static void SetXVelocity(this Rigidbody2D rb, float x)
        {
            rb.velocity = rb.velocity.With(x: x);
        }

        public static void SetYVelocity(this Rigidbody2D rb, float x)
        {
            rb.velocity = rb.velocity.With(x: x);
        }
    }
}
