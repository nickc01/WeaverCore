using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WeaverCore.Utilities
{
	public static class TransformUtilities
	{
		public static float GetXPosition(this Transform transform)
		{
			return transform.localPosition.x;
		}

		public static float SetXPosition(this Transform transform, float newX)
		{
			transform.localPosition = transform.localPosition.With(x: newX);
			return newX;
		}

		public static float GetYPosition(this Transform transform)
		{
			return transform.localPosition.y;
		}

		public static float SetYPosition(this Transform transform, float newY)
		{
			transform.localPosition = transform.localPosition.With(y: newY);
			return newY;
		}

		public static float GetZPosition(this Transform transform)
		{
			return transform.localPosition.z;
		}

		public static float SetZPosition(this Transform transform, float newZ)
		{
			transform.localPosition = transform.localPosition.With(z: newZ);
			return newZ;
		}

		public static Vector3 SetPosition(this Transform transform, float x = float.NaN, float y = float.NaN, float z = float.NaN)
		{
			Vector3 newPos = transform.localPosition;

			if (!float.IsNaN(x))
			{
				newPos.x = x;
			}
			if (!float.IsNaN(y))
			{
				newPos.y = y;
			}
			if (!float.IsNaN(z))
			{
				newPos.z = z;
			}
			transform.localPosition = newPos;
			return newPos;
		}

		public static float GetXLocalPosition(this Transform transform)
		{
			return transform.localPosition.x;
		}

		public static float SetXLocalPosition(this Transform transform, float newX)
		{
			transform.localPosition = transform.localPosition.With(x: newX);
			return newX;
		}

		public static float GetYLocalPosition(this Transform transform)
		{
			return transform.localPosition.y;
		}

		public static float SetYLocalPosition(this Transform transform, float newY)
		{
			transform.localPosition = transform.localPosition.With(y: newY);
			return newY;
		}

		public static float GetZLocalPosition(this Transform transform)
		{
			return transform.localPosition.z;
		}

		public static float SetZLocalPosition(this Transform transform, float newZ)
		{
			transform.localPosition = transform.localPosition.With(z: newZ);
			return newZ;
		}

		public static Vector3 SetLocalPosition(this Transform transform, float x = float.NaN, float y = float.NaN, float z = float.NaN)
		{
			Vector3 newPos = transform.localPosition;

			if (!float.IsNaN(x))
			{
				newPos.x = x;
			}
			if (!float.IsNaN(y))
			{
				newPos.y = y;
			}
			if (!float.IsNaN(z))
			{
				newPos.z = z;
			}
			transform.localPosition = newPos;
			return newPos;
		}


		public static float GetXLocalRotation(this Transform transform)
		{
			return transform.localEulerAngles.x;
		}

		public static float SetXLocalRotation(this Transform transform, float newX)
		{
			transform.localEulerAngles = transform.localEulerAngles.With(x: newX);
			return newX;
		}

		public static float GetYLocalRotation(this Transform transform)
		{
			return transform.localEulerAngles.y;
		}

		public static float SetYLocalRotation(this Transform transform, float newY)
		{
			transform.localEulerAngles = transform.localEulerAngles.With(y: newY);
			return newY;
		}

		public static float GetZLocalRotation(this Transform transform)
		{
			return transform.localEulerAngles.z;
		}

		public static float SetZLocalRotation(this Transform transform, float newZ)
		{
			transform.localEulerAngles = transform.localEulerAngles.With(z: newZ);
			return newZ;
		}

		public static Vector3 SetLocalRotation(this Transform transform, float x = float.NaN, float y = float.NaN, float z = float.NaN)
		{
			Vector3 newRotation = transform.localEulerAngles;

			if (!float.IsNaN(x))
			{
				newRotation.x = x;
			}
			if (!float.IsNaN(y))
			{
				newRotation.y = y;
			}
			if (!float.IsNaN(z))
			{
				newRotation.z = z;
			}
			transform.localEulerAngles = newRotation;
			return newRotation;
		}

		public static float GetXRotation(this Transform transform)
		{
			return transform.eulerAngles.x;
		}

		public static float SetXRotation(this Transform transform, float newX)
		{
			transform.eulerAngles = transform.eulerAngles.With(x: newX);
			return newX;
		}

		public static float GetYRotation(this Transform transform)
		{
			return transform.eulerAngles.y;
		}

		public static float SetYRotation(this Transform transform, float newY)
		{
			transform.eulerAngles = transform.eulerAngles.With(y: newY);
			return newY;
		}

		public static float GetZRotation(this Transform transform)
		{
			return transform.eulerAngles.z;
		}

		public static float SetZRotation(this Transform transform, float newZ)
		{
			transform.eulerAngles = transform.eulerAngles.With(z: newZ);
			return newZ;
		}

		public static Vector3 SetRotation(this Transform transform, float x = float.NaN, float y = float.NaN, float z = float.NaN)
		{
			Vector3 newRotation = transform.eulerAngles;

			if (!float.IsNaN(x))
			{
				newRotation.x = x;
			}
			if (!float.IsNaN(y))
			{
				newRotation.y = y;
			}
			if (!float.IsNaN(z))
			{
				newRotation.z = z;
			}
			transform.eulerAngles = newRotation;
			return newRotation;
		}

		public static IEnumerator JitterObject(Transform transform, Vector3 amount)
		{
			Vector3 startPosition = transform.position;

			while (true)
			{
				yield return null;
				transform.position = new Vector3(startPosition.x + Random.Range(-amount.x, amount.x), startPosition.y + Random.Range(-amount.y, amount.y), startPosition.z + Random.Range(-amount.z, amount.z));
			}
		}

		public static IEnumerator JitterObject(GameObject gameObject, Vector3 amount)
		{
			return JitterObject(gameObject.transform, amount);
		}

		public static GameObject[] SpawnRandomObjects(GameObject obj, Vector3 spawnPoint, int spawnMin, int spawnMax, float minSpeed, float maxSpeed, float angleMin, float angleMax, Vector2 originOffset = default(Vector2))
		{
			int spawnNum = Random.Range(spawnMin, spawnMax + 1);
			float speedNum = Random.Range(minSpeed, maxSpeed);
			float angleNum = Random.Range(angleMin, angleMax);

			GameObject[] instances = new GameObject[spawnNum];

			for (int i = 0; i < spawnNum; i++)
			{
				var instance = GameObject.Instantiate(obj, new Vector3(spawnPoint.x + Random.Range(-originOffset.x, originOffset.x), spawnPoint.y + Random.Range(-originOffset.y, originOffset.y), spawnPoint.z), Quaternion.identity);
				instances[i] = instance;
				var rigid = instance.GetComponent<Rigidbody2D>();
				if (rigid != null)
				{
					rigid.velocity = new Vector2(Mathf.Cos(angleNum) * Mathf.Deg2Rad, Mathf.Sin(angleNum) * Mathf.Deg2Rad);
				}
			}

			return instances;
		}

		public static int GetHashOfTransform(Transform transform, bool includeChildren = true)
		{
			int hash = transform.gameObject.name.GetHashCode();

			if (includeChildren)
			{
				for (int i = 0; i < transform.childCount; i++)
				{
					//hash = GetHashOfTransform(transform, true);
					MiscUtilities.AdditiveHash(ref hash, GetHashOfTransform(transform.GetChild(i), true));
				}
			}

			return hash;
		}
	}
}
