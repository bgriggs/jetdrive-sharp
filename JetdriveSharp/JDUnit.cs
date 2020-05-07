using System;
using System.Collections.Generic;
using System.Text;

namespace JetdriveSharp
{
	public enum JDUnit
	{
		/// <summary>
		/// Seconds
		/// </summary>
		Time = 0,

		/// <summary>
		/// meters
		/// </summary>
		Distance,

		/// <summary>
		/// kph
		/// </summary>
		Speed,

		/// <summary>
		/// newtons
		/// </summary>
		Force,

		/// <summary>
		/// kw
		/// </summary>
		Power,

		/// <summary>
		/// newton-meters
		/// </summary>
		Torque,

		/// <summary>
		/// C
		/// </summary>
		Temperature,

		/// <summary>
		/// kpa
		/// </summary>
		Pressure,

		/// <summary>
		/// RPM
		/// </summary>
		EngineSpeed,

		/// <summary>
		/// rpm/kph
		/// </summary>
		GearRatio,

		/// <summary>
		/// kph/s
		/// </summary>
		Acceleration,

		/// <summary>
		/// Air Fuel Ratio
		/// </summary>
		AFR,

		/// <summary>
		/// kg/hr
		/// </summary>
		FlowRate,

		/// <summary>
		/// Lambda
		/// </summary>
		Lambda,

		/// <summary>
		/// volts
		/// </summary>
		Volts,

		/// <summary>
		/// Amperes
		/// </summary>
		Amps,

		/// <summary>
		/// Percentage
		/// </summary>
		Percentage,

		//Reserved for future use...
		/// <summary>
		/// Not used yet, but acceptable - in a later revision, there will be a dynamic units table for this
		/// </summary>
		Extended = 254,

		/// <summary>
		/// No unit specifed/needed (look for unit in name)
		/// </summary>
		NoUnit = 255,
	}
}
