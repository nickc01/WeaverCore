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

namespace WeaverCore.Utilities
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
        //public static Thread CheckingThread = null;

        public static PropertyManager_I Manager = null;

        //public static List<IPropertyTableBase> propertyTables = new List<IPropertyTableBase>();

        //static bool Ending = false;

        //static void OnGameQuit()
        //{
        //    Ending = true;
        //}

        /*public static void CheckLoop()
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
            catch (System.Threading.ThreadAbortException)
            {

            }
            catch (Exception e)
            {
                Debugger.LogError("CLEANER ERROR -> " + e);
            }
        }

        public static void Clean(IPropertyTableBase table)
        {
            //Debugger.Log("Locking Thread");
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
        }*/
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

        //void StartChecker()
        //{

            
            /*if (PropertyHolder.CheckingThread == null)
            {
                PropertyHolder.CheckingThread = new Thread(PropertyHolder.CheckLoop);
                PropertyHolder.CheckingThread.Start();
            }*/
            /*lock (PropertyHolder.TableLock)
            {
                PropertyHolder.propertyTables.Add(this);
            }*/
        //}

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
            //Modding.Logger.Log("LOCKERA = " + Thread.CurrentThread.GetHashCode());
            lock (Lock)
            {
                /*if (!usedOnce)
                {
                    usedOnce = true;
                    StartChecker();
                }*/
                var reference = new WeakReference<InstanceType>(instance);
                if (properties.ContainsKey(reference))
                {
                    //Modding.Logger.Log("UNLOCKA = " + Thread.CurrentThread.GetHashCode());
                    return properties[reference];
                }
                else
                {
                    var prop = Factory();
                    //OnAdd(reference, prop);
                    properties.Add(reference, prop);
                    //Modding.Logger.Log("UNLOCKA = " + Thread.CurrentThread.GetHashCode());
                    return prop;
                }
            }
        }

        public bool Contains(InstanceType instance)
        {
            //Modding.Logger.Log("LOCKERB = " + Thread.CurrentThread.GetHashCode());
            lock (Lock)
            {
                /*if (!usedOnce)
                {
                    usedOnce = true;
                    StartChecker();
                }*/
                var reference = new WeakReference<InstanceType>(instance);
                //Modding.Logger.Log("UNLOCKB = " + Thread.CurrentThread.GetHashCode());
                return properties.ContainsKey(reference);
            }
        }

        public PropertyType Get(InstanceType instance)
        {
            //Modding.Logger.Log("LOCKERC = " + Thread.CurrentThread.GetHashCode());
            lock (Lock)
            {
                /*if (!usedOnce)
                {
                    usedOnce = true;
                    StartChecker();
                }*/
                var reference = new WeakReference<InstanceType>(instance);
                if (properties.ContainsKey(reference))
                {
                    //Modding.Logger.Log("UNLOCKC = " + Thread.CurrentThread.GetHashCode());
                    return properties[reference];
                }
                else
                {
                    //Modding.Logger.Log("UNLOCKC = " + Thread.CurrentThread.GetHashCode());
                    return null;
                }
            }
        }

        public bool GetIfExists(InstanceType instance,out PropertyType value)
        {
            lock (Lock)
            {
                /*if (!usedOnce)
                {
                    usedOnce = true;
                    StartChecker();
                }*/
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
            //Modding.Logger.Log("LOCKERD = " + Thread.CurrentThread.GetHashCode());
            lock (Lock)
            {
                /*if (!usedOnce)
                {
                    usedOnce = true;
                    StartChecker();
                }*/
                var reference = new WeakReference<InstanceType>(instance);
                if (properties.ContainsKey(reference))
                {
                    //OnRemoval(reference, properties[reference]);
                    properties.Remove(reference);
                    //Modding.Logger.Log("UNLOCKD = " + Thread.CurrentThread.GetHashCode());
                    return true;
                }
                else
                {
                    //Modding.Logger.Log("UNLOCK = " + Thread.CurrentThread.GetHashCode());
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
                //Modding.Logger.Log("LOCKERE = " + Thread.CurrentThread.GetHashCode());
                /*lock (InternalInfo.TableLock)
                {
                    if (usedOnce)
                    {
                        InternalInfo.propertyTables.Remove(this);
                    }
                    //Modding.Logger.Log("UNLOCKE = " + Thread.CurrentThread.GetHashCode());
                }*/
                GC.SuppressFinalize(this);
            }
        }

        void IPropertyTableBase.Remove(EquatableWeakReference instance)
        {
            //Modding.Logger.Log("LOCKERF = " + Thread.CurrentThread.GetHashCode());
            lock (Lock)
            {
                /*if (!usedOnce)
                {
                    usedOnce = true;
                    StartChecker();
                }*/
                var reference = new WeakReference<InstanceType>(instance);
                if (properties.ContainsKey(reference))
                {
                    //OnRemoval(reference, properties[reference]);
                    properties.Remove(reference);
                }
               // Modding.Logger.Log("UNLOCKF = " + Thread.CurrentThread.GetHashCode());
            }
        }

        /*protected virtual void OnRemoval(WeakReference<InstanceType> instance, PropertyType properties )
        {

        }

        protected virtual void OnAdd(WeakReference<InstanceType> instance, PropertyType properties)
        {

        }*/

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
