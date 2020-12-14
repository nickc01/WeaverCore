using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Attributes;

namespace WeaverCore.Game
{
	static class Tester
	{
		static List<char> characterList = new List<char>
			{
				'A',
				'B',
				'C',
				'D',
				'E',
				'F',
				'G',
				'H',
				'I',
				'J',
				'K',
				'L',
				'M',
				'N',
				'O',
				'P',
				'Q',
				'R',
				'S',
				'T',
				'U',
				'V',
				//'W',
				//'X',
				'Y',
				//'Z',
				/*'1',
				'2',
				'3',
				'4',
				'5',
				'6',
				'7',
				'8',
				'9',
				'0',*/
				'a',
				'b',
				'c',
				'd',
				'e',
				'f',
				'g',
				'h',
				'i',
				'j',
				'k',
				'l',
				'm',
				'n',
				'o',
				'p',
				'q',
				'r',
				's',
				't',
				'u',
				'v',
				//'w',
				//'x',
				'y'/*,
				'z',
				'-',
				'_',
				' '*/
			};

		class CompareSnapshots : Comparer<AudioMixerSnapshot>
		{
			Comparer<string> strCompare = Comparer<string>.Default;

			public override int Compare(AudioMixerSnapshot x, AudioMixerSnapshot y)
			{
				return strCompare.Compare(x.audioMixer.name, y.audioMixer.name);
			}
		}

		[OnInit]
		static void testInit()
		{



			AudioMixerSnapshot SilentSnapshot = null;

			var snapshots = Resources.FindObjectsOfTypeAll<AudioMixerSnapshot>().ToList();

			snapshots.Sort(new CompareSnapshots());
			foreach (var snapshot in snapshots)
			{
				//if (snapshot.name.Contains("Master"))
				//{
					WeaverLog.Log("--Snapshot = " + snapshot.name);
					WeaverLog.Log("Mixer = " + snapshot.audioMixer.name);
				//}
			}

			/*if (SilentSnapshot != null)
			{
				WeaverLog.Log("Found Silent Snapshot!");
			}

			for (int maxLength = 1; maxLength < 12; maxLength++)
			{
				char[] strTest = new char[maxLength];
				for (int j = 0; j < maxLength; j++)
				{
					strTest[j] = characterList[0];
				}

				do
				{
					string finalString = new string(strTest);

					WeaverLog.Log("Final String = " + finalString);

					try
					{
						float value = 0f;
						SilentSnapshot.audioMixer.GetFloat(finalString, out value);
						WeaverLog.Log("Final Value = " + value);
					}
					catch
					{

					}

					int settingIndex = 0;

					while (settingIndex >= 0 && settingIndex < maxLength)
					{
						var charIndex = characterList.IndexOf(strTest[settingIndex]);
						//WeaverLog.Log("Setting Index = " + settingIndex);
						//Debug.Log("Current Char Value = " + strTest[settingIndex]);
						//Debug.Log("Index = " + charIndex);
						charIndex++;
						if (charIndex >= characterList.Count)
						{
							//Debug.Log("Resetting Index to zero");
							charIndex = 0;
							strTest[settingIndex] = characterList[charIndex];
							settingIndex++;
						}
						else
						{
							//Debug.Log("Moving to next value");
							//charIndex++;
							//Debug.Log("Next index = " + charIndex);
							//Debug.Log("Next Value = " + characterList[charIndex]);
							strTest[settingIndex] = characterList[charIndex];
							settingIndex = -1;
						}
					}

				} while (!IsCharSetBlank(strTest));
			}*/
		}

		static bool IsCharSetBlank(char[] charSet)
		{
			for (int i = 0; i < charSet.GetLength(0); i++)
			{
				if (charSet[i] != characterList[0])
				{
					return false;
				}
			}

			return true;
		}
	}
}
