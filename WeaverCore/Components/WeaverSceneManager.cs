using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WeaverCore.Components
{
	public class WeaverSceneManager : SceneManager
	{
		[Header("Scene Info")]
		[Tooltip("Automatically sets the scene width and height")]
		[SerializeField]
		bool autoSetDimensions = true;
		[SerializeField]
		Rect sceneDimensions;

		[NonSerialized]
		bool sceneDimensionsRefreshed = false;

		[Header("Scene Audio")]
		[Space]
		//TODO TODO TODO
		/*[SerializeField]
		private AtmosCue atmosCue;*/

		[SerializeField]
		[Tooltip("The music pack that is applied when the scene is loaded")]
		private MusicPack music;

		[SerializeField]
		[Tooltip("Whether the scene can be in an infected state")]
		bool canBeInfected = false;

		[SerializeField]
		[Tooltip("The music pack that is played when the scene is in it's infected state. Only used of \"Can Be Infected\" is enabled")]
		private MusicPack infectedMusic;


		public Rect SceneDimensions
		{
			get
			{
				if (!sceneDimensionsRefreshed)
				{
					RefreshSceneDimensions();
				}
				return sceneDimensions;
			}
		}

		void RefreshSceneDimensions()
		{
			sceneDimensionsRefreshed = true;
			if (autoSetDimensions)
			{
				sceneDimensions = new Rect(float.PositiveInfinity, float.PositiveInfinity, float.NegativeInfinity, float.NegativeInfinity);
				foreach (var rootObj in gameObject.scene.GetRootGameObjects())
				{
					foreach (var collider in rootObj.GetComponentsInChildren<Collider2D>())
					{
						var bounds = collider.bounds;
						if (bounds.min.x < sceneDimensions.x)
						{
							sceneDimensions.x = bounds.min.x;
						}

						if (bounds.min.y < sceneDimensions.y)
						{
							sceneDimensions.y = bounds.min.y;
						}

						if (bounds.size.x > sceneDimensions.width)
						{
							sceneDimensions.width = bounds.size.x;
						}

						if (bounds.size.y > sceneDimensions.height)
						{
							sceneDimensions.height = bounds.size.y;
						}
					}

				}

				if (float.IsInfinity(sceneDimensions.width))
				{
					sceneDimensions = default;
				}
			}
		}

		private void Awake()
		{
			RefreshSceneDimensions();
		}

#if UNITY_EDITOR
		public override void OnValidate()
		{
			base.OnValidate();
			if (!canBeInfected)
			{
				infectedMusic = null;
			}
		}
#endif
	}
}
