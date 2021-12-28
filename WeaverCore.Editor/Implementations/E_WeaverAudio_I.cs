using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Attributes;
using WeaverCore.Editor.Utilities;
using WeaverCore.Enums;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class E_WeaverAudio_I : WeaverAudio_I
	{
		AudioMixer _mainMixer;
		public override AudioMixer MainMixer
		{
			get
			{
                if (_mainMixer == null)
                {
					_mainMixer = EditorAssets.LoadEditorAsset<AudioMixer>("Music");
                }
				return _mainMixer;
			}
		}

		static AudioMixerGroup _mainMusic;
		public override AudioMixerGroup MainMusic
		{
			get
			{
                if (_mainMusic == null)
                {
					_mainMusic = (AudioMixerGroup)AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(MainMixer)).First(o => o is AudioMixerGroup g && g.name == "Main");
				}
				return _mainMusic;
			}
		}

		static AudioMixerGroup _master;
		public override AudioMixerGroup Master
		{
			get
			{
				if (_master == null)
				{
					_master = (AudioMixerGroup)AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(MainMixer)).First(o => o is AudioMixerGroup g && g.name == "Master");
				}
				return _master;
			}
		}

		static AudioMixerGroup _sounds;
		public override AudioMixerGroup Sounds
		{
			get
			{
				if (_sounds == null)
				{
					_sounds = (AudioMixerGroup)AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(MainMixer)).First(o => o is AudioMixerGroup g && g.name == "Sounds");
				}
				return _sounds;
			}
		}


		public override AudioMixerGroup GetMixerForChannel(AudioChannel channel)
		{
            switch (channel)
            {
                case AudioChannel.Master:
					return Master;
                case AudioChannel.Sound:
					return Sounds;
                case AudioChannel.Music:
					return MainMusic;
                default:
					return null;
            }
        }
	}
}
