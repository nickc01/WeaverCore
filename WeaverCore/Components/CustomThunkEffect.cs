using GlobalEnums;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Overrides the nail-on-terrain thunk effect for the object this is attached to.
    /// </summary>
    public class CustomThunkEffect : MonoBehaviour
    {
        const int TerrainThunkLayerMask = (1 << (int)PhysLayers.TERRAIN) | (1 << (int)PhysLayers.SOFT_TERRAIN);

        static Func<HeroController, Collider2D> heroColliderGetter;
        static Func<HeroController, float> terrainCheckTimeGetter;
        static readonly RaycastHit2D[] terrainHitBuffer = new RaycastHit2D[1];

        [Tooltip("The effect prefab spawned when the player nails this object.")]
        public GameObject thunkEffectPrefab;

        [Tooltip("If enabled, the spawned thunk effect gets a random Z rotation like the vanilla terrain effect.")]
        public bool randomizeRotation = true;

        [OnHarmonyPatch]
        static void OnHarmonyPatch(HarmonyPatcher patcher)
        {
            heroColliderGetter = ReflectionUtilities.CreateFieldGetter<HeroController, Collider2D>("col2d");
            terrainCheckTimeGetter = ReflectionUtilities.CreateFieldGetter<HeroController, float>("NAIL_TERRAIN_CHECK_TIME");

            var original = typeof(HeroController).GetMethod(nameof(HeroController.CheckForTerrainThunk), BindingFlags.Public | BindingFlags.Instance);
            var prefix = typeof(CustomThunkEffect).GetMethod(nameof(CheckForTerrainThunk_Prefix), BindingFlags.NonPublic | BindingFlags.Static);

            if (original != null && prefix != null)
            {
                patcher.Patch(original, prefix, null);
            }
        }

        static bool CheckForTerrainThunk_Prefix(HeroController __instance, AttackDirection attackDir, ref IEnumerator __result)
        {
            if (__instance == null)
            {
                return true;
            }

            __result = CustomCheckForTerrainThunk(__instance, attackDir);
            return false;
        }

        static IEnumerator CustomCheckForTerrainThunk(HeroController heroController, AttackDirection attackDir)
        {
            bool terrainHit = false;
            float thunkTimer = terrainCheckTimeGetter != null ? terrainCheckTimeGetter(heroController) : 0.12f;

            while (thunkTimer > 0f)
            {
                if (!terrainHit)
                {
                    var heroCollider = heroColliderGetter != null ? heroColliderGetter(heroController) : null;

                    if (heroCollider == null)
                    {
                        yield break;
                    }

                    float originYOffset = 0.25f;
                    float castDistance = attackDir == AttackDirection.normal ? 2f : 1.5f;
                    float scaleMultiplier = 1f;

                    if (heroController.playerData.GetBool("equippedCharm_18"))
                    {
                        scaleMultiplier += 0.2f;
                    }

                    if (heroController.playerData.GetBool("equippedCharm_13"))
                    {
                        scaleMultiplier += 0.3f;
                    }

                    castDistance *= scaleMultiplier;

                    Vector2 size = new Vector2(0.45f, 0.45f);
                    Vector2 origin = new Vector2(heroCollider.bounds.center.x, heroCollider.bounds.center.y + originYOffset);
                    Vector2 upwardOrigin = new Vector2(heroCollider.bounds.center.x, heroCollider.bounds.max.y);
                    Vector2 downwardOrigin = new Vector2(heroCollider.bounds.center.x, heroCollider.bounds.min.y);

                    RaycastHit2D terrainHitInfo = default;
                    bool foundTerrainHit = false;

                    if (attackDir == AttackDirection.normal)
                    {
                        if ((heroController.cState.facingRight && !heroController.cState.wallSliding) || (!heroController.cState.facingRight && heroController.cState.wallSliding))
                        {
                            foundTerrainHit = TryBoxCast(origin, size, Vector2.right, castDistance, out terrainHitInfo);
                        }
                        else
                        {
                            foundTerrainHit = TryBoxCast(origin, size, Vector2.left, castDistance, out terrainHitInfo);
                        }
                    }
                    else if (attackDir == AttackDirection.upward)
                    {
                        foundTerrainHit = TryBoxCast(upwardOrigin, size, Vector2.up, castDistance, out terrainHitInfo);
                    }
                    else if (attackDir == AttackDirection.downward)
                    {
                        foundTerrainHit = TryBoxCast(downwardOrigin, size, Vector2.down, castDistance, out terrainHitInfo);
                    }

                    if (foundTerrainHit && terrainHitInfo.collider != null && !terrainHitInfo.collider.isTrigger)
                    {
                        var nonThunker = terrainHitInfo.collider.GetComponent<NonThunker>();
                        bool validThunkTarget = nonThunker == null || !nonThunker.active;

                        if (validThunkTarget)
                        {
                            terrainHit = true;
                            SpawnThunkEffect(heroController, terrainHitInfo);
                            ApplyThunkRecoil(heroController, attackDir);
                        }
                    }

                    thunkTimer -= Time.deltaTime;
                }

                yield return null;
            }
        }

        static bool TryBoxCast(Vector2 origin, Vector2 size, Vector2 direction, float distance, out RaycastHit2D hitInfo)
        {
            int hitCount = Physics2D.BoxCastNonAlloc(origin, size, 0f, direction, terrainHitBuffer, distance, TerrainThunkLayerMask);

            if (hitCount > 0)
            {
                hitInfo = terrainHitBuffer[0];
                terrainHitBuffer[0] = default;
                return true;
            }

            hitInfo = default;
            return false;
        }

        static void SpawnThunkEffect(HeroController heroController, RaycastHit2D terrainHitInfo)
        {
            var customThunkEffect = terrainHitInfo.collider.GetComponentInParent<CustomThunkEffect>();
            bool usingCustomPrefab = customThunkEffect != null && customThunkEffect.isActiveAndEnabled && customThunkEffect.thunkEffectPrefab != null;
            GameObject effectPrefab = usingCustomPrefab ? customThunkEffect.thunkEffectPrefab : heroController.nailTerrainImpactEffectPrefab;

            if (effectPrefab == null)
            {
                return;
            }

            Quaternion rotation = Quaternion.identity;
            if (!usingCustomPrefab || customThunkEffect.randomizeRotation)
            {
                rotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));
            }

            GameObject.Instantiate(effectPrefab, terrainHitInfo.point, rotation);
        }

        static void ApplyThunkRecoil(HeroController heroController, AttackDirection attackDir)
        {
            if (attackDir == AttackDirection.normal)
            {
                if (heroController.cState.facingRight)
                {
                    heroController.RecoilLeft();
                }
                else
                {
                    heroController.RecoilRight();
                }
            }
            else if (attackDir == AttackDirection.upward)
            {
                heroController.RecoilDown();
            }
        }
    }
}
