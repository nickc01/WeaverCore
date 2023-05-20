using GlobalEnums;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
	public void GameInit()
	{
		if (cameraCtrl == null)
		{
			cameraCtrl = base.transform.parent.GetComponent<CameraController>();
		}
	}

	public void SceneInit()
	{
		//Debug.LogError("TARGET SCENE INIT");
		if (GameManager.instance.IsGameplayScene())
		{
			isGameplayScene = true;
			heroTransform = HeroController.instance.transform;
			mode = CameraTarget.TargetMode.FOLLOW_HERO;
			stickToHeroX = true;
			stickToHeroY = true;
			fallCatcher = 0f;

			var cameraBounds = GameManager.instance.SceneDimensions;

			xLockMin = cameraBounds.xMin;
			xLockMax = cameraBounds.xMax;
			yLockMin = cameraBounds.yMin;
			yLockMax = cameraBounds.yMax;

			return;
		}
		isGameplayScene = false;
		mode = CameraTarget.TargetMode.FREE;
	}

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
					var sceneDimensions = GameManager.instance.SceneDimensions;
					if (cameraCtrl.transform.position.y < sceneDimensions.yMin)
					{
						cameraCtrl.transform.SetPositionY(sceneDimensions.yMin);
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
					var sceneDimensions = GameManager.instance.SceneDimensions;
					if (cameraCtrl.transform.position.y < sceneDimensions.yMin)
					{
						cameraCtrl.transform.SetPositionY(sceneDimensions.yMin);
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

	public void EnterLockZone(float xLockMin_var, float xLockMax_var, float yLockMin_var, float yLockMax_var)
	{
		var cameraBounds = GameManager.instance.SceneDimensions;

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
		if ((!enteredLeft || xLockMin != cameraBounds.xMin) && (!enteredRight || xLockMax != cameraBounds.xMax))
		{
			dampTimeX = dampTimeSlow;
		}
		if ((!enteredBot || yLockMin != cameraBounds.yMin) && (!enteredTop || yLockMax != cameraBounds.yMax))
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

	public void ExitLockZone()
	{
		var cameraBounds = GameManager.instance.SceneDimensions;

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
		if ((!exitedLeft || xLockMin != cameraBounds.xMin) && (!exitedRight || xLockMax != cameraBounds.xMax))
		{
			dampTimeX = dampTimeSlow;
		}
		if ((!exitedBot || yLockMin != cameraBounds.yMin) && (!exitedTop || yLockMax != cameraBounds.yMax))
		{
			dampTimeY = dampTimeSlow;
		}
		slowTimer = slowTime;
		stickToHeroX = false;
		stickToHeroY = false;
		fallStick = false;
		xLockMin = cameraBounds.xMin;
		xLockMax = cameraBounds.xMax;
		yLockMin = cameraBounds.yMin;
		yLockMax = cameraBounds.yMax;
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

	public void SetSuperDash(bool active)
	{
		superDashing = active;
	}

	public void SetQuake(bool quake)
	{
		quaking = quake;
	}

	public void FreezeInPlace()
	{
		mode = CameraTarget.TargetMode.FREE;
	}

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

	private readonly bool verboseMode;

	private Transform heroTransform;

	public CameraController cameraCtrl;

	public CameraTarget.TargetMode mode = TargetMode.FOLLOW_HERO;

	public Vector3 destination;

	private Vector3 velocityX;

	private Vector3 velocityY;

	public float xOffset;

	public float dashOffset;

	public float fallOffset;

	public float fallOffset_multiplier = 5f;

	public float xLockMin;

	public float xLockMax;

	public float yLockMin;

	public float yLockMax;

	public bool enteredLeft;

	public bool enteredRight;

	public bool enteredTop;

	public bool enteredBot;

	public bool exitedLeft;

	public bool exitedRight;

	public bool exitedTop;

	public bool exitedBot;

	public bool superDashing;

	public bool quaking;

	public float slowTime = 0.5f;

	public float dampTimeNormal = 0.075f;

	public float dampTimeSlow = 0.5f;

	public float xLookAhead = 1f;

	public float dashLookAhead = 1.5f;

	public float superDashLookAhead = 6f;

	private Vector3 heroPrevPosition;

	private readonly float dampTime;

	private float dampTimeX;

	private float dampTimeY;

	private float slowTimer;

	private readonly float snapDistance;

	public float fallCatcher;

	public bool stickToHeroX;

	public bool stickToHeroY;

	public bool enteredFromLockZone;

	private readonly float prevTarget_y;

	private readonly float prevCam_y;

	public bool fallStick;

	private bool isGameplayScene;

	public enum TargetMode
	{
		FOLLOW_HERO,
		LOCK_ZONE,
		BOSS,
		FREE
	}
}
