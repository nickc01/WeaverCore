using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Patches
{
	static class CameraTarget_Patches
	{
		const BindingFlags DEFAULT_FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;

		private static Func<CameraTarget,bool> verboseModeGetter;
		private static Func<CameraTarget,Transform> heroTransformGetter;
		private static Func<CameraTarget,Vector3> velocityXGetter;
		private static Func<CameraTarget,Vector3> velocityYGetter;
		private static Func<CameraTarget,Vector3> heroPrevPositionGetter;
		private static Func<CameraTarget,float> dampTimeGetter;
		private static Func<CameraTarget,float> dampTimeXGetter;
		private static Func<CameraTarget,float> dampTimeYGetter;
		private static Func<CameraTarget,float> slowTimerGetter;
		private static Func<CameraTarget,float> snapDistanceGetter;
		private static Func<CameraTarget,float> prevTarget_yGetter;
		private static Func<CameraTarget,float> prevCam_yGetter;
		private static Func<CameraTarget,bool> isGameplaySceneGetter;

		private static Action<CameraTarget, bool> verboseModeSetter;
		private static Action<CameraTarget, Transform> heroTransformSetter;
		private static Action<CameraTarget, Vector3> velocityXSetter;
		private static Action<CameraTarget, Vector3> velocityYSetter;
		private static Action<CameraTarget, Vector3> heroPrevPositionSetter;
		private static Action<CameraTarget, float> dampTimeSetter;
		private static Action<CameraTarget, float> dampTimeXSetter;
		private static Action<CameraTarget, float> dampTimeYSetter;
		private static Action<CameraTarget, float> slowTimerSetter;
		private static Action<CameraTarget, float> snapDistanceSetter;
		private static Action<CameraTarget, float> prevTarget_ySetter;
		private static Action<CameraTarget, float> prevCam_ySetter;
		private static Action<CameraTarget, bool> isGameplaySceneSetter;

		private static Action<CameraTarget> SetDampTime;


		[OnInit]
		static void Init()
		{
			SetDampTime = ReflectionUtilities.MethodToDelegate<Action<CameraTarget>, CameraTarget>("SetDampTime", DEFAULT_FLAGS);
			verboseModeGetter = ReflectionUtilities.CreateFieldGetter<CameraTarget, bool>("verboseMode",DEFAULT_FLAGS);
			heroTransformGetter = ReflectionUtilities.CreateFieldGetter<CameraTarget, Transform>("heroTransform",DEFAULT_FLAGS);
			velocityXGetter = ReflectionUtilities.CreateFieldGetter<CameraTarget, Vector3>("velocityX",DEFAULT_FLAGS);
			velocityYGetter = ReflectionUtilities.CreateFieldGetter<CameraTarget, Vector3>("velocityY",DEFAULT_FLAGS);
			heroPrevPositionGetter = ReflectionUtilities.CreateFieldGetter<CameraTarget, Vector3>("heroPrevPosition",DEFAULT_FLAGS);
			dampTimeGetter = ReflectionUtilities.CreateFieldGetter<CameraTarget, float>("dampTime",DEFAULT_FLAGS);
			dampTimeXGetter = ReflectionUtilities.CreateFieldGetter<CameraTarget, float>("dampTimeX",DEFAULT_FLAGS);
			dampTimeYGetter = ReflectionUtilities.CreateFieldGetter<CameraTarget, float>("dampTimeY",DEFAULT_FLAGS);
			slowTimerGetter = ReflectionUtilities.CreateFieldGetter<CameraTarget, float>("slowTimer",DEFAULT_FLAGS);
			snapDistanceGetter = ReflectionUtilities.CreateFieldGetter<CameraTarget, float>("snapDistance",DEFAULT_FLAGS);
			prevTarget_yGetter = ReflectionUtilities.CreateFieldGetter<CameraTarget, float>("prevTarget_y",DEFAULT_FLAGS);
			prevCam_yGetter = ReflectionUtilities.CreateFieldGetter<CameraTarget, float>("prevCam_y",DEFAULT_FLAGS);
			isGameplaySceneGetter = ReflectionUtilities.CreateFieldGetter<CameraTarget, bool>("isGameplayScene",DEFAULT_FLAGS);


			verboseModeSetter = ReflectionUtilities.CreateFieldSetter<CameraTarget, bool>("verboseMode", DEFAULT_FLAGS);
			heroTransformSetter = ReflectionUtilities.CreateFieldSetter<CameraTarget, Transform>("heroTransform", DEFAULT_FLAGS);
			velocityXSetter = ReflectionUtilities.CreateFieldSetter<CameraTarget, Vector3>("velocityX", DEFAULT_FLAGS);
			velocityYSetter = ReflectionUtilities.CreateFieldSetter<CameraTarget, Vector3>("velocityY", DEFAULT_FLAGS);
			heroPrevPositionSetter = ReflectionUtilities.CreateFieldSetter<CameraTarget, Vector3>("heroPrevPosition", DEFAULT_FLAGS);
			dampTimeSetter = ReflectionUtilities.CreateFieldSetter<CameraTarget, float>("dampTime", DEFAULT_FLAGS);
			dampTimeXSetter = ReflectionUtilities.CreateFieldSetter<CameraTarget, float>("dampTimeX", DEFAULT_FLAGS);
			dampTimeYSetter = ReflectionUtilities.CreateFieldSetter<CameraTarget, float>("dampTimeY", DEFAULT_FLAGS);
			slowTimerSetter = ReflectionUtilities.CreateFieldSetter<CameraTarget, float>("slowTimer", DEFAULT_FLAGS);
			snapDistanceSetter = ReflectionUtilities.CreateFieldSetter<CameraTarget, float>("snapDistance", DEFAULT_FLAGS);
			prevTarget_ySetter = ReflectionUtilities.CreateFieldSetter<CameraTarget, float>("prevTarget_y", DEFAULT_FLAGS);
			prevCam_ySetter = ReflectionUtilities.CreateFieldSetter<CameraTarget, float>("prevCam_y", DEFAULT_FLAGS);
			isGameplaySceneSetter = ReflectionUtilities.CreateFieldSetter<CameraTarget, bool>("isGameplayScene", DEFAULT_FLAGS);

			On.CameraTarget.SceneInit += CameraTarget_SceneInit;
			On.CameraTarget.EnterLockZone += CameraTarget_EnterLockZone;
			On.CameraTarget.ExitLockZone += CameraTarget_ExitLockZone;
			On.CameraTarget.Update += CameraTarget_Update;
		}

		private static void CameraTarget_SceneInit(On.CameraTarget.orig_SceneInit orig, CameraTarget self)
		{
			orig(self);
			if (GameManager.instance.IsGameplayScene())
			{
				var cameraBounds = CameraController_Patches.GetCameraLimits(self.cameraCtrl);

				self.xLockMin = cameraBounds.xMin;
				self.xLockMax = cameraBounds.xMax;
				self.yLockMin = cameraBounds.yMin;
				self.yLockMax = cameraBounds.yMax;
			}
		}

		private static void CameraTarget_EnterLockZone(On.CameraTarget.orig_EnterLockZone orig, CameraTarget self, float xLockMin_var, float xLockMax_var, float yLockMin_var, float yLockMax_var)
		{
			orig(self, xLockMin_var, xLockMax_var, yLockMin_var, yLockMax_var);

			var cameraBounds = CameraController_Patches.GetCameraLimits(self.cameraCtrl);
			if ((!self.enteredLeft || self.xLockMin != cameraBounds.xMin) && (!self.enteredRight || self.xLockMax != cameraBounds.xMax))
			{
				dampTimeXSetter(self, self.dampTimeSlow);
			}
			if ((!self.enteredBot || self.yLockMin != cameraBounds.yMin) && (!self.enteredTop || self.yLockMax != cameraBounds.yMax))
			{
				dampTimeYSetter(self, self.dampTimeSlow);
			}
		}

		private static void CameraTarget_ExitLockZone(On.CameraTarget.orig_ExitLockZone orig, CameraTarget self)
		{
			orig(self);

			var cameraBounds = CameraController_Patches.GetCameraLimits(self.cameraCtrl);

			if ((!self.enteredLeft || self.xLockMin != cameraBounds.xMin) && (!self.enteredRight || self.xLockMax != cameraBounds.xMax))
			{
				dampTimeXSetter(self, self.dampTimeSlow);
			}
			if ((!self.enteredBot || self.yLockMin != cameraBounds.yMin) && (!self.enteredTop || self.yLockMax != cameraBounds.yMax))
			{
				dampTimeYSetter(self, self.dampTimeSlow);
			}

			self.xLockMin = cameraBounds.xMin;
			self.xLockMax = cameraBounds.xMax;
			self.yLockMin = cameraBounds.yMin;
			self.yLockMax = cameraBounds.yMax;
		}

		private static void CameraTarget_Update(On.CameraTarget.orig_Update orig, CameraTarget self)
		{
			if (HeroController.instance == null || !isGameplaySceneGetter(self))
			{
				self.mode = CameraTarget.TargetMode.FREE;
				return;
			}

			if (isGameplaySceneGetter(self))
			{
				float num = self.transform.position.x;
				float num2 = self.transform.position.y;
				float z = self.transform.position.z;
				float x = heroTransformGetter(self).position.x;
				float y = heroTransformGetter(self).position.y;
				Vector3 position = heroTransformGetter(self).position;
				if (self.mode == CameraTarget.TargetMode.FOLLOW_HERO)
				{
					SetDampTime(self);
					self.destination = heroTransformGetter(self).position;
					var velocityX = velocityXGetter(self);
					var velocityY = velocityYGetter(self);
					if (!self.fallStick && self.fallCatcher <= 0f)
					{
						self.transform.position = new Vector3(Vector3.SmoothDamp(self.transform.position, new Vector3(self.destination.x, self.transform.position.y, z), ref velocityX, dampTimeXGetter(self)).x, Vector3.SmoothDamp(self.transform.position, new Vector3(self.transform.position.x, self.destination.y, z), ref velocityY, dampTimeYGetter(self)).y, z);
					}
					else
					{
						self.transform.position = new Vector3(Vector3.SmoothDamp(self.transform.position, new Vector3(self.destination.x, self.transform.position.y, z), ref velocityX, dampTimeXGetter(self)).x, self.transform.position.y, z);
					}
					velocityXSetter(self, velocityX);
					velocityYSetter(self, velocityY);
					num = self.transform.position.x;
					num2 = self.transform.position.y;
					z = self.transform.position.z;
					if ((heroPrevPositionGetter(self).x < num && x > num) || (heroPrevPositionGetter(self).x > num && x < num) || (num >= x - snapDistanceGetter(self) && num <= x + snapDistanceGetter(self)))
					{
						self.stickToHeroX = true;
					}
					if ((heroPrevPositionGetter(self).y < num2 && y > num2) || (heroPrevPositionGetter(self).y > num2 && y < num2) || (num2 >= y - snapDistanceGetter(self) && num2 <= y + snapDistanceGetter(self)))
					{
						self.stickToHeroY = true;
					}
					if (self.stickToHeroX)
					{
						self.transform.SetPositionX(x);
						num = x;
					}
					if (self.stickToHeroY)
					{
						self.transform.SetPositionY(y);
						num2 = y;
					}
				}
				if (self.mode == CameraTarget.TargetMode.LOCK_ZONE)
				{
					SetDampTime(self);
					self.destination = heroTransformGetter(self).position;
					if (self.destination.x < self.xLockMin)
					{
						self.destination.x = self.xLockMin;
					}
					if (self.destination.x > self.xLockMax)
					{
						self.destination.x = self.xLockMax;
					}
					if (self.destination.y < self.yLockMin)
					{
						self.destination.y = self.yLockMin;
					}
					if (self.destination.y > self.yLockMax)
					{
						self.destination.y = self.yLockMax;
					}
					if (!self.fallStick && self.fallCatcher <= 0f)
					{
						var velocityX = velocityXGetter(self);
						var velocityY = velocityYGetter(self);
						self.transform.position = new Vector3(Vector3.SmoothDamp(self.transform.position, new Vector3(self.destination.x, num2, z), ref velocityX, dampTimeXGetter(self)).x, Vector3.SmoothDamp(self.transform.position, new Vector3(num, self.destination.y, z), ref velocityY, dampTimeYGetter(self)).y, z);
						velocityXSetter(self, velocityX);
						velocityYSetter(self, velocityY);
					}
					else
					{
						var velocityX = velocityXGetter(self);
						self.transform.position = new Vector3(Vector3.SmoothDamp(self.transform.position, new Vector3(self.destination.x, num2, z), ref velocityX, dampTimeXGetter(self)).x, num2, z);
						velocityXSetter(self, velocityX);
					}
					num = self.transform.position.x;
					num2 = self.transform.position.y;
					z = self.transform.position.z;
					if ((heroPrevPositionGetter(self).x < num && x > num) || (heroPrevPositionGetter(self).x > num && x < num) || (num >= x - snapDistanceGetter(self) && num <= x + snapDistanceGetter(self)))
					{
						self.stickToHeroX = true;
					}
					if ((heroPrevPositionGetter(self).y < num2 && y > num2) || (heroPrevPositionGetter(self).y > num2 && y < num2) || (num2 >= y - snapDistanceGetter(self) && num2 <= y + snapDistanceGetter(self)))
					{
						self.stickToHeroY = true;
					}
					if (self.stickToHeroX)
					{
						bool flag = false;
						if (x >= self.xLockMin && x <= self.xLockMax)
						{
							flag = true;
						}
						if (x <= self.xLockMax && x >= num)
						{
							flag = true;
						}
						if (x >= self.xLockMin && x <= num)
						{
							flag = true;
						}
						if (flag)
						{
							self.transform.SetPositionX(x);
							num = x;
						}
					}
					if (self.stickToHeroY)
					{
						bool flag2 = false;
						if (y >= self.yLockMin && y <= self.yLockMax)
						{
							flag2 = true;
						}
						if (y <= self.yLockMax && y >= num2)
						{
							flag2 = true;
						}
						if (y >= self.yLockMin && y <= num2)
						{
							flag2 = true;
						}
						if (flag2)
						{
							self.transform.SetPositionY(y);
						}
					}
				}
				if (HeroController.instance != null)
				{
					if (HeroController.instance.cState.facingRight)
					{
						if (self.xOffset < self.xLookAhead)
						{
							self.xOffset += Time.deltaTime * 6f;
						}
					}
					else if (self.xOffset > -self.xLookAhead)
					{
						self.xOffset -= Time.deltaTime * 6f;
					}
					if (self.xOffset < -self.xLookAhead)
					{
						self.xOffset = -self.xLookAhead;
					}
					if (self.xOffset > self.xLookAhead)
					{
						self.xOffset = self.xLookAhead;
					}
					if (self.mode == CameraTarget.TargetMode.LOCK_ZONE)
					{
						if (x < self.xLockMin && HeroController.instance.cState.facingRight)
						{
							self.xOffset = x - num + 1f;
						}
						if (x > self.xLockMax && !HeroController.instance.cState.facingRight)
						{
							self.xOffset = x - num - 1f;
						}
						if (num + self.xOffset > self.xLockMax)
						{
							self.xOffset = self.xLockMax - num;
						}
						if (num + self.xOffset < self.xLockMin)
						{
							self.xOffset = self.xLockMin - num;
						}
					}
					if (self.xOffset < -self.xLookAhead)
					{
						self.xOffset = -self.xLookAhead;
					}
					if (self.xOffset > self.xLookAhead)
					{
						self.xOffset = self.xLookAhead;
					}
					if (HeroController.instance.cState.dashing && (HeroController.instance.current_velocity.x > 5f || HeroController.instance.current_velocity.x < -5f))
					{
						if (HeroController.instance.cState.facingRight)
						{
							self.dashOffset = self.dashLookAhead;
						}
						else
						{
							self.dashOffset = -self.dashLookAhead;
						}
						if (self.mode == CameraTarget.TargetMode.LOCK_ZONE)
						{
							if (num + self.dashOffset > self.xLockMax)
							{
								self.dashOffset = 0f;
							}
							if (num + self.dashOffset < self.xLockMin)
							{
								self.dashOffset = 0f;
							}
							if (x > self.xLockMax || x < self.xLockMin)
							{
								self.dashOffset = 0f;
							}
						}
					}
					else if (self.superDashing)
					{
						if (HeroController.instance.cState.facingRight)
						{
							self.dashOffset = self.superDashLookAhead;
						}
						else
						{
							self.dashOffset = -self.superDashLookAhead;
						}
						if (self.mode == CameraTarget.TargetMode.LOCK_ZONE)
						{
							if (num + self.dashOffset > self.xLockMax)
							{
								self.dashOffset = 0f;
							}
							if (num + self.dashOffset < self.xLockMin)
							{
								self.dashOffset = 0f;
							}
							if (x > self.xLockMax || x < self.xLockMin)
							{
								self.dashOffset = 0f;
							}
						}
					}
					else
					{
						self.dashOffset = 0f;
					}
					heroPrevPositionSetter(self, heroTransformGetter(self).position);
					//heroPrevPositionGetter(self) = heroTransformGetter(self).position;
				}
				if (HeroController.instance != null && !HeroController.instance.cState.falling)
				{
					self.fallCatcher = 0f;
					self.fallStick = false;
				}
				if (self.mode == CameraTarget.TargetMode.FOLLOW_HERO || self.mode == CameraTarget.TargetMode.LOCK_ZONE)
				{
					if (HeroController.instance.cState.falling && self.cameraCtrl.transform.position.y > y + 0.1f && !self.fallStick && !HeroController.instance.cState.transitioning && (self.cameraCtrl.transform.position.y - 0.1f >= self.yLockMin || self.mode != CameraTarget.TargetMode.LOCK_ZONE))
					{
						Debug.Log("A");
						self.cameraCtrl.transform.SetPositionY(self.cameraCtrl.transform.position.y - self.fallCatcher * Time.deltaTime);
						if (self.mode == CameraTarget.TargetMode.LOCK_ZONE && self.cameraCtrl.transform.position.y < self.yLockMin)
						{
							Debug.Log("B");
							self.cameraCtrl.transform.SetPositionY(self.yLockMin);
						}
						var sceneDimensions = CameraController_Patches.GetCameraLimits(self.cameraCtrl);
						if (self.cameraCtrl.transform.position.y < sceneDimensions.yMin)
						{
							Debug.Log("C");
							self.cameraCtrl.transform.SetPositionY(sceneDimensions.yMin);
						}
						if (self.fallCatcher < 25f)
						{
							self.fallCatcher += 80f * Time.deltaTime;
						}
						if (self.cameraCtrl.transform.position.y < heroTransformGetter(self).position.y + 0.1f)
						{
							self.fallStick = true;
						}
						self.transform.SetPositionY(self.cameraCtrl.transform.position.y);
						num2 = self.cameraCtrl.transform.position.y;
					}
					if (self.fallStick)
					{
						self.fallCatcher = 0f;
						if (heroTransformGetter(self).position.y + 0.1f >= self.yLockMin || self.mode != CameraTarget.TargetMode.LOCK_ZONE)
						{
							Debug.Log("D");
							self.cameraCtrl.transform.SetPositionY(heroTransformGetter(self).position.y + 0.1f);
							self.transform.SetPositionY(self.cameraCtrl.transform.position.y);
							num2 = self.cameraCtrl.transform.position.y;
						}
						if (self.mode == CameraTarget.TargetMode.LOCK_ZONE && self.cameraCtrl.transform.position.y < self.yLockMin)
						{
							Debug.Log("E");
							self.cameraCtrl.transform.SetPositionY(self.yLockMin);
						}
						var sceneDimensions = CameraController_Patches.GetCameraLimits(self.cameraCtrl);
						if (self.cameraCtrl.transform.position.y < sceneDimensions.yMin)
						{
							Debug.Log("F");
							self.cameraCtrl.transform.SetPositionY(sceneDimensions.yMin);
						}
					}
				}
				if (self.quaking)
				{
					num2 = heroTransformGetter(self).position.y;
					if (self.mode == CameraTarget.TargetMode.LOCK_ZONE && num2 < self.yLockMin)
					{
						self.transform.SetPositionY(self.yLockMin);
						num2 = self.yLockMin;
					}
					self.transform.SetPositionY(num2);
				}
			}
		}
	}
}
