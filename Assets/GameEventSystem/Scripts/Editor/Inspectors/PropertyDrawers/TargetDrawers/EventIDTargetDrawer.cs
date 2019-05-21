using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace GameSystems.Events.Drawers
{
	[CustomPropertyDrawer(typeof(EventIDTarget))]
	public class EventIDTargetDrawer : PropertyDrawer
	{
		//=====================================================================================================================//
		//=================================================== Private Fields ==================================================//
		//=====================================================================================================================//

		#region Private Fields

		private SerializedObject _serializedObject;
		private SerializedProperty _actionProp;
		private SerializedProperty _eventIDProp;
		private SerializedProperty _parameterProp;

		#endregion

		//=====================================================================================================================//
		//============================================= Unity Callback Methods ================================================//
		//=====================================================================================================================//

		#region Unity Callback Methods

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			_actionProp = property.FindPropertyRelative("action");
            _eventIDProp = property.FindPropertyRelative("eventID");
			_parameterProp = property.FindPropertyRelative("parameter");
            _serializedObject = property.serializedObject;

            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
			
            EditorGUI.BeginChangeCheck();
            //Create rect
            var fieldRect = position;
            fieldRect.x += 5f;
            fieldRect.width = 140f;
            fieldRect.y += EditorGUIUtility.singleLineHeight * 1.25f + 2.5f;
            fieldRect.height = EditorGUIUtility.singleLineHeight;
            GUILayout.BeginHorizontal();
                
            //Draw action field
            EditorGUI.PropertyField(fieldRect, _actionProp, GUIContent.none);
            fieldRect.x += 145f;
            fieldRect.width = position.width - 155f;
                
            //Draw target timeline
            EditorGUI.PropertyField(fieldRect, _eventIDProp, GUIContent.none);
            GUILayout.EndHorizontal();

            fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
            fieldRect.x = position.x + 5f;

            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 145f;
            switch (_actionProp.enumValueIndex){
	            case 0:
		            fieldRect.width = position.width - 10f;
		            EditorGUI.PropertyField(fieldRect, _parameterProp);
		            break;
				case 1:
					fieldRect.width = position.width - 10f;
					EditorGUI.PropertyField(fieldRect, _parameterProp);
					break;
	            case 2: // Abort 
		            var allOfType = property.FindPropertyRelative("allOfType");
		            
		            EditorGUI.PropertyField(fieldRect, allOfType, new GUIContent("All of Type"));
		            break;
	            case 3: //Schedule
		            fieldRect.width = position.width - 10f;
		            var delay = property.FindPropertyRelative("delay");
		            EditorGUI.PropertyField(fieldRect, delay, new GUIContent("Delay"));
		            fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
		            EditorGUI.PropertyField(fieldRect, _parameterProp);
		            break;
            }

			EditorGUIUtility.labelWidth = labelWidth;

            //Apply changes
            if (EditorGUI.EndChangeCheck()) {
                _serializedObject.ApplyModifiedProperties();
            }
            
            EditorGUI.indentLevel = indentLevel;     
		}

		#endregion        
	}
}

#endif