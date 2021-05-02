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

				//var allGroups = GameObject.FindObjectsOfType<AudioMixerGroup>();
				//var allSnapshots = GameObject.FindObjectsOfType<AudioMixerSnapshot>();

				/*WeaverLog.Log("All Snapshots Found = " + allSnapshots.GetLength(0));
				for (int i = 0; i < allSnapshots.GetLength(0); i++)
				{
					WeaverLog.Log("A_Snapshot = " + allSnapshots[i].name);
					WeaverLog.Log("A_Snapshot Mixer = " + allSnapshots[i].audioMixer.name);
				}

				WeaverLog.Log("All Groups Found = " + allGroups.GetLength(0));
				for (int i = 0; i < allGroups.GetLength(0); i++)
				{
					WeaverLog.Log("A_Group = " + allGroups[i].name);
					WeaverLog.Log("A_Group Mixer = " + allGroups[i].audioMixer.name);
				}*/

				//WeaverLog.Log("Targetting Mixer = " + targettingMixer);
				//WeaverLog.Log("Found Groups = " + MixerGroups.Count);
				/*for (int i = 0; i < MixerGroups.Count; i++)
				{
					WeaverLog.Log("Group = " + MixerGroups[i].name);
				}

				WeaverLog.Log("Found Snapshots = " + Snapshots.Count);
				for (int i = 0; i < Snapshots.Count; i++)
				{
					WeaverLog.Log("Snapshot = " + Snapshots[i].name);
				}*/


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
				//WeaverLog.Log("Setting snapshot index = " + index);
				//WeaverLog.Log("Setting Snapshot to = " + Snapshots[index].name);
				var snapshot = Snapshots[index];

				snapshot.TransitionTo(0f);
			}
		}

		public void OnGroupChange(int groupIndex)
		{
			//Doing this because the passing it by parameter is not working properly
			groupIndex = groupDropdown.value;

			//WeaverLog.Log("New Group Index = " + groupIndex);

			if (groupIndex < MixerGroups.Count)
			{
				//WeaverLog.Log("Setting group to = " + MixerGroups[groupIndex].name);
				var group = MixerGroups[groupIndex];

				TestAudioSource.outputAudioMixerGroup = group;
			}
		}

		public void OnSnapshotChange(int snapshotIndex)
		{
			//WeaverLog.Log("New Snapshot Index = " + snapshotIndex);
			/*if (snapshotIndex < Snapshots.Count)
			{
				WeaverLog.Log("Snapshot Name = " + Snapshots[snapshotIndex].name);
			}*/
		}
	}

}
