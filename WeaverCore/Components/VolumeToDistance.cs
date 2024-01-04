using UnityEngine;

namespace WeaverCore.Components
{
    /// <summary>
    /// Changes the volume depending on how far away this object is away from a specified target
    /// </summary>
    public class VolumeToDistance : MonoBehaviour
	{
		public Vector2 VolumeMinMax = new Vector2(0f,1f);

		public Vector2 DistanceMinMax = new Vector2(5f,25f);

		[Tooltip("The target to measure. If null, it will default to the player")]
		public Transform Target;

		AudioSource source;

        private void Awake()
        {
			source = GetComponent<AudioSource>();

            if (Target == null)
			{
				Target = Player.Player1.transform;
			}
        }

        private void LateUpdate()
        {
			var distance = Vector3.Distance(transform.position, Target.position);

			var amount = Mathf.InverseLerp(DistanceMinMax.x, DistanceMinMax.y, distance);

			source.volume = Mathf.Lerp(VolumeMinMax.y,VolumeMinMax.x,amount);
        }
    }
}
