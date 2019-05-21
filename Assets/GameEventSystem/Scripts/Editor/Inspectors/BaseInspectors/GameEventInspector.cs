using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;

namespace GameSystems.Events.Inspectors
{
	[CustomEditor(typeof(GameEvent), true)]
	public class GameEventInspector : Editor
	{
		static Editor cachedEditor;

		//=====================================================================================================================//
		//============================================= Unity Callback Methods ================================================//
		//=====================================================================================================================//

		#region Unity Callback Methods

		void OnEnable()
		{
			cachedEditor = null;
		}

		public override void OnInspectorGUI()
		{

			cachedEditor = CreateEditor(target);
			cachedEditor.DrawDefaultInspector();

			if (Application.isPlaying) {

				var gameEvent = (GameEvent)target;

				var text = "";
				if (gameEvent.Handler != null) {
					if (gameEvent.Handler.Count > 1) {
						var listeners = gameEvent.Handler.Listeners;
						for (int i = 1; i < listeners.Length; i++) {
							if (listeners[i].Method != null)
								text += listeners[i].Method.ToString() + "\n";
						}
					} else {
						text = "No Listeners are curently registered!";
					}
				} else {
					text = "Handler is null!";
				}

				GUILayout.Label("----------------------------------", EditorStyles.centeredGreyMiniLabel);
				EditorGUILayout.TextArea(text, EditorStyles.centeredGreyMiniLabel);
				GUILayout.Label("----------------------------------", EditorStyles.centeredGreyMiniLabel);

				if (GUILayout.Button("Raise")) {
					if (target != null)
						((GameEvent)target).Raise(null);
					serializedObject.ApplyModifiedProperties();
					return;

				}
			}

			serializedObject.ApplyModifiedProperties();
		}

		#endregion
	}
}
#endif