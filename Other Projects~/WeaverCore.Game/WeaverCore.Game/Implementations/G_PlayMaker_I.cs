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

		public override object CreateFSMActionFromWeaverAction(WeaverCore.Playmaker.WeaverFSMAction action)
		{
			if (action == null)
			{
				return null;
			}

			// Create a new WeaverCoreFSM_Impl that wraps around the WeaverFSMAction
			var weaverCoreFsmImpl = new WeaverCoreFSM_Impl(action);
			
			return weaverCoreFsmImpl;
		}
	}

	/// <summary>
	/// An implementation of FsmStateAction that wraps around a WeaverFSMAction
	/// </summary>
	public class WeaverCoreFSM_Impl : FsmStateAction
	{
		private WeaverCore.Playmaker.WeaverFSMAction _action;

		public WeaverCoreFSM_Impl(WeaverCore.Playmaker.WeaverFSMAction action)
		{
			_action = action;
			_action.FSMActionBase = this;
		}

		public override void Reset()
		{
			_action.Reset();
		}

		public override void OnPreprocess()
		{
			_action.OnPreprocess();
		}

		public override void Awake()
		{
			_action.Awake();
		}

		public override bool Event(FsmEvent fsmEvent)
		{
			return _action.Event(fsmEvent);
		}

		public override void OnEnter()
		{
			_action.OnEnter();
		}

		public override void OnFixedUpdate()
		{
			_action.OnFixedUpdate();
		}

		public override void OnUpdate()
		{
			_action.OnUpdate();
		}

		public override void OnGUI()
		{
			_action.OnGUI();
		}

		public override void OnLateUpdate()
		{
			_action.OnLateUpdate();
		}

		public override void OnExit()
		{
			_action.OnExit();
		}

		public override void DoCollisionEnter(Collision collisionInfo)
		{
			_action.DoCollisionEnter(collisionInfo);
		}

		public override void DoCollisionStay(Collision collisionInfo)
		{
			_action.DoCollisionStay(collisionInfo);
		}

		public override void DoCollisionExit(Collision collisionInfo)
		{
			_action.DoCollisionExit(collisionInfo);
		}

		public override void DoTriggerEnter(Collider other)
		{
			_action.DoTriggerEnter(other);
		}

		public override void DoTriggerStay(Collider other)
		{
			_action.DoTriggerStay(other);
		}

		public override void DoTriggerExit(Collider other)
		{
			_action.DoTriggerExit(other);
		}

		public override void DoParticleCollision(GameObject other)
		{
			_action.DoParticleCollision(other);
		}

		public override void DoCollisionEnter2D(Collision2D collisionInfo)
		{
			_action.DoCollisionEnter2D(collisionInfo);
		}

		public override void DoCollisionStay2D(Collision2D collisionInfo)
		{
			_action.DoCollisionStay2D(collisionInfo);
		}

		public override void DoCollisionExit2D(Collision2D collisionInfo)
		{
			_action.DoCollisionExit2D(collisionInfo);
		}

		public override void DoTriggerEnter2D(Collider2D other)
		{
			_action.DoTriggerEnter2D(other);
		}

		public override void DoTriggerStay2D(Collider2D other)
		{
			_action.DoTriggerStay2D(other);
		}

		public override void DoTriggerExit2D(Collider2D other)
		{
			_action.DoTriggerExit2D(other);
		}

		public override void DoControllerColliderHit(ControllerColliderHit collider)
		{
			_action.DoControllerColliderHit(collider);
		}

		public override void DoJointBreak(float force)
		{
			_action.DoJointBreak(force);
		}

		public override void DoJointBreak2D(Joint2D joint)
		{
			_action.DoJointBreak2D(joint);
		}

		public override void DoAnimatorMove()
		{
			_action.DoAnimatorMove();
		}

		public override void DoAnimatorIK(int layerIndex)
		{
			_action.DoAnimatorIK(layerIndex);
		}
	}
}