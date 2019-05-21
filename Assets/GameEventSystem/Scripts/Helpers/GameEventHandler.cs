using UnityEngine;

namespace GameSystems.Events
{
    public enum HandlerType
    {
        GameEvent,
        EventID
    }

    [System.Serializable]
    public sealed class GameEventHandler
    {
        //=====================================================================================================================//
        //================================================== Public Properties ================================================//
        //=====================================================================================================================//

        #region Public Properties
			
        public GameEventReference Event;
        [SerializeField] private GameEventTargetSet _targets;
		
        [HideInInspector] public bool hasBeenInitialized;		
		[HideInInspector] public bool isMuted;
		[HideInInspector] public bool isOpen;

        public string Name
        {
            get {                
                return Event == null ? "-- EVENT HANDLER -- " : Event.Name;
            }
        }

        public int TargetCount
        {
            get {
                return _targets.targets == null ? 0 : _targets.targets.Count;
            }
        }
		
        #endregion

        //=====================================================================================================================//
        //=================================================== Public Methods ==================================================//
        //=====================================================================================================================//

        #region Public Methods

        public static GameEventHandler Clone(GameEventHandler original)
        {
            if (original == null) 
                return null;

            var newHandler = new GameEventHandler {
                Event = GameEventReference.Clone(original.Event),
                isMuted = original.isMuted,
                hasBeenInitialized = false,
                _targets = GameEventTargetSet.Clone(original._targets)
            };

            return newHandler;
        }

        /// <summary>
        /// Called from GameEventListeners OnEnable or, alternatively start method
        /// Makes sure everything is ready
        /// </summary>
        public void Initialize()
        {
			hasBeenInitialized = true;

			if(isMuted) {
				Debug.LogWarning("GameEventHandler is muted. Targets will not be processed on event raised.");
			}

            if (Event == null || !Event.IsValid()) {
                Debug.LogWarning("GameEvent reference is invalid!");
                return;
            }

			_targets.Initialize();                  
        }

        public void OnValidate()
        {
            _targets.OnValidate();
        }

        public bool IsValid(out string message)
        {
            if (Event == null || !Event.IsValid()){
                message = Name + " is missing GameEvent reference or ID.";
                return false;
            }

            if (_targets == null){
                message = Name + "'s targets list has not been initialized.";
                return false;
            }

            
            return _targets.IsValid(out message);
        }

        public void Inspect()
        {
            var text = "Validating Handler: " + Name + " ...\n";

			if(Event == null)
				text += "[ERROR] GameEvent: " + "MISSING EVENT REFERENCE\n";
			else
				text += "Listening to GameEvent: " + Event.Name + "\n";
			
            if (_targets == null) {
                text += "[ERROR] Targets: " + "NO TARGET SETUP\n";
            } else {
                text += _targets.ToString();
            }
            Debug.Log(text);
        }

        public bool Register()
        {
			if(Event == null || !Event.IsValid()) {
				return false;
			}

            if (Event.type == HandlerType.GameEvent) {
                return Event.gameEvent.RegisterHandler(this);
            }

            if (Event.type  == HandlerType.EventID) {
                return GameEventManager.RegisterHandler(Event.eventID, OnEventRaised);
            }

            return false;
        }

        public bool Unregister()
        {
			if(Event == null || !Event.IsValid()) {
				return false;
			}

            if (Event.type == HandlerType.GameEvent) {
                return Event.gameEvent.UnregisterHandler(this);
            }

            if (Event.type == HandlerType.EventID) {
                return GameEventManager.UnregisterHandler(Event.eventID, OnEventRaised);
            }

            return false;
        }

        public void OnEventRaised(object data)
        {
			if(isMuted) {
				Debug.LogWarning("GameEventHandler is muted! Targets will not be processed.");
				return;
			}

			if(_targets != null) {
				_targets.Process(data);
			}
        }
		
        #endregion

        //=====================================================================================================================//
        //================================================ Management Methods =================================================//
        //=====================================================================================================================//

        /// <summary>
        /// Helper methods to populate GameEventListeners in the editor
        /// </summary>
        #region Management Methods

        public void ClearTargets()
        {
            if (_targets != null)
                _targets.Clear();
        }
		  
        #endregion		
    }
}