using System;

namespace WeaverCore.Interfaces
{
	// Token: 0x0200007E RID: 126
	public interface IDeathEffects
	{
		// Token: 0x06000253 RID: 595
		void PlayDeathEffects(HitInfo lastHit);

		// Token: 0x06000254 RID: 596
		void EmitEffects();
	}
}