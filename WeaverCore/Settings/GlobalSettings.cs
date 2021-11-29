using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Settings
{
    [ShowFeature]
    public abstract class GlobalSettings : ScriptableObject
    {
        public virtual string Title => StringUtilities.Prettify(GetType().FullName.Replace("Panel", ""));



        /// <summary>
        /// Called after the data has been loaded from a file
        /// </summary>
        public virtual void OnLoad()
        {

        }

        /// <summary>
        /// Called before the data is saved to a file
        /// </summary>
        public virtual void OnSave()
        {

        }

        /// <summary>
        /// Called when the menu for this global settings object has been opened
        /// </summary>
        public virtual void OnOpen()
        {

        }

        /// <summary>
        /// Called when the menu for this global settings object has been closed
        /// </summary>
        public virtual void OnClose()
        {

        }

    }
}
