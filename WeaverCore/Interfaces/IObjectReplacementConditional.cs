using UnityEngine;

namespace WeaverCore.Interfaces
{
    /// <summary>
    /// Should be used in conjunction with <see cref="IObjectReplacement"/>. This allows you to specify if an enemy should replace an object only under a certain condition
    /// </summary>
    public interface IObjectReplacementConditional
    {
		/// <summary>
		/// Can this object replace the specified object?
		/// </summary>
		bool CanReplaceObject(GameObject objectToReplace);
    }
}
