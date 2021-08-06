using System;
using System.Reflection;
using UnityEngine;

// Token: 0x02000107 RID: 263
[Serializable]
public class HeroControllerStates
{
	// Token: 0x06000669 RID: 1641 RVA: 0x00025ED4 File Offset: 0x000240D4
	public HeroControllerStates()
	{
		this.facingRight = false;
		this.Reset();
	}

	// Token: 0x0600066A RID: 1642 RVA: 0x00025EEC File Offset: 0x000240EC
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

	// Token: 0x0600066B RID: 1643 RVA: 0x00025F3C File Offset: 0x0002413C
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

	// Token: 0x0600066C RID: 1644 RVA: 0x00025FBC File Offset: 0x000241BC
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

	// Token: 0x040006B2 RID: 1714
	public bool facingRight;

	// Token: 0x040006B3 RID: 1715
	public bool onGround;

	// Token: 0x040006B4 RID: 1716
	public bool jumping;

	// Token: 0x040006B5 RID: 1717
	public bool wallJumping;

	// Token: 0x040006B6 RID: 1718
	public bool doubleJumping;

	// Token: 0x040006B7 RID: 1719
	public bool nailCharging;

	// Token: 0x040006B8 RID: 1720
	public bool shadowDashing;

	// Token: 0x040006B9 RID: 1721
	public bool swimming;

	// Token: 0x040006BA RID: 1722
	public bool falling;

	// Token: 0x040006BB RID: 1723
	public bool dashing;

	// Token: 0x040006BC RID: 1724
	public bool superDashing;

	// Token: 0x040006BD RID: 1725
	public bool superDashOnWall;

	// Token: 0x040006BE RID: 1726
	public bool backDashing;

	// Token: 0x040006BF RID: 1727
	public bool touchingWall;

	// Token: 0x040006C0 RID: 1728
	public bool wallSliding;

	// Token: 0x040006C1 RID: 1729
	public bool transitioning;

	// Token: 0x040006C2 RID: 1730
	public bool attacking;

	// Token: 0x040006C3 RID: 1731
	public bool lookingUp;

	// Token: 0x040006C4 RID: 1732
	public bool lookingDown;

	// Token: 0x040006C5 RID: 1733
	public bool lookingUpAnim;

	// Token: 0x040006C6 RID: 1734
	public bool lookingDownAnim;

	// Token: 0x040006C7 RID: 1735
	public bool altAttack;

	// Token: 0x040006C8 RID: 1736
	public bool upAttacking;

	// Token: 0x040006C9 RID: 1737
	public bool downAttacking;

	// Token: 0x040006CA RID: 1738
	public bool bouncing;

	// Token: 0x040006CB RID: 1739
	public bool shroomBouncing;

	// Token: 0x040006CC RID: 1740
	public bool recoilingRight;

	// Token: 0x040006CD RID: 1741
	public bool recoilingLeft;

	// Token: 0x040006CE RID: 1742
	public bool dead;

	// Token: 0x040006CF RID: 1743
	public bool hazardDeath;

	// Token: 0x040006D0 RID: 1744
	public bool hazardRespawning;

	// Token: 0x040006D1 RID: 1745
	public bool willHardLand;

	// Token: 0x040006D2 RID: 1746
	public bool recoilFrozen;

	// Token: 0x040006D3 RID: 1747
	public bool recoiling;

	// Token: 0x040006D4 RID: 1748
	public bool invulnerable;

	// Token: 0x040006D5 RID: 1749
	public bool casting;

	// Token: 0x040006D6 RID: 1750
	public bool castRecoiling;

	// Token: 0x040006D7 RID: 1751
	public bool preventDash;

	// Token: 0x040006D8 RID: 1752
	public bool preventBackDash;

	// Token: 0x040006D9 RID: 1753
	public bool dashCooldown;

	// Token: 0x040006DA RID: 1754
	public bool backDashCooldown;

	// Token: 0x040006DB RID: 1755
	public bool nearBench;

	// Token: 0x040006DC RID: 1756
	public bool inWalkZone;

	// Token: 0x040006DD RID: 1757
	public bool isPaused;

	// Token: 0x040006DE RID: 1758
	public bool onConveyor;

	// Token: 0x040006DF RID: 1759
	public bool onConveyorV;

	// Token: 0x040006E0 RID: 1760
	public bool inConveyorZone;

	// Token: 0x040006E1 RID: 1761
	public bool spellQuake;

	// Token: 0x040006E2 RID: 1762
	public bool freezeCharge;

	// Token: 0x040006E3 RID: 1763
	public bool focusing;

	// Token: 0x040006E4 RID: 1764
	public bool inAcid;

	// Token: 0x040006E5 RID: 1765
	public bool slidingLeft;

	// Token: 0x040006E6 RID: 1766
	public bool slidingRight;

	// Token: 0x040006E7 RID: 1767
	public bool touchingNonSlider;

	// Token: 0x040006E8 RID: 1768
	public bool wasOnGround;
}
