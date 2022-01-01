using GlobalEnums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

// Token: 0x020000D5 RID: 213
public class CameraController : MonoBehaviour
{
	internal static CameraController EditorInstance { get; private set; }

	private void Awake()
	{
		cam = GetComponent<Camera>();
		EditorInstance = this;


	}

	// Token: 0x06000458 RID: 1112 RVA: 0x00014FE8 File Offset: 0x000131E8
	public void GameInit()
	{
		cam = base.GetComponent<Camera>();
		cameraParent = base.transform.parent.transform;
		//this.fadeFSM = FSMUtility.LocateFSM(base.gameObject, "CameraFade");
		ApplyEffectConfiguration(false, false);
		//this.GameManager.instance.UnloadingLevel += this.OnLevelUnload;
	}

	// Token: 0x06000459 RID: 1113 RVA: 0x00015058 File Offset: 0x00013258
	public void SceneInit()
	{
		sceneWidth = 0;
		sceneHeight = 0;
		startLockedTimer = 0.5f;
		velocity = Vector3.zero;
		bool isBloomForced = false;
		if (GameManager.instance.IsGameplayScene())
		{
			isGameplayScene = true;
			if (hero_ctrl == null)
			{
				hero_ctrl = HeroController.instance;
				hero_ctrl.heroInPosition += PositionToHero;
			}
			lockZoneList = new List<CameraLockArea>();
			GetTilemapInfo();
			xLockMin = GameManager.instance.SceneDimensions.xMin + 14.6f;
			xLockMax = xLimit;
			yLockMin = GameManager.instance.SceneDimensions.yMin + 8.3f;
			yLockMax = yLimit;
			dampTimeX = dampTime;
			dampTimeY = dampTime;
			maxVelocityCurrent = maxVelocity;
			string currentMapZone = GameManager.instance.GetCurrentMapZone();
			if (currentMapZone == MapZone.WHITE_PALACE.ToString() || currentMapZone == MapZone.GODS_GLORY.ToString())
			{
				isBloomForced = true;
			}
			string name = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
			if (name != null && name.StartsWith("Dream_Guardian_"))
			{
				isBloomForced = true;
			}
		}
		else
		{
			isGameplayScene = false;
			if (GameManager.instance.IsMenuScene())
			{
				isBloomForced = true;
			}
		}
		ApplyEffectConfiguration(isGameplayScene, isBloomForced);
	}

	internal Rect GetCameraBounds()
	{
		/*var minX = xLimit - (sceneWidth - (14.6f * 2f));
		var minY = yLimit - (sceneHeight - (8.3f * 2f));
		var bounds = new Rect(minX, minY, xLimit - minX, yLimit - minY);
		return bounds;*/
		//return GameManager.instance.SceneDimensions;
		var sceneDims = GameManager.instance.SceneDimensions;
		return new Rect(sceneDims.xMin + 14.6f, sceneDims.yMin + 8.3f, sceneDims.width - 14.6f, sceneDims.height - 8.3f);
	}

	// Token: 0x0600045A RID: 1114 RVA: 0x000151AC File Offset: 0x000133AC
	public void ApplyEffectConfiguration(bool isGameplayLevel, bool isBloomForced)
	{
		//bool flag = Platform.Current.GraphicsTier > Platform.GraphicsTiers.Low;
		bool flag = true;
		//base.GetComponent<FastNoise>().enabled = (isGameplayLevel && flag);
		base.GetComponent<BloomOptimized>().enabled = (flag || isBloomForced);
		//base.GetComponent<BrightnessEffect>().enabled = flag;
		base.GetComponent<ColorCorrectionCurves>().enabled = true;
	}

	// Token: 0x0600045B RID: 1115 RVA: 0x000151FC File Offset: 0x000133FC
	private void LateUpdate()
	{
		if (cameraParent == null)
		{
			return;
		}
		float x = base.transform.position.x;
		float y = base.transform.position.y;
		float z = base.transform.position.z;
		float x2 = cameraParent.position.x;
		float y2 = cameraParent.position.y;
		if (isGameplayScene && mode != CameraController.CameraMode.FROZEN)
		{
			if (hero_ctrl.cState.lookingUp)
			{
				lookOffset = hero_ctrl.transform.position.y - camTarget.transform.position.y + 6f;
			}
			else if (hero_ctrl.cState.lookingDown)
			{
				lookOffset = hero_ctrl.transform.position.y - camTarget.transform.position.y - 6f;
			}
			else
			{
				lookOffset = 0f;
			}
			UpdateTargetDestinationDelta();
			Vector3 vector = cam.WorldToViewportPoint(camTarget.transform.position);
			Vector3 vector2 = new Vector3(targetDeltaX, targetDeltaY, 0f) - cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, vector.z));
			destination = new Vector3(x + vector2.x, y + vector2.y, z);
			if (mode == CameraController.CameraMode.LOCKED && currentLockArea != null)
			{
				if (lookOffset > 0f && currentLockArea.preventLookUp && destination.y > currentLockArea.cameraYMax)
				{
					if (base.transform.position.y > currentLockArea.cameraYMax)
					{
						destination = new Vector3(destination.x, destination.y - lookOffset, destination.z);
					}
					else
					{
						destination = new Vector3(destination.x, currentLockArea.cameraYMax, destination.z);
					}
				}
				if (lookOffset < 0f && currentLockArea.preventLookDown && destination.y < currentLockArea.cameraYMin)
				{
					if (base.transform.position.y < currentLockArea.cameraYMin)
					{
						destination = new Vector3(destination.x, destination.y - lookOffset, destination.z);
					}
					else
					{
						destination = new Vector3(destination.x, currentLockArea.cameraYMin, destination.z);
					}
				}
			}
			if (mode == CameraController.CameraMode.FOLLOWING || mode == CameraController.CameraMode.LOCKED)
			{
				destination = KeepWithinSceneBounds(destination);
			}
			Vector3 vector3 = Vector3.SmoothDamp(base.transform.position, new Vector3(destination.x, y, z), ref velocityX, dampTimeX);
			Vector3 vector4 = Vector3.SmoothDamp(base.transform.position, new Vector3(x, destination.y, z), ref velocityY, dampTimeY);
			base.transform.SetPosition2D(vector3.x, vector4.y);
			x = base.transform.position.x;
			y = base.transform.position.y;
			if (velocity.magnitude > maxVelocityCurrent)
			{
				velocity = velocity.normalized * maxVelocityCurrent;
			}
		}
		if (isGameplayScene)
		{
			var bounds = GetCameraBounds();

			//Debug.Log($"Bounds = ({bounds.xMin}, {bounds.yMin}) to ({bounds.xMax}, {bounds.yMax})");

			if (transform.position.x < bounds.xMin)
			{
				base.transform.SetPositionX(bounds.xMin);
			}
			if (transform.position.x > bounds.xMax)
			{
				base.transform.SetPositionX(bounds.xMax);
			}
			if (transform.position.y < bounds.yMin)
			{
				base.transform.SetPositionY(bounds.yMin);
			}
			if (transform.position.y > bounds.yMax)
			{
				base.transform.SetPositionY(bounds.yMax);
			}
			if (startLockedTimer > 0f)
			{
				startLockedTimer -= Time.deltaTime;
			}
		}
	}

	// Token: 0x0600045C RID: 1116 RVA: 0x000156D7 File Offset: 0x000138D7
	private void OnDisable()
	{
		if (hero_ctrl != null)
		{
			hero_ctrl.heroInPosition -= PositionToHero;
		}
	}

	// Token: 0x0600045D RID: 1117 RVA: 0x000156FE File Offset: 0x000138FE
	public void FreezeInPlace(bool freezeTargetAlso = false)
	{
		SetMode(CameraController.CameraMode.FROZEN);
		if (freezeTargetAlso)
		{
			camTarget.FreezeInPlace();
		}
	}

	// Token: 0x0600045E RID: 1118 RVA: 0x00015718 File Offset: 0x00013918
	public void FadeOut(CameraFadeType type)
	{
		SetMode(CameraController.CameraMode.FROZEN);
		/*if (type == CameraFadeType.LEVEL_TRANSITION)
		{
			this.fadeFSM.Fsm.Event("FADE OUT");
			return;
		}
		if (type == CameraFadeType.HERO_DEATH)
		{
			this.fadeFSM.Fsm.Event("RESPAWN FADE");
			return;
		}
		if (type == CameraFadeType.HERO_HAZARD_DEATH)
		{
			this.fadeFSM.Fsm.Event("HAZARD FADE");
			return;
		}
		if (type == CameraFadeType.JUST_FADE)
		{
			this.fadeFSM.Fsm.Event("JUST FADE");
			return;
		}
		if (type == CameraFadeType.START_FADE)
		{
			this.fadeFSM.Fsm.Event("START FADE");
		}*/
	}

	// Token: 0x0600045F RID: 1119 RVA: 0x000157AC File Offset: 0x000139AC
	public void FadeSceneIn()
	{
		//GameCameras.instance.cameraFadeFSM.Fsm.Event("FADE SCENE IN");
	}

	// Token: 0x06000460 RID: 1120 RVA: 0x000157C8 File Offset: 0x000139C8
	public void LockToArea(CameraLockArea lockArea)
	{
		if (!lockZoneList.Contains(lockArea))
		{
			if (verboseMode)
			{
				Debug.LogFormat("LockZone Activated: {0} at startLockedTimer {1} ({2}s)", new object[]
				{
					lockArea.name,
					startLockedTimer,
					Time.timeSinceLevelLoad
				});
			}
			lockZoneList.Add(lockArea);
			if (currentLockArea != null && currentLockArea.maxPriority && !lockArea.maxPriority)
			{
				return;
			}
			currentLockArea = lockArea;
			SetMode(CameraController.CameraMode.LOCKED);
			var bounds = GetCameraBounds();

			if (lockArea.cameraXMin < bounds.xMin)
			{
				xLockMin = bounds.xMin;
			}
			else
			{
				xLockMin = lockArea.cameraXMin;
			}
			if (lockArea.cameraXMax > bounds.xMax)
			{
				xLockMax = bounds.xMax;
			}
			else
			{
				xLockMax = lockArea.cameraXMax;
			}
			if (lockArea.cameraYMin < bounds.yMin)
			{
				yLockMin = bounds.yMin;
			}
			else
			{
				yLockMin = lockArea.cameraYMin;
			}
			if (lockArea.cameraYMax > bounds.yMax)
			{
				yLockMax = yLimit;
			}
			else
			{
				yLockMax = lockArea.cameraYMax;
			}
			if (startLockedTimer > 0f)
			{
				camTarget.transform.SetPosition2D(KeepWithinLockBounds(hero_ctrl.transform.position));
				camTarget.destination = camTarget.transform.position;
				camTarget.EnterLockZoneInstant(xLockMin, xLockMax, yLockMin, yLockMax);
				base.transform.SetPosition2D(KeepWithinLockBounds(hero_ctrl.transform.position));
				destination = base.transform.position;
				return;
			}
			camTarget.EnterLockZone(xLockMin, xLockMax, yLockMin, yLockMax);
		}
	}

	// Token: 0x06000461 RID: 1121 RVA: 0x000159D0 File Offset: 0x00013BD0
	public void ReleaseLock(CameraLockArea lockarea)
	{
		lockZoneList.Remove(lockarea);
		if (verboseMode)
		{
			Debug.Log("LockZone Released " + lockarea.name);
		}
		if (lockarea == currentLockArea)
		{
			if (lockZoneList.Count > 0)
			{
				currentLockArea = lockZoneList[lockZoneList.Count - 1];
				xLockMin = currentLockArea.cameraXMin;
				xLockMax = currentLockArea.cameraXMax;
				yLockMin = currentLockArea.cameraYMin;
				yLockMax = currentLockArea.cameraYMax;
				camTarget.enteredFromLockZone = true;
				camTarget.EnterLockZone(xLockMin, xLockMax, yLockMin, yLockMax);
				return;
			}
			lastLockPosition = base.transform.position;
			if (camTarget != null)
			{
				camTarget.enteredFromLockZone = false;
				camTarget.ExitLockZone();
			}
			currentLockArea = null;
			if (!hero_ctrl.cState.hazardDeath && !hero_ctrl.cState.dead && GameManager.instance.gameState != GameState.EXITING_LEVEL)
			{
				SetMode(CameraController.CameraMode.FOLLOWING);
				return;
			}
		}
		else if (verboseMode)
		{
			Debug.Log("LockZone was not the current lock when removed.");
		}
	}

	// Token: 0x06000462 RID: 1122 RVA: 0x00015B49 File Offset: 0x00013D49
	public void ResetStartTimer()
	{
		startLockedTimer = 0.5f;
	}

	// Token: 0x06000463 RID: 1123 RVA: 0x00015B58 File Offset: 0x00013D58
	public void SnapTo(float x, float y)
	{
		camTarget.transform.position = new Vector3(x, y, camTarget.transform.position.z);
		base.transform.position = new Vector3(x, y, base.transform.position.z);
	}

	// Token: 0x06000464 RID: 1124 RVA: 0x00015BB4 File Offset: 0x00013DB4
	private void UpdateTargetDestinationDelta()
	{
		targetDeltaX = camTarget.transform.position.x + camTarget.xOffset + camTarget.dashOffset;
		targetDeltaY = camTarget.transform.position.y + camTarget.fallOffset + lookOffset;
	}

	// Token: 0x06000465 RID: 1125 RVA: 0x00015C22 File Offset: 0x00013E22
	public void SetMode(CameraController.CameraMode newMode)
	{
		if (newMode != mode)
		{
			if (newMode == CameraController.CameraMode.PREVIOUS)
			{
				mode = prevMode;
				return;
			}
			prevMode = mode;
			mode = newMode;
		}
	}

	// Token: 0x06000466 RID: 1126 RVA: 0x00015C54 File Offset: 0x00013E54
	public Vector3 KeepWithinSceneBounds(Vector3 targetDest)
	{
		var bounds = GetCameraBounds();

		Vector3 result = targetDest;
		bool flag = false;
		bool flag2 = false;
		if (result.x < bounds.xMin)
		{
			result = new Vector3(bounds.xMin, result.y, result.z);
			flag = true;
			flag2 = true;
		}
		if (result.x > bounds.xMax)
		{
			result = new Vector3(bounds.xMax, result.y, result.z);
			flag = true;
			flag2 = true;
		}
		if (result.y < bounds.yMin)
		{
			result = new Vector3(result.x, bounds.yMin, result.z);
			flag = true;
		}
		if (result.y > bounds.yMax)
		{
			result = new Vector3(result.x, bounds.yMax, result.z);
			flag = true;
		}
		atSceneBounds = flag;
		atHorizontalSceneBounds = flag2;
		return result;
	}

	// Token: 0x06000467 RID: 1127 RVA: 0x00015D1C File Offset: 0x00013F1C
	private Vector2 KeepWithinSceneBounds(Vector2 targetDest)
	{
		var bounds = GetCameraBounds();

		bool flag = false;
		if (targetDest.x < bounds.xMin)
		{
			targetDest = new Vector2(bounds.xMin, targetDest.y);
			flag = true;
		}
		if (targetDest.x > bounds.xMax)
		{
			targetDest = new Vector2(bounds.xMax, targetDest.y);
			flag = true;
		}
		if (targetDest.y < bounds.yMin)
		{
			targetDest = new Vector2(targetDest.x, bounds.yMin);
			flag = true;
		}
		if (targetDest.y > bounds.yMax)
		{
			targetDest = new Vector2(targetDest.x, bounds.yMax);
			flag = true;
		}
		atSceneBounds = flag;
		return targetDest;
	}

	// Token: 0x06000468 RID: 1128 RVA: 0x00015DBC File Offset: 0x00013FBC
	private bool IsAtSceneBounds(Vector2 targetDest)
	{
		var bounds = GetCameraBounds();

		bool result = false;
		if (targetDest.x <= bounds.xMin)
		{
			result = true;
		}
		if (targetDest.x >= bounds.xMax)
		{
			result = true;
		}
		if (targetDest.y <= bounds.yMin)
		{
			result = true;
		}
		if (targetDest.y >= bounds.yMax)
		{
			result = true;
		}
		return result;
	}

	// Token: 0x06000469 RID: 1129 RVA: 0x00015E0C File Offset: 0x0001400C
	private bool IsAtHorizontalSceneBounds(Vector2 targetDest, out bool leftSide)
	{
		var bounds = GetCameraBounds();

		bool result = false;
		leftSide = false;
		if (targetDest.x <= bounds.xMin)
		{
			result = true;
			leftSide = true;
		}
		if (targetDest.x >= bounds.xMax)
		{
			result = true;
			leftSide = false;
		}
		return result;
	}

	// Token: 0x0600046A RID: 1130 RVA: 0x00015E44 File Offset: 0x00014044
	private bool IsTouchingSides(float x)
	{
		var bounds = GetCameraBounds();

		bool result = false;
		if (x <= bounds.xMin)
		{
			result = true;
		}
		if (x >= bounds.xMax)
		{
			result = true;
		}
		return result;
	}

	// Token: 0x0600046B RID: 1131 RVA: 0x00015E6C File Offset: 0x0001406C
	public Vector2 KeepWithinLockBounds(Vector2 targetDest)
	{
		float x = targetDest.x;
		float y = targetDest.y;
		if (x < xLockMin)
		{
			x = xLockMin;
		}
		if (x > xLockMax)
		{
			x = xLockMax;
		}
		if (y < yLockMin)
		{
			y = yLockMin;
		}
		if (y > yLockMax)
		{
			y = yLockMax;
		}
		return new Vector2(x, y);
	}

	// Token: 0x0600046C RID: 1132 RVA: 0x00015ED0 File Offset: 0x000140D0
	private void GetTilemapInfo()
	{
		var sceneDimensions = GameManager.instance.SceneDimensions;

		sceneWidth = sceneDimensions.width;
		sceneHeight = sceneDimensions.height;
		xLimit = sceneDimensions.xMax - 14.6f;
		yLimit = sceneDimensions.yMax - 8.3f;
		//this.tilemap = GameManager.instance.tilemap;
		//sceneWidth = (float)this.tilemap.width;
		//sceneHeight = (float)this.tilemap.height;
		//xLimit = sceneWidth - 14.6f;
		//yLimit = sceneHeight - 8.3f;
	}

	// Token: 0x0600046D RID: 1133 RVA: 0x00015F36 File Offset: 0x00014136
	public void PositionToHero(bool forceDirect)
	{
		base.StartCoroutine(DoPositionToHero(forceDirect));
	}

	// Token: 0x0600046E RID: 1134 RVA: 0x00015F46 File Offset: 0x00014146
	private IEnumerator DoPositionToHero(bool forceDirect)
	{
		yield return new WaitForFixedUpdate();
		GetTilemapInfo();
		camTarget.PositionToStart();
		UpdateTargetDestinationDelta();
		CameraController.CameraMode previousMode = mode;
		SetMode(CameraController.CameraMode.FROZEN);
		teleporting = true;
		Vector3 newPosition = this.KeepWithinSceneBounds(camTarget.transform.position);
		if (verboseMode)
		{
			Debug.LogFormat("CC - STR: NewPosition: {0} TargetDelta: ({1}, {2}) CT-XOffset: {3} HeroPos: {4} CT-Pos: {5}", new object[]
			{
				newPosition,
				targetDeltaX,
				targetDeltaY,
				camTarget.xOffset,
				hero_ctrl.transform.position,
				camTarget.transform.position
			});
		}
		if (forceDirect)
		{
			if (verboseMode)
			{
				Debug.Log("====> TEST 1a - ForceDirect Positioning Mode");
			}
			//transform.SetPosition2D(newPosition);
			transform.position = new Vector3(newPosition.x,newPosition.y,transform.position.z);
		}
		else
		{
			if (verboseMode)
			{
				Debug.Log("====> TEST 1b - Normal Positioning Mode");
			}
			bool flag = IsAtHorizontalSceneBounds(newPosition, out bool flag2);
			bool flag3 = false;
			if (currentLockArea != null)
			{
				flag3 = true;
			}
			if (flag3)
			{
				if (verboseMode)
				{
					Debug.Log("====> TEST 3 - Lock Zone Active");
				}
				PositionToHeroFacing(newPosition, true);
				Vector2 boundedPos = KeepWithinLockBounds(transform.position);
				//transform.SetPosition2D(KeepWithinLockBounds(transform.position));
				transform.position = new Vector3(boundedPos.x, boundedPos.y, transform.position.z);
			}
			else
			{
				if (verboseMode)
				{
					Debug.Log("====> TEST 4 - No Lock Zone");
				}
				PositionToHeroFacing(newPosition, false);
			}
			if (flag)
			{
				if (verboseMode)
				{
					Debug.Log("====> TEST 2 - At Horizontal Scene Bounds");
				}
				if ((flag2 && !hero_ctrl.cState.facingRight) || (!flag2 && hero_ctrl.cState.facingRight))
				{
					if (verboseMode)
					{
						Debug.Log("====> TEST 2a - Hero Facing Bounds");
					}
					transform.SetPosition2D(newPosition);
				}
				else
				{
					if (verboseMode)
					{
						Debug.Log("====> TEST 2b - Hero Facing Inwards");
					}
					if (IsTouchingSides(targetDeltaX))
					{
						if (verboseMode)
						{
							Debug.Log("Xoffset still touching sides");
						}
						transform.SetPosition2D(newPosition);
					}
					else
					{
						if (verboseMode)
						{
							Debug.LogFormat("Not Touching Sides with Xoffset CT: {0} Hero: {1}", new object[]
							{
								camTarget.transform.position,
								hero_ctrl.transform.position
							});
						}
						if (hero_ctrl.cState.facingRight)
						{
							transform.SetPosition2D(hero_ctrl.transform.position.x + 1f, newPosition.y);
						}
						else
						{
							transform.SetPosition2D(hero_ctrl.transform.position.x - 1f, newPosition.y);
						}
					}
				}
			}
		}
		destination = transform.position;
		velocity = Vector3.zero;
		velocityX = Vector3.zero;
		velocityY = Vector3.zero;
		yield return new WaitForSeconds(0.1f);
		//GameCameras.instance.cameraFadeFSM.Fsm.Event("LEVEL LOADED");
		teleporting = false;
		if (previousMode == CameraController.CameraMode.FROZEN)
		{
			SetMode(CameraController.CameraMode.FOLLOWING);
		}
		else if (previousMode == CameraController.CameraMode.LOCKED)
		{
			if (currentLockArea != null)
			{
				SetMode(previousMode);
			}
			else
			{
				SetMode(CameraController.CameraMode.FOLLOWING);
			}
		}
		else
		{
			SetMode(previousMode);
		}
		if (verboseMode)
		{
			Debug.LogFormat("CC - PositionToHero FIN: - TargetDelta: ({0}, {1}) Destination: {2} CT-XOffset: {3} NewPosition: {4} CamTargetPos: {5} HeroPos: {6}", new object[]
			{
				targetDeltaX,
				targetDeltaY,
				destination,
				camTarget.xOffset,
				newPosition,
				camTarget.transform.position,
				hero_ctrl.transform.position
			});
		}
		yield break;
	}

	// Token: 0x0600046F RID: 1135 RVA: 0x00015F5C File Offset: 0x0001415C
	private void PositionToHeroFacing()
	{
		if (hero_ctrl.cState.facingRight)
		{
			base.transform.SetPosition2D(camTarget.transform.position.x + 1f, camTarget.transform.position.y);
			return;
		}
		base.transform.SetPosition2D(camTarget.transform.position.x - 1f, camTarget.transform.position.y);
	}

	// Token: 0x06000470 RID: 1136 RVA: 0x00015FF4 File Offset: 0x000141F4
	private void PositionToHeroFacing(Vector2 newPosition, bool useXOffset)
	{
		if (useXOffset)
		{
			base.transform.SetPosition2D(newPosition.x + camTarget.xOffset, newPosition.y);
			return;
		}
		if (hero_ctrl.cState.facingRight)
		{
			base.transform.SetPosition2D(newPosition.x + 1f, newPosition.y);
			return;
		}
		base.transform.SetPosition2D(newPosition.x - 1f, newPosition.y);
	}

	// Token: 0x06000471 RID: 1137 RVA: 0x00016078 File Offset: 0x00014278
	private Vector2 GetPositionToHeroFacing(Vector2 newPosition, bool useXOffset)
	{
		if (useXOffset)
		{
			return new Vector2(newPosition.x + camTarget.xOffset, newPosition.y);
		}
		if (hero_ctrl.cState.facingRight)
		{
			return new Vector2(newPosition.x + 1f, newPosition.y);
		}
		return new Vector2(newPosition.x - 1f, newPosition.y);
	}

	// Token: 0x06000472 RID: 1138 RVA: 0x000160E7 File Offset: 0x000142E7
	private IEnumerator FadeInFailSafe()
	{
		yield return new WaitForSeconds(5f);
		/*if (this.fadeFSM.ActiveStateName != "Normal" && this.fadeFSM.ActiveStateName != "FadingOut")
		{
			Debug.LogFormat("Failsafe fade in activated. State: {0} Scene: {1}", new object[]
			{
				this.fadeFSM.ActiveStateName,
				GameManager.instance.sceneName
			});
			this.fadeFSM.Fsm.Event("FADE SCENE IN");
		}*/
		yield break;
	}

	// Token: 0x06000473 RID: 1139 RVA: 0x000160F6 File Offset: 0x000142F6
	private void StopFailSafe()
	{
		if (fadeInFailSafeCo != null)
		{
			base.StopCoroutine(fadeInFailSafeCo);
		}
	}

	// Token: 0x06000474 RID: 1140 RVA: 0x0001610C File Offset: 0x0001430C
	private void OnLevelUnload()
	{
		if (verboseMode)
		{
			Debug.Log("Removing cam locks. (" + lockZoneList.Count.ToString() + " total)");
		}
		while (lockZoneList.Count > 0)
		{
			ReleaseLock(lockZoneList[0]);
		}
	}

	// Token: 0x06000475 RID: 1141 RVA: 0x0001616A File Offset: 0x0001436A
	private void OnDestroy()
	{
		if (GameManager.instance != null)
		{
			GameManager.instance.UnloadingLevel -= this.OnLevelUnload;
		}
	}

	private readonly bool verboseMode;
	public CameraController.CameraMode mode = CameraMode.FROZEN;
	private CameraController.CameraMode prevMode;
	public bool atSceneBounds = true;
	public bool atHorizontalSceneBounds = false;
	private bool isGameplayScene;
	private bool teleporting;
	public Vector3 lastFramePosition;
	public Vector2 lastLockPosition;
	private readonly Coroutine fadeInFailSafeCo;
	[Header("Inspector Variables")]
	public float dampTime = 0.15f;
	public float dampTimeX = 0.15f;
	public float dampTimeY = 0.15f;
	public float dampTimeFalling = 0.15f;
	public float heroBotYLimit = 0f;
	private readonly float panTime;
	private readonly float currentPanTime;
	private Vector3 velocity;
	private Vector3 velocityX;
	private Vector3 velocityY;
	public float fallOffset;
	public float fallOffset_multiplier;
	public Vector3 destination;
	public float maxVelocity = 30f;
	public float maxVelocityFalling = 25f;
	private float maxVelocityCurrent;
	private readonly float horizontalOffset;
	public float lookOffset;
	private float startLockedTimer;
	private float targetDeltaX;
	private float targetDeltaY;
	[HideInInspector]
	public Vector2 panToTarget;
	public float sceneWidth = 250f;
	public float sceneHeight = 250f;
	public float xLimit = 235.548f;
	public float yLimit = 241.9f;
	private CameraLockArea currentLockArea;
	private Vector3 panStartPos;
	private Vector3 panEndPos;
	public Camera cam;
	private HeroController hero_ctrl;
	//public tk2dTileMap tilemap;
	public CameraTarget camTarget;
	//private PlayMakerFSM fadeFSM;
	private Transform cameraParent;
	public List<CameraLockArea> lockZoneList;
	public float xLockMin = 30f;
	public float xLockMax = 9999f;
	public float yLockMin = 14.11f;
	public float yLockMax = 9999f;

	public enum CameraMode
	{
		FROZEN,
		FOLLOWING,
		LOCKED,
		PANNING,
		FADEOUT,
		FADEIN,
		PREVIOUS
	}
}
