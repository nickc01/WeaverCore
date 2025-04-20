using UnityEngine;

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Contains utility functions for working with GameObjects
    /// </summary>
    public static class GameObjectUtilities
    {
        /// <summary>
        /// Activates a GameObject, as well as all children recursively
        /// </summary>
        /// <param name="gm">The gameObject to activate</param>
        /// <param name="active">Should the gameobject and children be active?</param>
        public static void ActivateAllChildren(this GameObject gm, bool active)
        {
            gm.SetActive(active);
            for (int i = 0; i < gm.transform.childCount; i++)
            {
                ActivateAllChildren(gm.transform.GetChild(i).gameObject, active);
            }
        }

        /// <summary>
        /// Gets the full path of the GameObject in the hierarchy.
        /// </summary>
        /// <param name="gameObject">The GameObject to get the path for.</param>
        /// <returns>The full path of the GameObject in the hierarchy.</returns>
        public static string GetFullPath(this GameObject gameObject)
        {
            if (gameObject == null)
            {
                return null;
            }

            string path = gameObject.name;
            Transform current = gameObject.transform;

            while (current.parent != null)
            {
                current = current.parent;
                path = $"{current.name}/{path}";
            }

            return path;
        }

        /// <summary>
        /// Finds a GameObject in the currently active scene by its full hierarchy path,
        /// e.g. "RootObject/ChildObject/GrandChild".
        /// </summary>
        /// <param name="fullPath">Slash‑delimited path of the object, starting at a root.</param>
        /// <returns>
        /// The matching GameObject if found; otherwise <c>null</c>.
        /// </returns>
        public static GameObject FindByFullPath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                return null;

            var segments = fullPath.Split('/');
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

            GameObject current = null;
            foreach (var root in roots)
            {
                if (root.name == segments[0])
                {
                    current = root;
                    break;
                }
            }

            if (current == null)
                return null;

            for (int i = 1; i < segments.Length; i++)
            {
                var child = current.transform.Find(segments[i]);
                if (child == null)
                    return null;
                current = child.gameObject;
            }

            return current;
        }

        public static string Declonify(string name)
        {
            return name.Replace("(Clone)", "").Trim();
        }

        public static GameObject Declonify(this GameObject gameObject)
        {
            gameObject.name = Declonify(gameObject.name);
            return gameObject;
        }
    }
}