using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
	class GMSorter : IComparer<GameObject>
	{
		public int Compare(GameObject x, GameObject y)
		{
			return Comparer<string>.Default.Compare(x.name, y.name);
		}
	}



	public class DumpUI : MonoBehaviour
	{
		[SerializeField]
		Button DumpButton;

		[SerializeField]
		TextMeshProUGUI gameObjectField;

		[SerializeField]
		GameObjectSelectButton firstElement;

		[SerializeField]
		GameObject contentHolder;

		[SerializeField]
		Text filterText;

		static ObjectDumper_I impl;

		public static DumpUI Instance { get; private set; }

		public GameObject SelectedObject { get; private set; }

		void Awake()
		{
			Instance = this;
			UnboundCoroutine.Start(KeyChecker());
			gameObject.SetActive(false);
		}

		IEnumerator KeyChecker()
		{
			while (true)
			{
				if (Input.GetKey(KeyCode.F10))
				{
					gameObject.SetActive(!gameObject.activeSelf);
					if (gameObject.activeSelf)
					{
						Refresh();
					}
					yield return new WaitUntil(() => !Input.GetKey(KeyCode.F10));
				}
				yield return null;

				if (!Application.isPlaying)
				{
					yield break;
				}
			}
		}

		public void Filter()
		{
			var filter = filterText.text;
			if (filter == null || filter == "")
			{
				if (contentHolder.transform.childCount == 1)
				{
					var child = contentHolder.transform.GetChild(0).GetComponent<GameObjectSelectButton>();
					if (child.GetSourceGM() != null)
					{
						child.gameObject.SetActive(true);
					}
				}
				else
				{
					for (int i = contentHolder.transform.childCount - 1; i >= 0; i--)
					{
						var child = contentHolder.transform.GetChild(i).GetComponent<GameObjectSelectButton>();
						if (child.GetSourceGM() != null)
						{
							child.gameObject.SetActive(true);
						}
					}
				}
			}
			else
			{
				if (filter.StartsWith("-t:"))
				{
					filter = filter.Replace("-t:", "");
					foreach (var child in GetChildren(contentHolder))
					{
						var selector = child.GetComponent<GameObjectSelectButton>();
						if (selector.GetSourceGM() == null)
						{
							continue;
						}
						foreach (var component in selector.GetSourceGM().GetComponents<Component>())
						{
							if (component != null && component.GetType().FullName.Contains(filter))
							{
								child.SetActive(true);
								goto NextChild;
							}
						}
						child.SetActive(false);
					NextChild:
						continue;
					}
					/*for (int i = contentHolder.transform.childCount - 1; i >= 0; i--)
					{
						var child = contentHolder.transform.GetChild(i).GetComponent<GameObjectSelectButton>();

					}*/
				}
				else
				{
					foreach (var child in GetChildren(contentHolder))
					{
						var selector = child.GetComponent<GameObjectSelectButton>();
						var sourceGM = selector.GetSourceGM();
						if (sourceGM != null && sourceGM.name.Contains(filter))
						{
							child.SetActive(true);
						}
						else
						{
							child.SetActive(false);
						}
					}
				}
			}
		}

		public static IEnumerable<GameObject> GetChildren(GameObject gm)
		{
			for (int i = gm.transform.childCount - 1; i >= 0; i--)
			{
				yield return gm.transform.GetChild(i).gameObject;
			}
		}

		public void Refresh()
		{
			for (int i = contentHolder.transform.childCount - 1; i >= 0; i--)
			{
				var child = contentHolder.transform.GetChild(i);
				if (child != firstElement.transform)
				{
					Destroy(child.gameObject);
				}
			}

			firstElement.gameObject.SetActive(false);
			firstElement.SetSourceGM(null, "");

			var objects = GameObject.FindObjectsOfType<GameObject>().ToList();

			objects.Sort(new GMSorter());

			foreach (var obj in objects)
			{
				if (obj.transform.parent == null && obj != gameObject.transform.parent.parent.gameObject)
				{
					AddObject(obj, obj.name);
					AddChildren(obj);
				}
			}
		}

		void AddChildren(GameObject gameObject, int recursionNumber = 1)
		{
			for (int i = 0; i < gameObject.transform.childCount; i++)
			{
				var child = gameObject.transform.GetChild(i);
				AddObject(child.gameObject, new string('-', recursionNumber) + child.name);
				AddChildren(child.gameObject);
			}
		}

		void AddObject(GameObject obj,string name = null)
		{
			if (!firstElement.gameObject.activeSelf)
			{
				firstElement.gameObject.SetActive(true);
				firstElement.SetSourceGM(obj, name);
			}
			else
			{
				var instance = GameObject.Instantiate(firstElement, contentHolder.transform);
				instance.SetSourceGM(obj, name);
			}
		}

		public void SetGameObject(GameObject newObj)
		{
			SelectedObject = newObj;
			if (SelectedObject == null)
			{
				gameObjectField.text = "";
			}
			else
			{
				gameObjectField.text = newObj.name;
			}
		}

		public void DumpObject()
		{
			if (impl == null)
			{
				impl = ImplFinder.GetImplementation<ObjectDumper_I>();
			}
			if (SelectedObject != null)
			{
				impl.Dump(SelectedObject);
			}
		}
	}
}
