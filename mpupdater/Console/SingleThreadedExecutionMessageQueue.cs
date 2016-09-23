using System;
using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace mpupdater
{
	public class SingleThreadedExecutionMessageQueue
	{
		public class QueueOperation
		{
			private SendOrPostCallback _d;
			private object _state;

			private ManualResetEventSlim invokeEvent;
			private Thread _responsibleThread;

			public bool Synchronous { get; }
			public ExceptionDispatchInfo ExceptionInfo { get; private set; }

			public QueueOperation(SendOrPostCallback d, object state, Thread responsibleThread, bool synchronous)
			{
				_d = d;
				_state = state;
				_responsibleThread = responsibleThread;

				Synchronous = synchronous;

				if (synchronous)
					invokeEvent = new ManualResetEventSlim(false);
			}

			public void Invoke()
			{
				try
				{
					_d(_state);
				}
				catch (Exception x) when (Synchronous)
				{
					ExceptionInfo = ExceptionDispatchInfo.Capture(x);
				}
				finally
				{
					if (Synchronous)
						invokeEvent.Set();
				}
			}
			
			public void Wait()
			{
				if (!Synchronous)
					throw new InvalidOperationException("Only operations marked as synchronous can be waited on.");

				if (Thread.CurrentThread == _responsibleThread) // That's a deadlock.
					throw new InvalidOperationException("Message queue thread is trying to wait on an operation it is responsible for executing.");

				invokeEvent.Wait();
				invokeEvent.Dispose();

				// If an exception was thrown by the operation, rethrow on the waiting thread.
				ExceptionInfo?.Throw();
			}
		}

		private readonly Thread ownerThread = Thread.CurrentThread;
		private BlockingCollection<QueueOperation> actionQueue = new BlockingCollection<QueueOperation>();

		public SingleThreadSynchronizationContext DefaultSynchronizationContext { get; }

		public SingleThreadedExecutionMessageQueue()
		{
			DefaultSynchronizationContext = new SingleThreadSynchronizationContext(this);
		}

		public void EnterMessageLoop()
		{
			QueueOperation item;
			while (actionQueue.TryTake(out item, Timeout.Infinite))
				item.Invoke();
		}

		public void Enqueue(SendOrPostCallback d, object state)
		{
			var operation = new QueueOperation(d, state, ownerThread, false);
			actionQueue.Add(operation);
		}

		public void Enqueue(Action a)
		{
			var operation = new QueueOperation((s) => a(), null, ownerThread, false);
			actionQueue.Add(operation);
		}

		public QueueOperation EnqueueSynchronous(SendOrPostCallback d, object state)
		{
			var operation = new QueueOperation(d, state, ownerThread, true);
			actionQueue.Add(operation);

			return operation;
		}

		public QueueOperation EnqueueSynchronous(Action a)
		{
			var operation = new QueueOperation((s) => a(), null, ownerThread, true);
			actionQueue.Add(operation);

			return operation;
		}

		public void TerminateMessageLoop()
		{
			actionQueue.CompleteAdding();
		}

		public bool CheckAccess()
		{
			return Thread.CurrentThread == ownerThread;
		}
	}
}
