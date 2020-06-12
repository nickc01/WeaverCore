﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore;
using WeaverCore.Utilities;

namespace WeaverTools.Machine
{
    //[JsonObject(MemberSerialization.Fields)]
    public class XMachine : IEnumerable<XState>
    {
        [Serializable]
        internal class SerializedData
        {
            public string initial;
            public string id;
            public Dictionary<string, XState.SerializedData> states = new Dictionary<string, XState.SerializedData>();
        }

        SerializedData jsonData = new SerializedData();

        //private string initial;
        //[JsonIgnore]
        private XState initial_State;
        //[JsonIgnore]
        public XState InitialState
        {
            get => InitialState;
            set
            {
                initial_State = value;
                jsonData.initial = value.Name;
            }
        }
        //private string id;
        //[JsonIgnore]
        public string Name
        {
            get => jsonData.id;
            set => jsonData.id = value;
        }

        private Dictionary<string, XState> states = new Dictionary<string, XState>();

        public void AddState(XState state)
        {
            states.Add(state.Name, state);
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

        public XMachine(string name, XState initialState, List<XState> states = null)
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

        public string Serialize()
        {
            jsonData.states.Clear();
            foreach (var state in states)
            {
                jsonData.states.Add(state.Key, state.Value.jsonData);
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("{\"id\": \"");
            builder.Append(jsonData.id);
            builder.Append("\",\"initial\": \"");
            builder.Append(jsonData.initial);
            builder.Append("\",\"states\": {");
            if (jsonData.states.Count > 0)
            {
                foreach (var state in jsonData.states)
                {
                    //final += "\"" + state.Key + "\": { \"on\": {";
                    builder.Append("\"");
                    builder.Append(state.Key);
                    builder.Append("\": { \"on\": {");
                    if (state.Value.on.Count > 0)
                    {
                        foreach (var onVal in state.Value.on)
                        {
                            //final += "\"" + onVal.Key + "\": \"" + onVal.Value + "\",";
                            builder.Append("\"");
                            builder.Append(onVal.Key);
                            builder.Append("\": \"");
                            builder.Append(onVal.Value);
                            builder.Append("\",");
                        }
                        //final = final.Remove(final.Length - 1, 1);
                        builder.Remove(builder.Length - 1, 1);
                        //final += "},";
                        builder.Append("},");
                    }
                    else
                    {
                        //final += "},";
                        builder.Append("},");
                    }
                    //final += @"""entry"": [";
                    builder.Append("\"entry\": [");

                    if (state.Value.entry.Count > 0)
                    {
                        foreach (var entryVal in state.Value.entry)
                        {
                            //final += "\"" + entryVal + "\",";
                            builder.Append("\"");
                            builder.Append(entryVal);
                            builder.Append("\",");
                            //final += @"""" + onVal.Key + @""": """ + onVal.Value + @""",";

                        }
                        //final = final.Remove(final.Length - 1, 1);
                        builder.Remove(builder.Length - 1, 1);
                        //final += "]";
                        builder.Append("]");
                    }
                    else
                    {
                        //final += "]";
                        builder.Append("]");
                    }
                    //final += @"},";
                    builder.Append("},");
                }
                //final = final.Remove(final.Length - 1, 1);
                builder.Remove(builder.Length - 1, 1);
                //final += @"}";
                builder.Append("}");
            }
            else
            {
                //final += @"}";
                builder.Append("}");
            }
            //final += @"}";
            builder.Append("}");

            //return final;
            return builder.ToString();
            /*jsonData.states.Clear();
            foreach (var state in states)
            {
                jsonData.states.Add(state.Key, state.Value.jsonData);
            }
            Debugger.Log("States = " + jsonData.states.Count);
            //return Json.Serialize(jsonData);
            return JsonUtility.ToJson(jsonData);*/
        }

        /*public static XMachine Deserialize(string serializedData)
        {
            SerializedData data = JsonUtility.FromJson<SerializedData>(serializedData);

            XMachine machine = new XMachine(data.id);

            machine.states.Clear();

            //List<XEvent> events = new List<XEvent>();

            foreach (var serializedState in data.states)
            {
                XState state = new XState(serializedState.Key);

                foreach (var serializedEvent in serializedState.Value.on)
                {
                    state.addedEvents.Add(new XEvent(serializedEvent.Key, new XState(serializedEvent.Value)));
                }

                machine.states.Add(state.Name, state);
            }
            return machine;
        }*/
    }

    //[JsonObject(MemberSerialization.Fields)]
    public struct XState : IEnumerable<XEvent>
    {
        [Serializable]
        internal class SerializedData
        {
            public Dictionary<string, string> on;
            public List<string> entry;
        }

        internal SerializedData jsonData;

        //[JsonIgnore]
        private string internal_name;
        //[JsonIgnore]
        public string Name
        {
            get => internal_name;
            set => internal_name = value;
        }

        //private Dictionary<string, string> on;
        //private List<string> entry;

        //[JsonIgnore]
        internal List<XEvent> addedEvents;


        public void AddEvent(XEvent e)
        {
            addedEvents.Add(e);
            jsonData.on.Add(e.Name, e.ToState.Name);
        }

        public void Add(XEvent e)
        {
            AddEvent(e);
        }

        public void AddAction(string action)
        {
            jsonData.entry.Add(action);
        }

        public void RemoveAction(string action)
        {
            jsonData.entry.Remove(action);
        }

        public void RemoveEvent(XEvent e)
        {
            if (addedEvents.Contains(e))
            {
                addedEvents.Remove(e);
                jsonData.on.Remove(e.Name);
            }
        }

        public XState(string name, List<XEvent> events = null, List<string> Actions = null)
        {
            jsonData = new SerializedData();
            jsonData.entry = new List<string>();
            addedEvents = new List<XEvent>();
            jsonData.on = new Dictionary<string, string>();
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
                    jsonData.entry.Add(action);
                }
            }
        }

        public XState(string name, XEvent[] events, string[] Actions) : this(name, events.ToList(), Actions.ToList())
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

    //[JsonObject(MemberSerialization.Fields)]
    public struct XEvent
    {

        //[JsonIgnore]
        public string Name { get; set; }

        //[JsonIgnore]
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
