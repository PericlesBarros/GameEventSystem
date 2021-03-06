//=============================================================================================================================//
//
//	Project: GameEventSystem
//	Copyright: DefaultCompany
//  Created on: 3/23/2019 12:20:02 AM
//
//=============================================================================================================================//


#region Usings

using System.Collections;
using GameSystems.Events;
using UnityEngine;

#endregion

namespace GameEventSystem.Samples
{
	public class ManualEventTrigger : MonoBehaviour
	{
		//=====================================================================================================================//
		//================================================= Inspector Variables ===============================================//
		//=====================================================================================================================//

		#region Inspector Variables

		[SerializeField] private GameEventTrigger _trigger;

		#endregion

		//=====================================================================================================================//
		//============================================= Unity Callback Methods ================================================//
		//=====================================================================================================================//

		#region Unity Callback Methods

		private void Start()
		{
			StartCoroutine(RaiseEvent());
		}

		#endregion
		
		//=====================================================================================================================//
		//===================================================== Coroutines ====================================================//
		//=====================================================================================================================//

		#region Coroutines

		private IEnumerator RaiseEvent()
		{
			Debug.Log("Raising event manually after 10 seconds");
			yield return  new WaitForSeconds(5);
			_trigger.RaiseEvents();
		}
		
		#endregion
	}
}