using System;
using System.Collections;
using System.Collections.Generic;
using GlobalEnums;
using Modding;
using UnityEngine;

// Token: 0x020000F7 RID: 247
public class HeroController : MonoBehaviour
{
	public abstract class INTERNAL_INPUTMANAGER
	{
		public abstract bool IsInputPressed(string inputName);
		public abstract bool WasInputPressed(string inputName);
		public abstract bool WasInputReleased(string inputName);
		public abstract Vector2 GetInputVector(string joystickName);
	}

	// Token: 0x17000096 RID: 150
	// (get) Token: 0x06000527 RID: 1319 RVA: 0x0001B59B File Offset: 0x0001979B
	// (set) Token: 0x06000528 RID: 1320 RVA: 0x0001B5A3 File Offset: 0x000197A3
	public float fallTimer { get; private set; }

	// Token: 0x17000097 RID: 151
	// (get) Token: 0x06000529 RID: 1321 RVA: 0x0001B5AC File Offset: 0x000197AC
	// (set) Token: 0x0600052A RID: 1322 RVA: 0x0001B5B4 File Offset: 0x000197B4
	//public GeoCounter geoCounter { get; private set; }

	// Token: 0x17000098 RID: 152
	// (get) Token: 0x0600052B RID: 1323 RVA: 0x0001B5BD File Offset: 0x000197BD
	// (set) Token: 0x0600052C RID: 1324 RVA: 0x0001B5C5 File Offset: 0x000197C5
	//public PlayMakerFSM proxyFSM { get; private set; }

	// Token: 0x17000099 RID: 153
	// (get) Token: 0x0600052D RID: 1325 RVA: 0x0001B5CE File Offset: 0x000197CE
	// (set) Token: 0x0600052E RID: 1326 RVA: 0x0001B5D6 File Offset: 0x000197D6
	public TransitionPoint sceneEntryGate { get; private set; }

	// Token: 0x14000008 RID: 8
	// (add) Token: 0x0600052F RID: 1327 RVA: 0x0001B5E0 File Offset: 0x000197E0
	// (remove) Token: 0x06000530 RID: 1328 RVA: 0x0001B618 File Offset: 0x00019818
	public event HeroController.HeroInPosition heroInPosition;

	// Token: 0x14000009 RID: 9
	// (add) Token: 0x06000531 RID: 1329 RVA: 0x0001B650 File Offset: 0x00019850
	// (remove) Token: 0x06000532 RID: 1330 RVA: 0x0001B688 File Offset: 0x00019888
	public event HeroController.TakeDamageEvent OnTakenDamage;

	// Token: 0x1400000A RID: 10
	// (add) Token: 0x06000533 RID: 1331 RVA: 0x0001B6C0 File Offset: 0x000198C0
	// (remove) Token: 0x06000534 RID: 1332 RVA: 0x0001B6F8 File Offset: 0x000198F8
	public event HeroController.HeroDeathEvent OnDeath;

	// Token: 0x1700009A RID: 154
	// (get) Token: 0x06000535 RID: 1333 RVA: 0x0001B730 File Offset: 0x00019930
	public bool IsDreamReturning
	{
		get
		{
			/*PlayMakerFSM playMakerFSM = PlayMakerFSM.FindFsmOnGameObject(base.gameObject, "Dream Return");
			if (playMakerFSM)
			{
				FsmBool fsmBool = playMakerFSM.FsmVariables.FindFsmBool("Dream Returning");
				if (fsmBool != null)
				{
					return fsmBool.Value;
				}
			}*/
			return false;
		}
	}

	// Token: 0x1700009B RID: 155
	// (get) Token: 0x06000536 RID: 1334 RVA: 0x0001B772 File Offset: 0x00019972
	public static HeroController instance
	{
		get
		{
			HeroController silentInstance = HeroController.SilentInstance;
			if (!silentInstance)
			{
				//Debug.LogError("Couldn't find a Hero, make sure one exists in the scene.");
			}
			return silentInstance;
		}
	}

	// Token: 0x1700009C RID: 156
	// (get) Token: 0x06000537 RID: 1335 RVA: 0x0001B78B File Offset: 0x0001998B
	public static HeroController SilentInstance
	{
		get
		{
			if (HeroController._instance == null)
			{
				HeroController._instance = UnityEngine.Object.FindObjectOfType<HeroController>();
				if (HeroController._instance && Application.isPlaying)
				{
					UnityEngine.Object.DontDestroyOnLoad(HeroController._instance.gameObject);
				}
			}
			return HeroController._instance;
		}
	}

	// Token: 0x1700009D RID: 157
	// (get) Token: 0x06000538 RID: 1336 RVA: 0x0001B7CB File Offset: 0x000199CB
	public static HeroController UnsafeInstance
	{
		get
		{
			return HeroController._instance;
		}
	}

	INTERNAL_INPUTMANAGER _inputManager;
	INTERNAL_INPUTMANAGER InputManager
	{
		get
		{
			if (_inputManager == null)
			{
				foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach (var type in asm.GetTypes())
					{
						if (type != typeof(INTERNAL_INPUTMANAGER) && typeof(INTERNAL_INPUTMANAGER).IsAssignableFrom(type))
						{
							_inputManager = (INTERNAL_INPUTMANAGER)Activator.CreateInstance(type);
							return _inputManager;
						}
					}
				}
			}
			return _inputManager;
		}
	}

	bool RightPressed => InputManager.IsInputPressed("right");
	bool LeftPressed => InputManager.IsInputPressed("left");
	bool UpPressed => InputManager.IsInputPressed("up");
	bool DownPressed => InputManager.IsInputPressed("down");

	bool JumpPressed => InputManager.IsInputPressed("jump");
	bool JumpWasPressed => InputManager.WasInputPressed("jump");
	bool JumpWasReleased => InputManager.WasInputReleased("jump");

	bool DashPressed => InputManager.IsInputPressed("dash");
	bool DashWasPressed => InputManager.WasInputPressed("dash");
	bool DashWasReleased => InputManager.WasInputReleased("dash");

	bool AttackPressed => InputManager.IsInputPressed("attack");
	bool AttackWasPressed => InputManager.WasInputPressed("attack");
	bool AttackWasReleased => InputManager.WasInputReleased("attack");

	bool RightWasPressed => InputManager.WasInputPressed("right");
	bool LeftWasPressed => InputManager.WasInputPressed("left");
	bool UpWasPressed => InputManager.WasInputPressed("up");
	bool DownWasPressed => InputManager.WasInputPressed("down");

	Vector2 InputVector => InputManager.GetInputVector("moveVector");

	/*bool RightPressed => Input.GetAxisRaw("Horizontal") > 0.5f || Input.GetAxisRaw("Dpad Horizontal") > 0.5f);
	bool LeftPressed => Input.GetAxisRaw("Horizontal") < -0.5f || Input.GetAxisRaw("Dpad Horizontal") < -0.5f);
	bool UpPressed => Input.GetAxisRaw("Vertical") > 0.5f || Input.GetAxisRaw("Dpad Vertical") > 0.5f);
	bool DownPressed => Input.GetAxisRaw("Vertical") < -0.5f || Input.GetAxisRaw("Dpad Vertical") < -0.5f);

	bool JumpPressed => Input.GetButton("Jump");
	bool JumpWasPressed => Input.GetButtonDown("Jump");
	bool JumpWasReleased => Input.GetButtonUp("Jump");

	bool DashPressed => Input.GetButton("Dash Keyboard");
	bool DashWasPressed => Input.GetButtonDown("Dash Keyboard");
	bool DashWasReleased => Input.GetButtonUp("Dash Keyboard");

	bool AttackPressed => Input.GetButton("Attack");
	bool AttackWasPressed => Input.GetButtonDown("Attack");
	bool AttackWasReleased => Input.GetButtonUp("Attack");


	bool RightWasPressed => Input.GetAxisRaw("Horizontal") > 0.1f && prevHorizontalState <= 0f;
	bool LeftWasPressed => Input.GetAxisRaw("Horizontal") < -0.1f && prevHorizontalState >= 0f;
	bool UpWasPressed => Input.GetAxisRaw("Vertical") > 0.1f && prevVerticalState <= 0f;
	bool DownWasPressed => Input.GetAxisRaw("Vertical") < -0.1f && prevVerticalState >= 0f;

	float prevHorizontalState = 0f;
	float prevVerticalState = 0f;

	Vector2 InputVector => new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));*/

	// Token: 0x06000539 RID: 1337 RVA: 0x0001B7D4 File Offset: 0x000199D4
	private void Awake()
	{
		if (HeroController._instance == null)
		{
			HeroController._instance = this;
			UnityEngine.Object.DontDestroyOnLoad(this);
		}
		else if (this != HeroController._instance)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		this.SetupGameRefs();
		this.SetupPools();
	}

	// Token: 0x0600053A RID: 1338 RVA: 0x0001B824 File Offset: 0x00019A24
	private void Start()
	{
		heroInPosition += (b) => isHeroInPosition = true;
		/*this.heroInPosition += delegate (bool < p0 >)
		{
			this.isHeroInPosition = true;
		};*/
		//this.ui = UIManager.instance;
		/*this.geoCounter = GameCameras.instance.geoCounter;
		if (this.superDash == null)
		{
			Debug.Log("SuperDash came up null, locating manually");
			this.superDash = FSMUtility.LocateFSM(base.gameObject, "Superdash");
		}
		if (this.fsm_thornCounter == null)
		{
			Debug.Log("Thorn Counter came up null, locating manually");
			this.fsm_thornCounter = FSMUtility.LocateFSM(this.transform.Find("Charm Effects").gameObject, "Thorn Counter");
		}
		if (this.dashBurst == null)
		{
			Debug.Log("DashBurst came up null, locating manually");
			this.dashBurst = FSMUtility.GetFSM(this.transform.Find("Effects").Find("Dash Burst").gameObject);
		}
		if (this.spellControl == null)
		{
			Debug.Log("SpellControl came up null, locating manually");
			this.spellControl = FSMUtility.LocateFSM(base.gameObject, "Spell Control");
		}*/
		if (this.playerData.GetBool("equippedCharm_26"))
		{
			this.nailChargeTime = this.NAIL_CHARGE_TIME_CHARM;
		}
		else
		{
			this.nailChargeTime = this.NAIL_CHARGE_TIME_DEFAULT;
		}
		if (this.gm.IsGameplayScene())
		{
			this.isGameplayScene = true;
			//this.vignette.enabled = true;
			if (this.heroInPosition != null)
			{
				this.heroInPosition(false);
			}
			this.FinishedEnteringScene(true, false);
		}
		else
		{
			this.isGameplayScene = false;
			this.transform.SetPositionY(-2000f);
			//this.vignette.enabled = false;
			this.AffectedByGravity(false);
		}
		this.CharmUpdate();
		/*if (this.acidDeathPrefab)
		{
			ObjectPool.CreatePool(this.acidDeathPrefab, 1);
		}
		if (this.spikeDeathPrefab)
		{
			ObjectPool.CreatePool(this.spikeDeathPrefab, 1);
		}*/
	}

	// Token: 0x0600053B RID: 1339 RVA: 0x0001BA10 File Offset: 0x00019C10
	public void SceneInit()
	{
		if (this == HeroController._instance)
		{
			if (!this.gm)
			{
				this.gm = GameManager.instance;
			}
			if (this.gm.IsGameplayScene())
			{
				this.isGameplayScene = true;
				HeroBox.inactive = false;
			}
			else
			{
				this.isGameplayScene = false;
				this.acceptingInput = false;
				this.SetState(ActorStates.no_input);
				this.transform.SetPositionY(-2000f);
				//this.vignette.enabled = false;
				this.AffectedByGravity(false);
			}
			this.transform.SetPositionZ(0.004f);
			if (!this.blockerFix)
			{
				if (this.playerData.GetBool("killedBlocker"))
				{
					this.gm.SetPlayerDataInt("killsBlocker", 0);
				}
				this.blockerFix = true;
			}
			this.SetWalkZone(false);
		}
	}

	// Token: 0x0600053C RID: 1340 RVA: 0x0001BAE2 File Offset: 0x00019CE2
	private void Update()
	{
		this.orig_Update();

	}

	private void LateUpdate()
	{
		//prevHorizontalState = Input.GetAxisRaw("Horizontal");
		//prevVerticalState = Input.GetAxisRaw("Vertical");
	}

	// Token: 0x0600053D RID: 1341 RVA: 0x0001BAF0 File Offset: 0x00019CF0
	private void FixedUpdate()
	{
		if (this.cState.recoilingLeft || this.cState.recoilingRight)
		{
			if ((float)this.recoilSteps <= this.RECOIL_HOR_STEPS)
			{
				this.recoilSteps++;
			}
			else
			{
				this.CancelRecoilHorizontal();
			}
		}
		if (this.cState.dead)
		{
			this.rb2d.velocity = new Vector2(0f, 0f);
		}
		if ((this.hero_state == ActorStates.hard_landing && !this.cState.onConveyor) || this.hero_state == ActorStates.dash_landing)
		{
			this.ResetMotion();
		}
		else if (this.hero_state == ActorStates.no_input)
		{
			if (this.cState.transitioning)
			{
				if (this.transitionState == HeroTransitionState.EXITING_SCENE)
				{
					this.AffectedByGravity(false);
					if (!this.stopWalkingOut)
					{
						this.rb2d.velocity = new Vector2(this.transition_vel.x, this.transition_vel.y + this.rb2d.velocity.y);
					}
				}
				else if (this.transitionState == HeroTransitionState.ENTERING_SCENE)
				{
					this.rb2d.velocity = this.transition_vel;
				}
				else if (this.transitionState == HeroTransitionState.DROPPING_DOWN)
				{
					this.rb2d.velocity = new Vector2(this.transition_vel.x, this.rb2d.velocity.y);
				}
			}
			else if (this.cState.recoiling)
			{
				this.AffectedByGravity(false);
				this.rb2d.velocity = this.recoilVector;
			}
		}
		else if (this.hero_state != ActorStates.no_input)
		{
			if (this.hero_state == ActorStates.running)
			{
				if (this.move_input > 0f)
				{
					if (this.CheckForBump(CollisionSide.right))
					{
						this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, this.BUMP_VELOCITY);
					}
				}
				else if (this.move_input < 0f && this.CheckForBump(CollisionSide.left))
				{
					this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, this.BUMP_VELOCITY);
				}
			}
			if (!this.cState.backDashing && !this.cState.dashing)
			{
				this.Move(this.move_input);
				if ((!this.cState.attacking || this.attack_time >= this.ATTACK_RECOVERY_TIME) && !this.cState.wallSliding && !this.wallLocked)
				{
					if (this.move_input > 0f && !this.cState.facingRight)
					{
						this.FlipSprite();
						this.CancelAttack();
					}
					else if (this.move_input < 0f && this.cState.facingRight)
					{
						this.FlipSprite();
						this.CancelAttack();
					}
				}
				if (this.cState.recoilingLeft)
				{
					float num;
					if (this.recoilLarge)
					{
						num = this.RECOIL_HOR_VELOCITY_LONG;
					}
					else
					{
						num = this.RECOIL_HOR_VELOCITY;
					}
					if (this.rb2d.velocity.x > -num)
					{
						this.rb2d.velocity = new Vector2(-num, this.rb2d.velocity.y);
					}
					else
					{
						this.rb2d.velocity = new Vector2(this.rb2d.velocity.x - num, this.rb2d.velocity.y);
					}
				}
				if (this.cState.recoilingRight)
				{
					float num2;
					if (this.recoilLarge)
					{
						num2 = this.RECOIL_HOR_VELOCITY_LONG;
					}
					else
					{
						num2 = this.RECOIL_HOR_VELOCITY;
					}
					if (this.rb2d.velocity.x < num2)
					{
						this.rb2d.velocity = new Vector2(num2, this.rb2d.velocity.y);
					}
					else
					{
						this.rb2d.velocity = new Vector2(this.rb2d.velocity.x + num2, this.rb2d.velocity.y);
					}
				}
			}
			if ((this.cState.lookingUp || this.cState.lookingDown) && Mathf.Abs(this.move_input) > 0.6f)
			{
				this.ResetLook();
			}
			if (this.cState.jumping)
			{
				this.Jump();
			}
			if (this.cState.doubleJumping)
			{
				this.DoubleJump();
			}
			if (this.cState.dashing)
			{
				this.Dash();
			}
			if (this.cState.casting)
			{
				if (this.cState.castRecoiling)
				{
					if (this.cState.facingRight)
					{
						this.rb2d.velocity = new Vector2(-this.CAST_RECOIL_VELOCITY, 0f);
					}
					else
					{
						this.rb2d.velocity = new Vector2(this.CAST_RECOIL_VELOCITY, 0f);
					}
				}
				else
				{
					this.rb2d.velocity = Vector2.zero;
				}
			}
			if (this.cState.bouncing)
			{
				this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, this.BOUNCE_VELOCITY);
			}
			bool shroomBouncing = this.cState.shroomBouncing;
			if (this.wallLocked)
			{
				if (this.wallJumpedR)
				{
					this.rb2d.velocity = new Vector2(this.currentWalljumpSpeed, this.rb2d.velocity.y);
				}
				else if (this.wallJumpedL)
				{
					this.rb2d.velocity = new Vector2(-this.currentWalljumpSpeed, this.rb2d.velocity.y);
				}
				this.wallLockSteps++;
				if (this.wallLockSteps > this.WJLOCK_STEPS_LONG)
				{
					this.wallLocked = false;
				}
				this.currentWalljumpSpeed -= this.walljumpSpeedDecel;
			}
			if (this.cState.wallSliding)
			{
				if (this.wallSlidingL && RightPressed/*this.inputHandler.inputActions.right.IsPressed*/)
				{
					this.wallUnstickSteps++;
				}
				else if (this.wallSlidingR && LeftPressed/*this.inputHandler.inputActions.left.IsPressed*/)
				{
					this.wallUnstickSteps++;
				}
				else
				{
					this.wallUnstickSteps = 0;
				}
				if (this.wallUnstickSteps >= this.WALL_STICKY_STEPS)
				{
					this.CancelWallsliding();
				}
				if (this.wallSlidingL)
				{
					if (!this.CheckStillTouchingWall(CollisionSide.left, false))
					{
						this.FlipSprite();
						this.CancelWallsliding();
					}
				}
				else if (this.wallSlidingR && !this.CheckStillTouchingWall(CollisionSide.right, false))
				{
					this.FlipSprite();
					this.CancelWallsliding();
				}
			}
		}
		if (this.rb2d.velocity.y < -this.MAX_FALL_VELOCITY && !this.inAcid && !this.controlReqlinquished && !this.cState.shadowDashing && !this.cState.spellQuake)
		{
			this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, -this.MAX_FALL_VELOCITY);
		}
		if (this.jumpQueuing)
		{
			this.jumpQueueSteps++;
		}
		if (this.doubleJumpQueuing)
		{
			this.doubleJumpQueueSteps++;
		}
		if (this.dashQueuing)
		{
			this.dashQueueSteps++;
		}
		if (this.attackQueuing)
		{
			this.attackQueueSteps++;
		}
		if (this.cState.wallSliding && !this.cState.onConveyorV)
		{
			if (this.rb2d.velocity.y > this.WALLSLIDE_SPEED)
			{
				this.rb2d.velocity = new Vector3(this.rb2d.velocity.x, this.rb2d.velocity.y - this.WALLSLIDE_DECEL);
				if (this.rb2d.velocity.y < this.WALLSLIDE_SPEED)
				{
					this.rb2d.velocity = new Vector3(this.rb2d.velocity.x, this.WALLSLIDE_SPEED);
				}
			}
			if (this.rb2d.velocity.y < this.WALLSLIDE_SPEED)
			{
				this.rb2d.velocity = new Vector3(this.rb2d.velocity.x, this.rb2d.velocity.y + this.WALLSLIDE_DECEL);
				if (this.rb2d.velocity.y < this.WALLSLIDE_SPEED)
				{
					this.rb2d.velocity = new Vector3(this.rb2d.velocity.x, this.WALLSLIDE_SPEED);
				}
			}
		}
		if (this.nailArt_cyclone)
		{
			//if (this.inputHandler.inputActions.right.IsPressed && !this.inputHandler.inputActions.left.IsPressed)
			if (RightPressed && !LeftPressed)
			{
				this.rb2d.velocity = new Vector3(this.CYCLONE_HORIZONTAL_SPEED, this.rb2d.velocity.y);
			}
			//else if (this.inputHandler.inputActions.left.IsPressed && !this.inputHandler.inputActions.right.IsPressed)
			else if (LeftPressed && !RightPressed)
			{
				this.rb2d.velocity = new Vector3(-this.CYCLONE_HORIZONTAL_SPEED, this.rb2d.velocity.y);
			}
			else
			{
				this.rb2d.velocity = new Vector3(0f, this.rb2d.velocity.y);
			}
		}
		if (this.cState.swimming)
		{
			this.rb2d.velocity = new Vector3(this.rb2d.velocity.x, this.rb2d.velocity.y + this.SWIM_ACCEL);
			if (this.rb2d.velocity.y > this.SWIM_MAX_SPEED)
			{
				this.rb2d.velocity = new Vector3(this.rb2d.velocity.x, this.SWIM_MAX_SPEED);
			}
		}
		if (this.cState.superDashOnWall && !this.cState.onConveyorV)
		{
			this.rb2d.velocity = new Vector3(0f, 0f);
		}
		if (this.cState.onConveyor && ((this.cState.onGround && !this.cState.superDashing) || this.hero_state == ActorStates.hard_landing))
		{
			if (this.cState.freezeCharge || this.hero_state == ActorStates.hard_landing || this.controlReqlinquished)
			{
				this.rb2d.velocity = new Vector3(0f, 0f);
			}
			this.rb2d.velocity = new Vector2(this.rb2d.velocity.x + this.conveyorSpeed, this.rb2d.velocity.y);
		}
		if (this.cState.inConveyorZone)
		{
			if (this.cState.freezeCharge || this.hero_state == ActorStates.hard_landing)
			{
				this.rb2d.velocity = new Vector3(0f, 0f);
			}
			this.rb2d.velocity = new Vector2(this.rb2d.velocity.x + this.conveyorSpeed, this.rb2d.velocity.y);
			//this.superDash.SendEvent("SLOPE CANCEL");
		}
		if (this.cState.slidingLeft && this.rb2d.velocity.x > -5f)
		{
			this.rb2d.velocity = new Vector2(-5f, this.rb2d.velocity.y);
		}
		if (this.landingBufferSteps > 0)
		{
			this.landingBufferSteps--;
		}
		if (this.ledgeBufferSteps > 0)
		{
			this.ledgeBufferSteps--;
		}
		if (this.headBumpSteps > 0)
		{
			this.headBumpSteps--;
		}
		if (this.jumpReleaseQueueSteps > 0)
		{
			this.jumpReleaseQueueSteps--;
		}
		this.positionHistory[1] = this.positionHistory[0];
		this.positionHistory[0] = this.transform.position;
		this.cState.wasOnGround = this.cState.onGround;
	}

	// Token: 0x0600053E RID: 1342 RVA: 0x0001C758 File Offset: 0x0001A958
	private void Update10()
	{
		if (this.isGameplayScene)
		{
			this.OutOfBoundsCheck();
		}
		float scaleX = this.transform.GetScaleX();
		if (scaleX < -1f)
		{
			this.transform.SetScaleX(-1f);
		}
		if (scaleX > 1f)
		{
			this.transform.SetScaleX(1f);
		}
		if (this.transform.position.z != 0.004f)
		{
			this.transform.SetPositionZ(0.004f);
		}
	}

	// Token: 0x0600053F RID: 1343 RVA: 0x0001C7D4 File Offset: 0x0001A9D4
	private void OnLevelUnload()
	{
		if (this.transform.parent != null)
		{
			this.SetHeroParent(null);
		}
	}

	// Token: 0x06000540 RID: 1344 RVA: 0x0001C7F0 File Offset: 0x0001A9F0
	private void OnDisable()
	{
		if (this.gm != null)
		{
			this.gm.UnloadingLevel -= this.OnLevelUnload;
		}
	}

	// Token: 0x06000541 RID: 1345 RVA: 0x0001C818 File Offset: 0x0001AA18
	private void Move(float move_direction)
	{
		if (this.cState.onGround)
		{
			this.SetState(ActorStates.grounded);
		}
		if (this.acceptingInput && !this.cState.wallSliding)
		{
			if (this.cState.inWalkZone)
			{
				this.rb2d.velocity = new Vector2(move_direction * this.WALK_SPEED, this.rb2d.velocity.y);
				return;
			}
			if (this.inAcid)
			{
				this.rb2d.velocity = new Vector2(move_direction * this.UNDERWATER_SPEED, this.rb2d.velocity.y);
				return;
			}
			if (this.playerData.GetBool("equippedCharm_37") && this.cState.onGround && this.playerData.GetBool("equippedCharm_31"))
			{
				this.rb2d.velocity = new Vector2(move_direction * this.RUN_SPEED_CH_COMBO, this.rb2d.velocity.y);
				return;
			}
			if (this.playerData.GetBool("equippedCharm_37") && this.cState.onGround)
			{
				this.rb2d.velocity = new Vector2(move_direction * this.RUN_SPEED_CH, this.rb2d.velocity.y);
				return;
			}
			this.rb2d.velocity = new Vector2(move_direction * this.RUN_SPEED, this.rb2d.velocity.y);
		}
	}

	// Token: 0x06000542 RID: 1346 RVA: 0x0001C988 File Offset: 0x0001AB88
	private void Jump()
	{
		if (this.jump_steps <= this.JUMP_STEPS)
		{
			if (this.inAcid)
			{
				this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, this.JUMP_SPEED_UNDERWATER);
			}
			else
			{
				this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, this.JUMP_SPEED);
			}
			this.jump_steps++;
			this.jumped_steps++;
			this.ledgeBufferSteps = 0;
			return;
		}
		this.CancelJump();
	}

	// Token: 0x06000543 RID: 1347 RVA: 0x0001CA28 File Offset: 0x0001AC28
	private void DoubleJump()
	{
		if (this.doubleJump_steps <= this.DOUBLE_JUMP_STEPS)
		{
			if (this.doubleJump_steps > 3)
			{
				this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, this.JUMP_SPEED * 1.1f);
			}
			this.doubleJump_steps++;
		}
		else
		{
			this.CancelDoubleJump();
		}
		if (this.cState.onGround)
		{
			this.CancelDoubleJump();
		}
	}

	// Token: 0x06000544 RID: 1348 RVA: 0x0001CAA4 File Offset: 0x0001ACA4
	public void Attack(AttackDirection attackDir)
	{
		ModHooks.OnAttack(attackDir);
		if (Time.timeSinceLevelLoad - this.altAttackTime > this.ALT_ATTACK_RESET)
		{
			this.cState.altAttack = false;
		}
		this.cState.attacking = true;
		if (this.playerData.GetBool("equippedCharm_32"))
		{
			this.attackDuration = this.ATTACK_DURATION_CH;
		}
		else
		{
			this.attackDuration = this.ATTACK_DURATION;
		}
		if (this.cState.wallSliding)
		{
			this.wallSlashing = true;
			//this.slashComponent = this.wallSlash;
			//this.slashFsm = this.wallSlashFsm;
		}
		else
		{
			this.wallSlashing = false;
			if (attackDir == AttackDirection.normal)
			{
				if (!this.cState.altAttack)
				{
					//this.slashComponent = this.normalSlash;
					//this.slashFsm = this.normalSlashFsm;
					this.cState.altAttack = true;
				}
				else
				{
					//this.slashComponent = this.alternateSlash;
					//this.slashFsm = this.alternateSlashFsm;
					this.cState.altAttack = false;
				}
				/*if (this.playerData.GetBool("equippedCharm_35"))
				{
					if ((this.playerData.health == this.playerData.maxHealth && !this.playerData.GetBool("equippedCharm_27")) || (this.joniBeam && this.playerData.GetBool("equippedCharm_27")))
					{
						if (base.transform.localScale.x < 0f)
						{
							this.grubberFlyBeam = this.grubberFlyBeamPrefabR.Spawn(base.transform.position);
						}
						else
						{
							this.grubberFlyBeam = this.grubberFlyBeamPrefabL.Spawn(base.transform.position);
						}
						if (this.playerData.equippedCharm_13)
						{
							this.grubberFlyBeam.transform.SetScaleY(this.MANTIS_CHARM_SCALE);
						}
						else
						{
							this.grubberFlyBeam.transform.SetScaleY(1f);
						}
					}
					if (this.playerData.health == 1 && this.playerData.equippedCharm_6 && this.playerData.healthBlue < 1)
					{
						if (base.transform.localScale.x < 0f)
						{
							this.grubberFlyBeam = this.grubberFlyBeamPrefabR_fury.Spawn(base.transform.position);
						}
						else
						{
							this.grubberFlyBeam = this.grubberFlyBeamPrefabL_fury.Spawn(base.transform.position);
						}
						if (this.playerData.equippedCharm_13)
						{
							this.grubberFlyBeam.transform.SetScaleY(this.MANTIS_CHARM_SCALE);
						}
						else
						{
							this.grubberFlyBeam.transform.SetScaleY(1f);
						}
					}
				}*/
			}
			else if (attackDir == AttackDirection.upward)
			{
				//this.slashComponent = this.upSlash;
				//this.slashFsm = this.upSlashFsm;
				this.cState.upAttacking = true;
				/*if (this.playerData.equippedCharm_35)
				{
					if ((this.playerData.health == this.playerData.maxHealth && !this.playerData.equippedCharm_27) || (this.joniBeam && this.playerData.equippedCharm_27))
					{
						this.grubberFlyBeam = this.grubberFlyBeamPrefabU.Spawn(base.transform.position);
						this.grubberFlyBeam.transform.SetScaleY(base.transform.localScale.x);
						this.grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 270f);
						if (this.playerData.equippedCharm_13)
						{
							this.grubberFlyBeam.transform.SetScaleY(this.grubberFlyBeam.transform.localScale.y * this.MANTIS_CHARM_SCALE);
						}
					}
					if (this.playerData.health == 1 && this.playerData.equippedCharm_6 && this.playerData.healthBlue < 1)
					{
						this.grubberFlyBeam = this.grubberFlyBeamPrefabU_fury.Spawn(base.transform.position);
						this.grubberFlyBeam.transform.SetScaleY(base.transform.localScale.x);
						this.grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 270f);
						if (this.playerData.equippedCharm_13)
						{
							this.grubberFlyBeam.transform.SetScaleY(this.grubberFlyBeam.transform.localScale.y * this.MANTIS_CHARM_SCALE);
						}
					}
				}*/
			}
			else if (attackDir == AttackDirection.downward)
			{
				//this.slashComponent = this.downSlash;
				//this.slashFsm = this.downSlashFsm;
				this.cState.downAttacking = true;
				/*if (this.playerData.equippedCharm_35)
				{
					if ((this.playerData.health == this.playerData.maxHealth && !this.playerData.equippedCharm_27) || (this.joniBeam && this.playerData.equippedCharm_27))
					{
						this.grubberFlyBeam = this.grubberFlyBeamPrefabD.Spawn(base.transform.position);
						this.grubberFlyBeam.transform.SetScaleY(base.transform.localScale.x);
						this.grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
						if (this.playerData.equippedCharm_13)
						{
							this.grubberFlyBeam.transform.SetScaleY(this.grubberFlyBeam.transform.localScale.y * this.MANTIS_CHARM_SCALE);
						}
					}
					if (this.playerData.health == 1 && this.playerData.equippedCharm_6 && this.playerData.healthBlue < 1)
					{
						this.grubberFlyBeam = this.grubberFlyBeamPrefabD_fury.Spawn(base.transform.position);
						this.grubberFlyBeam.transform.SetScaleY(base.transform.localScale.x);
						this.grubberFlyBeam.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
						if (this.playerData.equippedCharm_13)
						{
							this.grubberFlyBeam.transform.SetScaleY(this.grubberFlyBeam.transform.localScale.y * this.MANTIS_CHARM_SCALE);
						}
					}
				}*/
			}
		}
		if (this.cState.wallSliding)
		{
			/*if (this.cState.facingRight)
			{
				this.slashFsm.FsmVariables.GetFsmFloat("direction").Value = 180f;
			}
			else
			{
				this.slashFsm.FsmVariables.GetFsmFloat("direction").Value = 0f;
			}*/
		}
		/*else if (attackDir == AttackDirection.normal && this.cState.facingRight)
		{
			this.slashFsm.FsmVariables.GetFsmFloat("direction").Value = 0f;
		}
		else if (attackDir == AttackDirection.normal && !this.cState.facingRight)
		{
			this.slashFsm.FsmVariables.GetFsmFloat("direction").Value = 180f;
		}
		else if (attackDir == AttackDirection.upward)
		{
			this.slashFsm.FsmVariables.GetFsmFloat("direction").Value = 90f;
		}
		else if (attackDir == AttackDirection.downward)
		{
			this.slashFsm.FsmVariables.GetFsmFloat("direction").Value = 270f;
		}*/
		this.altAttackTime = Time.timeSinceLevelLoad;
		ModHooks.AfterAttack(attackDir);
		if (!this.cState.attacking)
		{
			return;
		}
		//TODO : MAIN ATTACK STUFF
		//this.slashComponent.StartSlash();
		/*if (this.playerData.equippedCharm_38)
		{
			this.fsm_orbitShield.SendEvent("SLASH");
		}*/
	}

	// Token: 0x06000545 RID: 1349 RVA: 0x0001D25C File Offset: 0x0001B45C
	private void Dash()
	{
		this.AffectedByGravity(false);
		this.ResetHardLandingTimer();
		if (this.dash_timer > this.DASH_TIME)
		{
			this.FinishedDashing();
			return;
		}
		Vector2 vector = this.OrigDashVector();
		vector = ModHooks.DashVelocityChange(vector);
		this.rb2d.velocity = vector;
		this.dash_timer += Time.deltaTime;
	}

	// Token: 0x06000546 RID: 1350 RVA: 0x00003603 File Offset: 0x00001803
	private void BackDash()
	{
	}

	// Token: 0x06000547 RID: 1351 RVA: 0x00003603 File Offset: 0x00001803
	private void ShadowDash()
	{
	}

	// Token: 0x06000548 RID: 1352 RVA: 0x00003603 File Offset: 0x00001803
	private void SuperDash()
	{
	}

	// Token: 0x06000549 RID: 1353 RVA: 0x0001D2B8 File Offset: 0x0001B4B8
	public void FaceRight()
	{
		this.cState.facingRight = true;
		Vector3 localScale = this.transform.localScale;
		localScale.x = -1f;
		this.transform.localScale = localScale;
	}

	// Token: 0x0600054A RID: 1354 RVA: 0x0001D2F8 File Offset: 0x0001B4F8
	public void FaceLeft()
	{
		this.cState.facingRight = false;
		Vector3 localScale = this.transform.localScale;
		localScale.x = 1f;
		this.transform.localScale = localScale;
	}

	// Token: 0x0600054B RID: 1355 RVA: 0x0001D335 File Offset: 0x0001B535
	public void StartMPDrain(float time)
	{
		this.orig_StartMPDrain(time);
		//this.focusMP_amount *= ModHooks.OnFocusCost();
	}

	// Token: 0x0600054C RID: 1356 RVA: 0x0001D350 File Offset: 0x0001B550
	public void StopMPDrain()
	{
		this.drainMP = false;
	}

	// Token: 0x0600054D RID: 1357 RVA: 0x0001D359 File Offset: 0x0001B559
	public void SetBackOnGround()
	{
		this.cState.onGround = true;
	}

	// Token: 0x0600054E RID: 1358 RVA: 0x0001D367 File Offset: 0x0001B567
	public void SetStartWithWallslide()
	{
		this.startWithWallslide = true;
	}

	// Token: 0x0600054F RID: 1359 RVA: 0x0001D370 File Offset: 0x0001B570
	public void SetStartWithJump()
	{
		this.startWithJump = true;
	}

	// Token: 0x06000550 RID: 1360 RVA: 0x0001D379 File Offset: 0x0001B579
	public void SetStartWithFullJump()
	{
		this.startWithFullJump = true;
	}

	// Token: 0x06000551 RID: 1361 RVA: 0x0001D382 File Offset: 0x0001B582
	public void SetStartWithDash()
	{
		this.startWithDash = true;
	}

	// Token: 0x06000552 RID: 1362 RVA: 0x0001D38B File Offset: 0x0001B58B
	public void SetStartWithAttack()
	{
		this.startWithAttack = true;
	}

	// Token: 0x06000553 RID: 1363 RVA: 0x0001D394 File Offset: 0x0001B594
	public void SetSuperDashExit()
	{
		this.exitedSuperDashing = true;
	}

	// Token: 0x06000554 RID: 1364 RVA: 0x0001D39D File Offset: 0x0001B59D
	public void SetQuakeExit()
	{
		this.exitedQuake = true;
	}

	// Token: 0x06000555 RID: 1365 RVA: 0x0001D3A6 File Offset: 0x0001B5A6
	public void SetTakeNoDamage()
	{
		this.takeNoDamage = true;
	}

	// Token: 0x06000556 RID: 1366 RVA: 0x0001D3AF File Offset: 0x0001B5AF
	public void EndTakeNoDamage()
	{
		this.takeNoDamage = false;
	}

	// Token: 0x06000557 RID: 1367 RVA: 0x0001D3B8 File Offset: 0x0001B5B8
	public void SetHeroParent(Transform newParent)
	{
		this.transform.parent = newParent;
		if (newParent == null)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	// Token: 0x06000558 RID: 1368 RVA: 0x0001D3DA File Offset: 0x0001B5DA
	public void IsSwimming()
	{
		this.cState.swimming = true;
	}

	// Token: 0x06000559 RID: 1369 RVA: 0x0001D3E8 File Offset: 0x0001B5E8
	public void NotSwimming()
	{
		this.cState.swimming = false;
	}

	// Token: 0x0600055A RID: 1370 RVA: 0x0001D3F6 File Offset: 0x0001B5F6
	public void EnableRenderer()
	{
		this.renderer.enabled = true;
	}

	// Token: 0x0600055B RID: 1371 RVA: 0x0001D404 File Offset: 0x0001B604
	public void ResetAirMoves()
	{
		this.doubleJumped = false;
		this.airDashed = false;
	}

	// Token: 0x0600055C RID: 1372 RVA: 0x0001D414 File Offset: 0x0001B614
	public void SetConveyorSpeed(float speed)
	{
		this.conveyorSpeed = speed;
	}

	// Token: 0x0600055D RID: 1373 RVA: 0x0001D41D File Offset: 0x0001B61D
	public void SetConveyorSpeedV(float speed)
	{
		this.conveyorSpeedV = speed;
	}

	// Token: 0x0600055E RID: 1374 RVA: 0x0001D426 File Offset: 0x0001B626
	public void EnterWithoutInput(bool flag)
	{
		this.enterWithoutInput = flag;
	}

	// Token: 0x0600055F RID: 1375 RVA: 0x0001D42F File Offset: 0x0001B62F
	public void SetDarkness(int darkness)
	{
		if (darkness > 0 && this.playerData.GetBool("hasLantern"))
		{
			this.wieldingLantern = true;
			return;
		}
		this.wieldingLantern = false;
	}

	// Token: 0x06000560 RID: 1376 RVA: 0x0001D458 File Offset: 0x0001B658
	public void CancelHeroJump()
	{
		if (this.cState.jumping)
		{
			this.CancelJump();
			this.CancelDoubleJump();
			if (this.rb2d.velocity.y > 0f)
			{
				this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, 0f);
			}
		}
	}

	// Token: 0x06000561 RID: 1377 RVA: 0x0001D4BA File Offset: 0x0001B6BA
	public void CharmUpdate()
	{
		this.orig_CharmUpdate();
		//ModHooks.OnCharmUpdate();
		//this.playerData.UpdateBlueHealth();
	}

	// Token: 0x06000562 RID: 1378 RVA: 0x0001D4D4 File Offset: 0x0001B6D4
	public void checkEnvironment()
	{
		if (this.playerData.GetInt("environmentType") == 0)
		{
			this.footStepsRunAudioSource.clip = this.footstepsRunDust;
			this.footStepsWalkAudioSource.clip = this.footstepsWalkDust;
			return;
		}
		if (this.playerData.GetInt("environmentType") == 1)
		{
			this.footStepsRunAudioSource.clip = this.footstepsRunGrass;
			this.footStepsWalkAudioSource.clip = this.footstepsWalkGrass;
			return;
		}
		if (this.playerData.GetInt("environmentType") == 2)
		{
			this.footStepsRunAudioSource.clip = this.footstepsRunBone;
			this.footStepsWalkAudioSource.clip = this.footstepsWalkBone;
			return;
		}
		if (this.playerData.GetInt("environmentType") == 3)
		{
			this.footStepsRunAudioSource.clip = this.footstepsRunSpa;
			this.footStepsWalkAudioSource.clip = this.footstepsWalkSpa;
			return;
		}
		if (this.playerData.GetInt("environmentType") == 4)
		{
			this.footStepsRunAudioSource.clip = this.footstepsRunMetal;
			this.footStepsWalkAudioSource.clip = this.footstepsWalkMetal;
			return;
		}
		if (this.playerData.GetInt("environmentType") == 6)
		{
			this.footStepsRunAudioSource.clip = this.footstepsRunWater;
			this.footStepsWalkAudioSource.clip = this.footstepsRunWater;
			return;
		}
		if (this.playerData.GetInt("environmentType") == 7)
		{
			this.footStepsRunAudioSource.clip = this.footstepsRunGrass;
			this.footStepsWalkAudioSource.clip = this.footstepsWalkGrass;
		}
	}

	// Token: 0x06000563 RID: 1379 RVA: 0x0001D659 File Offset: 0x0001B859
	public void SetBenchRespawn(string spawnMarker, string sceneName, int spawnType, bool facingRight)
	{
		//this.playerData.SetBenchRespawn(spawnMarker, sceneName, spawnType, facingRight);
	}

	// Token: 0x06000564 RID: 1380 RVA: 0x0001D66B File Offset: 0x0001B86B
	public void SetHazardRespawn(Vector3 position, bool facingRight)
	{
		//this.playerData.SetHazardRespawn(position, facingRight);
	}

	// Token: 0x06000565 RID: 1381 RVA: 0x0001D67A File Offset: 0x0001B87A
	public void AddGeo(int amount)
	{
		
		//this.playerData.AddGeo(amount);
		//this.geoCounter.AddGeo(amount);
	}

	// Token: 0x06000566 RID: 1382 RVA: 0x0001D694 File Offset: 0x0001B894
	public void ToZero()
	{
		//this.geoCounter.ToZero();
	}

	// Token: 0x06000567 RID: 1383 RVA: 0x0001D6A1 File Offset: 0x0001B8A1
	public void AddGeoQuietly(int amount)
	{
		//this.playerData.AddGeo(amount);
	}

	// Token: 0x06000568 RID: 1384 RVA: 0x0001D6AF File Offset: 0x0001B8AF
	public void AddGeoToCounter(int amount)
	{
		//this.geoCounter.AddGeo(amount);
	}

	// Token: 0x06000569 RID: 1385 RVA: 0x0001D6BD File Offset: 0x0001B8BD
	public void TakeGeo(int amount)
	{
		//this.playerData.TakeGeo(amount);
		//this.geoCounter.TakeGeo(amount);
	}

	// Token: 0x0600056A RID: 1386 RVA: 0x0001D6D7 File Offset: 0x0001B8D7
	public void UpdateGeo()
	{
		//this.geoCounter.UpdateGeo();
	}

	// Token: 0x0600056B RID: 1387 RVA: 0x0001D6E4 File Offset: 0x0001B8E4
	public bool CanInput()
	{
		return this.acceptingInput;
	}

	// Token: 0x0600056C RID: 1388 RVA: 0x0001D6EC File Offset: 0x0001B8EC
	public bool CanTalk()
	{
		bool result = false;
		if (this.CanInput() && this.hero_state != ActorStates.no_input && !this.controlReqlinquished && this.cState.onGround && !this.cState.attacking && !this.cState.dashing)
		{
			result = true;
		}
		return result;
	}

	// Token: 0x0600056D RID: 1389 RVA: 0x0001D740 File Offset: 0x0001B940
	public void FlipSprite()
	{
		this.cState.facingRight = !this.cState.facingRight;
		Vector3 localScale = this.transform.localScale;
		localScale.x *= -1f;
		this.transform.localScale = localScale;
	}

	// Token: 0x0600056E RID: 1390 RVA: 0x0001D78E File Offset: 0x0001B98E
	public void NailParry()
	{
		this.parryInvulnTimer = this.INVUL_TIME_PARRY;
	}

	// Token: 0x0600056F RID: 1391 RVA: 0x0001D79C File Offset: 0x0001B99C
	public void NailParryRecover()
	{
		this.attackDuration = 0f;
		this.attack_cooldown = 0f;
		this.CancelAttack();
	}

	// Token: 0x06000570 RID: 1392 RVA: 0x0001D7BA File Offset: 0x0001B9BA
	public void QuakeInvuln()
	{
		this.parryInvulnTimer = this.INVUL_TIME_QUAKE;
	}

	// Token: 0x06000571 RID: 1393 RVA: 0x0001D7C8 File Offset: 0x0001B9C8
	public void CancelParryInvuln()
	{
		this.parryInvulnTimer = 0f;
	}

	// Token: 0x06000572 RID: 1394 RVA: 0x0001D7D5 File Offset: 0x0001B9D5
	public void CycloneInvuln()
	{
		this.parryInvulnTimer = this.INVUL_TIME_CYCLONE;
	}

	// Token: 0x06000573 RID: 1395 RVA: 0x0001D7E3 File Offset: 0x0001B9E3
	public void SetWieldingLantern(bool set)
	{
		this.wieldingLantern = set;
	}

	// Token: 0x06000574 RID: 1396 RVA: 0x0001D7EC File Offset: 0x0001B9EC
	public void TakeDamage(GameObject go, CollisionSide damageSide, int damageAmount, int hazardType)
	{
		damageAmount = ModHooks.OnTakeDamage(ref hazardType, damageAmount);
		bool spawnDamageEffect = true;
		if (damageAmount > 0)
		{
			/*if (BossSceneController.IsBossScene)
			{
				int bossLevel = BossSceneController.Instance.BossLevel;
				if (bossLevel != 1)
				{
					if (bossLevel == 2)
					{
						damageAmount = 9999;
					}
				}
				else
				{
					damageAmount *= 2;
				}
			}*/
			if (this.CanTakeDamage())
			{
				if (this.damageMode == DamageMode.HAZARD_ONLY && hazardType == 1)
				{
					return;
				}
				if (this.cState.shadowDashing && hazardType == 1)
				{
					return;
				}
				if (this.parryInvulnTimer > 0f && hazardType == 1)
				{
					return;
				}
				//VibrationMixer mixer = VibrationManager.GetMixer();
				/*if (mixer != null)
				{
					mixer.StopAllEmissionsWithTag("heroAction");
				}*/
				bool flag = false;
				if (this.carefreeShieldEquipped && hazardType == 1)
				{
					if (this.hitsSinceShielded > 7)
					{
						this.hitsSinceShielded = 7;
					}
					switch (this.hitsSinceShielded)
					{
						case 1:
							if ((float)UnityEngine.Random.Range(1, 100) <= 10f)
							{
								flag = true;
							}
							break;
						case 2:
							if ((float)UnityEngine.Random.Range(1, 100) <= 20f)
							{
								flag = true;
							}
							break;
						case 3:
							if ((float)UnityEngine.Random.Range(1, 100) <= 30f)
							{
								flag = true;
							}
							break;
						case 4:
							if ((float)UnityEngine.Random.Range(1, 100) <= 50f)
							{
								flag = true;
							}
							break;
						case 5:
							if ((float)UnityEngine.Random.Range(1, 100) <= 70f)
							{
								flag = true;
							}
							break;
						case 6:
							if ((float)UnityEngine.Random.Range(1, 100) <= 80f)
							{
								flag = true;
							}
							break;
						case 7:
							if ((float)UnityEngine.Random.Range(1, 100) <= 90f)
							{
								flag = true;
							}
							break;
						default:
							flag = false;
							break;
					}
					if (flag)
					{
						this.hitsSinceShielded = 0;
						//this.carefreeShield.SetActive(true);
						damageAmount = 0;
						spawnDamageEffect = false;
					}
					else
					{
						this.hitsSinceShielded++;
					}
				}
				damageAmount = ModHooks.AfterTakeDamage(hazardType, damageAmount);
				if (this.playerData.GetBool("equippedCharm_5") && this.playerData.GetInt("blockerHits") > 0 && hazardType == 1 && this.cState.focusing && !flag)
				{
					//this.proxyFSM.SendEvent("HeroCtrl-TookBlockerHit");
					this.audioSource.PlayOneShot(this.blockerImpact, 1f);
					spawnDamageEffect = false;
					damageAmount = 0;
				}
				else
				{
					//this.proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
				}
				this.CancelAttack();
				if (this.cState.wallSliding)
				{
					this.cState.wallSliding = false;
					//this.wallSlideVibrationPlayer.Stop();
				}
				if (this.cState.touchingWall)
				{
					this.cState.touchingWall = false;
				}
				if (this.cState.recoilingLeft || this.cState.recoilingRight)
				{
					this.CancelRecoilHorizontal();
				}
				if (this.cState.bouncing)
				{
					this.CancelBounce();
					this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, 0f);
				}
				if (this.cState.shroomBouncing)
				{
					this.CancelBounce();
					this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, 0f);
				}
				if (!flag)
				{
					//this.audioCtrl.PlaySound(HeroSounds.TAKE_HIT);
				}
				if (!this.takeNoDamage && !this.playerData.GetBool("invinciTest"))
				{
					if (this.playerData.GetBool("overcharmed"))
					{
						this.playerData.TakeHealth(damageAmount * 2);
					}
					else
					{
						this.playerData.TakeHealth(damageAmount);
					}
				}
				if (this.playerData.GetBool("equippedCharm_3") && damageAmount > 0)
				{
					if (this.playerData.GetBool("equippedCharm_35"))
					{
						this.AddMPCharge(this.GRUB_SOUL_MP_COMBO);
					}
					else
					{
						this.AddMPCharge(this.GRUB_SOUL_MP);
					}
				}
				if (this.joniBeam && damageAmount > 0)
				{
					this.joniBeam = false;
				}
				if (this.cState.nailCharging || this.nailChargeTimer != 0f)
				{
					this.cState.nailCharging = false;
					this.nailChargeTimer = 0f;
				}
				if (damageAmount > 0 && this.OnTakenDamage != null)
				{
					this.OnTakenDamage();
				}
				if (this.playerData.GetInt("health") == 0)
				{
					base.StartCoroutine(this.Die());
					return;
				}
				if (hazardType == 2)
				{
					base.StartCoroutine(this.DieFromHazard(HazardType.SPIKES, (!(go != null)) ? 0f : go.transform.rotation.z));
					return;
				}
				if (hazardType == 3)
				{
					base.StartCoroutine(this.DieFromHazard(HazardType.ACID, 0f));
					return;
				}
				if (hazardType == 4)
				{
					Debug.Log("Lava death");
					return;
				}
				if (hazardType == 5)
				{
					base.StartCoroutine(this.DieFromHazard(HazardType.PIT, 0f));
					return;
				}
				base.StartCoroutine(this.StartRecoil(damageSide, spawnDamageEffect, damageAmount));
				return;
			}
			else if (this.cState.invulnerable && !this.cState.hazardDeath && !this.playerData.GetBool("isInvincible"))
			{
				if (hazardType == 2)
				{
					if (!this.takeNoDamage)
					{
						this.playerData.TakeHealth(damageAmount);
					}
					//this.proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
					if (this.playerData.GetInt("health") == 0)
					{
						base.StartCoroutine(this.Die());
						return;
					}
					//this.audioCtrl.PlaySound(HeroSounds.TAKE_HIT);
					base.StartCoroutine(this.DieFromHazard(HazardType.SPIKES, (!(go != null)) ? 0f : go.transform.rotation.z));
					return;
				}
				else if (hazardType == 3)
				{
					this.playerData.TakeHealth(damageAmount);
					//this.proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
					if (this.playerData.GetInt("health") == 0)
					{
						base.StartCoroutine(this.Die());
						return;
					}
					base.StartCoroutine(this.DieFromHazard(HazardType.ACID, 0f));
					return;
				}
				else if (hazardType == 4)
				{
					Debug.Log("Lava damage");
				}
			}
		}
	}

	// Token: 0x06000575 RID: 1397 RVA: 0x0001DDB3 File Offset: 0x0001BFB3
	public string GetEntryGateName()
	{
		if (this.sceneEntryGate != null)
		{
			return this.sceneEntryGate.name;
		}
		return "";
	}

	// Token: 0x06000576 RID: 1398 RVA: 0x0001DDD4 File Offset: 0x0001BFD4
	public void AddMPCharge(int amount)
	{
		/*int @int = this.playerData.GetInt("MPReserve");
		this.playerData.AddMPCharge(amount);
		GameCameras.instance.soulOrbFSM.SendEvent("MP GAIN");
		if (this.playerData.GetInt("MPReserve") != @int && this.gm && this.gm.soulVessel_fsm)
		{
			this.gm.soulVessel_fsm.SendEvent("MP RESERVE UP");
		}*/
	}

	// Token: 0x06000577 RID: 1399 RVA: 0x0001DE5C File Offset: 0x0001C05C
	public void SoulGain()
	{
		int num;
		if (this.playerData.GetInt("MPCharge") < this.playerData.GetInt("maxMP"))
		{
			num = 11;
			if (this.playerData.GetBool("equippedCharm_20"))
			{
				num += 3;
			}
			if (this.playerData.GetBool("equippedCharm_21"))
			{
				num += 8;
			}
		}
		else
		{
			num = 6;
			if (this.playerData.GetBool("equippedCharm_20"))
			{
				num += 2;
			}
			if (this.playerData.GetBool("equippedCharm_21"))
			{
				num += 6;
			}
		}
		int @int = this.playerData.GetInt("MPReserve");
		//num = ModHooks.OnSoulGain(num);
		//this.playerData.AddMPCharge(num);
		/*GameCameras.instance.soulOrbFSM.SendEvent("MP GAIN");
		if (this.playerData.GetInt("MPReserve") != @int)
		{
			this.gm.soulVessel_fsm.SendEvent("MP RESERVE UP");
		}*/
	}

	// Token: 0x06000578 RID: 1400 RVA: 0x0001DF4B File Offset: 0x0001C14B
	public void AddMPChargeSpa(int amount)
	{
		this.TryAddMPChargeSpa(amount);
	}

	// Token: 0x06000579 RID: 1401 RVA: 0x0001DF58 File Offset: 0x0001C158
	public bool TryAddMPChargeSpa(int amount)
	{
		/*int @int = this.playerData.GetInt("MPReserve");
		bool result = this.playerData.AddMPCharge(amount);
		this.gm.soulOrb_fsm.SendEvent("MP GAIN SPA");
		if (this.playerData.GetInt("MPReserve") != @int)
		{
			this.gm.soulVessel_fsm.SendEvent("MP RESERVE UP");
		}
		return result;*/
		return false;
	}

	// Token: 0x0600057A RID: 1402 RVA: 0x0001DFBF File Offset: 0x0001C1BF
	public void SetMPCharge(int amount)
	{
		this.playerData.SetIntSwappedArgs(amount, "MPCharge");
		//GameCameras.instance.soulOrbFSM.SendEvent("MP SET");
	}

	// Token: 0x0600057B RID: 1403 RVA: 0x0001DFE6 File Offset: 0x0001C1E6
	public void TakeMP(int amount)
	{
		if (this.playerData.GetInt("MPCharge") > 0)
		{
			/*this.playerData.TakeMP(amount);
			if (amount > 1)
			{
				GameCameras.instance.soulOrbFSM.SendEvent("MP LOSE");
			}*/
		}
	}

	// Token: 0x0600057C RID: 1404 RVA: 0x0001E01F File Offset: 0x0001C21F
	public void TakeMPQuick(int amount)
	{
		if (this.playerData.GetInt("MPCharge") > 0)
		{
			/*this.playerData.TakeMP(amount);
			if (amount > 1)
			{
				GameCameras.instance.soulOrbFSM.SendEvent("MP DRAIN");
			}*/
		}
	}

	// Token: 0x0600057D RID: 1405 RVA: 0x0001E058 File Offset: 0x0001C258
	public void TakeReserveMP(int amount)
	{
		//this.playerData.TakeReserveMP(amount);
		//this.gm.soulVessel_fsm.SendEvent("MP RESERVE DOWN");
	}

	// Token: 0x0600057E RID: 1406 RVA: 0x0001E07B File Offset: 0x0001C27B
	public void AddHealth(int amount)
	{
		this.playerData.AddHealth(amount);
		//this.proxyFSM.SendEvent("HeroCtrl-Healed");
	}

	// Token: 0x0600057F RID: 1407 RVA: 0x0001E099 File Offset: 0x0001C299
	public void TakeHealth(int amount)
	{
		this.playerData.TakeHealth(amount);
		//this.proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
	}

	// Token: 0x06000580 RID: 1408 RVA: 0x0001E0B7 File Offset: 0x0001C2B7
	public void MaxHealth()
	{
		//this.proxyFSM.SendEvent("HeroCtrl-MaxHealth");
		this.playerData.MaxHealth();
	}

	// Token: 0x06000581 RID: 1409 RVA: 0x0001E0D4 File Offset: 0x0001C2D4
	public void MaxHealthKeepBlue()
	{
		int @int = this.playerData.GetInt("healthBlue");
		this.playerData.MaxHealth();
		this.playerData.SetIntSwappedArgs(@int, "healthBlue");
		//this.proxyFSM.SendEvent("HeroCtrl-Healed");
	}

	// Token: 0x06000582 RID: 1410 RVA: 0x0001E120 File Offset: 0x0001C320
	public void AddToMaxHealth(int amount)
	{
		//this.playerData.AddToMaxHealth(amount);
		this.gm.AwardAchievement("PROTECTED");
		if (this.playerData.GetInt("maxHealthBase") == this.playerData.GetInt("maxHealthCap"))
		{
			this.gm.AwardAchievement("MASKED");
		}
	}

	// Token: 0x06000583 RID: 1411 RVA: 0x0001E17B File Offset: 0x0001C37B
	public void ClearMP()
	{
		//this.playerData.ClearMP();
	}

	// Token: 0x06000584 RID: 1412 RVA: 0x0001E188 File Offset: 0x0001C388
	public void ClearMPSendEvents()
	{
		this.ClearMP();
		//GameManager.instance.soulOrb_fsm.SendEvent("MP LOSE");
		//GameManager.instance.soulVessel_fsm.SendEvent("MP RESERVE DOWN");
	}

	// Token: 0x06000585 RID: 1413 RVA: 0x0001E1B8 File Offset: 0x0001C3B8
	public void AddToMaxMPReserve(int amount)
	{
		//this.playerData.AddToMaxMPReserve(amount);
		this.gm.AwardAchievement("SOULFUL");
		if (this.playerData.GetInt("MPReserveMax") == this.playerData.GetInt("MPReserveCap"))
		{
			this.gm.AwardAchievement("WORLDSOUL");
		}
	}

	// Token: 0x06000586 RID: 1414 RVA: 0x0001E213 File Offset: 0x0001C413
	public void Bounce()
	{
		if (!this.cState.bouncing && !this.cState.shroomBouncing && !this.controlReqlinquished)
		{
			this.doubleJumped = false;
			this.airDashed = false;
			this.cState.bouncing = true;
		}
	}

	// Token: 0x06000587 RID: 1415 RVA: 0x0001E254 File Offset: 0x0001C454
	public void BounceHigh()
	{
		if (!this.cState.bouncing && !this.controlReqlinquished)
		{
			this.doubleJumped = false;
			this.airDashed = false;
			this.cState.bouncing = true;
			this.bounceTimer = -0.03f;
			this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, this.BOUNCE_VELOCITY);
		}
	}

	// Token: 0x06000588 RID: 1416 RVA: 0x0001E2C4 File Offset: 0x0001C4C4
	public void ShroomBounce()
	{
		this.doubleJumped = false;
		this.airDashed = false;
		this.cState.bouncing = false;
		this.cState.shroomBouncing = true;
		this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, this.SHROOM_BOUNCE_VELOCITY);
	}

	// Token: 0x06000589 RID: 1417 RVA: 0x0001E320 File Offset: 0x0001C520
	public void RecoilLeft()
	{
		if (!this.cState.recoilingLeft && !this.cState.recoilingRight && !this.playerData.GetBool("equippedCharm_14") && !this.controlReqlinquished)
		{
			this.CancelDash();
			this.recoilSteps = 0;
			this.cState.recoilingLeft = true;
			this.cState.recoilingRight = false;
			this.recoilLarge = false;
			this.rb2d.velocity = new Vector2(-this.RECOIL_HOR_VELOCITY, this.rb2d.velocity.y);
		}
	}

	// Token: 0x0600058A RID: 1418 RVA: 0x0001E3B8 File Offset: 0x0001C5B8
	public void RecoilRight()
	{
		if (!this.cState.recoilingLeft && !this.cState.recoilingRight && !this.playerData.GetBool("equippedCharm_14") && !this.controlReqlinquished)
		{
			this.CancelDash();
			this.recoilSteps = 0;
			this.cState.recoilingRight = true;
			this.cState.recoilingLeft = false;
			this.recoilLarge = false;
			this.rb2d.velocity = new Vector2(this.RECOIL_HOR_VELOCITY, this.rb2d.velocity.y);
		}
	}

	// Token: 0x0600058B RID: 1419 RVA: 0x0001E450 File Offset: 0x0001C650
	public void RecoilRightLong()
	{
		if (!this.cState.recoilingLeft && !this.cState.recoilingRight && !this.controlReqlinquished)
		{
			this.CancelDash();
			this.ResetAttacks();
			this.recoilSteps = 0;
			this.cState.recoilingRight = true;
			this.cState.recoilingLeft = false;
			this.recoilLarge = true;
			this.rb2d.velocity = new Vector2(this.RECOIL_HOR_VELOCITY_LONG, this.rb2d.velocity.y);
		}
	}

	// Token: 0x0600058C RID: 1420 RVA: 0x0001E4D8 File Offset: 0x0001C6D8
	public void RecoilLeftLong()
	{
		if (!this.cState.recoilingLeft && !this.cState.recoilingRight && !this.controlReqlinquished)
		{
			this.CancelDash();
			this.ResetAttacks();
			this.recoilSteps = 0;
			this.cState.recoilingRight = false;
			this.cState.recoilingLeft = true;
			this.recoilLarge = true;
			this.rb2d.velocity = new Vector2(-this.RECOIL_HOR_VELOCITY_LONG, this.rb2d.velocity.y);
		}
	}

	// Token: 0x0600058D RID: 1421 RVA: 0x0001E560 File Offset: 0x0001C760
	public void RecoilDown()
	{
		this.CancelJump();
		if (this.rb2d.velocity.y > this.RECOIL_DOWN_VELOCITY && !this.controlReqlinquished)
		{
			this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, this.RECOIL_DOWN_VELOCITY);
		}
	}

	// Token: 0x0600058E RID: 1422 RVA: 0x0001E5B9 File Offset: 0x0001C7B9
	public void ForceHardLanding()
	{
		if (!this.cState.onGround)
		{
			this.cState.willHardLand = true;
		}
	}

	// Token: 0x0600058F RID: 1423 RVA: 0x0001E5D4 File Offset: 0x0001C7D4
	public void EnterSceneDreamGate()
	{
		this.IgnoreInputWithoutReset();
		this.ResetMotion();
		this.airDashed = false;
		this.doubleJumped = false;
		this.ResetHardLandingTimer();
		this.ResetAttacksDash();
		this.AffectedByGravity(false);
		this.sceneEntryGate = null;
		this.SetState(ActorStates.no_input);
		this.transitionState = HeroTransitionState.WAITING_TO_ENTER_LEVEL;
		//this.vignetteFSM.SendEvent("RESET");
		if (this.heroInPosition != null)
		{
			this.heroInPosition(false);
		}
		this.FinishedEnteringScene(true, false);
	}

	// Token: 0x06000590 RID: 1424 RVA: 0x0001E64F File Offset: 0x0001C84F
	public IEnumerator EnterScene(TransitionPoint enterGate, float delayBeforeEnter)
	{
		this.IgnoreInputWithoutReset();
		this.ResetMotion();
		this.airDashed = false;
		this.doubleJumped = false;
		this.ResetHardLandingTimer();
		this.ResetAttacksDash();
		this.AffectedByGravity(false);
		this.sceneEntryGate = enterGate;
		this.SetState(ActorStates.no_input);
		this.transitionState = HeroTransitionState.WAITING_TO_ENTER_LEVEL;
		//this.vignetteFSM.SendEvent("RESET");
		if (!this.cState.transitioning)
		{
			this.cState.transitioning = true;
		}
		this.gatePosition = enterGate.GetGatePosition();
		if (this.gatePosition == GatePosition.top)
		{
			this.cState.onGround = false;
			this.enteringVertically = true;
			this.exitedSuperDashing = false;
			this.renderer.enabled = false;
			float x2 = enterGate.transform.position.x + enterGate.entryOffset.x;
			float y2 = enterGate.transform.position.y + enterGate.entryOffset.y;
			this.transform.SetPosition2D(x2, y2);
			if (this.heroInPosition != null)
			{
				this.heroInPosition(false);
			}
			yield return new WaitForSeconds(0.165f);
			/*if (!enterGate.customFade)
			{
				this.gm.FadeSceneIn();
			}*/
			if (delayBeforeEnter > 0f)
			{
				yield return new WaitForSeconds(delayBeforeEnter);
			}
			if (enterGate.entryDelay > 0f)
			{
				yield return new WaitForSeconds(enterGate.entryDelay);
			}
			yield return new WaitForSeconds(0.4f);
			this.renderer.enabled = true;
			if (this.exitedQuake)
			{
				this.IgnoreInput();
				//this.proxyFSM.SendEvent("HeroCtrl-EnterQuake");
				yield return new WaitForSeconds(0.25f);
				this.FinishedEnteringScene(true, false);
			}
			else
			{
				this.rb2d.velocity = new Vector2(0f, this.SPEED_TO_ENTER_SCENE_DOWN);
				this.transitionState = HeroTransitionState.ENTERING_SCENE;
				this.transitionState = HeroTransitionState.DROPPING_DOWN;
				this.AffectedByGravity(true);
				if (enterGate.hardLandOnExit)
				{
					this.cState.willHardLand = true;
				}
				yield return new WaitForSeconds(0.33f);
				this.transitionState = HeroTransitionState.ENTERING_SCENE;
				if (this.transitionState != HeroTransitionState.WAITING_TO_TRANSITION)
				{
					this.FinishedEnteringScene(true, false);
				}
			}
		}
		else if (this.gatePosition == GatePosition.bottom)
		{
			this.cState.onGround = false;
			this.enteringVertically = true;
			this.exitedSuperDashing = false;
			if (enterGate.alwaysEnterRight)
			{
				this.FaceRight();
			}
			if (enterGate.alwaysEnterLeft)
			{
				this.FaceLeft();
			}
			float x = enterGate.transform.position.x + enterGate.entryOffset.x;
			float y = enterGate.transform.position.y + enterGate.entryOffset.y + 3f;
			this.transform.SetPosition2D(x, y);
			if (this.heroInPosition != null)
			{
				this.heroInPosition(false);
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
			/*if (!enterGate.customFade)
			{
				this.gm.FadeSceneIn();
			}*/
			if (this.cState.facingRight)
			{
				this.transition_vel = new Vector2(this.SPEED_TO_ENTER_SCENE_HOR, this.SPEED_TO_ENTER_SCENE_UP);
			}
			else
			{
				this.transition_vel = new Vector2(-this.SPEED_TO_ENTER_SCENE_HOR, this.SPEED_TO_ENTER_SCENE_UP);
			}
			this.transitionState = HeroTransitionState.ENTERING_SCENE;
			this.transform.SetPosition2D(x, y);
			yield return new WaitForSeconds(this.TIME_TO_ENTER_SCENE_BOT);
			this.transition_vel = new Vector2(this.rb2d.velocity.x, 0f);
			this.AffectedByGravity(true);
			this.transitionState = HeroTransitionState.DROPPING_DOWN;
		}
		else if (this.gatePosition == GatePosition.left)
		{
			this.cState.onGround = true;
			this.enteringVertically = false;
			this.SetState(ActorStates.no_input);
			float num = enterGate.transform.position.x + enterGate.entryOffset.x;
			float y3 = this.FindGroundPointY(num + 2f, enterGate.transform.position.y, false);
			this.transform.SetPosition2D(num, y3);
			if (this.heroInPosition != null)
			{
				this.heroInPosition(true);
			}
			this.FaceRight();
			yield return new WaitForSeconds(0.165f);
			/*if (!enterGate.customFade)
			{
				this.gm.FadeSceneIn();
			}*/
			if (delayBeforeEnter > 0f)
			{
				yield return new WaitForSeconds(delayBeforeEnter);
			}
			if (enterGate.entryDelay > 0f)
			{
				yield return new WaitForSeconds(enterGate.entryDelay);
			}
			yield return new WaitForSeconds(0.4f);
			if (this.exitedSuperDashing)
			{
				this.IgnoreInput();
				//this.proxyFSM.SendEvent("HeroCtrl-EnterSuperDash");
				yield return new WaitForSeconds(0.25f);
				this.FinishedEnteringScene(true, false);
			}
			else
			{
				this.transition_vel = new Vector2(this.RUN_SPEED, 0f);
				this.transitionState = HeroTransitionState.ENTERING_SCENE;
				yield return new WaitForSeconds(0.33f);
				this.FinishedEnteringScene(true, true);
			}
		}
		else if (this.gatePosition == GatePosition.right)
		{
			this.cState.onGround = true;
			this.enteringVertically = false;
			this.SetState(ActorStates.no_input);
			float num2 = enterGate.transform.position.x + enterGate.entryOffset.x;
			float y4 = this.FindGroundPointY(num2 - 2f, enterGate.transform.position.y, false);
			this.transform.SetPosition2D(num2, y4);
			if (this.heroInPosition != null)
			{
				this.heroInPosition(true);
			}
			this.FaceLeft();
			yield return new WaitForSeconds(0.165f);
			/*if (!enterGate.customFade)
			{
				this.gm.FadeSceneIn();
			}*/
			if (delayBeforeEnter > 0f)
			{
				yield return new WaitForSeconds(delayBeforeEnter);
			}
			if (enterGate.entryDelay > 0f)
			{
				yield return new WaitForSeconds(enterGate.entryDelay);
			}
			yield return new WaitForSeconds(0.4f);
			if (this.exitedSuperDashing)
			{
				this.IgnoreInput();
				//this.proxyFSM.SendEvent("HeroCtrl-EnterSuperDash");
				yield return new WaitForSeconds(0.25f);
				this.FinishedEnteringScene(true, false);
			}
			else
			{
				this.transition_vel = new Vector2(-this.RUN_SPEED, 0f);
				this.transitionState = HeroTransitionState.ENTERING_SCENE;
				yield return new WaitForSeconds(0.33f);
				this.FinishedEnteringScene(true, true);
			}
		}
		else if (this.gatePosition == GatePosition.door)
		{
			if (enterGate.alwaysEnterRight)
			{
				this.FaceRight();
			}
			if (enterGate.alwaysEnterLeft)
			{
				this.FaceLeft();
			}
			this.cState.onGround = true;
			this.enteringVertically = false;
			this.SetState(ActorStates.idle);
			this.SetState(ActorStates.no_input);
			this.exitedSuperDashing = false;
			//this.animCtrl.PlayClip("Idle");
			this.transform.SetPosition2D(this.FindGroundPoint(enterGate.transform.position, false));
			if (this.heroInPosition != null)
			{
				this.heroInPosition(false);
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
			/*if (!enterGate.customFade)
			{
				this.gm.FadeSceneIn();
			}*/
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (enterGate.dontWalkOutOfDoor)
			{
				yield return new WaitForSeconds(0.33f);
			}
			else
			{
				/*float clipDuration = this.animCtrl.GetClipDuration("Exit Door To Idle");
				this.animCtrl.PlayClip("Exit Door To Idle");
				if (clipDuration > 0f)
				{
					yield return new WaitForSeconds(clipDuration);
				}
				else
				{
					yield return new WaitForSeconds(0.33f);
				}*/
				yield return new WaitForSeconds(0.33f);
			}
			this.FinishedEnteringScene(true, false);
		}
		yield break;
	}

	// Token: 0x06000591 RID: 1425 RVA: 0x0001E66C File Offset: 0x0001C86C
	public void LeaveScene(GatePosition? gate = null)
	{
		this.isHeroInPosition = false;
		this.IgnoreInputWithoutReset();
		this.ResetHardLandingTimer();
		this.SetState(ActorStates.no_input);
		this.SetDamageMode(DamageMode.NO_DAMAGE);
		this.transitionState = HeroTransitionState.EXITING_SCENE;
		this.CancelFallEffects();
		this.tilemapTestActive = false;
		this.SetHeroParent(null);
		this.StopTilemapTest();
		if (gate != null)
		{
			switch (gate.Value)
			{
				case GatePosition.top:
					this.transition_vel = new Vector2(0f, this.MIN_JUMP_SPEED);
					this.cState.onGround = false;
					break;
				case GatePosition.right:
					this.transition_vel = new Vector2(this.RUN_SPEED, 0f);
					break;
				case GatePosition.left:
					this.transition_vel = new Vector2(-this.RUN_SPEED, 0f);
					break;
				case GatePosition.bottom:
					this.transition_vel = Vector2.zero;
					this.cState.onGround = false;
					break;
			}
		}
		this.cState.transitioning = true;
	}

	// Token: 0x06000592 RID: 1426 RVA: 0x0001E75F File Offset: 0x0001C95F
	/*public IEnumerator BetaLeave(EndBeta betaEndTrigger)
	{
		if (!this.playerData.betaEnd)
		{
			this.endBeta = betaEndTrigger;
			this.IgnoreInput();
			this.playerData.disablePause = true;
			this.SetState(ActorStates.no_input);
			this.ResetInput();
			this.tilemapTestActive = false;
			yield return new WaitForSeconds(0.66f);
			GameObject.Find("Beta Ender").GetComponent<SimpleSpriteFade>().FadeIn();
			this.ResetMotion();
			yield return new WaitForSeconds(1.25f);
			this.playerData.betaEnd = true;
		}
		yield break;
	}

	// Token: 0x06000593 RID: 1427 RVA: 0x0001E775 File Offset: 0x0001C975
	public IEnumerator BetaReturn()
	{
		this.rb2d.velocity = new Vector2(this.RUN_SPEED, 0f);
		if (!this.cState.facingRight)
		{
			this.FlipSprite();
		}
		GameObject.Find("Beta Ender").GetComponent<SimpleSpriteFade>().FadeOut();
		this.animCtrl.PlayClip("Run");
		yield return new WaitForSeconds(1.4f);
		this.SetState(ActorStates.grounded);
		this.SetStartingMotionState();
		this.AcceptInput();
		this.playerData.betaEnd = false;
		this.playerData.disablePause = false;
		this.tilemapTestActive = true;
		if (this.endBeta != null)
		{
			this.endBeta.Reactivate();
		}
		yield break;
	}*/

	// Token: 0x06000594 RID: 1428 RVA: 0x0001E784 File Offset: 0x0001C984
	public IEnumerator Respawn()
	{
		this.playerData = PlayerData.instance;
		//this.playerData.disablePause = true;
		this.gameObject.layer = 9;
		this.renderer.enabled = true;
		this.rb2d.isKinematic = false;
		this.cState.dead = false;
		this.cState.onGround = true;
		this.cState.hazardDeath = false;
		this.cState.recoiling = false;
		this.enteringVertically = false;
		this.airDashed = false;
		this.doubleJumped = false;
		this.CharmUpdate();
		this.MaxHealth();
		this.ClearMP();
		this.ResetMotion();
		this.ResetHardLandingTimer();
		this.ResetAttacks();
		this.ResetInput();
		this.CharmUpdate();
		Transform spawnPoint = this.LocateSpawnPoint();
		if (spawnPoint != null)
		{
			this.transform.SetPosition2D(this.FindGroundPoint(spawnPoint.transform.position, false));
			/*PlayMakerFSM component = spawnPoint.GetComponent<PlayMakerFSM>();
			if (component != null)
			{
				FSMUtility.GetVector3(component, "Adjust Vector");
			}
			else if (this.verboseMode)
			{
				Debug.Log("Could not find Bench Control FSM on respawn point. Ignoring Adjustment offset.");
			}*/
		}
		/*else
		{
			Debug.LogError("Couldn't find the respawn point named " + this.playerData.respawnMarkerName + " within objects tagged with RespawnPoint");
		}*/
		/*if (this.verboseMode)
		{
			Debug.Log("HC Respawn Type: " + this.playerData.respawnType.ToString());
		}*/
		//GameCameras.instance.cameraFadeFSM.SendEvent("RESPAWN");
		if (this.playerData.respawnType == 1)
		{
			this.AffectedByGravity(false);
			//PlayMakerFSM benchFSM = FSMUtility.LocateFSM(spawnPoint.gameObject, "Bench Control");
			/*if (benchFSM == null)
			{
				Debug.LogError("HeroCtrl: Could not find Bench Control FSM on this spawn point, respawn type is set to Bench");
				yield break;
			}
			benchFSM.FsmVariables.GetFsmBool("RespawnResting").Value = true;*/
			yield return new WaitForEndOfFrame();
			if (this.heroInPosition != null)
			{
				this.heroInPosition(false);
			}
			//this.proxyFSM.SendEvent("HeroCtrl-Respawned");
			this.FinishedEnteringScene(true, false);
			var weaverBench = spawnPoint.gameObject.GetComponent("WeaverCore.Assets.Components.WeaverBench");
            if (weaverBench != null)
            {
				weaverBench.SendMessage("RespawnSittingOnBench");
            }
			//benchFSM.SendEvent("RESPAWN");
			//benchFSM = null;
		}
		else
		{
			yield return new WaitForEndOfFrame();
			this.IgnoreInput();
			RespawnMarker component2 = spawnPoint.GetComponent<RespawnMarker>();
			if (component2)
			{
				if (component2.respawnFacingRight)
				{
					this.FaceRight();
				}
				else
				{
					this.FaceLeft();
				}
			}
			else
			{
				Debug.LogError("Spawn point does not contain a RespawnMarker");
			}
			if (this.heroInPosition != null)
			{
				this.heroInPosition(false);
			}
			/*if (this.gm.GetSceneNameString() != "GG_Atrium")
			{
				float clipDuration = this.animCtrl.GetClipDuration("Wake Up Ground");
				this.animCtrl.PlayClip("Wake Up Ground");
				this.StopAnimationControl();
				this.controlReqlinquished = true;
				yield return new WaitForSeconds(clipDuration);
				this.StartAnimationControl();
				this.controlReqlinquished = false;
			}*/
			//this.proxyFSM.SendEvent("HeroCtrl-Respawned");
			this.FinishedEnteringScene(true, false);
		}
		//this.playerData.disablePause = false;
		this.playerData.isInvincible = false;
		yield break;
	}

	// Token: 0x06000595 RID: 1429 RVA: 0x0001E793 File Offset: 0x0001C993
	public IEnumerator HazardRespawn()
	{
		this.cState.hazardDeath = false;
		this.cState.onGround = true;
		this.cState.hazardRespawning = true;
		this.ResetMotion();
		this.ResetHardLandingTimer();
		this.ResetAttacks();
		this.ResetInput();
		this.cState.recoiling = false;
		this.enteringVertically = false;
		this.airDashed = false;
		this.doubleJumped = false;
		this.transform.SetPosition2D(this.FindGroundPoint(this.playerData.hazardRespawnLocation, true));
		this.gameObject.layer = 9;
		this.renderer.enabled = true;
		yield return new WaitForEndOfFrame();
		if (this.playerData.hazardRespawnFacingRight)
		{
			this.FaceRight();
		}
		else
		{
			this.FaceLeft();
		}
		if (this.heroInPosition != null)
		{
			this.heroInPosition(false);
		}
		this.StartCoroutine(this.Invulnerable(this.INVUL_TIME * 2f));
		//GameCameras.instance.cameraFadeFSM.SendEvent("RESPAWN");
		/*float clipDuration = this.animCtrl.GetClipDuration("Hazard Respawn");
		this.animCtrl.PlayClip("Hazard Respawn");
		yield return new WaitForSeconds(clipDuration);*/
		this.cState.hazardRespawning = false;
		this.rb2d.interpolation = RigidbodyInterpolation2D.Interpolate;
		this.FinishedEnteringScene(false, false);
		yield break;
	}

	// Token: 0x06000596 RID: 1430 RVA: 0x0001E7A2 File Offset: 0x0001C9A2
	public void StartCyclone()
	{
		this.nailArt_cyclone = true;
	}

	// Token: 0x06000597 RID: 1431 RVA: 0x0001E7AB File Offset: 0x0001C9AB
	public void EndCyclone()
	{
		this.nailArt_cyclone = false;
	}

	// Token: 0x06000598 RID: 1432 RVA: 0x0001E7B4 File Offset: 0x0001C9B4
	public bool GetState(string stateName)
	{
		return this.cState.GetState(stateName);
	}

	// Token: 0x06000599 RID: 1433 RVA: 0x0001E7B4 File Offset: 0x0001C9B4
	public bool GetCState(string stateName)
	{
		return this.cState.GetState(stateName);
	}

	// Token: 0x0600059A RID: 1434 RVA: 0x0001E7C2 File Offset: 0x0001C9C2
	public void SetCState(string stateName, bool value)
	{
		this.cState.SetState(stateName, value);
	}

	// Token: 0x0600059B RID: 1435 RVA: 0x0001E7D1 File Offset: 0x0001C9D1
	public void ResetHardLandingTimer()
	{
		this.cState.willHardLand = false;
		this.hardLandingTimer = 0f;
		this.fallTimer = 0f;
		this.hardLanded = false;
	}

	// Token: 0x0600059C RID: 1436 RVA: 0x0001E7FC File Offset: 0x0001C9FC
	public void CancelSuperDash()
	{
		//this.superDash.SendEvent("SLOPE CANCEL");
	}

	// Token: 0x0600059D RID: 1437 RVA: 0x0001E810 File Offset: 0x0001CA10
	public void RelinquishControlNotVelocity()
	{
		if (!this.controlReqlinquished)
		{
			this.prev_hero_state = ActorStates.idle;
			this.ResetInput();
			this.ResetMotionNotVelocity();
			this.SetState(ActorStates.no_input);
			this.IgnoreInput();
			this.controlReqlinquished = true;
			this.ResetLook();
			this.ResetAttacks();
			this.touchingWallL = false;
			this.touchingWallR = false;
		}
	}

	// Token: 0x0600059E RID: 1438 RVA: 0x0001E868 File Offset: 0x0001CA68
	public void RelinquishControl()
	{
		if (!this.controlReqlinquished && !this.cState.dead)
		{
			this.ResetInput();
			this.ResetMotion();
			this.IgnoreInput();
			this.controlReqlinquished = true;
			this.ResetLook();
			this.ResetAttacks();
			this.touchingWallL = false;
			this.touchingWallR = false;
		}
	}

	// Token: 0x0600059F RID: 1439 RVA: 0x0001E8C0 File Offset: 0x0001CAC0
	public void RegainControl()
	{
		this.enteringVertically = false;
		this.doubleJumpQueuing = false;
		this.AcceptInput();
		this.hero_state = ActorStates.idle;
		if (this.controlReqlinquished && !this.cState.dead)
		{
			this.AffectedByGravity(true);
			this.SetStartingMotionState();
			this.controlReqlinquished = false;
			if (this.startWithWallslide)
			{
				//this.wallSlideVibrationPlayer.Play();
				this.cState.wallSliding = true;
				this.cState.willHardLand = false;
				this.cState.touchingWall = true;
				this.airDashed = false;
				//this.wallslideDustPrefab.enableEmission = true;
				/*var e = wallslideDustPrefab.emission;
				e.enabled = true;*/
				this.startWithWallslide = false;
				if (this.transform.localScale.x < 0f)
				{
					this.wallSlidingR = true;
					this.touchingWallR = true;
					return;
				}
				this.wallSlidingL = true;
				this.touchingWallL = true;
				return;
			}
			else
			{
				if (this.startWithJump)
				{
					this.HeroJumpNoEffect();
					this.doubleJumpQueuing = false;
					this.startWithJump = false;
					return;
				}
				if (this.startWithFullJump)
				{
					this.HeroJump();
					this.doubleJumpQueuing = false;
					this.startWithFullJump = false;
					return;
				}
				if (this.startWithDash)
				{
					this.HeroDash();
					this.doubleJumpQueuing = false;
					this.startWithDash = false;
					return;
				}
				if (this.startWithAttack)
				{
					this.DoAttack();
					this.doubleJumpQueuing = false;
					this.startWithAttack = false;
					return;
				}
				this.cState.touchingWall = false;
				this.touchingWallL = false;
				this.touchingWallR = false;
			}
		}
	}

	// Token: 0x060005A0 RID: 1440 RVA: 0x0001EA2E File Offset: 0x0001CC2E
	public void PreventCastByDialogueEnd()
	{
		this.preventCastByDialogueEndTimer = 0.3f;
	}

	// Token: 0x060005A1 RID: 1441 RVA: 0x0001EA3C File Offset: 0x0001CC3C
	public bool CanCast()
	{
		return !this.gm.isPaused && !this.cState.dashing && this.hero_state != ActorStates.no_input && !this.cState.backDashing && (!this.cState.attacking || this.attack_time >= this.ATTACK_RECOVERY_TIME) && !this.cState.recoiling && !this.cState.recoilFrozen && !this.cState.transitioning && !this.cState.hazardDeath && !this.cState.hazardRespawning && this.CanInput() && this.preventCastByDialogueEndTimer <= 0f;
	}

	// Token: 0x060005A2 RID: 1442 RVA: 0x0001EAFC File Offset: 0x0001CCFC
	public bool CanFocus()
	{
		return !this.gm.isPaused && this.hero_state != ActorStates.no_input && !this.cState.dashing && !this.cState.backDashing && (!this.cState.attacking || this.attack_time >= this.ATTACK_RECOVERY_TIME) && !this.cState.recoiling && this.cState.onGround && !this.cState.transitioning && !this.cState.recoilFrozen && !this.cState.hazardDeath && !this.cState.hazardRespawning && this.CanInput();
	}

	// Token: 0x060005A3 RID: 1443 RVA: 0x0001EBBC File Offset: 0x0001CDBC
	public bool CanNailArt()
	{
		if (!this.cState.transitioning && this.hero_state != ActorStates.no_input && !this.cState.attacking && !this.cState.hazardDeath && !this.cState.hazardRespawning && this.nailChargeTimer >= this.nailChargeTime)
		{
			this.nailChargeTimer = 0f;
			return true;
		}
		this.nailChargeTimer = 0f;
		return false;
	}

	// Token: 0x060005A4 RID: 1444 RVA: 0x0001EC30 File Offset: 0x0001CE30
	public bool CanQuickMap()
	{
		return !this.gm.isPaused && !this.controlReqlinquished && this.hero_state != ActorStates.no_input && !this.cState.onConveyor && !this.cState.dashing && !this.cState.backDashing && (!this.cState.attacking || this.attack_time >= this.ATTACK_RECOVERY_TIME) && !this.cState.recoiling && !this.cState.transitioning && !this.cState.hazardDeath && !this.cState.hazardRespawning && !this.cState.recoilFrozen && this.cState.onGround && this.CanInput();
	}

	// Token: 0x060005A5 RID: 1445 RVA: 0x0001ED08 File Offset: 0x0001CF08
	public bool CanInspect()
	{
		return !this.gm.isPaused && !this.cState.dashing && this.hero_state != ActorStates.no_input && !this.cState.backDashing && (!this.cState.attacking || this.attack_time >= this.ATTACK_RECOVERY_TIME) && !this.cState.recoiling && !this.cState.transitioning && !this.cState.hazardDeath && !this.cState.hazardRespawning && !this.cState.recoilFrozen && this.cState.onGround && this.CanInput();
	}

	// Token: 0x060005A6 RID: 1446 RVA: 0x0001EDC8 File Offset: 0x0001CFC8
	public bool CanBackDash()
	{
		return !this.gm.isPaused && !this.cState.dashing && this.hero_state != ActorStates.no_input && !this.cState.backDashing && (!this.cState.attacking || this.attack_time >= this.ATTACK_RECOVERY_TIME) && !this.cState.preventBackDash && !this.cState.backDashCooldown && !this.controlReqlinquished && !this.cState.recoilFrozen && !this.cState.recoiling && !this.cState.transitioning && this.cState.onGround && this.playerData.GetBool("canBackDash");
	}

	// Token: 0x060005A7 RID: 1447 RVA: 0x0001EE9C File Offset: 0x0001D09C
	public bool CanSuperDash()
	{
		return !this.gm.isPaused && this.hero_state != ActorStates.no_input && !this.cState.dashing && !this.cState.hazardDeath && !this.cState.hazardRespawning && !this.cState.backDashing && (!this.cState.attacking || this.attack_time >= this.ATTACK_RECOVERY_TIME) && !this.cState.slidingLeft && !this.cState.slidingRight && !this.controlReqlinquished && !this.cState.recoilFrozen && !this.cState.recoiling && !this.cState.transitioning && this.playerData.GetBool("hasSuperDash") && (this.cState.onGround || this.cState.wallSliding);
	}

	// Token: 0x060005A8 RID: 1448 RVA: 0x0001EFA0 File Offset: 0x0001D1A0
	public bool CanDreamNail()
	{
		return !this.gm.isPaused && this.hero_state != ActorStates.no_input && !this.cState.dashing && !this.cState.backDashing && (!this.cState.attacking || this.attack_time >= this.ATTACK_RECOVERY_TIME) && !this.controlReqlinquished && !this.cState.hazardDeath && this.rb2d.velocity.y > -0.1f && !this.cState.hazardRespawning && !this.cState.recoilFrozen && !this.cState.recoiling && !this.cState.transitioning && this.playerData.GetBool("hasDreamNail") && this.cState.onGround;
	}

	// Token: 0x060005A9 RID: 1449 RVA: 0x0001F090 File Offset: 0x0001D290
	public bool CanDreamGate()
	{
		return !this.gm.isPaused && this.hero_state != ActorStates.no_input && !this.cState.dashing && !this.cState.backDashing && (!this.cState.attacking || this.attack_time >= this.ATTACK_RECOVERY_TIME) && !this.controlReqlinquished && !this.cState.hazardDeath && !this.cState.hazardRespawning && !this.cState.recoilFrozen && !this.cState.recoiling && !this.cState.transitioning && this.playerData.GetBool("hasDreamGate") && this.cState.onGround;
	}

	// Token: 0x060005AA RID: 1450 RVA: 0x0001F164 File Offset: 0x0001D364
	public bool CanInteract()
	{
		return this.CanInput() && this.hero_state != ActorStates.no_input && !this.gm.isPaused && !this.cState.dashing && !this.cState.backDashing && !this.cState.attacking && !this.controlReqlinquished && !this.cState.hazardDeath && !this.cState.hazardRespawning && !this.cState.recoilFrozen && !this.cState.recoiling && !this.cState.transitioning && this.cState.onGround;
	}

	// Token: 0x060005AB RID: 1451 RVA: 0x0001F21C File Offset: 0x0001D41C
	public bool CanOpenInventory()
	{
		return (!this.gm.isPaused && this.hero_state != ActorStates.airborne && !this.controlReqlinquished && !this.cState.recoiling && !this.cState.transitioning && !this.cState.hazardDeath && !this.cState.hazardRespawning && this.cState.onGround && !this.playerData.GetBool("disablePause") && !this.cState.dashing && this.CanInput()) || this.playerData.GetBool("atBench");
	}

	// Token: 0x060005AC RID: 1452 RVA: 0x0001F2CD File Offset: 0x0001D4CD
	public void SetDamageMode(int invincibilityType)
	{
		switch (invincibilityType)
		{
			case 0:
				this.damageMode = DamageMode.FULL_DAMAGE;
				return;
			case 1:
				this.damageMode = DamageMode.HAZARD_ONLY;
				return;
			case 2:
				this.damageMode = DamageMode.NO_DAMAGE;
				return;
			default:
				return;
		}
	}

	// Token: 0x060005AD RID: 1453 RVA: 0x0001F2CD File Offset: 0x0001D4CD
	public void SetDamageModeFSM(int invincibilityType)
	{
		switch (invincibilityType)
		{
			case 0:
				this.damageMode = DamageMode.FULL_DAMAGE;
				return;
			case 1:
				this.damageMode = DamageMode.HAZARD_ONLY;
				return;
			case 2:
				this.damageMode = DamageMode.NO_DAMAGE;
				return;
			default:
				return;
		}
	}

	// Token: 0x060005AE RID: 1454 RVA: 0x0001F2F9 File Offset: 0x0001D4F9
	public void ResetQuakeDamage()
	{
		if (this.damageMode == DamageMode.HAZARD_ONLY)
		{
			this.damageMode = DamageMode.FULL_DAMAGE;
		}
	}

	// Token: 0x060005AF RID: 1455 RVA: 0x0001F30B File Offset: 0x0001D50B
	public void SetDamageMode(DamageMode newDamageMode)
	{
		this.damageMode = newDamageMode;
		if (newDamageMode == DamageMode.NO_DAMAGE)
		{
			this.playerData.SetBoolSwappedArgs(true, "isInvincible");
			return;
		}
		this.playerData.SetBoolSwappedArgs(false, "isInvincible");
	}

	// Token: 0x060005B0 RID: 1456 RVA: 0x0001F33B File Offset: 0x0001D53B
	public void StopAnimationControl()
	{
		//this.animCtrl.StopControl();
	}

	// Token: 0x060005B1 RID: 1457 RVA: 0x0001F348 File Offset: 0x0001D548
	public void StartAnimationControl()
	{
		//this.animCtrl.StartControl();
	}

	// Token: 0x060005B2 RID: 1458 RVA: 0x0001F355 File Offset: 0x0001D555
	public void IgnoreInput()
	{
		if (this.acceptingInput)
		{
			this.acceptingInput = false;
			this.ResetInput();
		}
	}

	// Token: 0x060005B3 RID: 1459 RVA: 0x0001F36C File Offset: 0x0001D56C
	public void IgnoreInputWithoutReset()
	{
		if (this.acceptingInput)
		{
			this.acceptingInput = false;
		}
	}

	// Token: 0x060005B4 RID: 1460 RVA: 0x0001F37D File Offset: 0x0001D57D
	public void AcceptInput()
	{
		this.acceptingInput = true;
	}

	// Token: 0x060005B5 RID: 1461 RVA: 0x0001F386 File Offset: 0x0001D586
	public void Pause()
	{
		this.PauseInput();
		this.PauseAudio();
		this.JumpReleased();
		this.cState.isPaused = true;
	}

	// Token: 0x060005B6 RID: 1462 RVA: 0x0001F3A6 File Offset: 0x0001D5A6
	public void UnPause()
	{
		this.cState.isPaused = false;
		this.UnPauseAudio();
		this.UnPauseInput();
	}

	// Token: 0x060005B7 RID: 1463 RVA: 0x0001F3C0 File Offset: 0x0001D5C0
	public void NearBench(bool isNearBench)
	{
		this.cState.nearBench = isNearBench;
	}

	// Token: 0x060005B8 RID: 1464 RVA: 0x0001F3CE File Offset: 0x0001D5CE
	public void SetWalkZone(bool inWalkZone)
	{
		this.cState.inWalkZone = inWalkZone;
	}

	// Token: 0x060005B9 RID: 1465 RVA: 0x0001F3DC File Offset: 0x0001D5DC
	public void ResetState()
	{
		this.cState.Reset();
	}

	// Token: 0x060005BA RID: 1466 RVA: 0x0001F3E9 File Offset: 0x0001D5E9
	public void StopPlayingAudio()
	{
		//this.audioCtrl.StopAllSounds();
	}

	// Token: 0x060005BB RID: 1467 RVA: 0x0001F3F6 File Offset: 0x0001D5F6
	public void PauseAudio()
	{
		//this.audioCtrl.PauseAllSounds();
	}

	// Token: 0x060005BC RID: 1468 RVA: 0x0001F403 File Offset: 0x0001D603
	public void UnPauseAudio()
	{
		//this.audioCtrl.UnPauseAllSounds();
	}

	// Token: 0x060005BD RID: 1469 RVA: 0x0001F410 File Offset: 0x0001D610
	private void PauseInput()
	{
		if (this.acceptingInput)
		{
			this.acceptingInput = false;
		}
		this.lastInputState = new Vector2(this.move_input, this.vertical_input);
	}

	// Token: 0x060005BE RID: 1470 RVA: 0x0001F438 File Offset: 0x0001D638
	private void UnPauseInput()
	{
		if (!this.controlReqlinquished)
		{
			Vector2 vector = this.lastInputState;
			//if (this.inputHandler.inputActions.right.IsPressed)
			if (RightPressed)
			{
				this.move_input = this.lastInputState.x;
			}
			//else if (this.inputHandler.inputActions.left.IsPressed)
			else if (LeftPressed)
			{
				this.move_input = this.lastInputState.x;
			}
			else
			{
				this.rb2d.velocity = new Vector2(0f, this.rb2d.velocity.y);
				this.move_input = 0f;
			}
			this.vertical_input = this.lastInputState.y;
			this.acceptingInput = true;
		}
	}

	// Token: 0x060005BF RID: 1471 RVA: 0x0001F4F3 File Offset: 0x0001D6F3
	public void SpawnSoftLandingPrefab()
	{
		//this.softLandingEffectPrefab.Spawn(this.transform.position);
	}

	// Token: 0x060005C0 RID: 1472 RVA: 0x0001F50C File Offset: 0x0001D70C
	public void AffectedByGravity(bool gravityApplies)
	{
		float gravityScale = this.rb2d.gravityScale;
		if (this.rb2d.gravityScale > Mathf.Epsilon && !gravityApplies)
		{
			this.prevGravityScale = this.rb2d.gravityScale;
			this.rb2d.gravityScale = 0f;
			return;
		}
		if (this.rb2d.gravityScale <= Mathf.Epsilon && gravityApplies)
		{
			this.rb2d.gravityScale = this.prevGravityScale;
			this.prevGravityScale = 0f;
		}
	}

	// Token: 0x060005C1 RID: 1473 RVA: 0x0001F594 File Offset: 0x0001D794
	private void LookForInput()
	{
		if (this.acceptingInput && !this.gm.isPaused && this.isGameplayScene)
		{
			//this.move_input = this.inputHandler.inputActions.moveVector.Vector.x;
			//this.vertical_input = this.inputHandler.inputActions.moveVector.Vector.y;
			move_input = InputVector.x;
			vertical_input = InputVector.y;
			this.FilterInput();
			if (this.playerData.GetBool("hasWalljump") && this.CanWallSlide() && !this.cState.attacking)
			{
				if (this.touchingWallL && LeftPressed && !this.cState.wallSliding)
				{
					this.airDashed = false;
					this.doubleJumped = false;
					//this.wallSlideVibrationPlayer.Play();
					this.cState.wallSliding = true;
					this.cState.willHardLand = false;
					//this.wallslideDustPrefab.enableEmission = true;
					/*var e = wallslideDustPrefab.emission;
					e.enabled = true;*/
					this.wallSlidingL = true;
					this.wallSlidingR = false;
					this.FaceLeft();
					this.CancelFallEffects();
				}
				if (this.touchingWallR && RightPressed && !this.cState.wallSliding)
				{
					this.airDashed = false;
					this.doubleJumped = false;
					//this.wallSlideVibrationPlayer.Play();
					this.cState.wallSliding = true;
					this.cState.willHardLand = false;
					//this.wallslideDustPrefab.enableEmission = true;
					/*var e = wallslideDustPrefab.emission;
					e.enabled = true;*/
					this.wallSlidingL = false;
					this.wallSlidingR = true;
					this.FaceRight();
					this.CancelFallEffects();
				}
			}
			if (this.cState.wallSliding && DownWasPressed)
			{
				this.CancelWallsliding();
				this.FlipSprite();
			}
			if (this.wallLocked && this.wallJumpedL && RightPressed && this.wallLockSteps >= this.WJLOCK_STEPS_SHORT)
			{
				this.wallLocked = false;
			}
			if (this.wallLocked && this.wallJumpedR && LeftPressed && this.wallLockSteps >= this.WJLOCK_STEPS_SHORT)
			{
				this.wallLocked = false;
			}
			if (JumpWasReleased && this.jumpReleaseQueueingEnabled)
			{
				this.jumpReleaseQueueSteps = this.JUMP_RELEASE_QUEUE_STEPS;
				this.jumpReleaseQueuing = true;
			}
			if (!JumpPressed)
			{
				this.JumpReleased();
			}
			if (!DashPressed)
			{
				if (this.cState.preventDash && !this.cState.dashCooldown)
				{
					this.cState.preventDash = false;
				}
				this.dashQueuing = false;
			}
			if (!AttackPressed)
			{
				this.attackQueuing = false;
			}
		}
	}

	// Token: 0x060005C2 RID: 1474 RVA: 0x0001F8A4 File Offset: 0x0001DAA4
	private void LookForQueueInput()
	{
		if (this.acceptingInput && !this.gm.isPaused && this.isGameplayScene)
		{
			if (JumpWasPressed)
			{
				if (this.CanWallJump())
				{
					this.DoWallJump();
				}
				else if (this.CanJump())
				{
					this.HeroJump();
				}
				else if (this.CanDoubleJump())
				{
					this.DoDoubleJump();
				}
				else if (this.CanInfiniteAirJump())
				{
					this.CancelJump();
					//this.audioCtrl.PlaySound(HeroSounds.JUMP);
					this.ResetLook();
					this.cState.jumping = true;
				}
				else
				{
					this.jumpQueueSteps = 0;
					this.jumpQueuing = true;
					this.doubleJumpQueueSteps = 0;
					this.doubleJumpQueuing = true;
				}
			}
			if (DashWasPressed && !ModHooks.OnDashPressed())
			{
				if (this.CanDash())
				{
					this.HeroDash();
				}
				else
				{
					this.dashQueueSteps = 0;
					this.dashQueuing = true;
				}
			}
			if (AttackWasPressed)
			{
				if (this.CanAttack())
				{
					this.DoAttack();
				}
				else
				{
					this.attackQueueSteps = 0;
					this.attackQueuing = true;
				}
			}
			if (JumpPressed)
			{
				if (this.jumpQueueSteps <= this.JUMP_QUEUE_STEPS && this.CanJump() && this.jumpQueuing)
				{
					this.HeroJump();
				}
				else if (this.doubleJumpQueueSteps <= this.DOUBLE_JUMP_QUEUE_STEPS && this.CanDoubleJump() && this.doubleJumpQueuing)
				{
					if (this.cState.onGround)
					{
						this.HeroJump();
					}
					else
					{
						this.DoDoubleJump();
					}
				}
				if (this.CanSwim())
				{
					if (this.hero_state != ActorStates.airborne)
					{
						this.SetState(ActorStates.airborne);
					}
					this.cState.swimming = true;
				}
			}
			if (DashPressed && this.dashQueueSteps <= this.DASH_QUEUE_STEPS && this.CanDash() && this.dashQueuing && !ModHooks.OnDashPressed() && this.CanDash())
			{
				this.HeroDash();
			}
			if (AttackPressed && this.attackQueueSteps <= this.ATTACK_QUEUE_STEPS && this.CanAttack() && this.attackQueuing)
			{
				this.DoAttack();
			}
		}
	}

	// Token: 0x060005C3 RID: 1475 RVA: 0x0001FB00 File Offset: 0x0001DD00
	private void HeroJump()
	{
		//this.jumpEffectPrefab.Spawn(this.transform.position);
		//this.audioCtrl.PlaySound(HeroSounds.JUMP);
		this.ResetLook();
		this.cState.recoiling = false;
		this.cState.jumping = true;
		this.jumpQueueSteps = 0;
		this.jumped_steps = 0;
		this.doubleJumpQueuing = false;
	}

	// Token: 0x060005C4 RID: 1476 RVA: 0x0001FB63 File Offset: 0x0001DD63
	private void HeroJumpNoEffect()
	{
		this.ResetLook();
		this.jump_steps = 5;
		this.cState.jumping = true;
		this.jumpQueueSteps = 0;
		this.jumped_steps = 0;
		this.jump_steps = 5;
	}

	// Token: 0x060005C5 RID: 1477 RVA: 0x0001FB94 File Offset: 0x0001DD94
	private void DoWallJump()
	{
		//this.wallPuffPrefab.SetActive(true);
		//this.audioCtrl.PlaySound(HeroSounds.WALLJUMP);
		//VibrationManager.PlayVibrationClipOneShot(this.wallJumpVibration, null, false, "");
		if (this.touchingWallL)
		{
			this.FaceRight();
			this.wallJumpedR = true;
			this.wallJumpedL = false;
		}
		else if (this.touchingWallR)
		{
			this.FaceLeft();
			this.wallJumpedR = false;
			this.wallJumpedL = true;
		}
		this.CancelWallsliding();
		this.cState.touchingWall = false;
		this.touchingWallL = false;
		this.touchingWallR = false;
		this.airDashed = false;
		this.doubleJumped = false;
		this.currentWalljumpSpeed = this.WJ_KICKOFF_SPEED;
		this.walljumpSpeedDecel = (this.WJ_KICKOFF_SPEED - this.RUN_SPEED) / (float)this.WJLOCK_STEPS_LONG;
		//this.dashBurst.SendEvent("CANCEL");
		this.cState.jumping = true;
		this.wallLockSteps = 0;
		this.wallLocked = true;
		this.jumpQueueSteps = 0;
		this.jumped_steps = 0;
	}

	// Token: 0x060005C6 RID: 1478 RVA: 0x0001FC9C File Offset: 0x0001DE9C
	private void DoDoubleJump()
	{
		/*this.dJumpWingsPrefab.SetActive(true);
		this.dJumpFlashPrefab.SetActive(true);*/
		//this.dJumpFeathers.Play();
		//VibrationManager.PlayVibrationClipOneShot(this.doubleJumpVibration, null, false, "");
		if (doubleJumpClip != null)
		{
			this.audioSource.PlayOneShot(this.doubleJumpClip, 1f);
		}
		this.ResetLook();
		this.cState.jumping = false;
		this.cState.doubleJumping = true;
		this.doubleJump_steps = 0;
		this.doubleJumped = true;
	}

	// Token: 0x060005C7 RID: 1479 RVA: 0x0001FD2C File Offset: 0x0001DF2C
	private void DoHardLanding()
	{
		this.AffectedByGravity(true);
		this.ResetInput();
		this.SetState(ActorStates.hard_landing);
		this.CancelAttack();
		this.hardLanded = true;
		//this.audioCtrl.PlaySound(HeroSounds.HARD_LANDING);
		//this.hardLandingEffectPrefab.Spawn(this.transform.position);
	}

	// Token: 0x060005C8 RID: 1480 RVA: 0x0001FD7D File Offset: 0x0001DF7D
	private void DoAttack()
	{
		ModHooks.OnDoAttack();
		this.orig_DoAttack();
	}

	// Token: 0x060005C9 RID: 1481 RVA: 0x0001FD8C File Offset: 0x0001DF8C
	private void HeroDash()
	{
		if (!this.cState.onGround && !this.inAcid)
		{
			this.airDashed = true;
		}
		this.ResetAttacksDash();
		this.CancelBounce();
		//this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN);
		//this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK);
		//this.audioCtrl.PlaySound(HeroSounds.DASH);
		this.ResetLook();
		this.cState.recoiling = false;
		if (this.cState.wallSliding)
		{
			this.FlipSprite();
		}
		else if (RightPressed)
		{
			this.FaceRight();
		}
		else if (LeftPressed)
		{
			this.FaceLeft();
		}
		this.cState.dashing = true;
		this.dashQueueSteps = 0;
		//HeroActions inputActions = this.inputHandler.inputActions;
		if (DownPressed && !this.cState.onGround && this.playerData.GetBool("equippedCharm_31") && !LeftPressed && !RightPressed)
		{
			//this.dashBurst.transform.localPosition = new Vector3(-0.07f, 3.74f, 0.01f);
			//this.dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
			this.dashingDown = true;
		}
		else
		{
			//this.dashBurst.transform.localPosition = new Vector3(4.11f, -0.55f, 0.001f);
			//this.dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
			this.dashingDown = false;
		}
		if (this.playerData.GetBool("equippedCharm_31"))
		{
			this.dashCooldownTimer = this.DASH_COOLDOWN_CH;
		}
		else
		{
			this.dashCooldownTimer = this.DASH_COOLDOWN;
		}
		if (this.playerData.GetBool("hasShadowDash") && this.shadowDashTimer <= 0f)
		{
			this.shadowDashTimer = this.SHADOW_DASH_COOLDOWN;
			this.cState.shadowDashing = true;
			if (this.playerData.GetBool("equippedCharm_16"))
			{
				this.audioSource.PlayOneShot(this.sharpShadowClip, 1f);
				//this.sharpShadowPrefab.SetActive(true);
			}
			else
			{
				this.audioSource.PlayOneShot(this.shadowDashClip, 1f);
			}
		}
		/*if (this.cState.shadowDashing)
		{
			if (this.dashingDown)
			{
				this.dashEffect = this.shadowdashDownBurstPrefab.Spawn(new Vector3(this.transform.position.x, this.transform.position.y + 3.5f, this.transform.position.z + 0.00101f));
				this.dashEffect.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
			}
			else if (this.transform.localScale.x > 0f)
			{
				this.dashEffect = this.shadowdashBurstPrefab.Spawn(new Vector3(this.transform.position.x + 5.21f, this.transform.position.y - 0.58f, this.transform.position.z + 0.00101f));
				this.dashEffect.transform.localScale = new Vector3(1.919591f, this.dashEffect.transform.localScale.y, this.dashEffect.transform.localScale.z);
			}
			else
			{
				this.dashEffect = this.shadowdashBurstPrefab.Spawn(new Vector3(this.transform.position.x - 5.21f, this.transform.position.y - 0.58f, this.transform.position.z + 0.00101f));
				this.dashEffect.transform.localScale = new Vector3(-1.919591f, this.dashEffect.transform.localScale.y, this.dashEffect.transform.localScale.z);
			}
			this.shadowRechargePrefab.SetActive(true);
			FSMUtility.LocateFSM(this.shadowRechargePrefab, "Recharge Effect").SendEvent("RESET");
			this.shadowdashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission = true;
			VibrationManager.PlayVibrationClipOneShot(this.shadowDashVibration, null, false, "");
			this.shadowRingPrefab.Spawn(this.transform.position);
		}
		else
		{
			this.dashBurst.SendEvent("PLAY");
			this.dashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission = true;
			VibrationManager.PlayVibrationClipOneShot(this.dashVibration, null, false, "");
		}*/
		if (this.cState.onGround && !this.cState.shadowDashing)
		{
			//this.dashEffect = this.backDashPrefab.Spawn(this.transform.position);
			/*this.dashEffect = GameObject.Instantiate(backDashPrefab,transform.position,Quaternion.identity);
			this.dashEffect.transform.localScale = new Vector3(this.transform.localScale.x * -1f, this.transform.localScale.y, this.transform.localScale.z);*/
		}
	}

	// Token: 0x060005CA RID: 1482 RVA: 0x00020314 File Offset: 0x0001E514
	private void StartFallRumble()
	{
		this.fallRumble = true;
		//this.audioCtrl.PlaySound(HeroSounds.FALLING);
		//GameCameras.instance.cameraShakeFSM.Fsm.Variables.FindFsmBool("RumblingFall").Value = true;
	}

	// Token: 0x060005CB RID: 1483 RVA: 0x00020350 File Offset: 0x0001E550
	private void SetState(ActorStates newState)
	{
		if (newState == ActorStates.grounded)
		{
			if (Mathf.Abs(this.move_input) > Mathf.Epsilon)
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
			newState = this.prev_hero_state;
		}
		if (newState != this.hero_state)
		{
			this.prev_hero_state = this.hero_state;
			this.hero_state = newState;
			//this.animCtrl.UpdateState(newState);
		}
	}

	// Token: 0x060005CC RID: 1484 RVA: 0x000203B8 File Offset: 0x0001E5B8
	private void FinishedEnteringScene(bool setHazardMarker = true, bool preventRunBob = false)
	{
		if (this.isEnteringFirstLevel)
		{
			this.isEnteringFirstLevel = false;
		}
		else
		{
			this.playerData.SetBoolSwappedArgs(false, "disablePause");
		}
		this.cState.transitioning = false;
		this.transitionState = HeroTransitionState.WAITING_TO_TRANSITION;
		this.stopWalkingOut = false;
		if (this.exitedSuperDashing || this.exitedQuake)
		{
			this.controlReqlinquished = true;
			this.IgnoreInput();
		}
		else
		{
			this.SetStartingMotionState(preventRunBob);
			this.AffectedByGravity(true);
		}
		if (setHazardMarker)
		{
			if (/*this.gm.startedOnThisScene || */this.sceneEntryGate == null)
			{
				this.playerData.SetHazardRespawn(this.transform.position, this.cState.facingRight);
			}
			else if (!this.sceneEntryGate.nonHazardGate)
			{
				this.playerData.SetHazardRespawn(this.sceneEntryGate.respawnMarker);
			}
		}
		if (this.exitedQuake)
		{
			this.SetDamageMode(DamageMode.HAZARD_ONLY);
		}
		else
		{
			this.SetDamageMode(DamageMode.FULL_DAMAGE);
		}
		if (this.enterWithoutInput || this.exitedSuperDashing || this.exitedQuake)
		{
			this.enterWithoutInput = false;
		}
		else
		{
			this.AcceptInput();
		}
		this.gm.FinishedEnteringScene();
		if (this.exitedSuperDashing)
		{
			this.exitedSuperDashing = false;
		}
		if (this.exitedQuake)
		{
			this.exitedQuake = false;
		}
		this.positionHistory[0] = this.transform.position;
		this.positionHistory[1] = this.transform.position;
		this.tilemapTestActive = true;
	}

	// Token: 0x060005CD RID: 1485 RVA: 0x00020539 File Offset: 0x0001E739
	private IEnumerator Die()
	{
		if (this.OnDeath != null)
		{
			this.OnDeath();
		}
		if (!this.cState.dead)
		{
			//this.playerData.disablePause = true;
			this.boundsChecking = false;
			this.StopTilemapTest();
			this.cState.onConveyor = false;
			this.cState.onConveyorV = false;
			this.rb2d.velocity = new Vector2(0f, 0f);
			this.CancelRecoilHorizontal();
			string currentMapZone = this.gm.GetCurrentMapZone();
			if (currentMapZone == "DREAM_WORLD" || currentMapZone == "GODS_GLORY")
			{
				this.RelinquishControl();
				this.StopAnimationControl();
				this.AffectedByGravity(false);
				this.playerData.isInvincible = true;
				this.ResetHardLandingTimer();
				this.renderer.enabled = false;
				//this.heroDeathPrefab.SetActive(true);
			}
			else
			{
				/*if (this.playerData.permadeathMode == 1)
				{
					this.playerData.permadeathMode = 2;
				}*/
				this.AffectedByGravity(false);
				HeroBox.inactive = true;
				this.rb2d.isKinematic = true;
				this.SetState(ActorStates.no_input);
				this.cState.dead = true;
				this.ResetMotion();
				this.ResetHardLandingTimer();
				this.renderer.enabled = false;
				this.gameObject.layer = 2;
				//this.heroDeathPrefab.SetActive(true);
				yield return null;
				this.StartCoroutine(this.gm.PlayerDead(this.DEATH_WAIT));
			}
		}
		yield break;
	}

	// Token: 0x060005CE RID: 1486 RVA: 0x00020548 File Offset: 0x0001E748
	private IEnumerator DieFromHazard(HazardType hazardType, float angle)
	{
		if (!this.cState.hazardDeath)
		{
			//this.playerData.disablePause = true;
			this.StopTilemapTest();
			this.SetState(ActorStates.no_input);
			this.cState.hazardDeath = true;
			this.ResetMotion();
			this.ResetHardLandingTimer();
			this.AffectedByGravity(false);
			this.renderer.enabled = false;
			this.gameObject.layer = 2;
			if (hazardType == HazardType.SPIKES)
			{
				//GameObject gameObject = this.spikeDeathPrefab.Spawn();
				//GameObject gameObject = GameObject.Instantiate(spikeDeathPrefab);
				//gameObject.transform.position = this.transform.position;
				//FSMUtility.SetFloat(gameObject.GetComponent<PlayMakerFSM>(), "Spike Direction", angle * 57.29578f);
			}
			else if (hazardType == HazardType.ACID)
			{
				//GameObject gameObject2 = this.acidDeathPrefab.Spawn();
				//GameObject gameObject2 = GameObject.Instantiate(acidDeathPrefab);
				//gameObject2.transform.position = this.transform.position;
				//gameObject2.transform.localScale = this.transform.localScale;
			}
			yield return null;
			this.StartCoroutine(this.gm.PlayerDeadFromHazard(0f));
		}
		yield break;
	}

	// Token: 0x060005CF RID: 1487 RVA: 0x00020565 File Offset: 0x0001E765
	private IEnumerator StartRecoil(CollisionSide impactSide, bool spawnDamageEffect, int damageAmount)
	{
		if (!this.cState.recoiling)
		{
			//this.playerData.disablePause = true;
			this.ResetMotion();
			this.AffectedByGravity(false);
			if (impactSide == CollisionSide.left)
			{
				this.recoilVector = new Vector2(this.RECOIL_VELOCITY, this.RECOIL_VELOCITY * 0.5f);
				if (this.cState.facingRight)
				{
					this.FlipSprite();
				}
			}
			else if (impactSide == CollisionSide.right)
			{
				this.recoilVector = new Vector2(-this.RECOIL_VELOCITY, this.RECOIL_VELOCITY * 0.5f);
				if (!this.cState.facingRight)
				{
					this.FlipSprite();
				}
			}
			else
			{
				this.recoilVector = Vector2.zero;
			}
			this.SetState(ActorStates.no_input);
			this.cState.recoilFrozen = true;
			if (spawnDamageEffect)
			{
				//this.damageEffectFSM.SendEvent("DAMAGE");
				if (damageAmount > 1)
				{
					//UnityEngine.Object.Instantiate<GameObject>(this.takeHitDoublePrefab, this.transform.position, this.transform.rotation);
				}
			}
			if (this.playerData.GetBool("equippedCharm_4"))
			{
				this.StartCoroutine(this.Invulnerable(this.INVUL_TIME_STAL));
			}
			else
			{
				this.StartCoroutine(this.Invulnerable(this.INVUL_TIME));
			}
			yield return this.takeDamageCoroutine = this.StartCoroutine(this.gm.FreezeMoment(this.DAMAGE_FREEZE_DOWN, this.DAMAGE_FREEZE_WAIT, this.DAMAGE_FREEZE_UP, 0.0001f));
			this.cState.recoilFrozen = false;
			this.cState.recoiling = true;
			//this.playerData.disablePause = false;
		}
		yield break;
	}

	// Token: 0x060005D0 RID: 1488 RVA: 0x00020589 File Offset: 0x0001E789
	private IEnumerator Invulnerable(float duration)
	{
		this.cState.invulnerable = true;
		yield return new WaitForSeconds(this.DAMAGE_FREEZE_DOWN);
		//this.invPulse.startInvulnerablePulse();
		yield return new WaitForSeconds(duration);
		//this.invPulse.stopInvulnerablePulse();
		this.cState.invulnerable = false;
		this.cState.recoiling = false;
		yield break;
	}

	// Token: 0x060005D1 RID: 1489 RVA: 0x0002059F File Offset: 0x0001E79F
	private IEnumerator FirstFadeIn()
	{
		yield return new WaitForSeconds(0.25f);
		//this.gm.FadeSceneIn();
		this.fadedSceneIn = true;
		yield break;
	}

	// Token: 0x060005D2 RID: 1490 RVA: 0x000205B0 File Offset: 0x0001E7B0
	private void FallCheck()
	{
		if (this.rb2d.velocity.y <= -1E-06f)
		{
			if (!this.CheckTouchingGround())
			{
				this.cState.falling = true;
				this.cState.onGround = false;
				this.cState.wallJumping = false;
				//this.proxyFSM.SendEvent("HeroCtrl-LeftGround");
				if (this.hero_state != ActorStates.no_input)
				{
					this.SetState(ActorStates.airborne);
				}
				if (this.cState.wallSliding)
				{
					this.fallTimer = 0f;
				}
				else
				{
					this.fallTimer += Time.deltaTime;
				}
				if (this.fallTimer > this.BIG_FALL_TIME)
				{
					if (!this.cState.willHardLand)
					{
						this.cState.willHardLand = true;
					}
					if (!this.fallRumble)
					{
						this.StartFallRumble();
					}
				}
				if (this.fallCheckFlagged)
				{
					this.fallCheckFlagged = false;
					return;
				}
			}
		}
		else
		{
			this.cState.falling = false;
			this.fallTimer = 0f;
			if (this.transitionState != HeroTransitionState.ENTERING_SCENE)
			{
				this.cState.willHardLand = false;
			}
			if (this.fallCheckFlagged)
			{
				this.fallCheckFlagged = false;
			}
			if (this.fallRumble)
			{
				this.CancelFallEffects();
			}
		}
	}

	// Token: 0x060005D3 RID: 1491 RVA: 0x000206E0 File Offset: 0x0001E8E0
	private void OutOfBoundsCheck()
	{
		if (this.isGameplayScene)
		{
			Vector2 vector = this.transform.position;
			if ((vector.y < -60f || vector.y > this.gm.sceneHeight + 60f || vector.x < -60f || vector.x > this.gm.sceneWidth + 60f) && !this.cState.dead)
			{
				bool flag = this.boundsChecking;
			}
		}
	}

	// Token: 0x060005D4 RID: 1492 RVA: 0x0002076C File Offset: 0x0001E96C
	private void ConfirmOutOfBounds()
	{
		if (this.boundsChecking)
		{
			Debug.Log("Confirming out of bounds");
			Vector2 vector = this.transform.position;
			if (vector.y < -60f || vector.y > this.gm.sceneHeight + 60f || vector.x < -60f || vector.x > this.gm.sceneWidth + 60f)
			{
				if (!this.cState.dead)
				{
					this.rb2d.velocity = Vector2.zero;
					Debug.LogFormat("Pos: {0} Transition State: {1}", new object[]
					{
						this.transform.position,
						this.transitionState
					});
					return;
				}
			}
			else
			{
				this.boundsChecking = false;
			}
		}
	}

	// Token: 0x060005D5 RID: 1493 RVA: 0x00020844 File Offset: 0x0001EA44
	private void FailSafeChecks()
	{
		if (this.hero_state == ActorStates.hard_landing)
		{
			this.hardLandFailSafeTimer += Time.deltaTime;
			if (this.hardLandFailSafeTimer > this.HARD_LANDING_TIME + 0.3f)
			{
				this.SetState(ActorStates.grounded);
				this.BackOnGround();
				this.hardLandFailSafeTimer = 0f;
			}
		}
		else
		{
			this.hardLandFailSafeTimer = 0f;
		}
		if (this.cState.hazardDeath)
		{
			this.hazardDeathTimer += Time.deltaTime;
			if (this.hazardDeathTimer > this.HAZARD_DEATH_CHECK_TIME && this.hero_state != ActorStates.no_input)
			{
				this.ResetMotion();
				this.AffectedByGravity(false);
				this.SetState(ActorStates.no_input);
				this.hazardDeathTimer = 0f;
			}
		}
		else
		{
			this.hazardDeathTimer = 0f;
		}
		if (this.rb2d.velocity.y == 0f && !this.cState.onGround && !this.cState.falling && !this.cState.jumping && !this.cState.dashing && this.hero_state != ActorStates.hard_landing && this.hero_state != ActorStates.no_input)
		{
			if (this.CheckTouchingGround())
			{
				this.floatingBufferTimer += Time.deltaTime;
				if (this.floatingBufferTimer > this.FLOATING_CHECK_TIME)
				{
					if (this.cState.recoiling)
					{
						this.CancelDamageRecoil();
					}
					this.BackOnGround();
					this.floatingBufferTimer = 0f;
					return;
				}
			}
			else
			{
				this.floatingBufferTimer = 0f;
			}
		}
	}

	// Token: 0x060005D6 RID: 1494 RVA: 0x000209C8 File Offset: 0x0001EBC8
	public Transform LocateSpawnPoint()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("RespawnPoint");
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].name == this.playerData.GetString("respawnMarkerName"))
			{
				return array[i].transform;
			}
		}
		return null;
	}

	// Token: 0x060005D7 RID: 1495 RVA: 0x00020A17 File Offset: 0x0001EC17
	private void CancelJump()
	{
		this.cState.jumping = false;
		this.jumpReleaseQueuing = false;
		this.jump_steps = 0;
	}

	// Token: 0x060005D8 RID: 1496 RVA: 0x00020A33 File Offset: 0x0001EC33
	private void CancelDoubleJump()
	{
		this.cState.doubleJumping = false;
		this.doubleJump_steps = 0;
	}

	// Token: 0x060005D9 RID: 1497 RVA: 0x00020A48 File Offset: 0x0001EC48
	private void CancelDash()
	{
		if (this.cState.shadowDashing)
		{
			this.cState.shadowDashing = false;
		}
		this.cState.dashing = false;
		this.dash_timer = 0f;
		this.AffectedByGravity(true);
		//this.sharpShadowPrefab.SetActive(false);
		/*if (this.dashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission)
		{
			this.dashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission = false;
		}
		if (this.shadowdashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission)
		{
			this.shadowdashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission = false;
		}*/
		/*var dashEmit = dashParticlesPrefab.GetComponent<ParticleSystem>().emission;
		dashEmit.enabled = false;

		var shadowDashEmit = shadowdashParticlesPrefab.GetComponent<ParticleSystem>().emission;
		shadowDashEmit.enabled = false;*/
	}

	// Token: 0x060005DA RID: 1498 RVA: 0x00020AE0 File Offset: 0x0001ECE0
	private void CancelWallsliding()
	{
		//this.wallslideDustPrefab.enableEmission = false;
		/*var e = wallslideDustPrefab.emission;
		e.enabled = false;*/
		//this.wallSlideVibrationPlayer.Stop();
		this.cState.wallSliding = false;
		this.wallSlidingL = false;
		this.wallSlidingR = false;
		this.touchingWallL = false;
		this.touchingWallR = false;
	}

	// Token: 0x060005DB RID: 1499 RVA: 0x00020B2C File Offset: 0x0001ED2C
	private void CancelBackDash()
	{
		this.cState.backDashing = false;
		this.back_dash_timer = 0f;
	}

	// Token: 0x060005DC RID: 1500 RVA: 0x00020B45 File Offset: 0x0001ED45
	private void CancelDownAttack()
	{
		if (this.cState.downAttacking)
		{
			//this.slashComponent.CancelAttack();
			this.ResetAttacks();
		}
	}

	// Token: 0x060005DD RID: 1501 RVA: 0x00020B65 File Offset: 0x0001ED65
	private void CancelAttack()
	{
		if (this.cState.attacking)
		{
			//this.slashComponent.CancelAttack();
			this.ResetAttacks();
		}
	}

	// Token: 0x060005DE RID: 1502 RVA: 0x00020B85 File Offset: 0x0001ED85
	private void CancelBounce()
	{
		this.cState.bouncing = false;
		this.cState.shroomBouncing = false;
		this.bounceTimer = 0f;
	}

	// Token: 0x060005DF RID: 1503 RVA: 0x00020BAA File Offset: 0x0001EDAA
	private void CancelRecoilHorizontal()
	{
		this.cState.recoilingLeft = false;
		this.cState.recoilingRight = false;
		this.recoilSteps = 0;
	}

	// Token: 0x060005E0 RID: 1504 RVA: 0x00020BCB File Offset: 0x0001EDCB
	private void CancelDamageRecoil()
	{
		this.cState.recoiling = false;
		this.recoilTimer = 0f;
		this.ResetMotion();
		this.AffectedByGravity(true);
		this.SetDamageMode(DamageMode.FULL_DAMAGE);
	}

	// Token: 0x060005E1 RID: 1505 RVA: 0x00020BF8 File Offset: 0x0001EDF8
	private void CancelFallEffects()
	{
		this.fallRumble = false;
		//this.audioCtrl.StopSound(HeroSounds.FALLING);
		//GameCameras.instance.cameraShakeFSM.Fsm.Variables.FindFsmBool("RumblingFall").Value = false;
	}

	// Token: 0x060005E2 RID: 1506 RVA: 0x00020C34 File Offset: 0x0001EE34
	private void ResetAttacks()
	{
		this.cState.nailCharging = false;
		this.nailChargeTimer = 0f;
		this.cState.attacking = false;
		this.cState.upAttacking = false;
		this.cState.downAttacking = false;
		this.attack_time = 0f;
	}

	// Token: 0x060005E3 RID: 1507 RVA: 0x00020C87 File Offset: 0x0001EE87
	private void ResetAttacksDash()
	{
		this.cState.attacking = false;
		this.cState.upAttacking = false;
		this.cState.downAttacking = false;
		this.attack_time = 0f;
	}

	// Token: 0x060005E4 RID: 1508 RVA: 0x00020CB8 File Offset: 0x0001EEB8
	private void ResetMotion()
	{
		this.CancelJump();
		this.CancelDoubleJump();
		this.CancelDash();
		this.CancelBackDash();
		this.CancelBounce();
		this.CancelRecoilHorizontal();
		this.CancelWallsliding();
		this.rb2d.velocity = Vector2.zero;
		this.transition_vel = Vector2.zero;
		this.wallLocked = false;
		this.nailChargeTimer = 0f;
	}

	// Token: 0x060005E5 RID: 1509 RVA: 0x00020D1C File Offset: 0x0001EF1C
	private void ResetMotionNotVelocity()
	{
		this.CancelJump();
		this.CancelDoubleJump();
		this.CancelDash();
		this.CancelBackDash();
		this.CancelBounce();
		this.CancelRecoilHorizontal();
		this.CancelWallsliding();
		this.transition_vel = Vector2.zero;
		this.wallLocked = false;
	}

	// Token: 0x060005E6 RID: 1510 RVA: 0x00020D5A File Offset: 0x0001EF5A
	private void ResetLook()
	{
		this.cState.lookingUp = false;
		this.cState.lookingDown = false;
		this.cState.lookingUpAnim = false;
		this.cState.lookingDownAnim = false;
		this.lookDelayTimer = 0f;
	}

	// Token: 0x060005E7 RID: 1511 RVA: 0x00020D97 File Offset: 0x0001EF97
	private void ResetInput()
	{
		this.move_input = 0f;
		this.vertical_input = 0f;
	}

	// Token: 0x060005E8 RID: 1512 RVA: 0x00020DB0 File Offset: 0x0001EFB0
	private void BackOnGround()
	{
		if (this.landingBufferSteps <= 0)
		{
			this.landingBufferSteps = this.LANDING_BUFFER_STEPS;
			if (!this.cState.onGround && !this.hardLanded && !this.cState.superDashing)
			{
				//this.softLandingEffectPrefab.Spawn(this.transform.position);
				//GameObject.Instantiate(softLandingEffectPrefab, transform.position,Quaternion.identity);
				//VibrationManager.PlayVibrationClipOneShot(this.softLandVibration, null, false, "");
			}
		}
		this.cState.falling = false;
		this.fallTimer = 0f;
		this.dashLandingTimer = 0f;
		this.cState.willHardLand = false;
		this.hardLandingTimer = 0f;
		this.hardLanded = false;
		this.jump_steps = 0;
		if (this.cState.doubleJumping)
		{
			this.HeroJump();
		}
		this.SetState(ActorStates.grounded);
		this.cState.onGround = true;
		this.airDashed = false;
		this.doubleJumped = false;
		/*if (this.dJumpWingsPrefab.activeSelf)
		{
			this.dJumpWingsPrefab.SetActive(false);
		}*/
	}

	// Token: 0x060005E9 RID: 1513 RVA: 0x00020EBC File Offset: 0x0001F0BC
	private void JumpReleased()
	{
		if (this.rb2d.velocity.y > 0f && this.jumped_steps >= this.JUMP_STEPS_MIN && !this.inAcid && !this.cState.shroomBouncing)
		{
			if (this.jumpReleaseQueueingEnabled)
			{
				if (this.jumpReleaseQueuing && this.jumpReleaseQueueSteps <= 0)
				{
					this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, 0f);
					this.CancelJump();
				}
			}
			else
			{
				this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, 0f);
				this.CancelJump();
			}
		}
		this.jumpQueuing = false;
		this.doubleJumpQueuing = false;
		if (this.cState.swimming)
		{
			this.cState.swimming = false;
		}
	}

	// Token: 0x060005EA RID: 1514 RVA: 0x00020FA4 File Offset: 0x0001F1A4
	private void FinishedDashing()
	{
		this.CancelDash();
		this.AffectedByGravity(true);
		//this.animCtrl.FinishedDash();
		//this.proxyFSM.SendEvent("HeroCtrl-DashEnd");
		if (this.cState.touchingWall && !this.cState.onGround && (this.playerData.GetBool("hasWalljump") & (this.touchingWallL || this.touchingWallR)))
		{
			//this.wallslideDustPrefab.enableEmission = true;
			/*var e = wallslideDustPrefab.emission;
			e.enabled = true;*/
			//this.wallSlideVibrationPlayer.Play();
			this.cState.wallSliding = true;
			this.cState.willHardLand = false;
			if (this.touchingWallL)
			{
				this.wallSlidingL = true;
			}
			if (this.touchingWallR)
			{
				this.wallSlidingR = true;
			}
			if (this.dashingDown)
			{
				this.FlipSprite();
			}
		}
	}

	// Token: 0x060005EB RID: 1515 RVA: 0x00021078 File Offset: 0x0001F278
	private void SetStartingMotionState()
	{
		this.SetStartingMotionState(false);
	}

	// Token: 0x060005EC RID: 1516 RVA: 0x00021084 File Offset: 0x0001F284
	private void SetStartingMotionState(bool preventRunDip)
	{
		this.move_input = ((this.acceptingInput || preventRunDip) ? InputVector.x : 0f);
		this.cState.touchingWall = false;
		if (this.CheckTouchingGround())
		{
			this.cState.onGround = true;
			this.SetState(ActorStates.grounded);
			this.ResetAirMoves();
			if (this.enteringVertically)
			{
				this.SpawnSoftLandingPrefab();
				//this.animCtrl.playLanding = true;
				this.enteringVertically = false;
			}
		}
		else
		{
			this.cState.onGround = false;
			this.SetState(ActorStates.airborne);
		}
		//this.animCtrl.UpdateState(this.hero_state);
	}

	// Token: 0x060005ED RID: 1517 RVA: 0x00021131 File Offset: 0x0001F331
	[Obsolete("This was used specifically for underwater swimming in acid but is no longer in use.")]
	private void EnterAcid()
	{
		this.rb2d.gravityScale = this.UNDERWATER_GRAVITY;
		this.inAcid = true;
		this.cState.inAcid = true;
	}

	// Token: 0x060005EE RID: 1518 RVA: 0x00021158 File Offset: 0x0001F358
	[Obsolete("This was used specifically for underwater swimming in acid but is no longer in use.")]
	private void ExitAcid()
	{
		this.rb2d.gravityScale = this.DEFAULT_GRAVITY;
		this.inAcid = false;
		this.cState.inAcid = false;
		this.airDashed = false;
		this.doubleJumped = false;
		if (JumpPressed)
		{
			this.HeroJump();
		}
	}

	// Token: 0x060005EF RID: 1519 RVA: 0x000211B4 File Offset: 0x0001F3B4
	private void TileMapTest()
	{
		if (this.tilemapTestActive && !this.cState.jumping)
		{
			Vector2 vector = this.transform.position;
			Vector2 direction = new Vector2(this.positionHistory[0].x - vector.x, this.positionHistory[0].y - vector.y);
			float magnitude = direction.magnitude;
			RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, direction, magnitude, 256);
			if (raycastHit2D.collider != null)
			{
				Debug.LogFormat("TERRAIN INGRESS {0} at {1} Jumping: {2}", new object[]
				{
					this.gm.GetSceneNameString(),
					vector,
					this.cState.jumping
				});
				this.ResetMotion();
				this.rb2d.velocity = Vector2.zero;
				if (this.cState.dashing)
				{
					this.FinishedDashing();
					this.transform.SetPosition2D(this.positionHistory[1]);
				}
				if (this.cState.superDashing)
				{
					this.transform.SetPosition2D(raycastHit2D.point);
					//this.superDash.SendEvent("HIT WALL");
				}
				if (this.cState.spellQuake)
				{
					//this.spellControl.SendEvent("Hero Landed");
					this.transform.SetPosition2D(this.positionHistory[1]);
				}
				this.tilemapTestActive = false;
				this.tilemapTestCoroutine = base.StartCoroutine(this.TilemapTestPause());
			}
		}
	}

	// Token: 0x060005F0 RID: 1520 RVA: 0x00021342 File Offset: 0x0001F542
	private IEnumerator TilemapTestPause()
	{
		yield return new WaitForSeconds(0.1f);
		this.tilemapTestActive = true;
		yield break;
	}

	// Token: 0x060005F1 RID: 1521 RVA: 0x00021351 File Offset: 0x0001F551
	private void StopTilemapTest()
	{
		if (this.tilemapTestCoroutine != null)
		{
			base.StopCoroutine(this.tilemapTestCoroutine);
			this.tilemapTestActive = false;
		}
	}

	// Token: 0x060005F2 RID: 1522 RVA: 0x0002136E File Offset: 0x0001F56E
	public IEnumerator CheckForTerrainThunk(AttackDirection attackDir)
	{
		bool terrainHit = false;
		float thunkTimer = this.NAIL_TERRAIN_CHECK_TIME;
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
				if (this.playerData.GetBool("equippedCharm_18"))
				{
					num3 += 0.2f;
				}
				if (this.playerData.GetBool("equippedCharm_13"))
				{
					num3 += 0.3f;
				}
				num2 *= num3;
				Vector2 size = new Vector2(0.45f, 0.45f);
				Vector2 origin = new Vector2(this.col2d.bounds.center.x, this.col2d.bounds.center.y + num);
				Vector2 origin2 = new Vector2(this.col2d.bounds.center.x, this.col2d.bounds.max.y);
				Vector2 origin3 = new Vector2(this.col2d.bounds.center.x, this.col2d.bounds.min.y);
				int layerMask = 33554688;
				RaycastHit2D raycastHit2D = default(RaycastHit2D);
				if (attackDir == AttackDirection.normal)
				{
					if ((this.cState.facingRight && !this.cState.wallSliding) || (!this.cState.facingRight && this.cState.wallSliding))
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
                        //this.nailTerrainImpactEffectPrefab.Spawn(raycastHit2D.point, Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f)));
                        if (nailTerrainImpactEffectPrefab != null)
                        {
							GameObject.Instantiate(nailTerrainImpactEffectPrefab, raycastHit2D.point, Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f)));
						}
						if (attackDir == AttackDirection.normal)
						{
							if (this.cState.facingRight)
							{
								this.RecoilLeft();
							}
							else
							{
								this.RecoilRight();
							}
						}
						else if (attackDir == AttackDirection.upward)
						{
							this.RecoilDown();
						}
					}
				}
				thunkTimer -= Time.deltaTime;
			}
			yield return null;
		}
		yield break;
	}

	// Token: 0x060005F3 RID: 1523 RVA: 0x00021384 File Offset: 0x0001F584
	private bool CheckStillTouchingWall(CollisionSide side, bool checkTop = false)
	{
		Vector2 origin = new Vector2(this.col2d.bounds.min.x, this.col2d.bounds.max.y);
		Vector2 origin2 = new Vector2(this.col2d.bounds.min.x, this.col2d.bounds.center.y);
		Vector2 origin3 = new Vector2(this.col2d.bounds.min.x, this.col2d.bounds.min.y);
		Vector2 origin4 = new Vector2(this.col2d.bounds.max.x, this.col2d.bounds.max.y);
		Vector2 origin5 = new Vector2(this.col2d.bounds.max.x, this.col2d.bounds.center.y);
		Vector2 origin6 = new Vector2(this.col2d.bounds.max.x, this.col2d.bounds.min.y);
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
				Debug.LogError("Invalid CollisionSide specified.");
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
			if (raycastHit2D2.collider.GetComponent<SteepSlope>() != null)
			{
				flag = false;
			}
			if (raycastHit2D2.collider.GetComponent<NonSlider>() != null)
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
			if (raycastHit2D3.collider.GetComponent<SteepSlope>() != null)
			{
				flag2 = false;
			}
			if (raycastHit2D3.collider.GetComponent<NonSlider>() != null)
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
			if (raycastHit2D.collider.GetComponent<SteepSlope>() != null)
			{
				flag3 = false;
			}
			if (raycastHit2D.collider.GetComponent<NonSlider>() != null)
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

	// Token: 0x060005F4 RID: 1524 RVA: 0x000216B0 File Offset: 0x0001F8B0
	public bool CheckForBump(CollisionSide side)
	{
		float num = 0.025f;
		float num2 = 0.2f;
		Vector2 vector = new Vector2(this.col2d.bounds.min.x + num2, this.col2d.bounds.min.y + 0.2f);
		Vector2 vector2 = new Vector2(this.col2d.bounds.min.x + num2, this.col2d.bounds.min.y - num);
		Vector2 vector3 = new Vector2(this.col2d.bounds.max.x - num2, this.col2d.bounds.min.y + 0.2f);
		Vector2 vector4 = new Vector2(this.col2d.bounds.max.x - num2, this.col2d.bounds.min.y - num);
		float num3 = 0.32f + num2;
		RaycastHit2D raycastHit2D = default(RaycastHit2D);
		RaycastHit2D raycastHit2D2 = default(RaycastHit2D);
		if (side == CollisionSide.left)
		{
			Debug.DrawLine(vector2, vector2 + Vector2.left * num3, Color.cyan, 0.15f);
			Debug.DrawLine(vector, vector + Vector2.left * num3, Color.cyan, 0.15f);
			raycastHit2D2 = Physics2D.Raycast(vector2, Vector2.left, num3, 256);
			raycastHit2D = Physics2D.Raycast(vector, Vector2.left, num3, 256);
		}
		else if (side == CollisionSide.right)
		{
			Debug.DrawLine(vector4, vector4 + Vector2.right * num3, Color.cyan, 0.15f);
			Debug.DrawLine(vector3, vector3 + Vector2.right * num3, Color.cyan, 0.15f);
			raycastHit2D2 = Physics2D.Raycast(vector4, Vector2.right, num3, 256);
			raycastHit2D = Physics2D.Raycast(vector3, Vector2.right, num3, 256);
		}
		else
		{
			Debug.LogError("Invalid CollisionSide specified.");
		}
		if (raycastHit2D2.collider != null && raycastHit2D.collider == null)
		{
			Vector2 vector5 = raycastHit2D2.point + new Vector2((side == CollisionSide.right) ? 0.1f : -0.1f, 1f);
			RaycastHit2D raycastHit2D3 = Physics2D.Raycast(vector5, Vector2.down, 1.5f, 256);
			Vector2 vector6 = raycastHit2D2.point + new Vector2((side == CollisionSide.right) ? -0.1f : 0.1f, 1f);
			RaycastHit2D raycastHit2D4 = Physics2D.Raycast(vector6, Vector2.down, 1.5f, 256);
			if (raycastHit2D3.collider != null)
			{
				Debug.DrawLine(vector5, raycastHit2D3.point, Color.cyan, 0.15f);
				if (!(raycastHit2D4.collider != null))
				{
					return true;
				}
				Debug.DrawLine(vector6, raycastHit2D4.point, Color.cyan, 0.15f);
				float num4 = raycastHit2D3.point.y - raycastHit2D4.point.y;
				if (num4 > 0f)
				{
					Debug.Log("Bump Height: " + num4.ToString());
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060005F5 RID: 1525 RVA: 0x00021A4C File Offset: 0x0001FC4C
	public bool CheckNearRoof()
	{
		Vector2 origin = this.col2d.bounds.max;
		Vector2 origin2 = new Vector2(this.col2d.bounds.min.x, this.col2d.bounds.max.y);
		new Vector2(this.col2d.bounds.center.x, this.col2d.bounds.max.y);
		Vector2 origin3 = new Vector2(this.col2d.bounds.center.x + this.col2d.bounds.size.x / 4f, this.col2d.bounds.max.y);
		Vector2 origin4 = new Vector2(this.col2d.bounds.center.x - this.col2d.bounds.size.x / 4f, this.col2d.bounds.max.y);
		Vector2 direction = new Vector2(-0.5f, 1f);
		Vector2 direction2 = new Vector2(0.5f, 1f);
		Vector2 up = Vector2.up;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(origin2, direction, 2f, 256);
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(origin, direction2, 2f, 256);
		RaycastHit2D raycastHit2D3 = Physics2D.Raycast(origin3, up, 1f, 256);
		RaycastHit2D raycastHit2D4 = Physics2D.Raycast(origin4, up, 1f, 256);
		return raycastHit2D.collider != null || raycastHit2D2.collider != null || raycastHit2D3.collider != null || raycastHit2D4.collider != null;
	}

	// Token: 0x060005F6 RID: 1526 RVA: 0x00021C4C File Offset: 0x0001FE4C
	public bool CheckTouchingGround()
	{
		Vector2 vector = new Vector2(this.col2d.bounds.min.x, this.col2d.bounds.center.y);
		Vector2 vector2 = this.col2d.bounds.center;
		Vector2 vector3 = new Vector2(this.col2d.bounds.max.x, this.col2d.bounds.center.y);
		float distance = this.col2d.bounds.extents.y + 0.16f;
		Debug.DrawRay(vector, Vector2.down, Color.yellow);
		Debug.DrawRay(vector2, Vector2.down, Color.yellow);
		Debug.DrawRay(vector3, Vector2.down, Color.yellow);
		RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, Vector2.down, distance, 256);
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(vector2, Vector2.down, distance, 256);
		RaycastHit2D raycastHit2D3 = Physics2D.Raycast(vector3, Vector2.down, distance, 256);
		return raycastHit2D.collider != null || raycastHit2D2.collider != null || raycastHit2D3.collider != null;
	}

	// Token: 0x060005F7 RID: 1527 RVA: 0x00021DB8 File Offset: 0x0001FFB8
	private List<CollisionSide> CheckTouching(PhysLayers layer)
	{
		List<CollisionSide> list = new List<CollisionSide>(4);
		Vector3 center = this.col2d.bounds.center;
		float distance = this.col2d.bounds.extents.x + 0.16f;
		float distance2 = this.col2d.bounds.extents.y + 0.16f;
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

	// Token: 0x060005F8 RID: 1528 RVA: 0x00021EDC File Offset: 0x000200DC
	private List<CollisionSide> CheckTouchingAdvanced(PhysLayers layer)
	{
		List<CollisionSide> list = new List<CollisionSide>();
		Vector2 origin = new Vector2(this.col2d.bounds.min.x, this.col2d.bounds.max.y);
		Vector2 origin2 = new Vector2(this.col2d.bounds.center.x, this.col2d.bounds.max.y);
		Vector2 origin3 = new Vector2(this.col2d.bounds.max.x, this.col2d.bounds.max.y);
		Vector2 origin4 = new Vector2(this.col2d.bounds.min.x, this.col2d.bounds.center.y);
		Vector2 origin5 = new Vector2(this.col2d.bounds.max.x, this.col2d.bounds.center.y);
		Vector2 origin6 = new Vector2(this.col2d.bounds.min.x, this.col2d.bounds.min.y);
		Vector2 origin7 = new Vector2(this.col2d.bounds.center.x, this.col2d.bounds.min.y);
		Vector2 origin8 = new Vector2(this.col2d.bounds.max.x, this.col2d.bounds.min.y);
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

	// Token: 0x060005F9 RID: 1529 RVA: 0x000222A0 File Offset: 0x000204A0
	private CollisionSide FindCollisionDirection(Collision2D collision)
	{
		List<ContactPoint2D> points = new List<ContactPoint2D>();
		collision.GetContacts(points);
		if (points.Count == 0)
		{
			return CollisionSide.bottom;
		}
		//Vector2 normal = collision.GetSafeContact().Normal;
		//float x = normal.x;
		//float y = normal.y;
		Vector2 normal = points[0].normal;
		float x = points[0].normal.x;
		float y = points[0].normal.y;
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
		Debug.LogError(string.Concat(new string[]
		{
			"ERROR: unable to determine direction of collision - contact points at (",
			normal.x.ToString(),
			",",
			normal.y.ToString(),
			")"
		}));
		return CollisionSide.bottom;
	}

	// Token: 0x060005FA RID: 1530 RVA: 0x00022338 File Offset: 0x00020538
	private bool CanJump()
	{
		if (this.hero_state == ActorStates.no_input || this.hero_state == ActorStates.hard_landing || this.hero_state == ActorStates.dash_landing || this.cState.wallSliding || this.cState.dashing || this.cState.backDashing || this.cState.jumping || this.cState.bouncing || this.cState.shroomBouncing)
		{
			return false;
		}
		if (this.cState.onGround)
		{
			return true;
		}
		if (this.ledgeBufferSteps > 0 && !this.cState.dead && !this.cState.hazardDeath && !this.controlReqlinquished && this.headBumpSteps <= 0 && !this.CheckNearRoof())
		{
			this.ledgeBufferSteps = 0;
			return true;
		}
		return false;
	}

	// Token: 0x060005FB RID: 1531 RVA: 0x0002241C File Offset: 0x0002061C
	private bool CanDoubleJump()
	{
		return this.playerData.GetBool("hasDoubleJump") && !this.controlReqlinquished && !this.doubleJumped && !this.inAcid && this.hero_state != ActorStates.no_input && this.hero_state != ActorStates.hard_landing && this.hero_state != ActorStates.dash_landing && !this.cState.dashing && !this.cState.wallSliding && !this.cState.backDashing && !this.cState.attacking && !this.cState.bouncing && !this.cState.shroomBouncing && !this.cState.onGround;
	}

	// Token: 0x060005FC RID: 1532 RVA: 0x000224DB File Offset: 0x000206DB
	private bool CanInfiniteAirJump()
	{
		return this.playerData.GetBool("infiniteAirJump") && this.hero_state != ActorStates.hard_landing && !this.cState.onGround;
	}

	// Token: 0x060005FD RID: 1533 RVA: 0x00022508 File Offset: 0x00020708
	private bool CanSwim()
	{
		return this.hero_state != ActorStates.no_input && this.hero_state != ActorStates.hard_landing && this.hero_state != ActorStates.dash_landing && !this.cState.attacking && !this.cState.dashing && !this.cState.jumping && !this.cState.bouncing && !this.cState.shroomBouncing && !this.cState.onGround && this.inAcid;
	}

	// Token: 0x060005FE RID: 1534 RVA: 0x0002258C File Offset: 0x0002078C
	private bool CanDash()
	{
		return this.hero_state != ActorStates.no_input && this.hero_state != ActorStates.hard_landing && this.hero_state != ActorStates.dash_landing && this.dashCooldownTimer <= 0f && !this.cState.dashing && !this.cState.backDashing && (!this.cState.attacking || this.attack_time >= this.ATTACK_RECOVERY_TIME) && !this.cState.preventDash && (this.cState.onGround || !this.airDashed || this.cState.wallSliding) && !this.cState.hazardDeath && this.playerData.GetBool("canDash");
	}

	// Token: 0x060005FF RID: 1535 RVA: 0x0002265C File Offset: 0x0002085C
	private bool CanAttack()
	{
		return this.attack_cooldown <= 0f && !this.cState.attacking && !this.cState.dashing && !this.cState.dead && !this.cState.hazardDeath && !this.cState.hazardRespawning && !this.controlReqlinquished && this.hero_state != ActorStates.no_input && this.hero_state != ActorStates.hard_landing && this.hero_state != ActorStates.dash_landing;
	}

	// Token: 0x06000600 RID: 1536 RVA: 0x000226E0 File Offset: 0x000208E0
	private bool CanNailCharge()
	{
		return !this.cState.attacking && !this.controlReqlinquished && !this.cState.recoiling && !this.cState.recoilingLeft && !this.cState.recoilingRight && this.playerData.GetBool("hasNailArt");
	}

	// Token: 0x06000601 RID: 1537 RVA: 0x00022740 File Offset: 0x00020940
	private bool CanWallSlide()
	{
		return (this.cState.wallSliding && this.gm.isPaused) || (!this.cState.touchingNonSlider && !this.inAcid && !this.cState.dashing && this.playerData.GetBool("hasWalljump") && !this.cState.onGround && !this.cState.recoiling && !this.gm.isPaused && !this.controlReqlinquished && !this.cState.transitioning && (this.cState.falling || this.cState.wallSliding) && !this.cState.doubleJumping && this.CanInput());
	}

	// Token: 0x06000602 RID: 1538 RVA: 0x00022818 File Offset: 0x00020A18
	private bool CanTakeDamage()
	{
		return this.damageMode != DamageMode.NO_DAMAGE && this.transitionState == HeroTransitionState.WAITING_TO_TRANSITION && !this.cState.invulnerable && !this.cState.recoiling && !this.playerData.GetBool("isInvincible") && !this.cState.dead && !this.cState.hazardDeath/* && !BossSceneController.IsTransitioning*/;
	}

	// Token: 0x06000603 RID: 1539 RVA: 0x00022888 File Offset: 0x00020A88
	private bool CanWallJump()
	{
		return this.playerData.GetBool("hasWalljump") && !this.cState.touchingNonSlider && (this.cState.wallSliding || (this.cState.touchingWall && !this.cState.onGround));
	}

	// Token: 0x06000604 RID: 1540 RVA: 0x000228E4 File Offset: 0x00020AE4
	private bool ShouldHardLand(Collision2D collision)
	{
		return /*!collision.gameObject.GetComponent<NoHardLanding>() &&*/ this.cState.willHardLand && !this.inAcid && this.hero_state != ActorStates.hard_landing;
	}

	// Token: 0x06000605 RID: 1541 RVA: 0x0002291C File Offset: 0x00020B1C
	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (this.cState.superDashing && (this.CheckStillTouchingWall(CollisionSide.left, false) || this.CheckStillTouchingWall(CollisionSide.right, false)))
		{
			//this.superDash.SendEvent("HIT WALL");
		}
		if ((collision.gameObject.layer == 8 || collision.gameObject.CompareTag("HeroWalkable")) && this.CheckTouchingGround())
		{
			//this.proxyFSM.SendEvent("HeroCtrl-Landed");
		}
		if (this.hero_state != ActorStates.no_input)
		{
			CollisionSide collisionSide = this.FindCollisionDirection(collision);
			if (collision.gameObject.layer == 8 || collision.gameObject.CompareTag("HeroWalkable"))
			{
				this.fallTrailGenerated = false;
				if (collisionSide == CollisionSide.top)
				{
					this.headBumpSteps = this.HEAD_BUMP_STEPS;
					if (this.cState.jumping)
					{
						this.CancelJump();
						this.CancelDoubleJump();
					}
					if (this.cState.bouncing)
					{
						this.CancelBounce();
						this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, 0f);
					}
					if (this.cState.shroomBouncing)
					{
						this.CancelBounce();
						this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, 0f);
					}
				}
				if (collisionSide == CollisionSide.bottom)
				{
					if (this.cState.attacking)
					{
						this.CancelDownAttack();
					}
					if (this.ShouldHardLand(collision))
					{
						this.DoHardLanding();
					}
					else if (collision.gameObject.GetComponent<SteepSlope>() == null && this.hero_state != ActorStates.hard_landing)
					{
						this.BackOnGround();
					}
					if (this.cState.dashing && this.dashingDown)
					{
						this.AffectedByGravity(true);
						this.SetState(ActorStates.dash_landing);
						this.hardLanded = true;
						return;
					}
				}
			}
		}
		else if (this.hero_state == ActorStates.no_input && this.transitionState == HeroTransitionState.DROPPING_DOWN && (this.gatePosition == GatePosition.bottom || this.gatePosition == GatePosition.top))
		{
			this.FinishedEnteringScene(true, false);
		}
	}

	// Token: 0x06000606 RID: 1542 RVA: 0x00022B10 File Offset: 0x00020D10
	private void OnCollisionStay2D(Collision2D collision)
	{
		if (this.cState.superDashing && (this.CheckStillTouchingWall(CollisionSide.left, false) || this.CheckStillTouchingWall(CollisionSide.right, false)))
		{
			//this.superDash.SendEvent("HIT WALL");
		}
		if (this.hero_state != ActorStates.no_input && collision.gameObject.layer == 8)
		{
			if (collision.gameObject.GetComponent<NonSlider>() == null)
			{
				this.cState.touchingNonSlider = false;
				if (this.CheckStillTouchingWall(CollisionSide.left, false))
				{
					this.cState.touchingWall = true;
					this.touchingWallL = true;
					this.touchingWallR = false;
				}
				else if (this.CheckStillTouchingWall(CollisionSide.right, false))
				{
					this.cState.touchingWall = true;
					this.touchingWallL = false;
					this.touchingWallR = true;
				}
				else
				{
					this.cState.touchingWall = false;
					this.touchingWallL = false;
					this.touchingWallR = false;
				}
				if (this.CheckTouchingGround())
				{
					if (this.ShouldHardLand(collision))
					{
						this.DoHardLanding();
						return;
					}
					if (this.hero_state != ActorStates.hard_landing && this.hero_state != ActorStates.dash_landing && this.cState.falling)
					{
						this.BackOnGround();
						return;
					}
				}
				else if (this.cState.jumping || this.cState.falling)
				{
					this.cState.onGround = false;
					//this.proxyFSM.SendEvent("HeroCtrl-LeftGround");
					this.SetState(ActorStates.airborne);
					return;
				}
			}
			else
			{
				this.cState.touchingNonSlider = true;
			}
		}
	}

	// Token: 0x06000607 RID: 1543 RVA: 0x00022C7C File Offset: 0x00020E7C
	private void OnCollisionExit2D(Collision2D collision)
	{
		if (this.cState.recoilingLeft || this.cState.recoilingRight)
		{
			this.cState.touchingWall = false;
			this.touchingWallL = false;
			this.touchingWallR = false;
			this.cState.touchingNonSlider = false;
		}
		if (this.touchingWallL && !this.CheckStillTouchingWall(CollisionSide.left, false))
		{
			this.cState.touchingWall = false;
			this.touchingWallL = false;
		}
		if (this.touchingWallR && !this.CheckStillTouchingWall(CollisionSide.right, false))
		{
			this.cState.touchingWall = false;
			this.touchingWallR = false;
		}
		if (this.hero_state != ActorStates.no_input && !this.cState.recoiling && collision.gameObject.layer == 8 && !this.CheckTouchingGround())
		{
			if (!this.cState.jumping && !this.fallTrailGenerated && this.cState.onGround)
			{
				if (this.playerData.GetInt("environmentType") != 6)
				{
					//this.fsm_fallTrail.SendEvent("PLAY");
				}
				this.fallTrailGenerated = true;
			}
			this.cState.onGround = false;
			//this.proxyFSM.SendEvent("HeroCtrl-LeftGround");
			this.SetState(ActorStates.airborne);
			if (this.cState.wasOnGround)
			{
				this.ledgeBufferSteps = this.LEDGE_BUFFER_STEPS;
			}
		}
	}

	// Token: 0x06000608 RID: 1544 RVA: 0x00022DD4 File Offset: 0x00020FD4
	private void SetupGameRefs()
	{
		if (this.cState == null)
		{
			this.cState = new HeroControllerStates();
		}
		this.gm = GameManager.instance;
		this.playerData = PlayerData.instance;
		//this.animCtrl = base.GetComponent<HeroAnimationController>();
		this.rb2d = base.GetComponent<Rigidbody2D>();
		this.col2d = base.GetComponent<Collider2D>();
		this.transform = base.GetComponent<Transform>();
		this.renderer = base.GetComponent<Renderer>();
		//this.audioCtrl = base.GetComponent<HeroAudioController>();
		//this.inputHandler = this.gm.GetComponent<InputHandler>();
		//this.proxyFSM = FSMUtility.LocateFSM(base.gameObject, "ProxyFSM");
		this.audioSource = base.GetComponent<AudioSource>();
		if (!this.footStepsRunAudioSource)
		{
			this.footStepsRunAudioSource = this.transform.Find("Sounds/FootstepsRun").GetComponent<AudioSource>();
		}
		if (!this.footStepsWalkAudioSource)
		{
			this.footStepsWalkAudioSource = this.transform.Find("Sounds/FootstepsWalk").GetComponent<AudioSource>();
		}
		//this.invPulse = base.GetComponent<InvulnerablePulse>();
		//this.spriteFlash = base.GetComponent<SpriteFlash>();
		this.gm.UnloadingLevel += this.OnLevelUnload;
		this.prevGravityScale = this.DEFAULT_GRAVITY;
		this.transition_vel = Vector2.zero;
		this.current_velocity = Vector2.zero;
		this.acceptingInput = true;
		this.positionHistory = new Vector2[2];
	}

	// Token: 0x06000609 RID: 1545 RVA: 0x00003603 File Offset: 0x00001803
	private void SetupPools()
	{
	}

	// Token: 0x0600060A RID: 1546 RVA: 0x00022F30 File Offset: 0x00021130
	private void FilterInput()
	{
		if (this.move_input > 0.3f)
		{
			this.move_input = 1f;
		}
		else if (this.move_input < -0.3f)
		{
			this.move_input = -1f;
		}
		else
		{
			this.move_input = 0f;
		}
		if (this.vertical_input > 0.5f)
		{
			this.vertical_input = 1f;
			return;
		}
		if (this.vertical_input < -0.5f)
		{
			this.vertical_input = -1f;
			return;
		}
		this.vertical_input = 0f;
	}

	// Token: 0x0600060B RID: 1547 RVA: 0x00022FBC File Offset: 0x000211BC
	public Vector3 FindGroundPoint(Vector2 startPoint, bool useExtended = false)
	{
		float num = this.FIND_GROUND_POINT_DISTANCE;
		if (useExtended)
		{
			num = this.FIND_GROUND_POINT_DISTANCE_EXT;
		}
		RaycastHit2D raycastHit2D = Physics2D.Raycast(startPoint, Vector2.down, num, 256);
		if (raycastHit2D.collider == null)
		{
			Debug.LogErrorFormat("FindGroundPoint: Could not find ground point below {0}, check reference position is not too high (more than {1} tiles).", new object[]
			{
				startPoint.ToString(),
				num
			});
		}
		return new Vector3(raycastHit2D.point.x, raycastHit2D.point.y + this.col2d.bounds.extents.y - this.col2d.offset.y + 0.01f, this.transform.position.z);
	}

	// Token: 0x0600060C RID: 1548 RVA: 0x00023084 File Offset: 0x00021284
	private float FindGroundPointY(float x, float y, bool useExtended = false)
	{
		float num = this.FIND_GROUND_POINT_DISTANCE;
		if (useExtended)
		{
			num = this.FIND_GROUND_POINT_DISTANCE_EXT;
		}
		RaycastHit2D raycastHit2D = Physics2D.Raycast(new Vector2(x, y), Vector2.down, num, 256);
		if (raycastHit2D.collider == null)
		{
			Debug.LogErrorFormat("FindGroundPoint: Could not find ground point below ({0},{1}), check reference position is not too high (more than {2} tiles).", new object[]
			{
				x,
				y,
				num
			});
		}
		return raycastHit2D.point.y + this.col2d.bounds.extents.y - this.col2d.offset.y + 0.01f;
	}

	// Token: 0x0600060D RID: 1549 RVA: 0x00023130 File Offset: 0x00021330
	public HeroController()
	{
		this.JUMP_QUEUE_STEPS = 2;
		this.JUMP_RELEASE_QUEUE_STEPS = 2;
		this.DOUBLE_JUMP_QUEUE_STEPS = 10;
		this.ATTACK_QUEUE_STEPS = 5;
		this.DELAY_BEFORE_ENTER = 0.1f;
		this.LOOK_DELAY = 0.85f;
		this.LOOK_ANIM_DELAY = 0.25f;
		this.DEATH_WAIT = 2.85f;
		this.HAZARD_DEATH_CHECK_TIME = 3f;
		this.FLOATING_CHECK_TIME = 0.18f;
		this.NAIL_TERRAIN_CHECK_TIME = 0.12f;
		this.BUMP_VELOCITY = 4f;
		this.BUMP_VELOCITY_DASH = 5f;
		this.LANDING_BUFFER_STEPS = 5;
		this.LEDGE_BUFFER_STEPS = 2;
		this.HEAD_BUMP_STEPS = 3;
		this.MANTIS_CHARM_SCALE = 1.35f;
		this.FIND_GROUND_POINT_DISTANCE = 10f;
		this.FIND_GROUND_POINT_DISTANCE_EXT = 50f;
		this.controller_deadzone = 0.2f;
		this.isHeroInPosition = true;
		this.oldPos = Vector2.zero;
	}

	// Token: 0x0600060F RID: 1551 RVA: 0x0002321F File Offset: 0x0002141F
	public void orig_StartMPDrain(float time)
	{
		this.drainMP = true;
		this.drainMP_timer = 0f;
		this.MP_drained = 0f;
		this.drainMP_time = time;
		this.focusMP_amount = (float)this.playerData.GetInt("focusMP_amount");
	}

	// Token: 0x06000610 RID: 1552 RVA: 0x0002325C File Offset: 0x0002145C
	private void orig_Update()
	{
		if (Time.frameCount % 10 == 0)
		{
			this.Update10();
		}
		this.current_velocity = this.rb2d.velocity;
		this.FallCheck();
		this.FailSafeChecks();
		if (this.hero_state == ActorStates.running && !this.cState.dashing && !this.cState.backDashing && !this.controlReqlinquished)
		{
			/*if (this.cState.inWalkZone)
			{
				this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN);
				this.audioCtrl.PlaySound(HeroSounds.FOOTSTEPS_WALK);
			}
			else
			{
				this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK);
				this.audioCtrl.PlaySound(HeroSounds.FOOTSTEPS_RUN);
			}*/
			if (this.runMsgSent && this.rb2d.velocity.x > -0.1f && this.rb2d.velocity.x < 0.1f)
			{
				//this.runEffect.GetComponent<PlayMakerFSM>().SendEvent("RUN STOP");
				this.runEffect.transform.SetParent(null, true);
				this.runMsgSent = false;
			}
			if (!this.runMsgSent && (this.rb2d.velocity.x < -0.1f || this.rb2d.velocity.x > 0.1f))
			{
				//this.runEffect = this.runEffectPrefab.Spawn();
				/*runEffect = GameObject.Instantiate(runEffectPrefab);
				this.runEffect.transform.SetParent(base.gameObject.transform, false);
				this.runMsgSent = true;*/
			}
		}
		else
		{
			/*this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN);
			this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK);*/
			if (this.runMsgSent)
			{
				//this.runEffect.GetComponent<PlayMakerFSM>().SendEvent("RUN STOP");
				this.runEffect.transform.SetParent(null, true);
				this.runMsgSent = false;
			}
		}
		if (this.hero_state == ActorStates.dash_landing)
		{
			this.dashLandingTimer += Time.deltaTime;
			if (this.dashLandingTimer > this.DOWN_DASH_TIME)
			{
				this.BackOnGround();
			}
		}
		if (this.hero_state == ActorStates.hard_landing)
		{
			this.hardLandingTimer += Time.deltaTime;
			if (this.hardLandingTimer > this.HARD_LANDING_TIME)
			{
				this.SetState(ActorStates.grounded);
				this.BackOnGround();
			}
		}
		else if (this.hero_state == ActorStates.no_input)
		{
			if (this.cState.recoiling)
			{
				if ((!this.playerData.GetBool("equippedCharm_4") && this.recoilTimer < this.RECOIL_DURATION) || (this.playerData.GetBool("equippedCharm_4") && this.recoilTimer < this.RECOIL_DURATION_STAL))
				{
					this.recoilTimer += Time.deltaTime;
				}
				else
				{
					this.CancelDamageRecoil();
					if ((this.prev_hero_state == ActorStates.idle || this.prev_hero_state == ActorStates.running) && !this.CheckTouchingGround())
					{
						this.cState.onGround = false;
						this.SetState(ActorStates.airborne);
					}
					else
					{
						this.SetState(ActorStates.previous);
					}
					//this.fsm_thornCounter.SendEvent("THORN COUNTER");
				}
			}
		}
		else if (this.hero_state != ActorStates.no_input)
		{
			this.LookForInput();
			if (this.cState.recoiling)
			{
				this.cState.recoiling = false;
				this.AffectedByGravity(true);
			}
			if (this.cState.attacking && !this.cState.dashing)
			{
				this.attack_time += Time.deltaTime;
				if (this.attack_time >= this.attackDuration)
				{
					this.ResetAttacks();
					//this.animCtrl.StopAttack();
				}
			}
			if (this.cState.bouncing)
			{
				if (this.bounceTimer < this.BOUNCE_TIME)
				{
					this.bounceTimer += Time.deltaTime;
				}
				else
				{
					this.CancelBounce();
					this.rb2d.velocity = new Vector2(this.rb2d.velocity.x, 0f);
				}
			}
			if (this.cState.shroomBouncing && this.current_velocity.y <= 0f)
			{
				this.cState.shroomBouncing = false;
			}
			if (this.hero_state == ActorStates.idle)
			{
				if (!this.controlReqlinquished && !this.gm.isPaused)
				{
					if (UpPressed/* || this.inputHandler.inputActions.rs_up.IsPressed*/)
					{
						this.cState.lookingDown = false;
						this.cState.lookingDownAnim = false;
						if (this.lookDelayTimer >= this.LOOK_DELAY || (/*this.inputHandler.inputActions.rs_up.IsPressed*/false && !this.cState.jumping && !this.cState.dashing))
						{
							this.cState.lookingUp = true;
						}
						else
						{
							this.lookDelayTimer += Time.deltaTime;
						}
						if (this.lookDelayTimer >= this.LOOK_ANIM_DELAY || /*this.inputHandler.inputActions.rs_up.IsPressed*/false)
						{
							this.cState.lookingUpAnim = true;
						}
						else
						{
							this.cState.lookingUpAnim = false;
						}
					}
					else if (DownPressed || /*this.inputHandler.inputActions.rs_down.IsPressed*/false)
					{
						this.cState.lookingUp = false;
						this.cState.lookingUpAnim = false;
						if (this.lookDelayTimer >= this.LOOK_DELAY || (/*this.inputHandler.inputActions.rs_down.IsPressed*/false && !this.cState.jumping && !this.cState.dashing))
						{
							this.cState.lookingDown = true;
						}
						else
						{
							this.lookDelayTimer += Time.deltaTime;
						}
						if (this.lookDelayTimer >= this.LOOK_ANIM_DELAY || /*this.inputHandler.inputActions.rs_down.IsPressed*/false)
						{
							this.cState.lookingDownAnim = true;
						}
						else
						{
							this.cState.lookingDownAnim = false;
						}
					}
					else
					{
						this.ResetLook();
					}
				}
				this.runPuffTimer = 0f;
			}
		}
		this.LookForQueueInput();
		if (this.drainMP)
		{
			this.drainMP_timer += Time.deltaTime;
			this.drainMP_seconds += Time.deltaTime;
			while (this.drainMP_timer >= this.drainMP_time)
			{
				this.MP_drained += 1f;
				this.drainMP_timer -= this.drainMP_time;
				this.TakeMP(1);
				//this.gm.soulOrb_fsm.SendEvent("MP DRAIN");
				if (this.MP_drained == this.focusMP_amount)
				{
					this.MP_drained -= this.drainMP_time;
					//this.proxyFSM.SendEvent("HeroCtrl-FocusCompleted");
				}
			}
		}
		if (this.cState.wallSliding)
		{
			if (this.airDashed)
			{
				this.airDashed = false;
			}
			if (this.doubleJumped)
			{
				this.doubleJumped = false;
			}
			if (this.cState.onGround)
			{
				this.FlipSprite();
				this.CancelWallsliding();
			}
			if (!this.cState.touchingWall)
			{
				this.FlipSprite();
				this.CancelWallsliding();
			}
			if (!this.CanWallSlide())
			{
				this.CancelWallsliding();
			}
			if (!this.playedMantisClawClip)
			{
				this.audioSource.PlayOneShot(this.mantisClawClip, 1f);
				this.playedMantisClawClip = true;
			}
			if (!this.playingWallslideClip)
			{
				if (this.wallslideClipTimer <= this.WALLSLIDE_CLIP_DELAY)
				{
					this.wallslideClipTimer += Time.deltaTime;
				}
				else
				{
					this.wallslideClipTimer = 0f;
					//this.audioCtrl.PlaySound(HeroSounds.WALLSLIDE);
					this.playingWallslideClip = true;
				}
			}
		}
		else if (this.playedMantisClawClip)
		{
			this.playedMantisClawClip = false;
		}
		if (!this.cState.wallSliding && this.playingWallslideClip)
		{
			//this.audioCtrl.StopSound(HeroSounds.WALLSLIDE);
			this.playingWallslideClip = false;
		}
		if (!this.cState.wallSliding && this.wallslideClipTimer > 0f)
		{
			this.wallslideClipTimer = 0f;
		}
		if (this.wallSlashing && !this.cState.wallSliding)
		{
			this.CancelAttack();
		}
		if (this.attack_cooldown > 0f)
		{
			this.attack_cooldown -= Time.deltaTime;
		}
		if (this.dashCooldownTimer > 0f)
		{
			this.dashCooldownTimer -= Time.deltaTime;
		}
		if (this.shadowDashTimer > 0f)
		{
			this.shadowDashTimer -= Time.deltaTime;
			if (this.shadowDashTimer <= 0f)
			{
				//this.spriteFlash.FlashShadowRecharge();
				Component flasher = GetComponent("WeaverCore.Components.SpriteFlasher");
				if (flasher != null)
				{
					flasher.SendMessage("FlashShadowRecharge");
					//flasher.FlashShadowRecharge();
				}
			}
		}
		this.preventCastByDialogueEndTimer -= Time.deltaTime;
		if (!this.gm.isPaused)
		{
			/*if (AttackPressed && this.CanNailCharge())
			{
				this.cState.nailCharging = true;
				this.nailChargeTimer += Time.deltaTime;
			}
			else if (this.cState.nailCharging || this.nailChargeTimer != 0f)
			{
				this.artChargeEffect.SetActive(false);
				this.cState.nailCharging = false;
				//this.audioCtrl.StopSound(HeroSounds.NAIL_ART_CHARGE);
			}
			if (this.cState.nailCharging && this.nailChargeTimer > 0.5f && !this.artChargeEffect.activeSelf && this.nailChargeTimer < this.nailChargeTime)
			{
				this.artChargeEffect.SetActive(true);
				//this.audioCtrl.PlaySound(HeroSounds.NAIL_ART_CHARGE);
			}
			if (this.artChargeEffect.activeSelf && (!this.cState.nailCharging || this.nailChargeTimer > this.nailChargeTime))
			{
				this.artChargeEffect.SetActive(false);
				//this.audioCtrl.StopSound(HeroSounds.NAIL_ART_CHARGE);
			}
			if (!this.artChargedEffect.activeSelf && this.nailChargeTimer >= this.nailChargeTime)
			{
				this.artChargedEffect.SetActive(true);
				this.artChargedFlash.SetActive(true);
				//this.artChargedEffectAnim.PlayFromFrame(0);
				//GameCameras.instance.cameraShakeFSM.SendEvent("EnemyKillShake");
				this.audioSource.PlayOneShot(this.nailArtChargeComplete, 1f);
				//this.audioCtrl.PlaySound(HeroSounds.NAIL_ART_READY);
				this.cState.nailCharging = true;
			}
			if (this.artChargedEffect.activeSelf && (this.nailChargeTimer < this.nailChargeTime || !this.cState.nailCharging))
			{
				this.artChargedEffect.SetActive(false);
				//this.audioCtrl.StopSound(HeroSounds.NAIL_ART_READY);
			}*/
		}
		if (this.gm.isPaused && !AttackPressed)
		{
			this.cState.nailCharging = false;
			this.nailChargeTimer = 0f;
		}
		if (this.cState.swimming && !this.CanSwim())
		{
			this.cState.swimming = false;
		}
		if (this.parryInvulnTimer > 0f)
		{
			this.parryInvulnTimer -= Time.deltaTime;
		}
	}

	// Token: 0x06000611 RID: 1553 RVA: 0x00023D68 File Offset: 0x00021F68
	private Vector2 OrigDashVector()
	{
		float num;
		if (this.playerData.GetBool("equippedCharm_16") && this.cState.shadowDashing)
		{
			num = this.DASH_SPEED_SHARP;
		}
		else
		{
			num = this.DASH_SPEED;
		}
		Vector2 result;
		if (this.dashingDown)
		{
			result = new Vector2(0f, -num);
		}
		else if (this.cState.facingRight)
		{
			if (this.CheckForBump(CollisionSide.right))
			{
				result = new Vector2(num, (!this.cState.onGround) ? 5f : 4f);
			}
			else
			{
				result = new Vector2(num, 0f);
			}
		}
		else if (this.CheckForBump(CollisionSide.left))
		{
			result = new Vector2(-num, (!this.cState.onGround) ? 5f : 4f);
		}
		else
		{
			result = new Vector2(-num, 0f);
		}
		return result;
	}

	// Token: 0x06000612 RID: 1554 RVA: 0x00023E40 File Offset: 0x00022040
	private void orig_Dash()
	{
		this.AffectedByGravity(false);
		this.ResetHardLandingTimer();
		if (this.dash_timer > this.DASH_TIME)
		{
			this.FinishedDashing();
			return;
		}
		float num;
		if (this.playerData.GetBool("equippedCharm_16") && this.cState.shadowDashing)
		{
			num = this.DASH_SPEED_SHARP;
		}
		else
		{
			num = this.DASH_SPEED;
		}
		if (this.dashingDown)
		{
			this.rb2d.velocity = new Vector2(0f, -num);
		}
		else if (this.cState.facingRight)
		{
			if (this.CheckForBump(CollisionSide.right))
			{
				this.rb2d.velocity = new Vector2(num, this.cState.onGround ? this.BUMP_VELOCITY : this.BUMP_VELOCITY_DASH);
			}
			else
			{
				this.rb2d.velocity = new Vector2(num, 0f);
			}
		}
		else if (this.CheckForBump(CollisionSide.left))
		{
			this.rb2d.velocity = new Vector2(-num, this.cState.onGround ? this.BUMP_VELOCITY : this.BUMP_VELOCITY_DASH);
		}
		else
		{
			this.rb2d.velocity = new Vector2(-num, 0f);
		}
		this.dash_timer += Time.deltaTime;
	}

	// Token: 0x06000613 RID: 1555 RVA: 0x00023F80 File Offset: 0x00022180
	public void orig_CharmUpdate()
	{
		if (this.playerData.GetBool("equippedCharm_26"))
		{
			this.nailChargeTime = this.NAIL_CHARGE_TIME_CHARM;
		}
		else
		{
			this.nailChargeTime = this.NAIL_CHARGE_TIME_DEFAULT;
		}
		if (this.playerData.GetBool("equippedCharm_23") && !this.playerData.GetBool("brokenCharm_23"))
		{
			this.playerData.SetIntSwappedArgs(this.playerData.GetInt("maxHealthBase") + 2, "maxHealth");
			this.MaxHealth();
		}
		else
		{
			this.playerData.SetIntSwappedArgs(this.playerData.GetInt("maxHealthBase"), "maxHealth");
			this.MaxHealth();
		}
		if (this.playerData.GetBool("equippedCharm_27"))
		{
			this.playerData.SetIntSwappedArgs((int)((float)this.playerData.GetInt("maxHealth") * 1.4f), "joniHealthBlue");
			this.playerData.SetIntSwappedArgs(1, "maxHealth");
			this.MaxHealth();
			this.joniBeam = true;
		}
		else
		{
			this.playerData.SetIntSwappedArgs(0, "joniHealthBlue");
		}
		if (this.playerData.GetBool("equippedCharm_40") && this.playerData.GetInt("grimmChildLevel") == 5)
		{
			this.carefreeShieldEquipped = true;
		}
		else
		{
			this.carefreeShieldEquipped = false;
		}
		//this.playerData.UpdateBlueHealth();
	}

	// Token: 0x06000614 RID: 1556 RVA: 0x000240D8 File Offset: 0x000222D8
	private void orig_DoAttack()
	{
		this.ResetLook();
		this.cState.recoiling = false;
		if (this.playerData.GetBool("equippedCharm_32"))
		{
			this.attack_cooldown = this.ATTACK_COOLDOWN_TIME_CH;
		}
		else
		{
			this.attack_cooldown = this.ATTACK_COOLDOWN_TIME;
		}
		if (this.vertical_input > Mathf.Epsilon)
		{
			this.Attack(AttackDirection.upward);
			base.StartCoroutine(this.CheckForTerrainThunk(AttackDirection.upward));
			return;
		}
		if (this.vertical_input >= -Mathf.Epsilon)
		{
			this.Attack(AttackDirection.normal);
			base.StartCoroutine(this.CheckForTerrainThunk(AttackDirection.normal));
			return;
		}
		if (this.hero_state != ActorStates.idle && this.hero_state != ActorStates.running)
		{
			this.Attack(AttackDirection.downward);
			base.StartCoroutine(this.CheckForTerrainThunk(AttackDirection.downward));
			return;
		}
		this.Attack(AttackDirection.normal);
		base.StartCoroutine(this.CheckForTerrainThunk(AttackDirection.normal));
	}

	// Token: 0x0400051A RID: 1306
	private bool verboseMode;

	// Token: 0x0400051B RID: 1307
	public HeroType heroType;

	// Token: 0x0400051C RID: 1308
	public float RUN_SPEED;

	// Token: 0x0400051D RID: 1309
	public float RUN_SPEED_CH;

	// Token: 0x0400051E RID: 1310
	public float RUN_SPEED_CH_COMBO;

	// Token: 0x0400051F RID: 1311
	public float WALK_SPEED;

	// Token: 0x04000520 RID: 1312
	public float UNDERWATER_SPEED;

	// Token: 0x04000521 RID: 1313
	public float JUMP_SPEED;

	// Token: 0x04000522 RID: 1314
	public float JUMP_SPEED_UNDERWATER;

	// Token: 0x04000523 RID: 1315
	public float MIN_JUMP_SPEED;

	// Token: 0x04000524 RID: 1316
	public int JUMP_STEPS;

	// Token: 0x04000525 RID: 1317
	public int JUMP_STEPS_MIN;

	// Token: 0x04000526 RID: 1318
	public int JUMP_TIME;

	// Token: 0x04000527 RID: 1319
	public int DOUBLE_JUMP_STEPS;

	// Token: 0x04000528 RID: 1320
	public int WJLOCK_STEPS_SHORT;

	// Token: 0x04000529 RID: 1321
	public int WJLOCK_STEPS_LONG;

	// Token: 0x0400052A RID: 1322
	public float WJ_KICKOFF_SPEED;

	// Token: 0x0400052B RID: 1323
	public int WALL_STICKY_STEPS;

	// Token: 0x0400052C RID: 1324
	public float DASH_SPEED;

	// Token: 0x0400052D RID: 1325
	public float DASH_SPEED_SHARP;

	// Token: 0x0400052E RID: 1326
	public float DASH_TIME;

	// Token: 0x0400052F RID: 1327
	public int DASH_QUEUE_STEPS;

	// Token: 0x04000530 RID: 1328
	public float BACK_DASH_SPEED;

	// Token: 0x04000531 RID: 1329
	public float BACK_DASH_TIME;

	// Token: 0x04000532 RID: 1330
	public float SHADOW_DASH_SPEED;

	// Token: 0x04000533 RID: 1331
	public float SHADOW_DASH_TIME;

	// Token: 0x04000534 RID: 1332
	public float SHADOW_DASH_COOLDOWN;

	// Token: 0x04000535 RID: 1333
	public float SUPER_DASH_SPEED;

	// Token: 0x04000536 RID: 1334
	public float DASH_COOLDOWN;

	// Token: 0x04000537 RID: 1335
	public float DASH_COOLDOWN_CH;

	// Token: 0x04000538 RID: 1336
	public float BACKDASH_COOLDOWN;

	// Token: 0x04000539 RID: 1337
	public float WALLSLIDE_SPEED;

	// Token: 0x0400053A RID: 1338
	public float WALLSLIDE_DECEL;

	// Token: 0x0400053B RID: 1339
	public float NAIL_CHARGE_TIME_DEFAULT;

	// Token: 0x0400053C RID: 1340
	public float NAIL_CHARGE_TIME_CHARM;

	// Token: 0x0400053D RID: 1341
	public float CYCLONE_HORIZONTAL_SPEED;

	// Token: 0x0400053E RID: 1342
	public float SWIM_ACCEL;

	// Token: 0x0400053F RID: 1343
	public float SWIM_MAX_SPEED;

	// Token: 0x04000540 RID: 1344
	public float TIME_TO_ENTER_SCENE_BOT;

	// Token: 0x04000541 RID: 1345
	public float TIME_TO_ENTER_SCENE_HOR;

	// Token: 0x04000542 RID: 1346
	public float SPEED_TO_ENTER_SCENE_HOR;

	// Token: 0x04000543 RID: 1347
	public float SPEED_TO_ENTER_SCENE_UP;

	// Token: 0x04000544 RID: 1348
	public float SPEED_TO_ENTER_SCENE_DOWN;

	// Token: 0x04000545 RID: 1349
	public float DEFAULT_GRAVITY;

	// Token: 0x04000546 RID: 1350
	public float UNDERWATER_GRAVITY;

	// Token: 0x04000547 RID: 1351
	public float ATTACK_DURATION;

	// Token: 0x04000548 RID: 1352
	public float ATTACK_DURATION_CH;

	// Token: 0x04000549 RID: 1353
	public float ALT_ATTACK_RESET;

	// Token: 0x0400054A RID: 1354
	public float ATTACK_RECOVERY_TIME;

	// Token: 0x0400054B RID: 1355
	public float ATTACK_COOLDOWN_TIME;

	// Token: 0x0400054C RID: 1356
	public float ATTACK_COOLDOWN_TIME_CH;

	// Token: 0x0400054D RID: 1357
	public float BOUNCE_TIME;

	// Token: 0x0400054E RID: 1358
	public float BOUNCE_SHROOM_TIME;

	// Token: 0x0400054F RID: 1359
	public float BOUNCE_VELOCITY;

	// Token: 0x04000550 RID: 1360
	public float SHROOM_BOUNCE_VELOCITY;

	// Token: 0x04000551 RID: 1361
	public float RECOIL_HOR_TIME;

	// Token: 0x04000552 RID: 1362
	public float RECOIL_HOR_VELOCITY;

	// Token: 0x04000553 RID: 1363
	public float RECOIL_HOR_VELOCITY_LONG;

	// Token: 0x04000554 RID: 1364
	public float RECOIL_HOR_STEPS;

	// Token: 0x04000555 RID: 1365
	public float RECOIL_DOWN_VELOCITY;

	// Token: 0x04000556 RID: 1366
	public float RUN_PUFF_TIME;

	// Token: 0x04000557 RID: 1367
	public float BIG_FALL_TIME;

	// Token: 0x04000558 RID: 1368
	public float HARD_LANDING_TIME;

	// Token: 0x04000559 RID: 1369
	public float DOWN_DASH_TIME;

	// Token: 0x0400055A RID: 1370
	public float MAX_FALL_VELOCITY;

	// Token: 0x0400055B RID: 1371
	public float MAX_FALL_VELOCITY_UNDERWATER;

	// Token: 0x0400055C RID: 1372
	public float RECOIL_DURATION;

	// Token: 0x0400055D RID: 1373
	public float RECOIL_DURATION_STAL;

	// Token: 0x0400055E RID: 1374
	public float RECOIL_VELOCITY;

	// Token: 0x0400055F RID: 1375
	public float DAMAGE_FREEZE_DOWN;

	// Token: 0x04000560 RID: 1376
	public float DAMAGE_FREEZE_WAIT;

	// Token: 0x04000561 RID: 1377
	public float DAMAGE_FREEZE_UP;

	// Token: 0x04000562 RID: 1378
	public float INVUL_TIME;

	// Token: 0x04000563 RID: 1379
	public float INVUL_TIME_STAL;

	// Token: 0x04000564 RID: 1380
	public float INVUL_TIME_PARRY;

	// Token: 0x04000565 RID: 1381
	public float INVUL_TIME_QUAKE;

	// Token: 0x04000566 RID: 1382
	public float INVUL_TIME_CYCLONE;

	// Token: 0x04000567 RID: 1383
	public float CAST_TIME;

	// Token: 0x04000568 RID: 1384
	public float CAST_RECOIL_TIME;

	// Token: 0x04000569 RID: 1385
	public float CAST_RECOIL_VELOCITY;

	// Token: 0x0400056A RID: 1386
	public float WALLSLIDE_CLIP_DELAY;

	// Token: 0x0400056B RID: 1387
	public int GRUB_SOUL_MP;

	// Token: 0x0400056C RID: 1388
	public int GRUB_SOUL_MP_COMBO;

	// Token: 0x0400056D RID: 1389
	private int JUMP_QUEUE_STEPS;

	// Token: 0x0400056E RID: 1390
	private int JUMP_RELEASE_QUEUE_STEPS;

	// Token: 0x0400056F RID: 1391
	private int DOUBLE_JUMP_QUEUE_STEPS;

	// Token: 0x04000570 RID: 1392
	private int ATTACK_QUEUE_STEPS;

	// Token: 0x04000571 RID: 1393
	private float DELAY_BEFORE_ENTER;

	// Token: 0x04000572 RID: 1394
	private float LOOK_DELAY;

	// Token: 0x04000573 RID: 1395
	private float LOOK_ANIM_DELAY;

	// Token: 0x04000574 RID: 1396
	private float DEATH_WAIT;

	// Token: 0x04000575 RID: 1397
	private float HAZARD_DEATH_CHECK_TIME;

	// Token: 0x04000576 RID: 1398
	private float FLOATING_CHECK_TIME;

	// Token: 0x04000577 RID: 1399
	private float NAIL_TERRAIN_CHECK_TIME;

	// Token: 0x04000578 RID: 1400
	private float BUMP_VELOCITY;

	// Token: 0x04000579 RID: 1401
	private float BUMP_VELOCITY_DASH;

	// Token: 0x0400057A RID: 1402
	private int LANDING_BUFFER_STEPS;

	// Token: 0x0400057B RID: 1403
	private int LEDGE_BUFFER_STEPS;

	// Token: 0x0400057C RID: 1404
	private int HEAD_BUMP_STEPS;

	// Token: 0x0400057D RID: 1405
	private float MANTIS_CHARM_SCALE;

	// Token: 0x0400057E RID: 1406
	private float FIND_GROUND_POINT_DISTANCE;

	// Token: 0x0400057F RID: 1407
	private float FIND_GROUND_POINT_DISTANCE_EXT;

	// Token: 0x04000580 RID: 1408
	public ActorStates hero_state;

	// Token: 0x04000581 RID: 1409
	public ActorStates prev_hero_state;

	// Token: 0x04000582 RID: 1410
	public HeroTransitionState transitionState;

	// Token: 0x04000583 RID: 1411
	public DamageMode damageMode;

	// Token: 0x04000584 RID: 1412
	public float move_input;

	// Token: 0x04000585 RID: 1413
	public float vertical_input;

	// Token: 0x04000586 RID: 1414
	public float controller_deadzone;

	// Token: 0x04000587 RID: 1415
	public Vector2 current_velocity;

	// Token: 0x04000588 RID: 1416
	private bool isGameplayScene;

	// Token: 0x04000589 RID: 1417
	public bool isEnteringFirstLevel;

	// Token: 0x0400058A RID: 1418
	public Vector2 slashOffset;

	// Token: 0x0400058B RID: 1419
	public Vector2 upSlashOffset;

	// Token: 0x0400058C RID: 1420
	public Vector2 downwardSlashOffset;

	// Token: 0x0400058D RID: 1421
	public Vector2 spell1Offset;

	// Token: 0x0400058E RID: 1422
	private int jump_steps;

	// Token: 0x0400058F RID: 1423
	private int jumped_steps;

	// Token: 0x04000590 RID: 1424
	private int doubleJump_steps;

	// Token: 0x04000591 RID: 1425
	private float dash_timer;

	// Token: 0x04000592 RID: 1426
	private float back_dash_timer;

	// Token: 0x04000593 RID: 1427
	private float shadow_dash_timer;

	// Token: 0x04000594 RID: 1428
	private float attack_time;

	// Token: 0x04000595 RID: 1429
	private float attack_cooldown;

	// Token: 0x04000596 RID: 1430
	private Vector2 transition_vel;

	// Token: 0x04000597 RID: 1431
	private float altAttackTime;

	// Token: 0x04000598 RID: 1432
	private float lookDelayTimer;

	// Token: 0x04000599 RID: 1433
	private float bounceTimer;

	// Token: 0x0400059A RID: 1434
	private float recoilHorizontalTimer;

	// Token: 0x0400059B RID: 1435
	private float runPuffTimer;

	// Token: 0x0400059D RID: 1437
	private float hardLandingTimer;

	// Token: 0x0400059E RID: 1438
	private float dashLandingTimer;

	// Token: 0x0400059F RID: 1439
	private float recoilTimer;

	// Token: 0x040005A0 RID: 1440
	private int recoilSteps;

	// Token: 0x040005A1 RID: 1441
	private int landingBufferSteps;

	// Token: 0x040005A2 RID: 1442
	private int dashQueueSteps;

	// Token: 0x040005A3 RID: 1443
	private bool dashQueuing;

	// Token: 0x040005A4 RID: 1444
	private float shadowDashTimer;

	// Token: 0x040005A5 RID: 1445
	private float dashCooldownTimer;

	// Token: 0x040005A6 RID: 1446
	private float nailChargeTimer;

	// Token: 0x040005A7 RID: 1447
	private int wallLockSteps;

	// Token: 0x040005A8 RID: 1448
	private float wallslideClipTimer;

	// Token: 0x040005A9 RID: 1449
	private float hardLandFailSafeTimer;

	// Token: 0x040005AA RID: 1450
	private float hazardDeathTimer;

	// Token: 0x040005AB RID: 1451
	private float floatingBufferTimer;

	// Token: 0x040005AC RID: 1452
	private float attackDuration;

	// Token: 0x040005AD RID: 1453
	public float parryInvulnTimer;

	// Token: 0x040005AE RID: 1454
	[Space(6f)]
	[Header("Slash Prefabs")]
	public GameObject slashPrefab;

	// Token: 0x040005AF RID: 1455
	public GameObject slashAltPrefab;

	// Token: 0x040005B0 RID: 1456
	public GameObject upSlashPrefab;

	// Token: 0x040005B1 RID: 1457
	public GameObject downSlashPrefab;

	// Token: 0x040005B2 RID: 1458
	public GameObject wallSlashPrefab;

	// Token: 0x040005B3 RID: 1459
	/*public NailSlash normalSlash;

	// Token: 0x040005B4 RID: 1460
	public NailSlash alternateSlash;

	// Token: 0x040005B5 RID: 1461
	public NailSlash upSlash;

	// Token: 0x040005B6 RID: 1462
	public NailSlash downSlash;

	// Token: 0x040005B7 RID: 1463
	public NailSlash wallSlash;

	// Token: 0x040005B8 RID: 1464
	public PlayMakerFSM normalSlashFsm;

	// Token: 0x040005B9 RID: 1465
	public PlayMakerFSM alternateSlashFsm;

	// Token: 0x040005BA RID: 1466
	public PlayMakerFSM upSlashFsm;

	// Token: 0x040005BB RID: 1467
	public PlayMakerFSM downSlashFsm;

	// Token: 0x040005BC RID: 1468
	public PlayMakerFSM wallSlashFsm;*/

	// Token: 0x040005BD RID: 1469
	[Space(6f)]
	[Header("Effect Prefabs")]
	public GameObject nailTerrainImpactEffectPrefab;

	// Token: 0x040005BE RID: 1470
	//public GameObject spell1Prefab;

	// Token: 0x040005BF RID: 1471
	//public GameObject takeHitPrefab;

	// Token: 0x040005C0 RID: 1472
	//public GameObject takeHitDoublePrefab;

	// Token: 0x040005C1 RID: 1473
	//public GameObject softLandingEffectPrefab;

	// Token: 0x040005C2 RID: 1474
	//public GameObject hardLandingEffectPrefab;

	// Token: 0x040005C3 RID: 1475
	//public GameObject runEffectPrefab;

	// Token: 0x040005C4 RID: 1476
	//public GameObject backDashPrefab;

	// Token: 0x040005C5 RID: 1477
	//public GameObject jumpEffectPrefab;

	// Token: 0x040005C6 RID: 1478
	//public GameObject jumpTrailPrefab;

	// Token: 0x040005C7 RID: 1479
	//public GameObject fallEffectPrefab;

	// Token: 0x040005C8 RID: 1480
	//public ParticleSystem wallslideDustPrefab;

	// Token: 0x040005C9 RID: 1481
	//public GameObject artChargeEffect;

	// Token: 0x040005CA RID: 1482
	//public GameObject artChargedEffect;

	// Token: 0x040005CB RID: 1483
	//public GameObject artChargedFlash;

	// Token: 0x040005CC RID: 1484
	//public tk2dSpriteAnimator artChargedEffectAnim;

	// Token: 0x040005CD RID: 1485
	//public GameObject shadowdashBurstPrefab;

	// Token: 0x040005CE RID: 1486
	//public GameObject shadowdashDownBurstPrefab;

	// Token: 0x040005CF RID: 1487
	//public GameObject dashParticlesPrefab;

	// Token: 0x040005D0 RID: 1488
	//public GameObject shadowdashParticlesPrefab;

	// Token: 0x040005D1 RID: 1489
	//public GameObject shadowRingPrefab;

	// Token: 0x040005D2 RID: 1490
	//public GameObject shadowRechargePrefab;

	// Token: 0x040005D3 RID: 1491
	//public GameObject dJumpWingsPrefab;

	// Token: 0x040005D4 RID: 1492
	//public GameObject dJumpFlashPrefab;

	// Token: 0x040005D5 RID: 1493
	//public ParticleSystem dJumpFeathers;

	// Token: 0x040005D6 RID: 1494
	//public GameObject wallPuffPrefab;

	// Token: 0x040005D7 RID: 1495
	//public GameObject sharpShadowPrefab;

	// Token: 0x040005D8 RID: 1496
	/*public GameObject grubberFlyBeamPrefabL;

	// Token: 0x040005D9 RID: 1497
	public GameObject grubberFlyBeamPrefabR;

	// Token: 0x040005DA RID: 1498
	public GameObject grubberFlyBeamPrefabU;

	// Token: 0x040005DB RID: 1499
	public GameObject grubberFlyBeamPrefabD;

	// Token: 0x040005DC RID: 1500
	public GameObject grubberFlyBeamPrefabL_fury;

	// Token: 0x040005DD RID: 1501
	public GameObject grubberFlyBeamPrefabR_fury;

	// Token: 0x040005DE RID: 1502
	public GameObject grubberFlyBeamPrefabU_fury;

	// Token: 0x040005DF RID: 1503
	public GameObject grubberFlyBeamPrefabD_fury;*/

	// Token: 0x040005E0 RID: 1504
	//public GameObject carefreeShield;

	// Token: 0x040005E1 RID: 1505
	[Space(6f)]
	[Header("Hero Death")]
	public GameObject corpsePrefab;

	// Token: 0x040005E2 RID: 1506
	public GameObject spikeDeathPrefab;

	// Token: 0x040005E3 RID: 1507
	public GameObject acidDeathPrefab;

	// Token: 0x040005E4 RID: 1508
	public GameObject lavaDeathPrefab;

	// Token: 0x040005E5 RID: 1509
	public GameObject heroDeathPrefab;

	// Token: 0x040005E6 RID: 1510
	[Space(6f)]
	[Header("Hero Other")]
	public GameObject cutscenePrefab;

	// Token: 0x040005E7 RID: 1511
	private GameManager gm;

	// Token: 0x040005E8 RID: 1512
	private Rigidbody2D rb2d;

	// Token: 0x040005E9 RID: 1513
	private Collider2D col2d;

	// Token: 0x040005EA RID: 1514
	private Renderer renderer;

	// Token: 0x040005EB RID: 1515
	private new Transform transform;

	// Token: 0x040005EC RID: 1516
	//private HeroAnimationController animCtrl;

	// Token: 0x040005ED RID: 1517
	public HeroControllerStates cState;

	// Token: 0x040005EE RID: 1518
	public PlayerData playerData;

	// Token: 0x040005EF RID: 1519
	//private HeroAudioController audioCtrl;

	// Token: 0x040005F0 RID: 1520
	private AudioSource audioSource;

	// Token: 0x040005F1 RID: 1521
	//[HideInInspector]
	//public UIManager ui;

	// Token: 0x040005F2 RID: 1522
	//private InputHandler inputHandler;

	// Token: 0x040005F5 RID: 1525
	//public PlayMakerFSM damageEffectFSM;

	// Token: 0x040005F6 RID: 1526
	private ParticleSystem dashParticleSystem;

	// Token: 0x040005F7 RID: 1527
	//private InvulnerablePulse invPulse;

	// Token: 0x040005F8 RID: 1528
	//private SpriteFlash spriteFlash;

	// Token: 0x040005F9 RID: 1529
	public AudioSource footStepsRunAudioSource;

	// Token: 0x040005FA RID: 1530
	public AudioSource footStepsWalkAudioSource;

	// Token: 0x040005FB RID: 1531
	private float prevGravityScale;

	// Token: 0x040005FC RID: 1532
	private Vector2 recoilVector;

	// Token: 0x040005FD RID: 1533
	private Vector2 lastInputState;

	// Token: 0x040005FE RID: 1534
	public GatePosition gatePosition;

	// Token: 0x04000600 RID: 1536
	private bool runMsgSent;

	// Token: 0x04000601 RID: 1537
	private bool hardLanded;

	// Token: 0x04000602 RID: 1538
	private bool fallRumble;

	// Token: 0x04000603 RID: 1539
	public bool acceptingInput;

	// Token: 0x04000604 RID: 1540
	private bool fallTrailGenerated;

	// Token: 0x04000605 RID: 1541
	private bool drainMP;

	// Token: 0x04000606 RID: 1542
	private float drainMP_timer;

	// Token: 0x04000607 RID: 1543
	private float drainMP_time;

	// Token: 0x04000608 RID: 1544
	private float MP_drained;

	// Token: 0x04000609 RID: 1545
	private float drainMP_seconds;

	// Token: 0x0400060A RID: 1546
	private float focusMP_amount;

	// Token: 0x0400060B RID: 1547
	private float dashBumpCorrection;

	// Token: 0x0400060C RID: 1548
	public bool controlReqlinquished;

	// Token: 0x0400060D RID: 1549
	public bool enterWithoutInput;

	// Token: 0x0400060E RID: 1550
	public bool lookingUpAnim;

	// Token: 0x0400060F RID: 1551
	public bool lookingDownAnim;

	// Token: 0x04000610 RID: 1552
	public bool carefreeShieldEquipped;

	// Token: 0x04000611 RID: 1553
	private int hitsSinceShielded;

	// Token: 0x04000612 RID: 1554
	//private EndBeta endBeta;

	// Token: 0x04000613 RID: 1555
	private int jumpQueueSteps;

	// Token: 0x04000614 RID: 1556
	private bool jumpQueuing;

	// Token: 0x04000615 RID: 1557
	private int doubleJumpQueueSteps;

	// Token: 0x04000616 RID: 1558
	private bool doubleJumpQueuing;

	// Token: 0x04000617 RID: 1559
	private int jumpReleaseQueueSteps;

	// Token: 0x04000618 RID: 1560
	private bool jumpReleaseQueuing;

	// Token: 0x04000619 RID: 1561
	private int attackQueueSteps;

	// Token: 0x0400061A RID: 1562
	private bool attackQueuing;

	// Token: 0x0400061B RID: 1563
	public bool touchingWallL;

	// Token: 0x0400061C RID: 1564
	public bool touchingWallR;

	// Token: 0x0400061D RID: 1565
	public bool wallSlidingL;

	// Token: 0x0400061E RID: 1566
	public bool wallSlidingR;

	// Token: 0x0400061F RID: 1567
	private bool airDashed;

	// Token: 0x04000620 RID: 1568
	public bool dashingDown;

	// Token: 0x04000621 RID: 1569
	public bool wieldingLantern;

	// Token: 0x04000622 RID: 1570
	private bool startWithWallslide;

	// Token: 0x04000623 RID: 1571
	private bool startWithJump;

	// Token: 0x04000624 RID: 1572
	private bool startWithFullJump;

	// Token: 0x04000625 RID: 1573
	private bool startWithDash;

	// Token: 0x04000626 RID: 1574
	private bool startWithAttack;

	// Token: 0x04000627 RID: 1575
	private bool nailArt_cyclone;

	// Token: 0x04000628 RID: 1576
	private bool wallSlashing;

	// Token: 0x04000629 RID: 1577
	private bool doubleJumped;

	// Token: 0x0400062A RID: 1578
	public bool inAcid;

	// Token: 0x0400062B RID: 1579
	private bool wallJumpedR;

	// Token: 0x0400062C RID: 1580
	private bool wallJumpedL;

	// Token: 0x0400062D RID: 1581
	public bool wallLocked;

	// Token: 0x0400062E RID: 1582
	private float currentWalljumpSpeed;

	// Token: 0x0400062F RID: 1583
	private float walljumpSpeedDecel;

	// Token: 0x04000630 RID: 1584
	private int wallUnstickSteps;

	// Token: 0x04000631 RID: 1585
	private bool recoilLarge;

	// Token: 0x04000632 RID: 1586
	public float conveyorSpeed;

	// Token: 0x04000633 RID: 1587
	public float conveyorSpeedV;

	// Token: 0x04000634 RID: 1588
	private bool enteringVertically;

	// Token: 0x04000635 RID: 1589
	private bool playingWallslideClip;

	// Token: 0x04000636 RID: 1590
	private bool playedMantisClawClip;

	// Token: 0x04000637 RID: 1591
	public bool exitedSuperDashing;

	// Token: 0x04000638 RID: 1592
	public bool exitedQuake;

	// Token: 0x04000639 RID: 1593
	private bool fallCheckFlagged;

	// Token: 0x0400063A RID: 1594
	private int ledgeBufferSteps;

	// Token: 0x0400063B RID: 1595
	private int headBumpSteps;

	// Token: 0x0400063C RID: 1596
	private float nailChargeTime;

	// Token: 0x0400063D RID: 1597
	public bool takeNoDamage;

	// Token: 0x0400063E RID: 1598
	private bool joniBeam;

	// Token: 0x0400063F RID: 1599
	public bool fadedSceneIn;

	// Token: 0x04000640 RID: 1600
	private bool stopWalkingOut;

	// Token: 0x04000641 RID: 1601
	private bool boundsChecking;

	// Token: 0x04000642 RID: 1602
	private bool blockerFix;

	// Token: 0x04000643 RID: 1603
	[SerializeField]
	private Vector2[] positionHistory;

	// Token: 0x04000644 RID: 1604
	private bool tilemapTestActive;

	// Token: 0x04000645 RID: 1605
	private Vector2 groundRayOriginC;

	// Token: 0x04000646 RID: 1606
	private Vector2 groundRayOriginL;

	// Token: 0x04000647 RID: 1607
	private Vector2 groundRayOriginR;

	// Token: 0x04000648 RID: 1608
	private Coroutine takeDamageCoroutine;

	// Token: 0x04000649 RID: 1609
	private Coroutine tilemapTestCoroutine;

	// Token: 0x0400064A RID: 1610
	public AudioClip footstepsRunDust;

	// Token: 0x0400064B RID: 1611
	public AudioClip footstepsRunGrass;

	// Token: 0x0400064C RID: 1612
	public AudioClip footstepsRunBone;

	// Token: 0x0400064D RID: 1613
	public AudioClip footstepsRunSpa;

	// Token: 0x0400064E RID: 1614
	public AudioClip footstepsRunMetal;

	// Token: 0x0400064F RID: 1615
	public AudioClip footstepsRunWater;

	// Token: 0x04000650 RID: 1616
	public AudioClip footstepsWalkDust;

	// Token: 0x04000651 RID: 1617
	public AudioClip footstepsWalkGrass;

	// Token: 0x04000652 RID: 1618
	public AudioClip footstepsWalkBone;

	// Token: 0x04000653 RID: 1619
	public AudioClip footstepsWalkSpa;

	// Token: 0x04000654 RID: 1620
	public AudioClip footstepsWalkMetal;

	// Token: 0x04000655 RID: 1621
	public AudioClip nailArtCharge;

	// Token: 0x04000656 RID: 1622
	public AudioClip nailArtChargeComplete;

	// Token: 0x04000657 RID: 1623
	public AudioClip blockerImpact;

	// Token: 0x04000658 RID: 1624
	public AudioClip shadowDashClip;

	// Token: 0x04000659 RID: 1625
	public AudioClip sharpShadowClip;

	// Token: 0x0400065A RID: 1626
	public AudioClip doubleJumpClip;

	// Token: 0x0400065B RID: 1627
	public AudioClip mantisClawClip;

	// Token: 0x0400065C RID: 1628
	private GameObject slash;

	// Token: 0x0400065D RID: 1629
	//private NailSlash slashComponent;

	// Token: 0x0400065E RID: 1630
	//private PlayMakerFSM slashFsm;

	// Token: 0x0400065F RID: 1631
	private GameObject runEffect;

	// Token: 0x04000660 RID: 1632
	private GameObject backDash;

	// Token: 0x04000661 RID: 1633
	private GameObject jumpEffect;

	// Token: 0x04000662 RID: 1634
	private GameObject fallEffect;

	// Token: 0x04000663 RID: 1635
	private GameObject dashEffect;

	// Token: 0x04000664 RID: 1636
	//private GameObject grubberFlyBeam;

	// Token: 0x04000665 RID: 1637
	private GameObject hazardCorpe;

	// Token: 0x04000666 RID: 1638
	//public PlayMakerFSM vignetteFSM;

	// Token: 0x04000667 RID: 1639
	//public SpriteRenderer heroLight;

	// Token: 0x04000668 RID: 1640
	//public SpriteRenderer vignette;

	// Token: 0x04000669 RID: 1641
	//public PlayMakerFSM dashBurst;

	// Token: 0x0400066A RID: 1642
	//public PlayMakerFSM superDash;

	// Token: 0x0400066B RID: 1643
	//public PlayMakerFSM fsm_thornCounter;

	// Token: 0x0400066C RID: 1644
	//public PlayMakerFSM spellControl;

	// Token: 0x0400066D RID: 1645
	//public PlayMakerFSM fsm_fallTrail;

	// Token: 0x0400066E RID: 1646
	//public PlayMakerFSM fsm_orbitShield;

	// Token: 0x0400066F RID: 1647
	/*public VibrationData softLandVibration;

	// Token: 0x04000670 RID: 1648
	public VibrationData wallJumpVibration;

	// Token: 0x04000671 RID: 1649
	public VibrationPlayer wallSlideVibrationPlayer;

	// Token: 0x04000672 RID: 1650
	public VibrationData dashVibration;

	// Token: 0x04000673 RID: 1651
	public VibrationData shadowDashVibration;

	// Token: 0x04000674 RID: 1652
	public VibrationData doubleJumpVibration;*/

	// Token: 0x04000676 RID: 1654
	public bool isHeroInPosition;

	// Token: 0x04000679 RID: 1657
	private bool jumpReleaseQueueingEnabled;

	// Token: 0x0400067A RID: 1658
	private static HeroController _instance;

	// Token: 0x0400067B RID: 1659
	private const float PreventCastByDialogueEndDuration = 0.3f;

	// Token: 0x0400067C RID: 1660
	private float preventCastByDialogueEndTimer;

	// Token: 0x0400067D RID: 1661
	private Vector2 oldPos;

	// Token: 0x020000F8 RID: 248
	// (Invoke) Token: 0x06000616 RID: 1558
	public delegate void HeroInPosition(bool forceDirect);

	// Token: 0x020000F9 RID: 249
	// (Invoke) Token: 0x0600061A RID: 1562
	public delegate void TakeDamageEvent();

	// Token: 0x020000FA RID: 250
	// (Invoke) Token: 0x0600061E RID: 1566
	public delegate void HeroDeathEvent();
}






/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroController : MonoBehaviour
{
	public delegate void HeroInPosition(bool forceDirect);
	public event HeroInPosition heroInPosition;

	private static HeroController _instance;
	public static HeroController instance => _instance;

	private GameManager gm;
	private Rigidbody2D rb2d;
	private float prevGravityScale;
	public HeroControllerStates cState = new HeroControllerStates();
	public Vector2 current_velocity;

	public float DEFAULT_GRAVITY = 0.79f;

	private void Awake()
	{
		_instance = this;

		SetupGameRefs();
	}

	private void Start()
	{
		if (gm.IsGameplayScene())
		{
			//this.isGameplayScene = true;
			//this.vignette.enabled = true;
			if (heroInPosition != null)
			{
				heroInPosition(false);
			}
			//this.FinishedEnteringScene(true, false);
		}
		else
		{
			//this.isGameplayScene = false;
			//transform.SetPositionY(-2000f);
			transform.position = new Vector3(transform.position.x, -2000f, transform.position.z);
			//this.vignette.enabled = false;
			AffectedByGravity(false);
		}
	}


	private void Update()
	{
		current_velocity = this.rb2d.velocity;
	}

	private void SetupGameRefs()
	{
		gm = GameManager.instance;
		rb2d = GetComponent<Rigidbody2D>();
		prevGravityScale = DEFAULT_GRAVITY;
	}

	public void SceneInit()
	{
		if (this == HeroController._instance)
		{
			if (!this.gm)
			{
				this.gm = GameManager.instance;
			}
			if (this.gm.IsGameplayScene())
			{
				//this.isGameplayScene = true;
				//HeroBox.inactive = false;
			}
			else
			{
				//this.isGameplayScene = false;
				//this.acceptingInput = false;
				//this.SetState(ActorStates.no_input);
				//this.transform.SetPositionY(-2000f);
				transform.position = new Vector3(transform.position.x,-2000f,transform.position.z);
				//this.vignette.enabled = false;
				this.AffectedByGravity(false);
			}
			//this.transform.SetPositionZ(0.004f);
			transform.position = new Vector3(transform.position.x,transform.position.y,0.004f);
			//if (!this.blockerFix)
			//{
				//if (this.playerData.GetBool("killedBlocker"))
				//{
					//this.gm.SetPlayerDataInt("killsBlocker", 0);
				//}
				//this.blockerFix = true;
			//}
			//this.SetWalkZone(false);
		}
	}

	public void FaceRight()
	{
		this.cState.facingRight = true;
		Vector3 localScale = this.transform.localScale;
		localScale.x = -1f;
		this.transform.localScale = localScale;
	}

	// Token: 0x0600054A RID: 1354 RVA: 0x0001D2F8 File Offset: 0x0001B4F8
	public void FaceLeft()
	{
		this.cState.facingRight = false;
		Vector3 localScale = this.transform.localScale;
		localScale.x = 1f;
		this.transform.localScale = localScale;
	}

	public void IgnoreInputWithoutReset()
	{
		if (this.acceptingInput)
		{
			this.acceptingInput = false;
		}
	}

	private void ResetMotion()
	{
		this.CancelJump();
		this.CancelDoubleJump();
		this.CancelDash();
		this.CancelBackDash();
		this.CancelBounce();
		this.CancelRecoilHorizontal();
		this.CancelWallsliding();
		this.rb2d.velocity = Vector2.zero;
		this.transition_vel = Vector2.zero;
		this.wallLocked = false;
		this.nailChargeTimer = 0f;
	}

	private void CancelJump()
	{
		this.cState.jumping = false;
		this.jumpReleaseQueuing = false;
		this.jump_steps = 0;
	}

	// Token: 0x060005D8 RID: 1496 RVA: 0x00020A33 File Offset: 0x0001EC33
	private void CancelDoubleJump()
	{
		this.cState.doubleJumping = false;
		this.doubleJump_steps = 0;
	}

	// Token: 0x060005D9 RID: 1497 RVA: 0x00020A48 File Offset: 0x0001EC48
	private void CancelDash()
	{
		if (this.cState.shadowDashing)
		{
			this.cState.shadowDashing = false;
		}
		this.cState.dashing = false;
		this.dash_timer = 0f;
		this.AffectedByGravity(true);
		this.sharpShadowPrefab.SetActive(false);
		if (this.dashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission)
		{
			this.dashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission = false;
		}
		if (this.shadowdashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission)
		{
			this.shadowdashParticlesPrefab.GetComponent<ParticleSystem>().enableEmission = false;
		}
	}

	// Token: 0x060005DA RID: 1498 RVA: 0x00020AE0 File Offset: 0x0001ECE0
	private void CancelWallsliding()
	{
		this.wallslideDustPrefab.enableEmission = false;
		this.wallSlideVibrationPlayer.Stop();
		this.cState.wallSliding = false;
		this.wallSlidingL = false;
		this.wallSlidingR = false;
		this.touchingWallL = false;
		this.touchingWallR = false;
	}

	// Token: 0x060005DB RID: 1499 RVA: 0x00020B2C File Offset: 0x0001ED2C
	private void CancelBackDash()
	{
		this.cState.backDashing = false;
		this.back_dash_timer = 0f;
	}

	// Token: 0x060005DC RID: 1500 RVA: 0x00020B45 File Offset: 0x0001ED45
	private void CancelDownAttack()
	{
		if (this.cState.downAttacking)
		{
			this.slashComponent.CancelAttack();
			this.ResetAttacks();
		}
	}

	// Token: 0x060005DD RID: 1501 RVA: 0x00020B65 File Offset: 0x0001ED65
	private void CancelAttack()
	{
		if (this.cState.attacking)
		{
			this.slashComponent.CancelAttack();
			this.ResetAttacks();
		}
	}

	// Token: 0x060005DE RID: 1502 RVA: 0x00020B85 File Offset: 0x0001ED85
	private void CancelBounce()
	{
		this.cState.bouncing = false;
		this.cState.shroomBouncing = false;
		this.bounceTimer = 0f;
	}

	// Token: 0x060005DF RID: 1503 RVA: 0x00020BAA File Offset: 0x0001EDAA
	private void CancelRecoilHorizontal()
	{
		this.cState.recoilingLeft = false;
		this.cState.recoilingRight = false;
		this.recoilSteps = 0;
	}

	// Token: 0x060005E0 RID: 1504 RVA: 0x00020BCB File Offset: 0x0001EDCB
	private void CancelDamageRecoil()
	{
		this.cState.recoiling = false;
		this.recoilTimer = 0f;
		this.ResetMotion();
		this.AffectedByGravity(true);
		this.SetDamageMode(DamageMode.FULL_DAMAGE);
	}

	// Token: 0x060005E1 RID: 1505 RVA: 0x00020BF8 File Offset: 0x0001EDF8
	private void CancelFallEffects()
	{
		this.fallRumble = false;
		this.audioCtrl.StopSound(HeroSounds.FALLING);
		GameCameras.instance.cameraShakeFSM.Fsm.Variables.FindFsmBool("RumblingFall").Value = false;
	}

	public IEnumerator EnterScene(TransitionPoint enterGate, float delayBeforeEnter)
	{
		this.IgnoreInputWithoutReset();
		this.ResetMotion();
		this.airDashed = false;
		this.doubleJumped = false;
		this.ResetHardLandingTimer();
		this.ResetAttacksDash();
		this.AffectedByGravity(false);
		this.sceneEntryGate = enterGate;
		this.SetState(ActorStates.no_input);
		this.transitionState = HeroTransitionState.WAITING_TO_ENTER_LEVEL;
		this.vignetteFSM.SendEvent("RESET");
		if (!this.cState.transitioning)
		{
			this.cState.transitioning = true;
		}
		this.gatePosition = enterGate.GetGatePosition();
		if (this.gatePosition == GatePosition.top)
		{
			this.cState.onGround = false;
			this.enteringVertically = true;
			this.exitedSuperDashing = false;
			this.renderer.enabled = false;
			float x2 = enterGate.transform.position.x + enterGate.entryOffset.x;
			float y2 = enterGate.transform.position.y + enterGate.entryOffset.y;
			this.transform.SetPosition2D(x2, y2);
			if (this.heroInPosition != null)
			{
				this.heroInPosition(false);
			}
			yield return new WaitForSeconds(0.165f);
			if (!enterGate.customFade)
			{
				this.gm.FadeSceneIn();
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
			this.renderer.enabled = true;
			if (this.exitedQuake)
			{
				this.IgnoreInput();
				this.proxyFSM.SendEvent("HeroCtrl-EnterQuake");
				yield return new WaitForSeconds(0.25f);
				this.FinishedEnteringScene(true, false);
			}
			else
			{
				this.rb2d.velocity = new Vector2(0f, this.SPEED_TO_ENTER_SCENE_DOWN);
				this.transitionState = HeroTransitionState.ENTERING_SCENE;
				this.transitionState = HeroTransitionState.DROPPING_DOWN;
				this.AffectedByGravity(true);
				if (enterGate.hardLandOnExit)
				{
					this.cState.willHardLand = true;
				}
				yield return new WaitForSeconds(0.33f);
				this.transitionState = HeroTransitionState.ENTERING_SCENE;
				if (this.transitionState != HeroTransitionState.WAITING_TO_TRANSITION)
				{
					this.FinishedEnteringScene(true, false);
				}
			}
		}
		else if (this.gatePosition == GatePosition.bottom)
		{
			this.cState.onGround = false;
			this.enteringVertically = true;
			this.exitedSuperDashing = false;
			if (enterGate.alwaysEnterRight)
			{
				this.FaceRight();
			}
			if (enterGate.alwaysEnterLeft)
			{
				this.FaceLeft();
			}
			float x = enterGate.transform.position.x + enterGate.entryOffset.x;
			float y = enterGate.transform.position.y + enterGate.entryOffset.y + 3f;
			this.transform.SetPosition2D(x, y);
			if (this.heroInPosition != null)
			{
				this.heroInPosition(false);
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
				this.gm.FadeSceneIn();
			}
			if (this.cState.facingRight)
			{
				this.transition_vel = new Vector2(this.SPEED_TO_ENTER_SCENE_HOR, this.SPEED_TO_ENTER_SCENE_UP);
			}
			else
			{
				this.transition_vel = new Vector2(-this.SPEED_TO_ENTER_SCENE_HOR, this.SPEED_TO_ENTER_SCENE_UP);
			}
			this.transitionState = HeroTransitionState.ENTERING_SCENE;
			this.transform.SetPosition2D(x, y);
			yield return new WaitForSeconds(this.TIME_TO_ENTER_SCENE_BOT);
			this.transition_vel = new Vector2(this.rb2d.velocity.x, 0f);
			this.AffectedByGravity(true);
			this.transitionState = HeroTransitionState.DROPPING_DOWN;
		}
		else if (this.gatePosition == GatePosition.left)
		{
			this.cState.onGround = true;
			this.enteringVertically = false;
			this.SetState(ActorStates.no_input);
			float num = enterGate.transform.position.x + enterGate.entryOffset.x;
			float y3 = this.FindGroundPointY(num + 2f, enterGate.transform.position.y, false);
			this.transform.SetPosition2D(num, y3);
			if (this.heroInPosition != null)
			{
				this.heroInPosition(true);
			}
			this.FaceRight();
			yield return new WaitForSeconds(0.165f);
			if (!enterGate.customFade)
			{
				this.gm.FadeSceneIn();
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
			if (this.exitedSuperDashing)
			{
				this.IgnoreInput();
				this.proxyFSM.SendEvent("HeroCtrl-EnterSuperDash");
				yield return new WaitForSeconds(0.25f);
				this.FinishedEnteringScene(true, false);
			}
			else
			{
				this.transition_vel = new Vector2(this.RUN_SPEED, 0f);
				this.transitionState = HeroTransitionState.ENTERING_SCENE;
				yield return new WaitForSeconds(0.33f);
				this.FinishedEnteringScene(true, true);
			}
		}
		else if (this.gatePosition == GatePosition.right)
		{
			this.cState.onGround = true;
			this.enteringVertically = false;
			this.SetState(ActorStates.no_input);
			float num2 = enterGate.transform.position.x + enterGate.entryOffset.x;
			float y4 = this.FindGroundPointY(num2 - 2f, enterGate.transform.position.y, false);
			this.transform.SetPosition2D(num2, y4);
			if (this.heroInPosition != null)
			{
				this.heroInPosition(true);
			}
			this.FaceLeft();
			yield return new WaitForSeconds(0.165f);
			if (!enterGate.customFade)
			{
				this.gm.FadeSceneIn();
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
			if (this.exitedSuperDashing)
			{
				this.IgnoreInput();
				this.proxyFSM.SendEvent("HeroCtrl-EnterSuperDash");
				yield return new WaitForSeconds(0.25f);
				this.FinishedEnteringScene(true, false);
			}
			else
			{
				this.transition_vel = new Vector2(-this.RUN_SPEED, 0f);
				this.transitionState = HeroTransitionState.ENTERING_SCENE;
				yield return new WaitForSeconds(0.33f);
				this.FinishedEnteringScene(true, true);
			}
		}
		else if (this.gatePosition == GatePosition.door)
		{
			if (enterGate.alwaysEnterRight)
			{
				this.FaceRight();
			}
			if (enterGate.alwaysEnterLeft)
			{
				this.FaceLeft();
			}
			this.cState.onGround = true;
			this.enteringVertically = false;
			this.SetState(ActorStates.idle);
			this.SetState(ActorStates.no_input);
			this.exitedSuperDashing = false;
			this.animCtrl.PlayClip("Idle");
			this.transform.SetPosition2D(this.FindGroundPoint(enterGate.transform.position, false));
			if (this.heroInPosition != null)
			{
				this.heroInPosition(false);
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
				this.gm.FadeSceneIn();
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (enterGate.dontWalkOutOfDoor)
			{
				yield return new WaitForSeconds(0.33f);
			}
			else
			{
				float clipDuration = this.animCtrl.GetClipDuration("Exit Door To Idle");
				this.animCtrl.PlayClip("Exit Door To Idle");
				if (clipDuration > 0f)
				{
					yield return new WaitForSeconds(clipDuration);
				}
				else
				{
					yield return new WaitForSeconds(0.33f);
				}
			}
			this.FinishedEnteringScene(true, false);
		}
		yield break;
	}


	public Transform LocateSpawnPoint()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("RespawnPoint");
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].name == PlayerData.instance.GetString("respawnMarkerName"))
			{
				return array[i].transform;
			}
		}
		return null;
	}

	public void AffectedByGravity(bool gravityApplies)
	{
		float gravityScale = rb2d.gravityScale;
		if (rb2d.gravityScale > Mathf.Epsilon && !gravityApplies)
		{
			prevGravityScale = rb2d.gravityScale;
			rb2d.gravityScale = 0f;
			return;
		}
		if (rb2d.gravityScale <= Mathf.Epsilon && gravityApplies)
		{
			rb2d.gravityScale = prevGravityScale;
			prevGravityScale = 0f;
		}
	}
}*/

