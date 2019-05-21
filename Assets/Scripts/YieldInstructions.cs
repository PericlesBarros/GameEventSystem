using System.Collections.Generic;
using UnityEngine;

namespace GameSystems.Utils
{
	public static class YieldInstructions
	{
		//=====================================================================================================================//
		//=================================================== Private Fields ==================================================//
		//=====================================================================================================================//

		#region Private Fields

		private static readonly Dictionary<float, WaitForSeconds> timeInterval = new Dictionary<float, WaitForSeconds>(100);

		#endregion

		//=====================================================================================================================//
		//================================================= Public Properties ==================================================//
		//=====================================================================================================================//

		#region Public Properties

		public static WaitForEndOfFrame EndOfFrame { get; } = new WaitForEndOfFrame();

		public static WaitForFixedUpdate FixedUpdate { get; } = new WaitForFixedUpdate();

		public static WaitForSeconds Get(float seconds)
		{
			if (!timeInterval.ContainsKey(seconds))
				timeInterval.Add(seconds, new WaitForSeconds(seconds));
			
			return timeInterval[seconds];
		}

		#endregion
	}
}