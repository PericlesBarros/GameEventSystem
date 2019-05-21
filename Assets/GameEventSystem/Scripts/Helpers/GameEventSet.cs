using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystems.Events
{
	[System.Serializable]
	public sealed class GameEventSet
	{
		//=====================================================================================================================//
		//================================================= Public Properties ==================================================//
		//=====================================================================================================================//

		#region Public Properties

		public bool doRaise;
		public List<GameEventReference> events;

		#endregion
		
		//=====================================================================================================================//
		//=================================================== Public Methods ==================================================//
		//=====================================================================================================================//

		#region Public Methods

		public void Raise()
		{
			for (int i = 0; i < events.Count; i++) {
				events[i].Raise();
			}
		}

		public bool IsValid()
		{
			if (events == null || events.Count == 0) {
				return false;
			}

			for (int i = 0; i < events.Count; i++) {
				if (!events[i].IsValid()) {				
					return false;
				}
			}

			return true;
		}

		#endregion		
	}
}