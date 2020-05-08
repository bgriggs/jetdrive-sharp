using System;
using System.Collections.Generic;
using System.Text;

namespace JetdriveSharp
{
	public class ChannelValuePostedEventArgs : EventArgs
	{
		public JDChannelInfo ChannelInfo
		{
			get;private set;
		}

		public String ProviderName
		{
			get;private set;
		}

		public DateTime Timestamp
		{
			get;private set;
		}

		public float Value
		{
			get;private set;
		}

		public InboundKLHDVMessage Message
		{
			get;private set;
		}

		public UInt16 ChanID
		{
			get;private set;
		}

		public ChannelValuePostedEventArgs(JDChannelInfo info, String providerName, DateTime timestamp, float value, UInt16 chanId, InboundKLHDVMessage msg)
		{
			this.ChannelInfo = info;
			this.ProviderName = providerName;
			this.Timestamp = timestamp;
			this.Value = value;
			this.Message = msg;
			this.ChanID = chanId;
		}



	}
}
