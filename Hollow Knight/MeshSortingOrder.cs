//using Language;
using UnityEngine;


public class MeshSortingOrder : MonoBehaviour
{
	public string layerName = "Over";

	public int order = 1;

	private MeshRenderer rend;

	private void Awake()
	{
		rend = GetComponent<MeshRenderer>();
		rend.sortingLayerName = layerName;
		rend.sortingOrder = order;
	}

	public void Update()
	{
		if (rend.sortingLayerName != layerName)
		{
			rend.sortingLayerName = layerName;
		}
		if (rend.sortingOrder != order)
		{
			rend.sortingOrder = order;
		}
	}

	public void OnValidate()
	{
		rend = GetComponent<MeshRenderer>();
		rend.sortingLayerName = layerName;
		rend.sortingOrder = order;
	}
}
