using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{

    /// <summary>
    /// Used for playing music and atmosphere sounds in the editor
    /// </summary>
    public class EditorMusic : MonoBehaviour
	{
		[Header("Music")]
		[SerializeField]
		AudioSource Action;
		[SerializeField]
		AudioSource Extra;
		[SerializeField]
		AudioSource Main;
		[SerializeField]
		AudioSource MainAlt;
		[SerializeField]
		AudioSource Sub;
		[SerializeField]
		AudioSource Tension;

		[Space]
		[Header("Atmos")]
		[SerializeField]
		List<AudioSource> atmosSources;

		static EditorMusic _instance;

		Coroutine ApplyMusicRoutine;
		Coroutine ApplySnapshotRoutine;
		Coroutine ApplyAtmosRoutine;

		IEnumerable<AudioSource> Sources
		{
			get
			{
				yield return Main;
				yield return MainAlt;
				yield return Action;
				yield return Sub;
				yield return Tension;
				yield return Extra;
			}
		}


		public static EditorMusic Instance
		{
			get
			{
				if (_instance == null)
				{
					var prefabGUIDs = AssetDatabase.FindAssets("Editor Music");

					foreach (var guid in prefabGUIDs)
					{
						var path = AssetDatabase.GUIDToAssetPath(guid);
						if (path.Contains("WeaverCore.Editor\\") || path.Contains("WeaverCore.Editor/"))
						{
							var obj = GameObject.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path), default(Vector3), Quaternion.identity);
							DontDestroyOnLoad(obj);
							_instance = obj.GetComponent<EditorMusic>();
							break;
						}
					}
				}
				return _instance;
			}
		}

		[OnRuntimeInit(int.MaxValue)]
		static void OnGameStart()
		{
			UnboundCoroutine.Start(WaitAFrame());
			IEnumerator WaitAFrame()
			{
				yield return null;
				Instance.ApplyAtmosPack(Atmos.SnapshotType.atNone, 0f, Atmos.GetEnabledSourcesForSnapshot(Atmos.SnapshotType.atNone));
			}
		}

		public void PlayMusicPack(MusicPack pack)
		{
			PlayMusicPack(pack, pack.delay, pack.snapshotTransitionTime, pack.applySnapshot);
		}

		public void PlayMusicPack(MusicPack pack, float delayTime, float snapshotTransitionTime, bool applySnapshot = true)
		{
			if (ApplyMusicRoutine != null)
			{
				StopCoroutine(ApplyMusicRoutine);
				ApplyMusicRoutine = null;
			}

			ApplyMusicRoutine = StartCoroutine(ApplyMusicPack(pack,delayTime,snapshotTransitionTime,applySnapshot));
		}

		public void ApplyMusicSnapshot(Music.SnapshotType snapshot, float delayTime, float transitionTime)
		{
			ApplyMusicSnapshot(Music.GetSnapshot(snapshot), delayTime, transitionTime);
		}

		public void ApplyMusicSnapshot(AudioMixerSnapshot snapshot, float delayTime, float transitionTime)
		{
			if (ApplySnapshotRoutine != null)
			{
				StopCoroutine(ApplySnapshotRoutine);
				ApplySnapshotRoutine = null;
			}

			ApplySnapshotRoutine = StartCoroutine(ApplyMusicSnapshotRoutine(snapshot,delayTime,transitionTime));
		}

		IEnumerator ApplyMusicSnapshotRoutine(AudioMixerSnapshot snapshot, float delayTime, float transitionTime)
		{
			yield return new WaitForSeconds(delayTime);
			if (snapshot != null)
			{
				snapshot.TransitionTo(transitionTime);
			}
			ApplySnapshotRoutine = null;
		}

		IEnumerator ApplyMusicPack(MusicPack pack, float delayTime, float snapshotTransitionTime, bool applySnapshot = true)
		{
			yield return new WaitForSeconds(delayTime);
			foreach (var source in Sources)
			{
				source.Stop();
				source.clip = null;
			}
			ApplyClipToSource(Action, pack.ActionTrack);
			ApplyClipToSource(Extra, pack.ExtraTrack);
			ApplyClipToSource(Main, pack.MainTrack);
			ApplyClipToSource(MainAlt, pack.MainAltTrack);
			ApplyClipToSource(Sub, pack.SubTrack);
			ApplyClipToSource(Tension, pack.TensionTrack);

			if (applySnapshot)
			{
				var snapshot = Music.GetSnapshot(pack.Snapshot);
				snapshot.TransitionTo(snapshotTransitionTime);
			}
			ApplyMusicRoutine = null;
		}


		void ApplyClipToSource(AudioSource source, AudioClip clip)
		{
			if (clip != null)
			{
				source.clip = clip;
				source.volume = 1f;
				source.Play();
			}
			//SYNC STUFF IS HERE
		}

		public void ApplyAtmosPack(Atmos.SnapshotType snapshot, float transitionTime, Atmos.AtmosSources enabledSources)
		{
			if (ApplyAtmosRoutine != null)
			{
				StopCoroutine(ApplyAtmosRoutine);
				ApplyAtmosRoutine = null;
			}
			ApplyAtmosRoutine = StartCoroutine(ApplyAtmosPackRoutine(snapshot, transitionTime, enabledSources));
		}

		protected IEnumerator ApplyAtmosPackRoutine(Atmos.SnapshotType snapshot, float transitionTime, Atmos.AtmosSources enabledSources)
		{
			Atmos.GetSnapshot(snapshot).TransitionTo(transitionTime);
			int counter = 0;
			for (int i = 1; i < (int)Atmos.AtmosSources.Everything; i *= 2)
			{
				if ((enabledSources & (Atmos.AtmosSources)i) == (Atmos.AtmosSources)i)
				{
					AudioSource source = atmosSources[counter];
					if (!source.isPlaying)
					{
						source.Play();
					}
				}
				counter++;
			}
			counter = 0;
			yield return new WaitForSecondsRealtime(transitionTime);
			for (int i = 1; i < (int)Atmos.AtmosSources.Everything; i *= 2)
			{
				if ((enabledSources & (Atmos.AtmosSources)i) != (Atmos.AtmosSources)i)
				{
					AudioSource source = atmosSources[counter];
					Debug.Log("Stopping Source = " + source.name);
					if (source.isPlaying)
					{
						source.Stop();
					}
				}
				counter++;
			}
		}

	}
}
