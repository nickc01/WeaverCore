using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace WeaverCore.Editor
{
	public class EditorMusic : MonoBehaviour
	{
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

		static EditorMusic _instance;

		Coroutine ApplyMusicRoutine;
		Coroutine ApplySnapshotRoutine;

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
						if (path.Contains("WeaverCore.Editor"))
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
			//WeaverLog.Log("Snapshot = " + snapshot);
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
	}
}
