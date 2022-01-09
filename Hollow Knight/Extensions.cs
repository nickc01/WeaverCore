using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class Extensions
{
	public static Selectable GetFirstInteractable(this Selectable start)
	{
		if (start == null)
		{
			return null;
		}
		if (start.interactable)
		{
			return start;
		}
		return start.navigation.selectOnDown.GetFirstInteractable();
	}

	public static void PlayOnSource(this AudioClip self, AudioSource source, float pitchMin = 1f, float pitchMax = 1f)
	{
		if (self && source)
		{
			source.pitch = UnityEngine.Random.Range(pitchMin, pitchMax);
			source.PlayOneShot(self);
		}
	}

	public static void SetActiveChildren(this GameObject self, bool value)
	{
		int childCount = self.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			self.transform.GetChild(i).gameObject.SetActive(value);
		}
	}

	public static void SetActiveWithChildren(this MeshRenderer self, bool value)
	{
		if (self.transform.childCount > 0)
		{
			MeshRenderer[] componentsInChildren = self.GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = value;
			}
			return;
		}
		self.enabled = value;
	}

	public static bool HasParameter(this Animator self, string paramName, AnimatorControllerParameterType? type = null)
	{
		foreach (AnimatorControllerParameter animatorControllerParameter in self.parameters)
		{
			if (animatorControllerParameter.name == paramName)
			{
				if (type != null)
				{
					AnimatorControllerParameterType type2 = animatorControllerParameter.type;
					AnimatorControllerParameterType? animatorControllerParameterType = type;
					if (!(type2 == animatorControllerParameterType.GetValueOrDefault() & animatorControllerParameterType != null))
					{
						goto IL_43;
					}
				}
				return true;
			}
		IL_43:;
		}
		return false;
	}

	public static Vector3 MultiplyElements(this Vector3 self, Vector3 other)
	{
		Vector3 result = self;
		result.x *= other.x;
		result.y *= other.y;
		result.z *= other.z;
		return result;
	}

	public static Vector2 MultiplyElements(this Vector2 self, Vector2 other)
	{
		Vector2 result = self;
		result.x *= other.x;
		result.y *= other.y;
		return result;
	}

	public static void SetPositionX(this Transform t, float newX)
	{
		t.position = new Vector3(newX, t.position.y, t.position.z);
	}

	public static void SetPositionY(this Transform t, float newY)
	{
		t.position = new Vector3(t.position.x, newY, t.position.z);
	}

	public static void SetPositionZ(this Transform t, float newZ)
	{
		t.position = new Vector3(t.position.x, t.position.y, newZ);
	}

	public static void SetPosition2D(this Transform t, float x, float y)
	{
		t.position = new Vector3(x, y, t.position.z);
	}

	public static void SetPosition2D(this Transform t, Vector2 position)
	{
		t.position = new Vector3(position.x, position.y, t.position.z);
	}

	public static void SetPosition3D(this Transform t, float x, float y, float z)
	{
		t.position = new Vector3(x, y, z);
	}

	public static void SetScaleX(this Transform t, float newXScale)
	{
		t.localScale = new Vector3(newXScale, t.localScale.y, t.localScale.z);
	}

	public static void SetScaleY(this Transform t, float newYScale)
	{
		t.localScale = new Vector3(t.localScale.x, newYScale, t.localScale.z);
	}

	public static void SetScaleZ(this Transform t, float newZScale)
	{
		t.localScale = new Vector3(t.localScale.x, t.localScale.y, newZScale);
	}

	public static void SetRotationZ(this Transform t, float newZRotation)
	{
		t.localEulerAngles = new Vector3(t.localEulerAngles.x, t.localEulerAngles.y, newZRotation);
	}

	public static void SetScaleMatching(this Transform t, float newScale)
	{
		t.localScale = new Vector3(newScale, newScale, newScale);
	}

	public static float GetPositionX(this Transform t)
	{
		return t.position.x;
	}

	public static float GetPositionY(this Transform t)
	{
		return t.position.y;
	}

	public static float GetPositionZ(this Transform t)
	{
		return t.position.z;
	}

	public static float GetScaleX(this Transform t)
	{
		return t.localScale.x;
	}

	public static float GetScaleY(this Transform t)
	{
		return t.localScale.y;
	}

	public static float GetScaleZ(this Transform t)
	{
		return t.localScale.z;
	}

	public static float GetRotation2D(this Transform t)
	{
		return t.localEulerAngles.z;
	}

	public static void SetRotation2D(this Transform t, float rotation)
	{
		Vector3 eulerAngles = t.eulerAngles;
		eulerAngles.z = rotation;
		t.eulerAngles = eulerAngles;
	}

	public static bool IsAny(this string value, params string[] others)
	{
		foreach (string b in others)
		{
			if (value == b)
			{
				return true;
			}
		}
		return false;
	}
}
