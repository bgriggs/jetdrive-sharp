using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace JetdriveSharp
{

	/// <summary>
	/// Using DateTime.Now or UtcNow typically only has a resolution of 16ms increments, so it's necessary to gain more precision by using a stopwatch on top of a base time. 
	/// </summary>
	public class HighAccuracyTimer
	{
		/// <summary>
		/// The time this timer was created at - ElapsedMs is the time since base time.
		/// </summary>
		public DateTime BaseTime
		{
			get;private set;
		}

		private Stopwatch stopWatch = new Stopwatch();

		public HighAccuracyTimer()
		{
			BaseTime = DateTime.UtcNow;
			stopWatch.Start();
		}

		/// <summary>
		/// Miliseconds elapsed since this timer was created
		/// </summary>
		public UInt32 ElapsedMs
		{
			get
			{
				return (UInt32)stopWatch.ElapsedMilliseconds;
			}
		}

		/// <summary>
		/// Base time plus stopwatch tracked time elapsed since starting.
		/// </summary>
		public DateTime Now
		{
			get
			{
				return BaseTime.AddTicks(stopWatch.Elapsed.Ticks);
			}
		}
	}
}
