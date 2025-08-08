using System;
using UnityEngine;

public class FaceAngleSimple : MonoBehaviour
{
	public float angleOffset;

	public bool everyFrame;

	private Rigidbody2D rb2d;

	private void OnEnable()
	{
		rb2d = GetComponent<Rigidbody2D>();
		DoAngle();
	}

	private void Update()
	{
		if (everyFrame)
		{
			DoAngle();
		}
	}

	private void DoAngle()
	{
		Vector2 velocity = rb2d.velocity;
		float z = Mathf.Atan2(velocity.y, velocity.x) * (180f / (float)Math.PI) + angleOffset;
		base.transform.localEulerAngles = new Vector3(0f, 0f, z);
	}
}
