using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GameSystems.Utils;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

namespace GameSystems.Events.Drawers
{
    [CustomPropertyDrawer(typeof(GameEventTargetSet))]
    public class GameEventTargetSetDrawer : PropertyDrawer
    {
        //=====================================================================================================================//
        //=================================================== Private Fields ==================================================//
        //=====================================================================================================================//

        #region Private Fields

        private readonly Dictionary<string, ReorderableList> _initializedLists = new Dictionary<string, ReorderableList>();
        
        #endregion

        //=====================================================================================================================//
        //============================================= Unity Callback Methods ================================================//
        //=====================================================================================================================//

        #region Unity Callback Methods

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!_initializedLists.ContainsKey(property.propertyPath)) {
                var serializedObject = property.serializedObject;

                var targets = property.FindPropertyRelative("targets");

                var inEditMode = !Application.isPlaying;

                var targetsList = new ReorderableList(serializedObject, targets, inEditMode, true, inEditMode, inEditMode);
                
                targetsList.onAddDropdownCallback = AddDropDownMenu;
                targetsList.elementHeightCallback = (index) => {
                    if (index >= targets.arraySize || index < 0)
                        return EditorGUIUtility.singleLineHeight;

                    var target = targets.GetArrayElementAtIndex(index);
                    if (target == null) {
                        return EditorGUIUtility.singleLineHeight;
                    }

                    var height = EditorGUIUtility.singleLineHeight * 1.35f;
                    var targetType = target.FindPropertyRelative("_targetType");
                    var isOpen = target.FindPropertyRelative("isOpen");

                    if (isOpen.boolValue) {
                        switch (targetType.enumValueIndex) {
                            case 0:  /* GameObject Target*/ {
                                var gameObjectTarget = target.FindPropertyRelative("_gameObjectTarget");
                                    var action = gameObjectTarget.FindPropertyRelative("action");

                                    if (action.enumValueIndex == 2) {
                                        height *= 4f;
                                    } else if (action.enumValueIndex >= 3) {
                                        var parameter = gameObjectTarget.FindPropertyRelative("parameter");
                                        var type = parameter.FindPropertyRelative("type");
                                        if (type.enumValueIndex == 0) {
                                            height *= 4f;
                                        } else {
                                            height *= 5f;
                                        }
                                    } else {
                                        height *= 2f;
                                    }
                                }
                                break;
                            case 1: /*Behaviour Target*/ {
                                    var behaviourTarget = target.FindPropertyRelative("_behaviourTarget");
                                    var action = behaviourTarget.FindPropertyRelative("_action");

                                    if (action.enumValueIndex == 2) {
                                        var parameter = behaviourTarget.FindPropertyRelative("_methodArg");
                                        var type = parameter.FindPropertyRelative("type");
                                        if (type.enumValueIndex == 0) {
                                            height *= 4f;
                                        } else {
                                            height *= 5f;
                                        }
                                    } else {
                                        height *= 2f;
                                    }
                                }
                                break;
                            case 2: /*Timeline Target*/ {
                                    var action = target.FindPropertyRelative("_timelineTarget").FindPropertyRelative("action");
									if(action.enumValueIndex < 4)
										height *= 2f;
									else
										height *= 3f;
                                }                                
                                break;
                            case 3: /*GameEvent Target*/{
                                    var eventTarget = target.FindPropertyRelative("_eventTarget");
                                    var action = eventTarget.FindPropertyRelative("action");
									var paramType = eventTarget.FindPropertyRelative("parameter").FindPropertyRelative("type");
									var paramHeight = height;
									if((ParameterType)paramType.enumValueIndex != ParameterType.None)
										paramHeight *= 2f;

									switch ((EventIDTarget.ActionType)action.enumValueIndex) {
										case EventIDTarget.ActionType.Abort:
											height *= 3f;
											break;
										case EventIDTarget.ActionType.Queue:
										case EventIDTarget.ActionType.Raise:
											height = height * 2f + paramHeight;
											break;
										case EventIDTarget.ActionType.Schedule:
											height = height * 3f + paramHeight;
											break;
									}
                                }
                                break;
                            case 4: /*Animator Target*/ {
                                    var animatorTarget = target.FindPropertyRelative("_animatorTarget");
                                    var action = animatorTarget.FindPropertyRelative("action");

                                    switch (action.enumValueIndex) {
                                        case 2: //SetParameter
                                            var paramType = animatorTarget.FindPropertyRelative("parameterType");
                                            if(paramType.enumValueIndex >= 1)
                                                height *= 4f;
                                            else
                                                height *= 3f;
                                            break;
                                        case 3: //Crossfade
                                            height *= 4.5f;
                                            break;
                                        case 4: //MatchTarget
                                            height *= 7f;
                                            break;
                                        case 5: //Play
                                            height *= 3.75f;
                                            break;
                                        case 6: //SetIK
                                            var multiplier = 6.75f;
                                            if (animatorTarget.FindPropertyRelative("ikPositionRef").objectReferenceValue != null) {
                                                multiplier += 1f;
                                            }
                                            if (animatorTarget.FindPropertyRelative("ikRotationRef").objectReferenceValue != null) {
                                                multiplier += 1f;
                                            }
                                            if (animatorTarget.FindPropertyRelative("lookAtRef").objectReferenceValue != null) {
                                                multiplier += 4;
                                            }
                                            
                                            height *= multiplier;
                                            break;
                                        case 7: //SetLookAt
                                            height *= 3.75f;
                                            break;
                                        default:
                                            height *= 2f;
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                    return height;
                };

                targetsList.drawElementCallback = (rect, idx, isActive, isFocused) => {
                    var target = targetsList.serializedProperty.GetArrayElementAtIndex(idx);
                    EditorGUI.PropertyField(rect, target);
                };

                targetsList.drawHeaderCallback = DrawHeader;

                _initializedLists.Add(property.propertyPath, targetsList);
            }

            EditorGUI.BeginProperty(position, label, property);
            ReorderableList list;
            if (_initializedLists.TryGetValue(property.propertyPath, out list) && list != null) {
                list.DoLayoutList();
            }
            EditorGUI.EndProperty();

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return 0f;
		}

        #endregion

        //=====================================================================================================================//
        //================================================== Private Methods ==================================================//
        //=====================================================================================================================//

        #region Private Methods

        private static void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Targets");
        }

        private static void AddDropDownMenu(Rect rect, ReorderableList list)
        {
            var menu = new GenericMenu();
            var targetTypes = (int[])Enum.GetValues(typeof(TargetType));
            foreach (var targetType in targetTypes){
                var type1 = targetType;
                menu.AddItem(new GUIContent(((TargetType)targetType).ToString()), false, () => {
                    var idx = list.serializedProperty.arraySize;
                    list.serializedProperty.arraySize++;
                    list.index = idx;

                    var element = list.serializedProperty.GetArrayElementAtIndex(idx);
                    var type = element.FindPropertyRelative("_targetType");
                    type.enumValueIndex = type1;
                    list.serializedProperty.serializedObject.ApplyModifiedProperties();
                });
            }

            menu.ShowAsContext();
        }
        
        #endregion        
    }
}

#endif