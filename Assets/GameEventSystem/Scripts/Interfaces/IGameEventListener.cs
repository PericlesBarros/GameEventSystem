using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystems.Events
{
	//All classes that implement this interface
	public interface IGameEventListener
	{
		[GameEventHandler]
		void OnEventRaised(object data);		
	}
}