using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore
{
    /// <summary>
    /// Contains a list of objects that are replacing existing ones in the game.
    /// 
    /// This is the main mechanism used for replacing old enemies with new ones
    /// </summary>
    public static class EntityReplacements
    {
        static Dictionary<uint, (string objectName, GameObject replacement, Func<GameObject, bool> condition)> replacements = new Dictionary<uint, (string objectName, GameObject replacement, Func<GameObject, bool> condition)>();

        static uint replacementCounter = 0;


        [WeaverCore.Attributes.OnFeatureLoad]
        static void ReplacementAdded(IObjectReplacement replacement)
        {
            if (replacement is Component c)
            {
                if (replacement is IObjectReplacementConditional conditional)
                {
                    AddReplacement(replacement.ThingToReplace, c.gameObject,conditional.CanReplaceObject);
                }
                else
                {
                    AddReplacement(replacement.ThingToReplace, c.gameObject);
                }
            }
        }

        [WeaverCore.Attributes.OnFeatureUnload]
        static void ReplacementRemoved(IObjectReplacement replacement)
        {
            if (replacement is Component c)
            {
                RemoveReplacement(replacement.ThingToReplace, c.gameObject);
            }
        }

        /// <summary>
        /// Replaces an existing object with a new one
        /// </summary>
        /// <param name="nameToReplace">The name of the object to be replaced</param>
        /// <param name="replacement">The object to replace it with</param>
        /// <returns>The ID of the new replacement. This is used in <see cref="RemoveReplacement(uint)"/> to undo the replacement</returns>
        public static uint AddReplacement(string nameToReplace, GameObject replacement)
        {
            replacementCounter++;

            replacements.Add(replacementCounter, (nameToReplace, replacement,gm => true));

            return replacementCounter;
        }

        /// <summary>
        /// Replaces an existing object with a new one
        /// </summary>
        /// <param name="nameToReplace">The name of the object to be replaced</param>
        /// <param name="replacement">The object to replace it with</param>
        /// <param name="condition">Used to only allow the replacement to occur under a certain condition</param>
        /// <returns>The ID of the new replacement. This is used in <see cref="RemoveReplacement(uint)"/> to undo the replacement</returns>
        public static uint AddReplacement(string nameToReplace, GameObject replacement, Func<GameObject, bool> condition)
        {
            replacementCounter++;

            replacements.Add(replacementCounter, (nameToReplace, replacement,condition));

            return replacementCounter;
        }

        /// <summary>
        /// Removes a replacement for an object
        /// </summary>
        /// <param name="replacementID">The ID of the replacement. You obtain one from adding a replacement via <see cref="AddReplacement(string, GameObject)"/></param>
        public static bool RemoveReplacement(uint replacementID)
        {
            return replacements.Remove(replacementID);
        }

        /// <summary>
        /// Removes all replacements for an object
        /// </summary>
        /// <param name="nameToReplace">The name of the object to be replaced</param>
        public static void RemoveAllReplacements(string nameToReplace)
        {
            List<uint> idsToRemove = new List<uint>();

            foreach (var replacement in replacements)
            {
                if (replacement.Value.objectName == nameToReplace)
                {
                    idsToRemove.Add(replacement.Key);
                }
            }

            foreach (var id in idsToRemove)
            {
                replacements.Remove(id);
            }
        }

        /// <summary>
        /// Removes a replacement
        /// </summary>
        /// <param name="nameToReplace">The name of the object being replaced</param>
        /// <param name="replacement">The object to replace it with</param>
        /// <returns>Returns true if the replacement has been removed</returns>
        public static bool RemoveReplacement(string nameToReplace, GameObject replacement)
        {
            List<uint> idsToRemove = new List<uint>();

            foreach (var pair in replacements)
            {
                if (pair.Value.objectName == nameToReplace && pair.Value.replacement == replacement)
                {
                    idsToRemove.Add(pair.Key);
                }
            }

            foreach (var id in idsToRemove)
            {
                replacements.Remove(id);
            }

            return idsToRemove.Count > 0;
        }

        /// <summary>
        /// Does the object name have replacements added?
        /// </summary>
        /// <param name="nameToReplace">The name of the object to replace</param>
        /// <returns>Returns whether the object has replacements</returns>
        public static bool HasReplacements(string nameToReplace)
        {
            return replacements.Any(pair => pair.Value.objectName == nameToReplace);
        }

        /// <summary>
        /// Does the object have replacements for it?
        /// </summary>
        /// <param name="objectToReplace">The object to be replaced</param>
        /// <returns>Returns whether the object has replacements for it</returns>
        public static bool HasReplacements(GameObject objectToReplace)
        {
            return HasReplacements(objectToReplace.name);
        }

        /// <summary>
        /// Replaces an object with new ones
        /// </summary>
        /// <param name="objectToReplace">The object to be replaced</param>
        /// <param name="newObjects">The new replacement objects that have been created</param>
        /// <param name="destroyOriginal">Should the original object be destroyed if it was successfully replaced?</param>
        /// <returns>Has the object been replaced?</returns>
        public static bool ReplaceObject(GameObject objectToReplace, out List<GameObject> newObjects, bool destroyOriginal = true)
        {
            newObjects = new List<GameObject>();
            foreach (var pair in replacements)
            {
                if ((pair.Value.objectName == null || objectToReplace.name == pair.Value.objectName) && pair.Value.condition(objectToReplace))
                {
                    var replacement = GameObject.Instantiate(pair.Value.replacement, objectToReplace.transform.position, objectToReplace.transform.rotation);
                    newObjects.Add(replacement);
                }
            }
            if (newObjects.Count > 0 && destroyOriginal)
            {
                GameObject.Destroy(objectToReplace);
            }
            return newObjects.Count > 0;
        }
    }
}
