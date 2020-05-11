using System;
using System.Collections.Generic;
using System.Text;

namespace JetdriveSharp
{
	public sealed class InboundKLHDVMessage : KLHDVMessage
	{
		public InboundKLHDVMessage(MessageKey key, ushort host, ushort destination, byte[] value, SequenceNumberFlags flags) : base(key, host, destination, value)
		{
			this.Flags = flags;
		}

		private InboundKLHDVMessage()
		{
		}

		public SequenceNumberFlags Flags
		{
			get;internal set;
		}

		public static InboundKLHDVMessage Decode(byte[] data, int offset)
		{
			int idx = 0;

			InboundKLHDVMessage msg = new InboundKLHDVMessage();
			msg.Key = (MessageKey)data[offset + idx++];

			int length = BitConverter.ToUInt16(data, offset + idx);
			idx += sizeof(UInt16);

			if (offset + idx + length <= data.Length)
			{
				msg.Host = BitConverter.ToUInt16(data, offset + idx);
				idx += sizeof(UInt16);

				msg.SequenceNumber = data[offset + idx++];

				msg.Destination = BitConverter.ToUInt16(data, offset + idx);
				idx += sizeof(UInt16);

				//Now copy in data
				msg.Value = new byte[length];
				Buffer.BlockCopy(data, offset + idx, msg.Value, 0, length);
			}
			else
			{
				throw new IndexOutOfRangeException("Message length is greater than provided buffer length!");
			}

			return msg;
		}


	}
}
