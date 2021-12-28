using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace WeaverCore.Assets.Components
{
	/// <summary>
	/// Was used early in development to test out different audio snapshots and audio groups
	/// </summary>
	public class AudioTester : MonoBehaviour
	{
		[SerializeField]
		AudioClip testingAudio;
		[SerializeField]
		string targettingMixer = "Music";
		[SerializeField]
		AudioSource TestAudioSource;
		[SerializeField]
		TMP_Dropdown groupDropdown;
		[SerializeField]
		TMP_Dropdown snapshotDropdown;

		//Mixer groups that audio sources can be isolated to
		List<AudioMixerGroup> MixerGroups;

		//Snapshots that can affect one or multiple different mixer groups
		List<AudioMixerSnapshot> Snapshots;

		void Start()
		{
			if (Initialization.Environment == Enums.RunningState.Game)
			{
				groupDropdown.ClearOptions();
				snapshotDropdown.ClearOptions();

				MixerGroups = Resources.FindObjectsOfTypeAll<AudioMixerGroup>().Where(g => g.audioMixer.name == targettingMixer).ToList();
				Snapshots = Resources.FindObjectsOfTypeAll<AudioMixerSnapshot>().Where(s => s.audioMixer.name == targettingMixer).ToList();
				groupDropdown.AddOptions(ToOptions(MixerGroups, g => g.name));
				snapshotDropdown.AddOptions(ToOptions(Snapshots, s => s.name));
			}
		}


		static List<TMP_Dropdown.OptionData> ToOptions<T>(List<T> values)
		{
			return ToOptions(values, v => v.ToString());
		}

		static List<TMP_Dropdown.OptionData> ToOptions<T>(List<T> values, Func<T,string> predicate)
		{
			List<TMP_Dropdown.OptionData> Options = new List<TMP_Dropdown.OptionData>();

			for (int i = 0; i < values.Count; i++)
			{
				Options.Add(new TMP_Dropdown.OptionData(predicate(values[i])));
			}
			return Options;
		}


		public void PlayTestAudio()
		{
			TestAudioSource.Play();
		}

		public void StopTestAudio()
		{
			TestAudioSource.Stop();
		}

		public void ApplySnapshot()
		{
			var index = snapshotDropdown.value;

			if (index < Snapshots.Count)
			{
				var snapshot = Snapshots[index];

				snapshot.TransitionTo(0f);
			}
		}

		public void OnGroupChange(int groupIndex)
		{
			//Doing this because the passing it by parameter is not working properly
			groupIndex = groupDropdown.value;
			if (groupIndex < MixerGroups.Count)
			{
				var group = MixerGroups[groupIndex];

				TestAudioSource.outputAudioMixerGroup = group;
			}
		}

		public void OnSnapshotChange(int snapshotIndex)
		{

		}
	}

}
