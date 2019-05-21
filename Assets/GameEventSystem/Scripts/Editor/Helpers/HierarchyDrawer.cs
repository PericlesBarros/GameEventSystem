using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR

namespace GameSystems.Events.Helpers
{
	[InitializeOnLoad]
	public static class HierarchyDrawer
	{		
		//=====================================================================================================================//
		//=================================================== Private Fields ==================================================//
		//=====================================================================================================================//

		#region Private Fields
		
		private static Color _defaultColor;

		#endregion
		
		//=====================================================================================================================//
		//================================================== Private Methods ==================================================//
		//=====================================================================================================================//

		#region Private Methods
		
		static HierarchyDrawer()
		{
			var hierarchyItemCallback = new EditorApplication.HierarchyWindowItemCallback(DrawHierarchyItem);
			EditorApplication.hierarchyWindowItemOnGUI += hierarchyItemCallback;
		}

		private static void DrawHierarchyItem(int instanceID, Rect selectionRect)
		{
			if(!GlobalSettings.DrawCustomInspectors)
				return;

			_defaultColor = GUI.color;
			
			var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
			if(go == null) {
				return;
			}
			
			var rectLabel = new Rect(selectionRect.x + selectionRect.width - 67f, selectionRect.y, 67f, 16f);
			var manager = go.GetComponent<GameEventManager>();
			
			if(manager != null) {
				GUI.color = manager.enabled ? Color.green : Color.gray;
				
				GUI.Label(rectLabel,"MANAGER");
				GUI.Box(selectionRect, new GUIContent("", manager.enabled?"GameEventManager": "GameEventManager is disabled and won't work at runtime."));
				GUI.color = _defaultColor;
				return;
			}

			var listener = go.GetComponent<GameEventListener>();
			if(listener != null){
				string tooltip;
				if (!listener.enabled || listener.IsEmpty() || listener.IsMuted()) {
					GUI.color = Color.gray;
					tooltip = "GameEventListener" + (!listener.enabled ? " is disabled " : (listener.IsEmpty() ? "'s handler list is empty " : " is muted ")) + " and will not be processed.";
				} else {
					GUI.color = listener.IsValid (out tooltip) ? Color.green : Color.red;
				}
				
				GUI.Label(rectLabel,"LISTENER");
				GUI.Box(selectionRect,new GUIContent("", tooltip));
				GUI.color = _defaultColor;
				return;
			}

			var trigger = go.GetComponent<GameEventTrigger>();
			if(trigger != null){
				string tooltip;
				if (!trigger.enabled) {
					GUI.color = Color.gray;
					tooltip = "GameEventTriggerAsset is disabled and will not be proccesed.";
				} else {
					GUI.color = trigger.IsValid (out tooltip) ? Color.green : Color.red;
				}
				rectLabel = new Rect(selectionRect.x + selectionRect.width - 64f, selectionRect.y, 64f, 16f);
				GUI.Label(rectLabel, "TRIGGER");
				GUI.Box(selectionRect, new GUIContent("", tooltip));
				GUI.color = _defaultColor;
				return;
			}
			
			string label;
			string tip;
			
			var behaviour = go.GetComponent<MonoBehaviour>();
			var isListener = (behaviour as IGameEventListener) != null;
			var isTrigger = (behaviour as IGameEventTrigger) != null;

			if(!isTrigger && !isListener)
				return;
			
			if (isListener && !isTrigger) {
				rectLabel = new Rect(selectionRect.x + selectionRect.width - 72f, selectionRect.y, 72f, 16f);
				label = "ILISTENER";
				tip = "IGameEventListener";
			} else if (!isListener) {
				rectLabel = new Rect(selectionRect.x + selectionRect.width - 69f, selectionRect.y, 69f, 16f);
				label = "ITRIGGER";
				tip = "IGameEventTrigger";
			} else {
				rectLabel = new Rect(selectionRect.x + selectionRect.width - 146f, selectionRect.y, 146f, 16f);
				label = "ILISTENER & ITRIGGER";
				tip = "IGameEventListener & IGameEventTrigger";
			}

			if (!behaviour.enabled) {
				GUI.color = Color.gray;
			} else {
				GUI.color = Color.green * 0.75f + Color.blue * 0.5f;
			}
			
			GUI.Label(rectLabel, label);
			GUI.Box(selectionRect, new GUIContent("", tip));
			GUI.color = _defaultColor;
			
		}
		#endregion        
	}
}
#endif