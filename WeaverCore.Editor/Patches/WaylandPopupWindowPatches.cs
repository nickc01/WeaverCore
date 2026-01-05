/*using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Editor.Patches
{
    /// <summary>
    /// Guards Unity's popup sizing logic on Linux/Wayland to avoid slow X server calls
    /// and destroyed window references that leave editor popups black.
    /// </summary>
    internal static class WaylandPopupWindowPatches
    {
        private static readonly TimeSpan FitCooldown = TimeSpan.FromMilliseconds(300);
        private static readonly ConditionalWeakTable<object, FitStamp> FitTimestamps = new ConditionalWeakTable<object, FitStamp>();

        [OnHarmonyPatch]
        private static void OnHarmonyPatch(HarmonyPatcher patcher)
        {
#if UNITY_EDITOR
            if (!IsLinuxWayland())
            {
                return;
            }

            try
            {
                var editorAssembly = typeof(UnityEditor.EditorWindow).Assembly;

                var popupWindowType = editorAssembly.GetType("UnityEditor.PopupWindow");
                var fitMethod = popupWindowType?.GetMethod("FitWindowToContent", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fitMethod != null)
                {
                    var prefix = typeof(WaylandPopupWindowPatches).GetMethod(nameof(FitWindowToContentPrefix), BindingFlags.Static | BindingFlags.NonPublic);
                    patcher.Patch(fitMethod, prefix, null);
                }

                var containerWindowType = editorAssembly.GetType("UnityEditor.ContainerWindow");
                var minMaxMethod = containerWindowType?.GetMethod("SetMinMaxSizes", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (minMaxMethod != null)
                {
                    var prefix = typeof(WaylandPopupWindowPatches).GetMethod(nameof(SetMinMaxSizesPrefix), BindingFlags.Static | BindingFlags.NonPublic);
                    patcher.Patch(minMaxMethod, prefix, null);
                }

                var positionProp = containerWindowType?.GetProperty("position", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var positionSetter = positionProp?.GetSetMethod(true);
                if (positionSetter != null)
                {
                    var prefix = typeof(WaylandPopupWindowPatches).GetMethod(nameof(SetPositionPrefix), BindingFlags.Static | BindingFlags.NonPublic);
                    patcher.Patch(positionSetter, prefix, null);
                }

                var editorWindowType = typeof(UnityEditor.EditorWindow);
                var minSizeSetter = editorWindowType.GetProperty("minSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetSetMethod(true);
                if (minSizeSetter != null)
                {
                    var prefix = typeof(WaylandPopupWindowPatches).GetMethod(nameof(EditorWindowSizePrefix), BindingFlags.Static | BindingFlags.NonPublic);
                    patcher.Patch(minSizeSetter, prefix, null);
                }

                var maxSizeSetter = editorWindowType.GetProperty("maxSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetSetMethod(true);
                if (maxSizeSetter != null)
                {
                    var prefix = typeof(WaylandPopupWindowPatches).GetMethod(nameof(EditorWindowSizePrefix), BindingFlags.Static | BindingFlags.NonPublic);
                    patcher.Patch(maxSizeSetter, prefix, null);
                }
            }
            catch (Exception e)
            {
                WeaverLog.Log("Wayland popup safety patch failed to apply: " + e);
            }
#endif
        }

        private static bool FitWindowToContentPrefix(object __instance)
        {
            if (!IsLinuxWayland())
            {
                return true;
            }

            // On Wayland we skip FitWindowToContent to avoid SetGtkWindowSizeAndPosition hangs and
            // subsequent MissingReferenceExceptions when the backing window is gone.
            if (!AllowWaylandPopupFit() || IsBackingWindowDestroyed(__instance))
            {
                LogOnce("Skipping FitWindowToContent on Wayland to avoid X/GTK stalls and destroyed window errors (set WEAVER_WAYLAND_POPUP_FIT=1 to re-enable).");
                TracePosition("FitWindowToContent SKIP", __instance);
                return false;
            }

            try
            {
                // Skip expensive native sizing calls if they are being spammed.
                if (ShouldThrottle(__instance))
                {
                    return false;
                }
                TracePosition("FitWindowToContent", __instance);
            }
            catch (Exception e)
            {
                WeaverLog.Log("FitWindowToContent Wayland guard failed, skipping original: " + e);
                return false;
            }

            return true;
        }

        private static bool SetMinMaxSizesPrefix(object __instance, Vector2 min, Vector2 max)
        {
            if (!IsLinuxWayland())
            {
                return true;
            }

            try
            {
                // When Unity destroys the ContainerWindow, any further calls throw MissingReferenceException.
                if (IsUnityObjectDestroyed(__instance))
                {
                    return false;
                }
                TracePosition($"SetMinMaxSizes {min} -> {max}", __instance);
            }
            catch (Exception e)
            {
                WeaverLog.Log("SetMinMaxSizes Wayland guard failed, skipping original: " + e);
                return false;
            }

            return true;
        }

        private static bool SetPositionPrefix(object __instance, ref Rect value)
        {
            if (!IsLinuxWayland())
            {
                return true;
            }

            try
            {
                if (IsUnityObjectDestroyed(__instance))
                {
                    return false;
                }
                value = ClampToPrimaryMonitor(value);
                TracePosition($"SetPosition {value}", __instance);
            }
            catch (Exception e)
            {
                WeaverLog.Log("SetPosition Wayland guard failed, skipping original: " + e);
                return false;
            }

            return true;
        }

        private static bool IsBackingWindowDestroyed(object popupInstance)
        {
            if (popupInstance == null)
            {
                return true;
            }

            var parentField = popupInstance.GetType().GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic);
            var hostView = parentField?.GetValue(popupInstance);
            if (hostView == null)
            {
                return true;
            }

            var windowProperty = hostView.GetType().GetProperty("window", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var containerWindow = windowProperty?.GetValue(hostView) as UnityEngine.Object;

            return containerWindow == null;
        }

        private static bool IsUnityObjectDestroyed(object instance)
        {
            if (instance is UnityEngine.Object unityObj)
            {
                try
                {
                    // Unity's overridden == operator returns true when the native object is gone.
                    return unityObj == null || Equals(unityObj, null);
                }
                catch
                {
                    return true;
                }
            }

            return instance == null;
        }

        private static bool EditorWindowSizePrefix(object __instance, Vector2 value)
        {
            if (!IsLinuxWayland())
            {
                return true;
            }

            try
            {
                // If the parent host/container is gone, any size write will trigger MissingReferenceException.
                if (IsBackingWindowDestroyed(__instance))
                {
                    return false;
                }
                TracePosition($"EditorWindow size {value}", __instance);
            }
            catch (Exception e)
            {
                WeaverLog.Log("EditorWindow size guard failed, skipping original: " + e);
                return false;
            }

            return true;
        }

        private static bool AllowWaylandPopupFit()
        {
            var allow = Environment.GetEnvironmentVariable("WEAVER_WAYLAND_POPUP_FIT");
            return !string.IsNullOrEmpty(allow) && allow != "0";
        }

        private static void LogOnce(string message)
        {
            if (_loggedMessage)
            {
                return;
            }

            _loggedMessage = true;
            WeaverLog.Log(message);
        }

        private static bool _loggedMessage;

        private static bool ShouldTrace()
        {
            // Always trace while debugging Wayland popup placement.
            return false;
        }

        private static Rect ClampToPrimaryMonitor(Rect rect)
        {
            var bounds = GetPrimaryBounds();

            var maxX = bounds.xMax - rect.width;
            var maxY = bounds.yMax - rect.height;

            rect.x = Mathf.Clamp(rect.x, bounds.xMin, maxX);
            rect.y = Mathf.Clamp(rect.y, bounds.yMin, maxY);

            return rect;
        }

        private static Rect GetPrimaryBounds()
        {
            try
            {
                var res = Screen.currentResolution;
                if (res.width > 0 && res.height > 0)
                {
                    return new Rect(0, 0, res.width, res.height);
                }
            }
            catch
            {
                // ignore, fall back below
            }

            // Sensible default if resolution cannot be read.
            return new Rect(0, 0, 1920, 1080);
        }

        private static void TracePosition(string label, object popupInstance)
        {
            if (!ShouldTrace())
            {
                return;
            }

            try
            {
                var info = BuildPopupInfo(popupInstance);
                WeaverLog.Log($"[WaylandPopupTrace] {label} :: {info}");
            }
            catch (Exception e)
            {
                WeaverLog.Log($"[WaylandPopupTrace] Failed ({label}): {e}");
            }
        }

        private static string BuildPopupInfo(object popupInstance)
        {
            var parentField = popupInstance?.GetType().GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic);
            var hostView = parentField?.GetValue(popupInstance);

            var windowProperty = hostView?.GetType().GetProperty("window", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var containerWindow = windowProperty?.GetValue(hostView);

            var popupType = popupInstance?.GetType().Name ?? "null";
            var hostType = hostView?.GetType().Name ?? "null";

            string popupPos = "(unknown)";
            string windowPos = "(unknown)";

            try
            {
                var popupPosProp = popupInstance?.GetType().GetProperty("position", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                popupPos = popupPosProp?.GetValue(popupInstance)?.ToString() ?? popupPos;
            }
            catch { }

            try
            {
                var posProp = containerWindow?.GetType().GetProperty("position", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                windowPos = posProp?.GetValue(containerWindow)?.ToString() ?? windowPos;
            }
            catch { }

            return $"popup={popupType} host={hostType} popupPos={popupPos} windowPos={windowPos}";
        }

        private static bool ShouldThrottle(object instance)
        {
            var now = DateTime.UtcNow;

            if (!FitTimestamps.TryGetValue(instance, out var stamp))
            {
                FitTimestamps.Add(instance, new FitStamp { LastCall = now });
                return false;
            }

            if (now - stamp.LastCall < FitCooldown)
            {
                return true;
            }

            stamp.LastCall = now;
            return false;
        }

        private static bool IsLinuxWayland()
        {
            if (Application.platform != RuntimePlatform.LinuxEditor)
            {
                return false;
            }

            var waylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY");
            var sessionType = Environment.GetEnvironmentVariable("XDG_SESSION_TYPE");

            return (!string.IsNullOrEmpty(waylandDisplay) && waylandDisplay.IndexOf("wayland", StringComparison.OrdinalIgnoreCase) >= 0) ||
                   (!string.IsNullOrEmpty(sessionType) && sessionType.IndexOf("wayland", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private sealed class FitStamp
        {
            public DateTime LastCall;
        }
    }
}
*/