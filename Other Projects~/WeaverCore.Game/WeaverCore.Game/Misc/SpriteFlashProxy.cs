using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Components;
using WeaverCore.Utilities;

public class SpriteFlashProxy : SpriteFlash
{
	static Func<SpriteFlash, Color> flashColour;
    static Func<SpriteFlash, float> amount;
    static Func<SpriteFlash, float> timeUp;
    static Func<SpriteFlash, float> stayTime;
    static Func<SpriteFlash, float> timeDown;

    /*SpriteFlasher _weaverFlasher;

	public SpriteFlasher WeaverFlasher
	{
		get
		{
			if (_weaverFlasher == null)
			{
				_weaverFlasher = GetComponent<SpriteFlasher>();
			}
			return _weaverFlasher;
		}
	}*/

	[OnRuntimeInit]
	static void Init()
	{
        On.SpriteFlash.Start += SpriteFlash_Start;
        On.SpriteFlash.OnDisable += SpriteFlash_OnDisable;
        On.SpriteFlash.Update += SpriteFlash_Update;

        On.SpriteFlash.flash += SpriteFlash_flash;

        flashColour = ReflectionUtilities.CreateFieldGetter<SpriteFlash, Color>(nameof(flashColour));
        amount = ReflectionUtilities.CreateFieldGetter<SpriteFlash, float>(nameof(amount));
        timeUp = ReflectionUtilities.CreateFieldGetter<SpriteFlash, float>(nameof(timeUp));
        stayTime = ReflectionUtilities.CreateFieldGetter<SpriteFlash, float>(nameof(stayTime));
        timeDown = ReflectionUtilities.CreateFieldGetter<SpriteFlash, float>(nameof(timeDown));
    }

	[OnHarmonyPatch]
	static void OnPatch(HarmonyPatcher patcher)
	{
		var post = typeof(SpriteFlashProxy).GetMethod(nameof(GeneralFlash_Post), BindingFlags.NonPublic | BindingFlags.Static);

        foreach (var method in typeof(SpriteFlash).GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            if (method.Name.ToLower().StartsWith("flash") && method.Name != "flash")
            {
				patcher.Patch(method, null, post);
            }
        }
    }

    static List<SpriteFlasher> flasherCollection = new List<SpriteFlasher>();

	static void GeneralFlash_Post(SpriteFlash __instance)
	{
		if (__instance is SpriteFlashProxy proxy)
		{
            var entityHealth = proxy.GetComponentInParent<EntityHealth>();

            entityHealth.GetComponentsInChildren(flasherCollection);

            foreach (var flasher in flasherCollection)
            {
                flasher.DoFlash(timeUp(proxy), timeDown(proxy), amount(proxy), flashColour(proxy), stayTime(proxy));
            }

			//proxy.WeaverFlasher.DoFlash(timeUp(proxy), timeDown(proxy), amount(proxy), flashColour(proxy), stayTime(proxy));
		}
	}

    private static void SpriteFlash_flash(On.SpriteFlash.orig_flash orig, SpriteFlash self, UnityEngine.Color flashColour_var, float amount_var, float timeUp_var, float stayTime_var, float timeDown_var)
    {
        if (self is SpriteFlashProxy proxy)
        {
            var entityHealth = proxy.GetComponentInParent<EntityHealth>();

            entityHealth.GetComponentsInChildren(flasherCollection);

            foreach (var flasher in flasherCollection)
            {
                flasher.DoFlash(timeUp_var, timeDown_var, amount_var, flashColour_var, stayTime_var);
            }

            //proxy.WeaverFlasher.DoFlash(timeUp_var, timeDown_var, amount_var, flashColour_var, stayTime_var);
        }
        else
        {
            orig(self,flashColour_var,amount_var,timeUp_var,stayTime_var,timeDown_var);
        }
    }

    private static void SpriteFlash_Start(On.SpriteFlash.orig_Start orig, SpriteFlash self)
    {
        if (self is SpriteFlashProxy proxy)
        {

        }
        else
        {
            orig(self);
        }
    }

    private static void SpriteFlash_Update(On.SpriteFlash.orig_Update orig, SpriteFlash self)
    {
        if (self is SpriteFlashProxy proxy)
        {

        }
        else
        {
            orig(self);
        }
    }

    private static void SpriteFlash_OnDisable(On.SpriteFlash.orig_OnDisable orig, SpriteFlash self)
    {
        if (self is SpriteFlashProxy proxy)
        {

        }
        else
        {
            orig(self);
        }
    }
}

