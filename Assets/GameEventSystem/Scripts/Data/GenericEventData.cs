using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystems.Events
{
	public sealed class GenericEventData : GameEventData
	{
		//=====================================================================================================================//
		//================================================= Public Properties ==================================================//
		//=====================================================================================================================//

		#region Public Properties

		public int intValue;
		public float floatValue;
		public string stringValue;
		public bool booleanValue;
		public Color colorValue;
		public Vector3 vector3Value;
		public Vector2 vector2Value;		

		#endregion
	}
}