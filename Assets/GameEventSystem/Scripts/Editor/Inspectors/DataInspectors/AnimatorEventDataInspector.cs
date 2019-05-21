using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace GameSystems.Events.Inspectors
{
	[CustomEditor (typeof(AnimatorEventData), false)]
	public class AnimatorEventDataInspector : Editor
	{
		//=====================================================================================================================//
		//============================================= Unity Callback Methods ================================================//
		//=====================================================================================================================//

		#region Unity Callback Methods

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			var data = (AnimatorEventData)target;
			var type = serializedObject.FindProperty("type");
			var name = serializedObject.FindProperty("parameterName");

			EditorGUILayout.PropertyField(name);
			EditorGUILayout.PropertyField(type);

			switch(type.enumValueIndex) {
				case 0: //Trigger					
					break;
				case 1: //Integer
					EditorGUILayout.PropertyField(serializedObject.FindProperty("integerValue"));
					break;
				case 2: //Float
					EditorGUILayout.PropertyField(serializedObject.FindProperty("floatValue"));
					break;
				case 3: //Boolean
					EditorGUILayout.PropertyField(serializedObject.FindProperty("booleanValue"));
					break;
			}

			serializedObject.ApplyModifiedProperties();
		}

		#endregion		
	}
}

#endif