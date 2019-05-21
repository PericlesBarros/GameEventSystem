using System;

namespace GameSystems.Events
{
	[Serializable]
	public sealed class GameEventReference
	{
		//=====================================================================================================================//
		//================================================= Public Properties ==================================================//
		//=====================================================================================================================//

		#region Public Properties

		public HandlerType type;
		public GameEvent gameEvent;
		public string eventID;

		public string Name
		{
			get
			{			
				if(type == HandlerType.EventID) {
					if(!string.IsNullOrEmpty(eventID))
						return "On [" + eventID+ "] \tEventID";
				}else {
					if(gameEvent != null) 
						return "On [" + gameEvent.name + "] \tGameEvent";
				}

				return "--- MISSING GAME EVENT REFERENCE ---";
			}
		}

		#endregion

		//=====================================================================================================================//
		//=================================================== Public Methods ==================================================//
		//=====================================================================================================================//

		#region Public Methods
		
		public static GameEventReference Clone(GameEventReference original)
		{
			
			if (original == null) 
				return null;

			var newWrapper = new GameEventReference {
				eventID = original.eventID,
				gameEvent = original.gameEvent,
				type = original.type
			};

			return newWrapper;
		}
		
		public bool Register(OnGameEventRaised handler)
		{
			switch (type) {
				case HandlerType.EventID:
					return !string.IsNullOrEmpty(eventID) && GameEventManager.RegisterHandler(eventID, handler);
				case HandlerType.GameEvent:
					return gameEvent != null && gameEvent.RegisterMethod(handler);
				default:
					return false;
			}
		}

		public bool Unregister(OnGameEventRaised handler)
		{
			switch (type) {
				case HandlerType.EventID:
					return !string.IsNullOrEmpty(eventID) && GameEventManager.UnregisterHandler(eventID, handler);
				case HandlerType.GameEvent:
					return gameEvent != null && gameEvent.UnregisterMethod(handler);
				default:
					return false;
			}
		}

		public void Raise()
		{
			if (type == HandlerType.EventID){
				GameEventManager.RaiseEvent(eventID, null);
			} else if (type == HandlerType.GameEvent){
				if (gameEvent != null)
					gameEvent.Raise(null);
			}
		}

		public bool IsValid()
		{
			if (type == HandlerType.EventID)
				return !string.IsNullOrEmpty(eventID);
			else if (type == HandlerType.GameEvent){
				return (gameEvent != null);
			}

			return true;
		}

		#endregion		
	}
	
}