using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverCore.Utilities;

namespace WeaverCore.DataTypes
{
	public struct Milestone
	{
		public int HealthNumber;
		public Action MilestoneReached;

		public Milestone(int health, Action action)
		{
			HealthNumber = health;
			MilestoneReached = action;
		}

		class ComparerDef : IComparer<Milestone>
		{
			public int Compare(Milestone x, Milestone y)
			{
				return x.HealthNumber - y.HealthNumber;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is Milestone)
			{
				var other = (Milestone)obj;

				return HealthNumber == other.HealthNumber && MilestoneReached == other.MilestoneReached;
			}
			return false;
		}

		public static IComparer<Milestone> Comparer = new ComparerDef();

		public override int GetHashCode()
		{
			int hash = 0;
			MiscUtilities.AdditiveHash(ref hash, HealthNumber.GetHashCode());
			MiscUtilities.AdditiveHash(ref hash, MilestoneReached.GetHashCode());
			return hash;
		}

		public static bool operator ==(Milestone left, Milestone right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Milestone left, Milestone right)
		{
			return !(left == right);
		}
	}
}
