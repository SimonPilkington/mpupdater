using System;

namespace mpupdater
{
	[Serializable()]
	public class UpdaterException : System.Exception
	{
		public UpdaterException() : base() { }
		public UpdaterException(string message) : base(message) { }
		public UpdaterException(string message, System.Exception inner) : base(message, inner) { }

		protected UpdaterException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	[Serializable()]
	public class ServerRegException : System.Exception
	{
		public ServerRegException() : base() { }
		public ServerRegException(string message) : base(message) { }
		public ServerRegException(string message, System.Exception inner) : base(message, inner) { }

		protected ServerRegException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
