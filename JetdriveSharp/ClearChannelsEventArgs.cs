using System;
using System.Collections.Generic;
using System.Text;

namespace JetdriveSharp
{
	public delegate void ClearChannelsHandler(Object sender, ClearChannelsEventArgs e);

	public class ClearChannelsEventArgs : EventArgs
	{
		public UInt16 HostId
		{
			get;private set;
		}

		public String HostName
		{
			get;private set;
		}

		public ClearChannelsEventArgs(UInt16 hostId, String hostName)
		{
			this.HostId = hostId;
			this.HostName = hostName;
		}

	}
}
