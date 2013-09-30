using System;

namespace MultiSync
{
	public static class EventHandlerExtensions
	{
		public static void RaiseEvent<T>(this EventHandler<T> eventHandler, object sender, T args)
		{
			var threadSafe = eventHandler;
			if (threadSafe != null)
			{
				threadSafe(sender, args);
			}
		}

		public static void RaiseEvent(this EventHandler eventHandler, object sender, EventArgs args)
		{
			var threadSafe = eventHandler;
			if (threadSafe != null)
			{
				threadSafe(sender, args);
			}
		}
	}
}