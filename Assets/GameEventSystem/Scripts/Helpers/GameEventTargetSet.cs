using System.Collections.Generic;
using UnityEngine;

namespace GameSystems.Events
{
	[System.Serializable]
	public sealed class GameEventTargetSet
	{
		//=====================================================================================================================//
		//================================================= Public Properties ==================================================//
		//=====================================================================================================================//

		#region Public Properties

		public List<GameEventTarget> targets;

		#endregion

		//=====================================================================================================================//
		//=================================================== Public Methods ==================================================//
		//=====================================================================================================================//

		#region Public Methods

		public static GameEventTargetSet Clone(GameEventTargetSet original)
		{
			var newSet = new GameEventTargetSet();

			if (original == null) 
				return newSet;
			
			for (var i = 0; i < original.targets.Count; i++) {
				newSet.targets.Add(GameEventTarget.Clone(original.targets[i]));
			}

			return newSet;
		}

		public bool IsValid(out string message)
		{
			if (targets == null || targets.Count == 0){
				message = "[Targets List is empty]";
				return false;
			}

			for (var i = 0; i < targets.Count; i++) {
				if (targets[i] == null){
					message = string.Format("GameEventTarget[{0}] is null.", i);
					return false;
				}

				if (!targets[i].IsValid(out message))
					return false;
			}

			message = "GameEventListener is Valid.";
			return true;
		}

		public void Initialize()
		{
			if (targets == null || targets.Count <= 0) 
			return;
			
			foreach (var target in targets){
				if (target != null)
					target.OnValidate();

				string message;
				
				if (target != null && target.IsValid(out message)) 
					continue;
				
				Debug.LogWarning("GameEventHandler target is not valid:\n" + target);
				return;
			}
		}

		public void OnValidate()
		{
			if (targets == null) 
				return;
			
			foreach (var target in targets){
				if (target != null)
					target.OnValidate();
			}
		}

		public override string ToString()
		{
			var text = "";

			if (targets == null || targets.Count <= 0) 
				return text;
			
			for (var i = 0; i < targets.Count; i++) {

				if (targets[i] == null) {
					text += string.Format("Target[{0}]: {1}\n", i, "NULL");
					continue;
				}

				text += string.Format("Target[{0}]: {1}\n", i, targets[i]);
			}

			return text;
		}

		public void Process(params object[] data)
		{
			if (targets == null)
				return;

			for (var i = targets.Count - 1; i >= 0; i--) {
				if (targets[i] != null) {
					targets[i].Process(data);
				}
			}
		}

		public void Clear()
		{
			if (targets != null && targets.Count > 0)
				targets.Clear();
		}
		
		#endregion
	}
}