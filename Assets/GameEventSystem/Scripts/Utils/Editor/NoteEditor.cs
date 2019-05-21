//=============================================================================================================================//
//
//	Project: GameEventSystem
//	Copyright: DefaultCompany
//  Created on: 3/23/2019 12:57:41 AM
//
//=============================================================================================================================//


#region Usings

using UnityEditor;
using UnityEngine;

#endregion

namespace GameEventSystem.Utils
{
	[CustomEditor(typeof(Note))]
	[CanEditMultipleObjects]
	public class NoteEditor : Editor
	{
		//=====================================================================================================================//
		//=================================================== Private Fields ==================================================//
		//=====================================================================================================================//

		#region Private Fields

		private SerializedProperty _noteProp;
		private SerializedProperty _editableProp;

		#endregion

		//=====================================================================================================================//
		//============================================= Unity Callback Methods ================================================//
		//=====================================================================================================================//

		#region Unity Callback Methods

		private void OnEnable()
		{
			_noteProp = serializedObject.FindProperty("_note");
			_editableProp = serializedObject.FindProperty("_editable");
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			
			var wasEnabled = GUI.enabled;
			GUI.enabled = _editableProp.boolValue;

			_noteProp.stringValue = EditorGUILayout.TextArea(_noteProp.stringValue, GUILayout.MaxHeight(150));

			GUI.enabled = true;
			
			if (GUILayout.Button(_editableProp.boolValue ? "Lock" : "Edit", GUI.skin.FindStyle("ToolbarButton"))) {
				_editableProp.boolValue = !_editableProp.boolValue;
			}

			GUI.enabled = wasEnabled;
			serializedObject.ApplyModifiedProperties();
		}

		#endregion
	}
}