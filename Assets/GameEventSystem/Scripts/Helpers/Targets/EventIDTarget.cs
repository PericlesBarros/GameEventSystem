using System;
using GameSystems.Utils;

namespace GameSystems.Events
{
    [Serializable]
    public sealed class EventIDTarget : IGameEventTarget
    {
        //=====================================================================================================================//
        //================================================== Internal Classes =================================================//
        //=====================================================================================================================//

        #region Internal Classes

        public enum ActionType
        {
            Raise = 0,
            Queue = 1,
            Abort = 2,
            Schedule = 3
        }

        #endregion

        //=====================================================================================================================//
        //================================================= Public Properties ==================================================//
        //=====================================================================================================================//

        #region Public Properties

        public ActionType action;
        public string eventID;
        public float delay;        
        public bool allOfType;
		public CallbackParameter parameter = new CallbackParameter();

        #endregion

        //=====================================================================================================================//
        //=================================================== Public Methods ==================================================//
        //=====================================================================================================================//

        #region Public Methods

        public static EventIDTarget Clone(EventIDTarget original)
        {
            if (original == null)
                return null;

            var newTarget = new EventIDTarget {
                action = original.action,
                eventID = original.eventID,
                delay = original.delay,
                allOfType = original.allOfType,
				parameter = CallbackParameter.Clone(original.parameter)
            };

            return newTarget;
        }

        public bool IsValid(out string message)
        {
            if (string.IsNullOrEmpty(eventID)){
                message = "[EventTarget] EventID is missing";
                return false;
            }

            if (!parameter.IsValid()){
                message = "[EventTarget] Parameter is invalid";
                return false;
            }

            message = "EventTarget is valid.";
            return true;
        }

        private bool _IsValid()
        {
            return !string.IsNullOrEmpty(eventID) && parameter != null && parameter.IsValid();
        }

        public bool OnValidate()
        {
            if (action == ActionType.Schedule) {
                if (delay <= 0.1f)
                    delay = 0.1f;
            }
            return _IsValid();
        }

        public void Process(params object[] data)
        {
            if (!_IsValid())
                return;

			var param = (parameter == null || parameter.type == ParameterType.None || !parameter.IsValid()) ? data : parameter.Value;

            switch (action) {
                case ActionType.Abort:
                    GameEventManager.AbortEvent(eventID, allOfType);
                    break;
                case ActionType.Queue:
                    GameEventManager.QueueEvent(eventID, param);
                    break;
                case ActionType.Raise:
                    GameEventManager.RaiseEvent(eventID, param);
                    break;
                case ActionType.Schedule:
                    if (delay <= 0.1f)
                        GameEventManager.QueueEvent(eventID, param);
                    else
                        GameEventManager.ScheduleEvent(eventID, delay, param);
                    break;
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(eventID)) {
                return "[ERROR] MISSING EVENT_ID";
            }

            switch (action) {
                case ActionType.Raise:
                    return "RAISE [" + eventID + "]";
                case ActionType.Queue:
                    return "QUEUE [" + eventID + "]";
                case ActionType.Schedule:
                    return "SCHEDULE [" + eventID + "] with [" + delay + "] seconds delay";
                case ActionType.Abort:
                    return "ABORT [" + eventID + "]";
            }

            //should never reach here
            return "[INVALID EVENT ID TARGET]";
        }
        #endregion        
    }

}