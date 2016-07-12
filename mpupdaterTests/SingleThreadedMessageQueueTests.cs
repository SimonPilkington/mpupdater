using Microsoft.VisualStudio.TestTools.UnitTesting;
using mpupdater;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace mpupdaterTests
{
	[TestClass]
	public class SingleThreadedMessageQueueTests
	{
		[TestMethod]
		public void SingleThreadedMessageQueue_SynchronousExceptionPropagation()
		{
			Thread exceptionThrower = null;

			var messageQueue = new SingleThreadedExecutionMessageQueue();

			var testTask = Task.Run(() => messageQueue.DefaultSynchronizationContext.Send((s) =>
			{
				exceptionThrower = Thread.CurrentThread;
				throw new Exception();
			}, null));

			testTask.ContinueWith((t) => messageQueue.TerminateMessageLoop());
			messageQueue.EnterMessageLoop();

			Assert.IsTrue(exceptionThrower == Thread.CurrentThread, "The exception was thrown on the wrong thread.");
			Assert.IsTrue(testTask.IsFaulted, "The exception was not propagated.");
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void SingleThreadedMessageQueue_ThrowOnSendFromQueueThread()
		{
			var messageQueue = new SingleThreadedExecutionMessageQueue();
			messageQueue.DefaultSynchronizationContext.Send((s) => { }, null);
		}
	}
}
