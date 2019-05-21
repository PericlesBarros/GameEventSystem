using UnityEngine;

namespace GameSystems.Events.Integration
{
	public abstract class GameEventBehaviour : MonoBehaviour, IGameEventListener, IGameEventTrigger
	{
		//=====================================================================================================================//
		//================================================ Inspector Variables ================================================//
		//=====================================================================================================================//

		#region Inspector Variables

		[SerializeField]
		protected bool verbose;

		[Header ("Delegates")]
		[SerializeField]
		protected string listenToEventID;
		[SerializeField]
		protected string raiseEventID;

		[Header("Game Events")]
		[SerializeField]
		protected GameEvent listenToGameEvent;
		[SerializeField]
		protected GameEvent raiseGameEvent;


		#endregion

		//=====================================================================================================================//
		//================================================ Unity Event Callbacks ==============================================//
		//=====================================================================================================================//

		#region Unity Event Callbacks

		protected void OnEnable()
		{

			if (string.IsNullOrEmpty(listenToEventID)) {
				var result = GameEventManager.RegisterHandler(listenToEventID, OnEventRaised);

				if (verbose) {
					Debug.Log(string.Format("GameEventBehaviour [{0}]: Handler Registration: {1}", name, (result ? "SUCCESS" : "FAILURE")), gameObject);
				}
			}

			if (listenToGameEvent != null) {
				var result = listenToGameEvent.RegisterMethod(OnEventRaised);

				if (verbose) {
					Debug.Log(string.Format("GameEventBehaviour [{0}]: Method Registration: {1}", name, (result ? "SUCCESS" : "FAILURE")), gameObject);
				}
			}
		}

		protected void OnDisable()
		{
			if (!string.IsNullOrEmpty(listenToEventID)) {
				var result = GameEventManager.UnregisterHandler(listenToEventID, OnEventRaised);

				if (verbose) {
					Debug.Log(string.Format("GameEventBehaviour [{0}]: Handler Unregistration: {1}", name, (result ? "SUCCESS" : "FAILURE")), gameObject);
				}
			} else {
				if (verbose) {
					Debug.LogWarning(string.Format("GameEventBehaviour [{0}]: GameEventID is null!", name), gameObject);
				}
			}

			if (listenToGameEvent != null) {
				var result = listenToGameEvent.UnregisterMethod(OnEventRaised);

				if (verbose) {
					Debug.Log(string.Format("GameEventBehaviour [{0}]: Method Unregistration: {1}", name, (result ? "SUCCESS" : "FAILURE")), gameObject);
				}
			} else {
				if (verbose) {
					Debug.LogWarning(string.Format("GameEventBehaviour [{0}]: GameEvent reference is null!", name), gameObject);
				}
			}
		}

		#endregion

		//=====================================================================================================================//
		//=================================================== Public Methods ==================================================//
		//=====================================================================================================================//

		#region Public Methods

		[GameEventHandler]
		public abstract void OnEventRaised(object data);

		#endregion
	}
}