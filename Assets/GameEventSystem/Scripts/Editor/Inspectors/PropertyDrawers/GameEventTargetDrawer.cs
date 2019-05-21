using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using System;
using GameSystems.Utils;
using UnityEditor.Animations;
using Debug = System.Diagnostics.Debug;
#if UNITY_EDITOR
using UnityEditor;

namespace GameSystems.Events.Drawers
{
	[CustomPropertyDrawer(typeof(GameEventTarget))]
	public class GameEventTargetDrawer : PropertyDrawer
	{
		//=====================================================================================================================//
		//=================================================== Private Fields ==================================================//
		//=====================================================================================================================//

		#region Private Fields

		private static string[] _gameObjectActions = { "Activate", "Deactivate", "BroadcastToListeners", "SendMessage", "BroadcastMessage" };
		private static string[] _behaviourActions = { "Enable", "Disable", "InvokeMethod" };
		private static string[] _timelineActions = { "Play", "Pause", "Stop", "Resume", "SkipTo" };
		private static string[] _gameEventActions = { "Raise", "Queue", "Abort", "Schedule" };
		private static string[] _animatorActions = { "Enable", "Disable", "SetParameter" };
		private static Color _defaultColor;

		#endregion

		//=====================================================================================================================//
		//============================================= Unity Callback Methods ================================================//
		//=====================================================================================================================//

		#region Unity Callback Methods

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			_defaultColor = GUI.color;
			_gameObjectActions = Enum.GetNames(typeof(GameObjectTarget.ActionType));
			_behaviourActions = Enum.GetNames(typeof(BehaviourTarget.ActionType));
			_timelineActions = Enum.GetNames(typeof(TimelineTarget.ActionType));
			_gameEventActions = Enum.GetNames(typeof(EventIDTarget.ActionType));
			_animatorActions = Enum.GetNames(typeof(AnimatorTarget.ActionType));

			var indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			var targetType = property.FindPropertyRelative("_targetType");
			var isOpen = property.FindPropertyRelative("isOpen");
			var isMuted = property.FindPropertyRelative("_isMuted");

			if (Application.isPlaying) {
				isOpen.boolValue = false;
			}

			GUI.enabled = !isMuted.boolValue;

			var type = (TargetType)targetType.enumValueIndex;
			SerializedProperty target = null;
			switch (type) {
				case TargetType.Animator:
					target = property.FindPropertyRelative("_animatorTarget");
					break;
				case TargetType.Behaviour:
					target = property.FindPropertyRelative("_behaviourTarget");
					break;
				case TargetType.Event:
					target = property.FindPropertyRelative("_eventTarget");
					break;
				case TargetType.GameObject:
					target = property.FindPropertyRelative("_gameObjectTarget");
					break;
				case TargetType.Timeline:
					target = property.FindPropertyRelative("_timelineTarget");
					break;
			}

			var buttonRect = position;
			if (isOpen.boolValue) {
				var targetLabel = type.ToString() + " Target";
				buttonRect.height = EditorGUIUtility.singleLineHeight;
				if (GUI.Button(buttonRect, targetLabel, GUI.skin.FindStyle("ToolbarButton"))) {
					isOpen.boolValue = false;
				} else {
					EditorGUI.PropertyField(position, target, new GUIContent("open"), true);
				}
			} else {
				GUILayout.BeginHorizontal();
				buttonRect.width = 20f;
				buttonRect.height = EditorGUIUtility.singleLineHeight * 1.25f;

				if (!Application.isPlaying) {
					if (GUI.Button(buttonRect, type.ToString()[0].ToString().ToUpper(), GUI.skin.FindStyle("ToolbarButton"))) {
						isOpen.boolValue = true;
						return;
					}
				} else {
					GUI.enabled = true;
					if (GUI.Button(buttonRect, new GUIContent(isMuted.boolValue ? "U" : "M", isMuted.boolValue ? "Unmute" : "Mute"), GUI.skin.FindStyle("ToolbarButton"))) {
						isMuted.boolValue = !isMuted.boolValue;
					}
					GUI.enabled = !isMuted.boolValue;
				}

				var labelRect = position;
				labelRect.x += 25;
				labelRect.width -= 25f;
				labelRect.height = EditorGUIUtility.singleLineHeight * 1.25f;
				var targetLabel = "";
				if (!GetLabel(type, property, out targetLabel))
					GUI.color = Color.yellow;

				EditorGUI.LabelField(labelRect, new GUIContent(targetLabel, targetLabel));
				GUILayout.EndHorizontal();
			}
			GUI.enabled = true;

			EditorGUI.indentLevel = indentLevel;
			GUI.color = _defaultColor;
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

		private static bool GetLabel(TargetType type, SerializedProperty property, out string label)
		{
			label = "";
			var targetIsValid = true;
			switch (type) {
				case TargetType.Animator:
					{
						var actionProp = property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("action");
						var animatorProp = property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("animator");
						if (animatorProp.objectReferenceValue == null) {
							label = "[ERROR] Missing Animator";
							targetIsValid = false;
						} else if (((Animator)animatorProp.objectReferenceValue).runtimeAnimatorController == null) {
							label = "[ERROR] Missing AnimatorController in Animator";
							targetIsValid = false;
						} else {
							label = _animatorActions[actionProp.enumValueIndex] + " ";
							var actionEnum = (AnimatorTarget.ActionType)(actionProp.enumValueIndex); 
							switch (actionEnum) {
								case AnimatorTarget.ActionType.SetParameter:
									var parameterNameProp = property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("parameterName");
									var parameterTypeProp = property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("parameterType");
									var action = "";

									if (string.IsNullOrEmpty(parameterNameProp.stringValue))
										targetIsValid = false;

									var parameter = (string.IsNullOrEmpty(parameterNameProp.stringValue) ? "--MISSING--" : parameterNameProp.stringValue);
									var value = "";
									switch ((AnimatorEventData.ParameterType)parameterTypeProp.enumValueIndex) {
										case AnimatorEventData.ParameterType.Boolean:
											action = "SetBool";
											value = property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("booleanValue").boolValue.ToString();
											break;
										case AnimatorEventData.ParameterType.Float:
											action = "SetFloat";
											value = property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("floatValue").floatValue.ToString();
											break;
										case AnimatorEventData.ParameterType.Integer:
											action = "SetInteger";
											value = property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("integerValue").intValue.ToString();
											break;
										case AnimatorEventData.ParameterType.Trigger:
											action = "SetTrigger";
											value = "ON";
											break;
									}
									label = string.Format("{0} [{1}] to [{2}] on ", action, parameter, value);
									break;
								case AnimatorTarget.ActionType.CrossFade:
									{
										var targetState = property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("targetStateName").stringValue;
										var transitionDuration = property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("transitionDuration").floatValue;
										label = string.Format("Crossfade to [{0}] over [{1}] seconds on ", targetState, transitionDuration);
									}
									break;
								case AnimatorTarget.ActionType.MatchTarget:
									{
										var matchTarget = property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("matchTarget").objectReferenceValue;
										if (matchTarget == null) {
											label = "[ERROR] Missing MatchTarget reference transform";
											targetIsValid = false;
										} else {
											var avatarTarget = (AvatarTarget)property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("avatarTarget").intValue;
											label = string.Format("Match [{0}] to [{1}] on ", avatarTarget, matchTarget.name);
										}
									}
									break;
								case AnimatorTarget.ActionType.Play:
									{
										var targetState = property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("targetStateName").stringValue;
										var targetStateOffset = property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("targetStateOffset").floatValue;
										label = string.Format("Play [{0}] on ", targetState, targetStateOffset);
									}
									break;
								case AnimatorTarget.ActionType.SetIK:
									{
										var ikPositionRef = property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("ikPositionRef").objectReferenceValue;
										var ikRotationRef = property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("ikRotationRef").objectReferenceValue;
										var lookAtRef = property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("lookAtRef").objectReferenceValue;
										if (ikPositionRef == null && ikRotationRef == null && lookAtRef == null) {
											targetIsValid = false;
											label = "[ERROR] At least one Ik reference transform needs to be setup.";
										} else {
											var avatarIkGoal = (AvatarIKGoal)property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("avatarIkGoal").intValue;
											label = string.Empty;
											if(ikPositionRef != null || ikRotationRef != null) {
												label = string.Format("SetIK [{0}] IK's ", avatarIkGoal);
												if (ikPositionRef != null)
													label += " | Position ";
												if(ikRotationRef != null)
													label += " | Rotation ";

												label += "| ";
											}
											
											if(lookAtRef != null) {
												if (string.IsNullOrEmpty(label))
													label = "Set Look At Position on";
												else {
													label += "LookAt Position | on ";
												}
											}
										}
									}
									break;
								case AnimatorTarget.ActionType.SetLayerWeight:
									{
										var targetLayer = property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("targetLayerName").stringValue;
										var layerWeight = property.FindPropertyRelative("_animatorTarget").FindPropertyRelative("layerWeight").floatValue;
										if(string.IsNullOrEmpty(targetLayer))
											label = "[ERROR] Target Animator only has Base Layer";
										else
											label = string.Format("Set [{0}] layer weight to [{1}] on ", targetLayer, layerWeight);
									}
									break;
							}
							
							if(targetIsValid)
								label += "[" + animatorProp.objectReferenceValue.name + " (Animator)]";
						}
					}
					break;
				case TargetType.Behaviour:
					{
						var behaviourTargetProp = property.FindPropertyRelative("_behaviourTarget");
						var actionProp = behaviourTargetProp.FindPropertyRelative("_action");
						var behaviourProp = behaviourTargetProp.FindPropertyRelative("_behaviour");
						var gameObjectProp = behaviourTargetProp.FindPropertyRelative("_gameObject");
						var clothProp = behaviourTargetProp.FindPropertyRelative("_cloth");
						var colliderProp = behaviourTargetProp.FindPropertyRelative("_collider");
						var lodGroupProp = behaviourTargetProp.FindPropertyRelative("_lodGroup");
						var rendererProp = behaviourTargetProp.FindPropertyRelative("_renderer");
						var typeProp = behaviourTargetProp.FindPropertyRelative("_type");

						var behaviourIsValid = true;
						if (gameObjectProp.objectReferenceValue == null && colliderProp.objectReferenceValue == null &&
							behaviourProp.objectReferenceValue == null && clothProp.objectReferenceValue == null &&
							rendererProp.objectReferenceValue == null && lodGroupProp.objectReferenceValue == null) {
							behaviourIsValid = false;
							targetIsValid = false;
						}

						if (!behaviourIsValid) {
							label = _behaviourActions[actionProp.enumValueIndex] + "[--MISSING COMPONENT--]";
						} else {
							if (actionProp.enumValueIndex < 2) {
								Debug.Assert(gameObjectProp.objectReferenceValue != null, "gameObject.objectReferenceValue != null");
								label = _behaviourActions[actionProp.enumValueIndex] + " [" + gameObjectProp.objectReferenceValue.name;
								switch ((BehaviourTarget.BehaviourType)typeProp.enumValueIndex) {
									case BehaviourTarget.BehaviourType.Behaviour:
										Debug.Assert(behaviourProp.objectReferenceValue != null, "behaviour.objectReferenceValue != null");
										label += " (" + behaviourProp.objectReferenceValue.GetType().Name + ")]";
										break;
									case BehaviourTarget.BehaviourType.Cloth:
										Debug.Assert(clothProp.objectReferenceValue != null, "cloth.objectReferenceValue != null");
										label += " (" + clothProp.objectReferenceValue.GetType().Name + ")]";
										break;
									case BehaviourTarget.BehaviourType.Collider:
										Debug.Assert(colliderProp.objectReferenceValue != null, "collider.objectReferenceValue != null");
										label += " (" + colliderProp.objectReferenceValue.GetType().Name + ")]";
										break;
									case BehaviourTarget.BehaviourType.LODGroup:
										label += " (" + lodGroupProp.objectReferenceValue.GetType().Name + ")]";
										break;
									case BehaviourTarget.BehaviourType.Renderer:
										Debug.Assert(rendererProp.objectReferenceValue != null, "renderer.objectReferenceValue != null");
										label += " (" + rendererProp.objectReferenceValue.GetType().Name + ")]";
										break;
								}
							} else {
								var methodNameProp = behaviourTargetProp.FindPropertyRelative("_methodName");
								if (string.IsNullOrEmpty(methodNameProp.stringValue) || behaviourProp.objectReferenceValue == null)
									targetIsValid = false;

								label = "Invoke " + " [" + (string.IsNullOrEmpty(methodNameProp.stringValue) ? "--MISSING METHOD NAME--" : methodNameProp.stringValue) + "] on [" + (behaviourProp.objectReferenceValue == null ? "--MISSING BEHAVIOUR--" : behaviourProp.objectReferenceValue.name) + " (" + behaviourProp.objectReferenceValue.GetType().Name + ")]";

								var methodArgProp = property.FindPropertyRelative("_behaviourTarget").FindPropertyRelative("_methodArg");
								switch ((ParameterType)methodArgProp.FindPropertyRelative("type").enumValueIndex) {
									case ParameterType.GameObject:
										if (methodArgProp.FindPropertyRelative("gameObjectValue").objectReferenceValue == null)
											targetIsValid = false;
										break;
									case ParameterType.Object:
										if (methodArgProp.FindPropertyRelative("objectValue").objectReferenceValue == null)
											targetIsValid = false;
										break;
									case ParameterType.Transform:
										if (methodArgProp.FindPropertyRelative("transformValue").objectReferenceValue == null)
											targetIsValid = false;
										break;
									case ParameterType.String:
										if (string.IsNullOrEmpty(methodArgProp.FindPropertyRelative("stringValue").stringValue))
											targetIsValid = false;
										break;
								}
							}
						}
					}
					break;
				case TargetType.Event:
					{
						var actionProp = property.FindPropertyRelative("_eventTarget").FindPropertyRelative("action");
						var eventIDProp = property.FindPropertyRelative("_eventTarget").FindPropertyRelative("eventID");

						if (string.IsNullOrEmpty(eventIDProp.stringValue)) {
							label = "[ERROR] Missing EventID";
							targetIsValid = false;
						} else {
							label = _gameEventActions[actionProp.enumValueIndex] + " eventID [" + eventIDProp.stringValue + "]";
							if (actionProp.enumValueIndex == 3) {
								var delayProp = property.FindPropertyRelative("_eventTarget").FindPropertyRelative("delay");
								label += " with [" + delayProp.floatValue + "] second" + (delayProp.floatValue == 1 ? "" : "s") + " delay and ";
								var paramProp = property.FindPropertyRelative("_eventTarget").FindPropertyRelative("parameter");
								var paramTypeProp = paramProp.FindPropertyRelative("type");
								switch ((ParameterType)paramTypeProp.enumValueIndex) {
									case ParameterType.None:
										label += "no data.";
										break;
									case ParameterType.Integer:
										label += "value [" + paramProp.FindPropertyRelative("integerValue").intValue + " (int)]";
										break;
									case ParameterType.Float:
										label += "value [" + paramProp.FindPropertyRelative("floatValue").floatValue + " (float)]";
										break;
									case ParameterType.Boolean:
										label += "value [" + paramProp.FindPropertyRelative("booleanValue").boolValue + " (bool)]";
										break;
									case ParameterType.String:
										label += "value [" + paramProp.FindPropertyRelative("stringValue").stringValue + " (string)]";
										break;
									case ParameterType.Transform:
										var xformProp = paramProp.FindPropertyRelative("transformValue").objectReferenceValue;
										label += "value [" + ((xformProp == null) ? "--MISSING--" : xformProp.name) + " (Transform)]";
										break;
									case ParameterType.GameObject:
										var go = paramProp.FindPropertyRelative("gameObjectValue").objectReferenceValue;
										label += "value [" + ((go == null) ? "--MISSING--" : go.name) + " (GameObject)]";
										break;
									case ParameterType.Color:
										label += "value [" + paramProp.FindPropertyRelative("colorValue").colorValue + " (Color)]";
										break;
									case ParameterType.Vector2:
										label += "value [" + paramProp.FindPropertyRelative("vector2Value").vector2Value + " (Vector2)]";
										break;
									case ParameterType.Vector3:
										label += "value [" + paramProp.FindPropertyRelative("vector3Value").vector3Value + " (Vector3)]";
										break;
									case ParameterType.Vector4:
										label += "value [" + paramProp.FindPropertyRelative("vector4Value").vector4Value + " (Vector4)]";
										break;
									case ParameterType.Vector2Int:
										label += "value [" + paramProp.FindPropertyRelative("vector2IntValue").vector2IntValue + " (Vector2Int)]";
										break;
									case ParameterType.Vector3Int:
										label += "value [" + paramProp.FindPropertyRelative("vector3IntValue").vector3IntValue + " (Vector3Int)]";
										break;									
									case ParameterType.Quaternion:
										label += "value [" + paramProp.FindPropertyRelative("quaternionValue").quaternionValue + " (Quaternion)]";
										break;
									case ParameterType.Object:
										var objProp = paramProp.FindPropertyRelative("objectValue").objectReferenceValue;
										if (objProp == null) {
											label += "value [--MISSING-- (Object)]";
										} else
											label += "value [" + objProp + " (" + objProp.GetType().Name + ")]";
										break;
								}
							} else if (actionProp.enumValueIndex == 2) {
								var allOfTypeProp = property.FindPropertyRelative("_eventTarget").FindPropertyRelative("allOfType");
								if (allOfTypeProp.boolValue) {
									label = "Abort all instances of eventID [" + eventIDProp.stringValue + "]";
								} else {
									label = "Abort first instance of eventID [" + eventIDProp.stringValue + "]";
								}
							}
						}
						var parameterProp = property.FindPropertyRelative("_eventTarget").FindPropertyRelative("parameter");
						switch ((ParameterType)parameterProp.FindPropertyRelative("type").enumValueIndex) {
							case ParameterType.GameObject:
								if (parameterProp.FindPropertyRelative("gameObjectValue").objectReferenceValue == null)
									targetIsValid = false;
								break;
							case ParameterType.Object:
								if (parameterProp.FindPropertyRelative("objectValue").objectReferenceValue == null)
									targetIsValid = false;
								break;
							case ParameterType.Transform:
								if (parameterProp.FindPropertyRelative("transformValue").objectReferenceValue == null)
									targetIsValid = false;
								break;
							case ParameterType.String:
								if (string.IsNullOrEmpty(parameterProp.FindPropertyRelative("stringValue").stringValue))
									targetIsValid = false;
								break;
						}
					}
					break;
				case TargetType.GameObject:
					{
						var actionProp = property.FindPropertyRelative("_gameObjectTarget").FindPropertyRelative("action");
						var gameObjectProp = property.FindPropertyRelative("_gameObjectTarget").FindPropertyRelative("gameObject");
						var parameterProp = property.FindPropertyRelative("_gameObjectTarget").FindPropertyRelative("parameter");

						if (gameObjectProp.objectReferenceValue == null) {
							label = "[ERROR] Missing GameObject";
							targetIsValid = false;
						} else {
							label = _gameObjectActions[actionProp.enumValueIndex];
							switch (actionProp.enumValueIndex) {
								case 2: //BroadcastToListeners
									var listenersProp = property.FindPropertyRelative("_gameObjectTarget").FindPropertyRelative("_cachedListenersView");
									var numListeners = listenersProp.arraySize;
									if (numListeners == 0)
										targetIsValid = false;

									label = "Broadcast to [" + numListeners + "] IGameEventListeners in [" + gameObjectProp.objectReferenceValue.name + "]";
									var includeChildrenProp = property.FindPropertyRelative("_gameObjectTarget").FindPropertyRelative("broadcastTarget");
									label += " " + includeChildrenProp.enumDisplayNames[includeChildrenProp.enumValueIndex];
									break;
								case 3: //SendMessage
								case 4: //BroadcastMessage
									var methodNameProp = property.FindPropertyRelative("_gameObjectTarget").FindPropertyRelative("_methodName");

									if (string.IsNullOrEmpty(methodNameProp.stringValue))
										targetIsValid = false;

									label += " [" + (string.IsNullOrEmpty(methodNameProp.stringValue) ? "--MISSING METHOD NAME--" : methodNameProp.stringValue) + "] on [" + gameObjectProp.objectReferenceValue.name + "]";

									switch ((ParameterType)parameterProp.FindPropertyRelative("type").enumValueIndex) {
										case ParameterType.GameObject:
											if (parameterProp.FindPropertyRelative("gameObjectValue").objectReferenceValue == null)
												targetIsValid = false;
											break;
										case ParameterType.Object:
											if (parameterProp.FindPropertyRelative("objectValue").objectReferenceValue == null)
												targetIsValid = false;
											break;
										case ParameterType.Transform:
											if (parameterProp.FindPropertyRelative("transformValue").objectReferenceValue == null)
												targetIsValid = false;
											break;
										case ParameterType.String:
											if (string.IsNullOrEmpty(parameterProp.FindPropertyRelative("stringValue").stringValue))
												targetIsValid = false;
											break;
									}
									break;
								default:
									label += " [" + gameObjectProp.objectReferenceValue.name + "]";
									break;
							}
						}
					}
					break;
				case TargetType.Timeline:
					{
						var actionProp = property.FindPropertyRelative("_timelineTarget").FindPropertyRelative("action");
						var timelineProp = property.FindPropertyRelative("_timelineTarget").FindPropertyRelative("timeline");

						if (timelineProp.objectReferenceValue == null) {
							label = "[ERROR] Missing PlayableDirector";
							targetIsValid = false;
						} else {
							if (((PlayableDirector)timelineProp.objectReferenceValue).playableAsset == null) {
								label = "[ERROR] Missing PlayableAsset in Director";
								targetIsValid = false;
							} else if (actionProp.enumValueIndex < 4) {
								label = _timelineActions[actionProp.enumValueIndex] + " " + timelineProp.objectReferenceValue.name + " (Playable Director)";
							} else {
								var timeProp = property.FindPropertyRelative("_timelineTarget").FindPropertyRelative("time");
								label = "Skip to [" + Mathf.Abs(timeProp.floatValue) + "] seconds on " + timelineProp.objectReferenceValue.name + " (Playable Director)";
							}
						}
					}
					break;
			}

			return targetIsValid;
		}

		#endregion
	}
}
#endif