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
	static class GlowResponse_Patches
	{
		[OnInit]
		static void Init()
		{
            On.GlowResponse.OnTriggerEnter2D += GlowResponse_OnTriggerEnter2D;
		}

        private static void GlowResponse_OnTriggerEnter2D(On.GlowResponse.orig_OnTriggerEnter2D orig, GlowResponse self, Collider2D collision)
        {
			if (self.soundEffect.Clips == null)
			{
				self.soundEffect.Clips = new AudioClip[0];
			}
			orig(self,collision);
        }
    }

	static class CameraController_Patches
	{
		static Func<CameraController, bool> verboseMode;
		static Func<CameraController, bool> isGameplayScene;
		static Func<CameraController, Vector3> velocity;
		static Func<CameraController, Vector3> velocityX;
		static Func<CameraController, Vector3> velocityY;
		static Func<CameraController, float> maxVelocityCurrent;
		static Func<CameraController, float> startLockedTimer;
		static Func<CameraController, float> targetDeltaX;
		static Func<CameraController, float> targetDeltaY;
		static Func<CameraController, CameraLockArea> currentLockArea;
		static Func<CameraController, HeroController> hero_ctrl;
		static Func<CameraController, Transform> cameraParent;

		static Action<CameraController, bool> atSceneBoundsSetter;
		static Action<CameraController, Vector3> velocitySetter;
		static Action<CameraController, Vector3> velocityXSetter;
		static Action<CameraController, Vector3> velocityYSetter;
		static Action<CameraController, float> startLockedTimerSetter;
		static Action<CameraController, CameraLockArea> currentLockAreaSetter;

		static MethodInfo UpdateTargetDestinationDeltaM;

		//static CameraController self;

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
			atSceneBoundsSetter(self,flag);
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
			atSceneBoundsSetter(self, flag);
			self.atHorizontalSceneBounds = flag2;
			return result;
		}

		private static void CameraController_GameInit(On.CameraController.orig_GameInit orig, CameraController self)
		{
			verboseMode = ReflectionUtilities.CreateFieldGetter<CameraController, bool>("verboseMode");
			isGameplayScene = ReflectionUtilities.CreateFieldGetter<CameraController, bool>("isGameplayScene");
			velocity = ReflectionUtilities.CreateFieldGetter<CameraController, Vector3>("velocity");
			velocityX = ReflectionUtilities.CreateFieldGetter<CameraController, Vector3>("velocityX");
			velocityY = ReflectionUtilities.CreateFieldGetter<CameraController, Vector3>("velocityY");
			maxVelocityCurrent = ReflectionUtilities.CreateFieldGetter<CameraController, float>("maxVelocityCurrent");
			startLockedTimer = ReflectionUtilities.CreateFieldGetter<CameraController, float>("startLockedTimer");
			targetDeltaX = ReflectionUtilities.CreateFieldGetter<CameraController, float>("targetDeltaX");
			targetDeltaY = ReflectionUtilities.CreateFieldGetter<CameraController, float>("targetDeltaY");
			currentLockArea = ReflectionUtilities.CreateFieldGetter<CameraController, CameraLockArea>("currentLockArea");
			hero_ctrl = ReflectionUtilities.CreateFieldGetter<CameraController, HeroController>("hero_ctrl");
			cameraParent = ReflectionUtilities.CreateFieldGetter<CameraController, Transform>("cameraParent");
			atSceneBoundsSetter = ReflectionUtilities.CreateFieldSetter<CameraController, bool>("atSceneBounds");
			velocitySetter = ReflectionUtilities.CreateFieldSetter<CameraController, Vector3>("velocity");
			velocityXSetter = ReflectionUtilities.CreateFieldSetter<CameraController, Vector3>("velocityX");
			velocityYSetter = ReflectionUtilities.CreateFieldSetter<CameraController, Vector3>("velocityY");
			startLockedTimerSetter = ReflectionUtilities.CreateFieldSetter<CameraController, float>("startLockedTimer");
			currentLockAreaSetter = ReflectionUtilities.CreateFieldSetter<CameraController, CameraLockArea>("currentLockArea");

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
			Debug.Log("INIT A = " + self);
			Debug.Log("Hero_Ctrl = " + self.GetType().GetField("hero_ctrl", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(self));
			Debug.Log("HeroController.instance = " + HeroController.instance);
			Debug.Log("GM = " + self.GetType().GetField("gm", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(self));
			Debug.Log("Active Scene = " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
			Debug.Log("ENTIRE OBJECT = " + JsonUtility.ToJson(self,true));
			self.sceneWidth = 0f;
			Debug.Log("INIT B");
			self.sceneHeight = 0f;
			Debug.Log("INIT C");
			orig(self);
			Debug.Log("INIT D");
			if (WeaverSceneManager.CurrentSceneManager is WeaverSceneManager wsm)
			{
				Debug.Log("INIT E = " + wsm);
				self.xLockMin = wsm.SceneDimensions.xMin + 14.6f;
				Debug.Log("INIT F");
				self.yLockMin = wsm.SceneDimensions.yMin + 8.3f;
				Debug.Log("INIT G");
			}
			Debug.Log("INIT H");
		}

		private static void CameraController_LateUpdate(On.CameraController.orig_LateUpdate orig, CameraController self)
		{
			float x = self.transform.position.x;
			float y = self.transform.position.y;
			float z = self.transform.position.z;
			float x2 = cameraParent(self).position.x;
			float y2 = cameraParent(self).position.y;
			if (isGameplayScene(self) && self.mode != CameraController.CameraMode.FROZEN)
			{
				if (hero_ctrl(self).cState.lookingUp)
				{
					self.lookOffset = hero_ctrl(self).transform.position.y - self.camTarget.transform.position.y + 6f;
				}
				else if (hero_ctrl(self).cState.lookingDown)
				{
					self.lookOffset = hero_ctrl(self).transform.position.y - self.camTarget.transform.position.y - 6f;
				}
				else
				{
					self.lookOffset = 0f;
				}
                UpdateTargetDestinationDeltaM.Invoke(self, null);
                Vector3 vector = self.cam.WorldToViewportPoint(self.camTarget.transform.position);
                Vector3 vector2 = new Vector3(targetDeltaX(self), targetDeltaY(self), 0f) - self.cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, vector.z));
				self.destination = new Vector3(x + vector2.x, y + vector2.y, z);
				if (self.mode == CameraController.CameraMode.LOCKED && currentLockArea(self) != null)
				{
					if (self.lookOffset > 0f && currentLockArea(self).preventLookUp && self.destination.y > currentLockArea(self).cameraYMax)
					{
						if (self.transform.position.y > currentLockArea(self).cameraYMax)
						{
							self.destination = new Vector3(self.destination.x, self.destination.y - self.lookOffset, self.destination.z);
						}
						else
						{
							self.destination = new Vector3(self.destination.x, currentLockArea(self).cameraYMax, self.destination.z);
						}
					}
					if (self.lookOffset < 0f && currentLockArea(self).preventLookDown && self.destination.y < currentLockArea(self).cameraYMin)
					{
						if (self.transform.position.y < currentLockArea(self).cameraYMin)
						{
							self.destination = new Vector3(self.destination.x, self.destination.y - self.lookOffset, self.destination.z);
						}
						else
						{
							self.destination = new Vector3(self.destination.x, currentLockArea(self).cameraYMin, self.destination.z);
						}
					}
				}
				if (self.mode == CameraController.CameraMode.FOLLOWING || self.mode == CameraController.CameraMode.LOCKED)
				{
					self.destination = self.KeepWithinSceneBounds(self.destination);
				}
				var velX = velocityX(self);
				var velY = velocityY(self);
                Vector3 vector3 = Vector3.SmoothDamp(self.transform.position, new Vector3(self.destination.x, y, z), ref velX, self.dampTimeX);
                Vector3 vector4 = Vector3.SmoothDamp(self.transform.position, new Vector3(x, self.destination.y, z), ref velY, self.dampTimeY);
                velocityXSetter(self, velX);
                velocityYSetter(self, velY);
				self.transform.SetPosition2D(vector3.x, vector4.y);
				x = self.transform.position.x;
				y = self.transform.position.y;
				if (velocity(self).magnitude > maxVelocityCurrent(self))
				{
                    velocitySetter(self, velocity(self).normalized * maxVelocityCurrent(self));
				}
			}
			if (isGameplayScene(self))
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
				if (startLockedTimer(self) > 0f)
				{
                    startLockedTimerSetter(self, startLockedTimer(self) - Time.deltaTime);
				}
			}
		}

		internal static Rect GetCameraLimits(CameraController instance)
		{
			var minX = instance.xLimit - instance.sceneWidth + (14.6f * 2f);
			var minY = instance.yLimit - instance.sceneHeight + (8.3f * 2f);
			var bounds = new Rect(minX, minY, instance.xLimit - minX, instance.yLimit - minY);
			return bounds;
		}

		private static void CameraController_LockToArea(On.CameraController.orig_LockToArea orig, CameraController self, CameraLockArea lockArea)
		{
			if (!self.lockZoneList.Contains(lockArea))
			{
				if (verboseMode(self))
				{
                    Debug.LogFormat("LockZone Activated: {0} at startLockedTimer {1} ({2}s)", new object[]
					{
					lockArea.name,
                    startLockedTimer(self),
                    Time.timeSinceLevelLoad
					});
				}
				self.lockZoneList.Add(lockArea);
				if (currentLockArea(self) != null && currentLockArea(self).maxPriority && !lockArea.maxPriority)
				{
					return;
				}
                currentLockAreaSetter(self, lockArea);
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
				if (startLockedTimer(self) > 0f)
				{
					self.camTarget.transform.SetPosition2D(self.KeepWithinLockBounds(hero_ctrl(self).transform.position));
					self.camTarget.destination = self.camTarget.transform.position;
					self.camTarget.EnterLockZoneInstant(self.xLockMin, self.xLockMax, self.yLockMin, self.yLockMax);
					self.transform.SetPosition2D(self.KeepWithinLockBounds(hero_ctrl(self).transform.position));
					self.destination = self.transform.position;
					return;
				}
				self.camTarget.EnterLockZone(self.xLockMin, self.xLockMax, self.yLockMin, self.yLockMax);
			}
		}


		private static System.Collections.IEnumerator CameraLockArea_Start(On.CameraLockArea.orig_Start orig, CameraLockArea self)
		{
			SetField(self, "box2d", self.GetComponent<Collider2D>());

			var gcams = GameCameras.instance;

			SetField(self, "gcams", GameCameras.instance);

			if (GameCameras.instance == null)
			{
				yield break;
			}

			SetField(self, "cameraCtrl", gcams.cameraController);
			SetField(self, "camTarget", gcams.cameraTarget);

            if (gcams.cameraController == null)
            {
				yield break;
            }


			while (gcams.cameraController.sceneWidth == 0f)
			{
				yield return null;
			}

			yield return orig(self);


			/*Debug.Log("START_A");
			var box2d = self.GetComponent<Collider2D>();
			Debug.Log("START_B = " + box2d);
			var camCtrl = GameCameras.instance.cameraController;
			Debug.Log("START_C = " + camCtrl);
			SetField(self, "gcams", GameCameras.instance);
			Debug.Log("START_D = " + GameCameras.instance);

			Debug.Log("START_E");

			SetField(self, "cameraCtrl", camCtrl);
			Debug.Log("START_F");
			SetField(self, "camTarget", GameCameras.instance.cameraTarget);

			if (GameCameras.instance == null)
            {
				yield break;
            }
			Debug.Log("START_G");
			Scene scene = self.gameObject.scene;
			Debug.Log("START_H = " + scene.name);
			if (camCtrl == null)
            {
				yield break;
            }
			Debug.Log("START_I");

			while (camCtrl.sceneWidth == 0f)
			{
				yield return null;
			}

			Debug.Log("START_WHILE_DONE");

			if (!(bool)typeof(CameraLockArea).GetMethod("ValidateBounds", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(self, null))
			{
				Debug.Log("START_JJ");
				Debug.LogError("Camera bounds are unspecified for " + self.name + ", please specify lock area bounds for this Camera Lock Area.");
			}
			Debug.Log("START_J");
			if (box2d != null)
			{
				Debug.Log("START_K");
				SetField(self, "leftSideX", box2d.bounds.min.x);
				Debug.Log("START_L");
				SetField(self, "rightSideX", box2d.bounds.max.x);
				Debug.Log("START_M");
				SetField(self, "botSideY", box2d.bounds.min.y);
				Debug.Log("START_N");
				SetField(self, "topSideY", box2d.bounds.max.y);
				Debug.Log("START_O");
			}
			Debug.Log("START_P");*/
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

				return self.cameraXMin != 0f || self.cameraXMax != 0f || self.cameraYMin != 0f || self.cameraYMax != 0f;
			}
			else
			{
				return orig(self);
			}
		}
	}
}
