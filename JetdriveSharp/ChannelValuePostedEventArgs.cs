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
namespace JetdriveSharp;

public class ChannelValuePostedEventArgs(JDChannelInfo? info, string? providerName, DateTime timestamp, float value, ushort chanId, InboundKLHDVMessage msg) : EventArgs
{
    public JDChannelInfo? ChannelInfo
    {
        get; private set;
    } = info;

    public string? ProviderName
    {
        get; private set;
    } = providerName;

    public DateTime Timestamp
    {
        get; private set;
    } = timestamp;

    public float Value
    {
        get; private set;
    } = value;

    public InboundKLHDVMessage Message
    {
        get; private set;
    } = msg;

    public UInt16 ChanID
    {
        get; private set;
    } = chanId;
}
