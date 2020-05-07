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

		public DateTime TimestampUTC
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

		public ChannelValuePostedEventArgs(JDChannelInfo info, String providerName, DateTime timestampUtc, float value, InboundKLHDVMessage msg)
		{
			this.ChannelInfo = info;
			this.ProviderName = providerName;
			this.TimestampUTC = timestampUtc;
			this.Value = value;
			this.Message = msg;
		}



	}
}
