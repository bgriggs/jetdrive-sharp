using System;
using System.Collections.Generic;
using System.Text;

namespace JetdriveSharp
{
	[Flags]
	public enum SequenceNumberFlags
	{
		None = 0,
		OUT_OF_ORDER = 0x1,
		PREVIOUS_MESSAGES_DROPPED = 0x2,
		FIRST_MESSAGE = 0x4,
	}
}
