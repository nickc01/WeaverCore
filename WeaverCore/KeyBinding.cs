using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


/*
 * TODO TODO TODO TODO
 */
namespace Assets.WeaverCore.WeaverCore
{
	[ExecuteAlways]
	class KeyListener : MonoBehaviour
	{
		public List<KeyBinding> Bindings = new List<KeyBinding>();

		List<KeyCode> AllKeyCodes = new List<KeyCode>();

		List<KeyCode> ExcludedKeys = new List<KeyCode>
		{
			KeyCode.None,
			KeyCode.Backspace,
			KeyCode.Escape,
			KeyCode.LeftWindows,
			KeyCode.RightWindows
		};

		List<KeyCode> SpecialKeys = new List<KeyCode>
		{
			KeyCode.LeftCommand,
			KeyCode.RightCommand,
			KeyCode.LeftControl,
			KeyCode.RightControl,
			KeyCode.LeftShift,
			KeyCode.RightShift,
			KeyCode.LeftAlt,
			KeyCode.RightAlt
		};

		private void Awake()
		{
			
		}

		private void OnDisable()
		{
			
		}

		private void OnDestroy()
		{
			
		}
	}


	[Serializable]
	public class KeyBinding
	{
		static GameObject globalObjectContext;

		[SerializeField]
		[HideInInspector]
		List<KeyCode> keys = new List<KeyCode>();

		public bool AddKey(KeyCode key)
		{
			if (keys.Contains(key))
			{
				return false;
			}
			else
			{
				keys.Add(key);
				return true;
			}
		}

		public bool RemoveKey(KeyCode key)
		{
			return keys.Remove(key);
		}

		public void RemoveAllKeys()
		{
			keys.Clear();
		}

		public KeyBinding()
		{
			
		}

		public KeyBinding(string keys)
		{
			var splitKeys = keys.Split('+');
			foreach (var keyWhitespace in splitKeys)
			{
				var key = keyWhitespace.Trim();
				if (Enum.TryParse<KeyCode>(key, out var result))
				{
					AddKey(result);
				}
			}
		}

		public KeyBinding(IEnumerable<KeyCode> keys)
		{
			foreach (var key in keys)
			{
				AddKey(key);
			}
		}

		public override string ToString()
		{
			string result = "";
			for (int i = 0; i < keys.Count; i++)
			{
				result += keys[i].ToString();
				if (i + 1 < keys.Count)
				{
					result += " + ";
				}
			}
			return result;
		}

		public void StopListening()
		{

		}

		public void ListenForKeyPresses()
		{
			ListenForKeyPresses(null);
		}

		public void ListenForKeyPresses(GameObject contextObject)
		{
			if (contextObject == null)
			{
				if (globalObjectContext == null)
				{
					globalObjectContext = new GameObject("GLOBAL INPUT CONTEXT");
					globalObjectContext.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
				}
				contextObject = globalObjectContext;
			}

			if (!contextObject.activeInHierarchy)
			{
				return;
			}

			var listener = contextObject.GetComponent<KeyListener>();
			if (listener == null)
			{
				listener = contextObject.AddComponent<KeyListener>();
			}
		}
	}
}
