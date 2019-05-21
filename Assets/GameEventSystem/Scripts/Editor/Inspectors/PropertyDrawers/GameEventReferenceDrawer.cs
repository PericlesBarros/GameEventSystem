using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace GameSystems.Events.Drawers
{
	[CustomPropertyDrawer(typeof(GameEventReference))]
	public class GameEventReferenceDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position.height = EditorGUIUtility.singleLineHeight;

			EditorGUI.BeginProperty(position, GUIContent.none, property);

			var type = property.FindPropertyRelative("type");
			var gameEvent = property.FindPropertyRelative("gameEvent");
			var eventID = property.FindPropertyRelative("eventID");
			
			EditorGUILayout.BeginHorizontal();
			position.y += 2f;
			if (label != null && !string.IsNullOrEmpty(label.text)) {				
				var labelWidth = EditorGUIUtility.labelWidth;				
				EditorGUIUtility.labelWidth = 50;
				GUILayout.Label(label);
				position.x += EditorGUIUtility.labelWidth;
				position.width -= EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = labelWidth;
			}

			GUI.enabled = !Application.isPlaying;

			var typeRect = position;
			typeRect.width = EditorGUIUtility.labelWidth - 10;
			EditorGUI.PropertyField(typeRect, type, GUIContent.none);

			var eventRect = position;
			eventRect.x += typeRect.width - 5;
			eventRect.width = position.width - typeRect.width;
			switch (type.enumValueIndex) {
				case 0:
					EditorGUI.PropertyField(eventRect, gameEvent, GUIContent.none);
					break;
				case 1:
					EditorGUI.PropertyField(eventRect, eventID, GUIContent.none);
					break;
			}

			GUI.enabled = true;

			EditorGUILayout.EndHorizontal();
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return 0f;
		}
	}
}

#endif