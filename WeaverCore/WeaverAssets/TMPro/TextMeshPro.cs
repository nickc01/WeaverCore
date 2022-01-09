using TMPro;
using UnityEngine;

namespace WeaverCore.Assets.TMPro
{

	/*
	 * THIS REPLACES THE DEFAULT TextMeshPro OBJECT TO MAKE IT WORK WITH ASSET BUNDLES
	 */

#if UNITY_EDITOR
	[System.ComponentModel.Browsable(false)]
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
#endif
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	[AddComponentMenu("Mesh/TextMeshPro - Text")]
	public class TextMeshPro : global::TMPro.TextMeshPro
	{
		[SerializeField]
		[HideInInspector]
		Color fontGradient_topLeft = Color.white;
		[SerializeField]
		[HideInInspector]
		Color fontGradient_topRight = Color.white;
		[SerializeField]
		[HideInInspector]
		Color fontGradient_bottomLeft = Color.white;
		[SerializeField]
		[HideInInspector]
		Color fontGradient_bottomRight = Color.white;


		protected override void Awake()
		{
			if (Application.isPlaying)
			{
				m_fontColorGradient = new VertexGradient(fontGradient_topLeft, fontGradient_topRight, fontGradient_bottomLeft, fontGradient_bottomRight);
			}
			base.Awake();
		}
#if UNITY_EDITOR
		protected override void OnValidate()
		{
			fontGradient_topLeft = m_fontColorGradient.topLeft;
			fontGradient_topRight = m_fontColorGradient.topRight;
			fontGradient_bottomLeft = m_fontColorGradient.bottomLeft;
			fontGradient_bottomRight = m_fontColorGradient.bottomRight;
			base.OnValidate();
		}
#endif
	}
}
