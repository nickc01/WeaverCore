using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class PlayMaker_I : IImplementation
	{
		public abstract bool PlayMakerAvailable { get; }

		public abstract Type PlayMakerFSMType { get; }

		public abstract Type FSMType { get; }

		public abstract IEnumerable<string> GetAllFsmsOnObject(GameObject gameObject);

		public abstract UnityEngine.Object GetFsmObject(GameObject obj, string fsmName, string varName);

		public abstract void SetFsmObject(GameObject obj, string fsmName, string varName, UnityEngine.Object value);

		public abstract Material GetFsmMaterial(GameObject obj, string fsmName, string varName);

		public abstract void SetFsmMaterial(GameObject obj, string fsmName, string varName, Material value);

		public abstract Texture GetFsmTexture(GameObject obj, string fsmName, string varName);

		public abstract void SetFsmTexture(GameObject obj, string fsmName, string varName, Texture value);

		public abstract float GetFsmFloat(GameObject obj, string fsmName, string varName);

		public abstract void SetFsmFloat(GameObject obj, string fsmName, string varName, float value);

		public abstract int GetFsmInt(GameObject obj, string fsmName, string varName);

		public abstract void SetFsmInt(GameObject obj, string fsmName, string varName, int value);

		public abstract bool GetFsmBool(GameObject obj, string fsmName, string varName);

		public abstract void SetFsmBool(GameObject obj, string fsmName, string varName, bool value);

		public abstract string GetFsmString(GameObject obj, string fsmName, string varName);

		public abstract void SetFsmString(GameObject obj, string fsmName, string varName, string value);

		public abstract Vector2 GetFsmVector2(GameObject obj, string fsmName, string varName);

		public abstract void SetFsmVector2(GameObject obj, string fsmName, string varName, Vector2 value);

		public abstract Vector3 GetFsmVector3(GameObject obj, string fsmName, string varName);

		public abstract void SetFsmVector3(GameObject obj, string fsmName, string varName, Vector3 value);

		public abstract Rect GetFsmRect(GameObject obj, string fsmName, string varName);

		public abstract void SetFsmRect(GameObject obj, string fsmName, string varName, Rect value);

		public abstract Quaternion GetFsmQuaternion(GameObject obj, string fsmName, string varName);

		public abstract void SetFsmQuaternion(GameObject obj, string fsmName, string varName, Quaternion value);

		public abstract Color GetFsmColor(GameObject obj, string fsmName, string varName);

		public abstract void SetFsmColor(GameObject obj, string fsmName, string varName, Color value);

		public abstract GameObject GetFsmGameObject(GameObject obj, string fsmName, string varName);

		public abstract void SetFsmGameObject(GameObject obj, string fsmName, string varName, GameObject value);

		public abstract object[] GetFsmArray(GameObject obj, string fsmName, string varName);

		public abstract void SetFsmArray(GameObject obj, string fsmName, string varName, object[] value);

		public abstract Enum GetFsmEnum(GameObject obj, string fsmName, string varName);

		public abstract void SetFsmEnum(GameObject obj, string fsmName, string varName, Enum value);
	}
}
