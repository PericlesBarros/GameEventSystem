using UnityEngine;
using GameSystems.Tools;

#if UNITY_EDITOR
using UnityEditor;

namespace GameSystems.Events.Inspectors
{
	[CustomEditor(typeof(GameEventListener))]
	public class GameEventListenerInspector : Editor
	{
        //=====================================================================================================================//
        //=================================================== Private Fields ==================================================//
        //=====================================================================================================================//

        #region Private Fields

		private static Color _defaultColor;
		private static GUIStyle _popupStyle;
		private static GameEventHandler _copiedHandler;
		private static int _clickedHandlerIdx = -1;

		private GameEventListener _eventListener;
		private SerializedProperty _handlers;

        #endregion

        //=====================================================================================================================//
        //============================================= Unity Callback Methods ================================================//
        //=====================================================================================================================//

        #region Unity Callback Methods

        private void OnEnable()
        {            
            _handlers = serializedObject.FindProperty("handlers");
            _eventListener = target as GameEventListener;
            _defaultColor = GUI.color;
        }

        public override void OnInspectorGUI()
		{
            if (_popupStyle == null) {
	            _popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions")) {
		            imagePosition = ImagePosition.ImageOnly,
		            alignment = TextAnchor.LowerCenter
	            };
            }

			var listener = (GameEventListener)target;
			if (listener == null)
				return;

			serializedObject.Update();

			this.DrawMonoBehaviourScriptField<GameEventListener>();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("verbose"));

			GUILayout.Label("-------------------------------- Game Events -------------------------------", EditorStyles.centeredGreyMiniLabel);

			var indentLvl = EditorGUI.indentLevel;
			EditorGUI.indentLevel++;

			for (var i = 0; i < listener.handlers.Count; i++) {
				DrawHandler(i, _handlers.GetArrayElementAtIndex(i));
			}

			EditorGUI.indentLevel--;

			DrawHandlerListButtons(listener);
			EditorGUI.indentLevel = indentLvl;
			serializedObject.ApplyModifiedProperties();

		}

		#endregion

		//=====================================================================================================================//
		//================================================ Helper Methods =====================================================//
		//=====================================================================================================================//

		#region Helper Methods

		private void DrawHandler(int curHandlerIdx, SerializedProperty handler)
		{
			GUILayout.BeginVertical("Box");

			var currentHandler = _eventListener.handlers[curHandlerIdx];
			var isMutedSelfProp = handler.FindPropertyRelative("isMuted");
			var isOpenProp = handler.FindPropertyRelative("isOpen");
			
			GUILayout.BeginHorizontal();

			string tooltip;
			//Color field in different colors depending on its validity
			if (currentHandler.isMuted) {
				GUI.color = Color.gray;
			} else if (!currentHandler.IsValid(out tooltip)) {
				GUI.color = Color.red;
			}

			var handlerRect = EditorGUILayout.GetControlRect();
			var evt = Event.current;

			isOpenProp.boolValue = EditorGUI.Foldout(handlerRect, isOpenProp.boolValue, new GUIContent(currentHandler.Name), true, EditorStyles.foldout);

			//Show Context Menu for handler
			if (handlerRect.Contains(evt.mousePosition) && evt.type == EventType.ContextClick) {
				_clickedHandlerIdx = curHandlerIdx;
				var menu = new GenericMenu();
				menu.AddItem(new GUIContent("Inspect"), false, currentHandler.Inspect);
				menu.AddItem(new GUIContent("Copy"), false, () => {
					_copiedHandler = currentHandler;
				});

				if (_copiedHandler == null) {
					menu.AddDisabledItem(new GUIContent("Paste"));
				} else {
					menu.AddItem(new GUIContent("Paste"), false, () => {
						if (_clickedHandlerIdx >= 0 && _clickedHandlerIdx < _handlers.arraySize) {
							Undo.RecordObject(_eventListener, "Copy Paste Handler");
							_eventListener.handlers[_clickedHandlerIdx] = GameEventHandler.Clone(_copiedHandler);
							serializedObject.ApplyModifiedProperties();
						}
					});
				}
				menu.ShowAsContext();
				evt.Use();
			}

			ResetColor();

			//Draw reordering buttons
			if (DrawHandlerButtons(curHandlerIdx, _eventListener, isMutedSelfProp)) {
				serializedObject.ApplyModifiedProperties();
				return;
			}

			GUILayout.EndHorizontal();

			if (isOpenProp.boolValue) {
				EditorGUI.indentLevel++;

				{// Draw GameEventReference
					var gameEvent = handler.FindPropertyRelative("Event");

					//Color field in different colors depending on its validity
					if (currentHandler.isMuted) {
						GUI.color = Color.gray;
					} else if (!currentHandler.Event.IsValid())
						GUI.color = Color.red;
					else
						ResetColor();

					EditorGUILayout.PropertyField(gameEvent, new GUIContent("Listen to"));
					ResetColor();
				}

				GUILayout.Label("-------------------------------- Targets -------------------------------", EditorStyles.centeredGreyMiniLabel);

				var eventTargets = handler.FindPropertyRelative("_targets");

				EditorGUILayout.PropertyField(eventTargets, new GUIContent(""+curHandlerIdx));

				DrawTargetListButtons(_eventListener, curHandlerIdx);

				EditorGUI.indentLevel--;
			}

			GUILayout.EndVertical();
		
			GUILayout.Space(2);
		}

		private void DrawHandlerListButtons(GameEventListener listener)
		{
			ResetColor();

			if (GUILayout.Button("Add Handler", GUI.skin.FindStyle("ToolbarButton"))) {
				Undo.RecordObject(listener, "Add New Handler");
				listener.AddHandler();
				serializedObject.ApplyModifiedProperties();
			} else if (GUILayout.Button("Clear Handlers", GUI.skin.FindStyle("ToolbarButton"))) {
				if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete all handlers?", "Yes", "No")) {
					Undo.RecordObject(listener, "Clear All Handlers");
					listener.ClearHandlers();
					serializedObject.ApplyModifiedProperties();
				}
			}
		}

		private bool DrawHandlerButtons(int curHandlerIdx, GameEventListener listener, SerializedProperty mutedProperty)
		{
			ResetColor();
			
			//Draw mute button
			if (GUILayout.Button(new GUIContent(mutedProperty.boolValue ? "U": "M", mutedProperty.boolValue ? "Unmute" : "Mute"), GUI.skin.FindStyle("PreButton"), GUILayout.MaxWidth(20))) {
				mutedProperty.boolValue = !mutedProperty.boolValue;
			}

			if (Application.isPlaying) {
				var wasEnabled = GUI.enabled;
				GUI.enabled = !listener.handlers[curHandlerIdx].isMuted;
				if (GUILayout.Button("Raise", GUI.skin.FindStyle("PreButton"), GUILayout.MaxWidth(75))) {
					listener.handlers[curHandlerIdx].OnEventRaised(null);
				}
				GUI.enabled = wasEnabled;
				return false;
			}
			
			//Draw shift down button
			if (curHandlerIdx == listener.handlers.Count - 1) {
				EditorGUI.BeginDisabledGroup(true);
				GUILayout.Button("﹀", GUI.skin.FindStyle("PreButton"), GUILayout.MaxWidth(25));
				EditorGUI.EndDisabledGroup();
			} else {
				if (GUILayout.Button(new GUIContent("﹀", "Shift Handler Down"), GUI.skin.FindStyle("PreButton"), GUILayout.MaxWidth(25))) {
					Undo.RecordObject(listener, "Shift Handler Down");
					listener.ShiftHandlerDown(curHandlerIdx);
					serializedObject.ApplyModifiedProperties();
				}
			}

			//Draw shift up button
			if (curHandlerIdx == 0) {
				EditorGUI.BeginDisabledGroup(true);
				GUILayout.Button("︿", GUI.skin.FindStyle("PreButton"), GUILayout.MaxWidth(25));
				EditorGUI.EndDisabledGroup();
			} else {
				if (GUILayout.Button(new GUIContent("︿", "Shift Handler Up"), GUI.skin.FindStyle("PreButton"), GUILayout.MaxWidth(25))) {
					Undo.RecordObject(listener, "Shift Handler Up");
					listener.ShiftHandlerUp(curHandlerIdx);
					serializedObject.ApplyModifiedProperties();
				}
			}

			//Draw delete button
			if (GUILayout.Button(new GUIContent("X", "Remove Handler"), GUI.skin.FindStyle("PreButton"), GUILayout.MaxWidth(20))) {
                if (listener.handlers[curHandlerIdx].TargetCount > 0) {
                    if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to remove this handler?", "Yes", "No")) {
                        Undo.RecordObject(listener, "Remove Handler");
                        listener.RemoveHandler(curHandlerIdx);
                        serializedObject.ApplyModifiedProperties();
                        GUILayout.EndHorizontal();
                        return true;
                    }
                } else {
                    listener.RemoveHandler(curHandlerIdx);
                    serializedObject.ApplyModifiedProperties();
                    GUILayout.EndHorizontal();
                    return true;
                }
			}

			return false;
		}

		private void DrawTargetListButtons(GameEventListener listener, int curHandlerIdx)
		{
			if (Application.isPlaying) {
				return;
			}
			
			ResetColor();
                        
			EditorGUILayout.Space();

            if (GUILayout.Button("Clear Targets", GUI.skin.FindStyle("ToolbarButton"))) {
                if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete all targets?", "Yes", "No")) {
                    Undo.RecordObject(listener, "Clear All Targets");
                    listener.handlers[curHandlerIdx].ClearTargets();
                    serializedObject.ApplyModifiedProperties();
                }
            }            
		}

		private static void ResetColor()
		{
			GUI.color = _defaultColor;
		}

		#endregion        
	}
#endif
}