using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;

namespace WeaverCore.Features
{
    [ShowFeature]
    public class EnemyReplacement : Enemy, IObjectReplacement
    {
        [SerializeField]
        private string enemyToReplace = "";

        public string ThingToReplace
        {
            get
            {
                return enemyToReplace;
            }
        }
    }
}
