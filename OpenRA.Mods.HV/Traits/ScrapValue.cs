#region Copyright & License Information
/*
 * Copyright 2019-2021 The OpenHV Developers (see CREDITS)
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
	public class ScrapValueInfo : TraitInfo
	{
		[Desc("Percentage of the killed actor's value.")]
		public readonly int Percentage = 10;

		[Desc("How much the scrap is worth when pre-placed with the map editor.")]
		public readonly int MinimumFallbackAmount = 40;
		public readonly int MaximumFallbackAmount = 200;

		public override object Create(ActorInitializer init) { return new ScrapValue(init, this); }
	}

	public class ScrapValue
	{
		public int Bounty;

		public ScrapValue(ActorInitializer init, ScrapValueInfo info)
		{
			var value = init.GetOrDefault<ValueInit>(info);
			if (value != null)
				Bounty = value.Value * info.Percentage / 100;
			else
				Bounty = init.Self.World.SharedRandom.Next(info.MinimumFallbackAmount, info.MaximumFallbackAmount);
		}
	}
}
