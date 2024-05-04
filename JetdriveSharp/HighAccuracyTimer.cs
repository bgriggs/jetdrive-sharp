﻿/*
 
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
using System.Diagnostics;

namespace JetdriveSharp;

public interface IHighAccuracyTimer
{
    DateTime BaseTime
    {
        get;
    }

    uint ElapsedMs
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
        get; private set;
    }

    private readonly uint offset = 0;

    private readonly Stopwatch stopWatch = new();

    public HighAccuracyTimer()
    {
        BaseTime = DateTime.Now;
        stopWatch.Start();
    }

    public HighAccuracyTimer(DateTime baseTime, uint offset)
    {
        this.BaseTime = baseTime;
        this.offset = offset;
        stopWatch.Start();
    }

    /// <summary>
    /// Milliseconds elapsed since this timer was created
    /// </summary>
    public uint ElapsedMs
    {
        get
        {
            return (uint)stopWatch.ElapsedMilliseconds + offset;
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
