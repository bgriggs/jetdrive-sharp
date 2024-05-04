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
namespace JetdriveSharp;

[Flags]
public enum SequenceNumberFlags
{
	None = 0,
	OUT_OF_ORDER = 0x1,
	PREVIOUS_MESSAGES_DROPPED = 0x2,
	FIRST_MESSAGE = 0x4,
}
