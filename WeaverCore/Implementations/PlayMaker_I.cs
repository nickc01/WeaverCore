using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	// Token: 0x0200006F RID: 111
	public abstract class PlayMaker_I : IImplementation
	{
		// Token: 0x17000060 RID: 96
		// (get) Token: 0x06000200 RID: 512
		public abstract bool PlayMakerAvailable { get; }

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x06000201 RID: 513
		public abstract Type PlayMakerFSMType { get; }

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x06000202 RID: 514
		public abstract Type FSMType { get; }

		// Token: 0x06000203 RID: 515
		public abstract IEnumerable<string> GetAllFsmsOnObject(GameObject gameObject);

		// Token: 0x06000204 RID: 516
		public abstract UnityEngine.Object GetFsmObject(GameObject obj, string fsmName, string varName);

		// Token: 0x06000205 RID: 517
		public abstract void SetFsmObject(GameObject obj, string fsmName, string varName, UnityEngine.Object value);

		// Token: 0x06000206 RID: 518
		public abstract Material GetFsmMaterial(GameObject obj, string fsmName, string varName);

		// Token: 0x06000207 RID: 519
		public abstract void SetFsmMaterial(GameObject obj, string fsmName, string varName, Material value);

		// Token: 0x06000208 RID: 520
		public abstract Texture GetFsmTexture(GameObject obj, string fsmName, string varName);

		// Token: 0x06000209 RID: 521
		public abstract void SetFsmTexture(GameObject obj, string fsmName, string varName, Texture value);

		// Token: 0x0600020A RID: 522
		public abstract float GetFsmFloat(GameObject obj, string fsmName, string varName);

		// Token: 0x0600020B RID: 523
		public abstract void SetFsmFloat(GameObject obj, string fsmName, string varName, float value);

		// Token: 0x0600020C RID: 524
		public abstract int GetFsmInt(GameObject obj, string fsmName, string varName);

		// Token: 0x0600020D RID: 525
		public abstract void SetFsmInt(GameObject obj, string fsmName, string varName, int value);

		// Token: 0x0600020E RID: 526
		public abstract bool GetFsmBool(GameObject obj, string fsmName, string varName);

		// Token: 0x0600020F RID: 527
		public abstract void SetFsmBool(GameObject obj, string fsmName, string varName, bool value);

		// Token: 0x06000210 RID: 528
		public abstract string GetFsmString(GameObject obj, string fsmName, string varName);

		// Token: 0x06000211 RID: 529
		public abstract void SetFsmString(GameObject obj, string fsmName, string varName, string value);

		// Token: 0x06000212 RID: 530
		public abstract Vector2 GetFsmVector2(GameObject obj, string fsmName, string varName);

		// Token: 0x06000213 RID: 531
		public abstract void SetFsmVector2(GameObject obj, string fsmName, string varName, Vector2 value);

		// Token: 0x06000214 RID: 532
		public abstract Vector3 GetFsmVector3(GameObject obj, string fsmName, string varName);

		// Token: 0x06000215 RID: 533
		public abstract void SetFsmVector3(GameObject obj, string fsmName, string varName, Vector3 value);

		// Token: 0x06000216 RID: 534
		public abstract Rect GetFsmRect(GameObject obj, string fsmName, string varName);

		// Token: 0x06000217 RID: 535
		public abstract void SetFsmRect(GameObject obj, string fsmName, string varName, Rect value);

		// Token: 0x06000218 RID: 536
		public abstract Quaternion GetFsmQuaternion(GameObject obj, string fsmName, string varName);

		// Token: 0x06000219 RID: 537
		public abstract void SetFsmQuaternion(GameObject obj, string fsmName, string varName, Quaternion value);

		// Token: 0x0600021A RID: 538
		public abstract Color GetFsmColor(GameObject obj, string fsmName, string varName);

		// Token: 0x0600021B RID: 539
		public abstract void SetFsmColor(GameObject obj, string fsmName, string varName, Color value);

		// Token: 0x0600021C RID: 540
		public abstract GameObject GetFsmGameObject(GameObject obj, string fsmName, string varName);

		// Token: 0x0600021D RID: 541
		public abstract void SetFsmGameObject(GameObject obj, string fsmName, string varName, GameObject value);

		// Token: 0x0600021E RID: 542
		public abstract object[] GetFsmArray(GameObject obj, string fsmName, string varName);

		// Token: 0x0600021F RID: 543
		public abstract void SetFsmArray(GameObject obj, string fsmName, string varName, object[] value);

		// Token: 0x06000220 RID: 544
		public abstract Enum GetFsmEnum(GameObject obj, string fsmName, string varName);

		// Token: 0x06000221 RID: 545
		public abstract void SetFsmEnum(GameObject obj, string fsmName, string varName, Enum value);
	}
}
