using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace GameSystems.Events.Inspectors
{
	[CustomEditor(typeof(GameEventManager), true)]
	public class GameEventManagerInspector : Editor
	{
		//=====================================================================================================================//
		//============================================= Unity Callback Methods ================================================//
		//=====================================================================================================================//

		#region Unity Callback Methods

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if(Application.isPlaying && GUILayout.Button("Test Raise")) {
				((GameEventManager)target).TestRaise();
			}
		}

		#endregion
	}
}

#endif