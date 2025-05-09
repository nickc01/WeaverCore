using System.Collections;
using WeaverCore.Utilities;

namespace WeaverCore.Playmaker
{
    public class EnumWeaverFSMAction : WeaverFSMAction
    {
        protected IEnumerator routine;

        public EnumWeaverFSMAction(IEnumerator routine)
        {
            this.routine = CoroutineUtilities.RunWhile(routine, () => true);
        }

        public override void OnUpdate()
        {
            if (!routine.MoveNext())
            {
                Finish();
            }
        }
    }
}