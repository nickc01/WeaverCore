using UnityEngine;

namespace WeaverCore.Features
{
	public class EnemyReplacement : Enemy
    {
        [SerializeField]
        private string enemyToReplace = "";

        public virtual string EnemyToReplace
        {
            get => enemyToReplace;
            set => enemyToReplace = value;
        }

    }
}
