using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore
{
    /// <summary>
    /// A feature that is to be added in the game. These are added to <see cref="Registry"/> to be activated
    /// </summary>
    public abstract class Feature : MonoBehaviour
    {
        [SerializeField]
        private bool featureEnabled = true;

        /// <summary>
        /// Whether the feature is enabled
        /// </summary>
        public virtual bool FeatureEnabled
        {
            get => featureEnabled;
            set => featureEnabled = value;
        }
    }
}
