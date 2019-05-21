using UnityEngine;

namespace GameSystems.Events
{
	[CreateAssetMenu(menuName = "GameSystems/Events/Game Event")]
	public sealed class GameEvent : ScriptableObject
	{
		//=====================================================================================================================//
		//=================================================== Private Fields ==================================================//
		//=====================================================================================================================//

		#region Private Fields
			
		//TODO: cache behaviour and gameobject names and instance IDs for debug purposes

		GameEventDelegate invoker = new GameEventDelegate();			
        
		public GameEventDelegate Handler
		{
			get { return invoker;}
		}

		public bool verbose;	

		#endregion

		//=====================================================================================================================//
		//=================================================== Public Methods ==================================================//
		//=====================================================================================================================//

		#region Public Methods

		public void Clear()
		{
			if(verbose) {
				Debug.Log(string.Format("Cleaning up GameEvent [{0}].", name));
			}
			
			if(invoker.Count > 0)
				invoker.Clear();
		}

		public void Raise(params object[] data)
		{
			if(verbose) {
				Debug.Log(string.Format("[{0}] event raised! {1} Delegate methods called", name, invoker.Count));
			}
		
			if(invoker != null) { }
				invoker.Invoke(data);
		}

		//---------------------------------------------------- For Listeners --------------------------------------------------//

		public bool RegisterListener(GameEventListener listener)
		{
			if(listener == null)
				return false;

			if(verbose) {
				Debug.Log(string.Format("[{0} EventListener]: Registering with [{1}] event...", name, listener.name));
			}

			RegisterMethod(listener.OnEventRaised);

			if(verbose) {
				Debug.Log(string.Format("[{0}]: Registration successful: [{1}]", name, listener.name));
			}
			
			return true;
		}

		public bool UnregisterListener(GameEventListener listener)
		{
			if(listener == null )
				return false;
			
			UnregisterMethod(listener.OnEventRaised);

			if (verbose) {
				Debug.Log(string.Format("[{0}]: Unregistering listener: [{1}]", name, listener.name));
			}
			
			return true;
		}
		
		//----------------------------------------------------- For Handlers --------------------------------------------------//

		public bool RegisterHandler (GameEventHandler handler)
		{
			if(handler == null)
				return false;

			if(verbose) {
				Debug.Log(string.Format("[{0} EventHandler]: Registering with [{1}] event...", name, handler.Name));
			}
			
			RegisterMethod(handler.OnEventRaised);
			
			if(verbose) {
				Debug.Log(string.Format("[{0}]: Registration successful: [{1}]", name, handler.Name));
			}
			
			return true;
		}

		public bool UnregisterHandler(GameEventHandler handler)
		{
			if(handler == null)
				return false;

			UnregisterMethod(handler.OnEventRaised);

			if(verbose) {
				Debug.Log(string.Format("[{0}]: Unregistering handler: [{1}]", name, handler.Name));
			}

			return true;
		}			

		//----------------------------------------------------- For Methods ---------------------------------------------------//

		public bool RegisterMethod(OnGameEventRaised method)
		{
			if(method == null)
				return false;

			if(invoker == null)
				invoker = new GameEventDelegate();

            invoker -= method;
            invoker += method;

			if(verbose) {
				Debug.Log(string.Format("[{0}]: Method Registration Succesfull: [{1}]", name, method.Method.Name));
			}

			return true;
		}

		public bool UnregisterMethod(OnGameEventRaised method)
		{
			if(method == null || invoker.IsEmpty)
				return false;
						
			invoker -= method;

			if(verbose) {
				Debug.Log(string.Format("[{0}]: Method Unregistration Succesfull: [{1}]", name, method.Method.Name));
			}

			return true;
		}

		#endregion		
	}
}