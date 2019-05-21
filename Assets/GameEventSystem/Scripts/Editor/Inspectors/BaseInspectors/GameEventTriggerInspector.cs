using UnityEngine;
using UnityEditorInternal;
using GameSystems.Tools;
using System;

#if UNITY_EDITOR
using UnityEditor;


namespace GameSystems.Events.Inspectors
{

	[CustomEditor(typeof(GameEventTrigger))]
	[CanEditMultipleObjects()]
	public class GameEventTriggerInspector : Editor
	{
		//=====================================================================================================================//
		//=================================================== Private Fields ==================================================//
		//=====================================================================================================================//

		#region Private Fields

		private static Color _defaultColor;

		private SerializedProperty _triggerType;
		private SerializedProperty _raiseOnce;
		private SerializedProperty _persistentState;
		private SerializedProperty _verbose;
		private SerializedProperty _colliderType;
		private SerializedProperty _triggeredBy;

		private SerializedProperty _areaSize;
		private SerializedProperty _radius;

		private SerializedProperty _events;
		private SerializedProperty _onEnableEvents;
		private SerializedProperty _onDisableEvents;

		private SerializedProperty _onEnterEvents;
		private SerializedProperty _onExitEvents;
		private SerializedProperty _onStayEvents;
        

		#endregion

		//=====================================================================================================================//
		//============================================= Unity Callback Methods ================================================//
		//=====================================================================================================================//

		#region Unity Callback Methods

		public void OnEnable()
		{	
			_triggerType = serializedObject.FindProperty("_triggerType");
				
			_raiseOnce = serializedObject.FindProperty("_raiseOnce");
			_persistentState = serializedObject.FindProperty("_persistentState");
			_verbose = serializedObject.FindProperty("_verbose");

			_colliderType = serializedObject.FindProperty("_colliderType");
			_triggeredBy = serializedObject.FindProperty("_triggeredBy");
			_areaSize = serializedObject.FindProperty("_areaSize");
			_radius = serializedObject.FindProperty("_radius");

			_events = serializedObject.FindProperty("_events");
			_onEnableEvents = serializedObject.FindProperty("_onEnableEvents");
			_onDisableEvents = serializedObject.FindProperty("_onDisableEvents");
			_onEnterEvents = serializedObject.FindProperty("_onEnterEvents");			
			_onExitEvents = serializedObject.FindProperty("_onExitEvents");
			_onStayEvents = serializedObject.FindProperty("_onStayEvents");
		}

		public override void OnInspectorGUI()
		{			
			_defaultColor = GUI.color;
			
			this.DrawMonoBehaviourScriptField<GameEventTrigger>();

			serializedObject.Update();

			if (!GlobalSettings.DrawCustomInspectors) {
				base.OnInspectorGUI();
			} else {				
				DrawTriggerType();

				EditorGUILayout.PropertyField(_raiseOnce);
				EditorGUILayout.PropertyField(_persistentState);
				EditorGUILayout.PropertyField(_verbose);
			}
			
			serializedObject.ApplyModifiedProperties();
		}

		#endregion

		//=====================================================================================================================//
		//================================================= Helper Methods ====================================================//
		//=====================================================================================================================//

		#region Helper Methods

		private void DrawTriggerType()
		{
			EditorGUILayout.PropertyField(_triggerType);
			var type =  (TriggerType) _triggerType.intValue;

			switch (type) {
				case TriggerType.Collision:					
				case TriggerType.Trigger:
					DrawTriggeredBy();
					DrawColliderType();
					
					EditorGUILayout.PropertyField(_onEnterEvents, new GUIContent("Raise On Enter: "));
					EditorGUILayout.PropertyField(_onStayEvents, new GUIContent("Raise On Stay: "));
					EditorGUILayout.PropertyField(_onExitEvents, new GUIContent("Raise On Exit: "));
					break;
				case TriggerType.EnableDisable:
					EditorGUILayout.PropertyField(_onEnableEvents, new GUIContent("Raise OnEnable: "));
					EditorGUILayout.PropertyField(_onDisableEvents, new GUIContent("Raise OnDisable: "));
					break;
				default:
					var raiseEvents = _events.FindPropertyRelative("doRaise");
					raiseEvents.boolValue = true;
					EditorGUILayout.PropertyField(_events, GUIContent.none);
					break;
			}

			GUI.color = _defaultColor;
		}

		private void DrawColliderType()
		{
			EditorGUILayout.PropertyField(_colliderType);
			if (_colliderType.enumValueIndex == 0) { // Box Collider							
				if (_areaSize.vector3Value == Vector3.zero) {
					GUI.color = Color.red;
				}
				EditorGUILayout.PropertyField(_areaSize);
				GUI.color = _defaultColor;
			} else if (_colliderType.enumValueIndex == 1) { // Sphere Collider							
				if (_radius.floatValue == 0f) {
					GUI.color = Color.red;
				}
				EditorGUILayout.PropertyField(_radius);
				GUI.color = _defaultColor;
			}
		}

		private void DrawTriggeredBy()
		{
			if (_triggeredBy.intValue == 0) {
				GUI.color = Color.red;
			}

			EditorGUILayout.PropertyField(_triggeredBy);
			GUI.color = _defaultColor;
		}

		private void DrawBox(SerializedProperty raiseOn, ReorderableList list, string label)
		{
			GUILayout.BeginVertical("Box");
			EditorGUILayout.PropertyField(raiseOn, new GUIContent(label));
			if (raiseOn.boolValue) {
				EditorGUILayout.Space();
				EditorGUI.indentLevel++;
				list.DoLayoutList();
				EditorGUI.indentLevel--;
			}
			GUILayout.EndVertical();
		}

		private void RegisterHeader(ReorderableList list, string header)
		{
			list.drawHeaderCallback = (rect) => {
				EditorGUI.LabelField(rect, header);
            };
		}

		private void RegisterDrawer(ReorderableList list)
		{
			list.drawElementCallback = (rect, idx, isActive, isFocused) => {

				rect.height = EditorGUIUtility.singleLineHeight;

				var property = list.serializedProperty.GetArrayElementAtIndex(idx);

				var type = property.FindPropertyRelative("type");
				var gameEvent = property.FindPropertyRelative("gameEvent");
				var eventID = property.FindPropertyRelative("eventID");

				EditorGUI.BeginProperty(rect, GUIContent.none, property);

				GUILayout.BeginHorizontal();
				var typeRect = rect;
				typeRect.width = EditorGUIUtility.labelWidth - 10;
				EditorGUI.PropertyField(typeRect, type, GUIContent.none);

				var eventRect = rect;
				eventRect.x += typeRect.width - 5;
				eventRect.width = rect.width - typeRect.width;
				switch (type.enumValueIndex) {
					case 0:
						EditorGUI.PropertyField(eventRect, gameEvent, GUIContent.none);
						break;
					case 1:
						EditorGUI.PropertyField(eventRect, eventID, GUIContent.none);
						break;
				}

				GUILayout.EndHorizontal();
				EditorGUI.EndProperty();
			};
		}
		#endregion
	}
}
#endif