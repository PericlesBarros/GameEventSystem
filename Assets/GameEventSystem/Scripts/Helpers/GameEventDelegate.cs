using UnityEngine;
using System.Reflection;

namespace GameSystems.Events
{
	public delegate void OnGameEventRaised(params object[] data);

	[System.Serializable]
	public sealed class GameEventDelegate
    {
		//=====================================================================================================================//
		//================================================== Private Fields ===================================================//
		//=====================================================================================================================//

		#region Private Fields

		OnGameEventRaised _invoker = (args) => { };

		event OnGameEventRaised Handler
		{
			add
			{
				lock (_invoker) {
					_invoker -= value;
					_invoker += value;
				}
			}

			remove
			{
				lock (_invoker) {
					_invoker -= value;
				}
			}
		}
		
		#endregion

		//=====================================================================================================================//
		//================================================= Public Properties ==================================================//
		//=====================================================================================================================//

		#region Public Properties

		public System.Delegate[] Listeners
		{
			get {return _invoker.GetInvocationList(); }
		}

		public string name = "INACTIVE";

		public bool IsEmpty
		{
			get
			{
				return _invoker == null || _invoker.GetInvocationList().Length == 1;
			}
		}

		public int Count
		{
			get
			{
				return _invoker == null ? 0 : _invoker.GetInvocationList().Length-1;
			}
		}

		#endregion

		//=====================================================================================================================//
		//================================================ Method Overriding ==================================================//
		//=====================================================================================================================//

		#region Method Overriding

		public static GameEventDelegate operator + (GameEventDelegate @event, OnGameEventRaised method)
		{
			if (method != null) {
				@event.Handler -= method;
				@event.Handler += method;				
			}

			return @event;
		}

		public static GameEventDelegate operator - (GameEventDelegate @event, OnGameEventRaised method)
		{
			if (method != null) {
				@event.Handler -= method;
			}

			return @event;
		}
		
		//------------------------------------------------- Factory Method ----------------------------------------------------//
		public static GameEventDelegate CreateDelegate(MonoBehaviour behaviour, string methodName, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
		{
			if(behaviour == null || string.IsNullOrEmpty(methodName))
				return null;

			var methodInfo = behaviour.GetType().GetMethod(methodName, flags, null, new[] { typeof(object)}, null);
			if(methodInfo == null) 
				return null;
            
			var method = (OnGameEventRaised)System.Delegate.CreateDelegate(typeof(OnGameEventRaised), behaviour, methodInfo );
			
			var _delegate = new GameEventDelegate(methodName);
			_delegate += method;
			
			return _delegate;
		}

		#endregion

		//=====================================================================================================================//
		//================================================== Public Methods ===================================================//
		//=====================================================================================================================//

		#region Public Methods

		public GameEventDelegate()
		{
			name = "INACTIVE";
		}

		public GameEventDelegate(string name)
		{
			this.name = name;
		}
		
		public void Invoke(params object[] data)
		{
			if (_invoker != null)
				_invoker(data);
		}

		public void Clear()
		{
			name = "INACTIVE";
			_invoker = null;
			_invoker = (data) => { };
		}

		#endregion

		
	}
}