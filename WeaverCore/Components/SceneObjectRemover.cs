using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    public class SceneObjectRemover : MonoBehaviour
    {
        [SerializeField]
        List<string> objectPaths = new List<string>();

        void Start()
        {
            foreach (string objectPath in objectPaths)
            {
                GameObject sceneObject = GameObjectUtilities.FindByFullPath(objectPath);
                if (sceneObject != null)
                {
                    Destroy(sceneObject);
                }
            }
        }
    }
}
