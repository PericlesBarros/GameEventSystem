using System;

namespace GameSystems.Events
{
	[AttributeUsage (AttributeTargets.Method, AllowMultiple = true)]
	public class GameEventHandlerAttribute : Attribute
	{
        /// <inheritdoc />
        /// <summary>
        /// Added to methods to turn them into "SendMessage" candidates
        /// Methods that have this attribute will show in the editor when creating GameEventListeners
        /// </summary>
		public GameEventHandlerAttribute()
		{
		}
	}
}