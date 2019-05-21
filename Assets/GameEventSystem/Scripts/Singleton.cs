using UnityEngine;

namespace GameSystems.Patterns
{
	[DisallowMultipleComponent]
	public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		//=====================================================================================================================//
		//=================================================== Private Fields ==================================================//
		//=====================================================================================================================//

		#region Private Fields

		private static T _instance;
		private static readonly object _lock = new Object();
		private static bool _applicationIsQuitting;

		#endregion

		//=====================================================================================================================//
		//=================================================== Public Fields ===================================================//
		//=====================================================================================================================//

		#region Public Fields

		public static T Instance
		{
			get {
				if (_applicationIsQuitting) {
					Debug.LogWarning ("[Singleton] Instance '" + typeof(T) + "' already destroyed on application quit. Won't create again - returning null.");
					return null;
				}

				lock (_lock) {
					if (_instance != null)
						return _instance;
					
					//Check if there's an object in the active scene
					_instance = FindObjectOfType<T>();
					if (_instance == null) {
						// Check if prefab exists in Resources Folder.
						// -- Prefab must have the same @name as the Singleton SubClass
						var prefab = Resources.Load<GameObject>("Control/" + typeof(T));

						//Create singleton from prefab or as new gameObject
						_instance = prefab != null ? Instantiate(prefab).GetComponent<T>() : new GameObject(typeof(T).ToString()).AddComponent<T>();
					}

					if (Application.isPlaying)
						DontDestroyOnLoad(_instance.gameObject);
				}

				return _instance;
			}
		}

		#endregion

		//=====================================================================================================================//
		//=============================================== Unity Event Methods =================================================//
		//=====================================================================================================================//

		#region Unity Event Functions

		protected virtual void Awake()
		{
			if (_instance == null) {
				_instance = this as T;
				DontDestroyOnLoad(gameObject);
				Initialize();
			} else
				Destroy(gameObject);
		}

		protected virtual void Initialize() { }

		protected virtual void OnDestroy ()
		{
			_applicationIsQuitting = true;
		}

		#endregion
	}
}