#region Copyright & License Information
/*
 * Copyright 2007-2021 The OpenHV Developers (see AUTHORS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Pauses unit production.")]
	public class ProductionThrottleBotModuleInfo : ConditionalTraitInfo
	{
		[Desc("How often in ticks to halt production.")]
		public readonly int Frequency = 1500;

		[Desc("How long in ticks to halt production.")]
		public readonly int Delay = 1000;

		public override object Create(ActorInitializer init) { return new ProductionThrottleBotModule(init.Self, this); }
	}

	public class ProductionThrottleBotModule : ConditionalTrait<ProductionThrottleBotModuleInfo>, IBotTick, IBotRequestPauseUnitProduction
	{
		readonly World world;

		int countdown;

		public ProductionThrottleBotModule(Actor self, ProductionThrottleBotModuleInfo info)
			: base(info)
		{
			world = self.World;
		}

		void IBotTick.BotTick(IBot bot)
		{
			if (world.WorldTick % Info.Frequency == 0)
				countdown = Info.Delay;

			countdown--;
		}

		bool IBotRequestPauseUnitProduction.PauseUnitProduction
		{
			get { return !IsTraitDisabled && countdown > 0; }
		}
	}
}
