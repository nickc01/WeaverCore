using System.Collections.Generic;
using System.Reflection;
using WeaverCore.Settings;

namespace WeaverCore.Editor.Settings
{
    public class MemberInfoSorter : IComparer<MemberInfo>
    {
        Dictionary<MemberInfo, int> OrderCache = new Dictionary<MemberInfo, int>();

        Comparer<int> intComparer = Comparer<int>.Default;

        public int Compare(MemberInfo x, MemberInfo y)
        {
            return intComparer.Compare(GetOrder(x), GetOrder(y));
        }

        int GetOrder(MemberInfo info)
        {
            if (OrderCache.TryGetValue(info, out var value))
            {
                return value;
            }
            else
            {
                var orderAttribute = info.GetCustomAttribute<SettingOrderAttribute>();
                if (orderAttribute != null)
                {
                    OrderCache.Add(info, orderAttribute.Order);
                    return orderAttribute.Order;
                }
                else
                {
                    OrderCache.Add(info, info.MetadataToken);
                    return info.MetadataToken;
                }
            }
        }
    }
}
