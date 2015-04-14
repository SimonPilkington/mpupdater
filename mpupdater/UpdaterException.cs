using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mpupdater
{
	[Serializable()]
	public abstract class UpdaterException : System.Exception
	{
		protected UpdaterException() : base() { }
		protected UpdaterException(string message) : base(message) { }
		protected UpdaterException(string message, System.Exception inner) : base(message, inner) { }

		protected UpdaterException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	[Serializable()]
	public class UpdateCheckException : UpdaterException
	{
		public UpdateCheckException() : base() { }
		public UpdateCheckException(string message) : base(message) { }
		public UpdateCheckException(string message, System.Exception inner) : base(message, inner) { }

		protected UpdateCheckException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	[Serializable()]
	public class SevenZipException : UpdaterException
	{
		public SevenZipException() : base() { }
		public SevenZipException(string message) : base(message) { }
		public SevenZipException(string message, System.Exception inner) : base(message, inner) { }

		protected SevenZipException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}

	[Serializable()]
	public class ServerRegException : UpdaterException
	{
		public ServerRegException() : base() { }
		public ServerRegException(string message) : base(message) { }
		public ServerRegException(string message, System.Exception inner) : base(message, inner) { }

		protected ServerRegException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
