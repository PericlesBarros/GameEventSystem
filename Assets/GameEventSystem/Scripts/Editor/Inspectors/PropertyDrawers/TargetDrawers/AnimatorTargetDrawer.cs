using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace GameSystems.Events.Drawers
{
	[CustomPropertyDrawer(typeof(AnimatorTarget))]
	public class AnimatorTargetDrawer : PropertyDrawer
	{
		//=====================================================================================================================//
		//=================================================== Private Fields ==================================================//
		//=====================================================================================================================//

		#region Private Fields

		private static Color _defaultColor;

		private SerializedObject _serializedObject;
		private SerializedProperty _action;
		private SerializedProperty _animator;
		//SetParameter
		private SerializedProperty _paramType;
		private SerializedProperty _paramName;
		private SerializedProperty _intValue;
		private SerializedProperty _floatValue;
		private SerializedProperty _boolValue;

		//Crossfade
		private SerializedProperty _selectedIndex;
		private SerializedProperty _targetState;
		private SerializedProperty _targetStateName;
		private SerializedProperty _transitionDuration;
		private SerializedProperty _targetStateOffset;

		//Match target
		private SerializedProperty _matchTarget;
		private SerializedProperty _avatarTarget;
		private SerializedProperty _positionWeight;
		private SerializedProperty _rotationWeight;
		private SerializedProperty _startTime;
		private SerializedProperty _endTime;

		//SetIK && LookAt
		private SerializedProperty _avatarIkGoal;
		private SerializedProperty _ikPositionRef;
		private SerializedProperty _ikPositionWeight;
		private SerializedProperty _ikRotationRef;
		private SerializedProperty _ikRotationWeight;
		private SerializedProperty _lookAtRef;
		private SerializedProperty _lookAtWeight;
		private SerializedProperty _bodyWeight;
		private SerializedProperty _headWeight;
		private SerializedProperty _eyesWeight;
		private SerializedProperty _clampWeight;

		//SetLayerWeight
		public SerializedProperty _targetLayer;
		public SerializedProperty _targetLayerName;
		public SerializedProperty _layerWeight;

		private readonly List<string> _stateNames = new List<string>();
		private readonly List<string> _layerNames = new List<string>();

		#endregion

		//=====================================================================================================================//
		//============================================= Unity Callback Methods ================================================//
		//=====================================================================================================================//

		#region Unity Callback Methods

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			_defaultColor = GUI.color;

			_action = property.FindPropertyRelative("action");
			_animator = property.FindPropertyRelative("animator");

			//SetParameter values
			_paramType = property.FindPropertyRelative("parameterType");
			_paramName = property.FindPropertyRelative("parameterName");
			_intValue = property.FindPropertyRelative("integerValue");
			_floatValue = property.FindPropertyRelative("floatValue");
			_boolValue = property.FindPropertyRelative("booleanValue");

			//Crossfade values
			_selectedIndex = property.FindPropertyRelative("selectedIndex");
			_targetState = property.FindPropertyRelative("targetState");
			_targetStateName = property.FindPropertyRelative("targetStateName");
			_transitionDuration = property.FindPropertyRelative("transitionDuration");
			_targetStateOffset = property.FindPropertyRelative("targetStateOffset");

			//MatchTarget values
			_matchTarget = property.FindPropertyRelative("matchTarget");
			_avatarTarget = property.FindPropertyRelative("avatarTarget");
			_positionWeight = property.FindPropertyRelative("weightMaskPositionWeight");
			_rotationWeight = property.FindPropertyRelative("weightMaskRotationWeight");
			_startTime = property.FindPropertyRelative("startTime");
			_endTime = property.FindPropertyRelative("endTime");

			//SetIK values
			_avatarIkGoal = property.FindPropertyRelative("avatarIkGoal");
			_ikPositionRef = property.FindPropertyRelative("ikPositionRef");
			_ikPositionWeight = property.FindPropertyRelative("ikPositionWeight");
			_ikRotationRef = property.FindPropertyRelative("ikRotationRef");
			_ikRotationWeight = property.FindPropertyRelative("ikRotationWeight");
			_lookAtRef = property.FindPropertyRelative("lookAtRef");
			_lookAtWeight = property.FindPropertyRelative("lookAtWeight");
			_bodyWeight = property.FindPropertyRelative("bodyWeight");
			_headWeight = property.FindPropertyRelative("headWeight");
			_eyesWeight = property.FindPropertyRelative("eyesWeight");
			_clampWeight = property.FindPropertyRelative("clampWeight");

			//SetLayerWeight values
			_targetLayer = property.FindPropertyRelative("targetLayer");
			_targetLayerName = property.FindPropertyRelative("targetLayerName");
			_layerWeight = property.FindPropertyRelative("layerWeight");

			_serializedObject = property.serializedObject;

			int indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			EditorGUI.BeginChangeCheck();
			//Create rect
			var fieldRect = position;
			fieldRect.x += 5f;
			fieldRect.width = 140f;
			fieldRect.y += EditorGUIUtility.singleLineHeight * 1.25f + 2.5f;
			fieldRect.height = EditorGUIUtility.singleLineHeight;

			// Action-Target line
			GUILayout.BeginHorizontal();
			//Draw action field
			EditorGUI.PropertyField(fieldRect, _action, GUIContent.none);
			fieldRect.x += 145f;
			fieldRect.width = position.width - 155f;

			if (_animator.objectReferenceValue == null)
				GUI.color = Color.red;

			//Draw target timeline
			EditorGUI.PropertyField(fieldRect, _animator, GUIContent.none);
			GUILayout.EndHorizontal();

			GUI.color = _defaultColor;
			var labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 145f;

			var animator = _animator.objectReferenceValue as Animator;
			if (animator == null || animator.runtimeAnimatorController == null) {
				_stateNames.Clear();
			} else {
				var assetPath = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
				var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
				if (controller == null) {
					_stateNames.Clear();
				} else {
					_stateNames.AddRange(GetFullStateNames(controller));
					_layerNames.AddRange(GetLayerNames(controller));
				}
			}

			switch (_action.enumValueIndex) {
				case 2: //SetParameter
					GUILayout.BeginHorizontal();
					fieldRect.width = 140f;
					fieldRect.x = position.x                         + 5f;
					fieldRect.y += EditorGUIUtility.singleLineHeight + 3.5f;
					EditorGUI.PropertyField(fieldRect, _paramType, new GUIContent("", "Parameter type"));
					fieldRect.x += 145f;
					fieldRect.width = position.width - 155f;

					if (string.IsNullOrEmpty(_paramName.stringValue))
						GUI.color = Color.red;

					EditorGUI.PropertyField(fieldRect, _paramName, new GUIContent("", "Parameter name"));
					GUILayout.EndHorizontal();

					GUI.color = _defaultColor;

					fieldRect.x = position.x                         + 5f;
					fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
					fieldRect.width = position.width                 - 10f;

					switch ((AnimatorEventData.ParameterType) _paramType.enumValueIndex) {
						case AnimatorEventData.ParameterType.Boolean:
							EditorGUI.PropertyField(fieldRect, _boolValue);
							break;
						case AnimatorEventData.ParameterType.Float:
							EditorGUI.PropertyField(fieldRect, _floatValue);
							break;
						case AnimatorEventData.ParameterType.Integer:
							EditorGUI.PropertyField(fieldRect, _intValue);
							break;
					}

					break;
				case 3: //Crossfade
					fieldRect.width = position.width                 - 10f;
					fieldRect.x = position.x                         + 5f;
					fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
					_selectedIndex.intValue = Mathf.Clamp(_selectedIndex.intValue, 0, _stateNames.Count);
					if (_stateNames.Count > 0) {
						_selectedIndex.intValue = EditorGUI.Popup(fieldRect, "Target State", _selectedIndex.intValue, _stateNames.ToArray());
						_targetState.intValue = Animator.StringToHash(_stateNames[_selectedIndex.intValue]);
						_targetStateName.stringValue = _stateNames[_selectedIndex.intValue];
					} else {
						EditorGUI.LabelField(fieldRect, "-- Missing Animator or AnimatorController --", EditorStyles.centeredGreyMiniLabel);
						_targetState.intValue = -1;
						_targetStateName.stringValue = string.Empty;
					}

					fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
					EditorGUI.PropertyField(fieldRect, _transitionDuration);
					_transitionDuration.floatValue = Mathf.Clamp(_transitionDuration.floatValue, 0, 25);

					fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
					EditorGUI.PropertyField(fieldRect, _targetStateOffset);
					break;
				case 4: //MatchTarget
					fieldRect.width = position.width                 - 10f;
					fieldRect.x = position.x                         + 5f;
					fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
					if (_matchTarget.objectReferenceValue == null)
						GUI.color = Color.red;

					EditorGUI.PropertyField(fieldRect, _matchTarget);
					GUI.color = _defaultColor;

					fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
					EditorGUI.PropertyField(fieldRect, _avatarTarget);

					fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
					_positionWeight.vector3Value = EditorGUI.Vector3Field(fieldRect, "Position Weight", _positionWeight.vector3Value);
					var vector3 = new Vector3(Mathf.Clamp01(_positionWeight.vector3Value.x), Mathf.Clamp01(_positionWeight.vector3Value.y), Mathf.Clamp01(_positionWeight.vector3Value.z));
					_positionWeight.vector3Value = vector3;
					fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
					EditorGUI.Slider(fieldRect, _rotationWeight, 0, 1);

					fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
					EditorGUI.Slider(fieldRect, _startTime, 0, 1);
					fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
					EditorGUI.Slider(fieldRect, _endTime, 0, 1);
					break;
				case 5: //Play
					fieldRect.width = position.width                 - 10f;
					fieldRect.x = position.x                         + 5f;
					fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
					if (_stateNames.Count > 0) {
						_selectedIndex.intValue = EditorGUI.Popup(fieldRect, "Target State", _selectedIndex.intValue, _stateNames.ToArray());
						_targetState.intValue = Animator.StringToHash(_stateNames[_selectedIndex.intValue]);
						_targetStateName.stringValue = _stateNames[_selectedIndex.intValue];
					} else {
						EditorGUI.LabelField(fieldRect, "-- Missing Animator or AnimatorController --", EditorStyles.centeredGreyMiniLabel);
						_targetState.intValue = -1;
						_targetStateName.stringValue = string.Empty;
					}

					fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
					EditorGUI.PropertyField(fieldRect, _targetStateOffset);
					break;
				case 6: //SetIK
					fieldRect.width = position.width                 - 10f;
					fieldRect.x = position.x                         + 5f;
					fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
					EditorGUI.PropertyField(fieldRect, _avatarIkGoal);
					fieldRect.y += EditorGUIUtility.singleLineHeight + 7.5f;

					var boxRect = fieldRect;
					boxRect.y -= 3f;
					boxRect.x -= 5f;
					boxRect.width += 10;
					boxRect.height = (EditorGUIUtility.singleLineHeight + 2.5f) * (_ikPositionRef.objectReferenceValue == null ? 1 : 2) + 5;
					GUI.Box(boxRect, GUIContent.none, GUI.skin.FindStyle("Box"));

					EditorGUI.PropertyField(fieldRect, _ikPositionRef);
					if (_ikPositionRef.objectReferenceValue != null) {
						fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
						EditorGUI.Slider(fieldRect, _ikPositionWeight, 0, 1);
					}

					fieldRect.y += EditorGUIUtility.singleLineHeight + 10f;

					boxRect.y = fieldRect.y                                                                                             - 3f;
					boxRect.height = (EditorGUIUtility.singleLineHeight + 2.5f) * (_ikRotationRef.objectReferenceValue == null ? 1 : 2) + 5;
					GUI.Box(boxRect, GUIContent.none, GUI.skin.FindStyle("Box"));

					EditorGUI.PropertyField(fieldRect, _ikRotationRef);
					if (_ikRotationRef.objectReferenceValue != null) {
						fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
						EditorGUI.Slider(fieldRect, _ikRotationWeight, 0, 1);
					}

					fieldRect.y += EditorGUIUtility.singleLineHeight + 10f;

					boxRect.y = fieldRect.y                                                                                         - 3f;
					boxRect.height = (EditorGUIUtility.singleLineHeight + 2.5f) * (_lookAtRef.objectReferenceValue == null ? 1 : 6) + 5;
					GUI.Box(boxRect, GUIContent.none, GUI.skin.FindStyle("Box"));

					EditorGUI.PropertyField(fieldRect, _lookAtRef);
					if (_lookAtRef.objectReferenceValue != null) {
						fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
						EditorGUI.Slider(fieldRect, _lookAtWeight, 0, 1);
						fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
						EditorGUI.Slider(fieldRect, _bodyWeight, 0, 1);
						fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
						EditorGUI.Slider(fieldRect, _headWeight, 0, 1);
						fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
						EditorGUI.Slider(fieldRect, _eyesWeight, 0, 1);
						fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
						EditorGUI.Slider(fieldRect, _clampWeight, 0, 1);
					}

					break;
				case 7: //SetLayerWeight
					fieldRect.width = position.width                 - 10f;
					fieldRect.x = position.x                         + 5f;
					
					if (_layerNames.Count > 0) {
						fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
						_targetLayer.intValue = EditorGUI.Popup(fieldRect, "Target Layer", _targetLayer.intValue - 1, _layerNames.ToArray());
						_targetLayerName.stringValue = _layerNames[_targetLayer.intValue];
						_targetLayer.intValue += 1;
					} else {
						_targetLayer.intValue = 0;
						_targetLayerName.stringValue = String.Empty;
					}

					fieldRect.y += EditorGUIUtility.singleLineHeight + 2.5f;
					EditorGUI.Slider(fieldRect, _layerWeight, 0 , 1);
					break;
			}

			EditorGUIUtility.labelWidth = labelWidth;

			//Apply changes
			if (EditorGUI.EndChangeCheck()) {
				_serializedObject.ApplyModifiedProperties();
			}

			EditorGUI.indentLevel = indentLevel;
			GUI.color = _defaultColor;
		}

		private List<string> GetLayerNames(AnimatorController controller)
		{
			var result = new List<string>();

			for (var index = 1; index < controller.layers.Length; index++) {
				var layer = controller.layers[index];
				result.Add(layer.name);
			}

			return result;
		}


		private List<string> GetFullStateNames(AnimatorController controller)
		{
			var result = new List<string>();

			foreach (var layer in controller.layers) {
				result.AddRange(GetStateMachineStates(layer.stateMachine));
			}

			return result;
		}

		private List<string> GetStateMachineStates(AnimatorStateMachine stateMachine)
		{
			var result = new List<string>();

			foreach (var childState in stateMachine.states) {
				result.Add(stateMachine.name + "." + childState.state.name);
			}

			foreach (var childStateMachine in stateMachine.stateMachines) {
				result.AddRange(GetStateMachineStates(childStateMachine.stateMachine));
			}

			return result;
		}

		#endregion
	}
}