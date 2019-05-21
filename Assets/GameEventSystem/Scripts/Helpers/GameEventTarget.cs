using System;
using UnityEngine;

namespace GameSystems.Events
{
    public enum TargetType
    {
        GameObject = 0,
        Behaviour = 1,
        Timeline = 2,
        Event = 3,
        Animator = 4
    }

    [Serializable]
    public sealed class GameEventTarget
    {
        //=====================================================================================================================//
        //================================================= Public Properties ==================================================//
        //=====================================================================================================================//

        #region Public Properties

        [SerializeField] private TargetType _targetType;

        [SerializeField] private BehaviourTarget _behaviourTarget;
        [SerializeField] private AnimatorTarget _animatorTarget;
        [SerializeField] private EventIDTarget _eventTarget;
        [SerializeField] private GameObjectTarget _gameObjectTarget;
        [SerializeField] private TimelineTarget _timelineTarget;
		
        [SerializeField] private bool _replayOnLoad;
        [SerializeField] private bool _isMuted;

		[HideInInspector] public bool isOpen;

        #endregion

        //=====================================================================================================================//
        //=================================================== Public Methods ==================================================//
        //=====================================================================================================================//

        #region Public Methods

        public static GameEventTarget Clone(GameEventTarget original)
        {
            if (original == null)
                return null;

            var newTarget = new GameEventTarget {
                _targetType = original._targetType,

                _animatorTarget = AnimatorTarget.Clone(original._animatorTarget),
                _behaviourTarget = BehaviourTarget.Clone(original._behaviourTarget),
                _eventTarget = EventIDTarget.Clone(original._eventTarget),
                _gameObjectTarget = GameObjectTarget.Clone(original._gameObjectTarget),
                _timelineTarget = TimelineTarget.Clone(original._timelineTarget),
				
                _replayOnLoad = original._replayOnLoad,    
				_isMuted = original._isMuted            
            };

            return newTarget;
        }

        /// <summary>
        /// Called when the event is triggered. Process accordingly
        /// </summary>
        /// <param @name="data"></param>
        public void Process(params object[] data)
        {
			if(_isMuted)
				return;

            switch (_targetType) {
                case TargetType.GameObject:
                    Debug.Assert(_gameObjectTarget != null);
                    _gameObjectTarget.Process(data);
                    break;
                case TargetType.Behaviour:
                    Debug.Assert(_behaviourTarget != null);
                    _behaviourTarget.Process(data);
                    break;
                case TargetType.Timeline:
                    Debug.Assert(_timelineTarget != null);
                    _timelineTarget.Process(data);
                    break;
                case TargetType.Event:
                    Debug.Assert(_eventTarget != null);
                    _eventTarget.Process(data);
                    break;
                case TargetType.Animator:
                    Debug.Assert(_animatorTarget != null);
                    _animatorTarget.Process(data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        #endregion

        //=====================================================================================================================//
        //================================================== Editing Methods ==================================================//
        //=====================================================================================================================//

        #region Editing Methods

        private void ClearAllExcept(IGameEventTarget target)
        {
            if(target != _gameObjectTarget)
                _gameObjectTarget = null;

            if(target != _timelineTarget)
                _timelineTarget = null;

            if(target != _eventTarget)
                _eventTarget = null;

            if(target != _animatorTarget)
                _animatorTarget = null;

            if (target != _behaviourTarget)
                _behaviourTarget = null;
        }
        
        /// <summary>
        /// Used by the CustomInspector to draw the appropriate color
        /// </summary>
        /// <returns>false if there's any missing reference, true otherwise</returns>
        public bool IsValid(out string message)
        {
            switch (_targetType) {
                case TargetType.Behaviour:
                    if (_behaviourTarget != null) 
                        return _behaviourTarget.IsValid(out message);
                    
                    message = "BehaviourTarget is null!";
                    break;
                    
                case TargetType.GameObject:
                    if (_gameObjectTarget != null) 
                        return _gameObjectTarget.IsValid(out message);
                    
                    message = "GameObjectTarget is null!";
                    break;
                case TargetType.Timeline:
                    if (_timelineTarget != null) 
                        return _timelineTarget.IsValid(out message);
                    
                    message = "TimelineTarget is null!";
                    break;
                case TargetType.Event:
                    if (_eventTarget != null) 
                        return _eventTarget.IsValid(out message);
                    
                    message = "EventTarget is null!";
                    break;
                case TargetType.Animator:
                    if (_animatorTarget != null) 
                        return _animatorTarget.IsValid(out message);
                    
                    message = "AnimatorTarget is null!";
                    break;
                default:
                    message = "INVALID";
                    break;
            }
            
            return false;
        }
        
        

        /// <summary>
        /// Similar to IsValid, but providing more detailed information
        /// </summary>
        /// <returns>debug string with information about the given references</returns>
        public override string ToString()
        {
            switch (_targetType) {
                case TargetType.Behaviour:
                    Debug.Assert(_behaviourTarget != null);
                    return _behaviourTarget.ToString();
                case TargetType.GameObject:
                    Debug.Assert(_gameObjectTarget != null);
                    return _gameObjectTarget.ToString();
                case TargetType.Timeline:
                    Debug.Assert(_timelineTarget != null);
                    return _timelineTarget.ToString();
                case TargetType.Event:
                    Debug.Assert(_eventTarget != null);
                    return _eventTarget.ToString();
                case TargetType.Animator:
                    Debug.Assert(_animatorTarget != null);
                    return _animatorTarget.ToString();
                default:
                    return "[INVALID GAME EVENT TARGET]";
            }
        }

        /// <summary>
        /// Called when script is loaded or when something changes in the inspector during EditMode only
        /// </summary>
        /// <returns></returns>
        public bool OnValidate()
        {
            switch (_targetType) {
                case TargetType.Behaviour:
                    ClearAllExcept(_behaviourTarget);
                    return _behaviourTarget != null && _behaviourTarget.OnValidate();
                case TargetType.GameObject:
                    ClearAllExcept(_gameObjectTarget);
                    return _gameObjectTarget != null && _gameObjectTarget.OnValidate();
                case TargetType.Timeline:
                    ClearAllExcept(_timelineTarget);
                    return _timelineTarget != null && _timelineTarget.OnValidate();
                case TargetType.Event:
                    ClearAllExcept(_eventTarget);
                    return _eventTarget != null && _eventTarget.OnValidate();
                case TargetType.Animator:
                    ClearAllExcept(_animatorTarget);
                    return _animatorTarget != null && _animatorTarget.OnValidate();
                default:
                    return true;
            }
        }

        #endregion
    }
}