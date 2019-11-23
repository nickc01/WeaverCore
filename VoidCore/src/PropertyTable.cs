using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Reflection;
using VoidCore.Helpers;

namespace VoidCore
{
    public class EquatableWeakReference : WeakReference
    {
        int hashCode;

        public EquatableWeakReference(object target) : base(target)
        {
            hashCode = target.GetHashCode();
        }

        public EquatableWeakReference(object target, bool trackResurrection) : base(target, trackResurrection)
        {
            hashCode = Target.GetHashCode();
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return hashCode == obj.GetHashCode();
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
            get => (T)Reference.Target;
            set => Reference.Target = value;
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
                return Target.GetHashCode();
            }
            else
            {
                return Reference.GetHashCode();
            }
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

    static class PropertyHolder
    {
        public static Thread CheckingThread = null;

        public static List<IPropertyTableBase> propertyTables = new List<IPropertyTableBase>();

        static int index = 0;
        public static object TableLock = new object();

        //static List<EquatableWeakReference> KeyList = new List<EquatableWeakReference>(100);
        //static int KeyCount = 0;

        public static void CheckLoop()
        {
            try
            {
                while (true)
                {
                    lock (TableLock)
                    {
                        index++;
                        if (index >= propertyTables.Count)
                        {
                            index = 0;
                        }
                        if (propertyTables.Count > 0)
                        {
                            Clean(propertyTables[index]);
                        }
                        Thread.Sleep((int)(10000.0f / (propertyTables.Count + 1)));
                    }
                }
            }
            catch (Exception e)
            {
                Modding.Logger.Log("CLEANER ERROR -> " + e);
            }
        }

        public static void Clean(IPropertyTableBase table)
        {
            lock (table.Lock)
            {
                foreach (var key in table.Keys.ToList())
                {
                    if (!table.Validate(key))
                    {
                        table.Remove(key);
                    }
                }
            }
        }
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
        public static Func<UnityEngine.Object, bool> IsNativeObjectAlive = Methods.GetFunction<Func<UnityEngine.Object, bool>, UnityEngine.Object>("IsNativeObjectAlive");
        //public static Func<UnityEngine.Object, bool> IsNativeObjectAlive = (Func<UnityEngine.Object, bool>)Delegate.CreateDelegate(typeof(Func<UnityEngine.Object, bool>), typeof(UnityEngine.Object).GetMethod("IsNativeObjectAlive", BindingFlags.Static | BindingFlags.NonPublic));
    }


    public class PropertyTable<InstanceType, PropertyType> : IPropertyTableBase, IDisposable where InstanceType : class where PropertyType : class, new()
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

        Dictionary<WeakReference<InstanceType>, PropertyType> properties = new Dictionary<WeakReference<InstanceType>, PropertyType>(new Comparer());

        public PropertyTable()
        {
            if (PropertyHolder.CheckingThread == null)
            {
                PropertyHolder.CheckingThread = new Thread(PropertyHolder.CheckLoop);
                PropertyHolder.CheckingThread.Start();
            }
            lock (PropertyHolder.TableLock)
            {
                PropertyHolder.propertyTables.Add(this);
            }
        }

        public PropertyType GetOrCreate(InstanceType instance)
        {
            return GetOrCreate(instance, () => new PropertyType());
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
                    OnAdd(reference, prop);
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

        public bool Remove(InstanceType instance)
        {
            lock (Lock)
            {
                var reference = new WeakReference<InstanceType>(instance);
                if (properties.ContainsKey(reference))
                {
                    OnRemoval(reference, properties[reference]);
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
            lock (PropertyHolder.TableLock)
            {
                PropertyHolder.propertyTables.Remove(this);
            }
            GC.SuppressFinalize(this);
        }

        void IPropertyTableBase.Remove(EquatableWeakReference instance)
        {
            lock (Lock)
            {
                var reference = new WeakReference<InstanceType>(instance);
                if (properties.ContainsKey(reference))
                {
                    OnRemoval(reference, properties[reference]);
                    properties.Remove(reference);
                }
            }
        }

        protected virtual void OnRemoval(WeakReference<InstanceType> instance, PropertyType properties )
        {

        }

        protected virtual void OnAdd(WeakReference<InstanceType> instance, PropertyType properties)
        {

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
}
