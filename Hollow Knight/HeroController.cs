using GlobalEnums;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroController : MonoBehaviour
{
    static Type spriteFlasherType;

    static Type GetFlasherType()
    {
        if (spriteFlasherType == null)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name == "WeaverCore")
                {
                    spriteFlasherType = asm.GetType("WeaverCore.Components.SpriteFlasher");
                    break;
                }
            }
        }
        return spriteFlasherType;
    }

    public abstract class INTERNAL_INPUTMANAGER
    {
        public abstract bool IsInputPressed(string inputName);
        public abstract bool WasInputPressed(string inputName);
        public abstract bool WasInputReleased(string inputName);
        public abstract Vector2 GetInputVector(string joystickName);
    }

    public float fallTimer { get; private set; }

    //public GeoCounter geoCounter { get; private set; }

    //public PlayMakerFSM proxyFSM { get; private set; }

    public TransitionPoint sceneEntryGate { get; private set; }

    public event HeroController.HeroInPosition heroInPosition;

    public event HeroController.TakeDamageEvent OnTakenDamage;

    public event HeroController.HeroDeathEvent OnDeath;

    public bool IsDreamReturning =>
            /*PlayMakerFSM playMakerFSM = PlayMakerFSM.FindFsmOnGameObject(base.gameObject, "Dream Return");
if (playMakerFSM)
{
FsmBool fsmBool = playMakerFSM.FsmVariables.FindFsmBool("Dream Returning");
if (fsmBool != null)
{
return fsmBool.Value;
}
}*/
            false;

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

    public static HeroController UnsafeInstance => HeroController._instance;

    private INTERNAL_INPUTMANAGER _inputManager;

    private INTERNAL_INPUTMANAGER InputManager
    {
        get
        {
            if (_inputManager == null)
            {
                foreach (System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (Type type in asm.GetTypes())
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

    private bool RightPressed => InputManager.IsInputPressed("right");

    private bool LeftPressed => InputManager.IsInputPressed("left");

    private bool UpPressed => InputManager.IsInputPressed("up");

    private bool DownPressed => InputManager.IsInputPressed("down");

    private bool JumpPressed => InputManager.IsInputPressed("jump");

    private bool JumpWasPressed => InputManager.WasInputPressed("jump");

    private bool JumpWasReleased => InputManager.WasInputReleased("jump");

    private bool DashPressed => InputManager.IsInputPressed("dash");

    private bool DashWasPressed => InputManager.WasInputPressed("dash");

    private bool DashWasReleased => InputManager.WasInputReleased("dash");

    private bool AttackPressed => InputManager.IsInputPressed("attack");

    private bool AttackWasPressed => InputManager.WasInputPressed("attack");

    private bool AttackWasReleased => InputManager.WasInputReleased("attack");

    private bool RightWasPressed => InputManager.WasInputPressed("right");

    private bool LeftWasPressed => InputManager.WasInputPressed("left");

    private bool UpWasPressed => InputManager.WasInputPressed("up");

    private bool DownWasPressed => InputManager.WasInputPressed("down");

    private Vector2 InputVector => InputManager.GetInputVector("moveVector");

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
        SetupGameRefs();
        SetupPools();
    }

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
            //this.vignette.enabled = true;
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
            //this.vignette.enabled = false;
            AffectedByGravity(false);
        }
        CharmUpdate();
        /*if (this.acidDeathPrefab)
		{
			ObjectPool.CreatePool(this.acidDeathPrefab, 1);
		}
		if (this.spikeDeathPrefab)
		{
			ObjectPool.CreatePool(this.spikeDeathPrefab, 1);
		}*/
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
                //this.vignette.enabled = false;
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

    private void LateUpdate()
    {
        //prevHorizontalState = Input.GetAxisRaw("Horizontal");
        //prevVerticalState = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        if (cState.recoilingLeft || cState.recoilingRight)
        {
            if (recoilSteps <= RECOIL_HOR_STEPS)
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
            bool shroomBouncing = cState.shroomBouncing;
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
                if (wallSlidingL && RightPressed/*this.inputHandler.inputActions.right.IsPressed*/)
                {
                    wallUnstickSteps++;
                }
                else if (wallSlidingR && LeftPressed/*this.inputHandler.inputActions.left.IsPressed*/)
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
            //if (this.inputHandler.inputActions.right.IsPressed && !this.inputHandler.inputActions.left.IsPressed)
            if (RightPressed && !LeftPressed)
            {
                rb2d.velocity = new Vector3(CYCLONE_HORIZONTAL_SPEED, rb2d.velocity.y);
            }
            //else if (this.inputHandler.inputActions.left.IsPressed && !this.inputHandler.inputActions.right.IsPressed)
            else if (LeftPressed && !RightPressed)
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
            //this.superDash.SendEvent("SLOPE CANCEL");
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
        float scaleX = transform.GetScaleX();
        if (scaleX < -1f)
        {
            transform.SetScaleX(-1f);
        }
        if (scaleX > 1f)
        {
            transform.SetScaleX(1f);
        }
        if (transform.position.z != 0.004f)
        {
            transform.SetPositionZ(0.004f);
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
                return;
            }
            if (inAcid)
            {
                rb2d.velocity = new Vector2(move_direction * UNDERWATER_SPEED, rb2d.velocity.y);
                return;
            }
            if (playerData.GetBool("equippedCharm_37") && cState.onGround && playerData.GetBool("equippedCharm_31"))
            {
                rb2d.velocity = new Vector2(move_direction * RUN_SPEED_CH_COMBO, rb2d.velocity.y);
                return;
            }
            if (playerData.GetBool("equippedCharm_37") && cState.onGround)
            {
                rb2d.velocity = new Vector2(move_direction * RUN_SPEED_CH, rb2d.velocity.y);
                return;
            }
            rb2d.velocity = new Vector2(move_direction * RUN_SPEED, rb2d.velocity.y);
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
            return;
        }
        CancelJump();
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
        ModHooks.OnAttack(attackDir);
        if (Time.timeSinceLevelLoad - altAttackTime > ALT_ATTACK_RESET)
        {
            cState.altAttack = false;
        }
        cState.attacking = true;
        if (playerData.GetBool("equippedCharm_32"))
        {
            attackDuration = ATTACK_DURATION_CH;
        }
        else
        {
            attackDuration = ATTACK_DURATION;
        }
        if (cState.wallSliding)
        {
            wallSlashing = true;
            //this.slashComponent = this.wallSlash;
            //this.slashFsm = this.wallSlashFsm;
        }
        else
        {
            wallSlashing = false;
            if (attackDir == AttackDirection.normal)
            {
                if (!cState.altAttack)
                {
                    //this.slashComponent = this.normalSlash;
                    //this.slashFsm = this.normalSlashFsm;
                    cState.altAttack = true;
                }
                else
                {
                    //this.slashComponent = this.alternateSlash;
                    //this.slashFsm = this.alternateSlashFsm;
                    cState.altAttack = false;
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
                cState.upAttacking = true;
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
                cState.downAttacking = true;
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
        if (cState.wallSliding)
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
        altAttackTime = Time.timeSinceLevelLoad;
        ModHooks.AfterAttack(attackDir);
        if (!cState.attacking)
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

    private void Dash()
    {
        AffectedByGravity(false);
        ResetHardLandingTimer();
        if (dash_timer > DASH_TIME)
        {
            FinishedDashing();
            return;
        }
        Vector2 vector = OrigDashVector();
        vector = ModHooks.DashVelocityChange(vector);
        rb2d.velocity = vector;
        dash_timer += Time.deltaTime;
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
        //this.focusMP_amount *= ModHooks.OnFocusCost();
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
            UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
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
            return;
        }
        wieldingLantern = false;
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
        //ModHooks.OnCharmUpdate();
        //this.playerData.UpdateBlueHealth();
    }

    public void checkEnvironment()
    {
        if (playerData.GetInt("environmentType") == 0)
        {
            footStepsRunAudioSource.clip = footstepsRunDust;
            footStepsWalkAudioSource.clip = footstepsWalkDust;
            return;
        }
        if (playerData.GetInt("environmentType") == 1)
        {
            footStepsRunAudioSource.clip = footstepsRunGrass;
            footStepsWalkAudioSource.clip = footstepsWalkGrass;
            return;
        }
        if (playerData.GetInt("environmentType") == 2)
        {
            footStepsRunAudioSource.clip = footstepsRunBone;
            footStepsWalkAudioSource.clip = footstepsWalkBone;
            return;
        }
        if (playerData.GetInt("environmentType") == 3)
        {
            footStepsRunAudioSource.clip = footstepsRunSpa;
            footStepsWalkAudioSource.clip = footstepsWalkSpa;
            return;
        }
        if (playerData.GetInt("environmentType") == 4)
        {
            footStepsRunAudioSource.clip = footstepsRunMetal;
            footStepsWalkAudioSource.clip = footstepsWalkMetal;
            return;
        }
        if (playerData.GetInt("environmentType") == 6)
        {
            footStepsRunAudioSource.clip = footstepsRunWater;
            footStepsWalkAudioSource.clip = footstepsRunWater;
            return;
        }
        if (playerData.GetInt("environmentType") == 7)
        {
            footStepsRunAudioSource.clip = footstepsRunGrass;
            footStepsWalkAudioSource.clip = footstepsWalkGrass;
        }
    }

    public void SetBenchRespawn(string spawnMarker, string sceneName, int spawnType, bool facingRight)
    {
        //this.playerData.SetBenchRespawn(spawnMarker, sceneName, spawnType, facingRight);
    }

    public void SetHazardRespawn(Vector3 position, bool facingRight)
    {
        //this.playerData.SetHazardRespawn(position, facingRight);
    }

    public void AddGeo(int amount)
    {

        //this.playerData.AddGeo(amount);
        //this.geoCounter.AddGeo(amount);
    }

    public void ToZero()
    {
        //this.geoCounter.ToZero();
    }

    public void AddGeoQuietly(int amount)
    {
        //this.playerData.AddGeo(amount);
    }

    public void AddGeoToCounter(int amount)
    {
        //this.geoCounter.AddGeo(amount);
    }

    public void TakeGeo(int amount)
    {
        //this.playerData.TakeGeo(amount);
        //this.geoCounter.TakeGeo(amount);
    }

    public void UpdateGeo()
    {
        //this.geoCounter.UpdateGeo();
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
            if (CanTakeDamage())
            {
                if (damageMode == DamageMode.HAZARD_ONLY && hazardType == 1)
                {
                    return;
                }
                if (cState.shadowDashing && hazardType == 1)
                {
                    return;
                }
                if (parryInvulnTimer > 0f && hazardType == 1)
                {
                    return;
                }
                //VibrationMixer mixer = VibrationManager.GetMixer();
                /*if (mixer != null)
				{
					mixer.StopAllEmissionsWithTag("heroAction");
				}*/
                bool flag = false;
                if (carefreeShieldEquipped && hazardType == 1)
                {
                    if (hitsSinceShielded > 7)
                    {
                        hitsSinceShielded = 7;
                    }
                    switch (hitsSinceShielded)
                    {
                        case 1:
                            if (UnityEngine.Random.Range(1, 100) <= 10f)
                            {
                                flag = true;
                            }
                            break;
                        case 2:
                            if (UnityEngine.Random.Range(1, 100) <= 20f)
                            {
                                flag = true;
                            }
                            break;
                        case 3:
                            if (UnityEngine.Random.Range(1, 100) <= 30f)
                            {
                                flag = true;
                            }
                            break;
                        case 4:
                            if (UnityEngine.Random.Range(1, 100) <= 50f)
                            {
                                flag = true;
                            }
                            break;
                        case 5:
                            if (UnityEngine.Random.Range(1, 100) <= 70f)
                            {
                                flag = true;
                            }
                            break;
                        case 6:
                            if (UnityEngine.Random.Range(1, 100) <= 80f)
                            {
                                flag = true;
                            }
                            break;
                        case 7:
                            if (UnityEngine.Random.Range(1, 100) <= 90f)
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
                        hitsSinceShielded = 0;
                        //this.carefreeShield.SetActive(true);
                        damageAmount = 0;
                        spawnDamageEffect = false;
                    }
                    else
                    {
                        hitsSinceShielded++;
                    }
                }
                damageAmount = ModHooks.AfterTakeDamage(hazardType, damageAmount);
                if (playerData.GetBool("equippedCharm_5") && playerData.GetInt("blockerHits") > 0 && hazardType == 1 && cState.focusing && !flag)
                {
                    //this.proxyFSM.SendEvent("HeroCtrl-TookBlockerHit");
                    audioSource.PlayOneShot(blockerImpact, 1f);
                    spawnDamageEffect = false;
                    damageAmount = 0;
                }
                else
                {
                    //this.proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
                }
                CancelAttack();
                if (cState.wallSliding)
                {
                    cState.wallSliding = false;
                    //this.wallSlideVibrationPlayer.Stop();
                }
                if (cState.touchingWall)
                {
                    cState.touchingWall = false;
                }
                if (cState.recoilingLeft || cState.recoilingRight)
                {
                    CancelRecoilHorizontal();
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
                if (!flag)
                {
                    //this.audioCtrl.PlaySound(HeroSounds.TAKE_HIT);
                }
                if (!takeNoDamage && !playerData.GetBool("invinciTest"))
                {
                    if (playerData.GetBool("overcharmed"))
                    {
                        playerData.TakeHealth(damageAmount * 2);
                    }
                    else
                    {
                        playerData.TakeHealth(damageAmount);
                    }
                }
                if (playerData.GetBool("equippedCharm_3") && damageAmount > 0)
                {
                    if (playerData.GetBool("equippedCharm_35"))
                    {
                        AddMPCharge(GRUB_SOUL_MP_COMBO);
                    }
                    else
                    {
                        AddMPCharge(GRUB_SOUL_MP);
                    }
                }
                if (joniBeam && damageAmount > 0)
                {
                    joniBeam = false;
                }
                if (cState.nailCharging || nailChargeTimer != 0f)
                {
                    cState.nailCharging = false;
                    nailChargeTimer = 0f;
                }
                if (damageAmount > 0 && OnTakenDamage != null)
                {
                    OnTakenDamage();
                }
                if (playerData.GetInt("health") == 0)
                {
                    base.StartCoroutine(Die());
                    return;
                }
                if (hazardType == 2)
                {
                    base.StartCoroutine(DieFromHazard(HazardType.SPIKES, (!(go != null)) ? 0f : go.transform.rotation.z));
                    return;
                }
                if (hazardType == 3)
                {
                    base.StartCoroutine(DieFromHazard(HazardType.ACID, 0f));
                    return;
                }
                if (hazardType == 4)
                {
                    Debug.Log("Lava death");
                    return;
                }
                if (hazardType == 5)
                {
                    base.StartCoroutine(DieFromHazard(HazardType.PIT, 0f));
                    return;
                }
                base.StartCoroutine(StartRecoil(damageSide, spawnDamageEffect, damageAmount));
                return;
            }
            else if (cState.invulnerable && !cState.hazardDeath && !playerData.GetBool("isInvincible"))
            {
                if (hazardType == 2)
                {
                    if (!takeNoDamage)
                    {
                        playerData.TakeHealth(damageAmount);
                    }
                    //this.proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
                    if (playerData.GetInt("health") == 0)
                    {
                        base.StartCoroutine(Die());
                        return;
                    }
                    //this.audioCtrl.PlaySound(HeroSounds.TAKE_HIT);
                    base.StartCoroutine(DieFromHazard(HazardType.SPIKES, (!(go != null)) ? 0f : go.transform.rotation.z));
                    return;
                }
                else if (hazardType == 3)
                {
                    playerData.TakeHealth(damageAmount);
                    //this.proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
                    if (playerData.GetInt("health") == 0)
                    {
                        base.StartCoroutine(Die());
                        return;
                    }
                    base.StartCoroutine(DieFromHazard(HazardType.ACID, 0f));
                    return;
                }
                else if (hazardType == 4)
                {
                    Debug.Log("Lava damage");
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
        return "";
    }

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

    public void SoulGain()
    {
        int num;
        if (playerData.GetInt("MPCharge") < playerData.GetInt("maxMP"))
        {
            num = 11;
            if (playerData.GetBool("equippedCharm_20"))
            {
                num += 3;
            }
            if (playerData.GetBool("equippedCharm_21"))
            {
                num += 8;
            }
        }
        else
        {
            num = 6;
            if (playerData.GetBool("equippedCharm_20"))
            {
                num += 2;
            }
            if (playerData.GetBool("equippedCharm_21"))
            {
                num += 6;
            }
        }
        int @int = playerData.GetInt("MPReserve");
        //num = ModHooks.OnSoulGain(num);
        //this.playerData.AddMPCharge(num);
        /*GameCameras.instance.soulOrbFSM.SendEvent("MP GAIN");
		if (this.playerData.GetInt("MPReserve") != @int)
		{
			this.gm.soulVessel_fsm.SendEvent("MP RESERVE UP");
		}*/
    }

    public void AddMPChargeSpa(int amount)
    {
        TryAddMPChargeSpa(amount);
    }

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

    public void SetMPCharge(int amount)
    {
        playerData.SetIntSwappedArgs(amount, "MPCharge");
        //GameCameras.instance.soulOrbFSM.SendEvent("MP SET");
    }

    public void TakeMP(int amount)
    {
        if (playerData.GetInt("MPCharge") > 0)
        {
            /*this.playerData.TakeMP(amount);
			if (amount > 1)
			{
				GameCameras.instance.soulOrbFSM.SendEvent("MP LOSE");
			}*/
        }
    }

    public void TakeMPQuick(int amount)
    {
        if (playerData.GetInt("MPCharge") > 0)
        {
            /*this.playerData.TakeMP(amount);
			if (amount > 1)
			{
				GameCameras.instance.soulOrbFSM.SendEvent("MP DRAIN");
			}*/
        }
    }

    public void TakeReserveMP(int amount)
    {
        //this.playerData.TakeReserveMP(amount);
        //this.gm.soulVessel_fsm.SendEvent("MP RESERVE DOWN");
    }

    public void AddHealth(int amount)
    {
        playerData.AddHealth(amount);
        //this.proxyFSM.SendEvent("HeroCtrl-Healed");
    }

    public void TakeHealth(int amount)
    {
        playerData.TakeHealth(amount);
        //this.proxyFSM.SendEvent("HeroCtrl-HeroDamaged");
    }

    public void MaxHealth()
    {
        //this.proxyFSM.SendEvent("HeroCtrl-MaxHealth");
        playerData.MaxHealth();
    }

    public void MaxHealthKeepBlue()
    {
        int @int = playerData.GetInt("healthBlue");
        playerData.MaxHealth();
        playerData.SetIntSwappedArgs(@int, "healthBlue");
        //this.proxyFSM.SendEvent("HeroCtrl-Healed");
    }

    public void AddToMaxHealth(int amount)
    {
        //this.playerData.AddToMaxHealth(amount);
        gm.AwardAchievement("PROTECTED");
        if (playerData.GetInt("maxHealthBase") == playerData.GetInt("maxHealthCap"))
        {
            gm.AwardAchievement("MASKED");
        }
    }

    public void ClearMP()
    {
        //this.playerData.ClearMP();
    }

    public void ClearMPSendEvents()
    {
        ClearMP();
        //GameManager.instance.soulOrb_fsm.SendEvent("MP LOSE");
        //GameManager.instance.soulVessel_fsm.SendEvent("MP RESERVE DOWN");
    }

    public void AddToMaxMPReserve(int amount)
    {
        //this.playerData.AddToMaxMPReserve(amount);
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
        //this.vignetteFSM.SendEvent("RESET");
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
        //this.vignetteFSM.SendEvent("RESET");
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
            renderer.enabled = false;
            float x2 = enterGate.transform.position.x + enterGate.entryOffset.x;
            float y2 = enterGate.transform.position.y + enterGate.entryOffset.y;
            transform.SetPosition2D(x2, y2);
            if (heroInPosition != null)
            {
                heroInPosition(false);
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
            renderer.enabled = true;
            if (exitedQuake)
            {
                IgnoreInput();
                //this.proxyFSM.SendEvent("HeroCtrl-EnterQuake");
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
            float x = enterGate.transform.position.x + enterGate.entryOffset.x;
            float y = enterGate.transform.position.y + enterGate.entryOffset.y + 3f;
            transform.SetPosition2D(x, y);
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
            /*if (!enterGate.customFade)
			{
				this.gm.FadeSceneIn();
			}*/
            if (cState.facingRight)
            {
                transition_vel = new Vector2(SPEED_TO_ENTER_SCENE_HOR, SPEED_TO_ENTER_SCENE_UP);
            }
            else
            {
                transition_vel = new Vector2(-SPEED_TO_ENTER_SCENE_HOR, SPEED_TO_ENTER_SCENE_UP);
            }
            transitionState = HeroTransitionState.ENTERING_SCENE;
            transform.SetPosition2D(x, y);
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
            float num = enterGate.transform.position.x + enterGate.entryOffset.x;
            float y3 = FindGroundPointY(num + 2f, enterGate.transform.position.y, false);
            transform.SetPosition2D(num, y3);
            if (heroInPosition != null)
            {
                heroInPosition(true);
            }
            FaceRight();
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
            if (exitedSuperDashing)
            {
                IgnoreInput();
                //this.proxyFSM.SendEvent("HeroCtrl-EnterSuperDash");
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
            float num2 = enterGate.transform.position.x + enterGate.entryOffset.x;
            float y4 = FindGroundPointY(num2 - 2f, enterGate.transform.position.y, false);
            transform.SetPosition2D(num2, y4);
            if (heroInPosition != null)
            {
                heroInPosition(true);
            }
            FaceLeft();
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
            if (exitedSuperDashing)
            {
                IgnoreInput();
                //this.proxyFSM.SendEvent("HeroCtrl-EnterSuperDash");
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
            //this.animCtrl.PlayClip("Idle");
            transform.SetPosition2D(FindGroundPoint(enterGate.transform.position, false));
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

    public IEnumerator Respawn()
    {
        playerData = PlayerData.instance;
        //this.playerData.disablePause = true;
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
            transform.SetPosition2D(FindGroundPoint(spawnPoint.transform.position, false));
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
        if (playerData.respawnType == 1)
        {
            AffectedByGravity(false);
            //PlayMakerFSM benchFSM = FSMUtility.LocateFSM(spawnPoint.gameObject, "Bench Control");
            /*if (benchFSM == null)
			{
				Debug.LogError("HeroCtrl: Could not find Bench Control FSM on this spawn point, respawn type is set to Bench");
				yield break;
			}
			benchFSM.FsmVariables.GetFsmBool("RespawnResting").Value = true;*/
            yield return new WaitForEndOfFrame();
            if (heroInPosition != null)
            {
                heroInPosition(false);
            }
            //this.proxyFSM.SendEvent("HeroCtrl-Respawned");
            FinishedEnteringScene(true, false);
            Component weaverBench = spawnPoint.gameObject.GetComponent("WeaverCore.Assets.Components.WeaverBench");
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
            IgnoreInput();
            RespawnMarker component2 = spawnPoint.GetComponent<RespawnMarker>();
            if (component2)
            {
                if (component2.respawnFacingRight)
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
                Debug.LogError("Spawn point does not contain a RespawnMarker");
            }
            if (heroInPosition != null)
            {
                heroInPosition(false);
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
            FinishedEnteringScene(true, false);
        }
        //this.playerData.disablePause = false;
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
        transform.SetPosition2D(FindGroundPoint(playerData.hazardRespawnLocation, true));
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
        //GameCameras.instance.cameraFadeFSM.SendEvent("RESPAWN");
        /*float clipDuration = this.animCtrl.GetClipDuration("Hazard Respawn");
		this.animCtrl.PlayClip("Hazard Respawn");
		yield return new WaitForSeconds(clipDuration);*/
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
        //this.superDash.SendEvent("SLOPE CANCEL");
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
                //this.wallSlideVibrationPlayer.Play();
                cState.wallSliding = true;
                cState.willHardLand = false;
                cState.touchingWall = true;
                airDashed = false;
                //this.wallslideDustPrefab.enableEmission = true;
                /*var e = wallslideDustPrefab.emission;
				e.enabled = true;*/
                startWithWallslide = false;
                if (transform.localScale.x < 0f)
                {
                    wallSlidingR = true;
                    touchingWallR = true;
                    return;
                }
                wallSlidingL = true;
                touchingWallL = true;
                return;
            }
            else
            {
                if (startWithJump)
                {
                    HeroJumpNoEffect();
                    doubleJumpQueuing = false;
                    startWithJump = false;
                    return;
                }
                if (startWithFullJump)
                {
                    HeroJump();
                    doubleJumpQueuing = false;
                    startWithFullJump = false;
                    return;
                }
                if (startWithDash)
                {
                    HeroDash();
                    doubleJumpQueuing = false;
                    startWithDash = false;
                    return;
                }
                if (startWithAttack)
                {
                    DoAttack();
                    doubleJumpQueuing = false;
                    startWithAttack = false;
                    return;
                }
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
        return (!gm.isPaused && hero_state != ActorStates.airborne && !controlReqlinquished && !cState.recoiling && !cState.transitioning && !cState.hazardDeath && !cState.hazardRespawning && cState.onGround && !playerData.GetBool("disablePause") && !cState.dashing && CanInput()) || playerData.GetBool("atBench");
    }

    public void SetDamageMode(int invincibilityType)
    {
        switch (invincibilityType)
        {
            case 0:
                damageMode = DamageMode.FULL_DAMAGE;
                return;
            case 1:
                damageMode = DamageMode.HAZARD_ONLY;
                return;
            case 2:
                damageMode = DamageMode.NO_DAMAGE;
                return;
            default:
                return;
        }
    }

    public void SetDamageModeFSM(int invincibilityType)
    {
        switch (invincibilityType)
        {
            case 0:
                damageMode = DamageMode.FULL_DAMAGE;
                return;
            case 1:
                damageMode = DamageMode.HAZARD_ONLY;
                return;
            case 2:
                damageMode = DamageMode.NO_DAMAGE;
                return;
            default:
                return;
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
            return;
        }
        playerData.SetBoolSwappedArgs(false, "isInvincible");
    }

    public void StopAnimationControl()
    {
        //this.animCtrl.StopControl();
    }

    public void StartAnimationControl()
    {
        //this.animCtrl.StartControl();
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
        //this.audioCtrl.StopAllSounds();
    }

    public void PauseAudio()
    {
        //this.audioCtrl.PauseAllSounds();
    }

    public void UnPauseAudio()
    {
        //this.audioCtrl.UnPauseAllSounds();
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
            Vector2 vector = lastInputState;
            //if (this.inputHandler.inputActions.right.IsPressed)
            if (RightPressed)
            {
                move_input = lastInputState.x;
            }
            //else if (this.inputHandler.inputActions.left.IsPressed)
            else if (LeftPressed)
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
        //this.softLandingEffectPrefab.Spawn(this.transform.position);
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

    private void LookForInput()
    {
        if (acceptingInput && !gm.isPaused && isGameplayScene)
        {
            //this.move_input = this.inputHandler.inputActions.moveVector.Vector.x;
            //this.vertical_input = this.inputHandler.inputActions.moveVector.Vector.y;
            move_input = InputVector.x;
            vertical_input = InputVector.y;
            FilterInput();
            if (playerData.GetBool("hasWalljump") && CanWallSlide() && !cState.attacking)
            {
                if (touchingWallL && LeftPressed && !cState.wallSliding)
                {
                    airDashed = false;
                    doubleJumped = false;
                    //this.wallSlideVibrationPlayer.Play();
                    cState.wallSliding = true;
                    cState.willHardLand = false;
                    //this.wallslideDustPrefab.enableEmission = true;
                    /*var e = wallslideDustPrefab.emission;
					e.enabled = true;*/
                    wallSlidingL = true;
                    wallSlidingR = false;
                    FaceLeft();
                    CancelFallEffects();
                }
                if (touchingWallR && RightPressed && !cState.wallSliding)
                {
                    airDashed = false;
                    doubleJumped = false;
                    //this.wallSlideVibrationPlayer.Play();
                    cState.wallSliding = true;
                    cState.willHardLand = false;
                    //this.wallslideDustPrefab.enableEmission = true;
                    /*var e = wallslideDustPrefab.emission;
					e.enabled = true;*/
                    wallSlidingL = false;
                    wallSlidingR = true;
                    FaceRight();
                    CancelFallEffects();
                }
            }
            if (cState.wallSliding && DownWasPressed)
            {
                CancelWallsliding();
                FlipSprite();
            }
            if (wallLocked && wallJumpedL && RightPressed && wallLockSteps >= WJLOCK_STEPS_SHORT)
            {
                wallLocked = false;
            }
            if (wallLocked && wallJumpedR && LeftPressed && wallLockSteps >= WJLOCK_STEPS_SHORT)
            {
                wallLocked = false;
            }
            if (JumpWasReleased && jumpReleaseQueueingEnabled)
            {
                jumpReleaseQueueSteps = JUMP_RELEASE_QUEUE_STEPS;
                jumpReleaseQueuing = true;
            }
            if (!JumpPressed)
            {
                JumpReleased();
            }
            if (!DashPressed)
            {
                if (cState.preventDash && !cState.dashCooldown)
                {
                    cState.preventDash = false;
                }
                dashQueuing = false;
            }
            if (!AttackPressed)
            {
                attackQueuing = false;
            }
        }
    }

    private void LookForQueueInput()
    {
        if (acceptingInput && !gm.isPaused && isGameplayScene)
        {
            if (JumpWasPressed)
            {
                if (CanWallJump())
                {
                    DoWallJump();
                }
                else if (CanJump())
                {
                    HeroJump();
                }
                else if (CanDoubleJump())
                {
                    DoDoubleJump();
                }
                else if (CanInfiniteAirJump())
                {
                    CancelJump();
                    //this.audioCtrl.PlaySound(HeroSounds.JUMP);
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
            if (DashWasPressed && !ModHooks.OnDashPressed())
            {
                if (CanDash())
                {
                    HeroDash();
                }
                else
                {
                    dashQueueSteps = 0;
                    dashQueuing = true;
                }
            }
            if (AttackWasPressed)
            {
                if (CanAttack())
                {
                    DoAttack();
                }
                else
                {
                    attackQueueSteps = 0;
                    attackQueuing = true;
                }
            }
            if (JumpPressed)
            {
                if (jumpQueueSteps <= JUMP_QUEUE_STEPS && CanJump() && jumpQueuing)
                {
                    HeroJump();
                }
                else if (doubleJumpQueueSteps <= DOUBLE_JUMP_QUEUE_STEPS && CanDoubleJump() && doubleJumpQueuing)
                {
                    if (cState.onGround)
                    {
                        HeroJump();
                    }
                    else
                    {
                        DoDoubleJump();
                    }
                }
                if (CanSwim())
                {
                    if (hero_state != ActorStates.airborne)
                    {
                        SetState(ActorStates.airborne);
                    }
                    cState.swimming = true;
                }
            }
            if (DashPressed && dashQueueSteps <= DASH_QUEUE_STEPS && CanDash() && dashQueuing && !ModHooks.OnDashPressed() && CanDash())
            {
                HeroDash();
            }
            if (AttackPressed && attackQueueSteps <= ATTACK_QUEUE_STEPS && CanAttack() && attackQueuing)
            {
                DoAttack();
            }
        }
    }

    private void HeroJump()
    {
        //this.jumpEffectPrefab.Spawn(this.transform.position);
        //this.audioCtrl.PlaySound(HeroSounds.JUMP);
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
        //this.wallPuffPrefab.SetActive(true);
        //this.audioCtrl.PlaySound(HeroSounds.WALLJUMP);
        //VibrationManager.PlayVibrationClipOneShot(this.wallJumpVibration, null, false, "");
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
        walljumpSpeedDecel = (WJ_KICKOFF_SPEED - RUN_SPEED) / WJLOCK_STEPS_LONG;
        //this.dashBurst.SendEvent("CANCEL");
        cState.jumping = true;
        wallLockSteps = 0;
        wallLocked = true;
        jumpQueueSteps = 0;
        jumped_steps = 0;
    }

    private void DoDoubleJump()
    {
        /*this.dJumpWingsPrefab.SetActive(true);
		this.dJumpFlashPrefab.SetActive(true);*/
        //this.dJumpFeathers.Play();
        //VibrationManager.PlayVibrationClipOneShot(this.doubleJumpVibration, null, false, "");
        if (doubleJumpClip != null)
        {
            audioSource.PlayOneShot(doubleJumpClip, 1f);
        }
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
        //this.audioCtrl.PlaySound(HeroSounds.HARD_LANDING);
        //this.hardLandingEffectPrefab.Spawn(this.transform.position);
    }

    private void DoAttack()
    {
        ModHooks.OnDoAttack();
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
        //this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_RUN);
        //this.audioCtrl.StopSound(HeroSounds.FOOTSTEPS_WALK);
        //this.audioCtrl.PlaySound(HeroSounds.DASH);
        ResetLook();
        cState.recoiling = false;
        if (cState.wallSliding)
        {
            FlipSprite();
        }
        else if (RightPressed)
        {
            FaceRight();
        }
        else if (LeftPressed)
        {
            FaceLeft();
        }
        cState.dashing = true;
        dashQueueSteps = 0;
        //HeroActions inputActions = this.inputHandler.inputActions;
        if (DownPressed && !cState.onGround && playerData.GetBool("equippedCharm_31") && !LeftPressed && !RightPressed)
        {
            //this.dashBurst.transform.localPosition = new Vector3(-0.07f, 3.74f, 0.01f);
            //this.dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
            dashingDown = true;
        }
        else
        {
            //this.dashBurst.transform.localPosition = new Vector3(4.11f, -0.55f, 0.001f);
            //this.dashBurst.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
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
                //this.sharpShadowPrefab.SetActive(true);
            }
            else
            {
                audioSource.PlayOneShot(shadowDashClip, 1f);
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
        if (cState.onGround && !cState.shadowDashing)
        {
            //this.dashEffect = this.backDashPrefab.Spawn(this.transform.position);
            /*this.dashEffect = GameObject.Instantiate(backDashPrefab,transform.position,Quaternion.identity);
			this.dashEffect.transform.localScale = new Vector3(this.transform.localScale.x * -1f, this.transform.localScale.y, this.transform.localScale.z);*/
        }
    }

    private void StartFallRumble()
    {
        fallRumble = true;
        //this.audioCtrl.PlaySound(HeroSounds.FALLING);
        //GameCameras.instance.cameraShakeFSM.Fsm.Variables.FindFsmBool("RumblingFall").Value = true;
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
            //this.animCtrl.UpdateState(newState);
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
            if (/*this.gm.startedOnThisScene || */sceneEntryGate == null)
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
            //this.playerData.disablePause = true;
            boundsChecking = false;
            StopTilemapTest();
            cState.onConveyor = false;
            cState.onConveyorV = false;
            rb2d.velocity = new Vector2(0f, 0f);
            CancelRecoilHorizontal();
            string currentMapZone = gm.GetCurrentMapZone();
            if (currentMapZone == "DREAM_WORLD" || currentMapZone == "GODS_GLORY")
            {
                RelinquishControl();
                StopAnimationControl();
                AffectedByGravity(false);
                playerData.isInvincible = true;
                ResetHardLandingTimer();
                renderer.enabled = false;
                //this.heroDeathPrefab.SetActive(true);
            }
            else
            {
                /*if (this.playerData.permadeathMode == 1)
				{
					this.playerData.permadeathMode = 2;
				}*/
                AffectedByGravity(false);
                HeroBox.inactive = true;
                rb2d.isKinematic = true;
                SetState(ActorStates.no_input);
                cState.dead = true;
                ResetMotion();
                ResetHardLandingTimer();
                renderer.enabled = false;
                gameObject.layer = 2;
                //this.heroDeathPrefab.SetActive(true);
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
            //this.playerData.disablePause = true;
            StopTilemapTest();
            SetState(ActorStates.no_input);
            cState.hazardDeath = true;
            ResetMotion();
            ResetHardLandingTimer();
            AffectedByGravity(false);
            renderer.enabled = false;
            gameObject.layer = 2;
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
            StartCoroutine(gm.PlayerDeadFromHazard(0f));
        }
        yield break;
    }

    private IEnumerator StartRecoil(CollisionSide impactSide, bool spawnDamageEffect, int damageAmount)
    {
        if (!cState.recoiling)
        {
            //this.playerData.disablePause = true;
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
                //this.damageEffectFSM.SendEvent("DAMAGE");
                if (damageAmount > 1)
                {
                    //UnityEngine.Object.Instantiate<GameObject>(this.takeHitDoublePrefab, this.transform.position, this.transform.rotation);
                }
            }
            if (playerData.GetBool("equippedCharm_4"))
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
            //this.playerData.disablePause = false;
        }
        yield break;
    }

    private IEnumerator Invulnerable(float duration)
    {
        cState.invulnerable = true;
        yield return new WaitForSeconds(DAMAGE_FREEZE_DOWN);
        //this.invPulse.startInvulnerablePulse();
        yield return new WaitForSeconds(duration);
        //this.invPulse.stopInvulnerablePulse();
        cState.invulnerable = false;
        cState.recoiling = false;
        yield break;
    }

    private IEnumerator FirstFadeIn()
    {
        yield return new WaitForSeconds(0.25f);
        //this.gm.FadeSceneIn();
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
                //this.proxyFSM.SendEvent("HeroCtrl-LeftGround");
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
                    return;
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
            if ((vector.y < -60f || vector.y > gm.sceneHeight + 60f || vector.x < -60f || vector.x > gm.sceneWidth + 60f) && !cState.dead)
            {
                bool flag = boundsChecking;
            }
        }
    }

    private void ConfirmOutOfBounds()
    {
        if (boundsChecking)
        {
            Debug.Log("Confirming out of bounds");
            Vector2 vector = transform.position;
            if (vector.y < -60f || vector.y > gm.sceneHeight + 60f || vector.x < -60f || vector.x > gm.sceneWidth + 60f)
            {
                if (!cState.dead)
                {
                    rb2d.velocity = Vector2.zero;
                    Debug.LogFormat("Pos: {0} Transition State: {1}", new object[]
                    {
                        transform.position,
                        transitionState
                    });
                    return;
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
                    return;
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

    private void CancelWallsliding()
    {
        //this.wallslideDustPrefab.enableEmission = false;
        /*var e = wallslideDustPrefab.emission;
		e.enabled = false;*/
        //this.wallSlideVibrationPlayer.Stop();
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
            //this.slashComponent.CancelAttack();
            ResetAttacks();
        }
    }

    private void CancelAttack()
    {
        if (cState.attacking)
        {
            //this.slashComponent.CancelAttack();
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
        //this.audioCtrl.StopSound(HeroSounds.FALLING);
        //GameCameras.instance.cameraShakeFSM.Fsm.Variables.FindFsmBool("RumblingFall").Value = false;
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
                //this.softLandingEffectPrefab.Spawn(this.transform.position);
                //GameObject.Instantiate(softLandingEffectPrefab, transform.position,Quaternion.identity);
                //VibrationManager.PlayVibrationClipOneShot(this.softLandVibration, null, false, "");
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
        /*if (this.dJumpWingsPrefab.activeSelf)
		{
			this.dJumpWingsPrefab.SetActive(false);
		}*/
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
        //this.animCtrl.FinishedDash();
        //this.proxyFSM.SendEvent("HeroCtrl-DashEnd");
        if (cState.touchingWall && !cState.onGround && (playerData.GetBool("hasWalljump") & (touchingWallL || touchingWallR)))
        {
            //this.wallslideDustPrefab.enableEmission = true;
            /*var e = wallslideDustPrefab.emission;
			e.enabled = true;*/
            //this.wallSlideVibrationPlayer.Play();
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
        move_input = ((acceptingInput || preventRunDip) ? InputVector.x : 0f);
        cState.touchingWall = false;
        if (CheckTouchingGround())
        {
            cState.onGround = true;
            SetState(ActorStates.grounded);
            ResetAirMoves();
            if (enteringVertically)
            {
                SpawnSoftLandingPrefab();
                //this.animCtrl.playLanding = true;
                enteringVertically = false;
            }
        }
        else
        {
            cState.onGround = false;
            SetState(ActorStates.airborne);
        }
        //this.animCtrl.UpdateState(this.hero_state);
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
        if (JumpPressed)
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
                Debug.LogFormat("TERRAIN INGRESS {0} at {1} Jumping: {2}", new object[]
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
                    transform.SetPosition2D(positionHistory[1]);
                }
                if (cState.superDashing)
                {
                    transform.SetPosition2D(raycastHit2D.point);
                    //this.superDash.SendEvent("HIT WALL");
                }
                if (cState.spellQuake)
                {
                    //this.spellControl.SendEvent("Hero Landed");
                    transform.SetPosition2D(positionHistory[1]);
                }
                tilemapTestActive = false;
                tilemapTestCoroutine = base.StartCoroutine(TilemapTestPause());
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
            base.StopCoroutine(tilemapTestCoroutine);
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
                if (playerData.GetBool("equippedCharm_18"))
                {
                    num3 += 0.2f;
                }
                if (playerData.GetBool("equippedCharm_13"))
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
                        //this.nailTerrainImpactEffectPrefab.Spawn(raycastHit2D.point, Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f)));
                        if (nailTerrainImpactEffectPrefab != null)
                        {
                            GameObject.Instantiate(nailTerrainImpactEffectPrefab, raycastHit2D.point, Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f)));
                        }
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

    public bool CheckNearRoof()
    {
        Vector2 origin = col2d.bounds.max;
        Vector2 origin2 = new Vector2(col2d.bounds.min.x, col2d.bounds.max.y);
        new Vector2(col2d.bounds.center.x, col2d.bounds.max.y);
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
        Debug.DrawRay(vector, Vector2.down, Color.yellow);
        Debug.DrawRay(vector2, Vector2.down, Color.yellow);
        Debug.DrawRay(vector3, Vector2.down, Color.yellow);
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
        return damageMode != DamageMode.NO_DAMAGE && transitionState == HeroTransitionState.WAITING_TO_TRANSITION && !cState.invulnerable && !cState.recoiling && !playerData.GetBool("isInvincible") && !cState.dead && !cState.hazardDeath/* && !BossSceneController.IsTransitioning*/;
    }

    private bool CanWallJump()
    {
        return playerData.GetBool("hasWalljump") && !cState.touchingNonSlider && (cState.wallSliding || (cState.touchingWall && !cState.onGround));
    }

    private bool ShouldHardLand(Collision2D collision)
    {
        return /*!collision.gameObject.GetComponent<NoHardLanding>() &&*/ cState.willHardLand && !inAcid && hero_state != ActorStates.hard_landing;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (cState.superDashing && (CheckStillTouchingWall(CollisionSide.left, false) || CheckStillTouchingWall(CollisionSide.right, false)))
        {
            //this.superDash.SendEvent("HIT WALL");
        }
        if ((collision.gameObject.layer == 8 || collision.gameObject.CompareTag("HeroWalkable")) && CheckTouchingGround())
        {
            //this.proxyFSM.SendEvent("HeroCtrl-Landed");
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
                        return;
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
            //this.superDash.SendEvent("HIT WALL");
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
                        return;
                    }
                    if (hero_state != ActorStates.hard_landing && hero_state != ActorStates.dash_landing && cState.falling)
                    {
                        BackOnGround();
                        return;
                    }
                }
                else if (cState.jumping || cState.falling)
                {
                    cState.onGround = false;
                    //this.proxyFSM.SendEvent("HeroCtrl-LeftGround");
                    SetState(ActorStates.airborne);
                    return;
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
        if (touchingWallL && !CheckStillTouchingWall(CollisionSide.left, false))
        {
            cState.touchingWall = false;
            touchingWallL = false;
        }
        if (touchingWallR && !CheckStillTouchingWall(CollisionSide.right, false))
        {
            cState.touchingWall = false;
            touchingWallR = false;
        }
        if (hero_state != ActorStates.no_input && !cState.recoiling && collision.gameObject.layer == 8 && !CheckTouchingGround())
        {
            if (!cState.jumping && !fallTrailGenerated && cState.onGround)
            {
                if (playerData.GetInt("environmentType") != 6)
                {
                    //this.fsm_fallTrail.SendEvent("PLAY");
                }
                fallTrailGenerated = true;
            }
            cState.onGround = false;
            //this.proxyFSM.SendEvent("HeroCtrl-LeftGround");
            SetState(ActorStates.airborne);
            if (cState.wasOnGround)
            {
                ledgeBufferSteps = LEDGE_BUFFER_STEPS;
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
        playerData = PlayerData.instance;
        //this.animCtrl = base.GetComponent<HeroAnimationController>();
        rb2d = base.GetComponent<Rigidbody2D>();
        col2d = base.GetComponent<Collider2D>();
        transform = base.GetComponent<Transform>();
        renderer = base.GetComponent<Renderer>();
        //this.audioCtrl = base.GetComponent<HeroAudioController>();
        //this.inputHandler = this.gm.GetComponent<InputHandler>();
        //this.proxyFSM = FSMUtility.LocateFSM(base.gameObject, "ProxyFSM");
        audioSource = base.GetComponent<AudioSource>();
        if (!footStepsRunAudioSource)
        {
            footStepsRunAudioSource = transform.Find("Sounds/FootstepsRun").GetComponent<AudioSource>();
        }
        if (!footStepsWalkAudioSource)
        {
            footStepsWalkAudioSource = transform.Find("Sounds/FootstepsWalk").GetComponent<AudioSource>();
        }
        //this.invPulse = base.GetComponent<InvulnerablePulse>();
        //this.spriteFlash = base.GetComponent<SpriteFlash>();
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
            return;
        }
        if (vertical_input < -0.5f)
        {
            vertical_input = -1f;
            return;
        }
        vertical_input = 0f;
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
            Debug.LogErrorFormat("FindGroundPoint: Could not find ground point below {0}, check reference position is not too high (more than {1} tiles).", new object[]
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
            Debug.LogErrorFormat("FindGroundPoint: Could not find ground point below ({0},{1}), check reference position is not too high (more than {2} tiles).", new object[]
            {
                x,
                y,
                num
            });
        }
        return raycastHit2D.point.y + col2d.bounds.extents.y - col2d.offset.y + 0.01f;
    }

    public HeroController()
    {
        JUMP_QUEUE_STEPS = 2;
        JUMP_RELEASE_QUEUE_STEPS = 2;
        DOUBLE_JUMP_QUEUE_STEPS = 10;
        ATTACK_QUEUE_STEPS = 5;
        DELAY_BEFORE_ENTER = 0.1f;
        LOOK_DELAY = 0.85f;
        LOOK_ANIM_DELAY = 0.25f;
        DEATH_WAIT = 2.85f;
        HAZARD_DEATH_CHECK_TIME = 3f;
        FLOATING_CHECK_TIME = 0.18f;
        NAIL_TERRAIN_CHECK_TIME = 0.12f;
        BUMP_VELOCITY = 4f;
        BUMP_VELOCITY_DASH = 5f;
        LANDING_BUFFER_STEPS = 5;
        LEDGE_BUFFER_STEPS = 2;
        HEAD_BUMP_STEPS = 3;
        MANTIS_CHARM_SCALE = 1.35f;
        FIND_GROUND_POINT_DISTANCE = 10f;
        FIND_GROUND_POINT_DISTANCE_EXT = 50f;
        controller_deadzone = 0.2f;
        isHeroInPosition = true;
        oldPos = Vector2.zero;
    }

    public void orig_StartMPDrain(float time)
    {
        drainMP = true;
        drainMP_timer = 0f;
        MP_drained = 0f;
        drainMP_time = time;
        focusMP_amount = playerData.GetInt("focusMP_amount");
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
            if (runMsgSent && rb2d.velocity.x > -0.1f && rb2d.velocity.x < 0.1f)
            {
                //this.runEffect.GetComponent<PlayMakerFSM>().SendEvent("RUN STOP");
                runEffect.transform.SetParent(null, true);
                runMsgSent = false;
            }
            if (!runMsgSent && (rb2d.velocity.x < -0.1f || rb2d.velocity.x > 0.1f))
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
            if (runMsgSent)
            {
                //this.runEffect.GetComponent<PlayMakerFSM>().SendEvent("RUN STOP");
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
                    //this.fsm_thornCounter.SendEvent("THORN COUNTER");
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
                    //this.animCtrl.StopAttack();
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
                    if (UpPressed/* || this.inputHandler.inputActions.rs_up.IsPressed*/)
                    {
                        cState.lookingDown = false;
                        cState.lookingDownAnim = false;
                        if (lookDelayTimer >= LOOK_DELAY || (/*this.inputHandler.inputActions.rs_up.IsPressed*/false && !cState.jumping && !cState.dashing))
                        {
                            cState.lookingUp = true;
                        }
                        else
                        {
                            lookDelayTimer += Time.deltaTime;
                        }
                        if (lookDelayTimer >= LOOK_ANIM_DELAY || /*this.inputHandler.inputActions.rs_up.IsPressed*/false)
                        {
                            cState.lookingUpAnim = true;
                        }
                        else
                        {
                            cState.lookingUpAnim = false;
                        }
                    }
                    else if (DownPressed || /*this.inputHandler.inputActions.rs_down.IsPressed*/false)
                    {
                        cState.lookingUp = false;
                        cState.lookingUpAnim = false;
                        if (lookDelayTimer >= LOOK_DELAY || (/*this.inputHandler.inputActions.rs_down.IsPressed*/false && !cState.jumping && !cState.dashing))
                        {
                            cState.lookingDown = true;
                        }
                        else
                        {
                            lookDelayTimer += Time.deltaTime;
                        }
                        if (lookDelayTimer >= LOOK_ANIM_DELAY || /*this.inputHandler.inputActions.rs_down.IsPressed*/false)
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
                //this.gm.soulOrb_fsm.SendEvent("MP DRAIN");
                if (MP_drained == focusMP_amount)
                {
                    MP_drained -= drainMP_time;
                    //this.proxyFSM.SendEvent("HeroCtrl-FocusCompleted");
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
                    //this.audioCtrl.PlaySound(HeroSounds.WALLSLIDE);
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
            //this.audioCtrl.StopSound(HeroSounds.WALLSLIDE);
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
                //this.spriteFlash.FlashShadowRecharge();
                //Component flasher = GetComponent("WeaverCore.Components.SpriteFlasher");
                Component[] flashers = GetComponentsInChildren(GetFlasherType());
                if (flashers != null)
                {
                    foreach (var flasher in flashers)
                    {
                        flasher.SendMessage("FlashShadowRecharge");
                    }
                    //flasher.FlashShadowRecharge();
                }
            }
        }
        preventCastByDialogueEndTimer -= Time.deltaTime;
        if (!gm.isPaused)
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
        if (gm.isPaused && !AttackPressed)
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
        float num;
        if (playerData.GetBool("equippedCharm_16") && cState.shadowDashing)
        {
            num = DASH_SPEED_SHARP;
        }
        else
        {
            num = DASH_SPEED;
        }
        Vector2 result;
        if (dashingDown)
        {
            result = new Vector2(0f, -num);
        }
        else if (cState.facingRight)
        {
            if (CheckForBump(CollisionSide.right))
            {
                result = new Vector2(num, (!cState.onGround) ? 5f : 4f);
            }
            else
            {
                result = new Vector2(num, 0f);
            }
        }
        else if (CheckForBump(CollisionSide.left))
        {
            result = new Vector2(-num, (!cState.onGround) ? 5f : 4f);
        }
        else
        {
            result = new Vector2(-num, 0f);
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
            return;
        }
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
                rb2d.velocity = new Vector2(num, cState.onGround ? BUMP_VELOCITY : BUMP_VELOCITY_DASH);
            }
            else
            {
                rb2d.velocity = new Vector2(num, 0f);
            }
        }
        else if (CheckForBump(CollisionSide.left))
        {
            rb2d.velocity = new Vector2(-num, cState.onGround ? BUMP_VELOCITY : BUMP_VELOCITY_DASH);
        }
        else
        {
            rb2d.velocity = new Vector2(-num, 0f);
        }
        dash_timer += Time.deltaTime;
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
            MaxHealth();
        }
        else
        {
            playerData.SetIntSwappedArgs(playerData.GetInt("maxHealthBase"), "maxHealth");
            MaxHealth();
        }
        if (playerData.GetBool("equippedCharm_27"))
        {
            playerData.SetIntSwappedArgs((int)(playerData.GetInt("maxHealth") * 1.4f), "joniHealthBlue");
            playerData.SetIntSwappedArgs(1, "maxHealth");
            MaxHealth();
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
        //this.playerData.UpdateBlueHealth();
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
            base.StartCoroutine(CheckForTerrainThunk(AttackDirection.upward));
            return;
        }
        if (vertical_input >= -Mathf.Epsilon)
        {
            Attack(AttackDirection.normal);
            base.StartCoroutine(CheckForTerrainThunk(AttackDirection.normal));
            return;
        }
        if (hero_state != ActorStates.idle && hero_state != ActorStates.running)
        {
            Attack(AttackDirection.downward);
            base.StartCoroutine(CheckForTerrainThunk(AttackDirection.downward));
            return;
        }
        Attack(AttackDirection.normal);
        base.StartCoroutine(CheckForTerrainThunk(AttackDirection.normal));
    }

    private bool verboseMode;

    public HeroType heroType;

    public float RUN_SPEED;

    public float RUN_SPEED_CH;

    public float RUN_SPEED_CH_COMBO;

    public float WALK_SPEED;

    public float UNDERWATER_SPEED;

    public float JUMP_SPEED;

    public float JUMP_SPEED_UNDERWATER;

    public float MIN_JUMP_SPEED;

    public int JUMP_STEPS;

    public int JUMP_STEPS_MIN;

    public int JUMP_TIME;

    public int DOUBLE_JUMP_STEPS;

    public int WJLOCK_STEPS_SHORT;

    public int WJLOCK_STEPS_LONG;

    public float WJ_KICKOFF_SPEED;

    public int WALL_STICKY_STEPS;

    public float DASH_SPEED;

    public float DASH_SPEED_SHARP;

    public float DASH_TIME;

    public int DASH_QUEUE_STEPS;

    public float BACK_DASH_SPEED;

    public float BACK_DASH_TIME;

    public float SHADOW_DASH_SPEED;

    public float SHADOW_DASH_TIME;

    public float SHADOW_DASH_COOLDOWN;

    public float SUPER_DASH_SPEED;

    public float DASH_COOLDOWN;

    public float DASH_COOLDOWN_CH;

    public float BACKDASH_COOLDOWN;

    public float WALLSLIDE_SPEED;

    public float WALLSLIDE_DECEL;

    public float NAIL_CHARGE_TIME_DEFAULT;

    public float NAIL_CHARGE_TIME_CHARM;

    public float CYCLONE_HORIZONTAL_SPEED;

    public float SWIM_ACCEL;

    public float SWIM_MAX_SPEED;

    public float TIME_TO_ENTER_SCENE_BOT;

    public float TIME_TO_ENTER_SCENE_HOR;

    public float SPEED_TO_ENTER_SCENE_HOR;

    public float SPEED_TO_ENTER_SCENE_UP;

    public float SPEED_TO_ENTER_SCENE_DOWN;

    public float DEFAULT_GRAVITY;

    public float UNDERWATER_GRAVITY;

    public float ATTACK_DURATION;

    public float ATTACK_DURATION_CH;

    public float ALT_ATTACK_RESET;

    public float ATTACK_RECOVERY_TIME;

    public float ATTACK_COOLDOWN_TIME;

    public float ATTACK_COOLDOWN_TIME_CH;

    public float BOUNCE_TIME;

    public float BOUNCE_SHROOM_TIME;

    public float BOUNCE_VELOCITY;

    public float SHROOM_BOUNCE_VELOCITY;

    public float RECOIL_HOR_TIME;

    public float RECOIL_HOR_VELOCITY;

    public float RECOIL_HOR_VELOCITY_LONG;

    public float RECOIL_HOR_STEPS;

    public float RECOIL_DOWN_VELOCITY;

    public float RUN_PUFF_TIME;

    public float BIG_FALL_TIME;

    public float HARD_LANDING_TIME;

    public float DOWN_DASH_TIME;

    public float MAX_FALL_VELOCITY;

    public float MAX_FALL_VELOCITY_UNDERWATER;

    public float RECOIL_DURATION;

    public float RECOIL_DURATION_STAL;

    public float RECOIL_VELOCITY;

    public float DAMAGE_FREEZE_DOWN;

    public float DAMAGE_FREEZE_WAIT;

    public float DAMAGE_FREEZE_UP;

    public float INVUL_TIME;

    public float INVUL_TIME_STAL;

    public float INVUL_TIME_PARRY;

    public float INVUL_TIME_QUAKE;

    public float INVUL_TIME_CYCLONE;

    public float CAST_TIME;

    public float CAST_RECOIL_TIME;

    public float CAST_RECOIL_VELOCITY;

    public float WALLSLIDE_CLIP_DELAY;

    public int GRUB_SOUL_MP;

    public int GRUB_SOUL_MP_COMBO;

    private int JUMP_QUEUE_STEPS;

    private int JUMP_RELEASE_QUEUE_STEPS;

    private int DOUBLE_JUMP_QUEUE_STEPS;

    private int ATTACK_QUEUE_STEPS;

    private float DELAY_BEFORE_ENTER;

    private float LOOK_DELAY;

    private float LOOK_ANIM_DELAY;

    private float DEATH_WAIT;

    private float HAZARD_DEATH_CHECK_TIME;

    private float FLOATING_CHECK_TIME;

    private float NAIL_TERRAIN_CHECK_TIME;

    private float BUMP_VELOCITY;

    private float BUMP_VELOCITY_DASH;

    private int LANDING_BUFFER_STEPS;

    private int LEDGE_BUFFER_STEPS;

    private int HEAD_BUMP_STEPS;

    private float MANTIS_CHARM_SCALE;

    private float FIND_GROUND_POINT_DISTANCE;

    private float FIND_GROUND_POINT_DISTANCE_EXT;

    public ActorStates hero_state;

    public ActorStates prev_hero_state;

    public HeroTransitionState transitionState;

    public DamageMode damageMode;

    public float move_input;

    public float vertical_input;

    public float controller_deadzone;

    public Vector2 current_velocity;

    private bool isGameplayScene;

    public bool isEnteringFirstLevel;

    public Vector2 slashOffset;

    public Vector2 upSlashOffset;

    public Vector2 downwardSlashOffset;

    public Vector2 spell1Offset;

    private int jump_steps;

    private int jumped_steps;

    private int doubleJump_steps;

    private float dash_timer;

    private float back_dash_timer;

    private float shadow_dash_timer;

    private float attack_time;

    private float attack_cooldown;

    private Vector2 transition_vel;

    private float altAttackTime;

    private float lookDelayTimer;

    private float bounceTimer;

    private float recoilHorizontalTimer;

    private float runPuffTimer;

    private float hardLandingTimer;

    private float dashLandingTimer;

    private float recoilTimer;

    private int recoilSteps;

    private int landingBufferSteps;

    private int dashQueueSteps;

    private bool dashQueuing;

    private float shadowDashTimer;

    private float dashCooldownTimer;

    private float nailChargeTimer;

    private int wallLockSteps;

    private float wallslideClipTimer;

    private float hardLandFailSafeTimer;

    private float hazardDeathTimer;

    private float floatingBufferTimer;

    private float attackDuration;

    public float parryInvulnTimer;

    [Space(6f)]
    [Header("Slash Prefabs")]
    public GameObject slashPrefab;

    public GameObject slashAltPrefab;

    public GameObject upSlashPrefab;

    public GameObject downSlashPrefab;

    public GameObject wallSlashPrefab;

    /*public NailSlash normalSlash;

	public NailSlash alternateSlash;

	public NailSlash upSlash;

	public NailSlash downSlash;

	public NailSlash wallSlash;

	public PlayMakerFSM normalSlashFsm;

	public PlayMakerFSM alternateSlashFsm;

	public PlayMakerFSM upSlashFsm;

	public PlayMakerFSM downSlashFsm;

	public PlayMakerFSM wallSlashFsm;*/

    [Space(6f)]
    [Header("Effect Prefabs")]
    public GameObject nailTerrainImpactEffectPrefab;

    //public GameObject spell1Prefab;

    //public GameObject takeHitPrefab;

    //public GameObject takeHitDoublePrefab;

    //public GameObject softLandingEffectPrefab;

    //public GameObject hardLandingEffectPrefab;

    //public GameObject runEffectPrefab;

    //public GameObject backDashPrefab;

    //public GameObject jumpEffectPrefab;

    //public GameObject jumpTrailPrefab;

    //public GameObject fallEffectPrefab;

    //public ParticleSystem wallslideDustPrefab;

    //public GameObject artChargeEffect;

    //public GameObject artChargedEffect;

    //public GameObject artChargedFlash;

    //public tk2dSpriteAnimator artChargedEffectAnim;

    //public GameObject shadowdashBurstPrefab;

    //public GameObject shadowdashDownBurstPrefab;

    //public GameObject dashParticlesPrefab;

    //public GameObject shadowdashParticlesPrefab;

    //public GameObject shadowRingPrefab;

    //public GameObject shadowRechargePrefab;

    //public GameObject dJumpWingsPrefab;

    //public GameObject dJumpFlashPrefab;

    //public ParticleSystem dJumpFeathers;

    //public GameObject wallPuffPrefab;

    //public GameObject sharpShadowPrefab;

    /*public GameObject grubberFlyBeamPrefabL;

	public GameObject grubberFlyBeamPrefabR;

	public GameObject grubberFlyBeamPrefabU;

	public GameObject grubberFlyBeamPrefabD;

	public GameObject grubberFlyBeamPrefabL_fury;

	public GameObject grubberFlyBeamPrefabR_fury;

	public GameObject grubberFlyBeamPrefabU_fury;

	public GameObject grubberFlyBeamPrefabD_fury;*/

    //public GameObject carefreeShield;

    [Space(6f)]
    [Header("Hero Death")]
    public GameObject corpsePrefab;

    public GameObject spikeDeathPrefab;

    public GameObject acidDeathPrefab;

    public GameObject lavaDeathPrefab;

    public GameObject heroDeathPrefab;

    [Space(6f)]
    [Header("Hero Other")]
    public GameObject cutscenePrefab;

    private GameManager gm;

    private Rigidbody2D rb2d;

    private Collider2D col2d;

    private Renderer renderer;

    private new Transform transform;

    //private HeroAnimationController animCtrl;

    public HeroControllerStates cState;

    public PlayerData playerData;

    //private HeroAudioController audioCtrl;

    private AudioSource audioSource;

    //[HideInInspector]
    //public UIManager ui;

    //private InputHandler inputHandler;

    //public PlayMakerFSM damageEffectFSM;

    private ParticleSystem dashParticleSystem;

    //private InvulnerablePulse invPulse;

    //private SpriteFlash spriteFlash;

    public AudioSource footStepsRunAudioSource;

    public AudioSource footStepsWalkAudioSource;

    private float prevGravityScale;

    private Vector2 recoilVector;

    private Vector2 lastInputState;

    public GatePosition gatePosition;

    private bool runMsgSent;

    private bool hardLanded;

    private bool fallRumble;

    public bool acceptingInput;

    private bool fallTrailGenerated;

    private bool drainMP;

    private float drainMP_timer;

    private float drainMP_time;

    private float MP_drained;

    private float drainMP_seconds;

    private float focusMP_amount;

    private float dashBumpCorrection;

    public bool controlReqlinquished;

    public bool enterWithoutInput;

    public bool lookingUpAnim;

    public bool lookingDownAnim;

    public bool carefreeShieldEquipped;

    private int hitsSinceShielded;

    //private EndBeta endBeta;

    private int jumpQueueSteps;

    private bool jumpQueuing;

    private int doubleJumpQueueSteps;

    private bool doubleJumpQueuing;

    private int jumpReleaseQueueSteps;

    private bool jumpReleaseQueuing;

    private int attackQueueSteps;

    private bool attackQueuing;

    public bool touchingWallL;

    public bool touchingWallR;

    public bool wallSlidingL;

    public bool wallSlidingR;

    private bool airDashed;

    public bool dashingDown;

    public bool wieldingLantern;

    private bool startWithWallslide;

    private bool startWithJump;

    private bool startWithFullJump;

    private bool startWithDash;

    private bool startWithAttack;

    private bool nailArt_cyclone;

    private bool wallSlashing;

    private bool doubleJumped;

    public bool inAcid;

    private bool wallJumpedR;

    private bool wallJumpedL;

    public bool wallLocked;

    private float currentWalljumpSpeed;

    private float walljumpSpeedDecel;

    private int wallUnstickSteps;

    private bool recoilLarge;

    public float conveyorSpeed;

    public float conveyorSpeedV;

    private bool enteringVertically;

    private bool playingWallslideClip;

    private bool playedMantisClawClip;

    public bool exitedSuperDashing;

    public bool exitedQuake;

    private bool fallCheckFlagged;

    private int ledgeBufferSteps;

    private int headBumpSteps;

    private float nailChargeTime;

    public bool takeNoDamage;

    private bool joniBeam;

    public bool fadedSceneIn;

    private bool stopWalkingOut;

    private bool boundsChecking;

    private bool blockerFix;

    [SerializeField]
    private Vector2[] positionHistory;

    private bool tilemapTestActive;

    private Vector2 groundRayOriginC;

    private Vector2 groundRayOriginL;

    private Vector2 groundRayOriginR;

    private Coroutine takeDamageCoroutine;

    private Coroutine tilemapTestCoroutine;

    public AudioClip footstepsRunDust;

    public AudioClip footstepsRunGrass;

    public AudioClip footstepsRunBone;

    public AudioClip footstepsRunSpa;

    public AudioClip footstepsRunMetal;

    public AudioClip footstepsRunWater;

    public AudioClip footstepsWalkDust;

    public AudioClip footstepsWalkGrass;

    public AudioClip footstepsWalkBone;

    public AudioClip footstepsWalkSpa;

    public AudioClip footstepsWalkMetal;

    public AudioClip nailArtCharge;

    public AudioClip nailArtChargeComplete;

    public AudioClip blockerImpact;

    public AudioClip shadowDashClip;

    public AudioClip sharpShadowClip;

    public AudioClip doubleJumpClip;

    public AudioClip mantisClawClip;

    private GameObject slash;

    //private NailSlash slashComponent;

    //private PlayMakerFSM slashFsm;

    private GameObject runEffect;

    private GameObject backDash;

    private GameObject jumpEffect;

    private GameObject fallEffect;

    private GameObject dashEffect;

    //private GameObject grubberFlyBeam;

    private GameObject hazardCorpe;

    //public PlayMakerFSM vignetteFSM;

    //public SpriteRenderer heroLight;

    //public SpriteRenderer vignette;

    //public PlayMakerFSM dashBurst;

    //public PlayMakerFSM superDash;

    //public PlayMakerFSM fsm_thornCounter;

    //public PlayMakerFSM spellControl;

    //public PlayMakerFSM fsm_fallTrail;

    //public PlayMakerFSM fsm_orbitShield;

    /*public VibrationData softLandVibration;

	public VibrationData wallJumpVibration;

	public VibrationPlayer wallSlideVibrationPlayer;

	public VibrationData dashVibration;

	public VibrationData shadowDashVibration;

	public VibrationData doubleJumpVibration;*/

    public bool isHeroInPosition;

    private bool jumpReleaseQueueingEnabled;

    private static HeroController _instance;

    private const float PreventCastByDialogueEndDuration = 0.3f;

    private float preventCastByDialogueEndTimer;

    private Vector2 oldPos;

    public delegate void HeroInPosition(bool forceDirect);

    public delegate void TakeDamageEvent();

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

	private void CancelDoubleJump()
	{
		this.cState.doubleJumping = false;
		this.doubleJump_steps = 0;
	}

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

	private void CancelBackDash()
	{
		this.cState.backDashing = false;
		this.back_dash_timer = 0f;
	}

	private void CancelDownAttack()
	{
		if (this.cState.downAttacking)
		{
			this.slashComponent.CancelAttack();
			this.ResetAttacks();
		}
	}

	private void CancelAttack()
	{
		if (this.cState.attacking)
		{
			this.slashComponent.CancelAttack();
			this.ResetAttacks();
		}
	}

	private void CancelBounce()
	{
		this.cState.bouncing = false;
		this.cState.shroomBouncing = false;
		this.bounceTimer = 0f;
	}

	private void CancelRecoilHorizontal()
	{
		this.cState.recoilingLeft = false;
		this.cState.recoilingRight = false;
		this.recoilSteps = 0;
	}

	private void CancelDamageRecoil()
	{
		this.cState.recoiling = false;
		this.recoilTimer = 0f;
		this.ResetMotion();
		this.AffectedByGravity(true);
		this.SetDamageMode(DamageMode.FULL_DAMAGE);
	}

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

