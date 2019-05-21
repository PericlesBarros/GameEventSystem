using UnityEngine;

namespace GameSystems.Utils
{
	public enum ParameterType
	{
		None,
		Integer,
		Float,
		String,
		Boolean,
		GameObject,
		Transform,
		Vector2,
		Vector3,
		Vector4,
		Color,
		Object,
		Vector2Int,
		Vector3Int,
		Quaternion
	}

	[System.Serializable]
	public sealed class CallbackParameter
	{
		//=====================================================================================================================//
		//================================================= Public Properties ==================================================//
		//=====================================================================================================================//

		#region Public Properties

		public ParameterType type;
		public int integerValue;
		public float floatValue;
		public string stringValue;
		public bool booleanValue;
		public GameObject gameObjectValue;
		public Transform transformValue;
		public Vector2 vector2Value;
		public Vector3 vector3Value;
		public Vector4 vector4Value;
		public Vector2Int vector2IntValue;
		public Vector3Int vector3IntValue;
		public Quaternion quaternionValue;
		public Color colorValue;
		public Object objectValue;
		
		public object Value
		{
			get
			{
				if (type == ParameterType.Integer)
					return integerValue;

				if (type == ParameterType.Float)
					return floatValue;

				if (type == ParameterType.String)
					return stringValue;

				if (type == ParameterType.Boolean)
					return booleanValue;

				if (type == ParameterType.GameObject)
					return gameObjectValue;

				if (type == ParameterType.Vector2)
					return vector2Value;

				if (type == ParameterType.Vector3)
					return vector3Value;

				if (type == ParameterType.Color)
					return colorValue;

				if (type == ParameterType.Transform)
					return transformValue;

				if (type == ParameterType.Object)
					return objectValue;

				if(type == ParameterType.Quaternion)
					return quaternionValue;

				if(type == ParameterType.Vector2Int)
					return vector2IntValue;

				if(type == ParameterType.Vector3Int)
					return vector3IntValue;

				if(type == ParameterType.Vector4)
					return vector4Value;

				return null;
			}
		}

		#endregion

		//=====================================================================================================================//
		//================================================== Public Methods ===================================================//
		//=====================================================================================================================//

		#region Public Methods

		public override string ToString()
		{
			if (type == ParameterType.None)
				return "EMPTY parameter";

			return string.Format("Parameter [{0}] | \nValue [{1}]", type, Value);
		}

		public bool IsValid()
		{
			switch (type) {
				case ParameterType.GameObject:
					return gameObjectValue != null;
				case ParameterType.Transform:
					return transformValue != null;
				case ParameterType.Object:
					return objectValue != null;
				case ParameterType.String:
					return !string.IsNullOrEmpty(stringValue);
			}

			return true;
		}

		public static CallbackParameter Clone(CallbackParameter original)
		{
			CallbackParameter newParam = null;
			if (original != null) {
				newParam = new CallbackParameter();
				newParam.type = original.type;
				newParam.booleanValue = original.booleanValue;
				newParam.colorValue = original.colorValue;
				newParam.floatValue = original.floatValue;
				newParam.integerValue = original.integerValue;
				newParam.stringValue = original.stringValue;
				newParam.gameObjectValue = original.gameObjectValue;
				newParam.vector2Value = original.vector2Value;
				newParam.vector3Value = original.vector3Value;
				newParam.transformValue = original.transformValue;
				newParam.objectValue = original.objectValue;
				newParam.vector4Value = original.vector4Value;
				newParam.vector3IntValue = original.vector3IntValue;
				newParam.vector2IntValue = original.vector2IntValue;
				newParam.quaternionValue = original.quaternionValue;
			}

			return newParam;
		}

		#endregion
	}
}