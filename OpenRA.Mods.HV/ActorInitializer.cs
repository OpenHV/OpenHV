#region Copyright & License Information
/*
 * Copyright 2019-2020 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

namespace OpenRA.Mods.HV
{
	public class ValueInit : IActorInit<int>
	{
		[FieldFromYamlKey]
		readonly int value = 0;

		public ValueInit() { }
		public ValueInit(int init) { value = init; }
		public int Value(World world) { return value; }
	}
}
