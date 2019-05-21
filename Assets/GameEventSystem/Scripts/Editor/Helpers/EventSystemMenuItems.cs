
using GameSystems.Events;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace GameSystems.AssetMenuItems
{
	public static partial class MenuItems 
	{
		[MenuItem("GameObject/GameEvent System/GameEventManager", false, 1)]
		public static void CreateEventManager()
		{			
			var manager = Object.FindObjectOfType<GameEventManager>();
			if(manager == null)
				manager = new GameObject("GameEventManager").AddComponent<GameEventManager>();
			
			UpdateSelection(manager.GetInstanceID());
		}

		[MenuItem("GameObject/GameEvent System/GameEventListener", false, 2)]
		public static void CreateEventListener()
		{
			UpdateSelection(new GameObject("New GameEventListener").AddComponent<GameEventListener>().GetInstanceID());
		}

		[MenuItem("GameObject/GameEvent System/GameEventTrigger", false, 3)]
		public static void GameEventTrigger()
		{
			UpdateSelection(new GameObject("New GameEventTrigger").AddComponent<GameEventTrigger>().GetInstanceID());
		}

		static void UpdateSelection(int instanceID)
		{
			Selection.activeInstanceID = instanceID;
		}

		[MenuItem("Tools/GameSystems/EventSystem/Toggle Custom Editor %&T")]
		public static void ToggleCustomEditor()
		{
			GlobalSettings.DrawCustomInspectors = !GlobalSettings.DrawCustomInspectors;			
		}
	}
}

#endif