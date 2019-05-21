//=============================================================================================================================//
//
//	Project: GameEventSystem
//	Copyright: DefaultCompany
//  Created on: 3/23/2019 12:49:02 AM
//
//=============================================================================================================================//


#region Usings

using UnityEngine;

#endregion

namespace GameEventSystem.Utils
{
	public class Note : MonoBehaviour
	{
		//=====================================================================================================================//
		//================================================= Inspector Variables ===============================================//
		//=====================================================================================================================//

		#region Inspector Variables

		[TextArea(5, 15), SerializeField, HideInInspector] private string _note;
		[SerializeField, HideInInspector] private bool _editable;

		#endregion
	}
}