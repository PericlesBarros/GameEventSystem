using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

namespace GameSystems.Events.Drawers
{
	[CustomPropertyDrawer(typeof(GameObjectTarget))]
	public class GameObjectTargetDrawer : PropertyDrawer
	{
		//=====================================================================================================================//
		//=================================================== Private Fields ==================================================//
		//=====================================================================================================================//

		#region Private Fields

		static Color defaultColor;

		SerializedObject serializedObject;
        SerializedProperty action;
		SerializedProperty gameObject;
		SerializedProperty cachedListeners;
		SerializedProperty methodName;
		SerializedProperty parameter;

		SerializedProperty broadcastTarget;

		#endregion

		//=====================================================================================================================//
		//============================================= Unity Callback Methods ================================================//
		//=====================================================================================================================//

		#region Unity Callback Methods

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			defaultColor = GUI.color;

			action = property.FindPropertyRelative("action");
			gameObject = property.FindPropertyRelative("gameObject");
			broadcastTarget = property.FindPropertyRelative("broadcastTarget");
			cachedListeners = property.FindPropertyRelative("_cachedListenersView");
			methodName = property.FindPropertyRelative("_methodName");
			parameter = property.FindPropertyRelative("parameter");
			serializedObject = property.serializedObject;

			int indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			//TODO: find a better alternative to disabled button                
			//TODO: change background of selected field                

			EditorGUI.BeginChangeCheck();
			//Create rect
			var fieldRect = position;
			fieldRect.width = 140f;
			fieldRect.x += 5f;
			fieldRect.y += EditorGUIUtility.singleLineHeight * 1.25f + 2.5f;
			fieldRect.height = EditorGUIUtility.singleLineHeight;
			GUILayout.BeginHorizontal();
			if(gameObject.objectReferenceValue == null)
				GUI.color = Color.red;
			//Draw action field
			EditorGUI.PropertyField(fieldRect, action, GUIContent.none);
			fieldRect.x += 145f;
			fieldRect.width = position.width - 155f;

			//Draw target timeline			
			EditorGUI.PropertyField(fieldRect, gameObject, GUIContent.none);
			GUI.color = defaultColor;
			GUILayout.EndHorizontal();

			fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
			fieldRect.x = position.x + 5f;

			var labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 145f;

			fieldRect.x = position.x + 5f;
			fieldRect.width = position.width - 10f;

			switch ((GameObjectTarget.ActionType)action.enumValueIndex) {
				case GameObjectTarget.ActionType.Activate:
				case GameObjectTarget.ActionType.Deactivate:
					break;
				case GameObjectTarget.ActionType.BroadcastToListeners:
					InspectListeners(fieldRect);
					fieldRect.y += EditorGUIUtility.singleLineHeight + 1f;
					EditorGUI.PropertyField(fieldRect, broadcastTarget);
					break;
				case GameObjectTarget.ActionType.BroadcastMessage:
				case GameObjectTarget.ActionType.SendMessage:
					if(string.IsNullOrEmpty(methodName.stringValue))
						GUI.color = Color.red;
					EditorGUI.PropertyField(fieldRect, methodName, new GUIContent("Target Method"));

					GUI.color = defaultColor;

					fieldRect.y += EditorGUIUtility.singleLineHeight + 1.25f;
					EditorGUI.PropertyField(fieldRect, parameter, true);
					break;
			}

			EditorGUIUtility.labelWidth = labelWidth;

			//Apply changes
			if (EditorGUI.EndChangeCheck()) {
				serializedObject.ApplyModifiedProperties();
			}

			EditorGUI.indentLevel = indentLevel;
			GUI.color = defaultColor;
		}

		#endregion

		//=====================================================================================================================//
		//================================================== Private Methods ==================================================//
		//=====================================================================================================================//

		#region Private Methods

		void InspectListeners(Rect position)
		{
			if (gameObject.objectReferenceValue == null) {
				GUI.color = Color.red;
				EditorGUI.LabelField(position, "--- MISSING GAME OBJECT ---", EditorStyles.centeredGreyMiniLabel);
				GUI.color = defaultColor;
				return;
			}

			var text = "";
			if (cachedListeners != null && cachedListeners.arraySize > 0) {
				text = "Broadcasting to [" + cachedListeners.arraySize + "] listeners in [" + gameObject.objectReferenceValue.name + "] " + broadcastTarget.enumDisplayNames[broadcastTarget.enumValueIndex];
			} else {
				GUI.color = new Color(0.85f, 0f, 0f, 1f);
				text = "NO AVAILABLE LISTENERS ON ";
				if (broadcastTarget.enumValueIndex == 0)
					text += "[" + gameObject.objectReferenceValue.name + "]";
				if (broadcastTarget.enumValueIndex == 1)
					text += "[" + gameObject.objectReferenceValue.name + "] OR ANY OF ITS CHILDREN.";
				
				if(broadcastTarget.enumValueIndex == 2)
					text += "ANY OF [" + gameObject.objectReferenceValue.name + "]'S CHILDREN.";
			}

			EditorGUI.LabelField(position, new GUIContent(text, text), EditorStyles.centeredGreyMiniLabel);
			GUI.color = defaultColor;
		}

		#endregion
	}
}

#endif