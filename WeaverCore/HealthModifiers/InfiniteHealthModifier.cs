using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Interfaces;

namespace WeaverCore
{
    /// <summary>
    /// When added to an <see cref="Components.EntityHealth"/> component, will cause the health to increase infinitely
    /// </summary>
    public class InfiniteHealthModifier : IHealthModifier
    {
        public int Priority { get; set; } = int.MaxValue / 2;

        public int OnHealthChange(int oldHealth, int newHealth)
        {
            return oldHealth + Mathf.Abs(oldHealth - newHealth);
        }
    }
}
