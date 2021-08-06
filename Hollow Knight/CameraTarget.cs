using GlobalEnums;
using UnityEngine;

// Token: 0x020000E1 RID: 225
public class CameraTarget : MonoBehaviour
{
	// Token: 0x060004AE RID: 1198 RVA: 0x00017399 File Offset: 0x00015599
	public void GameInit()
	{
		if (cameraCtrl == null)
		{
			cameraCtrl = base.transform.parent.GetComponent<CameraController>();
		}
	}

	// Token: 0x060004AF RID: 1199 RVA: 0x000173CC File Offset: 0x000155CC
	public void SceneInit()
	{
		if (GameManager.instance.IsGameplayScene())
		{
			isGameplayScene = true;
			heroTransform = HeroController.instance.transform;
			mode = CameraTarget.TargetMode.FOLLOW_HERO;
			stickToHeroX = true;
			stickToHeroY = true;
			fallCatcher = 0f;
			xLockMin = 0f;
			xLockMax = cameraCtrl.xLimit;
			yLockMin = 0f;
			yLockMax = cameraCtrl.yLimit;
			return;
		}
		isGameplayScene = false;
		mode = CameraTarget.TargetMode.FREE;
	}

	// Token: 0x060004B0 RID: 1200 RVA: 0x0001748C File Offset: 0x0001568C
	public void Update()
	{
		if (HeroController.instance == null || !isGameplayScene)
		{
			mode = CameraTarget.TargetMode.FREE;
			return;
		}
		if (isGameplayScene)
		{
			float num = base.transform.position.x;
			float num2 = base.transform.position.y;
			float z = base.transform.position.z;
			float x = heroTransform.position.x;
			float y = heroTransform.position.y;
			Vector3 position = heroTransform.position;
			if (mode == CameraTarget.TargetMode.FOLLOW_HERO)
			{
				SetDampTime();
				destination = heroTransform.position;
				if (!fallStick && fallCatcher <= 0f)
				{
					base.transform.position = new Vector3(Vector3.SmoothDamp(base.transform.position, new Vector3(destination.x, base.transform.position.y, z), ref velocityX, dampTimeX).x, Vector3.SmoothDamp(base.transform.position, new Vector3(base.transform.position.x, destination.y, z), ref velocityY, dampTimeY).y, z);
				}
				else
				{
					base.transform.position = new Vector3(Vector3.SmoothDamp(base.transform.position, new Vector3(destination.x, base.transform.position.y, z), ref velocityX, dampTimeX).x, base.transform.position.y, z);
				}
				num = base.transform.position.x;
				num2 = base.transform.position.y;
				z = base.transform.position.z;
				if ((heroPrevPosition.x < num && x > num) || (heroPrevPosition.x > num && x < num) || (num >= x - snapDistance && num <= x + snapDistance))
				{
					stickToHeroX = true;
				}
				if ((heroPrevPosition.y < num2 && y > num2) || (heroPrevPosition.y > num2 && y < num2) || (num2 >= y - snapDistance && num2 <= y + snapDistance))
				{
					stickToHeroY = true;
				}
				if (stickToHeroX)
				{
					base.transform.SetPositionX(x);
					num = x;
				}
				if (stickToHeroY)
				{
					base.transform.SetPositionY(y);
					num2 = y;
				}
			}
			if (mode == CameraTarget.TargetMode.LOCK_ZONE)
			{
				SetDampTime();
				destination = heroTransform.position;
				if (destination.x < xLockMin)
				{
					destination.x = xLockMin;
				}
				if (destination.x > xLockMax)
				{
					destination.x = xLockMax;
				}
				if (destination.y < yLockMin)
				{
					destination.y = yLockMin;
				}
				if (destination.y > yLockMax)
				{
					destination.y = yLockMax;
				}
				if (!fallStick && fallCatcher <= 0f)
				{
					base.transform.position = new Vector3(Vector3.SmoothDamp(base.transform.position, new Vector3(destination.x, num2, z), ref velocityX, dampTimeX).x, Vector3.SmoothDamp(base.transform.position, new Vector3(num, destination.y, z), ref velocityY, dampTimeY).y, z);
				}
				else
				{
					base.transform.position = new Vector3(Vector3.SmoothDamp(base.transform.position, new Vector3(destination.x, num2, z), ref velocityX, dampTimeX).x, num2, z);
				}
				num = base.transform.position.x;
				num2 = base.transform.position.y;
				z = base.transform.position.z;
				if ((heroPrevPosition.x < num && x > num) || (heroPrevPosition.x > num && x < num) || (num >= x - snapDistance && num <= x + snapDistance))
				{
					stickToHeroX = true;
				}
				if ((heroPrevPosition.y < num2 && y > num2) || (heroPrevPosition.y > num2 && y < num2) || (num2 >= y - snapDistance && num2 <= y + snapDistance))
				{
					stickToHeroY = true;
				}
				if (stickToHeroX)
				{
					bool flag = false;
					if (x >= xLockMin && x <= xLockMax)
					{
						flag = true;
					}
					if (x <= xLockMax && x >= num)
					{
						flag = true;
					}
					if (x >= xLockMin && x <= num)
					{
						flag = true;
					}
					if (flag)
					{
						base.transform.SetPositionX(x);
						num = x;
					}
				}
				if (stickToHeroY)
				{
					bool flag2 = false;
					if (y >= yLockMin && y <= yLockMax)
					{
						flag2 = true;
					}
					if (y <= yLockMax && y >= num2)
					{
						flag2 = true;
					}
					if (y >= yLockMin && y <= num2)
					{
						flag2 = true;
					}
					if (flag2)
					{
						base.transform.SetPositionY(y);
					}
				}
			}
			if (HeroController.instance != null)
			{
				if (HeroController.instance.cState.facingRight)
				{
					if (xOffset < xLookAhead)
					{
						xOffset += Time.deltaTime * 6f;
					}
				}
				else if (xOffset > -xLookAhead)
				{
					xOffset -= Time.deltaTime * 6f;
				}
				if (xOffset < -xLookAhead)
				{
					xOffset = -xLookAhead;
				}
				if (xOffset > xLookAhead)
				{
					xOffset = xLookAhead;
				}
				if (mode == CameraTarget.TargetMode.LOCK_ZONE)
				{
					if (x < xLockMin && HeroController.instance.cState.facingRight)
					{
						xOffset = x - num + 1f;
					}
					if (x > xLockMax && !HeroController.instance.cState.facingRight)
					{
						xOffset = x - num - 1f;
					}
					if (num + xOffset > xLockMax)
					{
						xOffset = xLockMax - num;
					}
					if (num + xOffset < xLockMin)
					{
						xOffset = xLockMin - num;
					}
				}
				if (xOffset < -xLookAhead)
				{
					xOffset = -xLookAhead;
				}
				if (xOffset > xLookAhead)
				{
					xOffset = xLookAhead;
				}
				if (HeroController.instance.cState.dashing && (HeroController.instance.current_velocity.x > 5f || HeroController.instance.current_velocity.x < -5f))
				{
					if (HeroController.instance.cState.facingRight)
					{
						dashOffset = dashLookAhead;
					}
					else
					{
						dashOffset = -dashLookAhead;
					}
					if (mode == CameraTarget.TargetMode.LOCK_ZONE)
					{
						if (num + dashOffset > xLockMax)
						{
							dashOffset = 0f;
						}
						if (num + dashOffset < xLockMin)
						{
							dashOffset = 0f;
						}
						if (x > xLockMax || x < xLockMin)
						{
							dashOffset = 0f;
						}
					}
				}
				else if (superDashing)
				{
					if (HeroController.instance.cState.facingRight)
					{
						dashOffset = superDashLookAhead;
					}
					else
					{
						dashOffset = -superDashLookAhead;
					}
					if (mode == CameraTarget.TargetMode.LOCK_ZONE)
					{
						if (num + dashOffset > xLockMax)
						{
							dashOffset = 0f;
						}
						if (num + dashOffset < xLockMin)
						{
							dashOffset = 0f;
						}
						if (x > xLockMax || x < xLockMin)
						{
							dashOffset = 0f;
						}
					}
				}
				else
				{
					dashOffset = 0f;
				}
				heroPrevPosition = heroTransform.position;
			}
			if (HeroController.instance != null && !HeroController.instance.cState.falling)
			{
				fallCatcher = 0f;
				fallStick = false;
			}
			if (mode == CameraTarget.TargetMode.FOLLOW_HERO || mode == CameraTarget.TargetMode.LOCK_ZONE)
			{
				if (HeroController.instance.cState.falling && cameraCtrl.transform.position.y > y + 0.1f && !fallStick && !HeroController.instance.cState.transitioning && (cameraCtrl.transform.position.y - 0.1f >= yLockMin || mode != CameraTarget.TargetMode.LOCK_ZONE))
				{
					cameraCtrl.transform.SetPositionY(cameraCtrl.transform.position.y - fallCatcher * Time.deltaTime);
					if (mode == CameraTarget.TargetMode.LOCK_ZONE && cameraCtrl.transform.position.y < yLockMin)
					{
						cameraCtrl.transform.SetPositionY(yLockMin);
					}
					if (cameraCtrl.transform.position.y < 8.3f)
					{
						cameraCtrl.transform.SetPositionY(8.3f);
					}
					if (fallCatcher < 25f)
					{
						fallCatcher += 80f * Time.deltaTime;
					}
					if (cameraCtrl.transform.position.y < heroTransform.position.y + 0.1f)
					{
						fallStick = true;
					}
					base.transform.SetPositionY(cameraCtrl.transform.position.y);
					num2 = cameraCtrl.transform.position.y;
				}
				if (fallStick)
				{
					fallCatcher = 0f;
					if (heroTransform.position.y + 0.1f >= yLockMin || mode != CameraTarget.TargetMode.LOCK_ZONE)
					{
						cameraCtrl.transform.SetPositionY(heroTransform.position.y + 0.1f);
						base.transform.SetPositionY(cameraCtrl.transform.position.y);
						num2 = cameraCtrl.transform.position.y;
					}
					if (mode == CameraTarget.TargetMode.LOCK_ZONE && cameraCtrl.transform.position.y < yLockMin)
					{
						cameraCtrl.transform.SetPositionY(yLockMin);
					}
					if (cameraCtrl.transform.position.y < 8.3f)
					{
						cameraCtrl.transform.SetPositionY(8.3f);
					}
				}
			}
			if (quaking)
			{
				num2 = heroTransform.position.y;
				if (mode == CameraTarget.TargetMode.LOCK_ZONE && num2 < yLockMin)
				{
					base.transform.SetPositionY(yLockMin);
					num2 = yLockMin;
				}
				base.transform.SetPositionY(num2);
			}
		}
	}

	// Token: 0x060004B1 RID: 1201 RVA: 0x00018088 File Offset: 0x00016288
	public void EnterLockZone(float xLockMin_var, float xLockMax_var, float yLockMin_var, float yLockMax_var)
	{
		xLockMin = xLockMin_var;
		xLockMax = xLockMax_var;
		yLockMin = yLockMin_var;
		yLockMax = yLockMax_var;
		mode = CameraTarget.TargetMode.LOCK_ZONE;
		float x = base.transform.position.x;
		float y = base.transform.position.y;
		Vector3 position = base.transform.position;
		float x2 = heroTransform.position.x;
		float y2 = heroTransform.position.y;
		Vector3 position2 = heroTransform.position;
		if ((!enteredLeft || xLockMin != 14.6f) && (!enteredRight || xLockMax != cameraCtrl.xLimit))
		{
			dampTimeX = dampTimeSlow;
		}
		if ((!enteredBot || yLockMin != 8.3f) && (!enteredTop || yLockMax != cameraCtrl.yLimit))
		{
			dampTimeY = dampTimeSlow;
		}
		slowTimer = slowTime;
		if (x >= x2 - snapDistance && x <= x2 + snapDistance)
		{
			stickToHeroX = true;
		}
		else
		{
			stickToHeroX = false;
		}
		if (y >= y2 - snapDistance && y <= y2 + snapDistance)
		{
			stickToHeroY = true;
			return;
		}
		stickToHeroY = false;
	}

	// Token: 0x060004B2 RID: 1202 RVA: 0x000181E4 File Offset: 0x000163E4
	public void EnterLockZoneInstant(float xLockMin_var, float xLockMax_var, float yLockMin_var, float yLockMax_var)
	{
		xLockMin = xLockMin_var;
		xLockMax = xLockMax_var;
		yLockMin = yLockMin_var;
		yLockMax = yLockMax_var;
		mode = CameraTarget.TargetMode.LOCK_ZONE;
		if (base.transform.position.x < xLockMin)
		{
			base.transform.SetPositionX(xLockMin);
		}
		if (base.transform.position.x > xLockMax)
		{
			base.transform.SetPositionX(xLockMax);
		}
		if (base.transform.position.y < yLockMin)
		{
			base.transform.SetPositionY(yLockMin);
		}
		if (base.transform.position.y > yLockMax)
		{
			base.transform.SetPositionY(yLockMax);
		}
		stickToHeroX = true;
		stickToHeroY = true;
	}

	// Token: 0x060004B3 RID: 1203 RVA: 0x000182C8 File Offset: 0x000164C8
	public void ExitLockZone()
	{
		if (mode == CameraTarget.TargetMode.FREE)
		{
			return;
		}
		if (HeroController.instance.cState.hazardDeath || HeroController.instance.cState.dead || (HeroController.instance.transitionState != HeroTransitionState.WAITING_TO_TRANSITION && HeroController.instance.transitionState != HeroTransitionState.WAITING_TO_ENTER_LEVEL))
		{
			mode = CameraTarget.TargetMode.FREE;
		}
		else
		{
			mode = CameraTarget.TargetMode.FOLLOW_HERO;
		}
		if ((!exitedLeft || xLockMin != 14.6f) && (!exitedRight || xLockMax != cameraCtrl.xLimit))
		{
			dampTimeX = dampTimeSlow;
		}
		if ((!exitedBot || yLockMin != 8.3f) && (!exitedTop || yLockMax != cameraCtrl.yLimit))
		{
			dampTimeY = dampTimeSlow;
		}
		slowTimer = slowTime;
		stickToHeroX = false;
		stickToHeroY = false;
		fallStick = false;
		xLockMin = 0f;
		xLockMax = cameraCtrl.xLimit;
		yLockMin = 0f;
		yLockMax = cameraCtrl.yLimit;
		if (HeroController.instance != null)
		{
			if (base.transform.position.x >= heroTransform.position.x - snapDistance && base.transform.position.x <= heroTransform.position.x + snapDistance)
			{
				stickToHeroX = true;
			}
			else
			{
				stickToHeroX = false;
			}
			if (base.transform.position.y >= heroTransform.position.y - snapDistance && base.transform.position.y <= heroTransform.position.y + snapDistance)
			{
				stickToHeroY = true;
				return;
			}
			stickToHeroY = false;
		}
	}

	// Token: 0x060004B4 RID: 1204 RVA: 0x000184D4 File Offset: 0x000166D4
	private void SetDampTime()
	{
		if (slowTimer > 0f)
		{
			slowTimer -= Time.deltaTime;
			return;
		}
		if (dampTimeX > dampTimeNormal)
		{
			dampTimeX -= 0.007f;
		}
		else if (dampTimeX < dampTimeNormal)
		{
			dampTimeX = dampTimeNormal;
		}
		if (dampTimeY > dampTimeNormal)
		{
			dampTimeY -= 0.007f;
			return;
		}
		if (dampTimeY < dampTimeNormal)
		{
			dampTimeY = dampTimeNormal;
		}
	}

	// Token: 0x060004B5 RID: 1205 RVA: 0x00018578 File Offset: 0x00016778
	public void SetSuperDash(bool active)
	{
		superDashing = active;
	}

	// Token: 0x060004B6 RID: 1206 RVA: 0x00018581 File Offset: 0x00016781
	public void SetQuake(bool quake)
	{
		quaking = quake;
	}

	// Token: 0x060004B7 RID: 1207 RVA: 0x0001858A File Offset: 0x0001678A
	public void FreezeInPlace()
	{
		mode = CameraTarget.TargetMode.FREE;
	}

	// Token: 0x060004B8 RID: 1208 RVA: 0x00018594 File Offset: 0x00016794
	public void PositionToStart()
	{
		float x = base.transform.position.x;
		Vector3 position = base.transform.position;
		float x2 = heroTransform.position.x;
		float y = heroTransform.position.y;
		velocityX = Vector3.zero;
		velocityY = Vector3.zero;
		destination = heroTransform.position;
		if (HeroController.instance.cState.facingRight)
		{
			xOffset = 1f;
		}
		else
		{
			xOffset = -1f;
		}
		if (mode == CameraTarget.TargetMode.LOCK_ZONE)
		{
			if (x2 < xLockMin && HeroController.instance.cState.facingRight)
			{
				xOffset = x2 - x + 1f;
			}
			if (x2 > xLockMax && !HeroController.instance.cState.facingRight)
			{
				xOffset = x2 - x - 1f;
			}
			if (x + xOffset > xLockMax)
			{
				xOffset = xLockMax - x;
			}
			if (x + xOffset < xLockMin)
			{
				xOffset = xLockMin - x;
			}
		}
		if (xOffset < -xLookAhead)
		{
			xOffset = -xLookAhead;
		}
		if (xOffset > xLookAhead)
		{
			xOffset = xLookAhead;
		}
		if (verboseMode)
		{
			Debug.LogFormat("CT PTS - xOffset: {0} HeroPos: {1}, {2}", new object[]
			{
				xOffset,
				x2,
				y
			});
		}
		if (mode == CameraTarget.TargetMode.FOLLOW_HERO)
		{
			if (verboseMode)
			{
				Debug.LogFormat("CT PTS - Follow Hero - CT Pos: {0}", new object[]
				{
					base.transform.position
				});
			}
			base.transform.position = cameraCtrl.KeepWithinSceneBounds(destination);
		}
		else if (mode == CameraTarget.TargetMode.LOCK_ZONE)
		{
			if (destination.x < xLockMin)
			{
				destination.x = xLockMin;
			}
			if (destination.x > xLockMax)
			{
				destination.x = xLockMax;
			}
			if (destination.y < yLockMin)
			{
				destination.y = yLockMin;
			}
			if (destination.y > yLockMax)
			{
				destination.y = yLockMax;
			}
			base.transform.position = destination;
			if (verboseMode)
			{
				Debug.LogFormat("CT PTS - Lock Zone - CT Pos: {0}", new object[]
				{
					base.transform.position
				});
			}
		}
		if (verboseMode)
		{
			Debug.LogFormat("CT - PTS: HeroPos: {0} Mode: {1} Dest: {2}", new object[]
			{
				heroTransform.position,
				mode,
				destination
			});
		}
		heroPrevPosition = heroTransform.position;
	}

	// Token: 0x04000455 RID: 1109
	private readonly bool verboseMode;

	// Token: 0x04000456 RID: 1110
	//[HideInInspector]
	//public GameManager gm;

	// Token: 0x04000457 RID: 1111
	//[HideInInspector]
	//public HeroController hero_ctrl;

	// Token: 0x04000458 RID: 1112
	private Transform heroTransform;

	// Token: 0x04000459 RID: 1113
	public CameraController cameraCtrl;

	// Token: 0x0400045A RID: 1114
	public CameraTarget.TargetMode mode = TargetMode.FOLLOW_HERO;

	// Token: 0x0400045B RID: 1115
	public Vector3 destination;

	// Token: 0x0400045C RID: 1116
	private Vector3 velocityX;

	// Token: 0x0400045D RID: 1117
	private Vector3 velocityY;

	// Token: 0x0400045E RID: 1118
	public float xOffset;

	// Token: 0x0400045F RID: 1119
	public float dashOffset;

	// Token: 0x04000460 RID: 1120
	public float fallOffset;

	// Token: 0x04000461 RID: 1121
	public float fallOffset_multiplier = 5f;

	// Token: 0x04000462 RID: 1122
	public float xLockMin;

	// Token: 0x04000463 RID: 1123
	public float xLockMax;

	// Token: 0x04000464 RID: 1124
	public float yLockMin;

	// Token: 0x04000465 RID: 1125
	public float yLockMax;

	// Token: 0x04000466 RID: 1126
	public bool enteredLeft;

	// Token: 0x04000467 RID: 1127
	public bool enteredRight;

	// Token: 0x04000468 RID: 1128
	public bool enteredTop;

	// Token: 0x04000469 RID: 1129
	public bool enteredBot;

	// Token: 0x0400046A RID: 1130
	public bool exitedLeft;

	// Token: 0x0400046B RID: 1131
	public bool exitedRight;

	// Token: 0x0400046C RID: 1132
	public bool exitedTop;

	// Token: 0x0400046D RID: 1133
	public bool exitedBot;

	// Token: 0x0400046E RID: 1134
	public bool superDashing;

	// Token: 0x0400046F RID: 1135
	public bool quaking;

	// Token: 0x04000470 RID: 1136
	public float slowTime = 0.5f;

	// Token: 0x04000471 RID: 1137
	public float dampTimeNormal = 0.075f;

	// Token: 0x04000472 RID: 1138
	public float dampTimeSlow = 0.5f;

	// Token: 0x04000473 RID: 1139
	public float xLookAhead = 1f;

	// Token: 0x04000474 RID: 1140
	public float dashLookAhead = 1.5f;

	// Token: 0x04000475 RID: 1141
	public float superDashLookAhead = 6f;

	// Token: 0x04000476 RID: 1142
	private Vector3 heroPrevPosition;

	// Token: 0x04000477 RID: 1143
	private readonly float dampTime;

	// Token: 0x04000478 RID: 1144
	private float dampTimeX;

	// Token: 0x04000479 RID: 1145
	private float dampTimeY;

	// Token: 0x0400047A RID: 1146
	private float slowTimer;

	// Token: 0x0400047B RID: 1147
	private readonly float snapDistance;

	// Token: 0x0400047C RID: 1148
	public float fallCatcher;

	// Token: 0x0400047D RID: 1149
	public bool stickToHeroX;

	// Token: 0x0400047E RID: 1150
	public bool stickToHeroY;

	// Token: 0x0400047F RID: 1151
	public bool enteredFromLockZone;

	// Token: 0x04000480 RID: 1152
	private readonly float prevTarget_y;

	// Token: 0x04000481 RID: 1153
	private readonly float prevCam_y;

	// Token: 0x04000482 RID: 1154
	public bool fallStick;

	// Token: 0x04000483 RID: 1155
	private bool isGameplayScene;

	// Token: 0x020000E2 RID: 226
	public enum TargetMode
	{
		// Token: 0x04000485 RID: 1157
		FOLLOW_HERO,
		// Token: 0x04000486 RID: 1158
		LOCK_ZONE,
		// Token: 0x04000487 RID: 1159
		BOSS,
		// Token: 0x04000488 RID: 1160
		FREE
	}
}
