using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// Contains many utility functions related to transforms
	/// </summary>
	public static class TransformUtilities
	{
		/// <summary>
		/// Gets the x position of a transform
		/// </summary>
		public static float GetXPosition(this Transform transform)
		{
			return transform.localPosition.x;
		}

		/// <summary>
		/// Sets the x position of a transform
		/// </summary>
		public static float SetXPosition(this Transform transform, float newX)
		{
			transform.localPosition = transform.localPosition.With(x: newX);
			return newX;
		}

		/// <summary>
		/// Gets the y position of a transform
		/// </summary>
		public static float GetYPosition(this Transform transform)
		{
			return transform.localPosition.y;
		}

		/// <summary>
		/// Sets the y position of a transform
		/// </summary>
		public static float SetYPosition(this Transform transform, float newY)
		{
			transform.localPosition = transform.localPosition.With(y: newY);
			return newY;
		}

		/// <summary>
		/// Gets the z position of a transform
		/// </summary>
		public static float GetZPosition(this Transform transform)
		{
			return transform.localPosition.z;
		}

		/// <summary>
		/// Sets the z position of a transform
		/// </summary>
		public static float SetZPosition(this Transform transform, float newZ)
		{
			transform.localPosition = transform.localPosition.With(z: newZ);
			return newZ;
		}

		/// <summary>
		/// Sets the position of the transform
		/// </summary>
		/// <param name="transform">The transform to set</param>
		/// <param name="x">The new x-position to set. If it's set to NaN, it will leave it unchanged</param>
		/// <param name="y">The new y-position to set. If it's set to NaN, it will leave it unchanged</param>
		/// <param name="z">The new z-position to set. If it's set to NaN, it will leave it unchanged</param>
		/// <returns>Returns the new position that has just been set</returns>
		public static Vector3 SetPosition(this Transform transform, float x = float.NaN, float y = float.NaN, float z = float.NaN)
		{
			Vector3 newPos = transform.position;

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
			transform.position = newPos;
			return newPos;
		}

		public static Vector2 GetLocalScaleXY(this Transform transform)
        {
			return transform.localScale;
        }

		public static void SetLocalScaleXY(this Transform transform, Vector2 value)
        {
			transform.localScale = new Vector3(value.x,value.y,transform.localScale.z);
        }

		public static void SetLocalScaleXY(this Transform transform, float x, float y)
		{
			transform.localScale = new Vector3(x, y, transform.localScale.z);
		}

		/// <summary>
		/// Gets the local x position of a transform
		/// </summary>
		public static float GetXLocalPosition(this Transform transform)
		{
			return transform.localPosition.x;
		}

		/// <summary>
		/// Sets the local x position of a transform
		/// </summary>
		public static float SetXLocalPosition(this Transform transform, float newX)
		{
			transform.localPosition = transform.localPosition.With(x: newX);
			return newX;
		}

		/// <summary>
		/// Gets the local y position of a transform
		/// </summary>
		public static float GetYLocalPosition(this Transform transform)
		{
			return transform.localPosition.y;
		}

		/// <summary>
		/// Sets the local y position of a transform
		/// </summary>
		public static float SetYLocalPosition(this Transform transform, float newY)
		{
			transform.localPosition = transform.localPosition.With(y: newY);
			return newY;
		}

		/// <summary>
		/// Gets the local z position of a transform
		/// </summary>
		public static float GetZLocalPosition(this Transform transform)
		{
			return transform.localPosition.z;
		}

		/// <summary>
		/// Sets the local z position of a transform
		/// </summary>
		public static float SetZLocalPosition(this Transform transform, float newZ)
		{
			transform.localPosition = transform.localPosition.With(z: newZ);
			return newZ;
		}

		/// <summary>
		/// Gets the local x scale of a transform
		/// </summary>
		public static float GetXLocalScale(this Transform transform)
		{
			return transform.localScale.x;
		}

		/// <summary>
		/// Sets the local x scale of a transform
		/// </summary>
		public static float SetXLocalScale(this Transform transform, float newX)
		{
			transform.localScale = transform.localScale.With(x: newX);
			return newX;
		}

		/// <summary>
		/// Gets the local y scale of a transform
		/// </summary>
		public static float GetYLocalScale(this Transform transform)
		{
			return transform.localScale.y;
		}

		/// <summary>
		/// Sets the local y scale of a transform
		/// </summary>
		public static float SetYLocalScale(this Transform transform, float newY)
		{
			transform.localScale = transform.localScale.With(y: newY);
			return newY;
		}

		/// <summary>
		/// Gets the local z scale of a transform
		/// </summary>
		public static float GetZLocalScale(this Transform transform)
		{
			return transform.localScale.z;
		}

		/// <summary>
		/// Sets the local z scale of a transform
		/// </summary>
		public static float SetZLocalScale(this Transform transform, float newZ)
		{
			transform.localScale = transform.localScale.With(z: newZ);
			return newZ;
		}

		/// <summary>
		/// Sets the local position of the transform
		/// </summary>
		/// <param name="transform">The transform to set</param>
		/// <param name="x">The new local x-position to set. If it's set to NaN, it will leave it unchanged</param>
		/// <param name="y">The new local y-position to set. If it's set to NaN, it will leave it unchanged</param>
		/// <param name="z">The new local z-position to set. If it's set to NaN, it will leave it unchanged</param>
		/// <returns>Returns the new local position that has just been set</returns>
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

		/// <summary>
		/// Gets the local x rotation of a transform
		/// </summary>
		public static float GetXLocalRotation(this Transform transform)
		{
			return transform.localEulerAngles.x;
		}

		/// <summary>
		/// Sets the local x rotation of a transform
		/// </summary>
		public static float SetXLocalRotation(this Transform transform, float newX)
		{
			transform.localEulerAngles = transform.localEulerAngles.With(x: newX);
			return newX;
		}

		/// <summary>
		/// Gets the local y rotation of a transform
		/// </summary>
		public static float GetYLocalRotation(this Transform transform)
		{
			return transform.localEulerAngles.y;
		}

		/// <summary>
		/// Sets the local y rotation of a transform
		/// </summary>
		public static float SetYLocalRotation(this Transform transform, float newY)
		{
			transform.localEulerAngles = transform.localEulerAngles.With(y: newY);
			return newY;
		}

		/// <summary>
		/// Gets the local z rotation of a transform
		/// </summary>
		public static float GetZLocalRotation(this Transform transform)
		{
			return transform.localEulerAngles.z;
		}

		/// <summary>
		/// Sets the local z rotation of a transform
		/// </summary>
		public static float SetZLocalRotation(this Transform transform, float newZ)
		{
			transform.localEulerAngles = transform.localEulerAngles.With(z: newZ);
			return newZ;
		}

		/// <summary>
		/// Sets the local rotation of the transform
		/// </summary>
		/// <param name="transform">The transform to set</param>
		/// <param name="x">The new local x-rotation to set. If it's set to NaN, it will leave it unchanged</param>
		/// <param name="y">The new local y-rotation to set. If it's set to NaN, it will leave it unchanged</param>
		/// <param name="z">The new local z-rotation to set. If it's set to NaN, it will leave it unchanged</param>
		/// <returns>Returns the new local rotation that has just been set</returns>
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

		/// <summary>
		/// Gets the x rotation of a transform
		/// </summary>
		public static float GetXRotation(this Transform transform)
		{
			return transform.eulerAngles.x;
		}

		/// <summary>
		/// Sets the x rotation of a transform
		/// </summary>
		public static float SetXRotation(this Transform transform, float newX)
		{
			transform.eulerAngles = transform.eulerAngles.With(x: newX);
			return newX;
		}

		/// <summary>
		/// Gets the y rotation of a transform
		/// </summary>
		public static float GetYRotation(this Transform transform)
		{
			return transform.eulerAngles.y;
		}

		/// <summary>
		/// Sets the y rotation of a transform
		/// </summary>
		public static float SetYRotation(this Transform transform, float newY)
		{
			transform.eulerAngles = transform.eulerAngles.With(y: newY);
			return newY;
		}

		/// <summary>
		/// Gets the z rotation of a transform
		/// </summary>
		public static float GetZRotation(this Transform transform)
		{
			return transform.eulerAngles.z;
		}

		/// <summary>
		/// Sets the z rotation of a transform
		/// </summary>
		public static float SetZRotation(this Transform transform, float newZ)
		{
			transform.eulerAngles = transform.eulerAngles.With(z: newZ);
			return newZ;
		}

		/// <summary>
		/// Sets the rotation of the transform
		/// </summary>
		/// <param name="transform">The transform to set</param>
		/// <param name="x">The new x-rotation to set. If it's set to NaN, it will leave it unchanged</param>
		/// <param name="y">The new y-rotation to set. If it's set to NaN, it will leave it unchanged</param>
		/// <param name="z">The new z-rotation to set. If it's set to NaN, it will leave it unchanged</param>
		/// <returns>Returns the new rotation that has just been set</returns>
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

		/// <summary>
		/// Will rapidly jitter an object until the coroutine gets cancelled
		/// </summary>
		/// <param name="transform">The transform to jitter</param>
		/// <param name="amount">The amount of jitter to be applied</param>
		/// <param name="jitterFPS">How fast the jitter should be applied</param>
		public static IEnumerator JitterObject(Transform transform, Vector3 amount, float jitterFPS = float.PositiveInfinity)
		{
			Vector3 startPosition = transform.position;
			float timer = 0f;

			while (true)
			{
				if (float.IsPositiveInfinity(jitterFPS) || float.IsNaN(jitterFPS) || float.IsNegativeInfinity(jitterFPS))
				{
					yield return null;
				}
				else
				{
					timer += Time.deltaTime;
					if (timer >= (1f / jitterFPS))
					{
						timer -= (1f / jitterFPS);
					}
					else
					{
						yield return null;
						continue;
					}
				}
				transform.position = new Vector3(startPosition.x + Random.Range(-amount.x, amount.x), startPosition.y + Random.Range(-amount.y, amount.y), startPosition.z + Random.Range(-amount.z, amount.z));
			}
		}

		/// <summary>
		/// Will rapidly jitter an object until the coroutine gets cancelled
		/// </summary>
		/// <param name="gameObject">The object to jitter</param>
		/// <param name="amount">The amount of jitter to be applied</param>
		/// <param name="jitterFPS">How fast the jitter should be applied</param>
		public static IEnumerator JitterObject(GameObject gameObject, Vector3 amount, float jitterFPS = float.PositiveInfinity)
		{
			return JitterObject(gameObject.transform, amount, jitterFPS);
		}

		/// <summary>
		/// Spawns a bunch of new objects. The quantity, rigidbody velocity, velocity angle, and spawn origin are all randomized
		/// </summary>
		/// <param name="obj">The object to spawn</param>
		/// <param name="spawnPoint">The place where all the objects will spawn</param>
		/// <param name="quantityMin">The minimum amount of objects to spawn</param>
		/// <param name="quantityMax">The maximum amount of objects to spawn</param>
		/// <param name="minVelocity">The minimum velocity to be applied to each object</param>
		/// <param name="maxVelocity">The maximum amount of velocity to be applied to each object</param>
		/// <param name="angleMin">The minimum velocity angle to be applied to each object</param>
		/// <param name="angleMax">The maximum velocity angle to be applied to each object</param>
		/// <param name="originOffset">How far away can objects spawn from the <paramref name="spawnPoint"/></param>
		/// <returns>Returns a list of all the objects that have been spawned</returns>
		public static GameObject[] SpawnRandomObjects(GameObject obj, Vector3 spawnPoint, int quantityMin, int quantityMax, float minVelocity, float maxVelocity, float angleMin, float angleMax, Vector2 originOffset = default(Vector2))
		{
			int spawnNum = Random.Range(quantityMin, quantityMax + 1);
			float speedNum = Random.Range(minVelocity, maxVelocity);
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

		/// <summary>
		/// Spawns a bunch of new objects. The quantity, rigidbody velocity, velocity angle, and spawn origin are all randomized
		/// </summary>
		/// <param name="pool">The pool to spawn the objects from</param>
		/// <param name="spawnPoint">The place where all the objects will spawn</param>
		/// <param name="quantityMin">The minimum amount of objects to spawn</param>
		/// <param name="quantityMax">The maximum amount of objects to spawn</param>
		/// <param name="minVelocity">The minimum velocity to be applied to each object</param>
		/// <param name="maxVelocity">The maximum amount of velocity to be applied to each object</param>
		/// <param name="angleMin">The minimum velocity angle to be applied to each object</param>
		/// <param name="angleMax">The maximum velocity angle to be applied to each object</param>
		/// <param name="originOffset">How far away can objects spawn from the <paramref name="spawnPoint"/></param>
		/// <returns>Returns a list of all the objects that have been spawned</returns>
		public static GameObject[] SpawnRandomObjects(ObjectPool pool, Vector3 spawnPoint, int quantityMin, int quantityMax, float minVelocity, float maxVelocity, float angleMin, float angleMax, Vector2 originOffset = default(Vector2))
		{
			int spawnNum = Random.Range(quantityMin, quantityMax + 1);
			float speedNum = Random.Range(minVelocity, maxVelocity);
			float angleNum = Random.Range(angleMin, angleMax);

			GameObject[] instances = new GameObject[spawnNum];

			for (int i = 0; i < spawnNum; i++)
			{
				var instance = pool.Instantiate(new Vector3(spawnPoint.x + Random.Range(-originOffset.x, originOffset.x), spawnPoint.y + Random.Range(-originOffset.y, originOffset.y), spawnPoint.z), Quaternion.identity);
				instances[i] = instance;
				var rigid = instance.GetComponent<Rigidbody2D>();
				if (rigid != null)
				{
					rigid.velocity = new Vector2(Mathf.Cos(angleNum) * Mathf.Deg2Rad, Mathf.Sin(angleNum) * Mathf.Deg2Rad);
				}
			}
			return instances;
		}

		/// <summary>
		/// Gets a hash of the transform and it's hiearchy
		/// </summary>
		/// <param name="transform">The transform to check</param>
		/// <param name="includeChildren">Should child objects also be included in the hash?</param>
		/// <returns>Returns the hash of the transform's hiearchy</returns>
		public static int GetHashOfTransform(Transform transform, bool includeChildren = true)
		{
			int hash = transform.gameObject.name.GetHashCode();

			if (includeChildren)
			{
				for (int i = 0; i < transform.childCount; i++)
				{
					HashUtilities.AdditiveHash(ref hash, GetHashOfTransform(transform.GetChild(i), true));
				}
			}

			return hash;
		}
	}
}
