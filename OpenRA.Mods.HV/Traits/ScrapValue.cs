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

using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("How much the unit leftovers are worth.")]
	public class ScrapValueInfo : ITraitInfo
	{
		[Desc("Percentage of the killed actor's value.")]
		public readonly int Percentage = 10;

		public object Create(ActorInitializer init) { return new ScrapValue(init, this); }
	}

	public class ScrapValue
	{
		public int Bounty;

		public ScrapValue(ActorInitializer init, ScrapValueInfo info)
		{
			Bounty = init.Get<ValueInit>().Value(init.World) * info.Percentage / 100;
		}
	}
}
