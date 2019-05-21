using System;
using System.Collections.Generic;
using GameSystems.Utils;
using UnityEngine;

namespace GameSystems.Events
{
    [Serializable]
    public sealed class GameObjectTarget : IGameEventTarget
    {
        //=====================================================================================================================//
        //================================================== Internal Classes =================================================//
        //=====================================================================================================================//

        #region Internal Classes

        public enum ActionType
        {
            Activate = 0,
            Deactivate = 1,
            BroadcastToListeners = 2,
            SendMessage = 3,
            BroadcastMessage = 4
        }

        public enum BroadcastTarget
        {
            ObjectOnly,
            ObjectAndChildren,
            ChildrenOnly
        }

        #endregion

        //=====================================================================================================================//
        //================================================= Public Properties ==================================================//
        //=====================================================================================================================//

        #region Public Properties

        public ActionType action;
        public GameObject gameObject;

        //BroadcastMessage Alternative
        public List<IGameEventListener> cachedListeners = new List<IGameEventListener>();
        public List<MonoBehaviour> _cachedListenersView = new List<MonoBehaviour>();
        public BroadcastTarget broadcastTarget;


        //SendMessage & BroadcastMessage
        public CallbackParameter parameter = new CallbackParameter();
        public string _methodName;

        #endregion

        //=====================================================================================================================//
        //================================================== Private Methods ==================================================//
        //=====================================================================================================================//

        #region Private Methods

        /// <summary>
        /// Cache IGameEventListener references for the BroadcastToListener action
        /// </summary>
        /// <returns></returns>
        bool PopulateListeners()
        {
            if (action != ActionType.BroadcastToListeners || gameObject == null) {
                cachedListeners = new List<IGameEventListener>();
                return false;
            }

            if (broadcastTarget == BroadcastTarget.ObjectOnly) {
                cachedListeners = new List<IGameEventListener>(gameObject.GetComponents<IGameEventListener>());
            } else {
                cachedListeners = new List<IGameEventListener>(gameObject.GetComponentsInChildren<IGameEventListener>());
                if (broadcastTarget == BroadcastTarget.ChildrenOnly) {
                    var parentListeners = new List<IGameEventListener>(gameObject.GetComponents<IGameEventListener>());
                    cachedListeners.RemoveAll(listener => parentListeners.Contains(listener));
                }
            }

            _cachedListenersView = new List<MonoBehaviour>();
            for (int i = 0; i < cachedListeners.Count; i++) {
                var script = cachedListeners[i] as MonoBehaviour;
                if (script != null)
                    _cachedListenersView.Add(script);
            }

            return !(cachedListeners == null || cachedListeners.Count == 0);
        }

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
            if (gameObject == null) {
                message = "[GameObjectTarget] Target GameObject is null.";
                return false;
            }

            if (action == ActionType.BroadcastToListeners && (cachedListeners == null || cachedListeners.Count == 0)) {
                message = "[GameObjectTarget] Broadcast has no target listeners.";
                return false;
            }

            if ((action == ActionType.SendMessage || action == ActionType.BroadcastMessage) && (string.IsNullOrEmpty(_methodName) || !parameter.IsValid())) {
                message = "[GameObjectTarget] SendMessage method is null or parameter is invalid.";
                return false;
            }

            message = "GameObjectTarget is valid.";
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool OnValidate()
        {
            if (action == ActionType.BroadcastToListeners)
                return PopulateListeners();

            cachedListeners = new List<IGameEventListener>();
            _cachedListenersView = new List<MonoBehaviour>();

            if (action == ActionType.SendMessage || action == ActionType.BroadcastMessage)
                return (gameObject != null && !string.IsNullOrEmpty(_methodName));

            _methodName = "";
            parameter = new CallbackParameter();

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void Process(params object[] data)
        {
            if (gameObject == null)
                return;

            if (action == ActionType.Activate) {
                gameObject.SetActive(true);
            } else if (action == ActionType.Deactivate) {
                gameObject.SetActive(false);
            } else if (action == ActionType.BroadcastToListeners) {
                if (cachedListeners != null) {
                    for (int i = 0; i < cachedListeners.Count; i++) {
                        if (cachedListeners[i] != null)
                            cachedListeners[i].OnEventRaised(data);
                    }
                }
            } else if (action == ActionType.SendMessage) {
                if (!string.IsNullOrEmpty(_methodName))
                    gameObject.SendMessage(_methodName, parameter.Value, SendMessageOptions.RequireReceiver);
            } else if (action == ActionType.BroadcastMessage) {
                if (!string.IsNullOrEmpty(_methodName))
                    gameObject.BroadcastMessage(_methodName, parameter.Value, SendMessageOptions.RequireReceiver);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (gameObject == null) {
                return "[ERROR] MISSING GAMEOBJECT";
            }

            switch (action) {
                case ActionType.Activate:
                    return "ACTIVATE " + gameObject.name;
                case ActionType.Deactivate:
                    return "DEACTIVATE " + gameObject.name;
                case ActionType.BroadcastToListeners:
                    return "BROADCAST_MESSAGE " + ((cachedListeners == null || cachedListeners.Count == 0) ? "[ERROR] MISSING LISTENERS" : " to [" + cachedListeners.Count + "] IGAMEEVENTLISTENERS on ") + gameObject.name + "[" + broadcastTarget + "]";
                case ActionType.SendMessage:
                    return "SEND_MESSAGE [" + (string.IsNullOrEmpty(_methodName) ? "[ERROR] METHOD NAME IS NULL" : _methodName + "] with ") + parameter;
                case ActionType.BroadcastMessage:
                    return "BROADCAST_MESSAGE [" + (string.IsNullOrEmpty(_methodName) ? "[ERROR] METHOD NAME IS NULL" : _methodName + "] with ") + parameter;
            }

            //should never reach here
            return "[INVALID GAME OBJECT TARGET]";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static GameObjectTarget Clone(GameObjectTarget original)
        {
            if (original == null)
                return null;

            var newTarget = new GameObjectTarget {
                action = original.action,
                gameObject = original.gameObject,
                broadcastTarget = original.broadcastTarget,
                parameter = CallbackParameter.Clone(original.parameter),
                _methodName = original._methodName
            };

            if (original.cachedListeners != null)
                newTarget.cachedListeners = new List<IGameEventListener>(original.cachedListeners);

            if (original._cachedListenersView != null)
                newTarget._cachedListenersView = new List<MonoBehaviour>(original._cachedListenersView);

            return newTarget;
        }

        #endregion

        //=====================================================================================================================//
        //================================================ Debugging & Testing ================================================//
        //=====================================================================================================================//

        #region Debugging & Testing

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ListenerNames()
        {
            if (cachedListeners == null || cachedListeners.Count == 0)
                return "--- EMPTY ---";

            var _text = "";

            for (int i = 0; i < cachedListeners.Count; i++) {
                if (cachedListeners[i] == null) {
                    _text += "--- MISSING LISTENER REFERENCE ---";
                } else {
                    _text += cachedListeners[i].GetType().Name;
                }

                if (i < cachedListeners.Count - 1)
                    _text += "\n";
            }

            return _text;
        }

        #endregion
    }
}