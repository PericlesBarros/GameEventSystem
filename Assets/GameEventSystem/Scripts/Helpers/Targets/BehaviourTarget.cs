using System;
using GameSystems.Utils;
using UnityEngine;

namespace GameSystems.Events
{
    [Serializable]
    public sealed class BehaviourTarget : IGameEventTarget
    {
        //=====================================================================================================================//
        //================================================== Internal Classes =================================================//
        //=====================================================================================================================//

        #region Internal Classes

        public enum ActionType
        {
            Enable = 0,
            Disable = 1,
            InvokeMethod = 2
        }

        public enum BehaviourType
        {
            Behaviour,
            Renderer,
            Collider,
            Cloth,
            LODGroup
        }

        #endregion

        //=====================================================================================================================//
        //================================================= Public Properties ==================================================//
        //=====================================================================================================================//

        #region Public Properties

        [SerializeField] private BehaviourType _type;
        [SerializeField] private Cloth _cloth;
        [SerializeField] private LODGroup _lodGroup;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Collider _collider;
        [SerializeField] private Behaviour _behaviour;
        [SerializeField] private ActionType _action;
        [SerializeField] private GameObject _gameObject;		
        [SerializeField] private string _methodName;
		[SerializeField] private CallbackParameter _methodArg;
        [SerializeField] private int _methodIdx = -1;

        private GameEventDelegate _methodDelegate;

        #endregion

        //=====================================================================================================================//
        //================================================== Private Methods ==================================================//
        //=====================================================================================================================//

        #region Private Methods

        /// <summary>
        /// Called OnValidate
        /// In EditMode caches method @name to be called at runtime (used as an alternative to Unity's SendMessage
        /// In PlayMode With the cached method @name, create a delegate method using reflection to fetche the corresponding method in the provided script
        /// </summary>
        /// <returns></returns>
        private bool ValidateMethod()
        {
            if (_action != ActionType.InvokeMethod)
                return false;

            if (Application.isPlaying) {
                if (_behaviour == null || ((MonoBehaviour)_behaviour) == null)
                    return false;

                if (string.IsNullOrEmpty(_methodName))
                    return false;

                _methodDelegate = GameEventDelegate.CreateDelegate((MonoBehaviour)_behaviour, _methodName);
                if (_methodDelegate == null || _methodDelegate.Count == 0)
                    return false;
            } else {
                if (_behaviour == null) {
                    _methodIdx = -1;
                    _methodName = "";
                    return false;
                }
            }
			
            return true;
        }

        private void SetEnabled(bool state)
        {
            switch (_type) {
                case BehaviourType.Behaviour:
                    if (_behaviour != null)
                        _behaviour.enabled = state;
                    break;
                case BehaviourType.Cloth:
                    if (_cloth != null)
                        _cloth.enabled = state;
                    break;
                case BehaviourType.Collider:
                    if (_collider != null)
                        _collider.enabled = state;
                    break;
                case BehaviourType.LODGroup:
                    if (_lodGroup != null)
                        _lodGroup.enabled = state;
                    break;
                case BehaviourType.Renderer:
                    if (_renderer != null)
                        _renderer.enabled = state;
                    break;
            }
        }

        private void ClearReferences(bool clearBehaviour = false)
        {
            _collider = null;
            _cloth = null;
            _lodGroup = null;
            _renderer = null;

			if (clearBehaviour) {
				_behaviour = null;
				_methodName = "";
				_methodIdx = -1;
			}
        }

        private void ClearAllExcept(Component comp)
		{
			if(!typeof(Collider).IsAssignableFrom(comp.GetType()))
				_collider = null;

			if(!typeof(Cloth).IsAssignableFrom(comp.GetType()))
				_cloth = null;

			if(!typeof(LODGroup).IsAssignableFrom(comp.GetType()))
				_lodGroup = null;

			if(!typeof(Renderer).IsAssignableFrom(comp.GetType()))
				_renderer = null;

            if(!typeof(Behaviour).IsAssignableFrom(comp.GetType()))
                _behaviour = null;
		}
        
        #endregion

        //=====================================================================================================================//
        //=================================================== Public Methods ==================================================//
        //=====================================================================================================================//

        #region Public Methods

        public static BehaviourTarget Clone(BehaviourTarget original)
        {
            if (original == null)
                return null;

            var newTarget = new BehaviourTarget {
                _behaviour = original._behaviour,
                _action = original._action,
                _gameObject = original._gameObject,
                _methodName = original._methodName,
                _methodIdx = original._methodIdx,
                _methodArg = CallbackParameter.Clone(original._methodArg),
                _type = original._type,
                _cloth = original._cloth,
                _lodGroup = original._lodGroup,
                _collider = original._collider,
                _renderer= original._renderer
            };
            
            return newTarget;
        }

        public bool IsValid(out string message)
        {
            if(_gameObject == null) {
                ClearReferences(true);
                message = "[BehaviourTarget] GameObject is null!";
                return false;
            }

            if (_action == ActionType.Disable || _action == ActionType.Enable) {
                switch (_type) {
                    case BehaviourType.Behaviour:
						ClearAllExcept(_behaviour);
                        if (_behaviour == null){
                            message = "[BehaviourTarget] Target Behaviour is null!";
                            return false;
                        }
                        break;
                    case BehaviourType.Cloth:
						ClearAllExcept(_cloth);
                        if (_cloth == null){
                            message = "[BehaviourTarget] Target Cloth is null!";
                            return false;
                        }
                        
                        break;
                    case BehaviourType.Collider:
						ClearAllExcept(_collider);
                        if (_collider == null){
                            message = "[BehaviourTarget] Target Collider is null!";
                            return false;
                        }

                        break;
                    case BehaviourType.LODGroup:
						ClearAllExcept(_lodGroup);
                        if (_lodGroup == null){
                            message = "[BehaviourTarget] Target LODGroup is null!";
                            return false;
                        }
                        
                        break;
                    case BehaviourType.Renderer:
						ClearAllExcept(_renderer);
                        if (_renderer == null){
                            message = "[BehaviourTarget] Target Renderer is null!";
                            return false;
                        }

                        break;
                }
            } else {
                ClearReferences();

				if (_behaviour == null) {
					_methodName = "";
					_methodIdx = -1;
				    message = "[BehaviourTarget] Target Behaviour is null!";
					return false;
				}

                if (_action == ActionType.InvokeMethod && (string.IsNullOrEmpty(_methodName) || !_methodArg.IsValid())){
                    message = "[BehaviourTarget] Target Method is invalid!";
                    return false;
                }
            }

            message = "BehaviourTarget is valid";
            return true;
        }

        public bool OnValidate()
        {
            string message;
            if (!IsValid(out message))
                return false;

            if (_action == ActionType.InvokeMethod)
                return ValidateMethod();

            return true;
        }

        public void Process(params object[] data)
        {

            if (_action == ActionType.Enable) {
                SetEnabled(true);                
            } else if (_action == ActionType.Disable) {
                SetEnabled(false);
            } else if (_action == ActionType.InvokeMethod) {

				var param = (_methodArg == null || _methodArg.type == ParameterType.None || !_methodArg.IsValid()) ? data : _methodArg.Value;

                if (_methodDelegate != null)
                    _methodDelegate.Invoke(param);
            }
        }

        public override string ToString()
        {
            if (_behaviour == null) {
                return "[ERROR] MISSING BEHAVIOUR REFERENCE";
            }
            
            switch (_action) {
                case ActionType.Enable:
                    return "ENABLE " + _behaviour.GetType().Name + " on " + _behaviour.name;
                case ActionType.Disable:
                    return "DISABLE " + _behaviour.GetType().Name + " on " + _behaviour.name;
                case ActionType.InvokeMethod:
                    return "INVOKE_HANDLER_METHOD " + (string.IsNullOrEmpty(_methodName) ? "[ERROR] MISSING METHOD NAME" : "(" + _methodName + ") on ") + _behaviour.GetType().Name + " of " + _behaviour.name;
            }
			
            //should never reach here
            return "[INVALID BEHAVIOUR TARGET]";
        }

        #endregion
    }
}