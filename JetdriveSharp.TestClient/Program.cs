using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using JetdriveSharp;
using JetdriveSharp.Utils;

namespace JetdriveSharp.TestClient
{
	class Program
	{
		static JetdriveProvider provider;

		static volatile bool exit = false;

		static void Main(string[] args)
		{
			Console.CancelKeyPress += Console_CancelKeyPress;

			Console.WriteLine($"JETDRIVE Demo App, V1.0, JETDRIVE V{JetdriveNode.JETDRIVE_VERSION}");

			IPAddress mcastIfaceAddr;
			if (!NetworkUtils.TryGetBestLocalIFAddr(out mcastIfaceAddr))
			{
				Console.WriteLine("Automatic IP address selection failed, reverting to manual selection.");

				//List all addresses on all interfaces and allow the user to choose...
				if (!TryManualIPSelection(out mcastIfaceAddr))
				{
					Console.WriteLine("No address selected, exiting program.");
					return;
				}
			}

			Console.WriteLine($"Using IP address: {mcastIfaceAddr}");

			using (NetworkPort port1 = new NetworkPort())
			{
				//Join our multicast group!
				port1.Join(mcastIfaceAddr);

				//Start listening for incoming traffic.
				port1.StartListening();

				//Just a random number so we can identify each test provider during this debug program
				int providerNum = new Random().Next(1, 256);

				using ((provider = new JetdriveProvider(true, port1, $"Test Provider {providerNum}", new HighAccuracyTimer())))
				{
					//The first thing we need to do is discover other hosts and ensure that we randomly generate a host id that doesn't already exist.
					provider.NegotiateHostId();

					//Listen for stall events so we can react accordingly.
					provider.Stalled += Client1_Stalled;

					//Subscribe for incoming channels (that's what we're here for!)
					provider.ChannelPosted += Client1_ChannelPosted;

					//For logging and UI updates, you want to subscribe to this event - show the user that the provider channel list has been cleared or zero out gauges, etc.
					provider.ChannelsCleared += Provider_ChannelsCleared;

					//We're just starting up, so we should ask for channel info from all hosts... 
					provider.RequestChannelInfo(JetdriveNode.ALL_HOSTS);

					//Begin posting channel values!
					StartPostChannelsThread(provider);

					//Main transmit loop for a provider.
					DateTime lastValuesTime = DateTime.MinValue;
					DateTime lastChanInfosTime = DateTime.MinValue;
					while (!exit)
					{
						//Transmit channel info every 30 seconds as per the spec!
						if (DateTime.UtcNow - lastChanInfosTime >= TimeSpan.FromSeconds(30))
						{
							lastChanInfosTime = DateTime.UtcNow;
							provider.TransmitChannelInfo(JetdriveNode.ALL_HOSTS);
						}

						//Transmit channel values however often you want - I wouldn't recommend going slower than 10hz though as slower values will appear kinda jerky on gauges and might make any realtime code perform poorly if it expects frequent updates.
						if (DateTime.UtcNow - lastValuesTime >= TimeSpan.FromMilliseconds(50))
						{
							bool transmitAgain = false;
							lastValuesTime = DateTime.UtcNow;

							do
							{
								(_, transmitAgain) = provider.TransmitChannelValues();
							} while (transmitAgain);
						}

						//Sleep (approximately) until the next event is supposed to occur
						int timeTillNextEvent = (int)Math.Min((TimeSpan.FromSeconds(30) - (DateTime.UtcNow - lastChanInfosTime)).TotalMilliseconds, (TimeSpan.FromMilliseconds(50) - (DateTime.UtcNow - lastValuesTime)).TotalMilliseconds);

						if (timeTillNextEvent > 0)
						{
							Thread.Sleep(timeTillNextEvent);
						}
						else
						{

						}
					}
				}
			}
		}

		private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			if (!exit)
			{
				e.Cancel = true;
				Console.WriteLine("Terminating.... (Cancel again to kill process)");
				exit = true;
			}
			else
			{
				Process.GetCurrentProcess().Kill();
			}
		}

		static bool TryManualIPSelection(out IPAddress selectedAddr)
		{
			bool selected = false;

			selectedAddr = null;

			List<IPAddress> addresses = new List<IPAddress>();

			int i = 0;
			foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (item.OperationalStatus == OperationalStatus.Up)
				{
					foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
					{
						if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
						{
							String description = $"{item.Name}, {item.NetworkInterfaceType}, {ip.Address}";
							Console.WriteLine($"[{i++}]\t{description}");
							addresses.Add(ip.Address);
						}
					}
				}
			}

			if (addresses.Count > 0)
			{
				tryAgain:
				Console.Write("Please input the index of the interface to use: ");
				String input = Console.ReadLine();
				if (Int32.TryParse(input, out int idx))
				{
					if (idx >= 0 && idx < addresses.Count)
					{
						selectedAddr = addresses[idx];
						selected = true;
					}
					else
					{
						goto tryAgain;
					}
				}
				else
				{
					goto tryAgain;
				}
			}
			else
			{
				Console.WriteLine("No interfaces available");
			}

			return selected;
		}


		private static void Provider_ChannelsCleared(object sender, ClearChannelsEventArgs e)
		{
			Console.WriteLine($"ClearChannels: {e.HostId:X4} (\"{e.HostName}\")");
		}

		private static void StartPostChannelsThread(JetdriveProvider _provider)
		{
			//To demonstrate that channel values can be posted asynchronously via another thread...
			ThreadPool.QueueUserWorkItem((userState) =>
			{
				if (userState is JetdriveProvider provider)
				{
					JDChannelInfo info1 = new JDChannelInfo("Test RPM 1", JDUnit.EngineSpeed);
					JDChannelInfo info2 = new JDChannelInfo("Test Power", JDUnit.Power);

					//Ensure all channels are cleared before we begin transmitting - it's possible another node had our address at one point and didn't clean up before terminating, so we want to clear that state for all other nodes before we get started.
					provider.ClearChannels();

					//Register any channels we want to transmit
					UInt16 id1 = provider.RegisterChannel(info1);
					UInt16 id2 = provider.RegisterChannel(info2);

					//Now that our channels are registered, transmit our channel info, then begin transmitting channels.
					provider.TransmitChannelInfo();

					int i = 0;
					while (true)
					{
						provider.QueueSample(id1, i++, DateTime.Now);
						provider.QueueSample(id2, (float)i * .1f, DateTime.Now);
						Thread.Sleep(100);
					}
				}
			}, _provider);
		}


		private static void Client1_ChannelPosted(object sender, ChannelValuePostedEventArgs e)
		{
			if (e.ChannelInfo != null) // If channel info is null, the provider hasn't transmitted channel info yet.
			{
				Console.WriteLine($"{e.ProviderName}.{e.ChannelInfo.channelName}\t=\t{e.Value:F4} {e.ChannelInfo.unit}\t @ {e.Timestamp:HH:mm:ss:fff}\tFlags={e.Message.Flags}");
			}
			else
			{
				Console.WriteLine($"?? {e.Value} \t @ {e.Timestamp.ToShortTimeString()}");
			}
		}

		private static void Client1_Stalled(object sender, EventArgs e)
		{
			if (sender is JetdriveNode node)
			{
				Console.WriteLine($"Stall: 0x{node.HostId:X4} {node.StallStatus}");

				//If we've collided with another host, renegotiate our host id.
				if (node.StallStatus == StallReason.AddressCollisionDetected)
				{
					node.NegotiateHostId();
				}
				else if (node.StallStatus == StallReason.InvalidVersionDetected)
				{
					Console.WriteLine("Error: Cannot communicate on this JETDRIVE network, at least one other node exists with an incompatible version.");
				}

			}
		}
	}
}
