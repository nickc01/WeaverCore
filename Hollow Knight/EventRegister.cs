//using Language;
using System.Collections.Generic;
using UnityEngine;

public class EventRegister : MonoBehaviour
{
	public delegate void RegisteredEvent();

	public static Dictionary<string, List<EventRegister>> eventRegister = new Dictionary<string, List<EventRegister>>();

	[SerializeField]
	private string subscribedEvent = "";

	public event RegisteredEvent OnReceivedEvent;

	private void Awake()
	{
		SubscribeEvent(this);
	}

	private void OnDestroy()
	{
		UnsubscribeEvent(this);
	}

	public void ReceiveEvent()
	{
		if (this.OnReceivedEvent != null)
		{
			this.OnReceivedEvent();
		}
	}

	public void SwitchEvent(string eventName)
	{
		UnsubscribeEvent(this);
		subscribedEvent = eventName;
		SubscribeEvent(this);
	}

	public static void SendEvent(string eventName)
	{
		if (eventName == "" || !eventRegister.ContainsKey(eventName))
		{
			return;
		}
		foreach (EventRegister item in eventRegister[eventName])
		{
			item.ReceiveEvent();
		}
	}

	public static void SubscribeEvent(EventRegister register)
	{
		List<EventRegister> list = null;
		string key = register.subscribedEvent;
		if (eventRegister.ContainsKey(key))
		{
			list = eventRegister[key];
		}
		else
		{
			list = new List<EventRegister>();
			eventRegister.Add(key, list);
		}
		list.Add(register);
	}

	public static void UnsubscribeEvent(EventRegister register)
	{
		string key = register.subscribedEvent;
		if (eventRegister.ContainsKey(key))
		{
			List<EventRegister> list = eventRegister[key];
			if (list.Contains(register))
			{
				list.Remove(register);
			}
			if (list.Count <= 0)
			{
				eventRegister.Remove(key);
			}
		}
	}
}
