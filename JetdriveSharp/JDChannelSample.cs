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
using System;
using System.Collections.Generic;
using System.Text;

namespace JetdriveSharp
{
	public readonly struct JDChannelSample
	{
		public UInt16 ChannelID
		{
			get;
		}

		public UInt32 TS
		{
			get;
		}

		public float Value
		{
			get;
		}


		public JDChannelSample(UInt16 id, UInt32 ts, float value)
		{
			this.ChannelID = id;
			this.TS = ts;
			this.Value = value;
		}

	}
}
