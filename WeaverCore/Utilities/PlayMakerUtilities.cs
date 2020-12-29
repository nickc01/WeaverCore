using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Implementations;

namespace WeaverCore.Utilities
{
	// Token: 0x020000AC RID: 172
	public static class PlayMakerUtilities
	{
		// Token: 0x17000089 RID: 137
		// (get) Token: 0x0600032F RID: 815 RVA: 0x0000A840 File Offset: 0x00008A40
		public static bool PlayMakerAvailable
		{
			get
			{
				return PlayMakerUtilities.impl.PlayMakerAvailable;
			}
		}

		// Token: 0x1700008A RID: 138
		// (get) Token: 0x06000330 RID: 816 RVA: 0x0000A84C File Offset: 0x00008A4C
		public static Type PlayMakerFSMType
		{
			get
			{
				return PlayMakerUtilities.impl.PlayMakerFSMType;
			}
		}

		// Token: 0x1700008B RID: 139
		// (get) Token: 0x06000331 RID: 817 RVA: 0x0000A858 File Offset: 0x00008A58
		public static Type FSMType
		{
			get
			{
				return PlayMakerUtilities.impl.FSMType;
			}
		}

		// Token: 0x06000332 RID: 818 RVA: 0x0000A864 File Offset: 0x00008A64
		public static IEnumerable<string> GetAllFsmsOnObject(GameObject gameObject)
		{
			return PlayMakerUtilities.impl.GetAllFsmsOnObject(gameObject);
		}

		// Token: 0x06000333 RID: 819 RVA: 0x0000A871 File Offset: 0x00008A71
		public static UnityEngine.Object GetFsmObject(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmObject(obj, fsmName, varName);
		}

		// Token: 0x06000334 RID: 820 RVA: 0x0000A880 File Offset: 0x00008A80
		public static void SetFsmObject(GameObject obj, string fsmName, string varName, UnityEngine.Object value)
		{
			PlayMakerUtilities.impl.SetFsmObject(obj, fsmName, varName, value);
		}

		// Token: 0x06000335 RID: 821 RVA: 0x0000A890 File Offset: 0x00008A90
		public static Material GetFsmMaterial(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmMaterial(obj, fsmName, varName);
		}

		// Token: 0x06000336 RID: 822 RVA: 0x0000A89F File Offset: 0x00008A9F
		public static void SetFsmMaterial(GameObject obj, string fsmName, string varName, Material value)
		{
			PlayMakerUtilities.impl.SetFsmMaterial(obj, fsmName, varName, value);
		}

		// Token: 0x06000337 RID: 823 RVA: 0x0000A8AF File Offset: 0x00008AAF
		public static Texture GetFsmTexture(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmTexture(obj, fsmName, varName);
		}

		// Token: 0x06000338 RID: 824 RVA: 0x0000A8BE File Offset: 0x00008ABE
		public static void SetFsmTexture(GameObject obj, string fsmName, string varName, Texture value)
		{
			PlayMakerUtilities.impl.SetFsmTexture(obj, fsmName, varName, value);
		}

		// Token: 0x06000339 RID: 825 RVA: 0x0000A8CE File Offset: 0x00008ACE
		public static float GetFsmFloat(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmFloat(obj, fsmName, varName);
		}

		// Token: 0x0600033A RID: 826 RVA: 0x0000A8DD File Offset: 0x00008ADD
		public static void SetFsmFloat(GameObject obj, string fsmName, string varName, float value)
		{
			PlayMakerUtilities.impl.SetFsmFloat(obj, fsmName, varName, value);
		}

		// Token: 0x0600033B RID: 827 RVA: 0x0000A8ED File Offset: 0x00008AED
		public static int GetFsmInt(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmInt(obj, fsmName, varName);
		}

		// Token: 0x0600033C RID: 828 RVA: 0x0000A8FC File Offset: 0x00008AFC
		public static void SetFsmInt(GameObject obj, string fsmName, string varName, int value)
		{
			PlayMakerUtilities.impl.SetFsmInt(obj, fsmName, varName, value);
		}

		// Token: 0x0600033D RID: 829 RVA: 0x0000A90C File Offset: 0x00008B0C
		public static bool GetFsmBool(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmBool(obj, fsmName, varName);
		}

		// Token: 0x0600033E RID: 830 RVA: 0x0000A91B File Offset: 0x00008B1B
		public static void SetFsmBool(GameObject obj, string fsmName, string varName, bool value)
		{
			PlayMakerUtilities.impl.SetFsmBool(obj, fsmName, varName, value);
		}

		// Token: 0x0600033F RID: 831 RVA: 0x0000A92B File Offset: 0x00008B2B
		public static string GetFsmString(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmString(obj, fsmName, varName);
		}

		// Token: 0x06000340 RID: 832 RVA: 0x0000A93A File Offset: 0x00008B3A
		public static void SetFsmString(GameObject obj, string fsmName, string varName, string value)
		{
			PlayMakerUtilities.impl.SetFsmString(obj, fsmName, varName, value);
		}

		// Token: 0x06000341 RID: 833 RVA: 0x0000A94A File Offset: 0x00008B4A
		public static Vector2 GetFsmVector2(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmVector2(obj, fsmName, varName);
		}

		// Token: 0x06000342 RID: 834 RVA: 0x0000A959 File Offset: 0x00008B59
		public static void SetFsmVector2(GameObject obj, string fsmName, string varName, Vector2 value)
		{
			PlayMakerUtilities.impl.SetFsmVector2(obj, fsmName, varName, value);
		}

		// Token: 0x06000343 RID: 835 RVA: 0x0000A969 File Offset: 0x00008B69
		public static Vector3 GetFsmVector3(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmVector3(obj, fsmName, varName);
		}

		// Token: 0x06000344 RID: 836 RVA: 0x0000A978 File Offset: 0x00008B78
		public static void SetFsmVector3(GameObject obj, string fsmName, string varName, Vector3 value)
		{
			PlayMakerUtilities.impl.SetFsmVector3(obj, fsmName, varName, value);
		}

		// Token: 0x06000345 RID: 837 RVA: 0x0000A988 File Offset: 0x00008B88
		public static Rect GetFsmRect(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmRect(obj, fsmName, varName);
		}

		// Token: 0x06000346 RID: 838 RVA: 0x0000A997 File Offset: 0x00008B97
		public static void SetFsmRect(GameObject obj, string fsmName, string varName, Rect value)
		{
			PlayMakerUtilities.impl.SetFsmRect(obj, fsmName, varName, value);
		}

		// Token: 0x06000347 RID: 839 RVA: 0x0000A9A7 File Offset: 0x00008BA7
		public static Quaternion GetFsmQuaternion(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmQuaternion(obj, fsmName, varName);
		}

		// Token: 0x06000348 RID: 840 RVA: 0x0000A9B6 File Offset: 0x00008BB6
		public static void SetFsmQuaternion(GameObject obj, string fsmName, string varName, Quaternion value)
		{
			PlayMakerUtilities.impl.SetFsmQuaternion(obj, fsmName, varName, value);
		}

		// Token: 0x06000349 RID: 841 RVA: 0x0000A9C6 File Offset: 0x00008BC6
		public static Color GetFsmColor(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmColor(obj, fsmName, varName);
		}

		// Token: 0x0600034A RID: 842 RVA: 0x0000A9D5 File Offset: 0x00008BD5
		public static void SetFsmColor(GameObject obj, string fsmName, string varName, Color value)
		{
			PlayMakerUtilities.impl.SetFsmColor(obj, fsmName, varName, value);
		}

		// Token: 0x0600034B RID: 843 RVA: 0x0000A9E5 File Offset: 0x00008BE5
		public static GameObject GetFsmGameObject(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmGameObject(obj, fsmName, varName);
		}

		// Token: 0x0600034C RID: 844 RVA: 0x0000A9F4 File Offset: 0x00008BF4
		public static void SetFsmGameObject(GameObject obj, string fsmName, string varName, GameObject value)
		{
			PlayMakerUtilities.impl.SetFsmGameObject(obj, fsmName, varName, value);
		}

		// Token: 0x0600034D RID: 845 RVA: 0x0000AA04 File Offset: 0x00008C04
		public static object[] GetFsmArray(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmArray(obj, fsmName, varName);
		}

		// Token: 0x0600034E RID: 846 RVA: 0x0000AA13 File Offset: 0x00008C13
		public static void SetFsmArray(GameObject obj, string fsmName, string varName, object[] value)
		{
			PlayMakerUtilities.impl.SetFsmArray(obj, fsmName, varName, value);
		}

		// Token: 0x0600034F RID: 847 RVA: 0x0000AA23 File Offset: 0x00008C23
		public static Enum GetFsmEnum(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmEnum(obj, fsmName, varName);
		}

		// Token: 0x06000350 RID: 848 RVA: 0x0000AA32 File Offset: 0x00008C32
		public static void SetFsmEnum(GameObject obj, string fsmName, string varName, Enum value)
		{
			PlayMakerUtilities.impl.SetFsmEnum(obj, fsmName, varName, value);
		}

		// Token: 0x0400022A RID: 554
		private static PlayMaker_I impl = ImplFinder.GetImplementation<PlayMaker_I>();
	}
}
