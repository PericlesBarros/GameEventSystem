using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;

namespace GameSystems.Events.Inspectors
{
    [CustomEditor (typeof (Integration.GameEventTriggerAsset))]
    public class GameEventTriggerBehaviourInspector : Editor
    {
        //=====================================================================================================================//
        //============================================= Unity Callback Methods ================================================//
        //=====================================================================================================================//

        #region Unity Callback Methods

        public override void OnInspectorGUI()
        {
            var clip = (Integration.GameEventTriggerAsset)target;

            DrawDefaultInspector();

            switch (clip.type) {
                case HandlerType.EventID:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("eventID"));
                    break;
                case HandlerType.GameEvent:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("gameEvent"));
                    break;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("data"));
            
            serializedObject.ApplyModifiedProperties();
        }

        #endregion        
    }
}
#endif