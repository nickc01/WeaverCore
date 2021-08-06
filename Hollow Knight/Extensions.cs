using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020004D8 RID: 1240
public static class Extensions
{
	// Token: 0x06001B5C RID: 7004 RVA: 0x00083010 File Offset: 0x00081210
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

	// Token: 0x06001B5D RID: 7005 RVA: 0x00083045 File Offset: 0x00081245
	public static void PlayOnSource(this AudioClip self, AudioSource source, float pitchMin = 1f, float pitchMax = 1f)
	{
		if (self && source)
		{
			source.pitch = UnityEngine.Random.Range(pitchMin, pitchMax);
			source.PlayOneShot(self);
		}
	}

	// Token: 0x06001B5E RID: 7006 RVA: 0x0008306C File Offset: 0x0008126C
	public static void SetActiveChildren(this GameObject self, bool value)
	{
		int childCount = self.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			self.transform.GetChild(i).gameObject.SetActive(value);
		}
	}

	// Token: 0x06001B5F RID: 7007 RVA: 0x000830A8 File Offset: 0x000812A8
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

	// Token: 0x06001B60 RID: 7008 RVA: 0x000830EC File Offset: 0x000812EC
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

	// Token: 0x06001B63 RID: 7011 RVA: 0x00083184 File Offset: 0x00081384
	public static Vector3 MultiplyElements(this Vector3 self, Vector3 other)
	{
		Vector3 result = self;
		result.x *= other.x;
		result.y *= other.y;
		result.z *= other.z;
		return result;
	}

	// Token: 0x06001B64 RID: 7012 RVA: 0x000831C8 File Offset: 0x000813C8
	public static Vector2 MultiplyElements(this Vector2 self, Vector2 other)
	{
		Vector2 result = self;
		result.x *= other.x;
		result.y *= other.y;
		return result;
	}

	// Token: 0x06001B65 RID: 7013 RVA: 0x000831FA File Offset: 0x000813FA
	public static void SetPositionX(this Transform t, float newX)
	{
		t.position = new Vector3(newX, t.position.y, t.position.z);
	}

	// Token: 0x06001B66 RID: 7014 RVA: 0x0008321E File Offset: 0x0008141E
	public static void SetPositionY(this Transform t, float newY)
	{
		t.position = new Vector3(t.position.x, newY, t.position.z);
	}

	// Token: 0x06001B67 RID: 7015 RVA: 0x00083242 File Offset: 0x00081442
	public static void SetPositionZ(this Transform t, float newZ)
	{
		t.position = new Vector3(t.position.x, t.position.y, newZ);
	}

	// Token: 0x06001B68 RID: 7016 RVA: 0x00083266 File Offset: 0x00081466
	public static void SetPosition2D(this Transform t, float x, float y)
	{
		t.position = new Vector3(x, y, t.position.z);
	}

	// Token: 0x06001B69 RID: 7017 RVA: 0x00083280 File Offset: 0x00081480
	public static void SetPosition2D(this Transform t, Vector2 position)
	{
		t.position = new Vector3(position.x, position.y, t.position.z);
	}

	// Token: 0x06001B6A RID: 7018 RVA: 0x000832A4 File Offset: 0x000814A4
	public static void SetPosition3D(this Transform t, float x, float y, float z)
	{
		t.position = new Vector3(x, y, z);
	}

	// Token: 0x06001B6B RID: 7019 RVA: 0x000832B4 File Offset: 0x000814B4
	public static void SetScaleX(this Transform t, float newXScale)
	{
		t.localScale = new Vector3(newXScale, t.localScale.y, t.localScale.z);
	}

	// Token: 0x06001B6C RID: 7020 RVA: 0x000832D8 File Offset: 0x000814D8
	public static void SetScaleY(this Transform t, float newYScale)
	{
		t.localScale = new Vector3(t.localScale.x, newYScale, t.localScale.z);
	}

	// Token: 0x06001B6D RID: 7021 RVA: 0x000832FC File Offset: 0x000814FC
	public static void SetScaleZ(this Transform t, float newZScale)
	{
		t.localScale = new Vector3(t.localScale.x, t.localScale.y, newZScale);
	}

	// Token: 0x06001B6E RID: 7022 RVA: 0x00083320 File Offset: 0x00081520
	public static void SetRotationZ(this Transform t, float newZRotation)
	{
		t.localEulerAngles = new Vector3(t.localEulerAngles.x, t.localEulerAngles.y, newZRotation);
	}

	// Token: 0x06001B6F RID: 7023 RVA: 0x00083344 File Offset: 0x00081544
	public static void SetScaleMatching(this Transform t, float newScale)
	{
		t.localScale = new Vector3(newScale, newScale, newScale);
	}

	// Token: 0x06001B70 RID: 7024 RVA: 0x00083354 File Offset: 0x00081554
	public static float GetPositionX(this Transform t)
	{
		return t.position.x;
	}

	// Token: 0x06001B71 RID: 7025 RVA: 0x00083361 File Offset: 0x00081561
	public static float GetPositionY(this Transform t)
	{
		return t.position.y;
	}

	// Token: 0x06001B72 RID: 7026 RVA: 0x0008336E File Offset: 0x0008156E
	public static float GetPositionZ(this Transform t)
	{
		return t.position.z;
	}

	// Token: 0x06001B73 RID: 7027 RVA: 0x0008337B File Offset: 0x0008157B
	public static float GetScaleX(this Transform t)
	{
		return t.localScale.x;
	}

	// Token: 0x06001B74 RID: 7028 RVA: 0x00083388 File Offset: 0x00081588
	public static float GetScaleY(this Transform t)
	{
		return t.localScale.y;
	}

	// Token: 0x06001B75 RID: 7029 RVA: 0x00083395 File Offset: 0x00081595
	public static float GetScaleZ(this Transform t)
	{
		return t.localScale.z;
	}

	// Token: 0x06001B76 RID: 7030 RVA: 0x000833A2 File Offset: 0x000815A2
	public static float GetRotation2D(this Transform t)
	{
		return t.localEulerAngles.z;
	}

	// Token: 0x06001B77 RID: 7031 RVA: 0x000833B0 File Offset: 0x000815B0
	public static void SetRotation2D(this Transform t, float rotation)
	{
		Vector3 eulerAngles = t.eulerAngles;
		eulerAngles.z = rotation;
		t.eulerAngles = eulerAngles;
	}

	// Token: 0x06001B78 RID: 7032 RVA: 0x000833D4 File Offset: 0x000815D4
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
