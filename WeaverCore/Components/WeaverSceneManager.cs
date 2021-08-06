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


#if UNITY_EDITOR
		private void OnValidate()
		{
			if (!canBeInfected)
			{
				infectedMusic = null;
			}
		}
#endif
	}
}
