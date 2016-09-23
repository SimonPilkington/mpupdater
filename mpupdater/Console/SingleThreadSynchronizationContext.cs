using System;
using System.Threading;

namespace mpupdater
{
	public sealed class SingleThreadSynchronizationContext : SynchronizationContext
	{
		private readonly SingleThreadedExecutionMessageQueue actionMessageQueue;

		public SingleThreadSynchronizationContext(SingleThreadedExecutionMessageQueue messageQueue)
		{
			actionMessageQueue = messageQueue;
		}

		public override void Send(SendOrPostCallback d, object state)
		{
			// This would deadlock. We should know what thread we're on, so this shouldn't happen.
			if (actionMessageQueue.CheckAccess()) 
				throw new InvalidOperationException("Send called from the message queue thread.");

			var operation = actionMessageQueue.EnqueueSynchronous(d, state);

			operation.Wait();
		}

		public override void Post(SendOrPostCallback d, object state)
		{
			actionMessageQueue.Enqueue(d, state);
		}

		public override SynchronizationContext CreateCopy() => this;
	}
}
