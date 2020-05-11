using System;
using System.Collections.Generic;
using System.Text;

namespace JetdriveSharp
{
	public readonly struct JDChannelSample
	{
		public UInt16 ChannelID
		{
			get;
		}

		public UInt32 TS
		{
			get;
		}

		public float Value
		{
			get;
		}


		public JDChannelSample(UInt16 id, UInt32 ts, float value)
		{
			this.ChannelID = id;
			this.TS = ts;
			this.Value = value;
		}

	}
}
