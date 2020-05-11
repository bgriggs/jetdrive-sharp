using System;
using System.Collections.Generic;
using System.Text;

namespace JetdriveSharp
{
	public class KLHDVMessageReceivedArgs : EventArgs
	{
		public InboundKLHDVMessage Message
		{
			get; private set;
		}

		public KLHDVMessageReceivedArgs(InboundKLHDVMessage msg)
		{
			this.Message = msg;
		}
	}
}
