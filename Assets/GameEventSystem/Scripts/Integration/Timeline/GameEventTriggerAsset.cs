using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GameSystems.Events.Integration
{
	[System.Serializable]
	public class GameEventTriggerAsset : PlayableAsset
	{
        public GameEventTriggerBehaviour template = new GameEventTriggerBehaviour();
        public HandlerType type;
        [HideInInspector] public GameEvent gameEvent;
        [HideInInspector] public string eventID;
        [HideInInspector] public GameEventData data;

        public override double duration
        {
            get {
                return Time.maximumDeltaTime;
            }
        }
        
        // Factory method that generates a playable based on this asset
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
		{
            var playable = ScriptPlayable<GameEventTriggerBehaviour>.Create(graph, template);
            var clone = playable.GetBehaviour();

            if(clone != null) {
                clone.type = type;
                clone.gameEvent = gameEvent;
                clone.eventID = eventID;
                clone.data = data;
            }

            return Playable.Create(graph);
		}
	}
}