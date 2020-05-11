using System;
using System.Collections.Generic;
using System.Text;

namespace JetdriveSharp
{
	public enum MessageKey : byte
	{
		/// <summary>
		/// A message containing channel information
		/// </summary>
		ChannelInfo = 0x01,

		/// <summary>
		/// A message containing channel values
		/// </summary>
		ChannelValues = 0x02,

		/// <summary>
		/// Tell the destination host to clear any cached channel information
		/// </summary>
		ClearChannelInfo = 0x03,

		Ping = 0x04,

		Pong = 0x05,

		RequestChannelInfo = 0x06,

		//Extended commands, not yet implemented in reference source
		Ext_RequestLogging = 0x10,
		Ext_RequestLoggingResponse = 0x20,
		Ext_RequestLoadControl = 0x11,
		Ext_RequestLoadControlResponse = 0x21,
		Ext_RequestBrake = 0x12,
		Ext_RequestBrakeResponse = 0x22,

	}
}
