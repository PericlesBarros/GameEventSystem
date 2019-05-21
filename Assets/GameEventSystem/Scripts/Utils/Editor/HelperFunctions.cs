using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
//using GameSystems.Attributes;

#if UNITY_EDITOR
using UnityEditor;

namespace GameSystems.Tools
{
	public static class HelperFunctions
	{
		//=====================================================================================================================//
		//=================================================== Public Methods ==================================================//
		//=====================================================================================================================//

		#region Public Methods
		
		/// <summary>
		/// Draws disabled script reference like the one in Unity's Default inspector
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="editor"></param>
		public static void DrawMonoBehaviourScriptField<T> (this Editor editor) where T : MonoBehaviour
		{			
			//EditorGUILayout.Space();
			var wasEnabled =GUI.enabled;
			GUI.enabled = false;
			EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((T)editor.target), typeof(T), false);
			GUI.enabled = wasEnabled;			
		}

		public static void DrawScriptableObjectScriptField<T> (this Editor editor) where T : ScriptableObject
		{
			//EditorGUILayout.Space();
			var wasEnabled =GUI.enabled;
			GUI.enabled = false;
			EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((T)editor.target), typeof(T), false);
			GUI.enabled = wasEnabled;
		}

		/// <summary>
		/// Gets all methods in the given script that have been marked with the provided attribute type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="script"></param>
		/// <returns></returns>
		public static string[] FetchMethodsWithAttribute<T>(this MonoBehaviour script) where T : Attribute
		{
			if (script == null)
				return new string[0];
						
			var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
			var methods = new List<string>();
			var methodInfo = script.GetType().GetMethods(flags).Where(method => HasValidAttribute<T>(method));
			foreach (var info in methodInfo) {
				methods.Add(info.Name);
			}

			return methods.ToArray();
		}
		
		/// <summary>
		/// Checks if given method has the provided attribute
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="methodInfo"></param>
		/// <returns></returns>		
		public static bool HasValidAttribute<T>(MethodInfo methodInfo) where T : Attribute
		{
			if (methodInfo == null)
				return false;

			var attrbs = methodInfo.GetCustomAttributes(typeof(T), true);
			if (attrbs.Length == 0)
				return false;

			return true;
		}
		
		
		public static IEnumerable<FieldInfo> GetAllFields(this object target, SerializedObject serializedObject)
        {
            List<Type> types = new List<Type>()
            {
                target.GetType()
            };

            while (types.Last().BaseType != null) {
                types.Add(types.Last().BaseType);
            }

            for (int i = types.Count - 1; i >= 0; i--) {
                IEnumerable<FieldInfo> fieldInfos = types[i]
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(f => serializedObject.FindProperty(f.Name) != null);

                foreach (var fieldInfo in fieldInfos) {
                    yield return fieldInfo;
                }
            }
        }

        public static IEnumerable<FieldInfo> GetAllFields<T>(this object target, SerializedObject serializedObject)
        {
            var allFields = target.GetAllFields(serializedObject);
            return allFields.Where(f => f.GetCustomAttributes(typeof(T), true).Length > 0);
        }

        /* public static IEnumerable<FieldInfo> GetFieldGroups(this Editor _editor)
         {

         }*/

        #endregion
    }
}

#endif