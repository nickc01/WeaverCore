using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Utilities;

namespace WeaverCore
{
	public struct HealthMilestone
	{
		public int HealthNumber;
		public Action MilestoneReached;

		public HealthMilestone(int health, Action action)
		{
			HealthNumber = health;
			MilestoneReached = action;
		}

		class ComparerDef : IComparer<HealthMilestone>
		{
			public int Compare(HealthMilestone x, HealthMilestone y)
			{
				return x.HealthNumber - y.HealthNumber;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is HealthMilestone)
			{
				var other = (HealthMilestone)obj;

				return HealthNumber == other.HealthNumber && MilestoneReached == other.MilestoneReached;
			}
			return false;
		}

		public static IComparer<HealthMilestone> Comparer = new ComparerDef();

		public override int GetHashCode()
		{
			int hash = 0;
			HashUtilities.AdditiveHash(ref hash, HealthNumber.GetHashCode());
			HashUtilities.AdditiveHash(ref hash, MilestoneReached.GetHashCode());
			return hash;
		}

		public static bool operator ==(HealthMilestone left, HealthMilestone right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(HealthMilestone left, HealthMilestone right)
		{
			return !(left == right);
		}
	}
}
