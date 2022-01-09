using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using WeaverCore.Components;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
	/// <summary>
	/// Used to create custom benches in WeaverCore
	/// </summary>
    public class WeaverBench : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("An offset applied to the hero when they sit on the bench")]
		Vector3 benchSitOffset = new Vector3(0f, 0.7f);
		[SerializeField]
		[Tooltip("Is the bench able to be sat on when immediately?")]
		bool benchActiveOnStart = true;
		[SerializeField]
		[Tooltip("How long before the player falls asleep again on the bench")]
		float sitWaitTime = 10f;
		[SerializeField]
		[Tooltip("If the player sits on this bench, should the player also respawn at this bench?")]
		bool setRespawnPoint = true;
		[SerializeField]
		[Tooltip("When the player starts up a game, should the player be resting on the bench? If false, the player will be laying on the ground in front of the bench")]
		bool spawnSittingOnBench = true;

		[Space]
		[Header("Label")]
		[SerializeField]
		[Tooltip("The tranlation sheet name that is used to display the \"Rest\" text in multiple different languages")]
		string sheetName = "Prompts";
		[SerializeField]
		[Tooltip("The language key that is used to display the \"Rest\" text in multiple different languages")]
		string langKey = "REST";
		[SerializeField]
		[Tooltip("This text is used if the language sheet and language key wasn't found")]
		string fallbackText = "Rest";

		[Space]
		[Header("Audio")]
		[SerializeField]
		[Tooltip("The sound that is played when the player rests at a bench")]
		AudioClip BenchRestSound;

		[Space]
		[Header("Prefabs")]
		[SerializeField]
		[Tooltip("The object that is created when the player is nearby the bench, and is used to display the \"Rest\" text")]
		WeaverArrowPrompt ArrowPrompt;
		[SerializeField]
		[Tooltip("The object that is instantiated when the player sits on the bench and the map has been updated. This displays a \"Map Updated\" icon on the bottom-right corner of the screen")]
		GameObject MapUpdateMsg;
		[SerializeField]
		[Tooltip("The object that is instantiated when the player sits on a bench and collects a charm for the first time. ")]
		GameObject CharmEquipMsg;

		Vector3 AdjustVectorInv;
		float tiltAmount = 0f;
		bool tilter = false;
		EventManager eventManager;
		bool benchActive = false;
		bool sleeping = false;
		bool faceHeroRight = true;
		bool facingRight = true;
		bool setRespawn = true;
		GameObject Lit;
		GameObject HudCamera;
		GameObject HudBlanker;
		GameObject GetInventory() => GameObject.FindGameObjectWithTag("Inventory Top");
		DetectHero DetectRange;
		WeaverAnimationPlayer BurstAnim;
		ParticleSystem ParticleB;
		ParticleSystem ParticleF;
		ParticleSystem ParticleRest;
		GameObject PromptMarker;
		GameObject HudCanvas;
		GameObject Health;
		WeaverArrowPrompt InstantiatedPrompt;

		/*/// <summary>
		/// When the player starts the game, should they be resting on the bench when they spawn in. Otherwise, they will be asleep next to the bench
		/// </summary>*/
		bool RespawnResting = false;

		/// <summary>
		/// An offset applied to the hero when they sit on the bench
		/// </summary>
		public Vector3 BenchSitOffset => benchSitOffset;

		string getOffAnimation;

		static GameObject BenchWhiteFlash;

		/// <summary>
		/// Is the bench currently active and ready to be sat on?
		/// </summary>
		public bool BenchActive => benchActive;


		private void Reset()
		{
			ArrowPrompt = WeaverAssets.LoadWeaverAsset<GameObject>("Arrow Prompt").GetComponent<WeaverArrowPrompt>();
			BenchRestSound = WeaverAssets.LoadWeaverAsset<AudioClip>("Bench Rest Sound");
			CharmEquipMsg = WeaverAssets.LoadWeaverAsset<GameObject>("Charm Equip Message");
			MapUpdateMsg = WeaverAssets.LoadWeaverAsset<GameObject>("Map Update Message");
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			UnboundCoroutine.Start(WaitAFrame(gameObject));

			static IEnumerator WaitAFrame(GameObject obj)
			{
				yield return null;
				if (obj != null)
				{
					obj.tag = "RespawnPoint";
				}
			}
		}
#endif

		private void Awake()
		{
			if (BenchWhiteFlash == null)
			{
				BenchWhiteFlash = WeaverAssets.LoadWeaverAsset<GameObject>("Bench White Flash");
			}
			eventManager = GetComponent<EventManager>();
			benchActive = benchActiveOnStart;
			Respawn();
			eventManager.OnReceivedEvent += EventManager_OnReceivedEvent;
		}

/*#if UNITY_EDITOR
		private void Start()
        {
			if (!HeroController.instance.isHeroInPosition)
			{
				HeroController.instance.heroInPosition += OnHeroInPosition;
			}
			else
			{
				Respawn();
			}
		}
#endif*/

		void OnHeroInPosition(bool forceDirect)
        {
			HeroController.instance.heroInPosition -= OnHeroInPosition;
			Respawn();
		}

		private void EventManager_OnReceivedEvent(string eventName, GameObject source)
		{
			if (eventName == "BENCH ACTIVE")
			{
				ActivateBench();
			}
            else if (eventName == "RESPAWN")
            {
				RespawnSittingOnBench();
            }
		}

		/// <summary>
		/// If the bench isn't active, this will make the bench active and ready to be sat on
		/// </summary>
		public void ActivateBench()
		{
			benchActive = true;
		}

		/// <summary>
		/// Respawns the player at this bench
		/// </summary>
		public void Respawn()
		{
			RespawnResting = false;
			StopAllCoroutines();
			StartCoroutine(RespawnRoutine());
		}

		/// <summary>
		/// Respawns the player sitting on the bench. If "Spawn Sitting On Bench" is false, then the player will be laying on the ground
		/// </summary>
		public void RespawnSittingOnBench()
        {
			RespawnResting = spawnSittingOnBench;
			StopAllCoroutines();
			StartCoroutine(RespawnRoutine());
		}

		IEnumerator RespawnRoutine()
		{
			if (RespawnResting)
			{
				var player = HeroController.instance;

				var blanker = WeaverCanvas.HUDBlankerWhite;
				if (blanker != null)
				{
					EventManager.SendEventToGameObject("FADE OUT", blanker, gameObject);
				}
				GameManager.instance.CheckAllAchievements();

				PlayerData.instance.SetBool("atBench", true);

				HeroController.instance.RelinquishControl();
				HeroController.instance.StopAnimationControl();
				HeroController.instance.MaxHealth();

				HeroUtilities.PlayPlayerClip("Sit Fall Asleep");
				EventManager.BroadcastEvent("UPDATE BLUE HEALTH", gameObject);
				EventManager.BroadcastEvent("HERO REVIVED", gameObject);

				HeroController.instance.AffectedByGravity(false);
				var playerRB = HeroController.instance.GetComponent<Rigidbody2D>();
				playerRB.isKinematic = true;
				playerRB.velocity = default;
			}
			yield return null;


			var tilt = GetComponent<RestBenchTilt>();
			if (tilt != null)
			{
				tiltAmount = tilt.GetTilt();
				tilter = true;
			}

			if (!benchActive)
			{
				yield return new WaitUntil(() => benchActive);
			}

			yield return InitRoutine();
		}

		IEnumerator InitRoutine()
		{
			Lit = transform.Find("Lit").gameObject;
			HudCamera = GameObject.FindObjectOfType<HUDCamera>()?.gameObject;
			HudCanvas = HudCamera.transform.Find("Hud Canvas")?.gameObject;
			DetectRange = GetComponentInChildren<DetectHero>();
			BurstAnim = GetComponentInChildren<WeaverAnimationPlayer>(true);
			ParticleB = transform.Find("Particle B")?.GetComponent<ParticleSystem>();
			ParticleF = transform.Find("Particle F")?.GetComponent<ParticleSystem>();
			ParticleRest = transform.Find("Particle Rest")?.GetComponent<ParticleSystem>();
			PromptMarker = transform.Find("Prompt Marker").gameObject;
			Health = HudCanvas?.transform?.Find("Health")?.gameObject;

			if (RespawnResting)
			{
				//Start Sitting
				var playerRB = HeroController.instance.GetComponent<Rigidbody2D>();
				playerRB.isKinematic = true;
				playerRB.velocity = default;
				playerRB.transform.position = HeroController.instance.FindGroundPoint(transform.position) + benchSitOffset;

				if (tilter)
				{
					playerRB.transform.eulerAngles = new Vector3(0f, 0f, tiltAmount);
					playerRB.transform.position += new Vector3(-0.25f, 0f);
				}

				if (HudBlanker != null)
				{
					EventManager.SendEventToGameObject("FADE OUT", HudBlanker, gameObject);
				}
				GameManager.instance.CheckAllAchievements();
				GameManager.instance.ResetSemiPersistentItems();
				PlayerData.instance.SetBool("atBench", true);

				HeroController.instance.RelinquishControl();
				HeroController.instance.StopAnimationControl();
				HeroController.instance.MaxHealth();

				HeroUtilities.PlayPlayerClip("Sit Fall Asleep");

				WeaverLog.Log("Sit Fall Asleep");
				EventManager.BroadcastEvent("UPDATE BLUE HEALTH", gameObject);
				EventManager.BroadcastEvent("HERO REVIVED", gameObject);

				HeroController.instance.AffectedByGravity(false);
				playerRB.isKinematic = true;
				playerRB.velocity = default;

				WeaverLog.Log("Before Wait");

				yield return new WaitForSeconds(1.2f);

				WeaverLog.Log("After Wait");

				getOffAnimation = "Get Off";
				playerRB.isKinematic = false;

				yield return HeroUtilities.PlayPlayerClipTillDone("Wake To Sit");

				if (PlayerData.instance.GetBool("hasQuill") && PlayerData.instance.GetBool("hasMap"))
				{
					var mapUpdated = GameManager.instance.UpdateGameMap();
					Component gameMap = GameManager.instance.gameMap?.GetComponent("GameMap");
					if (gameMap != null)
					{
						gameMap.SendMessage("SetupMap", false);
					}
					if (mapUpdated)
					{
						//Display Map Update MSG
						if (MapUpdateMsg != null)
						{
							GameObject.Instantiate(MapUpdateMsg);
						}
					}

				}

				//RESTING
				yield return RestingRoutine();


			}
			else
			{
				//Start Idle
				yield return new WaitForSeconds(0.25f);
				yield return IdleRoutine();
			}

			yield break;
		}

		IEnumerator RestingRoutine()
		{
			WeaverLog.Log("RESTING");
			PlayerData.instance.SetBool("atBench",true);

			while (true)
			{
				for (float i = 0; i < sitWaitTime; i += Time.deltaTime)
				{
					if (PlayerInput.jump.WasPressed || PlayerInput.attack.WasPressed || PlayerInput.up.WasPressed || PlayerInput.down.WasPressed)
					{
						WeaverLog.Log("GETTING UP_A");
						faceHeroRight = facingRight;
						StartCoroutine(GetUpRoutine());
						yield break;
					}
					if (PlayerInput.left.WasPressed)
					{
						WeaverLog.Log("GETTING UP_B");
						faceHeroRight = false;
						StartCoroutine(GetUpRoutine());
						yield break;
					}
					if (PlayerInput.right.WasPressed)
					{
						WeaverLog.Log("GETTING UP_C");
						faceHeroRight = true;
						StartCoroutine(GetUpRoutine());
						yield break;
					}

					if (PlayerInput.quickMap.WasPressed)
					{
						WeaverLog.Log("GETTING UP_D");
						StartCoroutine(QuickMapRoutine());
						yield break;
					}
					yield return null;
				}

				//FALL ASLEEP
				if (!sleeping)
				{
					sleeping = true;
					getOffAnimation = "Wake";
					HeroUtilities.PlayPlayerClip("Sit Fall Asleep");
				}
			}

			yield break;
		}

		//GET UP
		IEnumerator GetUpRoutine()
		{
			var invOpen = false;
			var inv = GetInventory();
			if (inv != null)
			{
				invOpen = PlayMakerUtilities.GetFsmBool(inv, "Inventory Control", "Open");
			}

			if (GameManager.instance.IsGamePaused() || invOpen)
			{
				yield return null;
				yield return RestingRoutine();
				yield break;
			}

			EventManager.BroadcastEvent("BENCH CLOSE", gameObject);

			if (faceHeroRight)
			{
				HeroController.instance.FaceRight();
			}
			else
			{
				HeroController.instance.FaceLeft();
			}
			PlayerData.instance.SetBool("atBench", false);
			if (sleeping)
			{
				yield return HeroUtilities.PlayPlayerClipTillDone("Wake To Sit");
			}

			if (tilter)
			{
				HeroController.instance.transform.position += new Vector3(0.25f,0f,0f);
			}

			HeroController.instance.AffectedByGravity(true);
			var playerRB = HeroController.instance.GetComponent<Rigidbody2D>();
			playerRB.isKinematic = false;
			HeroController.instance.transform.rotation = Quaternion.identity;
			PlayerData.instance.SetBool("atBench", false);
			EventManager.BroadcastEvent("BENCHREST END", gameObject);

			if (HudCanvas != null)
			{
				EventManager.SendEventToGameObject("IN", HudCamera, gameObject);
			}

			EventManager.BroadcastEvent("BENCH UNSIT", gameObject);

			AdjustVectorInv = new Vector3(0f, -0.1f, 0f);

			HeroUtilities.PlayPlayerClip(getOffAnimation);

			Vector3 oldPos = HeroController.instance.transform.position;
			Vector3 newPos = oldPos + AdjustVectorInv;

			for (float i = 0; i < 0.2f; i += Time.deltaTime)
			{
				HeroController.instance.transform.position = Vector3.Lerp(oldPos,newPos,i / 0.2f);
			}

			HeroController.instance.transform.position = newPos;

			yield return new WaitForSeconds(0.05f);

			HeroUtilities.PlayPlayerClip("Idle");

			yield return new WaitForSeconds(0.1f);

			PlayerData.instance.SetBool("disablePause", false);
			HeroController.instance.RegainControl();
			HeroController.instance.StartAnimationControl();
			yield return new WaitForSeconds(0.5f);

			if (ParticleF != null)
			{
				var e = ParticleF.emission;
				e.enabled = true;
			}

			if (ParticleB != null)
			{
				var e = ParticleB.emission;
				e.enabled = true;
			}

			yield return IdleRoutine();

			yield break;
		}

		IEnumerator IdleRoutine()
		{
			WeaverLog.Log("IDLING");
			if (InstantiatedPrompt != null)
			{
				InstantiatedPrompt.Hide();
				InstantiatedPrompt = null;
			}

			WeaverLog.Log("WAITING TO BE IN RANGE");
			yield return new WaitUntil(() => DetectRange.HeroInRange);
			WeaverLog.Log("IN RANGE");

			InstantiatedPrompt = WeaverArrowPrompt.Spawn(ArrowPrompt, gameObject, PromptMarker.transform.position);
			InstantiatedPrompt.SetLabelTextLang(langKey, sheetName);
			if (InstantiatedPrompt.Label.text == "PLACEHOLDER")
			{
				InstantiatedPrompt.SetLabelText(fallbackText);
			}
			InstantiatedPrompt.Show();

			while (true)
			{
				if (!DetectRange.HeroInRange)
				{
					StartCoroutine(IdleRoutine());
					yield break;
				}

				if (PlayerInput.down.WasPressed || PlayerInput.up.WasPressed)
				{
					WeaverLog.Log("GOING ONTO BENCH");
					if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Town")
					{
						EventRegister.SendEvent("HIDE INTERACT REMINDER");
					}
					WeaverLog.Log("BENCH_A");
					if (HeroController.instance.CanInput())
					{
						WeaverLog.Log("BENCH_B");
						var onGround = HeroController.instance.GetState("onGround");
						var attacking = HeroController.instance.GetState("attacking");
						var attackingUp = HeroController.instance.GetState("upAttacking");
						var attackingDown = HeroController.instance.GetState("downAttacking");
						var dashing = HeroController.instance.GetState("dashing");
						var backDashing = HeroController.instance.GetState("backDashing");

						if (onGround && !(attacking || attackingUp || attackingDown || dashing || backDashing))
						{
							WeaverLog.Log("BENCH_C");
							EventManager.BroadcastEvent("BENCH SIT",gameObject);
							HeroController.instance.RelinquishControl();
							PlayerData.instance.SetBool("atBench", true);
							HeroController.instance.CharmUpdate();

							if (BenchRestSound != null)
							{
								WeaverAudio.PlayAtPoint(BenchRestSound, transform.position, 1f, Enums.AudioChannel.Sound);
							}

							HeroController.instance.StopAnimationControl();
							HeroController.instance.AffectedByGravity(false);
							if (InstantiatedPrompt != null)
							{
								InstantiatedPrompt.Hide();
								InstantiatedPrompt = null;
							}
							HeroUtilities.PlayPlayerClip("Sit");


							var selfX = transform.GetXPosition();
							var sitVector = HeroController.instance.transform.position;
							sitVector.x = selfX;
							sitVector += benchSitOffset;

							var oldPos = HeroController.instance.transform.position;
							var newPos = sitVector;

							static float EaseOutCurve(float n)
							{
								return Mathf.Sin(n * Mathf.PI / 2f);
							}

							for (float i = 0; i < 0.2f; i += Time.deltaTime)
							{
								HeroController.instance.transform.position = Vector3.Lerp(oldPos,newPos, EaseOutCurve(i / 0.2f));
								yield return null;
							}
							HeroController.instance.transform.position = newPos;

							yield return new WaitForSeconds(0.1f);

							if (tilter)
							{
								HeroController.instance.transform.eulerAngles = new Vector3(tiltAmount,0f,0f);
								HeroController.instance.transform.SetPositionX(newPos.x - 0.25f);
							}

							yield return null;

							var flasher = HeroController.instance.GetComponent<SpriteFlasher>();
							if (flasher != null)
							{
								flasher.flashBenchRest();
							}

							Component gameFlasher = HeroController.instance.GetComponent("SpriteFlash");
							if (gameFlasher != null)
							{
								gameFlasher.SendMessage("flashBenchRest");
							}

							EventManager.BroadcastEvent("BENCHREST",gameObject);
							EventManager.BroadcastEvent("HERO REVIVED", gameObject);

							var charmEffects = HeroController.instance.transform.Find("Charm Effects");
							var blessingGhost = HeroController.instance.transform.Find("Blessing Ghost");

							if (blessingGhost != null)
							{
								EventManager.SendEventToGameObject("BENCHREST", blessingGhost.gameObject, gameObject);
							}

							var blockerShield = charmEffects?.Find("Blocker Shield");

							if (blockerShield != null)
							{
								EventManager.SendEventToGameObject("HERO REVIVED", blockerShield.gameObject, gameObject);
							}

							GameManager.instance.ResetSemiPersistentItems();

							var focusEffects = HeroController.instance.transform.Find("Focus Effects");

							sleeping = false;
							getOffAnimation = "Get Off";

							Pooling.Instantiate(BenchWhiteFlash,transform.position + new Vector3(0f,0f,-0.2f),Quaternion.identity);

							if (BurstAnim != null)
							{
								BurstAnim.gameObject.SetActive(true);
							}
							HeroController.instance.MaxHealth();

							if (Health != null)
							{
								EventManager.SendEventToGameObject("UPDATE BLUE HEALTH",Health,gameObject);
							}

							if (ParticleF != null)
							{
								var e = ParticleF.emission;
								e.enabled = false;
							}

							if (ParticleB != null)
							{
								var e = ParticleB.emission;
								e.enabled = false;
							}

							if (ParticleRest != null)
							{
								ParticleRest.Play();
							}

							var sceneName = GameManager.instance.GetSceneNameString();
							var spawnName = gameObject.name;

							if (setRespawnPoint)
							{
								PlayerData.instance.SetString("respawnMarkerName", spawnName);
								PlayerData.instance.SetString("respawnScene", sceneName);
								PlayerData.instance.SetInt("respawnType", 1);
								PlayerData.instance.SetBool("respawnFacingRight", facingRight);
								GameManager.instance.SetCurrentMapZoneAsRespawn();
								WeaverLog.Log("SETTING RESPAWN POINT!!!");
							}

							yield return new WaitForSeconds(0.1f);
							GameManager.instance.CheckAllAchievements();
							GameManager.instance.SaveGame();
							GameManager.instance.TimePasses();
							GameManager.instance.StoryRecord_rest();
							GameManager.instance.AddToBenchList();

							WeaverLog.Log("SAVED Respawn Type = " + PlayerData.instance.GetInt("respawnType"));

							if (PlayerData.instance.GetBool("hasQuill") && PlayerData.instance.GetBool("hasMap"))
							{
								var mapUpdated = GameManager.instance.UpdateGameMap();
								if (GameManager.instance.gameMap)
								{
									GameManager.instance.gameMap.SendMessage("SetupMap",false);
								}

								if (mapUpdated)
								{
									if (MapUpdateMsg != null)
									{
										GameObject.Instantiate(MapUpdateMsg);
									}
									yield return HeroUtilities.PlayPlayerClipTillDone("Map Update");
									HeroUtilities.PlayPlayerClip("Sit Idle");
								}
							}

							if (PlayerData.instance.GetBool("hasCharm") && !PlayerData.instance.GetBool("charmBenchMsg"))
							{
								PlayerData.instance.SetBool("charmBenchMsg",true);

								if (CharmEquipMsg != null)
								{
									GameObject.Instantiate(CharmEquipMsg, transform.position, Quaternion.identity);
								}

								PlayerData.instance.SetBool("atBench", true);
							}

							yield return new WaitForSeconds(0.5f);
							PlayerData.instance.SetBool("atBench", true);
							yield return RestingRoutine();

							yield break;
						}
					}

					StartCoroutine(IdleRoutine());
					yield break;
				}

				yield return null;
			}

			yield break;
		}

		void MapEventListener(string eventName, GameObject src)
		{
			eventManager.OnReceivedEvent -= MapEventListener;
			if (eventName == "INVENTORY OPENED")
			{
				StopAllCoroutines();
				StartCoroutine(CloseMapState());
			}
		}

		void OpenInventoryMapListener(string eventName, GameObject src)
		{
			eventManager.OnReceivedEvent -= OpenInventoryMapListener;
			if (eventName == "OPEN INVENTORY MAP")
			{
				StopAllCoroutines();
				StartCoroutine(CloseAnimState(true));
			}
		}

		//OPEN MAP
		IEnumerator QuickMapRoutine()
		{
			yield return null;
			eventManager.OnReceivedEvent += MapEventListener;
			Debug.Log("PLAYER HAS MAP = " + PlayerData.instance.GetBool("hasMap"));
			if (PlayerData.instance.GetBool("hasMap"))
			{
				var inv = GetInventory();
				Debug.Log("INV = " + inv);
				Debug.Log("INV Open = " + !PlayMakerUtilities.GetFsmBool(inv, "Inventory Control", "Open"));
				if (inv != null && !PlayMakerUtilities.GetFsmBool(inv, "Inventory Control","Open"))
				{
					HeroUtilities.PlayPlayerClip("Sit Map Open");
					EventManager.BroadcastEvent("MAP OPENED", gameObject);
					PlayerData.instance.SetBool("disablePause", true);
					EventManager.BroadcastEvent("OPEN QUICK MAP", gameObject);
					EventManager.BroadcastEvent("STOP HERO EXIT", gameObject);



					for (float i = 0; i < 0.25f; i += Time.deltaTime)
					{
						if (PlayerInput.quickMap.WasPressed)
						{
							PlayerData.instance.SetBool("disablePause", false);
							if (inv != null)
							{
								EventManager.BroadcastEvent("OPEN INVENTORY MAP", gameObject);
							}
							HeroUtilities.PlayPlayerClip("Sit Map Close");
							yield return new WaitForSeconds(0.42f);
							eventManager.OnReceivedEvent -= MapEventListener;
							yield return RestingRoutine();
							yield break;
						}

						yield return null;
					}
					eventManager.OnReceivedEvent -= MapEventListener;
					yield return MapIdleState();
					yield break;
				}
				else
				{
					EventManager.BroadcastEvent("CLOSE QUICK MAP", gameObject);
					eventManager.OnReceivedEvent -= MapEventListener;
					yield return RestingRoutine();
					yield break;
				}
			}
			else
			{
				eventManager.OnReceivedEvent -= MapEventListener;
				yield return RestingRoutine();
				yield break;
			}

			yield break;
		}

		IEnumerator MapIdleState()
		{
			eventManager.OnReceivedEvent += MapEventListener;
			yield return null;
			EventManager.BroadcastEvent("OPEN QUICK MAP", gameObject);

			yield return new WaitUntil(() => PlayerInput.quickMap.WasReleased || !PlayerInput.quickMap.IsPressed);
			eventManager.OnReceivedEvent -= MapEventListener;
			yield return CloseMapState();
		}

		IEnumerator CloseMapState()
		{
			yield return null;
			EventManager.BroadcastEvent("CLOSE QUICK MAP", gameObject);

			HeroUtilities.PlayPlayerClip("Sit Map Close");
			yield return null;
			yield return CloseAnimState(false);
		}

		IEnumerator CloseAnimState(bool waitAFrame)
		{
			if (waitAFrame)
			{
				yield return null;
			}
			eventManager.OnReceivedEvent += OpenInventoryMapListener;
			yield return null;
			HeroController.instance.RelinquishControl();
			HeroController.instance.StopAnimationControl();
			yield return HeroUtilities.PlayPlayerClipTillDone("Sit Map Close");

			eventManager.OnReceivedEvent -= OpenInventoryMapListener;
			yield return null;
			PlayerData.instance.SetBool("disablePause", false);

			yield return RestingRoutine();
		}
	}
}
