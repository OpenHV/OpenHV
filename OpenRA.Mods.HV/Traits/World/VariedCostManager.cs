#region Copyright & License Information
/*
 * Copyright 2020-2021 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[TraitLocation(SystemActors.World)]
	[Desc("Keeps cost variations in sync. Attach this to the world actor.")]
	public class VariedCostManagerInfo : TraitInfo
	{
		[Desc("Interval between new pricings measured in ticks.")]
		public readonly int AdjustmentDelay = 1000;

		[Desc("Range of percentage modifiers to apply.")]
		public readonly int2 Multiplier = new(100, 125);

		public override object Create(ActorInitializer init) { return new VariedCostManager(init.Self, this); }
	}

	class VariedCostTraitInfoWrapper
	{
		public readonly ActorInfo ActorInfo;
		public readonly VariedCostMultiplierInfo VariedCostMultiplierInfo;

		public VariedCostTraitInfoWrapper(ActorInfo actorInfo)
		{
			ActorInfo = actorInfo;
			VariedCostMultiplierInfo = actorInfo.TraitInfoOrDefault<VariedCostMultiplierInfo>();
		}
	}

	public class VariedCostManager : ITick
	{
		public readonly Dictionary<string, int> CachedCostPercentage = new();

		readonly VariedCostManagerInfo info;

		readonly Dictionary<ActorInfo, VariedCostTraitInfoWrapper> variedCostTraitInfo = new();

		int tick;

		public VariedCostManager(Actor self, VariedCostManagerInfo info)
		{
			this.info = info;

			foreach (var actorInfo in self.World.Map.Rules.Actors.Values)
				variedCostTraitInfo[actorInfo] = new VariedCostTraitInfoWrapper(actorInfo);
		}

		void ITick.Tick(Actor self)
		{
			if (--tick <= 0)
			{
				foreach (var wrapper in variedCostTraitInfo)
				{
					var variedCostMultiplierInfo = wrapper.Value.VariedCostMultiplierInfo;
					if (variedCostMultiplierInfo == null)
						continue;

					var randomPrice = self.World.SharedRandom.Next(info.Multiplier.X, info.Multiplier.Y);
					CachedCostPercentage[variedCostMultiplierInfo.Group ?? wrapper.Key.Name] = randomPrice;
				}

				tick = info.AdjustmentDelay;
			}
		}
	}
}
