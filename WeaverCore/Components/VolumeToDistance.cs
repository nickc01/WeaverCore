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
            source.volume = CalculateVolumeAtPoint(Target.position);
			/*var distance = Vector3.Distance(transform.position, Target.position);

			var amount = Mathf.InverseLerp(DistanceMinMax.x, DistanceMinMax.y, distance);
			source.volume = Mathf.Lerp(VolumeMinMax.y,VolumeMinMax.x,amount);*/
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, DistanceMinMax.y);
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawSphere(transform.position, DistanceMinMax.x);
        }

        public float CalculateVolumeMultiplierAtPoint(Vector3 position) 
        {
            var distance = Vector3.Distance(transform.position, position);

			var amount = Mathf.InverseLerp(DistanceMinMax.x, DistanceMinMax.y, distance);

            return 1 - amount;
			//return Mathf.Lerp(VolumeMinMax.y,VolumeMinMax.x,amount);
        }

        public float CalculateVolumeAtPoint(Vector3 position) 
        {
            var distance = Vector3.Distance(transform.position, position);

			var amount = Mathf.InverseLerp(DistanceMinMax.x, DistanceMinMax.y, distance);
			return Mathf.Lerp(VolumeMinMax.y,VolumeMinMax.x,amount);
        }
    }
}
