using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
#if UNITY_EDITOR

namespace GameSystems.Events.Drawers
{
	[CustomPropertyDrawer(typeof(GameEventSet))]
	public class GameEventSetDrawer : PropertyDrawer
	{
		//=====================================================================================================================//
		//=================================================== Private Fields ==================================================//
		//=====================================================================================================================//

		#region Private Fields

		private SerializedObject _serializedObject;
		private SerializedProperty _doRaise;
		private SerializedProperty _events;

		private ReorderableList _eventsList;

		private string _header;

		private bool _hasBeenInitialized;

		#endregion
			
		//=====================================================================================================================//
		//============================================= Unity Callback Methods ================================================//
		//=====================================================================================================================//

		#region Unity Callback Methods
						
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!_hasBeenInitialized) {
				_hasBeenInitialized = true;
				_serializedObject = property.serializedObject;
				_doRaise = property.FindPropertyRelative("doRaise");
				_events = property.FindPropertyRelative("events");

				var listName = property.propertyPath.Replace("_", "");
				_header = listName.First().ToString().ToUpper() + listName.Substring(1);

				_eventsList = new ReorderableList(_serializedObject, _events, true, true, true, true);
				_eventsList.drawHeaderCallback += DrawHeader;
				_eventsList.drawElementCallback += DrawElement;				
			}

			var indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			EditorGUI.BeginProperty(position, label, property);

			EditorGUI.BeginChangeCheck();
			GUILayout.BeginVertical("Box");
			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.PropertyField(_doRaise, new GUIContent("Raise " + _header));
				EditorGUI.BeginChangeCheck();

				if (!Application.isPlaying) {
					if (_doRaise.boolValue && _eventsList.count > 0 && GUILayout.Button("Clear", GUI.skin.FindStyle("toolbarbutton"))) {
						if (EditorUtility.DisplayDialog("Warning!", "Are you sure?", "Yes", "No")) {
							if (EditorGUI.EndChangeCheck()) {
								_events.ClearArray();
								_serializedObject.ApplyModifiedProperties();
							}
						}
					}
				}
				position.y += EditorGUIUtility.singleLineHeight;
			}
			GUILayout.EndHorizontal();

			if (_doRaise.boolValue) {
				EditorGUI.indentLevel++;
				EditorGUI.BeginChangeCheck();
				_eventsList.DoLayoutList();
				if (EditorGUI.EndChangeCheck()) {
					_events.serializedObject.ApplyModifiedProperties();
				}
				EditorGUI.indentLevel--;
			}

			GUILayout.EndVertical();

			EditorGUI.indentLevel = indentLevel;

			if (EditorGUI.EndChangeCheck()) {
				_serializedObject.ApplyModifiedProperties();
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{			
			float height = 0;
			if(_doRaise != null && _doRaise.boolValue) {
				if(_eventsList != null)
					_eventsList.GetHeight();
			}

			return height;
		}

		#endregion

		//=====================================================================================================================//
		//================================================== Private Methods ==================================================//
		//=====================================================================================================================//

		#region Private Methods

		private void DrawHeader(Rect rect)
		{
			EditorGUI.LabelField(rect, _header);            
		}

		private void DrawElement(Rect rect, int idx, bool isActive, bool isFocused)
		{			
			rect.height = EditorGUIUtility.singleLineHeight;

			var property = _eventsList.serializedProperty.GetArrayElementAtIndex(idx);
			EditorGUI.PropertyField(rect, property, GUIContent.none);
		}
		
		#endregion
		
	}
}

#endif