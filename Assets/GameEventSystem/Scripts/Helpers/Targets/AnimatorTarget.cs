using System;
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace GameSystems.Events
{
	[System.Serializable]
	public sealed class AnimatorTarget : IGameEventTarget
	{
		//=====================================================================================================================//
		//=================================================== Pending Tasks ===================================================//
		//=====================================================================================================================//

		#region Pending Tasks

		public enum ActionType
		{
			Enable = 0,
			Disable = 1,
			SetParameter = 2,
			CrossFade = 3,
			MatchTarget = 4,
			Play = 5,
			SetIK = 6,
			SetLayerWeight = 7
		}

		#endregion

		//=====================================================================================================================//
		//================================================= Public Properties ==================================================//
		//=====================================================================================================================//

		#region Public Properties

		public Animator animator;
		public ActionType action;

		//SetParameter values
		public AnimatorEventData.ParameterType parameterType;
		public string parameterName;
		public int integerValue;
		public float floatValue;
		public bool booleanValue;

		//Crossfade & Play values
		public int selectedIndex = 0;
		public int targetState;
		public string targetStateName;
		public float transitionDuration;
		public float targetStateOffset;

		//MatchTarget values
		public Transform matchTarget;
		public AvatarTarget avatarTarget;
		public Vector3 weightMaskPositionWeight;
		public float weightMaskRotationWeight;
		public float startTime;
		public float endTime;

		//SetIK values
		public AvatarIKGoal avatarIkGoal;
		public Transform ikPositionRef;
		public float ikPositionWeight;
		public Transform ikRotationRef;
		public float ikRotationWeight;

		//SetLookAt values
		public Transform lookAtRef;
		public float lookAtWeight;
		public float bodyWeight = 0f;
		public float headWeight = 1f;
		public float eyesWeight = 0f;
		public float clampWeight = 0.5f;

		//SetLayerWeight values
		public int targetLayer;
		public string targetLayerName;
		public float layerWeight;

		#endregion

		//=====================================================================================================================//
		//=================================================== Public Methods ==================================================//
		//=====================================================================================================================//

		#region Public Methods

		public static AnimatorTarget Clone(AnimatorTarget original)
		{
			if (original == null)
				return null;

			var newTarget = new AnimatorTarget
			                {
				                animator = original.animator,
				                action = original.action,

				                //SetParameter values
				                parameterType = original.parameterType,
				                parameterName = original.parameterName,
				                integerValue = original.integerValue,
				                floatValue = original.floatValue,
				                booleanValue = original.booleanValue,

				                //Crossfade & Play values
				                targetState = original.targetState,
				                transitionDuration = original.transitionDuration,
				                targetStateOffset = original.targetStateOffset,
				                targetStateName = original.targetStateName,

				                //MatchTarget values
				                matchTarget = original.matchTarget,
				                avatarTarget = original.avatarTarget,
				                weightMaskPositionWeight = original.weightMaskPositionWeight,
				                weightMaskRotationWeight = original.weightMaskRotationWeight,
				                startTime = original.startTime,
				                endTime = original.endTime,

				                //SetIK values
				                avatarIkGoal = original.avatarIkGoal,
				                ikPositionRef = original.ikPositionRef,
				                ikPositionWeight = original.ikPositionWeight,
				                ikRotationRef = original.ikRotationRef,
				                ikRotationWeight = original.ikRotationWeight,
				                lookAtRef = original.lookAtRef,
				                lookAtWeight = original.lookAtWeight,
				                bodyWeight = original.bodyWeight,
				                headWeight = original.headWeight,
				                eyesWeight = original.eyesWeight,
				                clampWeight = original.clampWeight,

				                //SetLayerWeight values
				                targetLayer = original.targetLayer,
				                targetLayerName = original.targetLayerName,
				                layerWeight = original.layerWeight
			                };

			return newTarget;
		}

		public void Process(params object[] data)
		{
			if (animator == null)
				return;

			switch (action) {
				case ActionType.Enable:
					animator.enabled = true;
					break;
				case ActionType.Disable:
					animator.enabled = false;
					break;
				case ActionType.SetParameter:
					switch (parameterType) {
						case AnimatorEventData.ParameterType.Boolean:
							animator.SetBool(parameterName, booleanValue);
							break;
						case AnimatorEventData.ParameterType.Float:
							animator.SetFloat(parameterName, floatValue);
							break;
						case AnimatorEventData.ParameterType.Integer:
							animator.SetInteger(parameterName, integerValue);
							break;
						case AnimatorEventData.ParameterType.Trigger:
							animator.SetTrigger(parameterName);
							break;
					}

					break;
				case ActionType.CrossFade:
					animator.CrossFadeInFixedTime(targetState, transitionDuration, -1, targetStateOffset);
					break;
				case ActionType.MatchTarget:
					if (matchTarget != null) {
						var weightMask = new MatchTargetWeightMask
						                 {
							                 positionXYZWeight = weightMaskPositionWeight,
							                 rotationWeight = weightMaskRotationWeight,
						                 };

						animator.MatchTarget(matchTarget.position, matchTarget.rotation, avatarTarget, weightMask, startTime, endTime);
					}

					break;
				case ActionType.Play:
					animator.PlayInFixedTime(targetState, -1, targetStateOffset);
					break;
				case ActionType.SetIK:
					if (ikPositionRef != null) {
						animator.SetIKPosition(avatarIkGoal, ikPositionRef.position);
						animator.SetIKPositionWeight(avatarIkGoal, ikPositionWeight);
					}

					if (ikRotationRef != null) {
						animator.SetIKRotation(avatarIkGoal, ikRotationRef.rotation);
						animator.SetIKRotationWeight(avatarIkGoal, ikRotationWeight);
					}

					if (lookAtRef != null) {
						animator.SetLookAtPosition(lookAtRef.position);
						animator.SetLookAtWeight(lookAtWeight, bodyWeight, headWeight, eyesWeight, clampWeight);
					}

					break;
				case ActionType.SetLayerWeight:
					animator.SetLayerWeight(targetLayer, layerWeight);
					break;
			}
		}

		public bool IsValid(out string message)
		{
			if (animator == null) {
				message = "[AnimatorTarget] Target Animator reference is null.";
				return false;
			}

			if (animator.runtimeAnimatorController == null) {
				message = "[AnimatorTarget] Target Animator is misssing AnimatorController reference.";
				return false;
			}

			switch (action) {
				case ActionType.SetParameter:
					if (string.IsNullOrEmpty(parameterName)) {
						message = "[AnimatorTarget] Target parameter to be set is missing.";
						return false;
					}

					break;
				case ActionType.MatchTarget:
					if (matchTarget == null) {
						message = "[AnimatorTarget] MatchTarget reference transform is missing.";
						return false;
					}

					break;
				case ActionType.SetIK:
					if (ikPositionRef == null && ikRotationRef == null && lookAtRef == null) {
						message = "[AnimatorTarget] At least one IK reference transform needs to be configured.";
						return false;
					}
					break;
				case ActionType.SetLayerWeight:
					if (string.IsNullOrEmpty(targetLayerName)) {
						message = "[ERROR] Target Animator only has Base Layer";
						return false;
					}
					break;
			}

			message = "AnimatorTarget is valid.";
			return true;
		}

		private bool _IsValid()
		{
			if (animator == null || animator.runtimeAnimatorController == null)
				return false;

			switch (action) {
				case ActionType.SetParameter:
					if (string.IsNullOrEmpty(parameterName)) {
						return false;
					}

					break;
				case ActionType.MatchTarget:
					if (matchTarget == null) {
						return false;
					}

					break;
				case ActionType.SetIK:
					if (ikPositionRef == null && ikRotationRef == null && lookAtRef == null) {
						return false;
					}
					break;
				case ActionType.SetLayerWeight:
					if (string.IsNullOrEmpty(targetLayerName))
						return false;
					break;
			}

			return true;
		}

		public bool OnValidate()
		{
			return _IsValid();
		}

		public override string ToString()
		{
			if (animator == null || animator.runtimeAnimatorController == null)
				return "[ERROR] MISSING ANIMATOR CONTROLLER REFERENCE";

			var res = "";
			switch (action) {
				case ActionType.Enable:
				case ActionType.Disable:
					res += action + " " + animator.name;
					break;
				case ActionType.SetParameter:
					switch (parameterType) {
						case AnimatorEventData.ParameterType.Boolean:
							res = "SetBool [" + parameterName + "] to " + "[" + booleanValue + "] on " + animator.name;
							break;
						case AnimatorEventData.ParameterType.Float:
							res = "SetFloat [" + parameterName + "] to " + "[" + floatValue + "] on " + animator.name;
							break;
						case AnimatorEventData.ParameterType.Integer:
							res = "SetInteger[" + parameterName + "] to " + "[" + integerValue + "] on " + animator.name;
							break;
						case AnimatorEventData.ParameterType.Trigger:
							res = "SetTrigger [" + parameterName + "] on " + animator.name;
							break;
					}

					break;
				case ActionType.CrossFade:
					res = string.Format("Crossfade to [{0}] over [{1}] seconds on ", targetStateName, transitionDuration) + animator.name;;
					break;
				case ActionType.MatchTarget:
					res = matchTarget == null ? "[ERROR] MISSING MatchTarget REFERENCE TRANSFORM" : string.Format("Match [{0}] to [{1}] on ", avatarTarget, matchTarget.name) + animator.name;;
					break;
				case ActionType.Play:
					res = string.Format("Play [{0}] on ", targetStateName) + animator.name;;
					break;
				case ActionType.SetIK:
					if (ikPositionRef == null && ikRotationRef == null && lookAtRef == null) {
						res = "[ERROR] At least one Ik reference transform needs to be setup.";
					} else {
						res = string.Empty;
						if (ikPositionRef != null || ikRotationRef != null) {
							res = string.Format("SetIK [{0}] IK's ", avatarIkGoal);
							if (ikPositionRef != null)
								res += " | Position ";
							if (ikRotationRef != null)
								res += " | Rotation ";

							res += "| ";
						}

						if (lookAtRef != null) {
							if (string.IsNullOrEmpty(res))
								res = "Set Look At Position on" + animator.name;
							else {
								res += "LookAt Position | on " + animator.name;
							}
						}
					}

					break;
				case ActionType.SetLayerWeight:
					res = targetLayer > 0 ? string.Format("Set [{0}] layer weight to [{1}] on ", targetLayerName, layerWeight) + animator.name: "[ERROR] Target Animator only has Base Layer";
					break;
			}

			return res;
		}

		#endregion
	}
}