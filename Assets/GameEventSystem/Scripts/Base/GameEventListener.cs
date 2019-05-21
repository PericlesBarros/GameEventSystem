using System.Collections.Generic;
using UnityEngine;

namespace GameSystems.Events
{
	[AddComponentMenu ("Game Event System/Game Event Listener")]	
	[DisallowMultipleComponent]
	public sealed class GameEventListener : MonoBehaviour, IGameEventListener
	{
		//=====================================================================================================================//
		//================================================= Inspector Variables ===============================================//
		//=====================================================================================================================//

		#region Inspector Variables
					
		public bool verbose;
		public List<GameEventHandler> handlers = new List<GameEventHandler>();
		
		#endregion
		
		//=====================================================================================================================//
		//============================================= Unity Callback Methods ================================================//
		//=====================================================================================================================//

		#region Unity Callback Methods

		private void Start()
		{
			if(handlers == null || handlers.Count == 0) {
				enabled = false;
				return;
			}

			foreach (var handler in handlers){
				if(handler.hasBeenInitialized)
					continue;

				handler.Initialize();
			}
		}

		private void OnEnable()
        {
	        if (handlers == null || handlers.Count == 0) {
                return;
            }

	        foreach (var handler in handlers){
		        if (!handler.hasBeenInitialized)
			        handler.Initialize();

		        var res = handler.Register();
		        if (verbose) {
			        Debug.Log(string.Format("GameEventListener [{0}]: Event [{1}]: Handler Registration Status [{2}]", name, handler.Name, res));
		        }
	        }
        }

		private void OnDisable()
		{
			if (handlers == null || handlers.Count == 0) {
                return;
            }

			foreach (var handler in handlers){
				var res = handler.Unregister();
				if (verbose) {
					Debug.Log(string.Format("GameEventListener [{0}]: Event [{1}]: Handler Unregistration Status [{2}]", name, handler.Name, (res ? "SUCCESS" : "FAIL")));
				}
			}
		}

		private void OnValidate()
		{
			foreach (var handler in handlers){
				if (handler != null)
					handler.OnValidate();
			}
		}

        #endregion

        //=====================================================================================================================//
        //================================================== Private Methods ==================================================//
        //=====================================================================================================================//

        #region Private Methods

        [ContextMenu("Inspect")]
        private void Inspect()
        {
	        if (handlers == null || handlers.Count == 0 || Application.isPlaying) {
                return;
            }

	        foreach (var handler in handlers){
		        handler.Inspect();
	        }
        }

        [ContextMenu("Raise All")]
        private void RaiseAll()
        {
            OnEventRaised(null);
        }
        
        #endregion

        //=====================================================================================================================//
        //=================================================== Public Methods ==================================================//
        //=====================================================================================================================//

        #region Public Methods
            
        [GameEventHandler]
		public void OnEventRaised(object data)
        {
	        if(handlers == null || handlers.Count == 0 || !Application.isPlaying) {				
				return;
			}

	        foreach (var handler in handlers){
		        if (verbose) {
			        Debug.Log(string.Format("GameEventListener [{0}]: Event [{1}]: [{2}] Targets processed in Handler", name, handler.Name, handler.TargetCount));
		        }

		        handler.OnEventRaised(data);
	        }
        }

		#endregion

		//=====================================================================================================================//
		//================================================= Management Methods ================================================//
		//=====================================================================================================================//

		#region Management Methods
		
#if UNITY_EDITOR
        public bool IsValid(out string message)
        {
            foreach (var handler in handlers){
	            if (handler != null && !handler.IsValid( out message))
		            return false;
            }

	        message = "GameEventListener is valid.";
            return true;
        }
		
		public void AddHandler()
		{
			if(handlers == null)
				handlers = new List<GameEventHandler>();

			handlers.Add(new GameEventHandler());
		}

		public void RemoveHandler(int idx)
		{
			if (handlers == null)
				return;

			if (idx >= 0 && idx < handlers.Count) {
				handlers.RemoveAt(idx);
			}
		}

		public void ClearHandlers()
		{
			if (handlers != null && handlers.Count > 0)
				handlers.Clear();
		}
		
		public void ShiftHandlerUp(int idx)
		{
			if (idx <= 0 || idx >= handlers.Count)
				return;

			var tmp = handlers[idx - 1];
			handlers[idx - 1] = handlers[idx];
			handlers[idx] = tmp;
		}

		public void ShiftHandlerDown(int idx)
		{
			if (idx < 0 || idx >= handlers.Count - 1)
				return;

			var tmp = handlers[idx + 1];
			handlers[idx + 1] = handlers[idx];
			handlers[idx] = tmp;
		}
		
		public bool IsEmpty()
		{
			return handlers == null || handlers.Count == 0;
		}

		public bool IsMuted()
		{
			if(IsEmpty())
				return true;
			
			foreach (var handler in handlers){
				if(handler != null && handler.isMuted)
					return true;
			}

			return false;
		}
	#endif
		
		#endregion
	}
}