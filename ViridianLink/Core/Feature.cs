using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ViridianLink.Core
{
    public abstract class Feature : MonoBehaviour
    {
        [SerializeField]
        private bool featureEnabled = true;

        public virtual bool FeatureEnabled
        {
            get => featureEnabled;
            set => featureEnabled = value;
        }
    }

    public abstract class EnemyReplacement : Feature
    {
        [SerializeField]
        private string enemyToReplace = "";

        public virtual string EnemyToReplace
        {
            get => enemyToReplace;
            set => enemyToReplace = value;
        }

    }

    public class EnemyTesting : EnemyReplacement
    {

    }
}
