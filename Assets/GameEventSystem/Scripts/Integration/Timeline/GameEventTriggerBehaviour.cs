using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GameSystems.Events.Integration
{
    // A behaviour that is attached to a playable
    public class GameEventTriggerBehaviour : PlayableBehaviour
    {
        public HandlerType type;
        public GameEvent gameEvent;
        public string eventID;
        public GameEventData data;
        
        // Called when the state of the playable is set to Play
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (!Application.isPlaying) {
                var evt = "[-- MISSING ";
                switch (type) {
                    case HandlerType.EventID:
                        if (!string.IsNullOrEmpty(eventID))
                            evt = "["+eventID+"]";
                        else
                            evt += "EVENT_ID --]";
                        break;
                    case HandlerType.GameEvent:
                        if(gameEvent != null)
                            evt = "[" + gameEvent.name + "]";
                        else
                            evt += "GAME_EVENT --]";
                        break;
                }
                Debug.Log("GameEventTriggerClip will raise " + evt + " at runtime!");
            }

            switch (type) {
                case HandlerType.EventID:
                    if (!string.IsNullOrEmpty(eventID))
                        GameEventManager.RaiseEvent(eventID, data);
                    
                    break;
                case HandlerType.GameEvent:
                    if (gameEvent != null)
                        gameEvent.Raise(data);                    
                    break;
            }
        }

    }
}