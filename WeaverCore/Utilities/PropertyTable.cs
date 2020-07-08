using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Reflection;
using WeaverCore.Utilities;
using WeaverCore.Implementations;

/*namespace WeaverCore.Utilities
{
    public class EquatableWeakReference : WeakReference
    {
        int hashCode;

        public EquatableWeakReference(object target) : base(target)
        {
            if (target == null)
            {
                hashCode = 0;
            }
            else
            {
                if (target is UnityEngine.Object obj)
                {
                    hashCode = obj.GetInstanceID();
                }
                else
                {
                    hashCode = target.GetHashCode();
                }
            }
        }

        public EquatableWeakReference(object target, bool trackResurrection) : base(target, trackResurrection)
        {
            if (target == null)
            {
                hashCode = 0;
            }
            else
            {
                if (target is UnityEngine.Object obj)
                {
                    hashCode = obj.GetInstanceID();
                }
                else
                {
                    hashCode = target.GetHashCode();
                }
            }
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public static bool operator==(EquatableWeakReference a, EquatableWeakReference b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(EquatableWeakReference a, EquatableWeakReference b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            if (obj is UnityEngine.Object unityObject)
            {
                return hashCode == unityObject.GetInstanceID();
            }
            else
            {
                return hashCode == obj.GetHashCode();
            }
        }
    }


    public class WeakReference<T>
    {
        public EquatableWeakReference Reference { get; private set; }

        public WeakReference(EquatableWeakReference reference)
        {
            Reference = reference;
        }

        public WeakReference(T target)
        {
            Reference = new EquatableWeakReference(target);
        }

        public WeakReference(T target, bool trackResurrection)
        {
            Reference = new EquatableWeakReference(target, trackResurrection);
        }

        public T Target
        {
            get { return (T)Reference.Target; }
            set { Reference.Target = value; }
        }

        public bool IsAlive => Reference.IsAlive;

        public bool TrackResurrection => Reference.TrackResurrection;

        public override string ToString()
        {
            return Reference.ToString();
        }

        public override int GetHashCode()
        {
            if (IsAlive)
            {
                if (Target is UnityEngine.Object unityObject)
                {
                    return unityObject.GetInstanceID();
                }
                else
                {
                    return Target.GetHashCode();
                }
            }
            else
            {
                return Reference.GetHashCode();
            }
        }

        public static bool operator ==(WeakReference<T> a, WeakReference<T> b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(WeakReference<T> a, WeakReference<T> b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            if (obj is WeakReference<T> refT)
            {
                return refT.Reference == Reference;
            }
            else if (obj is EquatableWeakReference r)
            {
                return r == Reference;
            }
            return false;
        }
    }

    static class InternalInfo
    {
        public static PropertyManager_I Manager = null;
    }

    public interface IPropertyTableBase
    {
        void Remove(EquatableWeakReference instance);
        bool Validate(EquatableWeakReference instance);
        IEnumerable<EquatableWeakReference> Keys { get; }
        object Lock { get; }
    }

    static class InternalStatics
    {
        public static Func<UnityEngine.Object, bool> IsNativeObjectAlive; // = Methods.GetFunction<Func<UnityEngine.Object, bool>, UnityEngine.Object>("IsNativeObjectAlive");
        public static Func<UnityEngine.Object, IntPtr> GetCachedPtr = MethodUtilities.ConvertToDelegate<Func<UnityEngine.Object, IntPtr>, UnityEngine.Object>("GetCachedPtr");

        static InternalStatics()
        {
            IsNativeObjectAlive = o =>
            {
                return GetCachedPtr(o) != IntPtr.Zero;
            };
        }
        //public static Func<UnityEngine.Object, bool> IsNativeObjectAlive = (Func<UnityEngine.Object, bool>)Delegate.CreateDelegate(typeof(Func<UnityEngine.Object, bool>), typeof(UnityEngine.Object).GetMethod("IsNativeObjectAlive", BindingFlags.Static | BindingFlags.NonPublic));
    }


    public sealed class PropertyTable<InstanceType, PropertyType> : IPropertyTableBase, IDisposable where InstanceType : class where PropertyType : class, new()
    {
        class Comparer : EqualityComparer<WeakReference<InstanceType>>
        {
            public override bool Equals(WeakReference<InstanceType> x, WeakReference<InstanceType> y)
            {
                return x.Reference.Equals(y.Reference);
            }

            public override int GetHashCode(WeakReference<InstanceType> obj)
            {
                return obj.Reference.GetHashCode();
            }
        }

        bool usedOnce = false;
        bool disposed = false;

        Dictionary<WeakReference<InstanceType>, PropertyType> properties;


        public PropertyTable()
        {
            properties = new Dictionary<WeakReference<InstanceType>, PropertyType>();
            Setup();
        }

        public PropertyTable(IEqualityComparer<WeakReference<InstanceType>> comparer)
        {
            properties = new Dictionary<WeakReference<InstanceType>, PropertyType>(comparer);
            Setup();
        }

        void Setup()
        {
            if (InternalInfo.Manager == null)
            {
                InternalInfo.Manager = ImplFinder.GetImplementation<PropertyManager_I>();
                InternalInfo.Manager.Start();
            }
            InternalInfo.Manager.AddTable(this);
        }

        public PropertyType GetOrCreate(InstanceType instance)
        {
            return GetOrCreate(instance, () => new PropertyType());
        }


        /// <summary>
        /// Gets all the values stored in the property table.
        /// WARNING: MAKE SURE YOU LOCK THE <see cref="Lock"/> OBJECT BEFORE USING THIS OR ELSE THE LIST COULD CHANGE WHILE YOU ARE USING IT
        /// </summary>
        public IEnumerable<KeyValuePair<WeakReference<InstanceType>,PropertyType>> AllValues
        {
            get
            {
                return properties;
            }
        }

        public PropertyType GetOrCreate(InstanceType instance, Func<PropertyType> Factory)
        {
            lock (Lock)
            {
                var reference = new WeakReference<InstanceType>(instance);
                if (properties.ContainsKey(reference))
                {
                    return properties[reference];
                }
                else
                {
                    var prop = Factory();
                    properties.Add(reference, prop);
                    return prop;
                }
            }
        }

        public bool Contains(InstanceType instance)
        {
            lock (Lock)
            {
                var reference = new WeakReference<InstanceType>(instance);
                return properties.ContainsKey(reference);
            }
        }

        public PropertyType Get(InstanceType instance)
        {
            lock (Lock)
            {
                var reference = new WeakReference<InstanceType>(instance);
                if (properties.ContainsKey(reference))
                {
                    return properties[reference];
                }
                else
                {
                    return null;
                }
            }
        }

        public bool GetIfExists(InstanceType instance,out PropertyType value)
        {
            lock (Lock)
            {
                var reference = new WeakReference<InstanceType>(instance);
                if (properties.ContainsKey(reference))
                {
                    value = properties[reference];
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }
        }

        public bool Remove(InstanceType instance)
        {
            lock (Lock)
            {
                var reference = new WeakReference<InstanceType>(instance);
                if (properties.ContainsKey(reference))
                {
                    properties.Remove(reference);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                InternalInfo.Manager.RemoveTable(this);
                GC.SuppressFinalize(this);
            }
        }

        void IPropertyTableBase.Remove(EquatableWeakReference instance)
        {
            lock (Lock)
            {
                var reference = new WeakReference<InstanceType>(instance);
                if (properties.ContainsKey(reference))
                {
                    properties.Remove(reference);
                }
            }
        }

        bool IPropertyTableBase.Validate(EquatableWeakReference instance)
        {
            if (instance.IsAlive)
            {
                if (instance.Target is UnityEngine.Object obj)
                {
                    return InternalStatics.IsNativeObjectAlive(obj);
                }
                return true;
            }
            return false;
        }


        IEnumerable<EquatableWeakReference> IPropertyTableBase.Keys
        {
            get
            {
                foreach (var key in properties.Keys)
                {
                    yield return key.Reference;
                }
            }
        }

        public object Lock { get; } = new object();

        ~PropertyTable()
        {
            Dispose();
        }
    }
}*/
