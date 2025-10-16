using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Playmaker
{
    /// <summary>
    /// Type-safe wrapper for FsmStateAction objects
    /// </summary>
    public struct FsmActionWrapper
	{
		public object InternalAction { get; }
		
		public FsmActionWrapper(object action)
		{
			InternalAction = action;
		}
		
		public object GetProperty(string propertyName)
		{
			return PlayMakerUtilities.GetActionProperty(InternalAction, propertyName);
		}
		
		public bool SetProperty(string propertyName, object value)
		{
			return PlayMakerUtilities.SetActionProperty(InternalAction, propertyName, value);
		}
		
		/// <summary>
		/// Gets whether this FSM action is enabled
		/// </summary>
		public bool Enabled
		{
			get
			{
				return PlayMakerUtilities.GetActionEnabled(InternalAction);
			}
			set
			{
				PlayMakerUtilities.SetActionEnabled(InternalAction, value);
			}
		}

		/// <summary>
		/// Gets or sets the name of this FSM action
		/// </summary>
		public string Name
		{
			get
			{
				return PlayMakerUtilities.GetActionProperty(InternalAction, "Name") as string;
			}
			set
			{
				PlayMakerUtilities.SetActionProperty(InternalAction, "Name", value);
			}
		}

		/// <summary>
		/// Gets or sets the display name of this FSM action
		/// </summary>
		public string DisplayName
		{
			get
			{
				return PlayMakerUtilities.GetActionProperty(InternalAction, "DisplayName") as string;
			}
			set
			{
				PlayMakerUtilities.SetActionProperty(InternalAction, "DisplayName", value);
			}
		}

		/// <summary>
		/// Gets the FSM that owns this action
		/// </summary>
		public FsmWrapper Fsm
		{
			get
			{
				return new FsmWrapper(PlayMakerUtilities.GetActionProperty(InternalAction, "Fsm"));
			}
		}

		/// <summary>
		/// Gets the GameObject that owns this action
		/// </summary>
		public GameObject Owner
		{
			get
			{
				return PlayMakerUtilities.GetActionProperty(InternalAction, "Owner") as GameObject;
			}
		}

		/// <summary>
		/// Gets the state that contains this action
		/// </summary>
		public FsmStateWrapper State
		{
			get
			{
				return new FsmStateWrapper(PlayMakerUtilities.GetActionProperty(InternalAction, "State"));
			}
		}

		/// <summary>
		/// Gets or sets whether this action is open in the editor
		/// </summary>
		public bool IsOpen
		{
			get
			{
				return (bool)(PlayMakerUtilities.GetActionProperty(InternalAction, "IsOpen") ?? false);
			}
			set
			{
				PlayMakerUtilities.SetActionProperty(InternalAction, "IsOpen", value);
			}
		}

		/// <summary>
		/// Gets or sets whether this action uses auto-naming
		/// </summary>
		public bool IsAutoNamed
		{
			get
			{
				return (bool)(PlayMakerUtilities.GetActionProperty(InternalAction, "IsAutoNamed") ?? false);
			}
			set
			{
				PlayMakerUtilities.SetActionProperty(InternalAction, "IsAutoNamed", value);
			}
		}

		/// <summary>
		/// Gets or sets whether this action has been entered
		/// </summary>
		public bool Entered
		{
			get
			{
				return (bool)(PlayMakerUtilities.GetActionProperty(InternalAction, "Entered") ?? false);
			}
			set
			{
				PlayMakerUtilities.SetActionProperty(InternalAction, "Entered", value);
			}
		}

		/// <summary>
		/// Gets or sets whether this action has finished execution
		/// </summary>
		public bool Finished
		{
			get
			{
				return (bool)(PlayMakerUtilities.GetActionProperty(InternalAction, "Finished") ?? false);
			}
			set
			{
				PlayMakerUtilities.SetActionProperty(InternalAction, "Finished", value);
			}
		}

		/// <summary>
		/// Gets or sets whether this action is currently active
		/// </summary>
		public bool Active
		{
			get
			{
				return (bool)(PlayMakerUtilities.GetActionProperty(InternalAction, "Active") ?? false);
			}
			set
			{
				PlayMakerUtilities.SetActionProperty(InternalAction, "Active", value);
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is FsmActionWrapper fsm)
			{
				return fsm.InternalAction == InternalAction;
			}

			return false;
		}
		
		public override int GetHashCode()
		{
			return InternalAction != null ? InternalAction.GetHashCode() : 0;
		}

		public static bool operator==(FsmActionWrapper a, FsmActionWrapper b)
		{
			return a.Equals(b);
		}

		public static bool operator!=(FsmActionWrapper a, FsmActionWrapper b)
		{
			return !a.Equals(b);
		}
	}
}
