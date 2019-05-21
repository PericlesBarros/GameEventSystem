using System;
using System.Collections;
using System.Collections.Generic;
using GameSystems.Patterns;
//using GameSystems.Utils;
using UnityEngine;

namespace GameSystems.Events
{
	[AddComponentMenu("Game Event System/Game Event Manager")]
	public sealed class GameEventManager : Singleton<GameEventManager>
	{
		//=====================================================================================================================//
		//=================================================== Pending Tasks ===================================================//
		//=====================================================================================================================//

		#region Pending Tasks

		//TODO: make all public methods return a string message or enum state

		#endregion

		//=====================================================================================================================//
		//================================================== Internal Classes =================================================//
		//=====================================================================================================================//

		#region Internal Classes

		[Serializable]
		private sealed class GameEventPool
		{
			public readonly int maxPoolSize;
			public readonly int increment;
			private Stack<GameEventDelegate> _inactive;

			public GameEventPool(int maxQuantity = 50, int increment = 15)
			{
				maxPoolSize = maxQuantity;
				this.increment = increment;

				_inactive = new Stack<GameEventDelegate>();

				PopulateStack(maxPoolSize);
			}

			public GameEventDelegate Fetch()
			{
				if (_inactive.Count <= 0) {
					PopulateStack(increment);
				}

				return _inactive.Pop();
			}

			public void Return(GameEventDelegate _event)
			{
				if (_event == null)
					return;

				_event.Clear();

				_inactive.Push(_event);
			}

			private void PopulateStack(int count)
			{
				if (_inactive == null)
					_inactive = new Stack<GameEventDelegate>();

				for (var i = 0; i < count; i++) {
					_inactive.Push(new GameEventDelegate());
				}
			}
		}

		private struct EventInstance
		{
			public readonly string eventID;
			public readonly object eventData;

			public EventInstance(string eventID, object eventData)
			{
				this.eventID = eventID;
				this.eventData = eventData;
			}
		}

		#endregion

		//=====================================================================================================================//
		//================================================= Inspector Variables ===============================================//
		//=====================================================================================================================//

		#region Inspector Variables

		[SerializeField] private int _initialEventPoolSize = 25;
		[SerializeField] private int _poolIncrement = 10;
		[SerializeField] private int _eventQueueCapacity = 50;
		[SerializeField] private bool _verbose;


		[Header ("[Debug]")]
		public string eventID;
		public GameEventData testData;

		#endregion

		//=====================================================================================================================//
		//================================================= Public Properties =================================================//
		//=====================================================================================================================//

		#region Private Fields

		#endregion

		//=====================================================================================================================//
		//=================================================== Private Fields ==================================================//
		//=====================================================================================================================//

		#region Private Fields

		private static readonly Dictionary<string, GameEvent> gameEvents = new Dictionary<string, GameEvent>();
		private static readonly Dictionary<string, GameEventDelegate> eventDelegates = new Dictionary<string, GameEventDelegate>();
		private static GameEventPool _eventPool;

		private static readonly object queueLock = new object();
		private static readonly List<EventInstance>[] eventQueues = new List<EventInstance>[2];
		private static int _activeQueue;
		private static int _processingQueue = 1;

		private static readonly List<KeyValuePair<string, Coroutine>> scheduledEvents = new List<KeyValuePair<string, Coroutine>>();

		#endregion

		//=====================================================================================================================//
		//============================================= Unity Callback Methods ================================================//
		//=====================================================================================================================//

		#region Unity Callback Methods

		protected override void Awake()
		{
			base.Awake();

			if (_verbose) {
				Debug.Log("GameEventManager: Initializing... ");
			}
			_eventPool = new GameEventPool(_initialEventPoolSize, _poolIncrement);

			if (_verbose) {
				Debug.Log(string.Format("GameEventManager: EventPool created with [{0}] initial slots. Increment: {1}", _eventPool.maxPoolSize, _eventPool.increment));
				Debug.Log("GameEventManager: Fetching GameEvent assets in Resources folder...");
			}


			var events = Resources.LoadAll<GameEvent>("GameEvents/");//Resources.FindObjectsOfTypeAll<GameEvent>;

			//Cache existing GameEvent assets
			foreach (var evt in events) {
				if (gameEvents.ContainsKey(evt.name)) 
					continue;
				
				gameEvents.Add(evt.name, evt);
				if (_verbose) {
					Debug.Log(string.Format("...GameEvent [{0}] asset loaded!", evt.name));
				}
			}

			if (_verbose) {
				Debug.Log(string.Format("GameEventManager: {0} GameEvent assets found!", gameEvents.Count));
			}

			eventQueues[_activeQueue] = new List<EventInstance>(_eventQueueCapacity);
			eventQueues[_processingQueue] = new List<EventInstance>(_eventQueueCapacity);
		}

		private void Update()
		{
			if (eventQueues[_processingQueue] != null && eventQueues[_processingQueue].Count > 0) {
				for (var i = eventQueues[_processingQueue].Count - 1; i >= 0; i--) {
					RaiseEvent(eventQueues[_processingQueue][i]);
					eventQueues[_processingQueue].RemoveAt(i);
				}
			}

			lock (queueLock) {
				var tmp = _activeQueue;
				_activeQueue = _processingQueue;
				_processingQueue = tmp;
			}
		}

		private void OnDisable()
		{
			UnregisterAllEvents();
		}

		#endregion

		//=====================================================================================================================//
		//================================================== Private Methods ==================================================//
		//=====================================================================================================================//

		#region Private Methods

		private void UnregisterAllEvents()
		{
			if (gameEvents != null) {
				foreach (var evt in gameEvents.Values) {
					if (evt != null)
						evt.Clear();
				}
			}

			if (eventDelegates != null) {
				foreach (var del in eventDelegates.Values) {
					if (del != null) 
						del.Clear();
				}
			}

			StopAllCoroutines();

			if (scheduledEvents != null) 
				scheduledEvents.Clear();

			if (eventDelegates != null) 
				eventDelegates.Clear();

			if (eventQueues == null)
				return;

			if (eventQueues[_activeQueue] != null)
				eventQueues[_activeQueue].Clear();

			if (eventQueues[_processingQueue] != null)
				eventQueues[_processingQueue].Clear();
		}

		private static IEnumerator _ScheduleEvent(string eventID, float delay, int selfIdx, params object[] data)
		{
			yield return new WaitForSeconds(delay - Time.deltaTime);

			RaiseEvent(eventID, data);

			if (Instance._verbose) {
				Debug.Log(string.Format("Scheduled Event [{0}] raised at Time [{1}]!", eventID, Time.time), Instance.gameObject);
			}

			if (scheduledEvents != null && selfIdx >= 0 && selfIdx < scheduledEvents.Count) 
				scheduledEvents.RemoveAt(selfIdx);
		}

		private static void RaiseEvent(EventInstance evtInstance)
		{
			RaiseEvent(evtInstance.eventID, evtInstance.eventData);
		}

		#endregion

		//=====================================================================================================================//
		//=================================================== Public Methods ==================================================//
		//=====================================================================================================================//

		#region Public Methods

		/// <summary>
		/// Register handler to eventID
		/// </summary>
		/// <param name="eventID">EventID to listen to</param>
		/// <param name="handler">Method delegate</param>
		/// <returns>True if successfuly registered. False otherwise</returns>
		public static bool RegisterHandler(string eventID, OnGameEventRaised handler)
		{
			if (Instance == null)
				return false;

			if (!Instance.enabled) {
				if (Instance._verbose) {
					Debug.LogWarning(string.Format("Attempting to register handler for event [{0}] while GameEventManager is disabled!", eventID), Instance.gameObject);
				}
				return false;
			}

			if (string.IsNullOrEmpty(eventID)) {
				if (Instance._verbose) {
					Debug.LogWarning("[ERROR] Given eventID to be registered is NULL!", Instance.gameObject);
				}
				return false;
			}

			if (handler == null) {
				if (Instance._verbose) {
					Debug.LogWarning("[ERROR] Given handler to be registered is NULL!", Instance.gameObject);
				}
				return false;
			}

			//Check if GameEvent asset exists and register handler if it does
			GameEvent gameEvent;
			if (gameEvents.TryGetValue(eventID, out gameEvent) && gameEvent != null) {
				if (Instance._verbose) {
					Debug.Log(string.Format("Handler[{0}] successfully registered to GameEvent[{1}]!", handler.Method.Name, gameEvent.name), Instance.gameObject);
				}
				return gameEvent.RegisterMethod(handler);
			}

			//Check if GameEventDelegate exists and fetch a new one if it doesn't
			GameEventDelegate evt;
			if (!eventDelegates.TryGetValue(eventID, out evt) || evt == null) {
				if (Instance._verbose) {
					Debug.Log(string.Format("EventID [{0}] has no listeners! Fetching new GameEventDelegate evtInstance from pool.", eventID), Instance.gameObject);
				}
				if (_eventPool != null)
					evt = _eventPool.Fetch();

				if (evt == null) {
					if (Instance._verbose) {
						Debug.LogError("Something went wrong fetching new GameEventDelegate evtInstance from pool.", Instance.gameObject);
					}
					return false;
				}

				evt.name = eventID;
				evt += handler;
				eventDelegates.Add(eventID, evt);
			}
			
			if (Instance._verbose) {
				Debug.Log(string.Format("Handler[{0}] successfully registered to new GameEventDelegate!", handler.Method.Name), Instance.gameObject);
			}
			return true;
		}

		/// <summary>
		/// Unregister handler from given eventID
		/// </summary>
		/// <param name="eventID"></param>
		/// <param name="handler"></param>
		/// <returns>true if successful, false if if either eventID or handler are NULL, or if handler is not currently registered</returns>
		public static bool UnregisterHandler(string eventID, OnGameEventRaised handler)
		{
			if (Instance == null)
				return false;

			if (!Instance.enabled) {
				if (Instance._verbose) {
					Debug.LogWarning(string.Format("Attempting to unregister handler for event [{0}] while GameEventManager is disabled!", eventID), Instance.gameObject);
				}
				return false;
			}

			if (string.IsNullOrEmpty(eventID)) {
				if (Instance._verbose) {
					Debug.LogWarning("[ERROR] Given eventID to be unregistered is null!", Instance.gameObject);
				}
				return false;
			}

			if (handler == null) {
				if (Instance._verbose) {
					Debug.LogWarning("[ERROR] Given handler to be unregistered is null!", Instance.gameObject);
				}
				return false;
			}

			GameEvent gameEvent;
			if (gameEvents.TryGetValue(eventID, out gameEvent) && gameEvent != null) {
				gameEvent.UnregisterMethod(handler);
				if (Instance._verbose) {
					Debug.Log(string.Format("Handler[{0}] successfully unregistered from GameEvent[{1}]!", handler.Method.Name, gameEvent.name), Instance.gameObject);
				}
				return true;
			}

			GameEventDelegate _event;
			if (eventDelegates.TryGetValue(eventID, out _event) && _event != null) {
				_event -= handler;
				if (Instance._verbose) {
					Debug.Log(string.Format("Handler[{0}] successfully unregistered from GameEventDelegate!", handler.Method.Name), Instance.gameObject);
				}

				if (!_event.IsEmpty) 
					return true;
				
				eventDelegates.Remove(eventID);
				if (Instance._verbose) {
					Debug.Log(string.Format("GameEventDelegate for EventID[{0}] has no more listeners! Returning evtInstance to pool.", eventID), Instance.gameObject);
				}
				_eventPool.Return(_event);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Bypass the event queue and Raise an event immediately
		/// </summary>
		/// <param name="eventID"></param>
		/// <param name="data"></param>
		/// <returns>true if event was raised, false if manager not enabled, if eventID is invalid or if there are no listeners registered for give event</returns>
		public static bool RaiseEvent(string eventID, params object[] data)
		{
			if (Instance == null)
				return false;

			if (!Instance.enabled) {
				if (Instance._verbose) {
					Debug.LogWarning(string.Format("Attempting to raise event [{0}] while GameEventManager is disabled!", eventID), Instance.gameObject);
				}
				return false;
			}

			if (string.IsNullOrEmpty(eventID)) {
				if (Instance._verbose) {
					Debug.LogWarning("Attempting to raise [NULL] event!", Instance.gameObject);
				}
				return false;
			}

			if (!EventHasListeners(eventID)) {
				if (Instance._verbose) {
					Debug.LogWarning(string.Format("EventID to raise [{0}] has no listeners. Aborting operation!", eventID), Instance.gameObject);
				}
				return false;
			}

			GameEvent gameEvent;
			if (gameEvents.TryGetValue(eventID, out gameEvent) && gameEvent != null) {
				gameEvent.Raise(data);
				if (Instance._verbose) {
					Debug.Log(string.Format("Event [{0}] raised from GameEvent at [{1}]", eventID, Time.time), Instance.gameObject);
				}
				return true;
			}

			GameEventDelegate _event;
			if (eventDelegates.TryGetValue(eventID, out _event) && _event != null) {
				_event.Invoke(data);
				if (Instance._verbose) {
					Debug.Log(string.Format("Event [{0}] raised from Delegate at [{1}]", eventID, Time.time), Instance.gameObject);
				}
				return true;
			}
			
			

			return false;
		}

		/// <summary>
		/// Adds an eventID to the active queue to be processed in the next frame
		/// </summary>
		/// <param name="eventID"></param>
		/// <param name="data"></param>
		/// <returns>true if successfully added to the queue, false if manager is disabled or given ID is invalid</returns>
		public static bool QueueEvent(string eventID, object data)
		{
			if (Instance == null)
				return false;

			if (!Instance.enabled) {
				if (Instance._verbose) {
					Debug.LogWarning(string.Format("Attempting to queue event [{0}] while GameEventManager is disabled!", eventID), Instance.gameObject);
				}
				return false;
			}

			if (string.IsNullOrEmpty(eventID)) {
				if (Instance._verbose) {
					Debug.LogWarning("Attempting to queue [NULL] event!", Instance.gameObject);
				}
				return false;
			}

			if (!EventHasListeners(eventID)) {
				if (Instance._verbose) {
					Debug.LogWarning(string.Format("EventID to queue [{0}] has no listeners. Aborting operation!", eventID), Instance.gameObject);
				}
				return false;
			}

			lock (queueLock) {
				eventQueues[_activeQueue].Insert(0, new EventInstance(eventID, data));
			}

			if (Instance._verbose) {
				Debug.Log(string.Format("Event [{0}] successfully queued at [{1}]", eventID, Time.time), Instance.gameObject);
			}

			return true;
		}

		/// <summary>
		/// Removes an eventID from the active queue and/or scheduled event list
		/// If allOfType is set to true, all events with te same ID will be removed, otherwise the first evtInstance will be removed
		/// </summary>
		/// <param name="eventID"></param>
		/// <param name="allOfType"></param>
		/// <returns>true if an evtInstance was removed, false if manager is disabled, given ID is invalid, or no matching eventID was found</returns>
		public static bool AbortEvent(string eventID, bool allOfType = false)
		{
			if (Instance == null)
				return false;

			if (!Instance.enabled) {
				if (Instance._verbose) {
					Debug.LogWarning(string.Format("Attempting to abort event [{0}] while GameEventManager is disabled!", eventID), Instance.gameObject);
				}
				return false;
			}

			if (string.IsNullOrEmpty(eventID)) {
				if (Instance._verbose) {
					Debug.LogWarning("Attempting to abort [NULL] event!", Instance.gameObject);
				}
				return false;
			}

			//Remove all instances from active queue to prevent them being called on the next frame. Processing queue is not affected
			bool wasRemoved;
			lock (queueLock) {
				if (allOfType) {
					wasRemoved = eventQueues[_activeQueue].RemoveAll((evt) => evt.eventID.Equals(eventID)) > 0;
				} else {
					var evtInstance = eventQueues[_activeQueue].Find((evt) => evt.eventID.Equals(eventID));
					wasRemoved = eventQueues[_activeQueue].Remove(evtInstance);
				}
			}

			if (!allOfType) {
				if (!wasRemoved) {
					//Find first instanced in scheduled list
					var idx = scheduledEvents.FindIndex((pair) => pair.Key.Equals(eventID));
					if (idx == -1) { //No evtInstance was found
						if (Instance._verbose) {
							Debug.Log(string.Format("Event [{0}] was not found in event queue, and no evtInstance of it has been scheduled.", eventID));
						}
						return false;
					}

					//Remove evtInstance found
					if (Instance != null && scheduledEvents[idx].Value != null) {
						Instance.StopCoroutine(scheduledEvents[idx].Value);
						scheduledEvents.RemoveAt(idx);

						if (Instance._verbose) {
							Debug.Log(string.Format("One evtInstance of event [{0}] was successfully removed from scheduled event queue", eventID), Instance.gameObject);
						}
						return true;
					}

					if (Instance._verbose) {
						Debug.Log(string.Format("Something went wrong when attempting to abort event [{0}]. Either GameManager is unavailable or Scheduled Instance was processed before it could be aborted!", eventID), Instance.gameObject);
					}
					return false;
				}

				if (Instance._verbose) {
					Debug.Log(string.Format("One evtInstance of event [{0}] successfully removed from event queue", eventID), Instance.gameObject);
				}
				return true;
			}

			//Find all instances that have been scheduled
			var idxs = scheduledEvents.FindAll((pair) => pair.Key.Equals(eventID));
			if (idxs.Count == 0) {
				if (wasRemoved) {
					if (Instance._verbose) {
						Debug.Log(string.Format("All instances of event [{0}] were successfully removed from event queue", eventID), Instance.gameObject);
					}
					return true;
				}

				if (Instance._verbose) {
					Debug.Log(string.Format("Event [{0}] was not found in event queue, and no evtInstance of it has been scheduled.", eventID), Instance.gameObject);
				}
				return false;
			}

			//Remove all of instances found
			for (var i = idxs.Count - 1; i >= 0; i--) {
				if (scheduledEvents[i].Value != null) {
					Instance.StopCoroutine(scheduledEvents[i].Value);
					scheduledEvents.RemoveAt(i);

					wasRemoved = true;
				}
			}

			if (wasRemoved) {
				if (Instance._verbose) {
					Debug.Log(string.Format("All instances of event [{0}] were successfully removed from event queue as well as scheduled event queue", eventID), Instance.gameObject);
				}

				return true;
			}

			if (Instance._verbose) {
				Debug.Log(string.Format("Event [{0}] was not found in any event queue", eventID), Instance.gameObject);
			}
			return false;
		}

		/// <summary>
		/// Attempts to schedule a delayed event 
		/// </summary>
		/// <param name="eventID"></param>
		/// <param name="data"></param>
		/// <param name="delay"></param>
		/// <returns>true is scheduling was successfull, false otherwise</returns>
		public static bool ScheduleEvent(string eventID, float delay, params object[] data)
		{
			if (Instance == null)
				return false;

			if (!Instance.enabled) {
				if (Instance._verbose) {
					Debug.LogWarning(string.Format("Attempting to schedule event [{0}] while GameEventManager is disabled!", eventID), Instance.gameObject);
				}
				return false;
			}

			if (string.IsNullOrEmpty(eventID)) {
				if (Instance._verbose) {
					Debug.LogWarning("Attempting to schedule [NULL] event!", Instance.gameObject);
				}
				return false;
			}

			if (!EventHasListeners(eventID)) {
				if (Instance._verbose) {
					Debug.LogWarning(string.Format("EventID to schedule [{0}] has no listeners. Aborting schedule operation!", eventID), Instance.gameObject);
				}
				return false;
			}

			if (delay <= 0.1f) {
				if (Instance._verbose) {
					Debug.LogWarning(string.Format("Attempting to schedule event [{0}] with 0 sec delay! Raising event immediately.", eventID), Instance.gameObject);
				}

				RaiseEvent(eventID, data);
				return true;
			}

			if (Instance._verbose) {
				Debug.Log(string.Format("Event [{0}] scheduled at [{1}] to be raised at [{2}]!", eventID, Time.time, Time.time + delay), Instance.gameObject);
			}

			var coroutine = Instance.StartCoroutine(_ScheduleEvent(eventID, delay, scheduledEvents.Count, data));
			scheduledEvents.Add(new KeyValuePair<string, Coroutine>(eventID, coroutine));

			return true;
		}

		/// <summary>
		/// Checks if given EventID has listeners
		/// </summary>
		/// <param name="eventID"></param>
		/// <returns></returns>
		public static bool EventHasListeners(string eventID)
		{
			if (Instance == null)
				return false;

			if (!Instance.enabled) {
				if (Instance._verbose) {
					Debug.LogWarning("GameEventManager is disabled!", Instance.gameObject);
				}
				return false;
			}

			if (string.IsNullOrEmpty(eventID)) {
				if (Instance._verbose) {
					Debug.LogWarning("Given eventID is NULL!", Instance.gameObject);
				}
				return false;
			}

			return eventDelegates.ContainsKey(eventID) || gameEvents.ContainsKey(eventID);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventID"></param>
		/// <returns></returns>
		public static int CountEventListeners(string eventID)
		{
			if (Instance == null)
				return -1;

			if (!Instance.enabled) {
				if (Instance._verbose) {
					Debug.LogWarning("GameEventManager is disabled!", Instance.gameObject);
				}
				return -2;
			}

			if (string.IsNullOrEmpty(eventID)) {
				if (Instance._verbose) {
					Debug.LogWarning("Given eventID is NULL!", Instance.gameObject);
				}
				return -3;
			}

			if (!EventHasListeners(eventID))
				return 0;

			var cnt = 0;

			if (eventDelegates.ContainsKey(eventID))
				cnt += eventDelegates[eventID].Count;

			if (gameEvents.ContainsKey(eventID))
				cnt += gameEvents[eventID].Handler.Count;

			return cnt;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="eventID"></param>
		/// <returns></returns>
		public static bool GameEventExistsInAssetsFolder(string eventID)
		{
			if (Instance == null)
				return false;

			if (!Instance.enabled) {
				if (Instance._verbose) {
					Debug.LogWarning("GameEventManager is disabled!", Instance.gameObject);
				}
				return false;
			}

			if (string.IsNullOrEmpty(eventID)) {
				if (Instance._verbose) {
					Debug.LogWarning("Given eventID is NULL!", Instance.gameObject);
				}
				return false;
			}

			return gameEvents.ContainsKey(eventID);
		}
		#endregion

		//=====================================================================================================================//
		//=================================================== Public Methods ==================================================//
		//=====================================================================================================================//

		#region Debug Methods

		public void TestRaise()
		{
			RaiseEvent(eventID, testData);
		}

		#endregion
	}
}