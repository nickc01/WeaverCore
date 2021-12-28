using GlobalEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore
{

	/// <summary>
	/// An object used to do actions that are related to the whole game
	/// </summary>
	public class WeaverGameManager : MonoBehaviour
	{
		/// <summary>
		/// An event that is called when the game state changes
		/// </summary>
		public static event Action<GameState> OnGameStateChange;

		public enum TimeFreezePreset
		{
			Preset0,
			Preset1,
			Preset2,
			Preset3,
			Preset4,
			Preset5
		}

		/// <summary>
		/// Freezes the game temporarily. Mainly used when the player gets hit, the enemy is stunned, and more
		/// </summary>
		/// <param name="preset">The specific preset to apply</param>
		public static void FreezeGameTime(TimeFreezePreset preset)
		{
			switch (preset)
			{
				case TimeFreezePreset.Preset0:
					GameManager.instance.StartCoroutine(GameManager.instance.FreezeMoment(0.01f, 0.35f, 0.1f, 0f));
					break;
				case TimeFreezePreset.Preset1:
					GameManager.instance.StartCoroutine(GameManager.instance.FreezeMoment(0.04f, 0.03f, 0.04f, 0f));
					break;
				case TimeFreezePreset.Preset2:
					GameManager.instance.StartCoroutine(GameManager.instance.FreezeMoment(0.25f, 2f, 0.25f, 0.15f));
					break;
				case TimeFreezePreset.Preset3:
					GameManager.instance.StartCoroutine(GameManager.instance.FreezeMoment(0.01f, 0.25f, 0.1f, 0f));
					break;
				case TimeFreezePreset.Preset4:
					GameManager.instance.StartCoroutine(GameManager.instance.FreezeMoment(0.01f, 0.25f, 0.1f, 0f));
					break;
				case TimeFreezePreset.Preset5:
					GameManager.instance.StartCoroutine(GameManager.instance.FreezeMoment(0.01f, 0.25f, 0.1f, 0f));
					break;
			}
		}

		public static void TriggerGameStateChange()
		{
			if (OnGameStateChange != null)
			{
				OnGameStateChange.Invoke(GameManager.instance.gameState);
			}
		}

		/// <summary>
		/// Gets the current map zone the player is located in
		/// </summary>
		public static MapZone CurrentMapZone
		{
			get
			{
				return GameManager.instance.sm.mapZone;
			}
		}
	}
}
