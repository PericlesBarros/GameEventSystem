using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;

namespace GameSystems.Events.Drawers
{
    [CustomPropertyDrawer(typeof(TimelineTarget))]
	public class TimelineTargetDrawer : PropertyDrawer
	{
        //=====================================================================================================================//
        //=================================================== Private Fields ==================================================//
        //=====================================================================================================================//

        #region Private Fields

        SerializedObject serializedObject;
        SerializedProperty action;
        SerializedProperty timeline;

		static Color defaultColor;

        #endregion

        //=====================================================================================================================//
        //============================================= Unity Callback Methods ================================================//
        //=====================================================================================================================//

        #region Unity Callback Methods

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
			defaultColor = GUI.color;

            action = property.FindPropertyRelative("action");
            timeline = property.FindPropertyRelative("timeline");
            serializedObject = property.serializedObject;

            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginChangeCheck();
            //Create rect
            var fieldRect = position;
            fieldRect.x += 5f;
            fieldRect.width = 140f;
            fieldRect.y += EditorGUIUtility.singleLineHeight * 1.25f + 2.5f;
            fieldRect.height = EditorGUIUtility.singleLineHeight;
			
            //Draw action field
            EditorGUI.PropertyField(fieldRect, action, GUIContent.none);
            fieldRect.x += 145f;
            fieldRect.width = position.width - 155f;

			if (timeline.objectReferenceValue == null || ((PlayableDirector)timeline.objectReferenceValue).playableAsset == null)
				GUI.color = Color.red;
			
            //Draw target timeline
            EditorGUI.PropertyField(fieldRect, timeline, GUIContent.none);
            
			GUI.color = defaultColor;

			if(action.enumValueIndex == 4) {
				var time = property.FindPropertyRelative("time");
				fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
				fieldRect.x = position.x + 5f;
				fieldRect.width = position.width - 10f;
				var labelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 145f;
				EditorGUI.PropertyField(fieldRect, time, new GUIContent("Time "));
                if(time.floatValue < 0)
				    time.floatValue = 0f;
				EditorGUIUtility.labelWidth = labelWidth;
			}

            //Apply changes
            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
            
            EditorGUI.indentLevel = indentLevel;    
			GUI.color = defaultColor;        
        }

        #endregion
    }
}

#endif