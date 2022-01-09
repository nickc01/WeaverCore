using System;
using System.Reflection;
using UnityEngine;

[Serializable]
public class HeroControllerStates
{
	public HeroControllerStates()
	{
		this.facingRight = false;
		this.Reset();
	}

	public bool GetState(string stateName)
	{
		FieldInfo field = base.GetType().GetField(stateName);
		if (field != null)
		{
			return (bool)field.GetValue(HeroController.instance.cState);
		}
		Debug.LogError("HeroControllerStates: Could not find bool named" + stateName + "in cState");
		return false;
	}

	public void SetState(string stateName, bool value)
	{
		FieldInfo field = base.GetType().GetField(stateName);
		if (field != null)
		{
			try
			{
				field.SetValue(HeroController.instance.cState, value);
				return;
			}
			catch (Exception ex)
			{
				string str = "Failed to set cState: ";
				Exception ex2 = ex;
				Debug.LogError(str + ((ex2 != null) ? ex2.ToString() : null));
				return;
			}
		}
		Debug.LogError("HeroControllerStates: Could not find bool named" + stateName + "in cState");
	}

	public void Reset()
	{
		this.onGround = false;
		this.jumping = false;
		this.falling = false;
		this.dashing = false;
		this.backDashing = false;
		this.touchingWall = false;
		this.wallSliding = false;
		this.transitioning = false;
		this.attacking = false;
		this.lookingUp = false;
		this.lookingDown = false;
		this.altAttack = false;
		this.upAttacking = false;
		this.downAttacking = false;
		this.bouncing = false;
		this.dead = false;
		this.hazardDeath = false;
		this.willHardLand = false;
		this.recoiling = false;
		this.recoilFrozen = false;
		this.invulnerable = false;
		this.casting = false;
		this.castRecoiling = false;
		this.preventDash = false;
		this.preventBackDash = false;
		this.dashCooldown = false;
		this.backDashCooldown = false;
	}

	public bool facingRight;

	public bool onGround;

	public bool jumping;

	public bool wallJumping;

	public bool doubleJumping;

	public bool nailCharging;

	public bool shadowDashing;

	public bool swimming;

	public bool falling;

	public bool dashing;

	public bool superDashing;

	public bool superDashOnWall;

	public bool backDashing;

	public bool touchingWall;

	public bool wallSliding;

	public bool transitioning;

	public bool attacking;

	public bool lookingUp;

	public bool lookingDown;

	public bool lookingUpAnim;

	public bool lookingDownAnim;

	public bool altAttack;

	public bool upAttacking;

	public bool downAttacking;

	public bool bouncing;

	public bool shroomBouncing;

	public bool recoilingRight;

	public bool recoilingLeft;

	public bool dead;

	public bool hazardDeath;

	public bool hazardRespawning;

	public bool willHardLand;

	public bool recoilFrozen;

	public bool recoiling;

	public bool invulnerable;

	public bool casting;

	public bool castRecoiling;

	public bool preventDash;

	public bool preventBackDash;

	public bool dashCooldown;

	public bool backDashCooldown;

	public bool nearBench;

	public bool inWalkZone;

	public bool isPaused;

	public bool onConveyor;

	public bool onConveyorV;

	public bool inConveyorZone;

	public bool spellQuake;

	public bool freezeCharge;

	public bool focusing;

	public bool inAcid;

	public bool slidingLeft;

	public bool slidingRight;

	public bool touchingNonSlider;

	public bool wasOnGround;
}
