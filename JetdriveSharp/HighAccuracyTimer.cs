using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace JetdriveSharp
{
	public interface IHighAccuracyTimer
	{
		DateTime BaseTime
		{
			get;
		}

		UInt32 ElapsedMs
		{
			get;
		}

		DateTime Now
		{
			get;
		}
	}


	/// <summary>
	/// Using DateTime.Now or UtcNow typically only has a resolution of 16ms increments, so it's necessary to gain more precision by using a stopwatch on top of a base time. 
	/// </summary>
	public class HighAccuracyTimer : IHighAccuracyTimer
	{
		/// <summary>
		/// The time this timer was created at - ElapsedMs is the time since base time.
		/// </summary>
		public DateTime BaseTime
		{
			get;private set;
		}

		private readonly UInt32 offset = 0;

		private Stopwatch stopWatch = new Stopwatch();

		public HighAccuracyTimer()
		{
			BaseTime = DateTime.Now;
			stopWatch.Start();
		}

		public HighAccuracyTimer(DateTime baseTime, UInt32 offset)
		{
			this.BaseTime = baseTime;
			this.offset = offset;
			stopWatch.Start();
		}

		/// <summary>
		/// Miliseconds elapsed since this timer was created
		/// </summary>
		public UInt32 ElapsedMs
		{
			get
			{
				return (UInt32)stopWatch.ElapsedMilliseconds + offset;
			}
		}

		/// <summary>
		/// Base time plus stopwatch tracked time elapsed since starting.
		/// </summary>
		public DateTime Now
		{
			get
			{
				return BaseTime.AddTicks(stopWatch.Elapsed.Ticks).AddMilliseconds(offset);
			}
		}
	}
}
