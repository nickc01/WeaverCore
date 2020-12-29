using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
	// Token: 0x0200000F RID: 15
	public class G_PlayMaker_I : PlayMaker_I
	{
		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000038 RID: 56 RVA: 0x0000305D File Offset: 0x0000125D
		public override bool PlayMakerAvailable
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000039 RID: 57 RVA: 0x00003060 File Offset: 0x00001260
		public override Type PlayMakerFSMType
		{
			get
			{
				return typeof(PlayMakerFSM);
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600003A RID: 58 RVA: 0x0000306C File Offset: 0x0000126C
		public override Type FSMType
		{
			get
			{
				return typeof(Fsm);
			}
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00003078 File Offset: 0x00001278
		public override IEnumerable<string> GetAllFsmsOnObject(GameObject gameObject)
		{
			PlayMakerFSM[] fsms = gameObject.GetComponents<PlayMakerFSM>();
			foreach (PlayMakerFSM fsm in fsms)
			{
				yield return fsm.FsmName;
			}
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00003090 File Offset: 0x00001290
		private static PlayMakerFSM GetFsm(GameObject obj, string fsmName)
		{
			return ActionHelpers.GetGameObjectFsm(obj, fsmName);
		}

		// Token: 0x0600003D RID: 61 RVA: 0x000030AC File Offset: 0x000012AC
		public override object[] GetFsmArray(GameObject obj, string fsmName, string varName)
		{
			return G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmArray(varName).Values;
		}

		// Token: 0x0600003E RID: 62 RVA: 0x000030D8 File Offset: 0x000012D8
		public override bool GetFsmBool(GameObject obj, string fsmName, string varName)
		{
			return G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmBool(varName).Value;
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00003104 File Offset: 0x00001304
		public override Color GetFsmColor(GameObject obj, string fsmName, string varName)
		{
			return G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmColor(varName).Value;
		}

		// Token: 0x06000040 RID: 64 RVA: 0x00003130 File Offset: 0x00001330
		public override Enum GetFsmEnum(GameObject obj, string fsmName, string varName)
		{
			return G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmEnum(varName).Value;
		}

		// Token: 0x06000041 RID: 65 RVA: 0x0000315C File Offset: 0x0000135C
		public override float GetFsmFloat(GameObject obj, string fsmName, string varName)
		{
			return G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmFloat(varName).Value;
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00003188 File Offset: 0x00001388
		public override GameObject GetFsmGameObject(GameObject obj, string fsmName, string varName)
		{
			return G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmGameObject(varName).Value;
		}

		// Token: 0x06000043 RID: 67 RVA: 0x000031B4 File Offset: 0x000013B4
		public override int GetFsmInt(GameObject obj, string fsmName, string varName)
		{
			return G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmInt(varName).Value;
		}

		// Token: 0x06000044 RID: 68 RVA: 0x000031E0 File Offset: 0x000013E0
		public override Material GetFsmMaterial(GameObject obj, string fsmName, string varName)
		{
			return G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmMaterial(varName).Value;
		}

		// Token: 0x06000045 RID: 69 RVA: 0x0000320C File Offset: 0x0000140C
		public override UnityEngine.Object GetFsmObject(GameObject obj, string fsmName, string varName)
		{
			return G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmObject(varName).Value;
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00003238 File Offset: 0x00001438
		public override Quaternion GetFsmQuaternion(GameObject obj, string fsmName, string varName)
		{
			return G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmQuaternion(varName).Value;
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00003264 File Offset: 0x00001464
		public override Rect GetFsmRect(GameObject obj, string fsmName, string varName)
		{
			return G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmRect(varName).Value;
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00003290 File Offset: 0x00001490
		public override string GetFsmString(GameObject obj, string fsmName, string varName)
		{
			return G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmString(varName).Value;
		}

		// Token: 0x06000049 RID: 73 RVA: 0x000032BC File Offset: 0x000014BC
		public override Texture GetFsmTexture(GameObject obj, string fsmName, string varName)
		{
			return G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmTexture(varName).Value;
		}

		// Token: 0x0600004A RID: 74 RVA: 0x000032E8 File Offset: 0x000014E8
		public override Vector2 GetFsmVector2(GameObject obj, string fsmName, string varName)
		{
			return G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmVector2(varName).Value;
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00003314 File Offset: 0x00001514
		public override Vector3 GetFsmVector3(GameObject obj, string fsmName, string varName)
		{
			return G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmVector3(varName).Value;
		}

		// Token: 0x0600004C RID: 76 RVA: 0x0000333D File Offset: 0x0000153D
		public override void SetFsmArray(GameObject obj, string fsmName, string varName, object[] value)
		{
			G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmArray(varName).Values = value;
		}

		// Token: 0x0600004D RID: 77 RVA: 0x0000335A File Offset: 0x0000155A
		public override void SetFsmBool(GameObject obj, string fsmName, string varName, bool value)
		{
			G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmBool(varName).Value = value;
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00003377 File Offset: 0x00001577
		public override void SetFsmColor(GameObject obj, string fsmName, string varName, Color value)
		{
			G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmColor(varName).Value = value;
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00003394 File Offset: 0x00001594
		public override void SetFsmEnum(GameObject obj, string fsmName, string varName, Enum value)
		{
			G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmEnum(varName).Value = value;
		}

		// Token: 0x06000050 RID: 80 RVA: 0x000033B1 File Offset: 0x000015B1
		public override void SetFsmFloat(GameObject obj, string fsmName, string varName, float value)
		{
			G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmFloat(varName).Value = value;
		}

		// Token: 0x06000051 RID: 81 RVA: 0x000033CE File Offset: 0x000015CE
		public override void SetFsmGameObject(GameObject obj, string fsmName, string varName, GameObject value)
		{
			G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmGameObject(varName).Value = value;
		}

		// Token: 0x06000052 RID: 82 RVA: 0x000033EB File Offset: 0x000015EB
		public override void SetFsmInt(GameObject obj, string fsmName, string varName, int value)
		{
			G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmInt(varName).Value = value;
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00003408 File Offset: 0x00001608
		public override void SetFsmMaterial(GameObject obj, string fsmName, string varName, Material value)
		{
			G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmMaterial(varName).Value = value;
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00003425 File Offset: 0x00001625
		public override void SetFsmObject(GameObject obj, string fsmName, string varName, UnityEngine.Object value)
		{
			G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmObject(varName).Value = value;
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00003442 File Offset: 0x00001642
		public override void SetFsmQuaternion(GameObject obj, string fsmName, string varName, Quaternion value)
		{
			G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmQuaternion(varName).Value = value;
		}

		// Token: 0x06000056 RID: 86 RVA: 0x0000345F File Offset: 0x0000165F
		public override void SetFsmRect(GameObject obj, string fsmName, string varName, Rect value)
		{
			G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmRect(varName).Value = value;
		}

		// Token: 0x06000057 RID: 87 RVA: 0x0000347C File Offset: 0x0000167C
		public override void SetFsmString(GameObject obj, string fsmName, string varName, string value)
		{
			G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmString(varName).Value = value;
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00003499 File Offset: 0x00001699
		public override void SetFsmTexture(GameObject obj, string fsmName, string varName, Texture value)
		{
			G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmTexture(varName).Value = value;
		}

		// Token: 0x06000059 RID: 89 RVA: 0x000034B6 File Offset: 0x000016B6
		public override void SetFsmVector2(GameObject obj, string fsmName, string varName, Vector2 value)
		{
			G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmVector2(varName).Value = value;
		}

		// Token: 0x0600005A RID: 90 RVA: 0x000034D3 File Offset: 0x000016D3
		public override void SetFsmVector3(GameObject obj, string fsmName, string varName, Vector3 value)
		{
			G_PlayMaker_I.GetFsm(obj, fsmName).FsmVariables.GetFsmVector3(varName).Value = value;
		}
	}
}
