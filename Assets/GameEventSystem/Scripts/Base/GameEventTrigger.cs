using UnityEngine;

namespace GameSystems.Events
{
	public enum TriggerType
	{
		Trigger,
		Collision,
		OnStart,
		EnableDisable,
		//TODO: if using a level loader, use this option and register to the "OnLoadginDone" event and raise events then
		OnLoadingDone, 
		Manual,
	}

	public enum ColliderType
	{
		Box,
		Sphere,
	}

	[AddComponentMenu("Game Event System/Game Event Trigger")]
	public sealed class GameEventTrigger : MonoBehaviour, IGameEventTrigger
	{
		private enum CheckType
		{
			None,
			OnEnter,
			OnStay,
			OnExit,
			OnEnable,
			OnDisable
		}
		//=====================================================================================================================//
		//================================================= Inspector Variables ===============================================//
		//=====================================================================================================================//

		#region Inspector Variables

		[SerializeField] private TriggerType _triggerType;
		[SerializeField] private ColliderType _colliderType;
		[SerializeField] private LayerMask _triggeredBy;
		[SerializeField] private Vector3 _areaSize;
		[SerializeField] private float _radius;

		[Tooltip("Trigger only once?")]
		[SerializeField] private bool _raiseOnce;

		[Tooltip("Should the event state persist when game is saved? [Requires integration with SaveLoadSystem]")]
		[SerializeField] private bool _persistentState;

		[SerializeField] private GameEventSet _events;
		[SerializeField] private GameEventSet _onEnableEvents;
		[SerializeField] private GameEventSet _onDisableEvents;
		[SerializeField] private GameEventSet _onEnterEvents;
		[SerializeField] private GameEventSet _onStayEvents;
		[SerializeField] private GameEventSet _onExitEvents;

		[Tooltip("Log state")]
		[SerializeField]
		private bool _verbose;

		#endregion

		//=====================================================================================================================//
		//=================================================== Private Fields ==================================================//
		//=====================================================================================================================//

		#region Private Fields

		private Collider _collider;

		private bool _hasBeenRaised;
		private bool _hasBeenRaisedOnEnable;
		private bool _hasBeenRaisedOnDisable;
		private bool _hasBeenRaisedOnEnter;
		private bool _hasBeenRaisedOnStay;
		private bool _hasBeenRaisedOnExit;

		private GameObject _triggeredByObj;

		#endregion

		//=====================================================================================================================//
		//============================================= Unity Callback Methods ================================================//
		//=====================================================================================================================//

		#region Unity Callback Methods

		private void Start()
		{
			/**
			 * TODO: Implement Save state check before initialization
			 *
			 * if (_persistentState == true){
			 * [1] Read saved state for relevant trigger type if available
			 * [2] trigger all events accordingly
				   return
			 * }
			 * Ignore and proceed with initialization otherwise 
			 **/
			
			Init();

			if (_triggerType == TriggerType.OnStart) {
				if (_verbose) {
					Debug.Log($"[{name}]: events raised [OnStart].");
				}

				_events.Raise();
				_hasBeenRaised = _raiseOnce;
				//TODO: if _persistentState, update event state with SaveLoadSystem
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (HasBeenRaised(CheckType.OnEnter)) {
				if (_verbose) {
					Debug.Log($"[{name}] events have already been raised.");
				}

				return;
			}

			if (!MaskContainsLayer(_triggeredBy, collision.gameObject.layer))
				return;

			if (_triggerType != TriggerType.Collision || _onEnterEvents == null || !_onEnterEvents.doRaise)
				return;

			_triggeredByObj = collision.gameObject;

			if (_verbose) {
				Debug.Log($"[{name}]: events raised [OnCollisionEnter] with {_triggeredByObj.name}.",
					gameObject);
			}

			_onEnterEvents?.Raise();
			_hasBeenRaisedOnEnter = _raiseOnce;
			//TODO: if _persistentState, update event state with SaveLoadSystem 
		}

		private void OnCollisionStay(Collision collision)
		{
			if (HasBeenRaised(CheckType.OnStay)) {
				if (_verbose) {
					Debug.Log($"[{name}] events have already been raised.");
				}

				return;
			}

			if (!MaskContainsLayer(_triggeredBy, collision.gameObject.layer))
				return;

			if (_triggerType != TriggerType.Collision || _onStayEvents == null || !_onStayEvents.doRaise)
				return;

			_triggeredByObj = collision.gameObject;

			if (_verbose) {
				Debug.Log($"[{name}]: events raised [OnCollisionStay] with {_triggeredByObj.name}.",
					gameObject);
			}

			_onStayEvents?.Raise();
			_hasBeenRaisedOnStay = _raiseOnce;
			//TODO: if _persistentState, update event state with SaveLoadSystem
		}

		private void OnCollisionExit(Collision collision)
		{
			if (HasBeenRaised(CheckType.OnExit)) {
				if (_verbose) {
					Debug.Log($"[{name}] events have already been raised.");
				}

				return;
			}

			if (!MaskContainsLayer(_triggeredBy, collision.gameObject.layer))
				return;

			if (_triggerType != TriggerType.Collision || _onExitEvents == null || !_onExitEvents.doRaise)
				return;

			_triggeredByObj = collision.gameObject;

			if (_verbose) {
				Debug.Log($"[{name}]: events raised [OnCollisionExit] with {_triggeredByObj.name}.",
					gameObject);
			}

			_onExitEvents?.Raise();
			_hasBeenRaisedOnExit = _raiseOnce;
			//TODO: if _persistentState, update event state with SaveLoadSystem
		}

		private void OnTriggerEnter(Collider other)
		{
			if (HasBeenRaised(CheckType.OnEnter)) {
				if (_verbose) {
					Debug.Log($"[{name}] events have already been raised.");
				}

				return;
			}

			if (!MaskContainsLayer(_triggeredBy, other.gameObject.layer))
				return;

			if (_triggerType != TriggerType.Trigger || _onEnterEvents == null || !_onEnterEvents.doRaise)
				return;

			_triggeredByObj = other.gameObject;

			if (_verbose) {
				Debug.Log($"[{name}]: events raised [OnTriggerEnter] by {_triggeredByObj.name}.",
					gameObject);
			}

			_onEnterEvents?.Raise();
			_hasBeenRaisedOnEnter = _raiseOnce;
			//TODO: if _persistentState, update event state with SaveLoadSystem
		}

		private void OnTriggerExit(Collider other)
		{
			if (HasBeenRaised(CheckType.OnExit)) {
				if (_verbose) {
					Debug.Log($"[{name}] events have already been raised.");
				}

				return;
			}

			if (_triggerType != TriggerType.Trigger || _onExitEvents == null || !_onExitEvents.doRaise)
				return;

			if (!MaskContainsLayer(_triggeredBy, other.gameObject.layer))
				return;

			_triggeredByObj = other.gameObject;

			if (_verbose) {
				Debug.Log($"[{name}]: events raised [OnTriggerExit] by {_triggeredByObj.name}.",
					gameObject);
			}

			_onExitEvents?.Raise();
			_hasBeenRaisedOnExit = _raiseOnce;
			//TODO: if _persistentState, update event state with SaveLoadSystem
		}

		private void OnTriggerStay(Collider other)
		{
			if (HasBeenRaised(CheckType.OnStay)) {
				if (_verbose) {
					Debug.Log($"[{name}] events have already been raised.");
				}

				return;
			}

			if (_triggerType != TriggerType.Trigger || _onStayEvents == null || !_onStayEvents.doRaise)
				return;

			if (!MaskContainsLayer(_triggeredBy, other.gameObject.layer))
				return;

			_triggeredByObj = other.gameObject;

			if (_verbose) {
				Debug.Log($"[{name}]: events raised [OnTriggerStay] by {_triggeredByObj.name}.");
			}

			_onStayEvents?.Raise();
			_hasBeenRaisedOnStay = _raiseOnce;
			//TODO: if _persistentState, update event state with SaveLoadSystem
		}

		private void OnEnable()
		{
			if (HasBeenRaised(CheckType.OnEnable)) {
				if (_verbose) {
					Debug.Log($"[{name}] events have already been raised.");
				}
				return;
			}

			if (_triggerType == TriggerType.EnableDisable && _onEnableEvents.doRaise) {
				if (_verbose) {
					Debug.Log($"[{name}]: events raised [OnEnable].");
				}

				_onEnableEvents?.Raise();
				_hasBeenRaisedOnEnable = _raiseOnce;
				//TODO: if _persistentState, update event state with SaveLoadSystem
			}
		}

		private void OnDisable()
		{
			if (HasBeenRaised(CheckType.OnDisable)) {
				if (_verbose) {
					Debug.Log($"[{name}] events have already been raised.");
				}
				return;
			}

			if (_triggerType == TriggerType.EnableDisable && _onDisableEvents.doRaise) {
				if (_verbose) {
					Debug.Log($"[{name}]: events raised [OnDisable].");
				}

				_onDisableEvents?.Raise();
				_hasBeenRaisedOnDisable = _raiseOnce;
				//TODO: if _persistentState, update event state with SaveLoadSystem
			}
		}
		
		#endregion

		//=====================================================================================================================//
		//=================================================== Public Methods ==================================================//
		//=====================================================================================================================//

		#region Public Methods

		[ContextMenu("Raise Events")]
		public void RaiseEvents()
		{
			if (HasBeenRaised(CheckType.None)) {
				if (_verbose) {
					Debug.Log($"[{name}] events have already been raised.");
				}
				return;
			}

			if (_triggerType == TriggerType.Manual && _events.doRaise) {
				if (_verbose) {
					Debug.Log($"[{name}]: events raised [Manual].");
				}

				_events?.Raise();
				//TODO: if _persistentState, update event state with SaveLoadSystem
			}
		}

		public void RaiseEvent(GameEvent @event)
		{
			if(@event != null)
				@event.Raise();
		}

		#endregion

		//=====================================================================================================================//
		//================================================== Private Methods ==================================================//
		//=====================================================================================================================//

		#region Private Methods

		private bool HasBeenRaised(CheckType type)
		{
			switch (_triggerType) {
				case TriggerType.Collision:
				case TriggerType.Trigger:
					if (type == CheckType.OnEnter && _onEnterEvents != null && _onEnterEvents.doRaise)
						return _raiseOnce && _hasBeenRaisedOnEnter;

					if (type == CheckType.OnStay && _onStayEvents != null && _onStayEvents.doRaise)
						return _raiseOnce && _hasBeenRaisedOnStay;

					if (type == CheckType.OnExit && _onExitEvents != null && _onExitEvents.doRaise)
						return _raiseOnce && _hasBeenRaisedOnExit;
					break;
				case TriggerType.EnableDisable:
					if (type == CheckType.OnEnable && _onEnableEvents != null && _onEnableEvents.doRaise)
						return _raiseOnce && _hasBeenRaisedOnEnable;

					if (type == CheckType.OnDisable && _onDisableEvents != null && _onDisableEvents.doRaise)
						return _raiseOnce && _hasBeenRaisedOnDisable;
					break;
			}

			return (_raiseOnce && _hasBeenRaised);
		}

		[ContextMenu("Init")]
		private void Init()
		{
			if (_triggerType == TriggerType.Collision || _triggerType == TriggerType.Trigger) {
				if (_colliderType == ColliderType.Box) {
					_collider = AddMissingComponent<BoxCollider>(gameObject);
					if (_collider != null) {
						((BoxCollider) _collider).size = _areaSize;
						((BoxCollider) _collider).center = _areaSize / 2f;
						_collider.isTrigger = _triggerType == TriggerType.Trigger;
					}
				} else if (_colliderType == ColliderType.Sphere) {
					_collider = AddMissingComponent<SphereCollider>(gameObject);
					if (_collider != null) {
						((SphereCollider) _collider).radius = _radius;
						((SphereCollider) _collider).center = Vector3.zero;
						_collider.isTrigger = _triggerType == TriggerType.Trigger;
					}
				}
			} else {
				var extraCollider = gameObject.GetComponent<Collider>();
				if (extraCollider != null) {
					if (Application.isPlaying)
						Destroy(extraCollider);
					else
						DestroyImmediate(extraCollider);
				}
			}
		}

		#endregion

		//=====================================================================================================================//
		//================================================ Debugging & Testing ================================================//
		//=====================================================================================================================//

		#region Debugging & Testing

		public bool IsValid(out string message)
		{
			if (_triggerType == TriggerType.Trigger || _triggerType == TriggerType.Collision) {
				if (_triggeredBy.value == 0) {
					message = $"GameEventTriggerAsset[{name}] - TriggeredBy field has not been initialized.";

					if (_verbose)
						Debug.LogWarning(message, gameObject);

					return false;
				}

				if (_colliderType == ColliderType.Box) {
					if (_areaSize == Vector3.zero) {
						message = $"GameEventTriggerAsset[{name}] - AreaSize field has not been initialized.";

						if (_verbose)
							Debug.LogWarning(message, gameObject);


						return false;
					}
				}

				if (_colliderType == ColliderType.Sphere) {
					if (_radius == 0f) {
						message = $"GameEventTriggerAsset[{name}] - Radius field has not been initialized.";

						if (_verbose)
							Debug.LogWarning(message, gameObject);

						return false;
					}
				}

				if ((_onExitEvents == null || !_onExitEvents.doRaise) && (_onEnterEvents == null || !_onEnterEvents.doRaise) && (_onStayEvents == null || !_onStayEvents.doRaise)) {
					message = $"GameEventTriggerAsset[{name}] - Trigger has not been properly initialized. Missing Event references.";
					if (_verbose)
						Debug.LogWarning(message, gameObject);

					return false;
				}

				if (_onEnterEvents != null && _onEnterEvents.doRaise) {
					if (!_onEnterEvents.IsValid()) {
						message = $"GameEventTriggerAsset[{name}] - Trigger has not been properly initialized. Missing OnEnterEvent references.";

						if (_verbose)
							Debug.LogWarning(message, gameObject);

						return false;
					}
				}

				if (_onExitEvents != null && _onExitEvents.doRaise) {
					if (!_onExitEvents.IsValid()) {
						message = $"GameEventTriggerAsset[{name}] - Trigger has not been properly initialized. Missing OnExitEvent references.";

						if (_verbose)
							Debug.LogWarning(message, gameObject);

						return false;
					}
				}

				if (_onStayEvents != null && _onStayEvents.doRaise) {
					if (!_onStayEvents.IsValid()) {
						message = $"GameEventTriggerAsset[{name}] - Trigger has not been properly initialized. Missing OnStayEvent references.";

						if (_verbose)
							Debug.LogWarning(message, gameObject);

						return false;
					}
				}
			} else if (_triggerType == TriggerType.EnableDisable) {
				if ((_onDisableEvents == null || !_onDisableEvents.doRaise) && (_onEnableEvents == null || !_onEnableEvents.doRaise)) {
					message = $"GameEventTriggerAsset[{name}] - Trigger has not been properly initialized. Missing Event references.";
					if (_verbose)
						Debug.LogWarning(message, gameObject);

					return false;
				}

				if (_onDisableEvents != null && _onDisableEvents.doRaise) {
					if (!_onDisableEvents.IsValid()) {
						message = $"GameEventTriggerAsset[{name}] - Trigger has not been properly initialized. Missing OnDisableEvent references.";

						if (_verbose)
							Debug.LogWarning(message, gameObject);

						return false;
					}
				} else if (_onEnableEvents != null && _onEnableEvents.doRaise) {
					if (!_onEnableEvents.IsValid()) {
						message = $"GameEventTriggerAsset[{name}] - Trigger has not been properly initialized. Missing Event references.";

						if (_verbose)
							Debug.LogWarning(message, gameObject);

						return false;
					}
				}
			} else {
				if (!_events.IsValid()) {
					message = $"GameEventTriggerAsset[{name}] - Trigger has not been properly initialized. Missing Event references.";

					if (_verbose)
						Debug.LogWarning(message, gameObject);

					return false;
				}
			}

			message = "GameEventTriggerAsset is valid.";
			return true;
		}

		private static bool MaskContainsLayer(LayerMask mask, int layer)
		{
			return (mask == (mask | (1 << layer)));
		}

		private static T AddMissingComponent<T>(GameObject go) where T : Component
		{
			if (go == null)
				return null;

			var comp = go.GetComponent<T>();
			if (comp == null)
				comp = go.AddComponent<T>();

			return comp;
		}

		private void OnDrawGizmosSelected()
		{
			if (_triggerType.ToString().Contains("Trigger") || _triggerType.ToString().Contains("Collision")) {
				if (_colliderType == ColliderType.Box) {
					Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
					Gizmos.color = Color.yellow;
					Gizmos.DrawWireCube(_areaSize / 2f, _areaSize);
				} else if (_colliderType == ColliderType.Sphere) {
					Gizmos.color = Color.cyan;
					Gizmos.DrawWireSphere(transform.position, _radius);
				}
			}
		}

		#endregion
	}
}