using System;

namespace MultiSync
{
	public static class EventHandlerExtensions
	{
		public static void RaiseEvent<T>(this EventHandler<T> eventHandler, object sender, T args)
		{
			var threadSafeEventHandler = eventHandler;
			if (threadSafeEventHandler != null)
			{
				threadSafeEventHandler(sender, args);
			}
		}

		public static void RaiseEvent(this EventHandler eventHandler, object sender, EventArgs args)
		{
			var threadSafeEventHandler = eventHandler;
			if (threadSafeEventHandler != null)
			{
				threadSafeEventHandler(sender, args);
			}
		}
	}
}