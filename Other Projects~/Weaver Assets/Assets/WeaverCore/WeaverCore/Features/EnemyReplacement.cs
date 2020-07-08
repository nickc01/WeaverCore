using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore.Features
{
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
