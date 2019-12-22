using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CrystalCore.Machine
{
    [JsonObject(MemberSerialization.Fields)]
    public class XMachine : IEnumerable<XState>
    {
        private string initial;
        [JsonIgnore]
        private XState initial_State;
        [JsonIgnore]
        public XState InitialState
        {
            get => InitialState;
            set
            {
                initial_State = value;
                initial = value.Name;
            }
        }
        private string id;
        [JsonIgnore]
        public string Name
        {
            get => id;
            set => id = value;
        }

        private Dictionary<string, XState> states = new Dictionary<string, XState>();

        public void AddState(XState state)
        {
            states.Add(state.Name,state);
        }

        public void RemoveState(XState state)
        {
            states.Remove(state.Name);
        }

        public override string ToString()
        {
            return Name;
        }

        public void Add(XState state)
        {
            AddState(state);
        }

        IEnumerator<XState> IEnumerable<XState>.GetEnumerator()
        {
            foreach (var state in states)
            {
                yield return state.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var state in states)
            {
                yield return state.Value;
            }
        }

        public XMachine(string name,XState initialState, List<XState> states = null)
        {
            Name = name;
            InitialState = initialState;
            if (states != null)
            {
                foreach (var state in states)
                {
                    AddState(state);
                }
            }
        }
        public XMachine(string name)
        {
            Name = name;
        }
    }

    [JsonObject(MemberSerialization.Fields)]
    public struct XState : IEnumerable<XEvent>
    {
        [JsonIgnore]
        private string internal_name;
        [JsonIgnore]
        public string Name
        {
            get => internal_name;
            set => internal_name = value;
        }

        private Dictionary<string, string> on;
        private List<string> entry;

        [JsonIgnore]
        private List<XEvent> addedEvents;

        public void AddEvent(XEvent e)
        {
            addedEvents.Add(e);
            on.Add(e.Name, e.ToState.Name);
        }

        public void Add(XEvent e)
        {
            AddEvent(e);
        }

        public void AddAction(string action)
        {
            entry.Add(action);
        }

        public void RemoveAction(string action)
        {
            entry.Remove(action);
        }

        public void RemoveEvent(XEvent e)
        {
            if (addedEvents.Contains(e))
            {
                addedEvents.Remove(e);
                on.Remove(e.Name);
            }
        }

        public XState(string name,List<XEvent> events = null,List<string> Actions = null)
        {
            entry = new List<string>();
            addedEvents = new List<XEvent>();
            on = new Dictionary<string, string>();
            internal_name = name;
            if (events != null)
            {
                foreach (var e in events)
                {
                    AddEvent(e);
                }
            }
            if (Actions != null)
            {
                foreach (var action in Actions)
                {
                    entry.Add(action);
                }
            }
        }

        public XState(string name,XEvent[] events, string[] Actions) : this(name,events.ToList(),Actions.ToList())
        {

        }
        public override bool Equals(object obj)
        {
            if (obj is XState state)
            {
                return state.Name == state.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        IEnumerator<XEvent> IEnumerable<XEvent>.GetEnumerator()
        {
            return addedEvents.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return addedEvents.GetEnumerator();
        }
    }

    [JsonObject(MemberSerialization.Fields)]
    public struct XEvent
    {
        [JsonIgnore]
        public string Name { get; set; }

        [JsonIgnore]
        public XState ToState { get; set; }

        public XEvent(string name, XState toState)
        {
            Name = name;
            ToState = toState;
        }

        public override bool Equals(object obj)
        {
            if (obj is XEvent e)
            {
                return e.Name == Name && e.ToState.Equals(ToState);
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Name.GetHashCode() + ToState.GetHashCode();
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
