/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPlayer : MonoBehaviour 
{
	public static NewPlayer Instance { get; private set; }

	void Awake()
	{
		Instance = this;
	}

	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
}*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
//using GlobalEnums;
//using HutongGames.PlayMaker;
using Modding;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

/*public enum ActorStates
{
	grounded,
	idle,
	running,
	airborne,
	wall_sliding,
	hard_landing,
	dash_landing,
	no_input,
	previous
}

public enum CollisionSide
{
	top,
	left,
	right,
	bottom,
	other
}

public enum DamageMode
{
	FULL_DAMAGE,
	HAZARD_ONLY,
	NO_DAMAGE
}

public enum AttackDirection
{
	normal,
	upward,
	downward
}

public enum HeroSounds
{
	FOOTSTEPS_RUN,
	FOOTSTEPS_WALK,
	JUMP,
	WALLJUMP,
	SOFT_LANDING,
	HARD_LANDING,
	BACKDASH,
	DASH,
	TAKE_HIT,
	WALLSLIDE,
	NAIL_ART_CHARGE,
	NAIL_ART_READY,
	FALLING
}

public enum HeroTransitionState
{
	WAITING_TO_TRANSITION,
	EXITING_SCENE,
	WAITING_TO_ENTER_LEVEL,
	ENTERING_SCENE,
	DROPPING_DOWN
}

public enum PhysLayers
{
	DEFAULT,
	IGNORE_RAYCAST = 2,
	WATER = 4,
	UI,
	TERRAIN = 8,
	PLAYER,
	TRANSITION_GATES,
	ENEMIES,
	PROJECTILES,
	HERO_DETECTOR,
	TERRAIN_DETECTOR,
	ENEMY_DETECTOR,
	ITEM,
	HERO_ATTACK,
	PARTICLE,
	INTERACTIVE_OBJECT,
	HERO_BOX,
	GRASS,
	ENEMY_ATTACK,
	DAMAGE_ALL,
	BOUNCER,
	SOFT_TERRAIN,
	CORPSE,
	UGUI,
	MAP_PIN = 30
}


public enum GatePosition
{
	top,
	right,
	left,
	bottom,
	door,
	unknown
}

namespace WeaverCore.Editor
{
	public class HeroController : MonoBehaviour
	{
		public float fallTimer { get; private set; }

		public GeoCounter geoCounter
		{
			get
			{
				return GeoCounter.Instance;
			}
		}

		public PlayMakerFSM proxyFSM { get; private set; }

		public TransitionPoint sceneEntryGate { get; private set; }

		public event HeroController.HeroInPosition heroInPosition;

		public event HeroController.TakeDamageEvent OnTakenDamage;

		public event HeroController.HeroDeathEvent OnDeath;

		// Token: 0x17000210 RID: 528
		// (get) Token: 0x060021E1 RID: 8673 RVA: 0x000BDB18 File Offset: 0x000BBD18
		public bool IsDreamReturning
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000211 RID: 529
		// (get) Token: 0x060021E2 RID: 8674 RVA: 0x000BDB5C File Offset: 0x000BBD5C
		public static HeroController instance
		{
			get
			{
				if (HeroController._instance == null)
				{
					HeroController._instance = UnityEngine.Object.FindObjectOfType<HeroController>();
					if (HeroController._instance == null)
					{
						UnityEngine.Debug.LogError("Couldn't find a Hero, make sure one exists in the scene.");
					}
					else
					{
						UnityEngine.Object.DontDestroyOnLoad(HeroController._instance.gameObject);
					}
				}
				return HeroController._instance;
			}
		}

		
		private void Awake()
		{
			if (HeroController._instance == null)
			{
				HeroController._instance = this;
				UnityEngine.Object.DontDestroyOnLoad(this);
			}
			else if (this != HeroController._instance)
			{
				UnityEngine.Object.Destroy(gameObject);
				return;
			}
			SetupGameRefs();
			SetupPools();
		}

		
		private void Start()
		{
			heroInPosition += delegate (bool A_1)
			{
				isHeroInPosition = true;
			};
			playerData = PlayerData.instance;
			ui = UIManager.instance;
			//geoCounter = GameCameras.instance.geoCounter;
			if (superDash == null)
			{
				UnityEngine.Debug.Log("SuperDash came up null, locating manually");
				superDash = FSMUtility.LocateFSM(gameObject, "Superdash");
			}
			if (fsm_thornCounter == null)
			{
				UnityEngine.Debug.Log("Thorn Counter came up null, locating manually");
				fsm_thornCounter = FSMUtility.LocateFSM(transform.Find("Charm Effects").gameObject, "Thorn Counter");
			}
			if (dashBurst == null)
			{
				UnityEngine.Debug.Log("DashBurst came up null, locating manually");
				dashBurst = FSMUtility.GetFSM(transform.Find("Effects").Find("Dash Burst").gameObject);
			}
			if (spellControl == null)
			{
				UnityEngine.Debug.Log("SpellControl came up null, locating manually");
				spellControl = FSMUtility.LocateFSM(gameObject, "Spell Control");
			}
			if (playerData.GetBool("equippedCharm_26"))
			{
				nailChargeTime = NAIL_CHARGE_TIME_CHARM;
			}
			else
			{
				nailChargeTime = NAIL_CHARGE_TIME_DEFAULT;
			}
			if (gm.IsGameplayScene())
			{
				isGameplayScene = true;
				vignette.enabled = true;
				if (heroInPosition != null)
				{
					heroInPosition(false);
				}
				FinishedEnteringScene(true, false);
			}
			else
			{
				isGameplayScene = false;
				transform.SetPositionY(-2000f);
				vignette.enabled = false;
				AffectedByGravity(false);
			}
			CharmUpdate();
			if (acidDeathPrefab)
			{
				ObjectPool.CreatePool(acidDeathPrefab, 1);
			}
			if (spikeDeathPrefab)
			{
				ObjectPool.CreatePool(spikeDeathPrefab, 1);
			}
		}

		
		public void SceneInit()
		{
			if (this == HeroController._instance)
			{
				if (!gm)
				{
					gm = GameManager.instance;
				}
				if (gm.IsGameplayScene())
				{
					isGameplayScene = true;
					HeroBox.inactive = false;
				}
				else
				{
					isGameplayScene = false;
					acceptingInput = false;
					SetState(ActorStates.no_input);
					transform.SetPositionY(-2000f);
					vignette.enabled = false;
					AffectedByGravity(false);
				}
				transform.SetPositionZ(0.004f);
				if (!blockerFix)
				{
					if (playerData.GetBool("killedBlocker"))
					{
						gm.SetPlayerDataInt("killsBlocker", 0);
					}
					blockerFix = true;
				}
				SetWalkZone(false);
			}
		}

		
		private void Update()
		{
			orig_Update();
		}

		
		private void FixedUpdate()
		{
			if (cState.recoilingLeft || cState.recoilingRight)
			{
				if ((float)recoilSteps <= RECOIL_HOR_STEPS)
				{
					recoilSteps++;
				}
				else
				{
					CancelRecoilHorizontal();
				}
			}
			if (cState.dead)
			{
				rb2d.velocity = new Vector2(0f, 0f);
			}
			if ((hero_state == ActorStates.hard_landing && !cState.onConveyor) || hero_state == ActorStates.dash_landing)
			{
				ResetMotion();
			}
			else if (hero_state == ActorStates.no_input)
			{
				if (cState.transitioning)
				{
					if (transitionState == HeroTransitionState.EXITING_SCENE)
					{
						AffectedByGravity(false);
						if (!stopWalkingOut)
						{
							rb2d.velocity = new Vector2(transition_vel.x, transition_vel.y + rb2d.velocity.y);
						}
					}
					else if (transitionState == HeroTransitionState.ENTERING_SCENE)
					{
						rb2d.velocity = transition_vel;
					}
					else if (transitionState == HeroTransitionState.DROPPING_DOWN)
					{
						rb2d.velocity = new Vector2(transition_vel.x, rb2d.velocity.y);
					}
				}
				else if (cState.recoiling)
				{
					AffectedByGravity(false);
					rb2d.velocity = recoilVector;
				}
			}
			else if (hero_state != ActorStates.no_input)
			{
				if (hero_state == ActorStates.running)
				{
					if (move_input > 0f)
					{
						if (CheckForBump(CollisionSide.right))
						{
							rb2d.velocity = new Vector2(rb2d.velocity.x, BUMP_VELOCITY);
						}
					}
					else if (move_input < 0f && CheckForBump(CollisionSide.left))
					{
						rb2d.velocity = new Vector2(rb2d.velocity.x, BUMP_VELOCITY);
					}
				}
				if (!cState.backDashing && !cState.dashing)
				{
					Move(move_input);
					if ((!cState.attacking || attack_time >= ATTACK_RECOVERY_TIME) && !cState.wallSliding && !wallLocked)
					{
						if (move_input > 0f && !cState.facingRight)
						{
							FlipSprite();
							CancelAttack();
						}
						else if (move_input < 0f && cState.facingRight)
						{
							FlipSprite();
							CancelAttack();
						}
					}
					if (cState.recoilingLeft)
					{
						float num;
						if (recoilLarge)
						{
							num = RECOIL_HOR_VELOCITY_LONG;
						}
						else
						{
							num = RECOIL_HOR_VELOCITY;
						}
						if (rb2d.velocity.x > -num)
						{
							rb2d.velocity = new Vector2(-num, rb2d.velocity.y);
						}
						else
						{
							rb2d.velocity = new Vector2(rb2d.velocity.x - num, rb2d.velocity.y);
						}
					}
					if (cState.recoilingRight)
					{
						float num2;
						if (recoilLarge)
						{
							num2 = RECOIL_HOR_VELOCITY_LONG;
						}
						else
						{
							num2 = RECOIL_HOR_VELOCITY;
						}
						if (rb2d.velocity.x < num2)
						{
							rb2d.velocity = new Vector2(num2, rb2d.velocity.y);
						}
						else
						{
							rb2d.velocity = new Vector2(rb2d.velocity.x + num2, rb2d.velocity.y);
						}
					}
				}
				if ((cState.lookingUp || cState.lookingDown) && Mathf.Abs(move_input) > 0.6f)
				{
					ResetLook();
				}
				if (cState.jumping)
				{
					Jump();
				}
				if (cState.doubleJumping)
				{
					DoubleJump();
				}
				if (cState.dashing)
				{
					Dash();
				}
				if (cState.casting)
				{
					if (cState.castRecoiling)
					{
						if (cState.facingRight)
						{
							rb2d.velocity = new Vector2(-CAST_RECOIL_VELOCITY, 0f);
						}
						else
						{
							rb2d.velocity = new Vector2(CAST_RECOIL_VELOCITY, 0f);
						}
					}
					else
					{
						rb2d.velocity = Vector2.zero;
					}
				}
				if (cState.bouncing)
				{
					rb2d.velocity = new Vector2(rb2d.velocity.x, BOUNCE_VELOCITY);
				}
				if (cState.shroomBouncing)
				{
				}
				if (wallLocked)
				{
					if (wallJumpedR)
					{
						rb2d.velocity = new Vector2(currentWalljumpSpeed, rb2d.velocity.y);
					}
					else if (wallJumpedL)
					{
						rb2d.velocity = new Vector2(-currentWalljumpSpeed, rb2d.velocity.y);
					}
					wallLockSteps++;
					if (wallLockSteps > WJLOCK_STEPS_LONG)
					{
						wallLocked = false;
					}
					currentWalljumpSpeed -= walljumpSpeedDecel;
				}
				if (cState.wallSliding)
				{
					if (wallSlidingL && inputHandler.inputActions.right.IsPressed)
					{
						wallUnstickSteps++;
					}
					else if (wallSlidingR && inputHandler.inputActions.left.IsPressed)
					{
						wallUnstickSteps++;
					}
					else
					{
						wallUnstickSteps = 0;
					}
					if (wallUnstickSteps >= WALL_STICKY_STEPS)
					{
						CancelWallsliding();
					}
					if (wallSlidingL)
					{
						if (!CheckStillTouchingWall(CollisionSide.left, false))
						{
							FlipSprite();
							CancelWallsliding();
						}
					}
					else if (wallSlidingR && !CheckStillTouchingWall(CollisionSide.right, false))
					{
						FlipSprite();
						CancelWallsliding();
					}
				}
			}
			if (rb2d.velocity.y < -MAX_FALL_VELOCITY && !inAcid && !controlReqlinquished && !cState.shadowDashing && !cState.spellQuake)
			{
				rb2d.velocity = new Vector2(rb2d.velocity.x, -MAX_FALL_VELOCITY);
			}
			if (jumpQueuing)
			{
				jumpQueueSteps++;
			}
			if (doubleJumpQueuing)
			{
				doubleJumpQueueSteps++;
			}
			if (dashQueuing)
			{
				dashQueueSteps++;
			}
			if (attackQueuing)
			{
				attackQueueSteps++;
			}
			if (cState.wallSliding && !cState.onConveyorV)
			{
				if (rb2d.velocity.y > WALLSLIDE_SPEED)
				{
					rb2d.velocity = new Vector3(rb2d.velocity.x, rb2d.velocity.y - WALLSLIDE_DECEL);
					if (rb2d.velocity.y < WALLSLIDE_SPEED)
					{
						rb2d.velocity = new Vector3(rb2d.velocity.x, WALLSLIDE_SPEED);
					}
				}
				if (rb2d.velocity.y < WALLSLIDE_SPEED)
				{
					rb2d.velocity = new Vector3(rb2d.velocity.x, rb2d.velocity.y + WALLSLIDE_DECEL);
					if (rb2d.velocity.y < WALLSLIDE_SPEED)
					{
						rb2d.velocity = new Vector3(rb2d.velocity.x, WALLSLIDE_SPEED);
					}
				}
			}
			if (nailArt_cyclone)
			{
				if (inputHandler.inputActions.right.IsPressed && !inputHandler.inputActions.left.IsPressed)
				{
					rb2d.velocity = new Vector3(CYCLONE_HORIZONTAL_SPEED, rb2d.velocity.y);
				}
				else if (inputHandler.inputActions.left.IsPressed && !inputHandler.inputActions.right.IsPressed)
				{
					rb2d.velocity = new Vector3(-CYCLONE_HORIZONTAL_SPEED, rb2d.velocity.y);
				}
				else
				{
					rb2d.velocity = new Vector3(0f, rb2d.velocity.y);
				}
			}
			if (cState.swimming)
			{
				rb2d.velocity = new Vector3(rb2d.velocity.x, rb2d.velocity.y + SWIM_ACCEL);
				if (rb2d.velocity.y > SWIM_MAX_SPEED)
				{
					rb2d.velocity = new Vector3(rb2d.velocity.x, SWIM_MAX_SPEED);
				}
			}
			if (cState.superDashOnWall && !cState.onConveyorV)
			{
				rb2d.velocity = new Vector3(0f, 0f);
			}
			if (cState.onConveyor && ((cState.onGround && !cState.superDashing) || hero_state == ActorStates.hard_landing))
			{
				if (cState.freezeCharge || hero_state == ActorStates.hard_landing || controlReqlinquished)
				{
					rb2d.velocity = new Vector3(0f, 0f);
				}
				rb2d.velocity = new Vector2(rb2d.velocity.x + conveyorSpeed, rb2d.velocity.y);
			}
			if (cState.inConveyorZone)
			{
				if (cState.freezeCharge || hero_state == ActorStates.hard_landing)
				{
					rb2d.velocity = new Vector3(0f, 0f);
				}
				rb2d.velocity = new Vector2(rb2d.velocity.x + conveyorSpeed, rb2d.velocity.y);
				superDash.SendEvent("SLOPE CANCEL");
			}
			if (cState.slidingLeft && rb2d.velocity.x > -5f)
			{
				rb2d.velocity = new Vector2(-5f, rb2d.velocity.y);
			}
			if (landingBufferSteps > 0)
			{
				landingBufferSteps--;
			}
			if (ledgeBufferSteps > 0)
			{
				ledgeBufferSteps--;
			}
			if (headBumpSteps > 0)
			{
				headBumpSteps--;
			}
			if (jumpReleaseQueueSteps > 0)
			{
				jumpReleaseQueueSteps--;
			}
			positionHistory[1] = positionHistory[0];
			positionHistory[0] = transform.position;
			cState.wasOnGround = cState.onGround;
		}

		
		private void Update10()
		{
			if (isGameplayScene)
			{
				OutOfBoundsCheck();
			}
			float scaleX = transform.localScale.x;
			if (scaleX < -1f)
			{
				//transform.SetScaleX(-1f);
				transform.SetXLocalScale(-1f);
			}
			if (scaleX > 1f)
			{
				//transform.SetScaleX(1f);
				transform.SetXLocalScale(1f);
			}
			if (transform.position.z != 0.004f)
			{
				//transform.SetPositionZ(0.004f);
				transform.SetZPosition(0.004f);
			}
		}

		
		private void OnLevelUnload()
		{
			if (transform.parent != null)
			{
				SetHeroParent(null);
			}
		}

		
		private void OnDisable()
		{
			if (gm != null)
			{
				gm.UnloadingLevel -= OnLevelUnload;
			}
		}

		
		private void Move(float move_direction)
		{
			if (cState.onGround)
			{
				SetState(ActorStates.grounded);
			}
			if (acceptingInput && !cState.wallSliding)
			{
				if (cState.inWalkZone)
				{
					rb2d.velocity = new Vector2(move_direction * WALK_SPEED, rb2d.velocity.y);
				}
				else if (inAcid)
				{
					rb2d.velocity = new Vector2(move_direction * UNDERWATER_SPEED, rb2d.velocity.y);
				}
				else if (playerData.GetBool("equippedCharm_37") && cState.onGround && playerData.GetBool("equippedCharm_31"))
				{
					rb2d.velocity = new Vector2(move_direction * RUN_SPEED_CH_COMBO, rb2d.velocity.y);
				}
				else if (playerData.GetBool("equippedCharm_37") && cState.onGround)
				{
					rb2d.velocity = new Vector2(move_direction * RUN_SPEED_CH, rb2d.velocity.y);
				}
				else
				{
					rb2d.velocity = new Vector2(move_direction * RUN_SPEED, rb2d.velocity.y);
				}
			}
		}

		
		private void Jump()
		{
			if (jump_steps <= JUMP_STEPS)
			{
				if (inAcid)
				{
					rb2d.velocity = new Vector2(rb2d.velocity.x, JUMP_SPEED_UNDERWATER);
				}
				else
				{
					rb2d.velocity = new Vector2(rb2d.velocity.x, JUMP_SPEED);
				}
				jump_steps++;
				jumped_steps++;
				ledgeBufferSteps = 0;
			}
			else
			{
				CancelJump();
			}
		}

		
		private void DoubleJump()
		{
			if (doubleJump_steps <= DOUBLE_JUMP_STEPS)
			{
				if (doubleJump_steps > 3)
				{
					rb2d.velocity = new Vector2(rb2d.velocity.x, JUMP_SPEED * 1.1f);
				}
				doubleJump_steps++;
			}
			else
			{
				CancelDoubleJump();
			}
			if (cState.onGround)
			{
				CancelDoubleJump();
			}
		}

		
		public void Attack(AttackDirection attackDir)
		{
			//ModHooks.Instance.OnAttack(attackDir);
			bool flag = Time.timeSinceLevelLoad - altAttackTime > ALT_ATTACK_RESET;
			if (flag)
			{
				cState.altAttack = false;
			}
			cState.attacking = true;
			bool equippedCharm_ = playerData.equippedCharm_32;
			if (equippedCharm_)
			{
				attackDuration = ATTACK_DURATION_CH;
			}
			else
			{
				attackDuration = ATTACK_DURATION;
			}
			bool wallSliding = cState.wallSliding;
			if (wallSliding)
			{
				wallSlashing = true;
				slashComponent = wallSlash;
				slashFsm = wallSlashFsm;
			}
			else
			{
				wallSlashing = false;
				bool flag2 = attackDir == AttackDirection.normal;
				if (flag2)
				{
					bool flag3 = !cState.altAttack;
					if (flag3)
					{
						slashComponent = normalSlash;
						slashFsm = normalSlashFsm;
						cState.altAttack = true;
					}
					else
					{
						slashComponent = alternateSlash;
						slashFsm = alternateSlashFsm;
						cState.altAttack = false;
					}
					bool equippedCharm_2 = playerData.equippedCharm_35;
					if (equippedCharm_2)
					{
						bool flag4 = (playerData.health == playerData.maxHealth && !playerData.equippedCharm_27) || (joniBeam && playerData.equippedCharm_27);
						if (flag4)
						{
							bool flag5 = transform.localScale.x < 0f;
							if (flag5)
							{
								//grubberFlyBeam = GameObject.Instantiate(grubberFlyBeamPrefabR,transform.position,Quaternion.identity);
								grubberFlyBeam = GameObject.Instantiate(grubberFlyBeamPrefabR, transform.position, Quaternion.identity);
							}
							else
							{
								//grubberFlyBeam = GameObject.Instantiate(grubberFlyBeamPrefabL,transform.position,Quaternion.identity);
								grubberFlyBeam = GameObject.Instantiate(grubberFlyBeamPrefabL, transform.position, Quaternion.identity);
							}
							bool equippedCharm_3 = playerData.equippedCharm_13;
							if (equippedCharm_3)
							{
								//grubberFlyBeam.transform.SetYLocalScale(MANTIS_CHARM_SCALE);
								grubberFlyBeam.transform.SetYLocalScale(MANTIS_CHARM_SCALE);
							}
							else
							{
								//grubberFlyBeam.transform.SetYLocalScale(1f);
								grubberFlyBeam.transform.SetYLocalScale(1f);
							}
						}
						bool flag6 = playerData.health == 1 && playerData.equippedCharm_6 && playerData.healthBlue < 1;
						if (flag6)
						{
							bool flag7 = transform.localScale.x < 0f;
							if (flag7)
							{
								//grubberFlyBeam = GameObject.Instantiate(grubberFlyBeamPrefabR_fury,transform.position,Quaternion.identity);
								grubberFlyBeam = GameObject.Instantiate(grubberFlyBeamPrefabR_fury, transform.position, Quaternion.identity);
							}
							else
							{
								//grubberFlyBeam = GameObject.Instantiate(grubberFlyBeamPrefabL_fury,transform.position,Quaternion.identity);
								grubberFlyBeam = GameObject.Instantiate(grubberFlyBeamPrefabR_fury, transform.position, Quaternion.identity);
							}
							bool equippedCharm_4 = playerData.equippedCharm_13;
							if (equippedCharm_4)
							{
								//grubberFlyBeam.transform.SetYLocalScale(MANTIS_CHARM_SCALE);
								grubberFlyBeam.transform.SetYLocalScale(MANTIS_CHARM_SCALE);
							}
							else
							{
								//grubberFlyBeam.transform.SetYLocalScale(1f);
								grubberFlyBeam.transform.SetYLocalScale(1f);
							}
						}
					}
				}
				else
				{
					bool flag8 = attackDir == AttackDirection.upward;
					if (flag8)
					{
						slashComponent = upSlash;
						slashFsm = upSlashFsm;
						cState.upAttacking = true;
						bool equippedCharm_5 = playerData.equippedCharm_35;
						if (equippedCharm_5)
						{
							bool flag9 = (playerData.health == playerData.maxHealth && !playerData.equippedCharm_27) || (joniBeam && playerData.equippedCharm_27);
							if (flag9)
							{
								grubberFlyBeam = GameObject.Instantiate(grubberFlyBeamPrefabU,transform.position,Quaternion.identity);
								grubberFlyBeam.transform.SetYLocalScale(transform.localScale.x);
								grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 270f);
								bool equippedCharm_6 = playerData.equippedCharm_13;
								if (equippedCharm_6)
								{
									grubberFlyBeam.transform.SetYLocalScale(grubberFlyBeam.transform.localScale.y * MANTIS_CHARM_SCALE);
								}
							}
							bool flag10 = playerData.health == 1 && playerData.equippedCharm_6 && playerData.healthBlue < 1;
							if (flag10)
							{
								grubberFlyBeam = GameObject.Instantiate(grubberFlyBeamPrefabU_fury,transform.position,Quaternion.identity);
								grubberFlyBeam.transform.SetYLocalScale(transform.localScale.x);
								grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 270f);
								bool equippedCharm_7 = playerData.equippedCharm_13;
								if (equippedCharm_7)
								{
									grubberFlyBeam.transform.SetYLocalScale(grubberFlyBeam.transform.localScale.y * MANTIS_CHARM_SCALE);
								}
							}
						}
					}
					else
					{
						bool flag11 = attackDir == AttackDirection.downward;
						if (flag11)
						{
							slashComponent = downSlash;
							slashFsm = downSlashFsm;
							cState.downAttacking = true;
							bool equippedCharm_8 = playerData.equippedCharm_35;
							if (equippedCharm_8)
							{
								bool flag12 = (playerData.health == playerData.maxHealth && !playerData.equippedCharm_27) || (joniBeam && playerData.equippedCharm_27);
								if (flag12)
								{
									grubberFlyBeam = GameObject.Instantiate(grubberFlyBeamPrefabD,transform.position,Quaternion.identity);
									grubberFlyBeam.transform.SetYLocalScale(transform.localScale.x);
									grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
									bool equippedCharm_9 = playerData.equippedCharm_13;
									if (equippedCharm_9)
									{
										grubberFlyBeam.transform.SetYLocalScale(grubberFlyBeam.transform.localScale.y * MANTIS_CHARM_SCALE);
									}
								}
								bool flag13 = playerData.health == 1 && playerData.equippedCharm_6 && playerData.healthBlue < 1;
								if (flag13)
								{
									grubberFlyBeam = GameObject.Instantiate(grubberFlyBeamPrefabD_fury,transform.position,Quaternion.identity);
									grubberFlyBeam.transform.SetYLocalScale(transform.localScale.x);
									grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
									bool equippedCharm_10 = playerData.equippedCharm_13;
									if (equippedCharm_10)
									{
										grubberFlyBeam.transform.SetYLocalScale(grubberFlyBeam.transform.localScale.y * MANTIS_CHARM_SCALE);
									}
								}
							}
						}
					}
				}
			}
			bool wallSliding2 = cState.wallSliding;
			if (wallSliding2)
			{
				bool facingRight = cState.facingRight;
				if (facingRight)
				{
					slashFsm.FsmVariables.GetFsmFloat("direction").Value = 180f;
				}
				else
				{
					slashFsm.FsmVariables.GetFsmFloat("direction").Value = 0f;
				}
			}
			else
			{
				bool flag14 = attackDir == AttackDirection.normal && cState.facingRight;
				if (flag14)
				{
					slashFsm.FsmVariables.GetFsmFloat("direction").Value = 0f;
				}
				else
				{
					bool flag15 = attackDir == AttackDirection.normal && !cState.facingRight;
					if (flag15)
					{
						slashFsm.FsmVariables.GetFsmFloat("direction").Value = 180f;
					}
					else
					{
						bool flag16 = attackDir == AttackDirection.upward;
						if (flag16)
						{
							slashFsm.FsmVariables.GetFsmFloat("direction").Value = 90f;
						}
						else
						{
							bool flag17 = attackDir == AttackDirection.downward;
							if (flag17)
							{
								slashFsm.FsmVariables.GetFsmFloat("direction").Value = 270f;
							}
						}
					}
				}
			}
			altAttackTime = Time.timeSinceLevelLoad;
			//ModHooks.Instance.AfterAttack(attackDir);
			bool flag18 = !cState.attacking;
			if (!flag18)
			{
				slashComponent.StartSlash();
				bool equippedCharm_11 = playerData.equippedCharm_38;
				if (equippedCharm_11)
				{
					fsm_orbitShield.SendEvent("SLASH");
				}
			}
		}

		
		private void Dash()
		{
			AffectedByGravity(false);
			ResetHardLandingTimer();
			bool flag = dash_timer > DASH_TIME;
			if (flag)
			{
				FinishedDashing();
			}
			else
			{
				Vector2 vector = OrigDashVector();
				//vector = ModHooks.Instance.DashVelocityChange(vector);
				rb2d.velocity = vector;
				dash_timer += Time.deltaTime;
			}
		}

		
		private void BackDash()
		{
		}

		
		private void ShadowDash()
		{
		}

		
		private void SuperDash()
		{
		}

		
		public void FaceRight()
		{
			cState.facingRight = true;
			Vector3 localScale = transform.localScale;
			localScale.x = -1f;
			transform.localScale = localScale;
		}

		
		public void FaceLeft()
		{
			cState.facingRight = false;
			Vector3 localScale = transform.localScale;
			localScale.x = 1f;
			transform.localScale = localScale;
		}

		
		public void StartMPDrain(float time)
		{
			orig_StartMPDrain(time);
			//focusMP_amount *= ModHooks.Instance.OnFocusCost();
		}

		
		public void StopMPDrain()
		{
			drainMP = false;
		}

		
		public void SetBackOnGround()
		{
			cState.onGround = true;
		}

		
		public void SetStartWithWallslide()
		{
			startWithWallslide = true;
		}

		
		public void SetStartWithJump()
		{
			startWithJump = true;
		}

		
		public void SetStartWithFullJump()
		{
			startWithFullJump = true;
		}

		
		public void SetStartWithDash()
		{
			startWithDash = true;
		}

		
		public void SetStartWithAttack()
		{
			startWithAttack = true;
		}

		
		public void SetSuperDashExit()
		{
			exitedSuperDashing = true;
		}

		
		public void SetQuakeExit()
		{
			exitedQuake = true;
		}

		
		public void SetTakeNoDamage()
		{
			takeNoDamage = true;
		}

		
		public void EndTakeNoDamage()
		{
			takeNoDamage = false;
		}

		
		public void SetHeroParent(Transform newParent)
		{
			transform.parent = newParent;
			if (newParent == null)
			{
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
			}
		}

		
		public void IsSwimming()
		{
			cState.swimming = true;
		}

		
		public void NotSwimming()
		{
			cState.swimming = false;
		}

		
		public void EnableRenderer()
		{
			renderer.enabled = true;
		}

		
		public void ResetAirMoves()
		{
			doubleJumped = false;
			airDashed = false;
		}

		
		public void SetConveyorSpeed(float speed)
		{
			conveyorSpeed = speed;
		}

		
		public void SetConveyorSpeedV(float speed)
		{
			conveyorSpeedV = speed;
		}

		
		public void EnterWithoutInput(bool flag)
		{
			enterWithoutInput = flag;
		}

		
		public void SetDarkness(int darkness)
		{
			if (darkness > 0 && playerData.GetBool("hasLantern"))
			{
				wieldingLantern = true;
			}
			else
			{
				wieldingLantern = false;
			}
		}

		
		public void CancelHeroJump()
		{
			if (cState.jumping)
			{
				CancelJump();
				CancelDoubleJump();
				if (rb2d.velocity.y > 0f)
				{
					rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
				}
			}
		}

		
		public void CharmUpdate()
		{
			orig_CharmUpdate();
			//ModHooks.Instance.OnCharmUpdate();
			playerData.UpdateBlueHealth();
		}

		
		public void checkEnvironment()
		{
			if (playerData.GetInt("environmentType") == 0)
			{
				footStepsRunAudioSource.clip = footstepsRunDust;
				footStepsWalkAudioSource.clip = footstepsWalkDust;
			}
			else if (playerData.GetInt("environmentType") == 1)
			{
				footStepsRunAudioSource.clip = footstepsRunGrass;
				footStepsWalkAudioSource.clip = footstepsWalkGrass;
			}
			else if (playerData.GetInt("environmentType") == 2)
			{
				footStepsRunAudioSource.clip = footstepsRunBone;
				footStepsWalkAudioSource.clip = footstepsWalkBone;
			}
			else if (playerData.GetInt("environmentType") == 3)
			{
				footStepsRunAudioSource.clip = footstepsRunSpa;
				footStepsWalkAudioSource.clip = footstepsWalkSpa;
			}
			else if (playerData.GetInt("environmentType") == 4)
			{
				footStepsRunAudioSource.clip = footstepsRunMetal;
				footStepsWalkAudioSource.clip = footstepsWalkMetal;
			}
			else if (playerData.GetInt("environmentType") == 6)
			{
				footStepsRunAudioSource.clip = footstepsRunWater;
				footStepsWalkAudioSource.clip = footstepsRunWater;
			}
			else if (playerData.GetInt("environmentType") == 7)
			{
				footStepsRunAudioSource.clip = footstepsRunGrass;
				footStepsWalkAudioSource.clip = footstepsWalkGrass;
			}
		}

		
		public void SetBenchRespawn(string spawnMarker, string sceneName, int spawnType, bool facingRight)
		{
			playerData.SetBenchRespawn(spawnMarker, sceneName, spawnType, facingRight);
		}

		
		public void SetHazardRespawn(Vector3 position, bool facingRight)
		{
			playerData.SetHazardRespawn(position, facingRight);
		}

		
		public void AddGeo(int amount)
		{
			playerData.AddGeo(amount);
			geoCounter.AddGeo(amount);
		}

		
		public void ToZero()
		{
			geoCounter.ToZero();
		}

		
		public void AddGeoQuietly(int amount)
		{
			playerData.AddGeo(amount);
		}

		
		public void AddGeoToCounter(int amount)
		{
			geoCounter.AddGeo(amount);
		}

		
		public void TakeGeo(int amount)
		{
			playerData.TakeGeo(amount);
			geoCounter.TakeGeo(amount);
		}

		
		public bool CanInput()
		{
			return acceptingInput;
		}

		
		public bool CanTalk()
		{
			bool result = false;
			if (CanInput() && hero_state != ActorStates.no_input && !controlReqlinquished && cState.onGround && !cState.attacking && !cState.dashing)
			{
				result = true;
			}
			return result;
		}

		
		public void FlipSprite()
		{
			cState.facingRight = !cState.facingRight;
			Vector3 localScale = transform.localScale;
			localScale.x *= -1f;
			transform.localScale = localScale;
		}

		
		public void NailParry()
		{
			parryInvulnTimer = INVUL_TIME_PARRY;
		}

		
		public void NailParryRecover()
		{
			attackDuration = 0f;
			attack_cooldown = 0f;
			CancelAttack();
		}

		
		public void QuakeInvuln()
		{
			parryInvulnTimer = INVUL_TIME_QUAKE;
		}

		
		public void CancelParryInvuln()
		{
			parryInvulnTimer = 0f;
		}

		
		public void CycloneInvuln()
		{
			parryInvulnTimer = INVUL_TIME_CYCLONE;
		}

		
		public void SetWieldingLantern(bool set)
		{
			wieldingLantern = set;
		}

		
		public void TakeDamage(GameObject go, CollisionSide damageSide, int damageAmount, int hazardType)
		{
			//damageAmount = ModHooks.Instance.OnTakeDamage(ref hazardType, damageAmount);
			bool spawnDamageEffect = true;
			bool flag = damageAmount > 0;
			if (flag)
			{
				bool isBossScene = BossSceneController.IsBossScene;
				if (isBossScene)
				{
					int bossLevel = BossSceneController.Instance.BossLevel;
					bool flag2 = bossLevel != 1;
					if (flag2)
					{
						bool flag3 = bossLevel == 2;
						if (flag3)
						{
							damageAmount = 9999;
						}
					}
					else
					{
						damageAmount *= 2;
					}
				}
				bool flag4 = CanTakeDamage();
				if (flag4)
				{
					bool flag5 = damageMode == DamageMode.HAZARD_ONLY && hazardType == 1;
					if (!flag5)
					{
						bool flag6 = cState.shadowDashing && hazardType == 1;
						if (!flag6)
						{
							bool flag7 = parryInvulnTimer > 0f && hazardType == 1;
							if (!flag7)
							{
								//VibrationMixer mixer = VibrationManager.GetMixer();
								bool flag8 = mixer != null;
								if (flag8)
								{
									mixer.StopAllEmissionsWithTag("heroAction");
								}
								bool flag9 = false;
								bool flag10 = carefreeShieldEquipped && hazardType == 1;
								if (flag10)
								{
									bool flag11 = hitsSinceShielded > 7;
									if (flag11)
									{
										hitsSinceShielded = 7;
									}
									switch (hitsSinceShielded)
									{
										case 1:
											{
												bool flag12 = (float)UnityEngine.Random.Range(1, 100) <= 10f;
												if (flag12)
												{
													flag9 = true;
												}
												break;
											}
										case 2:
											{
												bool flag13 = (float)UnityEngine.Random.Range(1, 100) <= 20f;
												if (flag13)
												{
													flag9 = true;
												}
												break;
											}
										case 3:
											{
												bool flag14 = (float)UnityEngine.Random.Range(1, 100) <= 30f;
												if (flag14)
												{
													flag9 = true;
												}
												break;
											}
										case 4:
											{
												bool flag15 = (float)UnityEngine.Random.Range(1, 100) <= 50f;
												if (flag15)
												{
													flag9 = true;
												}
												break;
											}
										case 5:
											{
												bool flag16 = (float)UnityEngine.Random.Range(1, 100) <= 70f;
												if (flag16)
												{
													flag9 = true;
												}
												break;
											}
										case 6:
											{
												bool flag17 = (float)UnityEngine.Random.Range(1, 100) <= 80f;
												if (flag17)
												{
													flag9 = true;
												}
												break;
											}
										case 7:
											{
												bool flag18 = (float)UnityEngine.Random.Range(1, 100) <= 90f;
												if (flag18)
												{
													flag9 = true;
												}
												break;
											}
										default:
											flag9 = false;
											break;
									}
									bool flag19 = flag9;
									if (flag19)
									{
										hitsSinceShielded = 0;
										carefreeShield.SetActive(true);
										damageAmount = 0;
										spawnDamageEffect = false;
									}
									else
									{
										hitsSinceShielded++;
									}
								}
								//damageAmount = ModHooks.Instance.AfterTakeDamage(hazardType, damageAmount);
								bool flag20 = playerData.GetBool("equippedCharm_5") && playerData.GetInt("blockerHits") > 0 && hazardType == 1 && cState.focusing && !flag9;
								if (flag20)
								{
									proxyFSM.SendEvent("HeroCtrl-TookBlockerHit");
									audioSource.PlayOneShot(blockerImpact, 1f);
									spawnDamageEffect = false;
									damageAmount = 0;
								}
								else
								{
									proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
								}
								CancelAttack();
								bool wallSliding = cState.wallSliding;
								if (wallSliding)
								{
									cState.wallSliding = false;
									//wallSlideVibrationPlayer.Stop();
								}
								bool touchingWall = cState.touchingWall;
								if (touchingWall)
								{
									cState.touchingWall = false;
								}
								bool flag21 = cState.recoilingLeft || cState.recoilingRight;
								if (flag21)
								{
									CancelRecoilHorizontal();
								}
								bool bouncing = cState.bouncing;
								if (bouncing)
								{
									CancelBounce();
									rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
								}
								bool shroomBouncing = cState.shroomBouncing;
								if (shroomBouncing)
								{
									CancelBounce();
									rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
								}
								bool flag22 = !flag9;
								if (flag22)
								{
									audioCtrl.PlaySound(HeroSounds.TAKE_HIT);
								}
								bool flag23 = !takeNoDamage && !playerData.GetBool("invinciTest");
								if (flag23)
								{
									bool @bool = playerData.GetBool("overcharmed");
									if (@bool)
									{
										playerData.TakeHealth(damageAmount * 2);
									}
									else
									{
										playerData.TakeHealth(damageAmount);
									}
								}
								bool flag24 = playerData.GetBool("equippedCharm_3") && damageAmount > 0;
								if (flag24)
								{
									bool bool2 = playerData.GetBool("equippedCharm_35");
									if (bool2)
									{
										AddMPCharge(GRUB_SOUL_MP_COMBO);
									}
									else
									{
										AddMPCharge(GRUB_SOUL_MP);
									}
								}
								bool flag25 = joniBeam && damageAmount > 0;
								if (flag25)
								{
									joniBeam = false;
								}
								bool flag26 = cState.nailCharging || nailChargeTimer != 0f;
								if (flag26)
								{
									cState.nailCharging = false;
									nailChargeTimer = 0f;
								}
								bool flag27 = damageAmount > 0 && OnTakenDamage != null;
								if (flag27)
								{
									OnTakenDamage();
								}
								bool flag28 = playerData.GetInt("health") == 0;
								if (flag28)
								{
									StartCoroutine(Die());
								}
								else
								{
									bool flag29 = hazardType == 2;
									if (flag29)
									{
										StartCoroutine(DieFromHazard(HazardType.Spikes, (!(go != null)) ? 0f : go.transform.rotation.z));
									}
									else
									{
										bool flag30 = hazardType == 3;
										if (flag30)
										{
											StartCoroutine(DieFromHazard(HazardType.Acid, 0f));
										}
										else
										{
											bool flag31 = hazardType == 4;
											if (flag31)
											{
												UnityEngine.Debug.Log("Lava death");
											}
											else
											{
												bool flag32 = hazardType == 5;
												if (flag32)
												{
													StartCoroutine(DieFromHazard(HazardType.Pit, 0f));
												}
												else
												{
													StartCoroutine(StartRecoil(damageSide, spawnDamageEffect, damageAmount));
												}
											}
										}
									}
								}
							}
						}
					}
				}
				else
				{
					bool flag33 = cState.invulnerable && !cState.hazardDeath && !playerData.GetBool("isInvincible");
					if (flag33)
					{
						bool flag34 = hazardType == 2;
						if (flag34)
						{
							bool flag35 = !takeNoDamage;
							if (flag35)
							{
								playerData.TakeHealth(damageAmount);
							}
							proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
							bool flag36 = playerData.GetInt("health") == 0;
							if (flag36)
							{
								StartCoroutine(Die());
							}
							else
							{
								audioCtrl.PlaySound(HeroSounds.TAKE_HIT);
								StartCoroutine(DieFromHazard(HazardType.Spikes, (!(go != null)) ? 0f : go.transform.rotation.z));
							}
						}
						else
						{
							bool flag37 = hazardType == 3;
							if (flag37)
							{
								playerData.TakeHealth(damageAmount);
								proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
								bool flag38 = playerData.GetInt("health") == 0;
								if (flag38)
								{
									StartCoroutine(Die());
								}
								else
								{
									StartCoroutine(DieFromHazard(HazardType.Acid, 0f));
								}
							}
							else
							{
								bool flag39 = hazardType == 4;
								if (flag39)
								{
									UnityEngine.Debug.Log("Lava damage");
								}
							}
						}
					}
				}
			}
		}

		
		public string GetEntryGateName()
		{
			if (sceneEntryGate != null)
			{
				return sceneEntryGate.name;
			}
			return string.Empty;
		}

		
		public void AddMPCharge(int amount)
		{
			int @int = playerData.GetInt("MPReserve");
			playerData.AddMPCharge(amount);
			GameCameras.instance.soulOrbFSM.SendEvent("MP GAIN");
			if (playerData.GetInt("MPReserve") != @int && gm && gm.soulVessel_fsm)
			{
				gm.soulVessel_fsm.SendEvent("MP RESERVE UP");
			}
		}

		
		public void SoulGain()
		{
			int @int = playerData.GetInt("MPCharge");
			bool flag = @int < playerData.GetInt("maxMP");
			int num;
			if (flag)
			{
				num = 11;
				bool @bool = playerData.GetBool("equippedCharm_20");
				if (@bool)
				{
					num += 3;
				}
				bool bool2 = playerData.GetBool("equippedCharm_21");
				if (bool2)
				{
					num += 8;
				}
			}
			else
			{
				num = 6;
				bool bool3 = playerData.GetBool("equippedCharm_20");
				if (bool3)
				{
					num += 2;
				}
				bool bool4 = playerData.GetBool("equippedCharm_21");
				if (bool4)
				{
					num += 6;
				}
			}
			int int2 = playerData.GetInt("MPReserve");
			//num = ModHooks.Instance.OnSoulGain(num);
			playerData.AddMPCharge(num);
			GameCameras.instance.soulOrbFSM.SendEvent("MP GAIN");
			bool flag2 = playerData.GetInt("MPReserve") != int2;
			if (flag2)
			{
				gm.soulVessel_fsm.SendEvent("MP RESERVE UP");
			}
		}

		
		public void AddMPChargeSpa(int amount)
		{
			TryAddMPChargeSpa(amount);
		}

		
		public bool TryAddMPChargeSpa(int amount)
		{
			int @int = playerData.GetInt("MPReserve");
			bool result = playerData.AddMPCharge(amount);
			gm.soulOrb_fsm.SendEvent("MP GAIN SPA");
			if (playerData.GetInt("MPReserve") != @int)
			{
				gm.soulVessel_fsm.SendEvent("MP RESERVE UP");
			}
			return result;
		}

		
		public void SetMPCharge(int amount)
		{
			playerData.SetIntSwappedArgs(amount, "MPCharge");
			GameCameras.instance.soulOrbFSM.SendEvent("MP SET");
		}

		
		public void TakeMP(int amount)
		{
			if (playerData.GetInt("MPCharge") > 0)
			{
				playerData.TakeMP(amount);
				if (amount > 1)
				{
					GameCameras.instance.soulOrbFSM.SendEvent("MP LOSE");
				}
			}
		}

		
		public void TakeMPQuick(int amount)
		{
			if (playerData.GetInt("MPCharge") > 0)
			{
				playerData.TakeMP(amount);
				if (amount > 1)
				{
					GameCameras.instance.soulOrbFSM.SendEvent("MP DRAIN");
				}
			}
		}

		
		public void TakeReserveMP(int amount)
		{
			playerData.TakeReserveMP(amount);
			gm.soulVessel_fsm.SendEvent("MP RESERVE DOWN");
		}

		
		public void AddHealth(int amount)
		{
			playerData.AddHealth(amount);
			proxyFSM.SendEvent("HeroCtrl-Healed");
		}

		
		public void TakeHealth(int amount)
		{
			playerData.TakeHealth(amount);
			proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
		}

		
		public void MaxHealth()
		{
			proxyFSM.SendEvent("HeroCtrl-MaxHealth");
			playerData.MaxHealth();
		}

		
		public void MaxHealthKeepBlue()
		{
			int @int = playerData.GetInt("healthBlue");
			playerData.MaxHealth();
			playerData.SetIntSwappedArgs(@int, "healthBlue");
			proxyFSM.SendEvent("HeroCtrl-Healed");
		}

		
		public void AddToMaxHealth(int amount)
		{
			playerData.AddToMaxHealth(amount);
			gm.AwardAchievement("PROTECTED");
			if (playerData.GetInt("maxHealthBase") == playerData.GetInt("maxHealthCap"))
			{
				gm.AwardAchievement("MASKED");
			}
		}

		
		public void ClearMP()
		{
			playerData.ClearMP();
		}

		
		public void ClearMPSendEvents()
		{
			ClearMP();
			GameManager.instance.soulOrb_fsm.SendEvent("MP LOSE");
			GameManager.instance.soulVessel_fsm.SendEvent("MP RESERVE DOWN");
		}

		
		public void AddToMaxMPReserve(int amount)
		{
			playerData.AddToMaxMPReserve(amount);
			gm.AwardAchievement("SOULFUL");
			if (playerData.GetInt("MPReserveMax") == playerData.GetInt("MPReserveCap"))
			{
				gm.AwardAchievement("WORLDSOUL");
			}
		}

		
		public void Bounce()
		{
			if (!cState.bouncing && !cState.shroomBouncing && !controlReqlinquished)
			{
				doubleJumped = false;
				airDashed = false;
				cState.bouncing = true;
			}
		}

		
		public void BounceHigh()
		{
			if (!cState.bouncing && !controlReqlinquished)
			{
				doubleJumped = false;
				airDashed = false;
				cState.bouncing = true;
				bounceTimer = -0.03f;
				rb2d.velocity = new Vector2(rb2d.velocity.x, BOUNCE_VELOCITY);
			}
		}

		
		public void ShroomBounce()
		{
			doubleJumped = false;
			airDashed = false;
			cState.bouncing = false;
			cState.shroomBouncing = true;
			rb2d.velocity = new Vector2(rb2d.velocity.x, SHROOM_BOUNCE_VELOCITY);
		}

		
		public void RecoilLeft()
		{
			if (!cState.recoilingLeft && !cState.recoilingRight && !playerData.GetBool("equippedCharm_14") && !controlReqlinquished)
			{
				CancelDash();
				recoilSteps = 0;
				cState.recoilingLeft = true;
				cState.recoilingRight = false;
				recoilLarge = false;
				rb2d.velocity = new Vector2(-RECOIL_HOR_VELOCITY, rb2d.velocity.y);
			}
		}

		
		public void RecoilRight()
		{
			if (!cState.recoilingLeft && !cState.recoilingRight && !playerData.GetBool("equippedCharm_14") && !controlReqlinquished)
			{
				CancelDash();
				recoilSteps = 0;
				cState.recoilingRight = true;
				cState.recoilingLeft = false;
				recoilLarge = false;
				rb2d.velocity = new Vector2(RECOIL_HOR_VELOCITY, rb2d.velocity.y);
			}
		}

		
		public void RecoilRightLong()
		{
			if (!cState.recoilingLeft && !cState.recoilingRight && !controlReqlinquished)
			{
				CancelDash();
				ResetAttacks();
				recoilSteps = 0;
				cState.recoilingRight = true;
				cState.recoilingLeft = false;
				recoilLarge = true;
				rb2d.velocity = new Vector2(RECOIL_HOR_VELOCITY_LONG, rb2d.velocity.y);
			}
		}

		
		public void RecoilLeftLong()
		{
			if (!cState.recoilingLeft && !cState.recoilingRight && !controlReqlinquished)
			{
				CancelDash();
				ResetAttacks();
				recoilSteps = 0;
				cState.recoilingRight = false;
				cState.recoilingLeft = true;
				recoilLarge = true;
				rb2d.velocity = new Vector2(-RECOIL_HOR_VELOCITY_LONG, rb2d.velocity.y);
			}
		}

		
		public void RecoilDown()
		{
			CancelJump();
			if (rb2d.velocity.y > RECOIL_DOWN_VELOCITY && !controlReqlinquished)
			{
				rb2d.velocity = new Vector2(rb2d.velocity.x, RECOIL_DOWN_VELOCITY);
			}
		}

		
		public void ForceHardLanding()
		{
			if (!cState.onGround)
			{
				cState.willHardLand = true;
			}
		}

		
		public void EnterSceneDreamGate()
		{
			IgnoreInputWithoutReset();
			ResetMotion();
			airDashed = false;
			doubleJumped = false;
			ResetHardLandingTimer();
			ResetAttacksDash();
			AffectedByGravity(false);
			sceneEntryGate = null;
			SetState(ActorStates.no_input);
			transitionState = HeroTransitionState.WAITING_TO_ENTER_LEVEL;
			vignetteFSM.SendEvent("RESET");
			if (heroInPosition != null)
			{
				heroInPosition(false);
			}
			FinishedEnteringScene(true, false);
		}

		
		public IEnumerator EnterScene(TransitionPoint enterGate, float delayBeforeEnter)
		{
			IgnoreInputWithoutReset();
			ResetMotion();
			airDashed = false;
			doubleJumped = false;
			ResetHardLandingTimer();
			ResetAttacksDash();
			AffectedByGravity(false);
			sceneEntryGate = enterGate;
			SetState(ActorStates.no_input);
			transitionState = HeroTransitionState.WAITING_TO_ENTER_LEVEL;
			vignetteFSM.SendEvent("RESET");
			if (!cState.transitioning)
			{
				cState.transitioning = true;
			}
			gatePosition = enterGate.GetGatePosition();
			if (gatePosition == GatePosition.top)
			{
				cState.onGround = false;
				enteringVertically = true;
				exitedSuperDashing = false;
				float x = enterGate.transform.position.x + enterGate.entryOffset.x;
				float y = enterGate.transform.position.y + enterGate.entryOffset.y;
				transform.SetPosition(x, y);
				if (heroInPosition != null)
				{
					heroInPosition(false);
				}
				yield return new WaitForSeconds(0.165f);
				if (!enterGate.customFade)
				{
					gm.FadeSceneIn();
				}
				if (delayBeforeEnter > 0f)
				{
					yield return new WaitForSeconds(delayBeforeEnter);
				}
				if (enterGate.entryDelay > 0f)
				{
					yield return new WaitForSeconds(enterGate.entryDelay);
				}
				yield return new WaitForSeconds(0.4f);
				if (exitedQuake)
				{
					IgnoreInput();
					proxyFSM.SendEvent("HeroCtrl-EnterQuake");
					yield return new WaitForSeconds(0.25f);
					FinishedEnteringScene(true, false);
				}
				else
				{
					rb2d.velocity = new Vector2(0f, SPEED_TO_ENTER_SCENE_DOWN);
					transitionState = HeroTransitionState.ENTERING_SCENE;
					transitionState = HeroTransitionState.DROPPING_DOWN;
					AffectedByGravity(true);
					if (enterGate.hardLandOnExit)
					{
						cState.willHardLand = true;
					}
					yield return new WaitForSeconds(0.33f);
					transitionState = HeroTransitionState.ENTERING_SCENE;
					if (transitionState != HeroTransitionState.WAITING_TO_TRANSITION)
					{
						FinishedEnteringScene(true, false);
					}
				}
			}
			else if (gatePosition == GatePosition.bottom)
			{
				cState.onGround = false;
				enteringVertically = true;
				exitedSuperDashing = false;
				if (enterGate.alwaysEnterRight)
				{
					FaceRight();
				}
				if (enterGate.alwaysEnterLeft)
				{
					FaceLeft();
				}
				float x2 = enterGate.transform.position.x + enterGate.entryOffset.x;
				float y2 = enterGate.transform.position.y + enterGate.entryOffset.y + 3f;
				transform.SetPosition(x2, y2);
				if (heroInPosition != null)
				{
					heroInPosition(false);
				}
				yield return new WaitForSeconds(0.165f);
				if (delayBeforeEnter > 0f)
				{
					yield return new WaitForSeconds(delayBeforeEnter);
				}
				if (enterGate.entryDelay > 0f)
				{
					yield return new WaitForSeconds(enterGate.entryDelay);
				}
				yield return new WaitForSeconds(0.4f);
				if (!enterGate.customFade)
				{
					gm.FadeSceneIn();
				}
				if (cState.facingRight)
				{
					transition_vel = new Vector2(SPEED_TO_ENTER_SCENE_HOR, SPEED_TO_ENTER_SCENE_UP);
				}
				else
				{
					transition_vel = new Vector2(-SPEED_TO_ENTER_SCENE_HOR, SPEED_TO_ENTER_SCENE_UP);
				}
				transitionState = HeroTransitionState.ENTERING_SCENE;
				transform.SetPosition(x2, y2);
				yield return new WaitForSeconds(TIME_TO_ENTER_SCENE_BOT);
				transition_vel = new Vector2(rb2d.velocity.x, 0f);
				AffectedByGravity(true);
				transitionState = HeroTransitionState.DROPPING_DOWN;
			}
			else if (gatePosition == GatePosition.left)
			{
				cState.onGround = true;
				enteringVertically = false;
				SetState(ActorStates.no_input);
				float x3 = enterGate.transform.position.x + enterGate.entryOffset.x;
				float y3 = FindGroundPointY(x3 + 2f, enterGate.transform.position.y, false);
				transform.SetPosition(x3, y3);
				if (heroInPosition != null)
				{
					heroInPosition(true);
				}
				FaceRight();
				yield return new WaitForSeconds(0.165f);
				if (!enterGate.customFade)
				{
					gm.FadeSceneIn();
				}
				if (delayBeforeEnter > 0f)
				{
					yield return new WaitForSeconds(delayBeforeEnter);
				}
				if (enterGate.entryDelay > 0f)
				{
					yield return new WaitForSeconds(enterGate.entryDelay);
				}
				yield return new WaitForSeconds(0.4f);
				if (exitedSuperDashing)
				{
					IgnoreInput();
					proxyFSM.SendEvent("HeroCtrl-EnterSuperDash");
					yield return new WaitForSeconds(0.25f);
					FinishedEnteringScene(true, false);
				}
				else
				{
					transition_vel = new Vector2(RUN_SPEED, 0f);
					transitionState = HeroTransitionState.ENTERING_SCENE;
					yield return new WaitForSeconds(0.33f);
					FinishedEnteringScene(true, true);
				}
			}
			else if (gatePosition == GatePosition.right)
			{
				cState.onGround = true;
				enteringVertically = false;
				SetState(ActorStates.no_input);
				float x4 = enterGate.transform.position.x + enterGate.entryOffset.x;
				float y4 = FindGroundPointY(x4 - 2f, enterGate.transform.position.y, false);
				transform.SetPosition(x4, y4);
				if (heroInPosition != null)
				{
					heroInPosition(true);
				}
				FaceLeft();
				yield return new WaitForSeconds(0.165f);
				if (!enterGate.customFade)
				{
					gm.FadeSceneIn();
				}
				if (delayBeforeEnter > 0f)
				{
					yield return new WaitForSeconds(delayBeforeEnter);
				}
				if (enterGate.entryDelay > 0f)
				{
					yield return new WaitForSeconds(enterGate.entryDelay);
				}
				yield return new WaitForSeconds(0.4f);
				if (exitedSuperDashing)
				{
					IgnoreInput();
					proxyFSM.SendEvent("HeroCtrl-EnterSuperDash");
					yield return new WaitForSeconds(0.25f);
					FinishedEnteringScene(true, false);
				}
				else
				{
					transition_vel = new Vector2(-RUN_SPEED, 0f);
					transitionState = HeroTransitionState.ENTERING_SCENE;
					yield return new WaitForSeconds(0.33f);
					FinishedEnteringScene(true, true);
				}
			}
			else if (gatePosition == GatePosition.door)
			{
				if (enterGate.alwaysEnterRight)
				{
					FaceRight();
				}
				if (enterGate.alwaysEnterLeft)
				{
					FaceLeft();
				}
				cState.onGround = true;
				enteringVertically = false;
				SetState(ActorStates.idle);
				SetState(ActorStates.no_input);
				exitedSuperDashing = false;
				animCtrl.PlayClip("Idle");
				transform.SetPosition(FindGroundPoint(enterGate.transform.position, false));
				if (heroInPosition != null)
				{
					heroInPosition(false);
				}
				yield return new WaitForEndOfFrame();
				if (delayBeforeEnter > 0f)
				{
					yield return new WaitForSeconds(delayBeforeEnter);
				}
				if (enterGate.entryDelay > 0f)
				{
					yield return new WaitForSeconds(enterGate.entryDelay);
				}
				yield return new WaitForSeconds(0.4f);
				if (!enterGate.customFade)
				{
					gm.FadeSceneIn();
				}
				float startTime = Time.realtimeSinceStartup;
				if (enterGate.dontWalkOutOfDoor)
				{
					yield return new WaitForSeconds(0.33f);
				}
				else
				{
					float clipLength = animCtrl.GetClipDuration("Exit Door To Idle");
					animCtrl.PlayClip("Exit Door To Idle");
					if (clipLength > 0f)
					{
						yield return new WaitForSeconds(clipLength);
					}
					else
					{
						yield return new WaitForSeconds(0.33f);
					}
				}
				FinishedEnteringScene(true, false);
			}
			yield break;
		}

		
		public void LeaveScene(GatePosition? gate = null)
		{
			isHeroInPosition = false;
			IgnoreInputWithoutReset();
			ResetHardLandingTimer();
			SetState(ActorStates.no_input);
			SetDamageMode(DamageMode.NO_DAMAGE);
			transitionState = HeroTransitionState.EXITING_SCENE;
			CancelFallEffects();
			tilemapTestActive = false;
			SetHeroParent(null);
			StopTilemapTest();
			if (gate != null)
			{
				switch (gate.Value)
				{
					case GatePosition.top:
						transition_vel = new Vector2(0f, MIN_JUMP_SPEED);
						cState.onGround = false;
						break;
					case GatePosition.right:
						transition_vel = new Vector2(RUN_SPEED, 0f);
						break;
					case GatePosition.left:
						transition_vel = new Vector2(-RUN_SPEED, 0f);
						break;
					case GatePosition.bottom:
						transition_vel = Vector2.zero;
						cState.onGround = false;
						break;
				}
			}
			cState.transitioning = true;
		}

		
		public IEnumerator Respawn()
		{
			playerData = PlayerData.instance;
			playerData.disablePause = true;
			gameObject.layer = 9;
			renderer.enabled = true;
			rb2d.isKinematic = false;
			cState.dead = false;
			cState.onGround = true;
			cState.hazardDeath = false;
			cState.recoiling = false;
			enteringVertically = false;
			airDashed = false;
			doubleJumped = false;
			CharmUpdate();
			MaxHealth();
			ClearMP();
			ResetMotion();
			ResetHardLandingTimer();
			ResetAttacks();
			ResetInput();
			CharmUpdate();
			Transform spawnPoint = LocateSpawnPoint();
			if (spawnPoint != null)
			{
				transform.SetPosition(FindGroundPoint(spawnPoint.transform.position, false));
				PlayMakerFSM component = spawnPoint.GetComponent<PlayMakerFSM>();
				if (component != null)
				{
					Vector3 vector = FSMUtility.GetVector3(component, "Adjust Vector");
				}
				else if (verboseMode)
				{
					UnityEngine.Debug.Log("Could not find Bench Control FSM on respawn point. Ignoring Adjustment offset.");
				}
			}
			else
			{
				UnityEngine.Debug.LogError("Couldn't find the respawn point named " + playerData.respawnMarkerName + " within objects tagged with RespawnPoint");
			}
			if (verboseMode)
			{
				UnityEngine.Debug.Log("HC Respawn Type: " + playerData.respawnType);
			}
			GameCameras.instance.cameraFadeFSM.SendEvent("RESPAWN");
			if (playerData.respawnType == 1)
			{
				AffectedByGravity(false);
				PlayMakerFSM benchFSM = FSMUtility.LocateFSM(spawnPoint.gameObject, "Bench Control");
				if (benchFSM == null)
				{
					UnityEngine.Debug.LogError("HeroCtrl: Could not find Bench Control FSM on this spawn point, respawn type is set to Bench");
					yield break;
				}
				benchFSM.FsmVariables.GetFsmBool("RespawnResting").Value = true;
				yield return new WaitForEndOfFrame();
				if (heroInPosition != null)
				{
					heroInPosition(false);
				}
				proxyFSM.SendEvent("HeroCtrl-Respawned");
				FinishedEnteringScene(true, false);
				benchFSM.SendEvent("RESPAWN");
			}
			else
			{
				yield return new WaitForEndOfFrame();
				IgnoreInput();
				RespawnMarker respawnMarker = spawnPoint.GetComponent<RespawnMarker>();
				if (respawnMarker)
				{
					if (respawnMarker.respawnFacingRight)
					{
						FaceRight();
					}
					else
					{
						FaceLeft();
					}
				}
				else
				{
					UnityEngine.Debug.LogError("Spawn point does not contain a RespawnMarker");
				}
				if (heroInPosition != null)
				{
					heroInPosition(false);
				}
				if (gm.GetSceneNameString() != "GG_Atrium")
				{
					float clipLength = animCtrl.GetClipDuration("Wake Up Ground");
					animCtrl.PlayClip("Wake Up Ground");
					StopAnimationControl();
					controlReqlinquished = true;
					yield return new WaitForSeconds(clipLength);
					StartAnimationControl();
					controlReqlinquished = false;
				}
				proxyFSM.SendEvent("HeroCtrl-Respawned");
				FinishedEnteringScene(true, false);
			}
			playerData.disablePause = false;
			playerData.isInvincible = false;
			yield break;
		}

		
		public IEnumerator HazardRespawn()
		{
			cState.hazardDeath = false;
			cState.onGround = true;
			cState.hazardRespawning = true;
			ResetMotion();
			ResetHardLandingTimer();
			ResetAttacks();
			ResetInput();
			cState.recoiling = false;
			enteringVertically = false;
			airDashed = false;
			doubleJumped = false;
			transform.SetPosition(FindGroundPoint(playerData.hazardRespawnLocation, true));
			gameObject.layer = 9;
			renderer.enabled = true;
			yield return new WaitForEndOfFrame();
			if (playerData.hazardRespawnFacingRight)
			{
				FaceRight();
			}
			else
			{
				FaceLeft();
			}
			if (heroInPosition != null)
			{
				heroInPosition(false);
			}
			StartCoroutine(Invulnerable(INVUL_TIME * 2f));
			GameCameras.instance.cameraFadeFSM.SendEvent("RESPAWN");
			float clipLength = animCtrl.GetClipDuration("Hazard Respawn");
			animCtrl.PlayClip("Hazard Respawn");
			yield return new WaitForSeconds(clipLength);
			cState.hazardRespawning = false;
			rb2d.interpolation = RigidbodyInterpolation2D.Interpolate;
			FinishedEnteringScene(false, false);
			yield break;
		}

		
		public void StartCyclone()
		{
			nailArt_cyclone = true;
		}

		
		public void EndCyclone()
		{
			nailArt_cyclone = false;
		}

		
		public bool GetState(string stateName)
		{
			return cState.GetState(stateName);
		}

		
		public bool GetCState(string stateName)
		{
			return cState.GetState(stateName);
		}

		
		public void SetCState(string stateName, bool value)
		{
			cState.SetState(stateName, value);
		}

		
		public void ResetHardLandingTimer()
		{
			cState.willHardLand = false;
			hardLandingTimer = 0f;
			fallTimer = 0f;
			hardLanded = false;
		}

		
		public void CancelSuperDash()
		{
			superDash.SendEvent("SLOPE CANCEL");
		}

		
		public void RelinquishControlNotVelocity()
		{
			if (!controlReqlinquished)
			{
				prev_hero_state = ActorStates.idle;
				ResetInput();
				ResetMotionNotVelocity();
				SetState(ActorStates.no_input);
				IgnoreInput();
				controlReqlinquished = true;
				ResetLook();
				ResetAttacks();
				touchingWallL = false;
				touchingWallR = false;
			}
		}

		
		public void RelinquishControl()
		{
			if (!controlReqlinquished && !cState.dead)
			{
				ResetInput();
				ResetMotion();
				IgnoreInput();
				controlReqlinquished = true;
				ResetLook();
				ResetAttacks();
				touchingWallL = false;
				touchingWallR = false;
			}
		}

		
		public void RegainControl()
		{
			enteringVertically = false;
			doubleJumpQueuing = false;
			AcceptInput();
			hero_state = ActorStates.idle;
			if (controlReqlinquished && !cState.dead)
			{
				AffectedByGravity(true);
				SetStartingMotionState();
				controlReqlinquished = false;
				if (startWithWallslide)
				{
					//wallSlideVibrationPlayer.Play();
					cState.wallSliding = true;
					cState.willHardLand = false;
					cState.touchingWall = true;
					airDashed = false;
					wallslideDustPrefab.enableEmission = true;
					startWithWallslide = false;
					if (transform.localScale.x < 0f)
					{
						wallSlidingR = true;
						touchingWallR = true;
					}
					else
					{
						wallSlidingL = true;
						touchingWallL = true;
					}
				}
				else if (startWithJump)
				{
					HeroJumpNoEffect();
					doubleJumpQueuing = false;
					startWithJump = false;
				}
				else if (startWithFullJump)
				{
					HeroJump();
					doubleJumpQueuing = false;
					startWithFullJump = false;
				}
				else if (startWithDash)
				{
					HeroDash();
					doubleJumpQueuing = false;
					startWithDash = false;
				}
				else if (startWithAttack)
				{
					DoAttack();
					doubleJumpQueuing = false;
					startWithAttack = false;
				}
				else
				{
					cState.touchingWall = false;
					touchingWallL = false;
					touchingWallR = false;
				}
			}
		}

		
		public void PreventCastByDialogueEnd()
		{
			preventCastByDialogueEndTimer = 0.3f;
		}

		
		public bool CanCast()
		{
			return !gm.isPaused && !cState.dashing && hero_state != ActorStates.no_input && !cState.backDashing && (!cState.attacking || attack_time >= ATTACK_RECOVERY_TIME) && !cState.recoiling && !cState.recoilFrozen && !cState.transitioning && !cState.hazardDeath && !cState.hazardRespawning && CanInput() && preventCastByDialogueEndTimer <= 0f;
		}

		
		public bool CanFocus()
		{
			return !gm.isPaused && hero_state != ActorStates.no_input && !cState.dashing && !cState.backDashing && (!cState.attacking || attack_time >= ATTACK_RECOVERY_TIME) && !cState.recoiling && cState.onGround && !cState.transitioning && !cState.recoilFrozen && !cState.hazardDeath && !cState.hazardRespawning && CanInput();
		}

		
		public bool CanNailArt()
		{
			if (!cState.transitioning && hero_state != ActorStates.no_input && !cState.attacking && !cState.hazardDeath && !cState.hazardRespawning && nailChargeTimer >= nailChargeTime)
			{
				nailChargeTimer = 0f;
				return true;
			}
			nailChargeTimer = 0f;
			return false;
		}

		
		public bool CanQuickMap()
		{
			return !gm.isPaused && !controlReqlinquished && hero_state != ActorStates.no_input && !cState.onConveyor && !cState.dashing && !cState.backDashing && (!cState.attacking || attack_time >= ATTACK_RECOVERY_TIME) && !cState.recoiling && !cState.transitioning && !cState.hazardDeath && !cState.hazardRespawning && !cState.recoilFrozen && cState.onGround && CanInput();
		}

		
		public bool CanInspect()
		{
			return !gm.isPaused && !cState.dashing && hero_state != ActorStates.no_input && !cState.backDashing && (!cState.attacking || attack_time >= ATTACK_RECOVERY_TIME) && !cState.recoiling && !cState.transitioning && !cState.hazardDeath && !cState.hazardRespawning && !cState.recoilFrozen && cState.onGround && CanInput();
		}

		
		public bool CanBackDash()
		{
			return !gm.isPaused && !cState.dashing && hero_state != ActorStates.no_input && !cState.backDashing && (!cState.attacking || attack_time >= ATTACK_RECOVERY_TIME) && !cState.preventBackDash && !cState.backDashCooldown && !controlReqlinquished && !cState.recoilFrozen && !cState.recoiling && !cState.transitioning && cState.onGround && playerData.GetBool("canBackDash");
		}

		
		public bool CanSuperDash()
		{
			return !gm.isPaused && hero_state != ActorStates.no_input && !cState.dashing && !cState.hazardDeath && !cState.hazardRespawning && !cState.backDashing && (!cState.attacking || attack_time >= ATTACK_RECOVERY_TIME) && !cState.slidingLeft && !cState.slidingRight && !controlReqlinquished && !cState.recoilFrozen && !cState.recoiling && !cState.transitioning && playerData.GetBool("hasSuperDash") && (cState.onGround || cState.wallSliding);
		}

		
		public bool CanDreamNail()
		{
			return !gm.isPaused && hero_state != ActorStates.no_input && !cState.dashing && !cState.backDashing && (!cState.attacking || attack_time >= ATTACK_RECOVERY_TIME) && !controlReqlinquished && !cState.hazardDeath && rb2d.velocity.y > -0.1f && !cState.hazardRespawning && !cState.recoilFrozen && !cState.recoiling && !cState.transitioning && playerData.GetBool("hasDreamNail") && cState.onGround;
		}

		
		public bool CanDreamGate()
		{
			return !gm.isPaused && hero_state != ActorStates.no_input && !cState.dashing && !cState.backDashing && (!cState.attacking || attack_time >= ATTACK_RECOVERY_TIME) && !controlReqlinquished && !cState.hazardDeath && !cState.hazardRespawning && !cState.recoilFrozen && !cState.recoiling && !cState.transitioning && playerData.GetBool("hasDreamGate") && cState.onGround;
		}

		
		public bool CanInteract()
		{
			return CanInput() && hero_state != ActorStates.no_input && !gm.isPaused && !cState.dashing && !cState.backDashing && !cState.attacking && !controlReqlinquished && !cState.hazardDeath && !cState.hazardRespawning && !cState.recoilFrozen && !cState.recoiling && !cState.transitioning && cState.onGround;
		}

		
		public bool CanOpenInventory()
		{
			return (!gm.isPaused && hero_state != ActorStates.airborne && !controlReqlinquished && !cState.recoiling && !cState.transitioning && !cState.hazardDeath && !cState.hazardRespawning && !playerData.GetBool("disablePause") && CanInput()) || playerData.GetBool("atBench");
		}

		
		public void SetDamageMode(int invincibilityType)
		{
			if (invincibilityType != 0)
			{
				if (invincibilityType != 1)
				{
					if (invincibilityType == 2)
					{
						damageMode = DamageMode.NO_DAMAGE;
					}
				}
				else
				{
					damageMode = DamageMode.HAZARD_ONLY;
				}
			}
			else
			{
				damageMode = DamageMode.FULL_DAMAGE;
			}
		}

		
		public void SetDamageModeFSM(int invincibilityType)
		{
			if (invincibilityType != 0)
			{
				if (invincibilityType != 1)
				{
					if (invincibilityType == 2)
					{
						damageMode = DamageMode.NO_DAMAGE;
					}
				}
				else
				{
					damageMode = DamageMode.HAZARD_ONLY;
				}
			}
			else
			{
				damageMode = DamageMode.FULL_DAMAGE;
			}
		}

		
		public void ResetQuakeDamage()
		{
			if (damageMode == DamageMode.HAZARD_ONLY)
			{
				damageMode = DamageMode.FULL_DAMAGE;
			}
		}

		
		public void SetDamageMode(DamageMode newDamageMode)
		{
			damageMode = newDamageMode;
			if (newDamageMode == DamageMode.NO_DAMAGE)
			{
				playerData.SetBoolSwappedArgs(true, "isInvincible");
			}
			else
			{
				playerData.SetBoolSwappedArgs(false, "isInvincible");
			}
		}

		
		public void StopAnimationControl()
		{
			animCtrl.StopControl();
		}

		
		public void StartAnimationControl()
		{
			animCtrl.StartControl();
		}

		
		public void IgnoreInput()
		{
			if (acceptingInput)
			{
				acceptingInput = false;
				ResetInput();
			}
		}

		
		public void IgnoreInputWithoutReset()
		{
			if (acceptingInput)
			{
				acceptingInput = false;
			}
		}

		
		public void AcceptInput()
		{
			acceptingInput = true;
		}

		
		public void Pause()
		{
			PauseInput();
			PauseAudio();
			JumpReleased();
			cState.isPaused = true;
		}

		
		public void UnPause()
		{
			cState.isPaused = false;
			UnPauseAudio();
			UnPauseInput();
		}

		
		public void NearBench(bool isNearBench)
		{
			cState.nearBench = isNearBench;
		}

		
		public void SetWalkZone(bool inWalkZone)
		{
			cState.inWalkZone = inWalkZone;
		}

		
		public void ResetState()
		{
			cState.Reset();
		}

		
		public void StopPlayingAudio()
		{
			audioCtrl.StopAllSounds();
		}

		
		public void PauseAudio()
		{
			audioCtrl.PauseAllSounds();
		}

		
		public void UnPauseAudio()
		{
			audioCtrl.UnPauseAllSounds();
		}

		
		private void PauseInput()
		{
			if (acceptingInput)
			{
				acceptingInput = false;
			}
			lastInputState = new Vector2(move_input, vertical_input);
		}

		
		private void UnPauseInput()
		{
			if (!controlReqlinquished)
			{
				if (inputHandler.inputActions.right.IsPressed)
				{
					move_input = lastInputState.x;
				}
				else if (inputHandler.inputActions.left.IsPressed)
				{
					move_input = lastInputState.x;
				}
				else
				{
					rb2d.velocity = new Vector2(0f, rb2d.velocity.y);
					move_input = 0f;
				}
				vertical_input = lastInputState.y;
				acceptingInput = true;
			}
		}

		
		public void SpawnSoftLandingPrefab()
		{
			GameObject.Instantiate(softLandingEffectPrefab,transform.position,Quaternion.identity);
		}

		
		public void AffectedByGravity(bool gravityApplies)
		{
			float gravityScale = rb2d.gravityScale;
			if (rb2d.gravityScale > Mathf.Epsilon && !gravityApplies)
			{
				prevGravityScale = rb2d.gravityScale;
				rb2d.gravityScale = 0f;
			}
			else if (rb2d.gravityScale <= Mathf.Epsilon && gravityApplies)
			{
				rb2d.gravityScale = prevGravityScale;
				prevGravityScale = 0f;
			}
		}

		
		private void LookForInput()
		{
			if (acceptingInput && !gm.isPaused && isGameplayScene)
			{
				move_input = inputHandler.inputActions.moveVector.Vector.x;
				vertical_input = inputHandler.inputActions.moveVector.Vector.y;
				FilterInput();
				if (playerData.GetBool("hasWalljump") && CanWallSlide() && !cState.attacking)
				{
					if (touchingWallL && inputHandler.inputActions.left.IsPressed && !cState.wallSliding)
					{
						airDashed = false;
						doubleJumped = false;
						//wallSlideVibrationPlayer.Play();
						cState.wallSliding = true;
						cState.willHardLand = false;
						wallslideDustPrefab.enableEmission = true;
						wallSlidingL = true;
						wallSlidingR = false;
						FaceLeft();
						CancelFallEffects();
					}
					if (touchingWallR && inputHandler.inputActions.right.IsPressed && !cState.wallSliding)
					{
						airDashed = false;
						doubleJumped = false;
						//wallSlideVibrationPlayer.Play();
						cState.wallSliding = true;
						cState.willHardLand = false;
						wallslideDustPrefab.enableEmission = true;
						wallSlidingL = false;
						wallSlidingR = true;
						FaceRight();
						CancelFallEffects();
					}
				}
				if (cState.wallSliding && inputHandler.inputActions.down.WasPressed)
				{
					CancelWallsliding();
					FlipSprite();
				}
				if (wallLocked && wallJumpedL && inputHandler.inputActions.right.IsPressed && wallLockSteps >= WJLOCK_STEPS_SHORT)
				{
					wallLocked = false;
				}
				if (wallLocked && wallJumpedR && inputHandler.inputActions.left.IsPressed && wallLockSteps >= WJLOCK_STEPS_SHORT)
				{
					wallLocked = false;
				}
				if (inputHandler.inputActions.jump.WasReleased && jumpReleaseQueueingEnabled)
				{
					jumpReleaseQueueSteps = JUMP_RELEASE_QUEUE_STEPS;
					jumpReleaseQueuing = true;
				}
				if (!inputHandler.inputActions.jump.IsPressed)
				{
					JumpReleased();
				}
				if (!inputHandler.inputActions.dash.IsPressed)
				{
					if (cState.preventDash && !cState.dashCooldown)
					{
						cState.preventDash = false;
					}
					dashQueuing = false;
				}
				if (!inputHandler.inputActions.attack.IsPressed)
				{
					attackQueuing = false;
				}
			}
		}

		
		private void LookForQueueInput()
		{
			bool flag = acceptingInput && !gm.isPaused && isGameplayScene;
			if (flag)
			{
				bool wasPressed = inputHandler.inputActions.jump.WasPressed;
				if (wasPressed)
				{
					bool flag2 = CanWallJump();
					if (flag2)
					{
						DoWallJump();
					}
					else
					{
						bool flag3 = CanJump();
						if (flag3)
						{
							HeroJump();
						}
						else
						{
							bool flag4 = CanDoubleJump();
							if (flag4)
							{
								DoDoubleJump();
							}
							else
							{
								bool flag5 = CanInfiniteAirJump();
								if (flag5)
								{
									CancelJump();
									audioCtrl.PlaySound(HeroSounds.JUMP);
									ResetLook();
									cState.jumping = true;
								}
								else
								{
									jumpQueueSteps = 0;
									jumpQueuing = true;
									doubleJumpQueueSteps = 0;
									doubleJumpQueuing = true;
								}
							}
						}
					}
				}
				bool flag6 = inputHandler.inputActions.dash.WasPressed;// && !ModHooks.Instance.OnDashPressed();
				if (flag6)
				{
					bool flag7 = CanDash();
					if (flag7)
					{
						HeroDash();
					}
					else
					{
						dashQueueSteps = 0;
						dashQueuing = true;
					}
				}
				bool wasPressed2 = inputHandler.inputActions.attack.WasPressed;
				if (wasPressed2)
				{
					bool flag8 = CanAttack();
					if (flag8)
					{
						DoAttack();
					}
					else
					{
						attackQueueSteps = 0;
						attackQueuing = true;
					}
				}
				bool isPressed = inputHandler.inputActions.jump.IsPressed;
				if (isPressed)
				{
					bool flag9 = jumpQueueSteps <= JUMP_QUEUE_STEPS && CanJump() && jumpQueuing;
					if (flag9)
					{
						HeroJump();
					}
					else
					{
						bool flag10 = doubleJumpQueueSteps <= DOUBLE_JUMP_QUEUE_STEPS && CanDoubleJump() && doubleJumpQueuing;
						if (flag10)
						{
							bool onGround = cState.onGround;
							if (onGround)
							{
								HeroJump();
							}
							else
							{
								DoDoubleJump();
							}
						}
					}
					bool flag11 = CanSwim();
					if (flag11)
					{
						bool flag12 = hero_state != ActorStates.airborne;
						if (flag12)
						{
							SetState(ActorStates.airborne);
						}
						cState.swimming = true;
					}
				}
				bool flag13 = inputHandler.inputActions.dash.IsPressed && dashQueueSteps <= DASH_QUEUE_STEPS && CanDash() && dashQueuing && CanDash();
				if (flag13)
				{
					HeroDash();
				}
				bool flag14 = inputHandler.inputActions.attack.IsPressed && attackQueueSteps <= ATTACK_QUEUE_STEPS && CanAttack() && attackQueuing;
				if (flag14)
				{
					DoAttack();
				}
			}
		}

		
		private void HeroJump()
		{
			GameObject.Instantiate(jumpEffectPrefab,transform.position,Quaternion.identity);
			audioCtrl.PlaySound(HeroSounds.JUMP);
			ResetLook();
			cState.recoiling = false;
			cState.jumping = true;
			jumpQueueSteps = 0;
			jumped_steps = 0;
			doubleJumpQueuing = false;
		}

		
		private void HeroJumpNoEffect()
		{
			ResetLook();
			jump_steps = 5;
			cState.jumping = true;
			jumpQueueSteps = 0;
			jumped_steps = 0;
			jump_steps = 5;
		}

		
		private void DoWallJump()
		{
			wallPuffPrefab.SetActive(true);
			audioCtrl.PlaySound(HeroSounds.WALLJUMP);
			//VibrationManager.PlayVibrationClipOneShot(wallJumpVibration, null, false, string.Empty);
			if (touchingWallL)
			{
				FaceRight();
				wallJumpedR = true;
				wallJumpedL = false;
			}
			else if (touchingWallR)
			{
				FaceLeft();
				wallJumpedR = false;
				wallJumpedL = true;
			}
			CancelWallsliding();
			cState.touchingWall = false;
			touchingWallL = false;
			touchingWallR = false;
			airDashed = false;
			doubleJumped = false;
			currentWalljumpSpeed = WJ_KICKOFF_SPEED;
			walljumpSpeedDecel = (WJ_KICKOFF_SPEED - RUN_SPEED) / (float)WJLOCK_STEPS_LONG;
			dashBurst.SendEvent("CANCEL");
			cState.jumping = true;
			wallLockSteps = 0;
			wallLocked = true;
			jumpQueueSteps = 0;
			jumped_steps = 0;
		}

		
		private void DoDoubleJump()
		{
			dJumpWingsPrefab.SetActive(true);
			dJumpFlashPrefab.SetActive(true);
			dJumpFeathers.Play();
			//VibrationManager.PlayVibrationClipOneShot(doubleJumpVibration, null, false, string.Empty);
			audioSource.PlayOneShot(doubleJumpClip, 1f);
			ResetLook();
			cState.jumping = false;
			cState.doubleJumping = true;
			doubleJump_steps = 0;
			doubleJumped = true;
		}

		
		private void DoHardLanding()
		{
			AffectedByGravity(true);
			ResetInput();
			SetState(ActorStates.hard_landing);
			CancelAttack();
			hardLanded = true;
			audioCtrl.PlaySound(HeroSounds.HARD_LANDING);
			GameObject.Instantiate(hardLandingEffectPrefab,transform.position,Quaternion.identity);
		}

		
		private void DoAttack()
		{
			//ModHooks.Instance.OnDoAttack();
			orig_DoAttack();
		}

		
		private void HeroDash()
		{
			if (!cState.onGround && !inAcid)
			{
				airDashed = true;
			}
			ResetAttacksDash();
			CancelBounce();
			audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN);
			audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK);
			audioCtrl.PlaySound(HeroSounds.DASH);
			ResetLook();
			cState.recoiling = false;
			if (cState.wallSliding)
			{
				FlipSprite();
			}
			else if (inputHandler.inputActions.right.IsPressed)
			{
				FaceRight();
			}
			else if (inputHandler.inputActions.left.IsPressed)
			{
				FaceLeft();
			}
			cState.dashing = true;
			dashQueueSteps = 0;
			if (!cState.onGround && inputHandler.inputActions.down.IsPressed && playerData.GetBool("equippedCharm_31"))
			{
				dashBurst.transform.localPosition = new Vector3(-0.07f, 3.74f, 0.01f);
				dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
				dashingDown = true;
			}
			else
			{
				dashBurst.transform.localPosition = new Vector3(4.11f, -0.55f, 0.001f);
				dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
				dashingDown = false;
			}
			if (playerData.GetBool("equippedCharm_31"))
			{
				dashCooldownTimer = DASH_COOLDOWN_CH;
			}
			else
			{
				dashCooldownTimer = DASH_COOLDOWN;
			}
			if (playerData.GetBool("hasShadowDash") && shadowDashTimer <= 0f)
			{
				shadowDashTimer = SHADOW_DASH_COOLDOWN;
				cState.shadowDashing = true;
				if (playerData.GetBool("equippedCharm_16"))
				{
					audioSource.PlayOneShot(sharpShadowClip, 1f);
					sharpShadowPrefab.SetActive(true);
				}
				else
				{
					audioSource.PlayOneShot(shadowDashClip, 1f);
				}
			}
			if (cState.shadowDashing)
			{
				if (dashingDown)
				{
					dashEffect = shadowdashDownBurstPrefab.Spawn(new Vector3(transform.position.x, transform.position.y + 3.5f, transform.position.z + 0.00101f));
					dashEffect.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
				}
				else if (transform.localScale.x > 0f)
				{
					dashEffect = shadowdashBurstPrefab.Spawn(new Vector3(transform.position.x + 5.21f, transform.position.y - 0.58f, transform.position.z + 0.00101f));
					dashEffect.transform.localScale = new Vector3(1.919591f, dashEffect.transform.localScale.y, dashEffect.transform.localScale.z);
				}
				else
				{
					dashEffect = shadowdashBurstPrefab.Spawn(new Vector3(transform.position.x - 5.21f, transform.position.y - 0.58f, transform.position.z + 0.00101f));
					dashEffect.transform.localScale = new Vector3(-1.919591f, dashEffect.transform.localScale.y, dashEffect.transform.localScale.z);
				}
				shadowRechargePrefab.SetActive(true);
				FSMUtility.LocateFSM(shadowRechargePrefab, "Recharge Effect").SendEvent("RESET");
				shadowdashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission = true;
				//VibrationManager.PlayVibrationClipOneShot(shadowDashVibration, null, false, string.Empty);
				GameObject.Instantiate(shadowRingPrefab,transform.position,Quaternion.identity);
			}
			else
			{
				dashBurst.SendEvent("PLAY");
				dashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission = true;
				//VibrationManager.PlayVibrationClipOneShot(dashVibration, null, false, string.Empty);
			}
			if (cState.onGround && !cState.shadowDashing)
			{
				dashEffect = GameObject.Instantiate(backDashPrefab,transform.position,Quaternion.identity);
				dashEffect.transform.localScale = new Vector3(transform.localScale.x * -1f, transform.localScale.y, transform.localScale.z);
			}
		}

		
		private void StartFallRumble()
		{
			fallRumble = true;
			audioCtrl.PlaySound(HeroSounds.FALLING);
			GameCameras.instance.cameraShakeFSM.Fsm.Variables.FindFsmBool("RumblingFall").Value = true;
		}

		
		private void SetState(ActorStates newState)
		{
			if (newState == ActorStates.grounded)
			{
				if (Mathf.Abs(move_input) > Mathf.Epsilon)
				{
					newState = ActorStates.running;
				}
				else
				{
					newState = ActorStates.idle;
				}
			}
			else if (newState == ActorStates.previous)
			{
				newState = prev_hero_state;
			}
			if (newState != hero_state)
			{
				prev_hero_state = hero_state;
				hero_state = newState;
				animCtrl.UpdateState(newState);
			}
		}

		
		private void FinishedEnteringScene(bool setHazardMarker = true, bool preventRunBob = false)
		{
			if (isEnteringFirstLevel)
			{
				isEnteringFirstLevel = false;
			}
			else
			{
				playerData.SetBoolSwappedArgs(false, "disablePause");
			}
			cState.transitioning = false;
			transitionState = HeroTransitionState.WAITING_TO_TRANSITION;
			stopWalkingOut = false;
			if (exitedSuperDashing || exitedQuake)
			{
				controlReqlinquished = true;
				IgnoreInput();
			}
			else
			{
				SetStartingMotionState(preventRunBob);
				AffectedByGravity(true);
			}
			if (setHazardMarker)
			{
				if (gm.startedOnThisScene || sceneEntryGate == null)
				{
					playerData.SetHazardRespawn(transform.position, cState.facingRight);
				}
				else if (!sceneEntryGate.nonHazardGate)
				{
					playerData.SetHazardRespawn(sceneEntryGate.respawnMarker);
				}
			}
			if (exitedQuake)
			{
				SetDamageMode(DamageMode.HAZARD_ONLY);
			}
			else
			{
				SetDamageMode(DamageMode.FULL_DAMAGE);
			}
			if (enterWithoutInput || exitedSuperDashing || exitedQuake)
			{
				enterWithoutInput = false;
			}
			else
			{
				AcceptInput();
			}
			gm.FinishedEnteringScene();
			if (exitedSuperDashing)
			{
				exitedSuperDashing = false;
			}
			if (exitedQuake)
			{
				exitedQuake = false;
			}
			positionHistory[0] = transform.position;
			positionHistory[1] = transform.position;
			tilemapTestActive = true;
		}

		
		private IEnumerator Die()
		{
			if (OnDeath != null)
			{
				OnDeath();
			}
			if (!cState.dead)
			{
				playerData.disablePause = true;
				boundsChecking = false;
				StopTilemapTest();
				cState.onConveyor = false;
				cState.onConveyorV = false;
				rb2d.velocity = new Vector2(0f, 0f);
				CancelRecoilHorizontal();
				string mapZone = gm.GetCurrentMapZone();
				if (mapZone == "DREAM_WORLD" || mapZone == "GODS_GLORY")
				{
					RelinquishControl();
					StopAnimationControl();
					AffectedByGravity(false);
					playerData.isInvincible = true;
					ResetHardLandingTimer();
					renderer.enabled = false;
					heroDeathPrefab.SetActive(true);
				}
				else
				{
					if (playerData.permadeathMode == 1)
					{
						playerData.permadeathMode = 2;
					}
					AffectedByGravity(false);
					HeroBox.inactive = true;
					rb2d.isKinematic = true;
					SetState(ActorStates.no_input);
					cState.dead = true;
					ResetMotion();
					ResetHardLandingTimer();
					renderer.enabled = false;
					gameObject.layer = 2;
					heroDeathPrefab.SetActive(true);
					yield return null;
					StartCoroutine(gm.PlayerDead(DEATH_WAIT));
				}
			}
			yield break;
		}

		
		private IEnumerator DieFromHazard(HazardType hazardType, float angle)
		{
			if (!cState.hazardDeath)
			{
				playerData.disablePause = true;
				StopTilemapTest();
				SetState(ActorStates.no_input);
				cState.hazardDeath = true;
				ResetMotion();
				ResetHardLandingTimer();
				AffectedByGravity(false);
				renderer.enabled = false;
				gameObject.layer = 2;
				if (hazardType == HazardType.Spikes)
				{
					GameObject gameObject = spikeDeathPrefab.Spawn();
					gameObject.transform.position = transform.position;
					FSMUtility.SetFloat(gameObject.GetComponent<PlayMakerFSM>(), "Spike Direction", angle * 57.29578f);
				}
				else if (hazardType == HazardType.Acid)
				{
					GameObject gameObject2 = acidDeathPrefab.Spawn();
					gameObject2.transform.position = transform.position;
					gameObject2.transform.localScale = transform.localScale;
				}
				yield return null;
				StartCoroutine(gm.PlayerDeadFromHazard(0f));
			}
			yield break;
		}

		
		private IEnumerator StartRecoil(CollisionSide impactSide, bool spawnDamageEffect, int damageAmount)
		{
			if (!cState.recoiling)
			{
				playerData.disablePause = true;
				ResetMotion();
				AffectedByGravity(false);
				if (impactSide == CollisionSide.left)
				{
					recoilVector = new Vector2(RECOIL_VELOCITY, RECOIL_VELOCITY * 0.5f);
					if (cState.facingRight)
					{
						FlipSprite();
					}
				}
				else if (impactSide == CollisionSide.right)
				{
					recoilVector = new Vector2(-RECOIL_VELOCITY, RECOIL_VELOCITY * 0.5f);
					if (!cState.facingRight)
					{
						FlipSprite();
					}
				}
				else
				{
					recoilVector = Vector2.zero;
				}
				SetState(ActorStates.no_input);
				cState.recoilFrozen = true;
				if (spawnDamageEffect)
				{
					damageEffectFSM.SendEvent("DAMAGE");
					if (damageAmount > 1)
					{
						UnityEngine.Object.Instantiate<GameObject>(takeHitDoublePrefab, transform.position, transform.rotation);
					}
				}
				if (playerData.equippedCharm_4)
				{
					StartCoroutine(Invulnerable(INVUL_TIME_STAL));
				}
				else
				{
					StartCoroutine(Invulnerable(INVUL_TIME));
				}
				yield return takeDamageCoroutine = StartCoroutine(gm.FreezeMoment(DAMAGE_FREEZE_DOWN, DAMAGE_FREEZE_WAIT, DAMAGE_FREEZE_UP, 0.0001f));
				cState.recoilFrozen = false;
				cState.recoiling = true;
				playerData.disablePause = false;
			}
			yield break;
		}

		
		private IEnumerator Invulnerable(float duration)
		{
			cState.invulnerable = true;
			yield return new WaitForSeconds(DAMAGE_FREEZE_DOWN);
			invPulse.startInvulnerablePulse();
			yield return new WaitForSeconds(duration);
			invPulse.stopInvulnerablePulse();
			cState.invulnerable = false;
			cState.recoiling = false;
			yield break;
		}

		
		private IEnumerator FirstFadeIn()
		{
			yield return new WaitForSeconds(0.25f);
			gm.FadeSceneIn();
			fadedSceneIn = true;
			yield break;
		}

		
		private void FallCheck()
		{
			if (rb2d.velocity.y <= -1E-06f)
			{
				if (!CheckTouchingGround())
				{
					cState.falling = true;
					cState.onGround = false;
					cState.wallJumping = false;
					proxyFSM.SendEvent("HeroCtrl-LeftGround");
					if (hero_state != ActorStates.no_input)
					{
						SetState(ActorStates.airborne);
					}
					if (cState.wallSliding)
					{
						fallTimer = 0f;
					}
					else
					{
						fallTimer += Time.deltaTime;
					}
					if (fallTimer > BIG_FALL_TIME)
					{
						if (!cState.willHardLand)
						{
							cState.willHardLand = true;
						}
						if (!fallRumble)
						{
							StartFallRumble();
						}
					}
					if (fallCheckFlagged)
					{
						fallCheckFlagged = false;
					}
				}
			}
			else
			{
				cState.falling = false;
				fallTimer = 0f;
				if (transitionState != HeroTransitionState.ENTERING_SCENE)
				{
					cState.willHardLand = false;
				}
				if (fallCheckFlagged)
				{
					fallCheckFlagged = false;
				}
				if (fallRumble)
				{
					CancelFallEffects();
				}
			}
		}

		
		private void OutOfBoundsCheck()
		{
			if (isGameplayScene)
			{
				Vector2 vector = transform.position;
				if ((vector.y >= -60f && vector.y <= gm.sceneHeight + 60f && vector.x >= -60f && vector.x <= gm.sceneWidth + 60f) || cState.dead || !boundsChecking)
				{
				}
			}
		}

		
		private void ConfirmOutOfBounds()
		{
			if (boundsChecking)
			{
				UnityEngine.Debug.Log("Confirming out of bounds");
				Vector2 vector = transform.position;
				if (vector.y < -60f || vector.y > gm.sceneHeight + 60f || vector.x < -60f || vector.x > gm.sceneWidth + 60f)
				{
					if (!cState.dead)
					{
						rb2d.velocity = Vector2.zero;
						UnityEngine.Debug.LogFormat("Pos: {0} Transition State: {1}", new object[]
						{
						transform.position,
						transitionState
						});
					}
				}
				else
				{
					boundsChecking = false;
				}
			}
		}

		
		private void FailSafeChecks()
		{
			if (hero_state == ActorStates.hard_landing)
			{
				hardLandFailSafeTimer += Time.deltaTime;
				if (hardLandFailSafeTimer > HARD_LANDING_TIME + 0.3f)
				{
					SetState(ActorStates.grounded);
					BackOnGround();
					hardLandFailSafeTimer = 0f;
				}
			}
			else
			{
				hardLandFailSafeTimer = 0f;
			}
			if (cState.hazardDeath)
			{
				hazardDeathTimer += Time.deltaTime;
				if (hazardDeathTimer > HAZARD_DEATH_CHECK_TIME && hero_state != ActorStates.no_input)
				{
					ResetMotion();
					AffectedByGravity(false);
					SetState(ActorStates.no_input);
					hazardDeathTimer = 0f;
				}
			}
			else
			{
				hazardDeathTimer = 0f;
			}
			if (rb2d.velocity.y == 0f && !cState.onGround && !cState.falling && !cState.jumping && !cState.dashing && hero_state != ActorStates.hard_landing && hero_state != ActorStates.no_input)
			{
				if (CheckTouchingGround())
				{
					floatingBufferTimer += Time.deltaTime;
					if (floatingBufferTimer > FLOATING_CHECK_TIME)
					{
						if (cState.recoiling)
						{
							CancelDamageRecoil();
						}
						BackOnGround();
						floatingBufferTimer = 0f;
					}
				}
				else
				{
					floatingBufferTimer = 0f;
				}
			}
		}

		
		public Transform LocateSpawnPoint()
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag("RespawnPoint");
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].name == playerData.GetString("respawnMarkerName"))
				{
					return array[i].transform;
				}
			}
			return null;
		}

		
		private void CancelJump()
		{
			cState.jumping = false;
			jumpReleaseQueuing = false;
			jump_steps = 0;
		}

		
		private void CancelDoubleJump()
		{
			cState.doubleJumping = false;
			doubleJump_steps = 0;
		}

		
		private void CancelDash()
		{
			if (cState.shadowDashing)
			{
				cState.shadowDashing = false;
			}
			cState.dashing = false;
			dash_timer = 0f;
			AffectedByGravity(true);
			sharpShadowPrefab.SetActive(false);
			if (dashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission)
			{
				dashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission = false;
			}
			if (shadowdashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission)
			{
				shadowdashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission = false;
			}
		}

		
		private void CancelWallsliding()
		{
			wallslideDustPrefab.enableEmission = false;
			//wallSlideVibrationPlayer.Stop();
			cState.wallSliding = false;
			wallSlidingL = false;
			wallSlidingR = false;
			touchingWallL = false;
			touchingWallR = false;
		}

		
		private void CancelBackDash()
		{
			cState.backDashing = false;
			back_dash_timer = 0f;
		}

		
		private void CancelDownAttack()
		{
			if (cState.downAttacking)
			{
				slashComponent.CancelAttack();
				ResetAttacks();
			}
		}

		
		private void CancelAttack()
		{
			if (cState.attacking)
			{
				slashComponent.CancelAttack();
				ResetAttacks();
			}
		}

		
		private void CancelBounce()
		{
			cState.bouncing = false;
			cState.shroomBouncing = false;
			bounceTimer = 0f;
		}

		
		private void CancelRecoilHorizontal()
		{
			cState.recoilingLeft = false;
			cState.recoilingRight = false;
			recoilSteps = 0;
		}

		
		private void CancelDamageRecoil()
		{
			cState.recoiling = false;
			recoilTimer = 0f;
			ResetMotion();
			AffectedByGravity(true);
			SetDamageMode(DamageMode.FULL_DAMAGE);
		}

		
		private void CancelFallEffects()
		{
			fallRumble = false;
			audioCtrl.StopSound(HeroSounds.FALLING);
			GameCameras.instance.cameraShakeFSM.Fsm.Variables.FindFsmBool("RumblingFall").Value = false;
		}

		
		private void ResetAttacks()
		{
			cState.nailCharging = false;
			nailChargeTimer = 0f;
			cState.attacking = false;
			cState.upAttacking = false;
			cState.downAttacking = false;
			attack_time = 0f;
		}

		
		private void ResetAttacksDash()
		{
			cState.attacking = false;
			cState.upAttacking = false;
			cState.downAttacking = false;
			attack_time = 0f;
		}

		
		private void ResetMotion()
		{
			CancelJump();
			CancelDoubleJump();
			CancelDash();
			CancelBackDash();
			CancelBounce();
			CancelRecoilHorizontal();
			CancelWallsliding();
			rb2d.velocity = Vector2.zero;
			transition_vel = Vector2.zero;
			wallLocked = false;
			nailChargeTimer = 0f;
		}

		
		private void ResetMotionNotVelocity()
		{
			CancelJump();
			CancelDoubleJump();
			CancelDash();
			CancelBackDash();
			CancelBounce();
			CancelRecoilHorizontal();
			CancelWallsliding();
			transition_vel = Vector2.zero;
			wallLocked = false;
		}

		
		private void ResetLook()
		{
			cState.lookingUp = false;
			cState.lookingDown = false;
			cState.lookingUpAnim = false;
			cState.lookingDownAnim = false;
			lookDelayTimer = 0f;
		}

		
		private void ResetInput()
		{
			move_input = 0f;
			vertical_input = 0f;
		}

		
		private void BackOnGround()
		{
			if (landingBufferSteps <= 0)
			{
				landingBufferSteps = LANDING_BUFFER_STEPS;
				if (!cState.onGround && !hardLanded && !cState.superDashing)
				{
					//GameObject.Instantiate(softLandingEffectPrefab,transform.position,Quaternion.identity);
					GameObject.Instantiate(softLandingEffectPrefab, transform.position,Quaternion.identity);
					//VibrationManager.PlayVibrationClipOneShot(softLandVibration, null, false, string.Empty);
				}
			}
			cState.falling = false;
			fallTimer = 0f;
			dashLandingTimer = 0f;
			cState.willHardLand = false;
			hardLandingTimer = 0f;
			hardLanded = false;
			jump_steps = 0;
			if (cState.doubleJumping)
			{
				HeroJump();
			}
			SetState(ActorStates.grounded);
			cState.onGround = true;
			airDashed = false;
			doubleJumped = false;
			if (dJumpWingsPrefab.activeSelf)
			{
				dJumpWingsPrefab.SetActive(false);
			}
		}

		
		private void JumpReleased()
		{
			if (rb2d.velocity.y > 0f && jumped_steps >= JUMP_STEPS_MIN && !inAcid && !cState.shroomBouncing)
			{
				if (jumpReleaseQueueingEnabled)
				{
					if (jumpReleaseQueuing && jumpReleaseQueueSteps <= 0)
					{
						rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
						CancelJump();
					}
				}
				else
				{
					rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
					CancelJump();
				}
			}
			jumpQueuing = false;
			doubleJumpQueuing = false;
			if (cState.swimming)
			{
				cState.swimming = false;
			}
		}

		
		private void FinishedDashing()
		{
			CancelDash();
			AffectedByGravity(true);
			animCtrl.FinishedDash();
			proxyFSM.SendEvent("HeroCtrl-DashEnd");
			if (cState.touchingWall && !cState.onGround && (playerData.GetBool("hasWalljump") & (touchingWallL || touchingWallR)))
			{
				wallslideDustPrefab.enableEmission = true;
				//wallSlideVibrationPlayer.Play();
				cState.wallSliding = true;
				cState.willHardLand = false;
				if (touchingWallL)
				{
					wallSlidingL = true;
				}
				if (touchingWallR)
				{
					wallSlidingR = true;
				}
				if (dashingDown)
				{
					FlipSprite();
				}
			}
		}

		
		private void SetStartingMotionState()
		{
			SetStartingMotionState(false);
		}

		
		private void SetStartingMotionState(bool preventRunDip)
		{
			move_input = ((!acceptingInput && !preventRunDip) ? 0f : inputHandler.inputActions.moveVector.X);
			cState.touchingWall = false;
			if (CheckTouchingGround())
			{
				cState.onGround = true;
				SetState(ActorStates.grounded);
				if (enteringVertically)
				{
					SpawnSoftLandingPrefab();
					animCtrl.playLanding = true;
					enteringVertically = false;
				}
			}
			else
			{
				cState.onGround = false;
				SetState(ActorStates.airborne);
			}
			animCtrl.UpdateState(hero_state);
		}

		
		[Obsolete("This was used specifically for underwater swimming in acid but is no longer in use.")]
		private void EnterAcid()
		{
			rb2d.gravityScale = UNDERWATER_GRAVITY;
			inAcid = true;
			cState.inAcid = true;
		}

		
		[Obsolete("This was used specifically for underwater swimming in acid but is no longer in use.")]
		private void ExitAcid()
		{
			rb2d.gravityScale = DEFAULT_GRAVITY;
			inAcid = false;
			cState.inAcid = false;
			airDashed = false;
			doubleJumped = false;
			if (inputHandler.inputActions.jump.IsPressed)
			{
				HeroJump();
			}
		}

		
		private void TileMapTest()
		{
			if (tilemapTestActive && !cState.jumping)
			{
				Vector2 vector = transform.position;
				Vector2 direction = new Vector2(positionHistory[0].x - vector.x, positionHistory[0].y - vector.y);
				float magnitude = direction.magnitude;
				RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, direction, magnitude, 256);
				if (raycastHit2D.collider != null)
				{
					UnityEngine.Debug.LogFormat("TERRAIN INGRESS {0} at {1} Jumping: {2}", new object[]
					{
					gm.GetSceneNameString(),
					vector,
					cState.jumping
					});
					ResetMotion();
					rb2d.velocity = Vector2.zero;
					if (cState.dashing)
					{
						FinishedDashing();
						transform.SetPosition(positionHistory[1]);
					}
					if (cState.superDashing)
					{
						transform.SetPosition(raycastHit2D.point);
						superDash.SendEvent("HIT WALL");
					}
					if (cState.spellQuake)
					{
						spellControl.SendEvent("Hero Landed");
						transform.SetPosition(positionHistory[1]);
					}
					tilemapTestActive = false;
					tilemapTestCoroutine = StartCoroutine(TilemapTestPause());
				}
			}
		}

		
		private IEnumerator TilemapTestPause()
		{
			yield return new WaitForSeconds(0.1f);
			tilemapTestActive = true;
			yield break;
		}

		
		private void StopTilemapTest()
		{
			if (tilemapTestCoroutine != null)
			{
				StopCoroutine(tilemapTestCoroutine);
				tilemapTestActive = false;
			}
		}

		
		public IEnumerator CheckForTerrainThunk(AttackDirection attackDir)
		{
			bool terrainHit = false;
			float thunkTimer = NAIL_TERRAIN_CHECK_TIME;
			while (thunkTimer > 0f)
			{
				if (!terrainHit)
				{
					float num = 0.25f;
					float num2;
					if (attackDir == AttackDirection.normal)
					{
						num2 = 2f;
					}
					else
					{
						num2 = 1.5f;
					}
					float num3 = 1f;
					if (playerData.equippedCharm_18)
					{
						num3 += 0.2f;
					}
					if (playerData.equippedCharm_13)
					{
						num3 += 0.3f;
					}
					num2 *= num3;
					Vector2 size = new Vector2(0.45f, 0.45f);
					Vector2 origin = new Vector2(col2d.bounds.center.x, col2d.bounds.center.y + num);
					Vector2 origin2 = new Vector2(col2d.bounds.center.x, col2d.bounds.max.y);
					Vector2 origin3 = new Vector2(col2d.bounds.center.x, col2d.bounds.min.y);
					int layerMask = 33554688;
					RaycastHit2D raycastHit2D = default(RaycastHit2D);
					if (attackDir == AttackDirection.normal)
					{
						if ((cState.facingRight && !cState.wallSliding) || (!cState.facingRight && cState.wallSliding))
						{
							raycastHit2D = Physics2D.BoxCast(origin, size, 0f, Vector2.right, num2, layerMask);
						}
						else
						{
							raycastHit2D = Physics2D.BoxCast(origin, size, 0f, Vector2.left, num2, layerMask);
						}
					}
					else if (attackDir == AttackDirection.upward)
					{
						raycastHit2D = Physics2D.BoxCast(origin2, size, 0f, Vector2.up, num2, layerMask);
					}
					else if (attackDir == AttackDirection.downward)
					{
						raycastHit2D = Physics2D.BoxCast(origin3, size, 0f, Vector2.down, num2, layerMask);
					}
					if (raycastHit2D.collider != null && !raycastHit2D.collider.isTrigger)
					{
						NonThunker component = raycastHit2D.collider.gameObject.GetComponent<NonThunker>();
						bool flag = !(component != null) || !component.active;
						if (flag)
						{
							terrainHit = true;
							nailTerrainImpactEffectPrefab.Spawn(raycastHit2D.point, Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f)));
							if (attackDir == AttackDirection.normal)
							{
								if (cState.facingRight)
								{
									RecoilLeft();
								}
								else
								{
									RecoilRight();
								}
							}
							else if (attackDir == AttackDirection.upward)
							{
								RecoilDown();
							}
						}
					}
					thunkTimer -= Time.deltaTime;
				}
				yield return null;
			}
			yield break;
		}

		
		private bool CheckStillTouchingWall(CollisionSide side, bool checkTop = false)
		{
			Vector2 origin = new Vector2(col2d.bounds.min.x, col2d.bounds.max.y);
			Vector2 origin2 = new Vector2(col2d.bounds.min.x, col2d.bounds.center.y);
			Vector2 origin3 = new Vector2(col2d.bounds.min.x, col2d.bounds.min.y);
			Vector2 origin4 = new Vector2(col2d.bounds.max.x, col2d.bounds.max.y);
			Vector2 origin5 = new Vector2(col2d.bounds.max.x, col2d.bounds.center.y);
			Vector2 origin6 = new Vector2(col2d.bounds.max.x, col2d.bounds.min.y);
			float distance = 0.1f;
			RaycastHit2D raycastHit2D = default(RaycastHit2D);
			RaycastHit2D raycastHit2D2 = default(RaycastHit2D);
			RaycastHit2D raycastHit2D3 = default(RaycastHit2D);
			if (side == CollisionSide.left)
			{
				if (checkTop)
				{
					raycastHit2D = Physics2D.Raycast(origin, Vector2.left, distance, 256);
				}
				raycastHit2D2 = Physics2D.Raycast(origin2, Vector2.left, distance, 256);
				raycastHit2D3 = Physics2D.Raycast(origin3, Vector2.left, distance, 256);
			}
			else
			{
				if (side != CollisionSide.right)
				{
					UnityEngine.Debug.LogError("Invalid CollisionSide specified.");
					return false;
				}
				if (checkTop)
				{
					raycastHit2D = Physics2D.Raycast(origin4, Vector2.right, distance, 256);
				}
				raycastHit2D2 = Physics2D.Raycast(origin5, Vector2.right, distance, 256);
				raycastHit2D3 = Physics2D.Raycast(origin6, Vector2.right, distance, 256);
			}
			if (raycastHit2D2.collider != null)
			{
				bool flag = true;
				if (raycastHit2D2.collider.isTrigger)
				{
					flag = false;
				}
				SteepSlope component = raycastHit2D2.collider.GetComponent<SteepSlope>();
				if (component != null)
				{
					flag = false;
				}
				NonSlider component2 = raycastHit2D2.collider.GetComponent<NonSlider>();
				if (component2 != null)
				{
					flag = false;
				}
				if (flag)
				{
					return true;
				}
			}
			if (raycastHit2D3.collider != null)
			{
				bool flag2 = true;
				if (raycastHit2D3.collider.isTrigger)
				{
					flag2 = false;
				}
				SteepSlope component3 = raycastHit2D3.collider.GetComponent<SteepSlope>();
				if (component3 != null)
				{
					flag2 = false;
				}
				NonSlider component4 = raycastHit2D3.collider.GetComponent<NonSlider>();
				if (component4 != null)
				{
					flag2 = false;
				}
				if (flag2)
				{
					return true;
				}
			}
			if (checkTop && raycastHit2D.collider != null)
			{
				bool flag3 = true;
				if (raycastHit2D.collider.isTrigger)
				{
					flag3 = false;
				}
				SteepSlope component5 = raycastHit2D.collider.GetComponent<SteepSlope>();
				if (component5 != null)
				{
					flag3 = false;
				}
				NonSlider component6 = raycastHit2D.collider.GetComponent<NonSlider>();
				if (component6 != null)
				{
					flag3 = false;
				}
				if (flag3)
				{
					return true;
				}
			}
			return false;
		}

		
		public bool CheckForBump(CollisionSide side)
		{
			float num = 0.025f;
			float num2 = 0.2f;
			Vector2 vector = new Vector2(col2d.bounds.min.x + num2, col2d.bounds.min.y + 0.2f);
			Vector2 vector2 = new Vector2(col2d.bounds.min.x + num2, col2d.bounds.min.y - num);
			Vector2 vector3 = new Vector2(col2d.bounds.max.x - num2, col2d.bounds.min.y + 0.2f);
			Vector2 vector4 = new Vector2(col2d.bounds.max.x - num2, col2d.bounds.min.y - num);
			float num3 = 0.32f + num2;
			RaycastHit2D raycastHit2D = default(RaycastHit2D);
			RaycastHit2D raycastHit2D2 = default(RaycastHit2D);
			if (side == CollisionSide.left)
			{
				UnityEngine.Debug.DrawLine(vector2, vector2 + Vector2.left * num3, Color.cyan, 0.15f);
				UnityEngine.Debug.DrawLine(vector, vector + Vector2.left * num3, Color.cyan, 0.15f);
				raycastHit2D2 = Physics2D.Raycast(vector2, Vector2.left, num3, 256);
				raycastHit2D = Physics2D.Raycast(vector, Vector2.left, num3, 256);
			}
			else if (side == CollisionSide.right)
			{
				UnityEngine.Debug.DrawLine(vector4, vector4 + Vector2.right * num3, Color.cyan, 0.15f);
				UnityEngine.Debug.DrawLine(vector3, vector3 + Vector2.right * num3, Color.cyan, 0.15f);
				raycastHit2D2 = Physics2D.Raycast(vector4, Vector2.right, num3, 256);
				raycastHit2D = Physics2D.Raycast(vector3, Vector2.right, num3, 256);
			}
			else
			{
				UnityEngine.Debug.LogError("Invalid CollisionSide specified.");
			}
			if (raycastHit2D2.collider != null && raycastHit2D.collider == null)
			{
				Vector2 vector5 = raycastHit2D2.point + new Vector2((side != CollisionSide.right) ? -0.1f : 0.1f, 1f);
				RaycastHit2D raycastHit2D3 = Physics2D.Raycast(vector5, Vector2.down, 1.5f, 256);
				Vector2 vector6 = raycastHit2D2.point + new Vector2((side != CollisionSide.right) ? 0.1f : -0.1f, 1f);
				RaycastHit2D raycastHit2D4 = Physics2D.Raycast(vector6, Vector2.down, 1.5f, 256);
				if (raycastHit2D3.collider != null)
				{
					UnityEngine.Debug.DrawLine(vector5, raycastHit2D3.point, Color.cyan, 0.15f);
					if (!(raycastHit2D4.collider != null))
					{
						return true;
					}
					UnityEngine.Debug.DrawLine(vector6, raycastHit2D4.point, Color.cyan, 0.15f);
					float num4 = raycastHit2D3.point.y - raycastHit2D4.point.y;
					if (num4 > 0f)
					{
						UnityEngine.Debug.Log("Bump Height: " + num4);
						return true;
					}
				}
			}
			return false;
		}

		
		public bool CheckNearRoof()
		{
			Vector2 origin = col2d.bounds.max;
			Vector2 origin2 = new Vector2(col2d.bounds.min.x, col2d.bounds.max.y);
			Vector2 vector = new Vector2(col2d.bounds.center.x, col2d.bounds.max.y);
			Vector2 origin3 = new Vector2(col2d.bounds.center.x + col2d.bounds.size.x / 4f, col2d.bounds.max.y);
			Vector2 origin4 = new Vector2(col2d.bounds.center.x - col2d.bounds.size.x / 4f, col2d.bounds.max.y);
			Vector2 direction = new Vector2(-0.5f, 1f);
			Vector2 direction2 = new Vector2(0.5f, 1f);
			Vector2 up = Vector2.up;
			RaycastHit2D raycastHit2D = Physics2D.Raycast(origin2, direction, 2f, 256);
			RaycastHit2D raycastHit2D2 = Physics2D.Raycast(origin, direction2, 2f, 256);
			RaycastHit2D raycastHit2D3 = Physics2D.Raycast(origin3, up, 1f, 256);
			RaycastHit2D raycastHit2D4 = Physics2D.Raycast(origin4, up, 1f, 256);
			return raycastHit2D.collider != null || raycastHit2D2.collider != null || raycastHit2D3.collider != null || raycastHit2D4.collider != null;
		}

		
		public bool CheckTouchingGround()
		{
			Vector2 vector = new Vector2(col2d.bounds.min.x, col2d.bounds.center.y);
			Vector2 vector2 = col2d.bounds.center;
			Vector2 vector3 = new Vector2(col2d.bounds.max.x, col2d.bounds.center.y);
			float distance = col2d.bounds.extents.y + 0.16f;
			UnityEngine.Debug.DrawRay(vector, Vector2.down, Color.yellow);
			UnityEngine.Debug.DrawRay(vector2, Vector2.down, Color.yellow);
			UnityEngine.Debug.DrawRay(vector3, Vector2.down, Color.yellow);
			RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, Vector2.down, distance, 256);
			RaycastHit2D raycastHit2D2 = Physics2D.Raycast(vector2, Vector2.down, distance, 256);
			RaycastHit2D raycastHit2D3 = Physics2D.Raycast(vector3, Vector2.down, distance, 256);
			return raycastHit2D.collider != null || raycastHit2D2.collider != null || raycastHit2D3.collider != null;
		}

		
		private List<CollisionSide> CheckTouching(PhysLayers layer)
		{
			List<CollisionSide> list = new List<CollisionSide>(4);
			Vector3 center = col2d.bounds.center;
			float distance = col2d.bounds.extents.x + 0.16f;
			float distance2 = col2d.bounds.extents.y + 0.16f;
			RaycastHit2D raycastHit2D = Physics2D.Raycast(center, Vector2.up, distance2, 1 << (int)layer);
			RaycastHit2D raycastHit2D2 = Physics2D.Raycast(center, Vector2.right, distance, 1 << (int)layer);
			RaycastHit2D raycastHit2D3 = Physics2D.Raycast(center, Vector2.down, distance2, 1 << (int)layer);
			RaycastHit2D raycastHit2D4 = Physics2D.Raycast(center, Vector2.left, distance, 1 << (int)layer);
			if (raycastHit2D.collider != null)
			{
				list.Add(CollisionSide.top);
			}
			if (raycastHit2D2.collider != null)
			{
				list.Add(CollisionSide.right);
			}
			if (raycastHit2D3.collider != null)
			{
				list.Add(CollisionSide.bottom);
			}
			if (raycastHit2D4.collider != null)
			{
				list.Add(CollisionSide.left);
			}
			return list;
		}

		
		private List<CollisionSide> CheckTouchingAdvanced(PhysLayers layer)
		{
			List<CollisionSide> list = new List<CollisionSide>();
			Vector2 origin = new Vector2(col2d.bounds.min.x, col2d.bounds.max.y);
			Vector2 origin2 = new Vector2(col2d.bounds.center.x, col2d.bounds.max.y);
			Vector2 origin3 = new Vector2(col2d.bounds.max.x, col2d.bounds.max.y);
			Vector2 origin4 = new Vector2(col2d.bounds.min.x, col2d.bounds.center.y);
			Vector2 origin5 = new Vector2(col2d.bounds.max.x, col2d.bounds.center.y);
			Vector2 origin6 = new Vector2(col2d.bounds.min.x, col2d.bounds.min.y);
			Vector2 origin7 = new Vector2(col2d.bounds.center.x, col2d.bounds.min.y);
			Vector2 origin8 = new Vector2(col2d.bounds.max.x, col2d.bounds.min.y);
			RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, Vector2.up, 0.16f, 1 << (int)layer);
			RaycastHit2D raycastHit2D2 = Physics2D.Raycast(origin2, Vector2.up, 0.16f, 1 << (int)layer);
			RaycastHit2D raycastHit2D3 = Physics2D.Raycast(origin3, Vector2.up, 0.16f, 1 << (int)layer);
			RaycastHit2D raycastHit2D4 = Physics2D.Raycast(origin3, Vector2.right, 0.16f, 1 << (int)layer);
			RaycastHit2D raycastHit2D5 = Physics2D.Raycast(origin5, Vector2.right, 0.16f, 1 << (int)layer);
			RaycastHit2D raycastHit2D6 = Physics2D.Raycast(origin8, Vector2.right, 0.16f, 1 << (int)layer);
			RaycastHit2D raycastHit2D7 = Physics2D.Raycast(origin8, Vector2.down, 0.16f, 1 << (int)layer);
			RaycastHit2D raycastHit2D8 = Physics2D.Raycast(origin7, Vector2.down, 0.16f, 1 << (int)layer);
			RaycastHit2D raycastHit2D9 = Physics2D.Raycast(origin6, Vector2.down, 0.16f, 1 << (int)layer);
			RaycastHit2D raycastHit2D10 = Physics2D.Raycast(origin6, Vector2.left, 0.16f, 1 << (int)layer);
			RaycastHit2D raycastHit2D11 = Physics2D.Raycast(origin4, Vector2.left, 0.16f, 1 << (int)layer);
			RaycastHit2D raycastHit2D12 = Physics2D.Raycast(origin, Vector2.left, 0.16f, 1 << (int)layer);
			if (raycastHit2D.collider != null || raycastHit2D2.collider != null || raycastHit2D3.collider != null)
			{
				list.Add(CollisionSide.top);
			}
			if (raycastHit2D4.collider != null || raycastHit2D5.collider != null || raycastHit2D6.collider != null)
			{
				list.Add(CollisionSide.right);
			}
			if (raycastHit2D7.collider != null || raycastHit2D8.collider != null || raycastHit2D9.collider != null)
			{
				list.Add(CollisionSide.bottom);
			}
			if (raycastHit2D10.collider != null || raycastHit2D11.collider != null || raycastHit2D12.collider != null)
			{
				list.Add(CollisionSide.left);
			}
			return list;
		}

		
		private CollisionSide FindCollisionDirection(Collision2D collision)
		{
			Vector2 normal = collision.GetSafeContact().Normal;
			float x = normal.x;
			float y = normal.y;
			if (y >= 0.5f)
			{
				return CollisionSide.bottom;
			}
			if (y <= -0.5f)
			{
				return CollisionSide.top;
			}
			if (x < 0f)
			{
				return CollisionSide.right;
			}
			if (x > 0f)
			{
				return CollisionSide.left;
			}
			UnityEngine.Debug.LogError(string.Concat(new object[]
			{
			"ERROR: unable to determine direction of collision - contact points at (",
			normal.x,
			",",
			normal.y,
			")"
			}));
			return CollisionSide.bottom;
		}

		
		private bool CanJump()
		{
			if (hero_state == ActorStates.no_input || hero_state == ActorStates.hard_landing || hero_state == ActorStates.dash_landing || cState.wallSliding || cState.dashing || cState.backDashing || cState.jumping || cState.bouncing || cState.shroomBouncing)
			{
				return false;
			}
			if (cState.onGround)
			{
				return true;
			}
			if (ledgeBufferSteps > 0 && !cState.dead && !cState.hazardDeath && !controlReqlinquished && headBumpSteps <= 0 && !CheckNearRoof())
			{
				ledgeBufferSteps = 0;
				return true;
			}
			return false;
		}

		
		private bool CanDoubleJump()
		{
			return playerData.GetBool("hasDoubleJump") && !controlReqlinquished && !doubleJumped && !inAcid && hero_state != ActorStates.no_input && hero_state != ActorStates.hard_landing && hero_state != ActorStates.dash_landing && !cState.dashing && !cState.wallSliding && !cState.backDashing && !cState.attacking && !cState.bouncing && !cState.shroomBouncing && !cState.onGround;
		}

		
		private bool CanInfiniteAirJump()
		{
			return playerData.GetBool("infiniteAirJump") && hero_state != ActorStates.hard_landing && !cState.onGround;
		}

		
		private bool CanSwim()
		{
			return hero_state != ActorStates.no_input && hero_state != ActorStates.hard_landing && hero_state != ActorStates.dash_landing && !cState.attacking && !cState.dashing && !cState.jumping && !cState.bouncing && !cState.shroomBouncing && !cState.onGround && inAcid;
		}

		
		private bool CanDash()
		{
			return hero_state != ActorStates.no_input && hero_state != ActorStates.hard_landing && hero_state != ActorStates.dash_landing && dashCooldownTimer <= 0f && !cState.dashing && !cState.backDashing && (!cState.attacking || attack_time >= ATTACK_RECOVERY_TIME) && !cState.preventDash && (cState.onGround || !airDashed || cState.wallSliding) && !cState.hazardDeath && playerData.GetBool("canDash");
		}

		
		private bool CanAttack()
		{
			return attack_cooldown <= 0f && !cState.attacking && !cState.dashing && !cState.dead && !cState.hazardDeath && !cState.hazardRespawning && !controlReqlinquished && hero_state != ActorStates.no_input && hero_state != ActorStates.hard_landing && hero_state != ActorStates.dash_landing;
		}

		
		private bool CanNailCharge()
		{
			return !cState.attacking && !controlReqlinquished && !cState.recoiling && !cState.recoilingLeft && !cState.recoilingRight && playerData.GetBool("hasNailArt");
		}

		
		private bool CanWallSlide()
		{
			return (cState.wallSliding && gm.isPaused) || (!cState.touchingNonSlider && !inAcid && !cState.dashing && playerData.GetBool("hasWalljump") && !cState.onGround && !cState.recoiling && !gm.isPaused && !controlReqlinquished && !cState.transitioning && (cState.falling || cState.wallSliding) && !cState.doubleJumping && CanInput());
		}

		
		private bool CanTakeDamage()
		{
			return damageMode != DamageMode.NO_DAMAGE && transitionState == HeroTransitionState.WAITING_TO_TRANSITION && !cState.invulnerable && !cState.recoiling && !playerData.GetBool("isInvincible") && !cState.dead && !cState.hazardDeath && !BossSceneController.IsTransitioning;
		}

		
		private bool CanWallJump()
		{
			return playerData.GetBool("hasWalljump") && !cState.touchingNonSlider && (cState.wallSliding || (cState.touchingWall && !cState.onGround));
		}

		
		private bool ShouldHardLand(Collision2D collision)
		{
			return !collision.gameObject.GetComponent<NoHardLanding>() && cState.willHardLand && !inAcid && hero_state != ActorStates.hard_landing;
		}

		
		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (cState.superDashing && (CheckStillTouchingWall(CollisionSide.left, false) || CheckStillTouchingWall(CollisionSide.right, false)))
			{
				superDash.SendEvent("HIT WALL");
			}
			if ((collision.gameObject.layer == 8 || collision.gameObject.CompareTag("HeroWalkable")) && CheckTouchingGround())
			{
				proxyFSM.SendEvent("HeroCtrl-Landed");
			}
			if (hero_state != ActorStates.no_input)
			{
				CollisionSide collisionSide = FindCollisionDirection(collision);
				if (collision.gameObject.layer == 8 || collision.gameObject.CompareTag("HeroWalkable"))
				{
					fallTrailGenerated = false;
					if (collisionSide == CollisionSide.top)
					{
						headBumpSteps = HEAD_BUMP_STEPS;
						if (cState.jumping)
						{
							CancelJump();
							CancelDoubleJump();
						}
						if (cState.bouncing)
						{
							CancelBounce();
							rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
						}
						if (cState.shroomBouncing)
						{
							CancelBounce();
							rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
						}
					}
					if (collisionSide == CollisionSide.bottom)
					{
						if (cState.attacking)
						{
							CancelDownAttack();
						}
						if (ShouldHardLand(collision))
						{
							DoHardLanding();
						}
						else if (collision.gameObject.GetComponent<SteepSlope>() == null && hero_state != ActorStates.hard_landing)
						{
							BackOnGround();
						}
						if (cState.dashing && dashingDown)
						{
							AffectedByGravity(true);
							SetState(ActorStates.dash_landing);
							hardLanded = true;
						}
					}
				}
			}
			else if (hero_state == ActorStates.no_input && transitionState == HeroTransitionState.DROPPING_DOWN && (gatePosition == GatePosition.bottom || gatePosition == GatePosition.top))
			{
				FinishedEnteringScene(true, false);
			}
		}

		
		private void OnCollisionStay2D(Collision2D collision)
		{
			if (cState.superDashing && (CheckStillTouchingWall(CollisionSide.left, false) || CheckStillTouchingWall(CollisionSide.right, false)))
			{
				superDash.SendEvent("HIT WALL");
			}
			if (hero_state != ActorStates.no_input && collision.gameObject.layer == 8)
			{
				if (collision.gameObject.GetComponent<NonSlider>() == null)
				{
					cState.touchingNonSlider = false;
					if (CheckStillTouchingWall(CollisionSide.left, false))
					{
						cState.touchingWall = true;
						touchingWallL = true;
						touchingWallR = false;
					}
					else if (CheckStillTouchingWall(CollisionSide.right, false))
					{
						cState.touchingWall = true;
						touchingWallL = false;
						touchingWallR = true;
					}
					else
					{
						cState.touchingWall = false;
						touchingWallL = false;
						touchingWallR = false;
					}
					if (CheckTouchingGround())
					{
						if (ShouldHardLand(collision))
						{
							DoHardLanding();
						}
						else if (hero_state != ActorStates.hard_landing && hero_state != ActorStates.dash_landing && cState.falling)
						{
							BackOnGround();
						}
					}
					else if (cState.jumping || cState.falling)
					{
						cState.onGround = false;
						proxyFSM.SendEvent("HeroCtrl-LeftGround");
						SetState(ActorStates.airborne);
					}
				}
				else
				{
					cState.touchingNonSlider = true;
				}
			}
		}

		
		private void OnCollisionExit2D(Collision2D collision)
		{
			if (cState.recoilingLeft || cState.recoilingRight)
			{
				cState.touchingWall = false;
				touchingWallL = false;
				touchingWallR = false;
				cState.touchingNonSlider = false;
			}
			if (hero_state != ActorStates.no_input && !cState.recoiling && collision.gameObject.layer == 8)
			{
				if (!CheckTouchingGround())
				{
					if (!cState.jumping && !fallTrailGenerated && cState.onGround)
					{
						if (playerData.GetInt("environmentType") != 6)
						{
							fsm_fallTrail.SendEvent("PLAY");
						}
						fallTrailGenerated = true;
					}
					cState.onGround = false;
					proxyFSM.SendEvent("HeroCtrl-LeftGround");
					SetState(ActorStates.airborne);
					if (cState.wasOnGround)
					{
						ledgeBufferSteps = LEDGE_BUFFER_STEPS;
					}
				}
			}
		}

		
		private void SetupGameRefs()
		{
			if (cState == null)
			{
				cState = new HeroControllerStates();
			}
			gm = GameManager.instance;
			animCtrl = GetComponent<HeroAnimationController>();
			rb2d = GetComponent<Rigidbody2D>();
			col2d = GetComponent<Collider2D>();
			transform = GetComponent<Transform>();
			renderer = GetComponent<MeshRenderer>();
			audioCtrl = GetComponent<HeroAudioController>();
			inputHandler = gm.GetComponent<InputHandler>();
			proxyFSM = FSMUtility.LocateFSM(gameObject, "ProxyFSM");
			audioSource = GetComponent<AudioSource>();
			if (!footStepsRunAudioSource)
			{
				footStepsRunAudioSource = transform.Find("Sounds/FootstepsRun").GetComponent<AudioSource>();
			}
			if (!footStepsWalkAudioSource)
			{
				footStepsWalkAudioSource = transform.Find("Sounds/FootstepsWalk").GetComponent<AudioSource>();
			}
			invPulse = GetComponent<InvulnerablePulse>();
			spriteFlash = GetComponent<SpriteFlash>();
			gm.UnloadingLevel += OnLevelUnload;
			prevGravityScale = DEFAULT_GRAVITY;
			transition_vel = Vector2.zero;
			current_velocity = Vector2.zero;
			acceptingInput = true;
			positionHistory = new Vector2[2];
		}

		
		private void SetupPools()
		{
		}

		
		private void FilterInput()
		{
			if (move_input > 0.3f)
			{
				move_input = 1f;
			}
			else if (move_input < -0.3f)
			{
				move_input = -1f;
			}
			else
			{
				move_input = 0f;
			}
			if (vertical_input > 0.5f)
			{
				vertical_input = 1f;
			}
			else if (vertical_input < -0.5f)
			{
				vertical_input = -1f;
			}
			else
			{
				vertical_input = 0f;
			}
		}

		
		public Vector3 FindGroundPoint(Vector2 startPoint, bool useExtended = false)
		{
			float num = FIND_GROUND_POINT_DISTANCE;
			if (useExtended)
			{
				num = FIND_GROUND_POINT_DISTANCE_EXT;
			}
			RaycastHit2D raycastHit2D = Physics2D.Raycast(startPoint, Vector2.down, num, 256);
			if (raycastHit2D.collider == null)
			{
				UnityEngine.Debug.LogErrorFormat("FindGroundPoint: Could not find ground point below {0}, check reference position is not too high (more than {1} tiles).", new object[]
				{
				startPoint.ToString(),
				num
				});
			}
			return new Vector3(raycastHit2D.point.x, raycastHit2D.point.y + col2d.bounds.extents.y - col2d.offset.y + 0.01f, transform.position.z);
		}

		
		private float FindGroundPointY(float x, float y, bool useExtended = false)
		{
			float num = FIND_GROUND_POINT_DISTANCE;
			if (useExtended)
			{
				num = FIND_GROUND_POINT_DISTANCE_EXT;
			}
			RaycastHit2D raycastHit2D = Physics2D.Raycast(new Vector2(x, y), Vector2.down, num, 256);
			if (raycastHit2D.collider == null)
			{
				UnityEngine.Debug.LogErrorFormat("FindGroundPoint: Could not find ground point below ({0},{1}), check reference position is not too high (more than {2} tiles).", new object[]
				{
				x,
				y,
				num
				});
			}
			return raycastHit2D.point.y + col2d.bounds.extents.y - col2d.offset.y + 0.01f;
		}

		
		public void orig_StartMPDrain(float time)
		{
			drainMP = true;
			drainMP_timer = 0f;
			MP_drained = 0f;
			drainMP_time = time;
			focusMP_amount = (float)playerData.GetInt("focusMP_amount");
		}

		
		private void orig_Update()
		{
			if (Time.frameCount % 10 == 0)
			{
				Update10();
			}
			current_velocity = rb2d.velocity;
			FallCheck();
			FailSafeChecks();
			if (hero_state == ActorStates.running && !cState.dashing && !cState.backDashing && !controlReqlinquished)
			{
				if (cState.inWalkZone)
				{
					audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN);
					audioCtrl.PlaySound(HeroSounds.FOOTSTEPS_WALK);
				}
				else
				{
					audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK);
					audioCtrl.PlaySound(HeroSounds.FOOTSTEPS_RUN);
				}
				if (runMsgSent && rb2d.velocity.x > -0.1f && rb2d.velocity.x < 0.1f)
				{
					runEffect.GetComponent<PlayMakerFSM>().SendEvent("RUN STOP");
					runEffect.transform.SetParent(null, true);
					runMsgSent = false;
				}
				if (!runMsgSent && (rb2d.velocity.x < -0.1f || rb2d.velocity.x > 0.1f))
				{
					runEffect = runEffectPrefab.Spawn();
					runEffect.transform.SetParent(gameObject.transform, false);
					runMsgSent = true;
				}
			}
			else
			{
				audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN);
				audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK);
				if (runMsgSent)
				{
					runEffect.GetComponent<PlayMakerFSM>().SendEvent("RUN STOP");
					runEffect.transform.SetParent(null, true);
					runMsgSent = false;
				}
			}
			if (hero_state == ActorStates.dash_landing)
			{
				dashLandingTimer += Time.deltaTime;
				if (dashLandingTimer > DOWN_DASH_TIME)
				{
					BackOnGround();
				}
			}
			if (hero_state == ActorStates.hard_landing)
			{
				hardLandingTimer += Time.deltaTime;
				if (hardLandingTimer > HARD_LANDING_TIME)
				{
					SetState(ActorStates.grounded);
					BackOnGround();
				}
			}
			else if (hero_state == ActorStates.no_input)
			{
				if (cState.recoiling)
				{
					if ((!playerData.GetBool("equippedCharm_4") && recoilTimer < RECOIL_DURATION) || (playerData.GetBool("equippedCharm_4") && recoilTimer < RECOIL_DURATION_STAL))
					{
						recoilTimer += Time.deltaTime;
					}
					else
					{
						CancelDamageRecoil();
						if ((prev_hero_state == ActorStates.idle || prev_hero_state == ActorStates.running) && !CheckTouchingGround())
						{
							cState.onGround = false;
							SetState(ActorStates.airborne);
						}
						else
						{
							SetState(ActorStates.previous);
						}
						fsm_thornCounter.SendEvent("THORN COUNTER");
					}
				}
			}
			else if (hero_state != ActorStates.no_input)
			{
				LookForInput();
				if (cState.recoiling)
				{
					cState.recoiling = false;
					AffectedByGravity(true);
				}
				if (cState.attacking && !cState.dashing)
				{
					attack_time += Time.deltaTime;
					if (attack_time >= attackDuration)
					{
						ResetAttacks();
						animCtrl.StopAttack();
					}
				}
				if (cState.bouncing)
				{
					if (bounceTimer < BOUNCE_TIME)
					{
						bounceTimer += Time.deltaTime;
					}
					else
					{
						CancelBounce();
						rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
					}
				}
				if (cState.shroomBouncing && current_velocity.y <= 0f)
				{
					cState.shroomBouncing = false;
				}
				if (hero_state == ActorStates.idle)
				{
					if (!controlReqlinquished && !gm.isPaused)
					{
						if (inputHandler.inputActions.up.IsPressed || inputHandler.inputActions.rs_up.IsPressed)
						{
							cState.lookingDown = false;
							cState.lookingDownAnim = false;
							if (lookDelayTimer >= LOOK_DELAY || (inputHandler.inputActions.rs_up.IsPressed && !cState.jumping && !cState.dashing))
							{
								cState.lookingUp = true;
							}
							else
							{
								lookDelayTimer += Time.deltaTime;
							}
							if (lookDelayTimer >= LOOK_ANIM_DELAY || inputHandler.inputActions.rs_up.IsPressed)
							{
								cState.lookingUpAnim = true;
							}
							else
							{
								cState.lookingUpAnim = false;
							}
						}
						else if (inputHandler.inputActions.down.IsPressed || inputHandler.inputActions.rs_down.IsPressed)
						{
							cState.lookingUp = false;
							cState.lookingUpAnim = false;
							if (lookDelayTimer >= LOOK_DELAY || (inputHandler.inputActions.rs_down.IsPressed && !cState.jumping && !cState.dashing))
							{
								cState.lookingDown = true;
							}
							else
							{
								lookDelayTimer += Time.deltaTime;
							}
							if (lookDelayTimer >= LOOK_ANIM_DELAY || inputHandler.inputActions.rs_down.IsPressed)
							{
								cState.lookingDownAnim = true;
							}
							else
							{
								cState.lookingDownAnim = false;
							}
						}
						else
						{
							ResetLook();
						}
					}
					runPuffTimer = 0f;
				}
			}
			LookForQueueInput();
			if (drainMP)
			{
				drainMP_timer += Time.deltaTime;
				drainMP_seconds += Time.deltaTime;
				while (drainMP_timer >= drainMP_time)
				{
					MP_drained += 1f;
					drainMP_timer -= drainMP_time;
					TakeMP(1);
					gm.soulOrb_fsm.SendEvent("MP DRAIN");
					if (MP_drained == focusMP_amount)
					{
						MP_drained -= drainMP_time;
						proxyFSM.SendEvent("HeroCtrl-FocusCompleted");
					}
				}
			}
			if (cState.wallSliding)
			{
				if (airDashed)
				{
					airDashed = false;
				}
				if (doubleJumped)
				{
					doubleJumped = false;
				}
				if (cState.onGround)
				{
					FlipSprite();
					CancelWallsliding();
				}
				if (!cState.touchingWall)
				{
					FlipSprite();
					CancelWallsliding();
				}
				if (!CanWallSlide())
				{
					CancelWallsliding();
				}
				if (!playedMantisClawClip)
				{
					audioSource.PlayOneShot(mantisClawClip, 1f);
					playedMantisClawClip = true;
				}
				if (!playingWallslideClip)
				{
					if (wallslideClipTimer <= WALLSLIDE_CLIP_DELAY)
					{
						wallslideClipTimer += Time.deltaTime;
					}
					else
					{
						wallslideClipTimer = 0f;
						audioCtrl.PlaySound(HeroSounds.WALLSLIDE);
						playingWallslideClip = true;
					}
				}
			}
			else if (playedMantisClawClip)
			{
				playedMantisClawClip = false;
			}
			if (!cState.wallSliding && playingWallslideClip)
			{
				audioCtrl.StopSound(HeroSounds.WALLSLIDE);
				playingWallslideClip = false;
			}
			if (!cState.wallSliding && wallslideClipTimer > 0f)
			{
				wallslideClipTimer = 0f;
			}
			if (wallSlashing && !cState.wallSliding)
			{
				CancelAttack();
			}
			if (attack_cooldown > 0f)
			{
				attack_cooldown -= Time.deltaTime;
			}
			if (dashCooldownTimer > 0f)
			{
				dashCooldownTimer -= Time.deltaTime;
			}
			if (shadowDashTimer > 0f)
			{
				shadowDashTimer -= Time.deltaTime;
				if (shadowDashTimer <= 0f)
				{
					spriteFlash.FlashShadowRecharge();
				}
			}
			preventCastByDialogueEndTimer -= Time.deltaTime;
			if (!gm.isPaused)
			{
				if (inputHandler.inputActions.attack.IsPressed && CanNailCharge())
				{
					cState.nailCharging = true;
					nailChargeTimer += Time.deltaTime;
				}
				else if (cState.nailCharging || nailChargeTimer != 0f)
				{
					artChargeEffect.SetActive(false);
					cState.nailCharging = false;
					audioCtrl.StopSound(HeroSounds.NAIL_ART_CHARGE);
				}
				if (cState.nailCharging && nailChargeTimer > 0.5f && !artChargeEffect.activeSelf && nailChargeTimer < nailChargeTime)
				{
					artChargeEffect.SetActive(true);
					audioCtrl.PlaySound(HeroSounds.NAIL_ART_CHARGE);
				}
				if (artChargeEffect.activeSelf && (!cState.nailCharging || nailChargeTimer > nailChargeTime))
				{
					artChargeEffect.SetActive(false);
					audioCtrl.StopSound(HeroSounds.NAIL_ART_CHARGE);
				}
				if (!artChargedEffect.activeSelf && nailChargeTimer >= nailChargeTime)
				{
					artChargedEffect.SetActive(true);
					artChargedFlash.SetActive(true);
					artChargedEffectAnim.PlayFromFrame(0);
					GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");
					audioSource.PlayOneShot(nailArtChargeComplete, 1f);
					audioCtrl.PlaySound(HeroSounds.NAIL_ART_READY);
					cState.nailCharging = true;
				}
				if (artChargedEffect.activeSelf && (nailChargeTimer < nailChargeTime || !cState.nailCharging))
				{
					artChargedEffect.SetActive(false);
					audioCtrl.StopSound(HeroSounds.NAIL_ART_READY);
				}
			}
			if (gm.isPaused && !inputHandler.inputActions.attack.IsPressed)
			{
				cState.nailCharging = false;
				nailChargeTimer = 0f;
			}
			if (cState.swimming && !CanSwim())
			{
				cState.swimming = false;
			}
			if (parryInvulnTimer > 0f)
			{
				parryInvulnTimer -= Time.deltaTime;
			}
		}

		
		private Vector2 OrigDashVector()
		{
			bool flag = playerData.equippedCharm_16 && cState.shadowDashing;
			float num;
			if (flag)
			{
				num = DASH_SPEED_SHARP;
			}
			else
			{
				num = DASH_SPEED;
			}
			bool flag2 = dashingDown;
			Vector2 result;
			if (flag2)
			{
				result = new Vector2(0f, -num);
			}
			else
			{
				bool facingRight = cState.facingRight;
				if (facingRight)
				{
					bool flag3 = CheckForBump(CollisionSide.right);
					if (flag3)
					{
						result = new Vector2(num, (!cState.onGround) ? 5f : 4f);
					}
					else
					{
						result = new Vector2(num, 0f);
					}
				}
				else
				{
					bool flag4 = CheckForBump(CollisionSide.left);
					if (flag4)
					{
						result = new Vector2(-num, (!cState.onGround) ? 5f : 4f);
					}
					else
					{
						result = new Vector2(-num, 0f);
					}
				}
			}
			return result;
		}

		
		private void orig_Dash()
		{
			AffectedByGravity(false);
			ResetHardLandingTimer();
			if (dash_timer > DASH_TIME)
			{
				FinishedDashing();
			}
			else
			{
				float num;
				if (playerData.GetBool("equippedCharm_16") && cState.shadowDashing)
				{
					num = DASH_SPEED_SHARP;
				}
				else
				{
					num = DASH_SPEED;
				}
				if (dashingDown)
				{
					rb2d.velocity = new Vector2(0f, -num);
				}
				else if (cState.facingRight)
				{
					if (CheckForBump(CollisionSide.right))
					{
						rb2d.velocity = new Vector2(num, (!cState.onGround) ? BUMP_VELOCITY_DASH : BUMP_VELOCITY);
					}
					else
					{
						rb2d.velocity = new Vector2(num, 0f);
					}
				}
				else if (CheckForBump(CollisionSide.left))
				{
					rb2d.velocity = new Vector2(-num, (!cState.onGround) ? BUMP_VELOCITY_DASH : BUMP_VELOCITY);
				}
				else
				{
					rb2d.velocity = new Vector2(-num, 0f);
				}
				dash_timer += Time.deltaTime;
			}
		}

		
		public void orig_CharmUpdate()
		{
			if (playerData.GetBool("equippedCharm_26"))
			{
				nailChargeTime = NAIL_CHARGE_TIME_CHARM;
			}
			else
			{
				nailChargeTime = NAIL_CHARGE_TIME_DEFAULT;
			}
			if (playerData.GetBool("equippedCharm_23") && !playerData.GetBool("brokenCharm_23"))
			{
				playerData.SetIntSwappedArgs(playerData.GetInt("maxHealthBase") + 2, "maxHealth");
				playerData.MaxHealth();
			}
			else
			{
				playerData.SetIntSwappedArgs(playerData.GetInt("maxHealthBase"), "maxHealth");
				playerData.MaxHealth();
			}
			if (playerData.GetBool("equippedCharm_27"))
			{
				playerData.SetIntSwappedArgs((int)((float)playerData.GetInt("maxHealth") * 1.4f), "joniHealthBlue");
				playerData.SetIntSwappedArgs(1, "maxHealth");
				playerData.MaxHealth();
				joniBeam = true;
			}
			else
			{
				playerData.SetIntSwappedArgs(0, "joniHealthBlue");
			}
			if (playerData.GetBool("equippedCharm_40") && playerData.GetInt("grimmChildLevel") == 5)
			{
				carefreeShieldEquipped = true;
			}
			else
			{
				carefreeShieldEquipped = false;
			}
			playerData.UpdateBlueHealth();
		}

		
		private void orig_DoAttack()
		{
			ResetLook();
			cState.recoiling = false;
			if (playerData.GetBool("equippedCharm_32"))
			{
				attack_cooldown = ATTACK_COOLDOWN_TIME_CH;
			}
			else
			{
				attack_cooldown = ATTACK_COOLDOWN_TIME;
			}
			if (vertical_input > Mathf.Epsilon)
			{
				Attack(AttackDirection.upward);
				StartCoroutine(CheckForTerrainThunk(AttackDirection.upward));
			}
			else if (vertical_input < -Mathf.Epsilon)
			{
				if (hero_state != ActorStates.idle && hero_state != ActorStates.running)
				{
					Attack(AttackDirection.downward);
					StartCoroutine(CheckForTerrainThunk(AttackDirection.downward));
				}
				else
				{
					Attack(AttackDirection.normal);
					StartCoroutine(CheckForTerrainThunk(AttackDirection.normal));
				}
			}
			else
			{
				Attack(AttackDirection.normal);
				StartCoroutine(CheckForTerrainThunk(AttackDirection.normal));
			}
		}

		// Token: 0x040023F3 RID: 9203
		private bool verboseMode;

		// Token: 0x040023F4 RID: 9204
		public HeroType heroType;

		// Token: 0x040023F5 RID: 9205
		public float RUN_SPEED;

		// Token: 0x040023F6 RID: 9206
		public float RUN_SPEED_CH;

		// Token: 0x040023F7 RID: 9207
		public float RUN_SPEED_CH_COMBO;

		// Token: 0x040023F8 RID: 9208
		public float WALK_SPEED;

		// Token: 0x040023F9 RID: 9209
		public float UNDERWATER_SPEED;

		// Token: 0x040023FA RID: 9210
		public float JUMP_SPEED;

		// Token: 0x040023FB RID: 9211
		public float JUMP_SPEED_UNDERWATER;

		// Token: 0x040023FC RID: 9212
		public float MIN_JUMP_SPEED;

		// Token: 0x040023FD RID: 9213
		public int JUMP_STEPS;

		// Token: 0x040023FE RID: 9214
		public int JUMP_STEPS_MIN;

		// Token: 0x040023FF RID: 9215
		public int JUMP_TIME;

		// Token: 0x04002400 RID: 9216
		public int DOUBLE_JUMP_STEPS;

		// Token: 0x04002401 RID: 9217
		public int WJLOCK_STEPS_SHORT;

		// Token: 0x04002402 RID: 9218
		public int WJLOCK_STEPS_LONG;

		// Token: 0x04002403 RID: 9219
		public float WJ_KICKOFF_SPEED;

		// Token: 0x04002404 RID: 9220
		public int WALL_STICKY_STEPS;

		// Token: 0x04002405 RID: 9221
		public float DASH_SPEED;

		// Token: 0x04002406 RID: 9222
		public float DASH_SPEED_SHARP;

		// Token: 0x04002407 RID: 9223
		public float DASH_TIME;

		// Token: 0x04002408 RID: 9224
		public int DASH_QUEUE_STEPS;

		// Token: 0x04002409 RID: 9225
		public float BACK_DASH_SPEED;

		// Token: 0x0400240A RID: 9226
		public float BACK_DASH_TIME;

		// Token: 0x0400240B RID: 9227
		public float SHADOW_DASH_SPEED;

		// Token: 0x0400240C RID: 9228
		public float SHADOW_DASH_TIME;

		// Token: 0x0400240D RID: 9229
		public float SHADOW_DASH_COOLDOWN;

		// Token: 0x0400240E RID: 9230
		public float SUPER_DASH_SPEED;

		// Token: 0x0400240F RID: 9231
		public float DASH_COOLDOWN;

		// Token: 0x04002410 RID: 9232
		public float DASH_COOLDOWN_CH;

		// Token: 0x04002411 RID: 9233
		public float BACKDASH_COOLDOWN;

		// Token: 0x04002412 RID: 9234
		public float WALLSLIDE_SPEED;

		// Token: 0x04002413 RID: 9235
		public float WALLSLIDE_DECEL;

		// Token: 0x04002414 RID: 9236
		public float NAIL_CHARGE_TIME_DEFAULT;

		// Token: 0x04002415 RID: 9237
		public float NAIL_CHARGE_TIME_CHARM;

		// Token: 0x04002416 RID: 9238
		public float CYCLONE_HORIZONTAL_SPEED;

		// Token: 0x04002417 RID: 9239
		public float SWIM_ACCEL;

		// Token: 0x04002418 RID: 9240
		public float SWIM_MAX_SPEED;

		// Token: 0x04002419 RID: 9241
		public float TIME_TO_ENTER_SCENE_BOT;

		// Token: 0x0400241A RID: 9242
		public float TIME_TO_ENTER_SCENE_HOR;

		// Token: 0x0400241B RID: 9243
		public float SPEED_TO_ENTER_SCENE_HOR;

		// Token: 0x0400241C RID: 9244
		public float SPEED_TO_ENTER_SCENE_UP;

		// Token: 0x0400241D RID: 9245
		public float SPEED_TO_ENTER_SCENE_DOWN;

		// Token: 0x0400241E RID: 9246
		public float DEFAULT_GRAVITY;

		// Token: 0x0400241F RID: 9247
		public float UNDERWATER_GRAVITY;

		// Token: 0x04002420 RID: 9248
		public float ATTACK_DURATION;

		// Token: 0x04002421 RID: 9249
		public float ATTACK_DURATION_CH;

		// Token: 0x04002422 RID: 9250
		public float ALT_ATTACK_RESET;

		// Token: 0x04002423 RID: 9251
		public float ATTACK_RECOVERY_TIME;

		// Token: 0x04002424 RID: 9252
		public float ATTACK_COOLDOWN_TIME;

		// Token: 0x04002425 RID: 9253
		public float ATTACK_COOLDOWN_TIME_CH;

		// Token: 0x04002426 RID: 9254
		public float BOUNCE_TIME;

		// Token: 0x04002427 RID: 9255
		public float BOUNCE_SHROOM_TIME;

		// Token: 0x04002428 RID: 9256
		public float BOUNCE_VELOCITY;

		// Token: 0x04002429 RID: 9257
		public float SHROOM_BOUNCE_VELOCITY;

		// Token: 0x0400242A RID: 9258
		public float RECOIL_HOR_TIME;

		// Token: 0x0400242B RID: 9259
		public float RECOIL_HOR_VELOCITY;

		// Token: 0x0400242C RID: 9260
		public float RECOIL_HOR_VELOCITY_LONG;

		// Token: 0x0400242D RID: 9261
		public float RECOIL_HOR_STEPS;

		// Token: 0x0400242E RID: 9262
		public float RECOIL_DOWN_VELOCITY;

		// Token: 0x0400242F RID: 9263
		public float RUN_PUFF_TIME;

		// Token: 0x04002430 RID: 9264
		public float BIG_FALL_TIME;

		// Token: 0x04002431 RID: 9265
		public float HARD_LANDING_TIME;

		// Token: 0x04002432 RID: 9266
		public float DOWN_DASH_TIME;

		// Token: 0x04002433 RID: 9267
		public float MAX_FALL_VELOCITY;

		// Token: 0x04002434 RID: 9268
		public float MAX_FALL_VELOCITY_UNDERWATER;

		// Token: 0x04002435 RID: 9269
		public float RECOIL_DURATION;

		// Token: 0x04002436 RID: 9270
		public float RECOIL_DURATION_STAL;

		// Token: 0x04002437 RID: 9271
		public float RECOIL_VELOCITY;

		// Token: 0x04002438 RID: 9272
		public float DAMAGE_FREEZE_DOWN;

		// Token: 0x04002439 RID: 9273
		public float DAMAGE_FREEZE_WAIT;

		// Token: 0x0400243A RID: 9274
		public float DAMAGE_FREEZE_UP;

		// Token: 0x0400243B RID: 9275
		public float INVUL_TIME;

		// Token: 0x0400243C RID: 9276
		public float INVUL_TIME_STAL;

		// Token: 0x0400243D RID: 9277
		public float INVUL_TIME_PARRY;

		// Token: 0x0400243E RID: 9278
		public float INVUL_TIME_QUAKE;

		// Token: 0x0400243F RID: 9279
		public float INVUL_TIME_CYCLONE;

		// Token: 0x04002440 RID: 9280
		public float CAST_TIME;

		// Token: 0x04002441 RID: 9281
		public float CAST_RECOIL_TIME;

		// Token: 0x04002442 RID: 9282
		public float CAST_RECOIL_VELOCITY;

		// Token: 0x04002443 RID: 9283
		public float WALLSLIDE_CLIP_DELAY;

		// Token: 0x04002444 RID: 9284
		public int GRUB_SOUL_MP;

		// Token: 0x04002445 RID: 9285
		public int GRUB_SOUL_MP_COMBO;

		// Token: 0x04002446 RID: 9286
		private int JUMP_QUEUE_STEPS = 2;

		// Token: 0x04002447 RID: 9287
		private int JUMP_RELEASE_QUEUE_STEPS = 2;

		// Token: 0x04002448 RID: 9288
		private int DOUBLE_JUMP_QUEUE_STEPS = 10;

		// Token: 0x04002449 RID: 9289
		private int ATTACK_QUEUE_STEPS = 5;

		// Token: 0x0400244A RID: 9290
		private float DELAY_BEFORE_ENTER = 0.1f;

		// Token: 0x0400244B RID: 9291
		private float LOOK_DELAY = 0.85f;

		// Token: 0x0400244C RID: 9292
		private float LOOK_ANIM_DELAY = 0.25f;

		// Token: 0x0400244D RID: 9293
		private float DEATH_WAIT = 2.85f;

		// Token: 0x0400244E RID: 9294
		private float HAZARD_DEATH_CHECK_TIME = 3f;

		// Token: 0x0400244F RID: 9295
		private float FLOATING_CHECK_TIME = 0.18f;

		// Token: 0x04002450 RID: 9296
		private float NAIL_TERRAIN_CHECK_TIME = 0.12f;

		// Token: 0x04002451 RID: 9297
		private float BUMP_VELOCITY = 4f;

		// Token: 0x04002452 RID: 9298
		private float BUMP_VELOCITY_DASH = 5f;

		// Token: 0x04002453 RID: 9299
		private int LANDING_BUFFER_STEPS = 5;

		// Token: 0x04002454 RID: 9300
		private int LEDGE_BUFFER_STEPS = 2;

		// Token: 0x04002455 RID: 9301
		private int HEAD_BUMP_STEPS = 3;

		// Token: 0x04002456 RID: 9302
		private float MANTIS_CHARM_SCALE = 1.35f;

		// Token: 0x04002457 RID: 9303
		private float FIND_GROUND_POINT_DISTANCE = 10f;

		// Token: 0x04002458 RID: 9304
		private float FIND_GROUND_POINT_DISTANCE_EXT = 50f;

		// Token: 0x04002459 RID: 9305
		public ActorStates hero_state;

		// Token: 0x0400245A RID: 9306
		public ActorStates prev_hero_state;

		// Token: 0x0400245B RID: 9307
		public HeroTransitionState transitionState;

		// Token: 0x0400245C RID: 9308
		public DamageMode damageMode;

		// Token: 0x0400245D RID: 9309
		public float move_input;

		// Token: 0x0400245E RID: 9310
		public float vertical_input;

		// Token: 0x0400245F RID: 9311
		public float controller_deadzone = 0.2f;

		// Token: 0x04002460 RID: 9312
		public Vector2 current_velocity;

		// Token: 0x04002461 RID: 9313
		private bool isGameplayScene;

		// Token: 0x04002462 RID: 9314
		public bool isEnteringFirstLevel;

		// Token: 0x04002463 RID: 9315
		public Vector2 slashOffset;

		// Token: 0x04002464 RID: 9316
		public Vector2 upSlashOffset;

		// Token: 0x04002465 RID: 9317
		public Vector2 downwardSlashOffset;

		// Token: 0x04002466 RID: 9318
		public Vector2 spell1Offset;

		// Token: 0x04002467 RID: 9319
		private int jump_steps;

		// Token: 0x04002468 RID: 9320
		private int jumped_steps;

		// Token: 0x04002469 RID: 9321
		private int doubleJump_steps;

		// Token: 0x0400246A RID: 9322
		private float dash_timer;

		// Token: 0x0400246B RID: 9323
		private float back_dash_timer;

		// Token: 0x0400246C RID: 9324
		private float shadow_dash_timer;

		// Token: 0x0400246D RID: 9325
		private float attack_time;

		// Token: 0x0400246E RID: 9326
		private float attack_cooldown;

		// Token: 0x0400246F RID: 9327
		private Vector2 transition_vel;

		// Token: 0x04002470 RID: 9328
		private float altAttackTime;

		// Token: 0x04002471 RID: 9329
		private float lookDelayTimer;

		// Token: 0x04002472 RID: 9330
		private float bounceTimer;

		// Token: 0x04002473 RID: 9331
		private float recoilHorizontalTimer;

		// Token: 0x04002474 RID: 9332
		private float runPuffTimer;

		// Token: 0x04002476 RID: 9334
		private float hardLandingTimer;

		// Token: 0x04002477 RID: 9335
		private float dashLandingTimer;

		// Token: 0x04002478 RID: 9336
		private float recoilTimer;

		// Token: 0x04002479 RID: 9337
		private int recoilSteps;

		// Token: 0x0400247A RID: 9338
		private int landingBufferSteps;

		// Token: 0x0400247B RID: 9339
		private int dashQueueSteps;

		// Token: 0x0400247C RID: 9340
		private bool dashQueuing;

		// Token: 0x0400247D RID: 9341
		private float shadowDashTimer;

		// Token: 0x0400247E RID: 9342
		private float dashCooldownTimer;

		// Token: 0x0400247F RID: 9343
		private float nailChargeTimer;

		// Token: 0x04002480 RID: 9344
		private int wallLockSteps;

		// Token: 0x04002481 RID: 9345
		private float wallslideClipTimer;

		// Token: 0x04002482 RID: 9346
		private float hardLandFailSafeTimer;

		// Token: 0x04002483 RID: 9347
		private float hazardDeathTimer;

		// Token: 0x04002484 RID: 9348
		private float floatingBufferTimer;

		// Token: 0x04002485 RID: 9349
		private float attackDuration;

		// Token: 0x04002486 RID: 9350
		public float parryInvulnTimer;

		// Token: 0x04002487 RID: 9351
		[Space(6f)]
		[Header("Slash Prefabs")]
		public GameObject slashPrefab;

		// Token: 0x04002488 RID: 9352
		public GameObject slashAltPrefab;

		// Token: 0x04002489 RID: 9353
		public GameObject upSlashPrefab;

		// Token: 0x0400248A RID: 9354
		public GameObject downSlashPrefab;

		// Token: 0x0400248B RID: 9355
		public GameObject wallSlashPrefab;

		// Token: 0x0400248C RID: 9356
		public NailSlash normalSlash;

		// Token: 0x0400248D RID: 9357
		public NailSlash alternateSlash;

		// Token: 0x0400248E RID: 9358
		public NailSlash upSlash;

		// Token: 0x0400248F RID: 9359
		public NailSlash downSlash;

		// Token: 0x04002490 RID: 9360
		public NailSlash wallSlash;

		// Token: 0x04002491 RID: 9361
		public PlayMakerFSM normalSlashFsm;

		// Token: 0x04002492 RID: 9362
		public PlayMakerFSM alternateSlashFsm;

		// Token: 0x04002493 RID: 9363
		public PlayMakerFSM upSlashFsm;

		// Token: 0x04002494 RID: 9364
		public PlayMakerFSM downSlashFsm;

		// Token: 0x04002495 RID: 9365
		public PlayMakerFSM wallSlashFsm;

		// Token: 0x04002496 RID: 9366
		[Header("Effect Prefabs")]
		[Space(6f)]
		public GameObject nailTerrainImpactEffectPrefab;

		// Token: 0x04002497 RID: 9367
		public GameObject spell1Prefab;

		// Token: 0x04002498 RID: 9368
		public GameObject takeHitPrefab;

		// Token: 0x04002499 RID: 9369
		public GameObject takeHitDoublePrefab;

		// Token: 0x0400249A RID: 9370
		public GameObject softLandingEffectPrefab;

		// Token: 0x0400249B RID: 9371
		public GameObject hardLandingEffectPrefab;

		// Token: 0x0400249C RID: 9372
		public GameObject runEffectPrefab;

		// Token: 0x0400249D RID: 9373
		public GameObject backDashPrefab;

		// Token: 0x0400249E RID: 9374
		public GameObject jumpEffectPrefab;

		// Token: 0x0400249F RID: 9375
		public GameObject jumpTrailPrefab;

		// Token: 0x040024A0 RID: 9376
		public GameObject fallEffectPrefab;

		// Token: 0x040024A1 RID: 9377
		public ParticleSystem wallslideDustPrefab;

		// Token: 0x040024A2 RID: 9378
		public GameObject artChargeEffect;

		// Token: 0x040024A3 RID: 9379
		public GameObject artChargedEffect;

		// Token: 0x040024A4 RID: 9380
		public GameObject artChargedFlash;

		// Token: 0x040024A5 RID: 9381
		public tk2dSpriteAnimator artChargedEffectAnim;

		// Token: 0x040024A6 RID: 9382
		public GameObject shadowdashBurstPrefab;

		// Token: 0x040024A7 RID: 9383
		public GameObject shadowdashDownBurstPrefab;

		// Token: 0x040024A8 RID: 9384
		public GameObject dashParticlesPrefab;

		// Token: 0x040024A9 RID: 9385
		public GameObject shadowdashParticlesPrefab;

		// Token: 0x040024AA RID: 9386
		public GameObject shadowRingPrefab;

		// Token: 0x040024AB RID: 9387
		public GameObject shadowRechargePrefab;

		// Token: 0x040024AC RID: 9388
		public GameObject dJumpWingsPrefab;

		// Token: 0x040024AD RID: 9389
		public GameObject dJumpFlashPrefab;

		// Token: 0x040024AE RID: 9390
		public ParticleSystem dJumpFeathers;

		// Token: 0x040024AF RID: 9391
		public GameObject wallPuffPrefab;

		// Token: 0x040024B0 RID: 9392
		public GameObject sharpShadowPrefab;

		// Token: 0x040024B1 RID: 9393
		public GameObject grubberFlyBeamPrefabL;

		// Token: 0x040024B2 RID: 9394
		public GameObject grubberFlyBeamPrefabR;

		// Token: 0x040024B3 RID: 9395
		public GameObject grubberFlyBeamPrefabU;

		// Token: 0x040024B4 RID: 9396
		public GameObject grubberFlyBeamPrefabD;

		// Token: 0x040024B5 RID: 9397
		public GameObject grubberFlyBeamPrefabL_fury;

		// Token: 0x040024B6 RID: 9398
		public GameObject grubberFlyBeamPrefabR_fury;

		// Token: 0x040024B7 RID: 9399
		public GameObject grubberFlyBeamPrefabU_fury;

		// Token: 0x040024B8 RID: 9400
		public GameObject grubberFlyBeamPrefabD_fury;

		// Token: 0x040024B9 RID: 9401
		public GameObject carefreeShield;

		// Token: 0x040024BA RID: 9402
		[Space(6f)]
		[Header("Hero Death")]
		public GameObject corpsePrefab;

		// Token: 0x040024BB RID: 9403
		public GameObject spikeDeathPrefab;

		// Token: 0x040024BC RID: 9404
		public GameObject acidDeathPrefab;

		// Token: 0x040024BD RID: 9405
		public GameObject lavaDeathPrefab;

		// Token: 0x040024BE RID: 9406
		public GameObject heroDeathPrefab;

		// Token: 0x040024BF RID: 9407
		[Header("Hero Other")]
		[Space(6f)]
		public GameObject cutscenePrefab;

		// Token: 0x040024C0 RID: 9408
		private GameManager gm;

		// Token: 0x040024C1 RID: 9409
		private Rigidbody2D rb2d;

		// Token: 0x040024C2 RID: 9410
		private Collider2D col2d;

		// Token: 0x040024C3 RID: 9411
		private MeshRenderer renderer;

		// Token: 0x040024C4 RID: 9412
		private new Transform transform;

		// Token: 0x040024C5 RID: 9413
		private HeroAnimationController animCtrl;

		// Token: 0x040024C6 RID: 9414
		public HeroControllerStates cState;

		// Token: 0x040024C7 RID: 9415
		public PlayerData playerData;

		// Token: 0x040024C8 RID: 9416
		private HeroAudioController audioCtrl;

		// Token: 0x040024C9 RID: 9417
		private AudioSource audioSource;

		// Token: 0x040024CA RID: 9418
		[HideInInspector]
		public UIManager ui;

		// Token: 0x040024CB RID: 9419
		private InputHandler inputHandler;

		// Token: 0x040024CE RID: 9422
		public PlayMakerFSM damageEffectFSM;

		// Token: 0x040024CF RID: 9423
		private ParticleSystem dashParticleSystem;

		// Token: 0x040024D0 RID: 9424
		private InvulnerablePulse invPulse;

		// Token: 0x040024D1 RID: 9425
		private SpriteFlash spriteFlash;

		// Token: 0x040024D2 RID: 9426
		public AudioSource footStepsRunAudioSource;

		// Token: 0x040024D3 RID: 9427
		public AudioSource footStepsWalkAudioSource;

		// Token: 0x040024D4 RID: 9428
		private float prevGravityScale;

		// Token: 0x040024D5 RID: 9429
		private Vector2 recoilVector;

		// Token: 0x040024D6 RID: 9430
		private Vector2 lastInputState;

		// Token: 0x040024D7 RID: 9431
		public GatePosition gatePosition;

		// Token: 0x040024D9 RID: 9433
		private bool runMsgSent;

		// Token: 0x040024DA RID: 9434
		private bool hardLanded;

		// Token: 0x040024DB RID: 9435
		private bool fallRumble;

		// Token: 0x040024DC RID: 9436
		public bool acceptingInput;

		// Token: 0x040024DD RID: 9437
		private bool fallTrailGenerated;

		// Token: 0x040024DE RID: 9438
		private bool drainMP;

		// Token: 0x040024DF RID: 9439
		private float drainMP_timer;

		// Token: 0x040024E0 RID: 9440
		private float drainMP_time;

		// Token: 0x040024E1 RID: 9441
		private float MP_drained;

		// Token: 0x040024E2 RID: 9442
		private float drainMP_seconds;

		// Token: 0x040024E3 RID: 9443
		private float focusMP_amount;

		// Token: 0x040024E4 RID: 9444
		private float dashBumpCorrection;

		// Token: 0x040024E5 RID: 9445
		public bool controlReqlinquished;

		// Token: 0x040024E6 RID: 9446
		public bool enterWithoutInput;

		// Token: 0x040024E7 RID: 9447
		public bool lookingUpAnim;

		// Token: 0x040024E8 RID: 9448
		public bool lookingDownAnim;

		// Token: 0x040024E9 RID: 9449
		public bool carefreeShieldEquipped;

		// Token: 0x040024EA RID: 9450
		private int hitsSinceShielded;

		// Token: 0x040024EB RID: 9451
		private EndBeta endBeta;

		// Token: 0x040024EC RID: 9452
		private int jumpQueueSteps;

		// Token: 0x040024ED RID: 9453
		private bool jumpQueuing;

		// Token: 0x040024EE RID: 9454
		private int doubleJumpQueueSteps;

		// Token: 0x040024EF RID: 9455
		private bool doubleJumpQueuing;

		// Token: 0x040024F0 RID: 9456
		private int jumpReleaseQueueSteps;

		// Token: 0x040024F1 RID: 9457
		private bool jumpReleaseQueuing;

		// Token: 0x040024F2 RID: 9458
		private int attackQueueSteps;

		// Token: 0x040024F3 RID: 9459
		private bool attackQueuing;

		// Token: 0x040024F4 RID: 9460
		public bool touchingWallL;

		// Token: 0x040024F5 RID: 9461
		public bool touchingWallR;

		// Token: 0x040024F6 RID: 9462
		public bool wallSlidingL;

		// Token: 0x040024F7 RID: 9463
		public bool wallSlidingR;

		// Token: 0x040024F8 RID: 9464
		private bool airDashed;

		// Token: 0x040024F9 RID: 9465
		public bool dashingDown;

		// Token: 0x040024FA RID: 9466
		public bool wieldingLantern;

		// Token: 0x040024FB RID: 9467
		private bool startWithWallslide;

		// Token: 0x040024FC RID: 9468
		private bool startWithJump;

		// Token: 0x040024FD RID: 9469
		private bool startWithFullJump;

		// Token: 0x040024FE RID: 9470
		private bool startWithDash;

		// Token: 0x040024FF RID: 9471
		private bool startWithAttack;

		// Token: 0x04002500 RID: 9472
		private bool nailArt_cyclone;

		// Token: 0x04002501 RID: 9473
		private bool wallSlashing;

		// Token: 0x04002502 RID: 9474
		private bool doubleJumped;

		// Token: 0x04002503 RID: 9475
		public bool inAcid;

		// Token: 0x04002504 RID: 9476
		private bool wallJumpedR;

		// Token: 0x04002505 RID: 9477
		private bool wallJumpedL;

		// Token: 0x04002506 RID: 9478
		public bool wallLocked;

		// Token: 0x04002507 RID: 9479
		private float currentWalljumpSpeed;

		// Token: 0x04002508 RID: 9480
		private float walljumpSpeedDecel;

		// Token: 0x04002509 RID: 9481
		private int wallUnstickSteps;

		// Token: 0x0400250A RID: 9482
		private bool recoilLarge;

		// Token: 0x0400250B RID: 9483
		public float conveyorSpeed;

		// Token: 0x0400250C RID: 9484
		public float conveyorSpeedV;

		// Token: 0x0400250D RID: 9485
		private bool enteringVertically;

		// Token: 0x0400250E RID: 9486
		private bool playingWallslideClip;

		// Token: 0x0400250F RID: 9487
		private bool playedMantisClawClip;

		// Token: 0x04002510 RID: 9488
		public bool exitedSuperDashing;

		// Token: 0x04002511 RID: 9489
		public bool exitedQuake;

		// Token: 0x04002512 RID: 9490
		private bool fallCheckFlagged;

		// Token: 0x04002513 RID: 9491
		private int ledgeBufferSteps;

		// Token: 0x04002514 RID: 9492
		private int headBumpSteps;

		// Token: 0x04002515 RID: 9493
		private float nailChargeTime;

		// Token: 0x04002516 RID: 9494
		public bool takeNoDamage;

		// Token: 0x04002517 RID: 9495
		private bool joniBeam;

		// Token: 0x04002518 RID: 9496
		public bool fadedSceneIn;

		// Token: 0x04002519 RID: 9497
		private bool stopWalkingOut;

		// Token: 0x0400251A RID: 9498
		private bool boundsChecking;

		// Token: 0x0400251B RID: 9499
		private bool blockerFix;

		// Token: 0x0400251C RID: 9500
		[SerializeField]
		private Vector2[] positionHistory;

		// Token: 0x0400251D RID: 9501
		private bool tilemapTestActive;

		// Token: 0x0400251E RID: 9502
		private Vector2 groundRayOriginC;

		// Token: 0x0400251F RID: 9503
		private Vector2 groundRayOriginL;

		// Token: 0x04002520 RID: 9504
		private Vector2 groundRayOriginR;

		// Token: 0x04002521 RID: 9505
		private Coroutine takeDamageCoroutine;

		// Token: 0x04002522 RID: 9506
		private Coroutine tilemapTestCoroutine;

		// Token: 0x04002523 RID: 9507
		public AudioClip footstepsRunDust;

		// Token: 0x04002524 RID: 9508
		public AudioClip footstepsRunGrass;

		// Token: 0x04002525 RID: 9509
		public AudioClip footstepsRunBone;

		// Token: 0x04002526 RID: 9510
		public AudioClip footstepsRunSpa;

		// Token: 0x04002527 RID: 9511
		public AudioClip footstepsRunMetal;

		// Token: 0x04002528 RID: 9512
		public AudioClip footstepsRunWater;

		// Token: 0x04002529 RID: 9513
		public AudioClip footstepsWalkDust;

		// Token: 0x0400252A RID: 9514
		public AudioClip footstepsWalkGrass;

		// Token: 0x0400252B RID: 9515
		public AudioClip footstepsWalkBone;

		// Token: 0x0400252C RID: 9516
		public AudioClip footstepsWalkSpa;

		// Token: 0x0400252D RID: 9517
		public AudioClip footstepsWalkMetal;

		// Token: 0x0400252E RID: 9518
		public AudioClip nailArtCharge;

		// Token: 0x0400252F RID: 9519
		public AudioClip nailArtChargeComplete;

		// Token: 0x04002530 RID: 9520
		public AudioClip blockerImpact;

		// Token: 0x04002531 RID: 9521
		public AudioClip shadowDashClip;

		// Token: 0x04002532 RID: 9522
		public AudioClip sharpShadowClip;

		// Token: 0x04002533 RID: 9523
		public AudioClip doubleJumpClip;

		// Token: 0x04002534 RID: 9524
		public AudioClip mantisClawClip;

		// Token: 0x04002535 RID: 9525
		private GameObject slash;

		// Token: 0x04002536 RID: 9526
		private NailSlash slashComponent;

		// Token: 0x04002537 RID: 9527
		private PlayMakerFSM slashFsm;

		// Token: 0x04002538 RID: 9528
		private GameObject runEffect;

		// Token: 0x04002539 RID: 9529
		private GameObject backDash;

		// Token: 0x0400253A RID: 9530
		private GameObject jumpEffect;

		// Token: 0x0400253B RID: 9531
		private GameObject fallEffect;

		// Token: 0x0400253C RID: 9532
		private GameObject dashEffect;

		// Token: 0x0400253D RID: 9533
		private GameObject grubberFlyBeam;

		// Token: 0x0400253E RID: 9534
		private GameObject hazardCorpe;

		// Token: 0x0400253F RID: 9535
		public PlayMakerFSM vignetteFSM;

		// Token: 0x04002540 RID: 9536
		public SpriteRenderer heroLight;

		// Token: 0x04002541 RID: 9537
		public SpriteRenderer vignette;

		// Token: 0x04002542 RID: 9538
		public PlayMakerFSM dashBurst;

		// Token: 0x04002543 RID: 9539
		public PlayMakerFSM superDash;

		// Token: 0x04002544 RID: 9540
		public PlayMakerFSM fsm_thornCounter;

		// Token: 0x04002545 RID: 9541
		public PlayMakerFSM spellControl;

		// Token: 0x04002546 RID: 9542
		public PlayMakerFSM fsm_fallTrail;

		// Token: 0x04002547 RID: 9543
		public PlayMakerFSM fsm_orbitShield;

		// Token: 0x04002548 RID: 9544

		// Token: 0x0400254F RID: 9551
		public bool isHeroInPosition = true;

		// Token: 0x04002552 RID: 9554
		private bool jumpReleaseQueueingEnabled;

		// Token: 0x04002553 RID: 9555
		private static HeroController _instance;

		// Token: 0x04002554 RID: 9556
		private const float PreventCastByDialogueEndDuration = 0.3f;

		// Token: 0x04002555 RID: 9557
		private float preventCastByDialogueEndTimer;

		// Token: 0x04002556 RID: 9558
		private Vector2 oldPos = Vector2.zero;

		// Token: 0x02000771 RID: 1905
		// (Invoke) Token: 0x060022BF RID: 8895
		public delegate void HeroInPosition(bool forceDirect);

		// Token: 0x02000772 RID: 1906
		// (Invoke) Token: 0x060022C3 RID: 8899
		public delegate void TakeDamageEvent();

		// Token: 0x02000773 RID: 1907
		// (Invoke) Token: 0x060022C7 RID: 8903
		public delegate void HeroDeathEvent();
	}

}*/