using System;
using System.Collections.Generic;
using System.Text;

namespace JetdriveSharp
{
	public class JDChannelInfo
	{
		public byte vendor_specific;
		public String channelName;
		public JDUnit unit;

		public JDChannelInfo(String channelName, JDUnit unit)
		{
			this.vendor_specific = 0;
			this.channelName = channelName;
			this.unit = unit;
		}

		public JDChannelInfo()
		{
		}


	}
}
