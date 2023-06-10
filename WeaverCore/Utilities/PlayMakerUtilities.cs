using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Implementations;

namespace WeaverCore.Utilities
{
	/// <summary>
	/// Contains some utility functions related to PlayMaker
	/// 
	/// These only work in-game
	/// </summary>
	public static class PlayMakerUtilities
	{
		/// <summary>
		/// Is playmaker avaiable? (This is only true if playing in-game)
		/// </summary>
		public static bool PlayMakerAvailable
		{
			get
			{
				return PlayMakerUtilities.impl.PlayMakerAvailable;
			}
		}

		/// <summary>
		/// The typeof(PlayMakerFSM). Will be null if <see cref="PlayMakerAvailable"/> is false
		/// </summary>
		public static Type PlayMakerFSMType
		{
			get
			{
				return PlayMakerUtilities.impl.PlayMakerFSMType;
			}
		}

		/// <summary>
		/// The typeof(Fsm). Will be null if <see cref="PlayMakerAvailable"/> is false
		/// </summary>
		public static Type FSMType
		{
			get
			{
				return PlayMakerUtilities.impl.FSMType;
			}
		}

		/// <summary>
		/// Gets the names of all PlayMakerFSM objects on a GameObject
		/// </summary>
		/// <param name="gameObject">The gameobject to check</param>
		/// <returns>Returns the names of all PlayMakerFSM objects on the GameObject</returns>
		public static IEnumerable<string> GetAllFsmsOnObject(GameObject gameObject)
		{
			return PlayMakerUtilities.impl.GetAllFsmsOnObject(gameObject);
		}

		public static MonoBehaviour GetPlaymakerFSMOnObject(GameObject gameObject, string name)
		{
			if (PlayMakerAvailable)
			{
				var playMakerFSMs = gameObject.GetComponents(PlayMakerFSMType);

				foreach (var pmFSM in playMakerFSMs)
				{
					if ((string)pmFSM.ReflectGetProperty("Fsm").ReflectGetProperty("Name") == name)
					{
						return (MonoBehaviour)pmFSM;
					}
				}
			}
            return null;
        }

		public static object GetFSMOnPlayMakerComponent(Component playmakerComponent)
		{
			try
			{
				if (playmakerComponent == null)
				{
					throw new ArgumentNullException(nameof(playmakerComponent));
				}

				return playmakerComponent.ReflectGetProperty("Fsm");

            }
			catch (Exception)
			{
				throw new Exception("The component is not a PlayMakerFSM");
			}
		}

		public static IEnumerable GetStatesOnFSM(object fsm)
		{
            if (fsm == null)
            {
                throw new ArgumentNullException(nameof(fsm));
            }

			try
			{
				return (IEnumerable)fsm.ReflectGetProperty("States");
			}
			catch (Exception)
			{

                throw new Exception("The component is not an Fsm");
            }
        }

		public static string GetStateName(object stateObject)
		{
            if (stateObject == null)
            {
                throw new ArgumentNullException(nameof(stateObject));
            }

            try
            {
                return (string)stateObject.ReflectGetProperty("Name");
            }
            catch (Exception)
            {

                throw new Exception("The component is not an FSMEvent");
            }
        }

		public static object FindStateOnFSM(object fsm, string name)
		{
			foreach (var state in GetStatesOnFSM(fsm))
			{
				if (GetStateName(state) == name)
				{
					return state;
				}
			}
			return null;
        }

		public static IEnumerable GetStateActions(object stateObject)
		{
            if (stateObject == null)
            {
                throw new ArgumentNullException(nameof(stateObject));
            }

            try
            {
                return (IEnumerable)stateObject.ReflectGetProperty("Actions");
            }
            catch (Exception)
            {

                throw new Exception("The component is not an FSMEvent");
            }
        }

        public static object GetActionData(object stateObject)
        {
            if (stateObject == null)
            {
                throw new ArgumentNullException(nameof(stateObject));
            }

            try
            {
                return stateObject.ReflectGetProperty("ActionData");
            }
            catch (Exception)
            {

                throw new Exception("The component is not an FSMEvent");
            }
        }

        public static string GetActionName(object actionObject)
		{
            if (actionObject == null)
            {
                throw new ArgumentNullException(nameof(actionObject));
            }

            try
            {
                return (string)actionObject.ReflectGetProperty("Name");
            }
            catch (Exception)
            {

                throw new Exception("The component is not an FSMStateAction");
            }
        }

		/// <summary>
		/// Gets the Object value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Object variable</param>
		/// <returns>Returns the Object value of the variable</returns>
		public static UnityEngine.Object GetFsmObject(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmObject(obj, fsmName, varName);
		}

		/// <summary>
		/// Sets the Object value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Object variable</param>
		/// <param name="value">The value to set in the Object variable</param>
		public static void SetFsmObject(GameObject obj, string fsmName, string varName, UnityEngine.Object value)
		{
			PlayMakerUtilities.impl.SetFsmObject(obj, fsmName, varName, value);
		}

		/// <summary>
		/// Gets the Material value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Material variable</param>
		/// <returns>Returns the Material value of the variable</returns>
		public static Material GetFsmMaterial(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmMaterial(obj, fsmName, varName);
		}

		/// <summary>
		/// Sets the Material value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Material variable</param>
		/// <param name="value">The value to set in the Material variable</param>
		public static void SetFsmMaterial(GameObject obj, string fsmName, string varName, Material value)
		{
			PlayMakerUtilities.impl.SetFsmMaterial(obj, fsmName, varName, value);
		}

		/// <summary>
		/// Gets the Texture value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Texture variable</param>
		/// <returns>Returns the Texture value of the variable</returns>
		public static Texture GetFsmTexture(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmTexture(obj, fsmName, varName);
		}

		/// <summary>
		/// Sets the Texture value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Texture variable</param>
		/// <param name="value">The value to set in the Texture variable</param>
		public static void SetFsmTexture(GameObject obj, string fsmName, string varName, Texture value)
		{
			PlayMakerUtilities.impl.SetFsmTexture(obj, fsmName, varName, value);
		}

		/// <summary>
		/// Gets the Float value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Float variable</param>
		/// <returns>Returns the Float value of the variable</returns>
		public static float GetFsmFloat(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmFloat(obj, fsmName, varName);
		}

		/// <summary>
		/// Sets the Float value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Float variable</param>
		/// <param name="value">The value to set in the Float variable</param>
		public static void SetFsmFloat(GameObject obj, string fsmName, string varName, float value)
		{
			PlayMakerUtilities.impl.SetFsmFloat(obj, fsmName, varName, value);
		}

		/// <summary>
		/// Gets the Int value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Int variable</param>
		/// <returns>Returns the Int value of the variable</returns>
		public static int GetFsmInt(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmInt(obj, fsmName, varName);
		}

		/// <summary>
		/// Sets the Int value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Int variable</param>
		/// <param name="value">The value to set in the Int variable</param>
		public static void SetFsmInt(GameObject obj, string fsmName, string varName, int value)
		{
			PlayMakerUtilities.impl.SetFsmInt(obj, fsmName, varName, value);
		}

		/// <summary>
		/// Gets the Bool value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Bool variable</param>
		/// <returns>Returns the Bool value of the variable</returns>
		public static bool GetFsmBool(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmBool(obj, fsmName, varName);
		}

		/// <summary>
		/// Sets the Bool value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Bool variable</param>
		/// <param name="value">The value to set in the Bool variable</param>
		public static void SetFsmBool(GameObject obj, string fsmName, string varName, bool value)
		{
			PlayMakerUtilities.impl.SetFsmBool(obj, fsmName, varName, value);
		}

		/// <summary>
		/// Gets the String value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the String variable</param>
		/// <returns>Returns the String value of the variable</returns>
		public static string GetFsmString(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmString(obj, fsmName, varName);
		}

		/// <summary>
		/// Sets the String value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the String variable</param>
		/// <param name="value">The value to set in the String variable</param>
		public static void SetFsmString(GameObject obj, string fsmName, string varName, string value)
		{
			PlayMakerUtilities.impl.SetFsmString(obj, fsmName, varName, value);
		}

		/// <summary>
		/// Gets the Vector2 value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Vector2 variable</param>
		/// <returns>Returns the Vector2 value of the variable</returns>
		public static Vector2 GetFsmVector2(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmVector2(obj, fsmName, varName);
		}

		/// <summary>
		/// Sets the Vector2 value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Vector2 variable</param>
		/// <param name="value">The value to set in the Vector2 variable</param>
		public static void SetFsmVector2(GameObject obj, string fsmName, string varName, Vector2 value)
		{
			PlayMakerUtilities.impl.SetFsmVector2(obj, fsmName, varName, value);
		}

		/// <summary>
		/// Gets the Vector3 value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Vector3 variable</param>
		/// <returns>Returns the Vector3 value of the variable</returns>
		public static Vector3 GetFsmVector3(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmVector3(obj, fsmName, varName);
		}

		/// <summary>
		/// Sets the Vector3 value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Vector3 variable</param>
		/// <param name="value">The value to set in the Vector3 variable</param>
		public static void SetFsmVector3(GameObject obj, string fsmName, string varName, Vector3 value)
		{
			PlayMakerUtilities.impl.SetFsmVector3(obj, fsmName, varName, value);
		}

		/// <summary>
		/// Gets the Rect value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Rect variable</param>
		/// <returns>Returns the Rect value of the variable</returns>
		public static Rect GetFsmRect(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmRect(obj, fsmName, varName);
		}

		/// <summary>
		/// Sets the Rect value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Rect variable</param>
		/// <param name="value">The value to set in the Rect variable</param>
		public static void SetFsmRect(GameObject obj, string fsmName, string varName, Rect value)
		{
			PlayMakerUtilities.impl.SetFsmRect(obj, fsmName, varName, value);
		}

		/// <summary>
		/// Gets the Quaternion value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Quaternion variable</param>
		/// <returns>Returns the Quaternion value of the variable</returns>
		public static Quaternion GetFsmQuaternion(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmQuaternion(obj, fsmName, varName);
		}

		/// <summary>
		/// Sets the Quaternion value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Quaternion variable</param>
		/// <param name="value">The value to set in the Quaternion variable</param>
		public static void SetFsmQuaternion(GameObject obj, string fsmName, string varName, Quaternion value)
		{
			PlayMakerUtilities.impl.SetFsmQuaternion(obj, fsmName, varName, value);
		}

		/// <summary>
		/// Gets the Color value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Color variable</param>
		/// <returns>Returns the Color value of the variable</returns>
		public static Color GetFsmColor(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmColor(obj, fsmName, varName);
		}

		/// <summary>
		/// Sets the Color value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Color variable</param>
		/// <param name="value">The value to set in the Color variable</param>
		public static void SetFsmColor(GameObject obj, string fsmName, string varName, Color value)
		{
			PlayMakerUtilities.impl.SetFsmColor(obj, fsmName, varName, value);
		}

		/// <summary>
		/// Gets the GameObject value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the GameObject variable</param>
		/// <returns>Returns the GameObject value of the variable</returns>
		public static GameObject GetFsmGameObject(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmGameObject(obj, fsmName, varName);
		}

		/// <summary>
		/// Sets the GameObject value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the GameObject variable</param>
		/// <param name="value">The value to set in the GameObject variable</param>
		public static void SetFsmGameObject(GameObject obj, string fsmName, string varName, GameObject value)
		{
			PlayMakerUtilities.impl.SetFsmGameObject(obj, fsmName, varName, value);
		}

		/// <summary>
		/// Gets the Array value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Array variable</param>
		/// <returns>Returns the Array value of the variable</returns>
		public static object[] GetFsmArray(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmArray(obj, fsmName, varName);
		}

		/// <summary>
		/// Sets the Array value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Array variable</param>
		/// <param name="value">The value to set in the Array variable</param>
		public static void SetFsmArray(GameObject obj, string fsmName, string varName, object[] value)
		{
			PlayMakerUtilities.impl.SetFsmArray(obj, fsmName, varName, value);
		}

		/// <summary>
		/// Gets the Enum value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to get</param>
		/// <param name="varName">The name of the Enum variable</param>
		/// <returns>Returns the Enum value of the variable</returns>
		public static Enum GetFsmEnum(GameObject obj, string fsmName, string varName)
		{
			return PlayMakerUtilities.impl.GetFsmEnum(obj, fsmName, varName);
		}

		/// <summary>
		/// Sets the Enum value in a PlayMakerFSM
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <param name="fsmName">The name of the PlayMakerFSM to set</param>
		/// <param name="varName">The name of the Enum variable</param>
		/// <param name="value">The value to set in the Enum variable</param>
		public static void SetFsmEnum(GameObject obj, string fsmName, string varName, Enum value)
		{
			PlayMakerUtilities.impl.SetFsmEnum(obj, fsmName, varName, value);
		}

		private static PlayMaker_I impl = ImplFinder.GetImplementation<PlayMaker_I>();
	}
}
