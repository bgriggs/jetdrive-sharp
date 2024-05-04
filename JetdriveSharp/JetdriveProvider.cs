/*
 
   Copyright 2020 Dynojet Research Inc

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

 */
using System.Collections.Concurrent;
using System.Text;

namespace JetdriveSharp;

public class JetdriveProvider(bool receiveChannels, NetworkPort port, string providerName, IHighAccuracyTimer timer) : JetdriveClient(port, true, timer)
{
    public volatile int MaxMtu = 1400; // Most networks allow an MTU of 1500 bytes - we'll default to 1400 to give ourselves a little headroom for the KLHDV header and to be a little more likely to support networks that can't do a full 1500 byte MTU

    private readonly List<JDChannelInfo> channels = [];

    private readonly BlockingCollection<JDChannelSample> outboundSamples = new(2048);

    /// <summary>
    /// The name of the provider, which will be output as part of the ChannelInfo message
    /// </summary>
    public string Name
    {
        get; private set;
    } = providerName;

    /// <summary>
    /// Indicates whether the client should read channel info and channel values. 
    /// If this is set to false, the OnMessageReceived base class method (for JetdriveClient) will not be called.
    /// </summary>
    public bool ReceiveChannels
    {
        get; private set;
    } = receiveChannels;

    /// <summary>
    /// Register a channel for output. Next time the ChannelInfo message is transmitted, the channel will be included. 
    /// </summary>
    /// <param name="info"></param>
    /// <returns>The ID of the channel - keep track of this for publishing channels later.</returns>
    public ushort RegisterChannel(JDChannelInfo info)
    {
        lock (channels)
        {
            if (channels.Count < ushort.MaxValue)
            {
                channels.Add(info);
                return (ushort)(channels.Count - 1);
            }
            else
            {
                throw new InvalidOperationException($"Cannot register more than {ushort.MaxValue} channels with a single provider. Create additional providers for more channels.");
            }
        }
    }

    /// <summary>
    /// Clears registered channels from the provider and transmits the ClearChannelInfo message.
    /// </summary>
    /// <param name="chanId"></param>
    /// <returns></returns>
    public void ClearChannels()
    {
        lock (channels)
        {
            channels.Clear();
            while (outboundSamples.TryTake(out _)) // Clear the outbound samples collection to ensure we don't transmit anything now that channels are cleared
                ;
        }

        KLHDVMessage msg = new(MessageKey.ClearChannelInfo, this.HostId, ALL_HOSTS, []);
        Transmit(msg);
    }

    /// <summary>
    /// Enqueue a sample to be transmitted next time TransmitChannelValues is called.
    /// </summary>
    /// <param name="chanId">The ID of the channel to add the sample to</param>
    /// <param name="value">The value to transmit</param>
    /// <param name="localTs">The timestamp relative to the local node time (in UTC), or null to use UTCNow (not very granular, do not recommend)</param>
    /// <returns>True if the sample was queued, false if the channel doesn't exist or the outbound sample buffer is full.</returns>
    public bool QueueSample(ushort chanId, float value, DateTime? localTs)
    {
        //Check to ensure the channel exists in our registered channels collection
        lock (channels)
        {
            if (chanId >= channels.Count)
            {
                return false;
            }
        }

        uint time;
        if (localTs != null)
        {
            time = (uint)(localTs.Value - this.Timer.BaseTime).TotalMilliseconds;
        }
        else
        {
            time = this.Timer.ElapsedMs;
        }

        JDChannelSample sample = new(chanId, time, value);

        return this.outboundSamples.TryAdd(sample);
    }

    /// <summary>
    /// Send channel info to the specified host (recommended practice is to always use ALL_HOSTS, even for specific requests, so that all hosts are updated).
    /// May send multiple packets if the number of registered channels is greater than the MTU for a message can handle
    /// </summary>
    /// <param name="dstHostId">The host to send channel info to</param>
    public void TransmitChannelInfo(ushort dstHostId = ALL_HOSTS)
    {
        const int CHAN_INFO_SIZE = 34;

        int maxChannels = (MaxMtu - PROVIDER_NAME_LEN) / CHAN_INFO_SIZE;

        byte[] providerNameBytes = Encoding.UTF8.GetBytes(Name);

        lock (channels)
        {
            int page = 0;
            int chanCount = 0;
            JDChannelInfo[] infos;
            while ((chanCount = (infos = channels.Skip(page * maxChannels).Take(maxChannels).ToArray()).Length) > 0)
            {
                byte[] buf = new byte[PROVIDER_NAME_LEN + chanCount * 34];

                //Zero out name slot, needs to be null padded
                for (int i = 0; i < PROVIDER_NAME_LEN; ++i)
                {
                    buf[i] = 0;
                }

                //Copy name into name slot
                Buffer.BlockCopy(providerNameBytes, 0, buf, 0, Math.Min(PROVIDER_NAME_LEN, providerNameBytes.Length));

                int dstIdx = PROVIDER_NAME_LEN;
                for (int i = 0; i < chanCount; ++i)
                {
                    ushort chanId = (ushort)(page * maxChannels + i);
                    BitConverter.GetBytes(chanId).CopyTo(buf, dstIdx);
                    dstIdx += sizeof(ushort);

                    //skip vendor byte
                    buf[dstIdx++] = 0;

                    byte[] chanNameBytes = Encoding.UTF8.GetBytes(infos[i].channelName);
                    for (int j = 0; j < CHANNEL_NAME_LEN; ++j)
                    {
                        if (j < chanNameBytes.Length)
                        {
                            buf[dstIdx++] = chanNameBytes[j];
                        }
                        else
                        {
                            buf[dstIdx++] = 0x0; // Null padded as well
                        }
                    }

                    buf[dstIdx++] = (byte)infos[i].unit;
                }

                ++page;

                KLHDVMessage chanInfoMessage = new(MessageKey.ChannelInfo, this.HostId, dstHostId, buf);
                Transmit(chanInfoMessage);
            }
        }
    }

    /// <summary>
    /// Pull channels out of the outbound channel queue and transmit them as a single ChannelValues message block.
    /// Will transmit up to the max number of channels that will fit in a single UDP packet, given the MaxMtu setting.
    /// 
    /// If the limit is reached, the caller of this function should call it again without delay to transmit any remaining channel samples.
    /// 
    /// </summary>
    /// <param name="destinationHost">The host to send the channel values to - in most cases, this should be ALL_HOSTS</param>
    /// <param name="timeout">The amount of time to pend upon the samples queue for any additional samples before transmitting a partially filled message.</param>
    /// <returns>An anonymous tuple containing the number of samples transmitted and a boolean indicating whether the message buffer was filled completely (if true, call this function again)</returns>
    public (int, bool) TransmitChannelValues(ushort destinationHost = ALL_HOSTS, int timeout = 0)
    {
        List<byte>? data = null;

        int chansPerMessage = MaxMtu / CHANNEL_VALUES_SIZE;

        int chanCount = 0;

        bool belowLimit;
        while ((belowLimit = chanCount <= chansPerMessage) && outboundSamples.TryTake(out JDChannelSample sample, timeout))
        {
            data ??= new List<byte>(MaxMtu / 4);

            ++chanCount;

            byte[] buf = BitConverter.GetBytes(sample.ChannelID);
            data.AddRange(buf);

            buf = BitConverter.GetBytes(sample.TS);
            data.AddRange(buf);

            buf = BitConverter.GetBytes(sample.Value);
            data.AddRange(buf);
        }

        if (data != null)
        {
            KLHDVMessage msg = new(MessageKey.ChannelValues, this.HostId, destinationHost, [.. data]);
            Transmit(msg);
        }

        return (chanCount, !belowLimit);
    }

    protected override void OnMessageReceived(NetworkPort? port, InboundKLHDVMessage msg)
    {
        switch (msg.Key)
        {
            case MessageKey.RequestChannelInfo:
                {
                    TransmitChannelInfo(msg.Host);
                    break;
                }
        }

        if (ReceiveChannels)
        {
            base.OnMessageReceived(port, msg);
        }
    }
}
