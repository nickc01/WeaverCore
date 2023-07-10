using UnityEngine;

public class SpinSelf : MonoBehaviour
{
    public float spinFactor = -7.5f;

    private int stepCounter;

    private bool spun;

    private void Start()
    {
        base.transform.localEulerAngles = new Vector3(base.transform.localEulerAngles.x, base.transform.localEulerAngles.y, Random.Range(0, 360));
    }

    private void FixedUpdate()
    {
        if (!spun)
        {
            if (stepCounter >= 1)
            {
                Rigidbody2D component = GetComponent<Rigidbody2D>();
                float torque = component.velocity.x * spinFactor;
                component.AddTorque(torque);
                spun = true;
            }
            stepCounter++;
        }
    }
}
