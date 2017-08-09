using System;
using System.Runtime.Serialization;

namespace XamUBot.Dialogs
{
	[Serializable]
	internal class PopToRootDialogException : Exception
	{
		public PopToRootDialogException()
		{
		}

		public PopToRootDialogException(string message) : base(message)
		{
		}

		public PopToRootDialogException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected PopToRootDialogException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}