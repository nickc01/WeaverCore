using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class BlurPlane : MonoBehaviour
{
	public delegate void BlurPlanesChangedDelegate();

	private MeshRenderer meshRenderer;

	private Material originalMaterial;

	private static List<BlurPlane> blurPlanes = new List<BlurPlane>();

	public static int BlurPlaneCount => blurPlanes.Count;

	public static BlurPlane ClosestBlurPlane
	{
		get
		{
			if (blurPlanes.Count <= 0)
			{
				return null;
			}
			return blurPlanes[0];
		}
	}

	public float PlaneZ => base.transform.position.z;

	public static event BlurPlanesChangedDelegate BlurPlanesChanged;

	public static BlurPlane GetBlurPlane(int index)
	{
		return blurPlanes[index];
	}

	protected void Awake()
	{
		meshRenderer = GetComponent<MeshRenderer>();
		originalMaterial = meshRenderer.sharedMaterial;
	}

	private void Reset()
	{
		transform.SetPositionZ(6.62f);
	}

	protected void OnEnable()
	{
		int i;
		for (i = 0; i < blurPlanes.Count; i++)
		{
			BlurPlane blurPlane = blurPlanes[i];
			if (blurPlane.PlaneZ > blurPlane.PlaneZ)
			{
				break;
			}
		}
		blurPlanes.Insert(i, this);
		if (BlurPlane.BlurPlanesChanged != null)
		{
			BlurPlane.BlurPlanesChanged();
		}
	}

	protected void OnDisable()
	{
		blurPlanes.Remove(this);
		if (BlurPlane.BlurPlanesChanged != null)
		{
			BlurPlane.BlurPlanesChanged();
		}
	}

	public void SetPlaneVisibility(bool isVisible)
	{
		meshRenderer.enabled = isVisible;
	}

	public void SetPlaneMaterial(Material material)
	{
		if (material == null)
		{
			meshRenderer.sharedMaterial = originalMaterial;
		}
		else
		{
			meshRenderer.sharedMaterial = material;
		}
	}
}
