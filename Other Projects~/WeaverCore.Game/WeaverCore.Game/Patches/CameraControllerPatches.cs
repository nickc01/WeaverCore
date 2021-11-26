using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Patches
{
	static class CameraControllerPatches
	{
		static ReflectionAccessor<CameraController> accessor;

		static FieldAccessor<CameraController, bool> verboseMode;
		static FieldAccessor<CameraController, bool> atSceneBounds;
		static FieldAccessor<CameraController, bool> isGameplayScene;
		static FieldAccessor<CameraController, bool> teleporting;
		static FieldAccessor<CameraController, Coroutine> fadeInFailSafeCo;
		static FieldAccessor<CameraController, float> panTime;
		static FieldAccessor<CameraController, float> currentPanTime;
		static FieldAccessor<CameraController, Vector3> velocity;
		static FieldAccessor<CameraController, Vector3> velocityX;
		static FieldAccessor<CameraController, Vector3> velocityY;
		static FieldAccessor<CameraController, float> maxVelocityCurrent;
		static FieldAccessor<CameraController, float> horizontalOffset;
		static FieldAccessor<CameraController, float> startLockedTimer;
		static FieldAccessor<CameraController, float> targetDeltaX;
		static FieldAccessor<CameraController, float> targetDeltaY;
		static FieldAccessor<CameraController, CameraLockArea> currentLockArea;
		static FieldAccessor<CameraController, Vector3> panStartPos;
		static FieldAccessor<CameraController, Vector3> panEndPos;
		static FieldAccessor<CameraController, HeroController> hero_ctrl;
		static FieldAccessor<CameraController, GameManager> gm;
		static FieldAccessor<CameraController, PlayMakerFSM> fadeFSM;
		static FieldAccessor<CameraController, Transform> cameraParent;

		static MethodInfo UpdateTargetDestinationDeltaM;

		static Rect currentSceneDimensions;

		[OnInit]
		static void Init()
		{
			UpdateTargetDestinationDeltaM = typeof(CameraController).GetMethod("UpdateTargetDestinationDelta", BindingFlags.Instance | BindingFlags.NonPublic);
			On.CameraController.SceneInit += CameraController_SceneInit;
			On.CameraController.GetTilemapInfo += CameraController_GetTilemapInfo;
			On.CameraController.GameInit += CameraController_GameInit;
			On.CameraController.LateUpdate += CameraController_LateUpdate;
			On.CameraController.LockToArea += CameraController_LockToArea;
			On.CameraController.KeepWithinSceneBounds_Vector3 += CameraController_KeepWithinSceneBounds_Vector3;
			On.CameraController.KeepWithinSceneBounds_Vector2 += CameraController_KeepWithinSceneBounds_Vector2;
			On.CameraController.IsAtSceneBounds += CameraController_IsAtSceneBounds;
			On.CameraController.IsAtHorizontalSceneBounds += CameraController_IsAtHorizontalSceneBounds;
			On.CameraController.IsTouchingSides += CameraController_IsTouchingSides;

			On.CameraLockArea.Start += CameraLockArea_Start;
			On.CameraLockArea.ValidateBounds += CameraLockArea_ValidateBounds;
			//On.CameraTarget.
		}

		private static bool CameraController_IsTouchingSides(On.CameraController.orig_IsTouchingSides orig, CameraController self, float x)
		{
			var bounds = GetCameraLimits(self);

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

		private static bool CameraController_IsAtHorizontalSceneBounds(On.CameraController.orig_IsAtHorizontalSceneBounds orig, CameraController self, Vector2 targetDest, out bool leftSide)
		{
			var bounds = GetCameraLimits(self);

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

		private static bool CameraController_IsAtSceneBounds(On.CameraController.orig_IsAtSceneBounds orig, CameraController self, Vector2 targetDest)
		{
			var bounds = GetCameraLimits(self);

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

		private static Vector2 CameraController_KeepWithinSceneBounds_Vector2(On.CameraController.orig_KeepWithinSceneBounds_Vector2 orig, CameraController self, Vector2 targetDest)
		{
			var bounds = GetCameraLimits(self);

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
			atSceneBounds.Value = flag;
			return targetDest;
		}

		private static Vector3 CameraController_KeepWithinSceneBounds_Vector3(On.CameraController.orig_KeepWithinSceneBounds_Vector3 orig, CameraController self, Vector3 targetDest)
		{
			var bounds = GetCameraLimits(self);

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
			atSceneBounds.Value = flag;
			self.atHorizontalSceneBounds = flag2;
			return result;
		}

		private static void CameraController_GameInit(On.CameraController.orig_GameInit orig, CameraController self)
		{
			accessor = new ReflectionAccessor<CameraController>(self);

			verboseMode = accessor.GetFieldAccessor<bool>("verboseMode");
			atSceneBounds = accessor.GetFieldAccessor<bool>("atSceneBounds");
			isGameplayScene = accessor.GetFieldAccessor<bool>("isGameplayScene");
			teleporting = accessor.GetFieldAccessor<bool>("teleporting");
			fadeInFailSafeCo = accessor.GetFieldAccessor<Coroutine>("fadeInFailSafeCo");
			panTime = accessor.GetFieldAccessor<float>("panTime");
			currentPanTime = accessor.GetFieldAccessor<float>("currentPanTime");
			velocity = accessor.GetFieldAccessor<Vector3>("velocity");
			velocityX = accessor.GetFieldAccessor<Vector3>("velocityX");
			velocityY = accessor.GetFieldAccessor<Vector3>("velocityY");
			maxVelocityCurrent = accessor.GetFieldAccessor<float>("maxVelocityCurrent");
			horizontalOffset = accessor.GetFieldAccessor<float>("horizontalOffset");
			startLockedTimer = accessor.GetFieldAccessor<float>("startLockedTimer");
			targetDeltaX = accessor.GetFieldAccessor<float>("targetDeltaX");
			targetDeltaY = accessor.GetFieldAccessor<float>("targetDeltaY");
			currentLockArea = accessor.GetFieldAccessor<CameraLockArea>("currentLockArea");
			panStartPos = accessor.GetFieldAccessor<Vector3>("panStartPos");
			panEndPos = accessor.GetFieldAccessor<Vector3>("panEndPos");
			hero_ctrl = accessor.GetFieldAccessor<HeroController>("hero_ctrl");
			gm = accessor.GetFieldAccessor<GameManager>("gm");
			fadeFSM = accessor.GetFieldAccessor<PlayMakerFSM>("fadeFSM");
			cameraParent = accessor.GetFieldAccessor<Transform>("cameraParent");

			orig(self);
		}

		static T GetField<T>(CameraLockArea instance, string name)
		{
			return (T)typeof(CameraLockArea).GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
		}

		static void SetField<T>(CameraLockArea instance, string name, T value)
		{
			typeof(CameraLockArea).GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, value);
		}

		private static void CameraController_GetTilemapInfo(On.CameraController.orig_GetTilemapInfo orig, CameraController self)
		{
			var sceneManager = WeaverSceneManager.CurrentSceneManager;
			if (sceneManager != null && sceneManager is WeaverSceneManager wsm)
			{
				self.sceneWidth = wsm.SceneDimensions.width;
				self.sceneHeight = wsm.SceneDimensions.height;
				self.xLimit = wsm.SceneDimensions.xMax - 14.6f;
				self.yLimit = wsm.SceneDimensions.yMax - 8.3f;
			}
			else
			{
				orig(self);
			}
		}

		private static void CameraController_SceneInit(On.CameraController.orig_SceneInit orig, CameraController self)
		{
			self.sceneWidth = 0f;
			self.sceneHeight = 0f;
			orig(self);
			if (WeaverSceneManager.CurrentSceneManager is WeaverSceneManager wsm)
			{
				self.xLockMin = wsm.SceneDimensions.xMin + 14.6f;
				self.yLockMin = wsm.SceneDimensions.yMin + 8.3f;
			}
		}

		private static void CameraController_LateUpdate(On.CameraController.orig_LateUpdate orig, CameraController self)
		{
			float x = self.transform.position.x;
			float y = self.transform.position.y;
			float z = self.transform.position.z;
			float x2 = cameraParent.Value.position.x;
			float y2 = cameraParent.Value.position.y;
			if (isGameplayScene.Value && self.mode != CameraController.CameraMode.FROZEN)
			{
				if (hero_ctrl.Value.cState.lookingUp)
				{
					self.lookOffset = hero_ctrl.Value.transform.position.y - self.camTarget.transform.position.y + 6f;
				}
				else if (hero_ctrl.Value.cState.lookingDown)
				{
					self.lookOffset = hero_ctrl.Value.transform.position.y - self.camTarget.transform.position.y - 6f;
				}
				else
				{
					self.lookOffset = 0f;
				}
				UpdateTargetDestinationDeltaM.Invoke(self, null);
				//self.UpdateTargetDestinationDelta();
				Vector3 vector = self.cam.WorldToViewportPoint(self.camTarget.transform.position);
				Vector3 vector2 = new Vector3(targetDeltaX.Value, targetDeltaY.Value, 0f) - self.cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, vector.z));
				self.destination = new Vector3(x + vector2.x, y + vector2.y, z);
				if (self.mode == CameraController.CameraMode.LOCKED && currentLockArea.Value != null)
				{
					if (self.lookOffset > 0f && currentLockArea.Value.preventLookUp && self.destination.y > currentLockArea.Value.cameraYMax)
					{
						if (self.transform.position.y > currentLockArea.Value.cameraYMax)
						{
							self.destination = new Vector3(self.destination.x, self.destination.y - self.lookOffset, self.destination.z);
						}
						else
						{
							self.destination = new Vector3(self.destination.x, currentLockArea.Value.cameraYMax, self.destination.z);
						}
					}
					if (self.lookOffset < 0f && currentLockArea.Value.preventLookDown && self.destination.y < currentLockArea.Value.cameraYMin)
					{
						if (self.transform.position.y < currentLockArea.Value.cameraYMin)
						{
							self.destination = new Vector3(self.destination.x, self.destination.y - self.lookOffset, self.destination.z);
						}
						else
						{
							self.destination = new Vector3(self.destination.x, currentLockArea.Value.cameraYMin, self.destination.z);
						}
					}
				}
				if (self.mode == CameraController.CameraMode.FOLLOWING || self.mode == CameraController.CameraMode.LOCKED)
				{
					self.destination = self.KeepWithinSceneBounds(self.destination);
				}
				var velX = velocityX.Value;
				var velY = velocityY.Value;
				Vector3 vector3 = Vector3.SmoothDamp(self.transform.position, new Vector3(self.destination.x, y, z), ref velX, self.dampTimeX);
				Vector3 vector4 = Vector3.SmoothDamp(self.transform.position, new Vector3(x, self.destination.y, z), ref velY, self.dampTimeY);
				velocityX.Value = velX;
				velocityY.Value = velY;
				self.transform.SetPosition2D(vector3.x, vector4.y);
				x = self.transform.position.x;
				y = self.transform.position.y;
				if (velocity.Value.magnitude > maxVelocityCurrent.Value)
				{
					velocity.Value = velocity.Value.normalized * maxVelocityCurrent.Value;
				}
			}
			if (isGameplayScene.Value)
			{
				Rect bounds = GetCameraLimits(self);

				if (x + x2 < bounds.xMin)
				{
					self.transform.SetPositionX(bounds.xMin);
				}
				if (self.transform.position.x + x2 > bounds.xMax)
				{
					self.transform.SetPositionX(bounds.xMax);
				}
				if (self.transform.position.y + y2 < bounds.yMin)
				{
					self.transform.SetPositionY(bounds.yMin);
				}
				if (self.transform.position.y + y2 > bounds.yMax)
				{
					self.transform.SetPositionY(bounds.yMax);
				}
				if (startLockedTimer.Value > 0f)
				{
					startLockedTimer.Value -= Time.deltaTime;
				}
			}
		}

		internal static Rect GetCameraLimits(CameraController instance)
		{
			var minX = instance.xLimit - (instance.sceneWidth - (14.6f * 2f));
			var minY = instance.yLimit - (instance.sceneHeight - (8.3f * 2f));

			//var minX = instance.xLimit + 14.6f - instance.sceneWidth;
			//var minY = instance.yLimit + 8.3f - instance.sceneHeight;

			var bounds = new Rect(minX, minY, instance.xLimit - minX, instance.yLimit - minY);
			return bounds;
		}

		private static void CameraController_LockToArea(On.CameraController.orig_LockToArea orig, CameraController self, CameraLockArea lockArea)
		{
			if (!self.lockZoneList.Contains(lockArea))
			{
				if (verboseMode.Value)
				{
					Debug.LogFormat("LockZone Activated: {0} at startLockedTimer {1} ({2}s)", new object[]
					{
					lockArea.name,
					startLockedTimer.Value,
					Time.timeSinceLevelLoad
					});
				}
				self.lockZoneList.Add(lockArea);
				if (currentLockArea.Value != null && currentLockArea.Value.maxPriority && !lockArea.maxPriority)
				{
					return;
				}
				currentLockArea.Value = lockArea;
				self.SetMode(CameraController.CameraMode.LOCKED);

				var bounds = GetCameraLimits(self);

				if (lockArea.cameraXMin < bounds.xMin)
				{
					self.xLockMin = bounds.xMin;
				}
				else
				{
					self.xLockMin = lockArea.cameraXMin;
				}
				if (lockArea.cameraXMax > bounds.xMax)
				{
					self.xLockMax = bounds.xMax;
				}
				else
				{
					self.xLockMax = lockArea.cameraXMax;
				}
				if (lockArea.cameraYMin < bounds.yMin)
				{
					self.yLockMin = bounds.yMin;
				}
				else
				{
					self.yLockMin = lockArea.cameraYMin;
				}
				if (lockArea.cameraYMax > bounds.yMax)
				{
					self.yLockMax = self.yLimit;
				}
				else
				{
					self.yLockMax = lockArea.cameraYMax;
				}
				if (startLockedTimer.Value > 0f)
				{
					self.camTarget.transform.SetPosition2D(self.KeepWithinLockBounds(hero_ctrl.Value.transform.position));
					self.camTarget.destination = self.camTarget.transform.position;
					self.camTarget.EnterLockZoneInstant(self.xLockMin, self.xLockMax, self.yLockMin, self.yLockMax);
					self.transform.SetPosition2D(self.KeepWithinLockBounds(hero_ctrl.Value.transform.position));
					self.destination = self.transform.position;
					return;
				}
				self.camTarget.EnterLockZone(self.xLockMin, self.xLockMax, self.yLockMin, self.yLockMax);
			}
		}




















		private static System.Collections.IEnumerator CameraLockArea_Start(On.CameraLockArea.orig_Start orig, CameraLockArea self)
		{


			var box2d = self.GetComponent<Collider2D>();

			var camCtrl = GameCameras.instance.cameraController;

			//self.gcams = GameCameras.instance;
			SetField(self, "gcams", GameCameras.instance);
			//self.cameraCtrl = camCtrl;
			SetField(self, "cameraCtrl", camCtrl);
			//self.camTarget = GameCameras.instance.cameraTarget;
			SetField(self, "camTarget", GameCameras.instance.cameraTarget);
			Scene scene = self.gameObject.scene;
			//while (camCtrl.tilemap == null || camCtrl.tilemap.gameObject.scene != scene)
			while (camCtrl.sceneWidth == 0f)
			{
				yield return null;
			}
			if (!(bool)typeof(CameraLockArea).GetMethod("ValidateBounds", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, null))
			{
				Debug.LogError("Camera bounds are unspecified for " + self.name + ", please specify lock area bounds for this Camera Lock Area.");
			}

			/*var validateBoundsM = typeof(CameraLockArea).GetMethod("ValidateBounds", BindingFlags.Instance | BindingFlags.NonPublic);

			if (!self.ValidateBounds())
			{
				Debug.LogError("Camera bounds are unspecified for " + base.name + ", please specify lock area bounds for this Camera Lock Area.");
			}*/
			if (box2d != null)
			{
				/*self.leftSideX = box2d.bounds.min.x;
				self.rightSideX = box2d.bounds.max.x;
				self.botSideY = box2d.bounds.min.y;
				self.topSideY = box2d.bounds.max.y;*/

				SetField(self, "leftSideX", box2d.bounds.min.x);
				SetField(self, "rightSideX", box2d.bounds.max.x);
				SetField(self, "botSideY", box2d.bounds.min.y);
				SetField(self, "topSideY", box2d.bounds.max.y);
			}
		}

		private static bool CameraLockArea_ValidateBounds(On.CameraLockArea.orig_ValidateBounds orig, CameraLockArea self)
		{
			if (WeaverSceneManager.CurrentSceneManager is WeaverSceneManager)
			{
				var cameraCtrl = typeof(CameraLockArea).GetField("cameraCtrl", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(self) as CameraController;

				var cameraBounds = GetCameraLimits(cameraCtrl);

				if (self.cameraXMin < cameraBounds.xMin)
				{
					self.cameraXMin = cameraBounds.xMin;
				}
				if (self.cameraXMax > cameraBounds.xMax)
				{
					self.cameraXMax = cameraBounds.xMax;
				}
				if (self.cameraYMin < cameraBounds.yMin)
				{
					self.cameraYMin = cameraBounds.yMin;
				}
				if (self.cameraYMax > cameraBounds.yMax)
				{
					self.cameraYMax = cameraBounds.yMax;
				}

				/*if (self.cameraXMin == -1f)
				{
					self.cameraXMin = 14.6f;
				}
				if (self.cameraXMax == -1f)
				{
					self.cameraXMax = cameraCtrl.xLimit;
				}
				if (self.cameraYMin == -1f)
				{
					self.cameraYMin = 8.3f;
				}
				if (self.cameraYMax == -1f)
				{
					self.cameraYMax = cameraCtrl.yLimit;
				}*/
				return self.cameraXMin != 0f || self.cameraXMax != 0f || self.cameraYMin != 0f || self.cameraYMax != 0f;
			}
			else
			{
				return orig(self);
			}
		}


		/*class CameraControllerPatch : IInit
		{
			bool Initialized = false;

			private void CameraController_LateUpdate(On.CameraController.orig_LateUpdate orig, CameraController self)
			{
				if (!Initialized)
				{
					if (_instance == null)
					{
						_instance = staticImpl.Create();
						_instance.Initialize();
					}
					Initialized = true;
				}

				orig(self);
			}

			void IInit.OnInit()
			{
				On.CameraController.LateUpdate += CameraController_LateUpdate;
			}
		}*/
	}
}
