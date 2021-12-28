using UnityEngine;

/// <summary>
/// Used to apply a tilt to a bench so when the player sits on it, the player will be sitting on the bench in a tilted position
/// </summary>
public class RestBenchTilt : MonoBehaviour
{
	[Tooltip("The amount of tilt to be applied to the player when they sit on the bench")]
	public float tilt;

	/// <summary>
	/// Gets the amount of tilt to be applied to the player when they sit on the bench
	/// </summary>
	public float GetTilt()
	{
		return tilt;
	}
}
