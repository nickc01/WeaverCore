using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
    public class E_PlayMaker_I : PlayMaker_I
    {
        public override bool PlayMakerAvailable => false;

        public override Type PlayMakerFSMType => null;

        public override Type FSMType => null;

        public override IEnumerable<string> GetAllFsmsOnObject(GameObject gameObject)
        {
            yield break;
        }

        public override UnityEngine.Object GetFsmObject(GameObject obj, string fsmName, string varName)
        {
            return null;
        }

        public override void SetFsmObject(GameObject obj, string fsmName, string varName, UnityEngine.Object value)
        {
        }

        public override Material GetFsmMaterial(GameObject obj, string fsmName, string varName)
        {
            return null;
        }

        public override void SetFsmMaterial(GameObject obj, string fsmName, string varName, Material value)
        {
        }

        public override Texture GetFsmTexture(GameObject obj, string fsmName, string varName)
        {
            return null;
        }

        public override void SetFsmTexture(GameObject obj, string fsmName, string varName, Texture value)
        {
        }

        public override float GetFsmFloat(GameObject obj, string fsmName, string varName)
        {
            return 0f;
        }

        public override void SetFsmFloat(GameObject obj, string fsmName, string varName, float value)
        {
        }

        public override int GetFsmInt(GameObject obj, string fsmName, string varName)
        {
            return 0;
        }

        public override void SetFsmInt(GameObject obj, string fsmName, string varName, int value)
        {
        }

        public override bool GetFsmBool(GameObject obj, string fsmName, string varName)
        {
            return false;
        }

        public override void SetFsmBool(GameObject obj, string fsmName, string varName, bool value)
        {
        }

        public override string GetFsmString(GameObject obj, string fsmName, string varName)
        {
            return null;
        }

        public override void SetFsmString(GameObject obj, string fsmName, string varName, string value)
        {
        }

        public override Vector2 GetFsmVector2(GameObject obj, string fsmName, string varName)
        {
            return Vector2.zero;
        }

        public override void SetFsmVector2(GameObject obj, string fsmName, string varName, Vector2 value)
        {
        }

        public override Vector3 GetFsmVector3(GameObject obj, string fsmName, string varName)
        {
            return Vector3.zero;
        }

        public override void SetFsmVector3(GameObject obj, string fsmName, string varName, Vector3 value)
        {
        }

        public override Rect GetFsmRect(GameObject obj, string fsmName, string varName)
        {
            return Rect.zero;
        }

        public override void SetFsmRect(GameObject obj, string fsmName, string varName, Rect value)
        {
        }

        public override Quaternion GetFsmQuaternion(GameObject obj, string fsmName, string varName)
        {
            return Quaternion.identity;
        }

        public override void SetFsmQuaternion(GameObject obj, string fsmName, string varName, Quaternion value)
        {
        }

        public override Color GetFsmColor(GameObject obj, string fsmName, string varName)
        {
            return Color.white;
        }

        public override void SetFsmColor(GameObject obj, string fsmName, string varName, Color value)
        {
        }

        public override GameObject GetFsmGameObject(GameObject obj, string fsmName, string varName)
        {
            return null;
        }

        public override void SetFsmGameObject(GameObject obj, string fsmName, string varName, GameObject value)
        {
        }

        public override object[] GetFsmArray(GameObject obj, string fsmName, string varName)
        {
            return null;
        }

        public override void SetFsmArray(GameObject obj, string fsmName, string varName, object[] value)
        {
        }

        public override Enum GetFsmEnum(GameObject obj, string fsmName, string varName)
        {
            return null;
        }

        public override void SetFsmEnum(GameObject obj, string fsmName, string varName, Enum value)
        {
        }

        public override object CreateFSMActionFromWeaverAction(WeaverCore.Playmaker.WeaverFSMAction action)
        {
            // In editor mode, return null since PlayMaker is not available
            return null;
        }
    }
}