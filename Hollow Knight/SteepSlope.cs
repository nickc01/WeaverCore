using UnityEngine;

public class SteepSlope : MonoBehaviour
{
	private void OnCollisionStay2D(Collision2D collision)
	{
		GameObject gameObject = collision.gameObject;
		Rigidbody2D component = gameObject.GetComponent<Rigidbody2D>();
		component.velocity = new Vector2(component.velocity.x, -20f);
		if (gameObject.CompareTag("Player"))
		{
			HeroController.instance.ResetHardLandingTimer();
			HeroController.instance.CancelSuperDash();
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Player"))
		{
			if (slideLeft)
			{
				HeroController.instance.cState.slidingLeft = true;
			}
			if (slideRight)
			{
				HeroController.instance.cState.slidingRight = true;
			}
		}
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Player"))
		{
			if (slideLeft)
			{
				HeroController.instance.cState.slidingLeft = false;
			}
			if (slideRight)
			{
				HeroController.instance.cState.slidingRight = false;
			}
		}
	}

	public bool slideLeft;
	public bool slideRight;
}
