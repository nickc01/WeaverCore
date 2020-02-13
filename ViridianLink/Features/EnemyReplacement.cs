using UnityEngine;

namespace ViridianLink.Features
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
