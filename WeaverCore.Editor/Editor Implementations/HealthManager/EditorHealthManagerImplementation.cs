using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.Editor.Implementations
{
	class EditorHealthManagerImplementation : WeaverCore.Implementations.HealthManagerImplementation
    {
        public override void OnDeath()
        {
            throw new NotImplementedException();
        }

        public override void ReceiveHit(HitInfo info)
        {
            throw new NotImplementedException();
        }
    }
}
