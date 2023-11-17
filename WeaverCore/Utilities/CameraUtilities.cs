using UnityEngine;

namespace WeaverCore.Utilities
{
    public static class CameraUtilities
	{
		/// <summary>
		/// Attempts to retrieve the hud camera. Returns null if not found
		/// </summary>
		public static HUDCamera GetHudCamera()
		{
            return GameObject.FindObjectOfType<HUDCamera>();
        }

		/// <summary>
		/// Attempts to retrieve the hud canvas. Returns null if not found
		/// </summary>
		/// <returns></returns>
		public static GameObject GetHudCanvas()
		{
			var hudCamera = GetHudCamera()?.gameObject;

            if (hudCamera != null)
            {
                var hudCanvas = hudCamera.transform.Find("Hud Canvas")?.gameObject;

                if (hudCanvas != null)
				{
                    return hudCamera;
                }
				else
				{
					return null;
				}
            }
			return null;
        }
	}

}
