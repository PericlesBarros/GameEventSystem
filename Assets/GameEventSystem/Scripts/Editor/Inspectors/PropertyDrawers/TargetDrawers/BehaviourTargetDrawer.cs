using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR

namespace GameSystems.Events.Drawers
{
    [CustomPropertyDrawer (typeof(BehaviourTarget))]
    public class BehaviourTargetDrawer : PropertyDrawer
    {
        //=====================================================================================================================//
        //=================================================== Private Fields ==================================================//
        //=====================================================================================================================//

        #region Private Fields

	    private static Color _defaultColor;

	    private SerializedObject _serializedObject;
	    private SerializedProperty _action;
	    private SerializedProperty _type;
	    private SerializedProperty _behaviour;
	    private SerializedProperty _cloth;
	    private SerializedProperty _lodGroup;
	    private SerializedProperty _renderer;
	    private SerializedProperty _collider;
	    private SerializedProperty _gameObject;
	    private SerializedProperty _methodName;
	    private SerializedProperty _methodIndex;
	    private SerializedProperty _methodArg;

	    private int _pickerID = -1;
        
        #endregion

        //=====================================================================================================================//
        //============================================= Unity Callback Methods ================================================//
        //=====================================================================================================================//

        #region Unity Callback Methods

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
			_defaultColor = GUI.color;
            _action = property.FindPropertyRelative("_action");
            _type = property.FindPropertyRelative("_type");
            _behaviour = property.FindPropertyRelative("_behaviour");
            _collider = property.FindPropertyRelative("_collider");
            _renderer = property.FindPropertyRelative("_renderer");
            _cloth = property.FindPropertyRelative("_cloth");
            _lodGroup = property.FindPropertyRelative("_lodGroup");
            _gameObject = property.FindPropertyRelative("_gameObject");
            _methodName = property.FindPropertyRelative("_methodName");
            _methodIndex = property.FindPropertyRelative("_methodIdx");
            _methodArg = property.FindPropertyRelative("_methodArg");

            _serializedObject = property.serializedObject;

            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            EditorGUI.BeginChangeCheck();

            //Draw Action field
            var fieldRect = position;
			fieldRect.x += 5f;
            fieldRect.width = 140f;
            fieldRect.y += EditorGUIUtility.singleLineHeight * 1.25f + 2.5f;
            fieldRect.height = EditorGUIUtility.singleLineHeight;

			if(_gameObject.objectReferenceValue == null || (_action.enumValueIndex == 2 && (_behaviour.objectReferenceValue == null || _methodIndex.intValue == -1)))
				GUI.color = Color.red;

            EditorGUI.PropertyField(fieldRect, _action, GUIContent.none);
						
            //Draw Target field
            fieldRect.x += 145f;
            fieldRect.width = position.width - 155f;
            var buttonName = "None (Behaviour)";
            Texture texture = null;

            if (_gameObject.objectReferenceValue != null) {
                buttonName = _gameObject.objectReferenceValue.name + ".";
            }
            
            SerializedProperty targetProperty = null;
            switch ((BehaviourTarget.BehaviourType)_type.enumValueIndex) {
                case BehaviourTarget.BehaviourType.Behaviour:
                    targetProperty = _behaviour;
                    break;
                case BehaviourTarget.BehaviourType.Cloth:
                    targetProperty = _cloth;
                    break;
                case BehaviourTarget.BehaviourType.Collider:
                    targetProperty = _collider;
                    break;
                case BehaviourTarget.BehaviourType.LODGroup:
                    targetProperty = _lodGroup;
                    break;
                case BehaviourTarget.BehaviourType.Renderer:
                    targetProperty = _renderer;
                    break;
            }

            if (targetProperty != null && targetProperty.objectReferenceValue != null) {
                buttonName += targetProperty.objectReferenceValue.GetType().Name;
                texture = AssetPreview.GetMiniThumbnail(targetProperty.objectReferenceValue);
            }

            GUI.SetNextControlName("Target Field");
            GUI.Box(fieldRect, new GUIContent(buttonName, texture), GUI.skin.FindStyle("objectField"));

			GUI.color = _defaultColor;

            var pickerRect = fieldRect;
            pickerRect.x = fieldRect.xMax - 15f;
            pickerRect.width = 15f;

            fieldRect.xMax -= 15f;

            var evt = Event.current;
            if (pickerRect.Contains(evt.mousePosition)) {
                GUI.FocusControl("Target Field");
                switch (evt.type) {
                    case EventType.MouseDown:
                        if (evt.button == 0) {
                            _pickerID = GUIUtility.GetControlID(FocusType.Passive) + 100;
                            var filter = (BehaviourTarget.ActionType)_action.enumValueIndex == BehaviourTarget.ActionType.InvokeMethod ? "t:MonoBehaviour": "t:Behaviour t:Collider t:Cloth t:Renderer t:LODGroup";
                            EditorGUIUtility.ShowObjectPicker<GameObject>(null, true, filter , _pickerID);
                            evt.Use();
                        }
                        break;
                }
            }
            if (Event.current.type == EventType.Layout) {
                if (Event.current.commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == _pickerID) {
                    _pickerID = -1;
                    GUI.FocusControl("Target Field");
                    var selectedObj = (GameObject)EditorGUIUtility.GetObjectPickerObject();
                    if (selectedObj != null) {
                        var rect = position;
                        rect.position += fieldRect.position;
                        rect.x -= fieldRect.width / 4f;
                        rect.y += fieldRect.height * 1.5f;
                        DisplayBehaviours(selectedObj, rect);
                        Event.current.Use();
                        _serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            evt = Event.current;
            if (fieldRect.Contains(evt.mousePosition)) {
                GUI.FocusControl("Target Field");
                switch (evt.type) {
					case EventType.KeyDown:
						if (evt.keyCode == KeyCode.Delete || evt.keyCode == KeyCode.Backspace) {
							_gameObject.objectReferenceValue = null;                            
							GUI.changed = true;
                            _serializedObject.ApplyModifiedProperties();
                            evt.Use();
						}
						break;						
					case EventType.MouseDown:
						if (evt.button == 0) {
							if (_gameObject.objectReferenceValue != null) {
								EditorGUIUtility.PingObject(_gameObject.objectReferenceValue.GetInstanceID());
								evt.Use();
							}
						}
						break;
					case EventType.ContextClick:
						if (_gameObject.objectReferenceValue != null) {
							EditorGUIUtility.PingObject(_gameObject.objectReferenceValue.GetInstanceID());
							DisplayBehaviours((GameObject)_gameObject.objectReferenceValue);
                            evt.Use();
                        }
						break;
					case EventType.DragUpdated:
					case EventType.DragPerform:                        
						DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

						if (evt.type == EventType.DragPerform && DragAndDrop.objectReferences.Length == 1 && DragAndDrop.objectReferences[0] != null && GUI.enabled) {
							var b = DragAndDrop.objectReferences[0] as Behaviour;
						    var go = DragAndDrop.objectReferences[0] as GameObject;
						    var cl = DragAndDrop.objectReferences[0] as Cloth;
						    var col = DragAndDrop.objectReferences[0] as Collider;
						    var lod = DragAndDrop.objectReferences[0] as LODGroup;
							var rend = DragAndDrop.objectReferences[0] as Renderer;

							if (b != null) {
								SetComponent(b, b.gameObject);
							}else if (cl != null) {
								SetComponent(cl, cl.gameObject);
							}else if (col != null) {
								SetComponent(col, col.gameObject);
							}else if (lod != null) {
								SetComponent(lod, lod.gameObject);
							}else if (rend != null) {
								SetComponent(rend, rend.gameObject);
							} else {
								DisplayBehaviours(go);	
							}
							
                            DragAndDrop.AcceptDrag();
                            evt.Use();
                        }
						break;
				}
            }
            
			if((BehaviourTarget.ActionType)_action.enumValueIndex == BehaviourTarget.ActionType.InvokeMethod) {
				var methodRect = position;
				methodRect.y = fieldRect.y + EditorGUIUtility.singleLineHeight + 2.5f;
                methodRect.height = EditorGUIUtility.singleLineHeight;
                methodRect.width = position.width - 10f;
				ResolveMethods(methodRect);
			}
			
            //Apply changes
            if (EditorGUI.EndChangeCheck()) {
                _serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.indentLevel = indentLevel;
			GUI.color = _defaultColor;
        }

        #endregion
				
        //=====================================================================================================================//
        //================================================== Private Methods ==================================================//
        //=====================================================================================================================//

        #region Private Methods

	    private void ResolveMethods(Rect rect)
		{
			if(_behaviour.objectReferenceValue == null || (_behaviour.objectReferenceValue as MonoBehaviour) == null) {				
                rect.y += EditorGUIUtility.singleLineHeight / 2f;
				EditorGUI.LabelField(rect, "-- Missing Behaviour --", EditorStyles.centeredGreyMiniLabel);
				return;
			}
			
            var candidateMethods = FetchCandidateMethods((MonoBehaviour)_behaviour.objectReferenceValue);
            if(candidateMethods.Length == 0) {
                rect.y += EditorGUIUtility.singleLineHeight / 2f;
                _methodIndex.intValue = -1;
                _methodName.stringValue = string.Empty;
                EditorGUI.LabelField(rect, "-- Only methods marked [GameEventHandler] are accepted --", EditorStyles.centeredGreyMiniLabel);
            } else {
                rect.x += 5f;
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 145f;
                _methodIndex.intValue = Mathf.Clamp(_methodIndex.intValue, 0, candidateMethods.Length);
                _methodIndex.intValue = EditorGUI.Popup(rect, "Method",  _methodIndex.intValue, candidateMethods);
                _methodName.stringValue = candidateMethods[_methodIndex.intValue];
                rect.y += EditorGUIUtility.singleLineHeight + 2.5f;
                EditorGUI.PropertyField(rect, _methodArg);
                EditorGUIUtility.labelWidth = labelWidth;
            }
		}

	    private void DisplayBehaviours(GameObject go, Rect position = default(Rect))
        {            
            Component[] components;
            if ((BehaviourTarget.ActionType)_action.enumValueIndex == BehaviourTarget.ActionType.InvokeMethod) {                
                components = go.GetComponents<MonoBehaviour>() as MonoBehaviour[];
            } else {
                var comps = new List<Component>(go.GetComponents<Behaviour>());
                comps.AddRange(go.GetComponents<Collider>());
                comps.AddRange(go.GetComponents<Cloth>());
                comps.AddRange(go.GetComponents<LODGroup>());
                comps.AddRange(go.GetComponents<Renderer>());
                components = comps.ToArray();
            }
            
            var behaviourMenu = new GenericMenu();            
            if (components.Length == 1) {
                SetComponent(components[0], go);
            } else {
                for (var i = 0; i < components.Length; i++) {
                    var comp = components[i];
                    if (comp == null)
                        continue;

                    var content = new GUIContent(comp.GetType().Name, AssetPreview.GetMiniThumbnail(comp));
                    behaviourMenu.AddItem(content, false, () => {
                        SetComponent(comp, go);                        
                    });
                }
                if (position == default(Rect))
                    behaviourMenu.ShowAsContext();
                else {
                    position.y += behaviourMenu.GetItemCount() * EditorGUIUtility.singleLineHeight;
                    behaviourMenu.DropDown(position);
                }
            }
        }

	    private void SetComponent(Component comp, GameObject go)
        {
            if (comp == null)
                return;

            Undo.RecordObject(_serializedObject.targetObject, "Set BehaviourTarget");
            _gameObject.objectReferenceValue = go;
            var type = comp.GetType();
            if ((typeof(Collider)).IsAssignableFrom(type)) {
                _type.enumValueIndex = (int)BehaviourTarget.BehaviourType.Collider;
                _collider.objectReferenceValue = comp;
            } else if ((typeof(Cloth)).IsAssignableFrom(type)) {
                _type.enumValueIndex = (int)BehaviourTarget.BehaviourType.Cloth;
                _cloth.objectReferenceValue = comp;
            } else if ((typeof(LODGroup)).IsAssignableFrom(type)) {
                _type.enumValueIndex = (int)BehaviourTarget.BehaviourType.LODGroup;
                _lodGroup.objectReferenceValue = comp;
            } else if ((typeof(Renderer)).IsAssignableFrom(type)) {
                _type.enumValueIndex = (int)BehaviourTarget.BehaviourType.Renderer;
                _renderer.objectReferenceValue = comp;
            } else {
                _type.enumValueIndex = (int)BehaviourTarget.BehaviourType.Behaviour;
                _behaviour.objectReferenceValue = comp;
            }

            GUI.changed = true;
            _serializedObject.ApplyModifiedProperties();
        }

	    private string[] FetchCandidateMethods(MonoBehaviour script)
        {
            if (script == null)
                return new string[0];

            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
            var methods = new List<string>();
            var methodInfo = script.GetType().GetMethods(flags).Where(method => HasValidAttribute(method));
            foreach (var info in methodInfo) {
                methods.Add(info.Name);
            }

            return methods.ToArray();
        }

	    private bool HasValidAttribute(MethodInfo info)
        {
            var attrbs = info.GetCustomAttributes(typeof(GameEventHandlerAttribute), true);
            return attrbs.Length != 0;
        }
        

        #endregion
    }
}

#endif