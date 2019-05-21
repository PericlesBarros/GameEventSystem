using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameSystems.Events
{	
	public sealed class AnimatorEventData : GameEventData
	{
		//=====================================================================================================================//
		//================================================== Internal Classes =================================================//
		//=====================================================================================================================//

		#region Internal Classes

		public enum ParameterType
		{
			//None,
			Trigger,
			Integer,
			Float,
			Boolean
		}
		
		#endregion

		//=====================================================================================================================//
		//================================================== Public Properties ================================================//
		//=====================================================================================================================//

		#region Inspector Variables

		public ParameterType type;
		public string parameterName;
		public int integerValue;
		public float floatValue;
		public bool booleanValue;
		
		#endregion
	}
}