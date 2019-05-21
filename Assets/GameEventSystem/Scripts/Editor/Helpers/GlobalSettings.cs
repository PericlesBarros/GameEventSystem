using System.Collections;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace GameSystems.Events
{
	[System.Serializable]
	[InitializeOnLoad]
	public static class GlobalSettings
	{
		//=====================================================================================================================//
		//================================================= Public Properties ==================================================//
		//=====================================================================================================================//

		#region Public Properties

		static bool hasBeenLoaded = false;
		static bool drawCustomInspectors = true;

		public static bool DrawCustomInspectors
		{
			get
			{
				DeserializeData();
				return drawCustomInspectors;
			}

			set
			{
				drawCustomInspectors = value;
				SerializeData();
			}
		}

		#endregion

		//=====================================================================================================================//
		//================================================== Public Methods ===================================================//
		//=====================================================================================================================//

		#region Public Methods

		public static void SerializeData()
		{

		}

		public static void DeserializeData()
		{
			if (hasBeenLoaded)
				return;
			
			hasBeenLoaded = true;

			//var text = "";//LoadFromFile();
			//drawCustomInspectors = JsonUtility.FromJson<bool>(text);
		}

		#endregion
	}
}
#endif