using UnityEngine;
using System.Collections;
using UnityEngine.Playables;

namespace GameSystems.Events
{
    [System.Serializable]
    public sealed class TimelineTarget : IGameEventTarget
    {
        //=====================================================================================================================//
        //================================================== Internal Classes =================================================//
        //=====================================================================================================================//

        #region Internal Classes

        public enum ActionType
        {
            Play = 0,
            Pause = 1,
            Stop = 2,
            Resume = 3,
			SkipTo = 4
        }

        #endregion
		
        //=====================================================================================================================//
        //================================================= Public Properties ==================================================//
        //=====================================================================================================================//

        #region Public Properties

        public ActionType action;
        public PlayableDirector timeline;
		public float time;

        #endregion

        //=====================================================================================================================//
        //=================================================== Public Methods ==================================================//
        //=====================================================================================================================//

        #region Public Methods

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        public bool IsValid(out string message)
        {
            if (timeline == null){
                message = "[TimelineTarget] PlayableDirector reference is null.";
                return false;
            }

            if (timeline.playableAsset == null){
                message = "[TimelineTarget] PlayableDirector is missing a PlayableAsset.";
                return false;
            }

            if (action == ActionType.SkipTo) {
                time = Mathf.Clamp(time, 0, (float)timeline.duration);
            }

            message = "TimelineTarget is valid.";
            return true;
        }

        private bool _IsValid()
        {
            return timeline != null && timeline.playableAsset != null;
        }
		
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool OnValidate()
        {
			return _IsValid();
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param @name="data"></param>
		public void Process(params object[] data)
        {
            if (!_IsValid())
                return;

            if (action == ActionType.Play) {
                timeline.Play();
            } else if (action == ActionType.Pause) {
                timeline.Pause();
            } else if (action == ActionType.Resume) {
                timeline.Resume();
            } else if (action == ActionType.Stop) {
                timeline.Stop();
            }else if(action == ActionType.SkipTo) {
				time = Mathf.Abs(time);
				timeline.time = time;
			}
        }

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
        {
            if (timeline == null) {
                return "[ERROR] MISSING PLAYABLE_DIRECTOR";
            }

            if (timeline.playableAsset == null) {
                return "[ERROR] MISSING PLAYABLE_ASSET IN PLAYABLE_DIRECTOR";
            }

            switch (action) {
                case ActionType.Play:
                    return "PLAY [" + timeline.playableAsset.name + "] in " + timeline.name;
                case ActionType.Pause:
                    return "PAUSE [" + timeline.playableAsset.name + "] in " + timeline.name;
                case ActionType.Resume:
                    return "RESUME [" + timeline.playableAsset.name + "] in " + timeline.name;
                case ActionType.Stop:
                    return "STOP [" + timeline.playableAsset + "] in " + timeline.name;
				case ActionType.SkipTo:
					return "SKIP [" + timeline.playableAsset + "] to [" + Mathf.Abs(time) + "] seconds.";
            }

            //should never reach here
            return "[INVALID TIMELINE TARGET]";
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param @name="original"></param>
		/// <returns></returns>
		public static TimelineTarget Clone(TimelineTarget original)
        {
            if (original == null)
                return null;

            var newTarget = new TimelineTarget() {
                action = original.action,
                timeline = original.timeline,
				time = original.time
            };

            return newTarget;
        }

        #endregion
    }
}