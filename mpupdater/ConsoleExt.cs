using System;
using System.Threading;

namespace mpupdater
{
	public static class ConsoleExt
	{
		public static SingleThreadedExecutionMessageQueue ConsoleMessageQueue { get; set; }

		public static void Invoke(Action a)
		{
			if (ConsoleMessageQueue == null)
				throw new InvalidOperationException("Message queue must be set.");

			if (ConsoleMessageQueue.CheckAccess())
				throw new InvalidOperationException("Invoke called from the message queue thread.");

			var oldSyncContext = SynchronizationContext.Current;

			try
			{
				SynchronizationContext.SetSynchronizationContext(ConsoleMessageQueue.DefaultSynchronizationContext);
				var operation = ConsoleMessageQueue.EnqueueSynchronous(a);

				operation.Wait();
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext(oldSyncContext);
			}
		}

		public static void InvokeAsync(Action a)
		{
			if (ConsoleMessageQueue == null)
				throw new InvalidOperationException("Message queue must be set.");

			var oldSyncContext = SynchronizationContext.Current;

			try
			{
				SynchronizationContext.SetSynchronizationContext(ConsoleMessageQueue.DefaultSynchronizationContext);
				ConsoleMessageQueue.Enqueue(a);
			}
			finally
			{
				SynchronizationContext.SetSynchronizationContext(oldSyncContext);
			}
		}
	}
}
