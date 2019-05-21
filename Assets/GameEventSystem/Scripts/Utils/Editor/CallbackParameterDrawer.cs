using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;

namespace GameSystems.Utils
{
    [CustomPropertyDrawer (typeof(CallbackParameter))]
    public class CallbackParameterDrawer : PropertyDrawer
    {
        //=====================================================================================================================//
        //=================================================== Private Fields ==================================================//
        //=====================================================================================================================//

        #region Private Fields

        SerializedProperty type;
        SerializedProperty integerValue;
        SerializedProperty floatValue;
        SerializedProperty stringValue;
        SerializedProperty booleanValue;
        SerializedProperty gameObjectValue;
        SerializedProperty transformValue;
        SerializedProperty vector2Value;
        SerializedProperty vector3Value;
		SerializedProperty vector4Value;
        SerializedProperty vector2IntValue;
		SerializedProperty vector3IntValue;
        SerializedProperty quaternionValue;
        SerializedProperty colorValue;
		SerializedProperty objectValue;
        SerializedObject serializedObject;
		static Color defaultColor;

        #endregion

        //=====================================================================================================================//
        //============================================= Unity Callback Methods ================================================//
        //=====================================================================================================================//

        #region Unity Callback Methods

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
			defaultColor = GUI.color;

            type = property.FindPropertyRelative("type");
            integerValue = property.FindPropertyRelative("integerValue");
            floatValue = property.FindPropertyRelative("floatValue");
            stringValue = property.FindPropertyRelative("stringValue");
            booleanValue = property.FindPropertyRelative("booleanValue");
            gameObjectValue = property.FindPropertyRelative("gameObjectValue");
            transformValue = property.FindPropertyRelative("transformValue");
            vector2Value = property.FindPropertyRelative("vector2Value");
            vector3Value = property.FindPropertyRelative("vector3Value");
			vector2IntValue = property.FindPropertyRelative("vector2IntValue");
            vector3IntValue = property.FindPropertyRelative("vector3IntValue");
			vector4Value = property.FindPropertyRelative("vector4Value");
            quaternionValue = property.FindPropertyRelative("quaternionValue");
            colorValue = property.FindPropertyRelative("colorValue");
			objectValue = property.FindPropertyRelative("objectValue");

            serializedObject = property.serializedObject;
			//serializedObject.Update();

	        //EditorGUI.BeginProperty(position, label, property);
	        
            int indentLevel = EditorGUI.indentLevel;
            //EditorGUI.indentLevel = 0;

            EditorGUI.PropertyField(position, type, true);
            position.y += EditorGUIUtility.singleLineHeight + 2.5f;

            switch ((ParameterType)type.enumValueIndex) {
                case ParameterType.None:
                    break;
                case ParameterType.Integer:
                    EditorGUI.PropertyField(position, integerValue);
                    break;
                case ParameterType.Float:
                    EditorGUI.PropertyField(position, floatValue);
                    break;
                case ParameterType.String:
					if(string.IsNullOrEmpty(stringValue.stringValue))
						GUI.color = Color.red;
                    EditorGUI.PropertyField(position, stringValue);
                    break;
                case ParameterType.Boolean:
                    EditorGUI.PropertyField(position, booleanValue);
                    break;
                case ParameterType.GameObject:
					if(gameObjectValue.objectReferenceValue == null)
						GUI.color = Color.red;
                    EditorGUI.PropertyField(position, gameObjectValue);
                    break;
                case ParameterType.Transform:
					if(transformValue.objectReferenceValue == null)
						GUI.color = Color.red;
                    EditorGUI.PropertyField(position, transformValue);
                    break;
                case ParameterType.Vector2:
                    EditorGUI.PropertyField(position, vector2Value);
                    break;
                case ParameterType.Vector3:
                    EditorGUI.PropertyField(position, vector3Value);
                    break;
                case ParameterType.Color:
                    EditorGUI.PropertyField(position, colorValue);
                    break;
				case ParameterType.Object:
					if(objectValue.objectReferenceValue == null)
						GUI.color = Color.red;
					EditorGUI.PropertyField(position, objectValue);
					break;
				case ParameterType.Vector4:
					vector4Value.vector4Value = EditorGUI.Vector4Field(position, "Vector 4 Value", vector4Value.vector4Value);
					break;
				case ParameterType.Vector3Int:
					EditorGUI.PropertyField(position, vector3IntValue);
					break;
				case ParameterType.Vector2Int:
					EditorGUI.PropertyField(position, vector2IntValue);
					break;
				case ParameterType.Quaternion:
					var value = quaternionValue.quaternionValue;
					var v4Value = EditorGUI.Vector4Field(position, "Quaternion Value", new Vector4(value.x, value.y, value.z, value.w));
					quaternionValue.quaternionValue = new Quaternion(v4Value.x, v4Value.y, v4Value.z, v4Value.w);					
					break;
            }

            EditorGUI.indentLevel = indentLevel;
	        

            //serializedObject.ApplyModifiedProperties();
			GUI.color = defaultColor;
        }

        #endregion        
    }
}

#endif