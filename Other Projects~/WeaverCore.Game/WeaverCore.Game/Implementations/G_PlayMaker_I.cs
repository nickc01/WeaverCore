using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
	public class G_PlayMaker_I : PlayMaker_I
	{
		public override bool PlayMakerAvailable
		{
			get
			{
				return true;
			}
		}

		public override Type PlayMakerFSMType
		{
			get
			{
				return typeof(PlayMakerFSM);
			}
		}

		public override Type FSMType
		{
			get
			{
				return typeof(Fsm);
			}
		}

		public override IEnumerable<string> GetAllFsmsOnObject(GameObject gameObject)
		{
			PlayMakerFSM[] fsms = gameObject.GetComponents<PlayMakerFSM>();
			foreach (PlayMakerFSM fsm in fsms)
			{
				yield return fsm.FsmName;
			}
		}

		private static PlayMakerFSM GetFsm(GameObject obj, string fsmName)
		{
			return ActionHelpers.GetGameObjectFsm(obj, fsmName);
		}

		public override object[] GetFsmArray(GameObject obj, string fsmName, string varName)
		{
			return GetFsm(obj, fsmName).FsmVariables.GetFsmArray(varName).Values;
		}

		public override bool GetFsmBool(GameObject obj, string fsmName, string varName)
		{
			return GetFsm(obj, fsmName).FsmVariables.GetFsmBool(varName).Value;
		}

		public override Color GetFsmColor(GameObject obj, string fsmName, string varName)
		{
			return GetFsm(obj, fsmName).FsmVariables.GetFsmColor(varName).Value;
		}

		public override Enum GetFsmEnum(GameObject obj, string fsmName, string varName)
		{
			return GetFsm(obj, fsmName).FsmVariables.GetFsmEnum(varName).Value;
		}

		public override float GetFsmFloat(GameObject obj, string fsmName, string varName)
		{
			return GetFsm(obj, fsmName).FsmVariables.GetFsmFloat(varName).Value;
		}

		public override GameObject GetFsmGameObject(GameObject obj, string fsmName, string varName)
		{
			return GetFsm(obj, fsmName).FsmVariables.GetFsmGameObject(varName).Value;
		}

		public override int GetFsmInt(GameObject obj, string fsmName, string varName)
		{
			return GetFsm(obj, fsmName).FsmVariables.GetFsmInt(varName).Value;
		}

		public override Material GetFsmMaterial(GameObject obj, string fsmName, string varName)
		{
			return GetFsm(obj, fsmName).FsmVariables.GetFsmMaterial(varName).Value;
		}

		public override UnityEngine.Object GetFsmObject(GameObject obj, string fsmName, string varName)
		{
			return GetFsm(obj, fsmName).FsmVariables.GetFsmObject(varName).Value;
		}

		public override Quaternion GetFsmQuaternion(GameObject obj, string fsmName, string varName)
		{
			return GetFsm(obj, fsmName).FsmVariables.GetFsmQuaternion(varName).Value;
		}

		public override Rect GetFsmRect(GameObject obj, string fsmName, string varName)
		{
			return GetFsm(obj, fsmName).FsmVariables.GetFsmRect(varName).Value;
		}

		public override string GetFsmString(GameObject obj, string fsmName, string varName)
		{
			return GetFsm(obj, fsmName).FsmVariables.GetFsmString(varName).Value;
		}

		public override Texture GetFsmTexture(GameObject obj, string fsmName, string varName)
		{
			return GetFsm(obj, fsmName).FsmVariables.GetFsmTexture(varName).Value;
		}

		public override Vector2 GetFsmVector2(GameObject obj, string fsmName, string varName)
		{
			return GetFsm(obj, fsmName).FsmVariables.GetFsmVector2(varName).Value;
		}

		public override Vector3 GetFsmVector3(GameObject obj, string fsmName, string varName)
		{
			return GetFsm(obj, fsmName).FsmVariables.GetFsmVector3(varName).Value;
		}

		public override void SetFsmArray(GameObject obj, string fsmName, string varName, object[] value)
		{
			GetFsm(obj, fsmName).FsmVariables.GetFsmArray(varName).Values = value;
		}

		public override void SetFsmBool(GameObject obj, string fsmName, string varName, bool value)
		{
			GetFsm(obj, fsmName).FsmVariables.GetFsmBool(varName).Value = value;
		}

		public override void SetFsmColor(GameObject obj, string fsmName, string varName, Color value)
		{
			GetFsm(obj, fsmName).FsmVariables.GetFsmColor(varName).Value = value;
		}

		public override void SetFsmEnum(GameObject obj, string fsmName, string varName, Enum value)
		{
			GetFsm(obj, fsmName).FsmVariables.GetFsmEnum(varName).Value = value;
		}

		public override void SetFsmFloat(GameObject obj, string fsmName, string varName, float value)
		{
			GetFsm(obj, fsmName).FsmVariables.GetFsmFloat(varName).Value = value;
		}

		public override void SetFsmGameObject(GameObject obj, string fsmName, string varName, GameObject value)
		{
			GetFsm(obj, fsmName).FsmVariables.GetFsmGameObject(varName).Value = value;
		}

		public override void SetFsmInt(GameObject obj, string fsmName, string varName, int value)
		{
			GetFsm(obj, fsmName).FsmVariables.GetFsmInt(varName).Value = value;
		}

		public override void SetFsmMaterial(GameObject obj, string fsmName, string varName, Material value)
		{
			GetFsm(obj, fsmName).FsmVariables.GetFsmMaterial(varName).Value = value;
		}

		public override void SetFsmObject(GameObject obj, string fsmName, string varName, UnityEngine.Object value)
		{
			GetFsm(obj, fsmName).FsmVariables.GetFsmObject(varName).Value = value;
		}

		public override void SetFsmQuaternion(GameObject obj, string fsmName, string varName, Quaternion value)
		{
			GetFsm(obj, fsmName).FsmVariables.GetFsmQuaternion(varName).Value = value;
		}

		public override void SetFsmRect(GameObject obj, string fsmName, string varName, Rect value)
		{
			GetFsm(obj, fsmName).FsmVariables.GetFsmRect(varName).Value = value;
		}

		public override void SetFsmString(GameObject obj, string fsmName, string varName, string value)
		{
			GetFsm(obj, fsmName).FsmVariables.GetFsmString(varName).Value = value;
		}

		public override void SetFsmTexture(GameObject obj, string fsmName, string varName, Texture value)
		{
			GetFsm(obj, fsmName).FsmVariables.GetFsmTexture(varName).Value = value;
		}

		public override void SetFsmVector2(GameObject obj, string fsmName, string varName, Vector2 value)
		{
			GetFsm(obj, fsmName).FsmVariables.GetFsmVector2(varName).Value = value;
		}

		public override void SetFsmVector3(GameObject obj, string fsmName, string varName, Vector3 value)
		{
			GetFsm(obj, fsmName).FsmVariables.GetFsmVector3(varName).Value = value;
		}
	}
}
