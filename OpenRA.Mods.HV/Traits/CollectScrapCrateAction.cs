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

using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Gives cash to the collector.")]
	class CollectScrapCrateActionInfo : CrateActionInfo, Requires<ScrapValueInfo>
	{
		[Desc("Should the collected amount be displayed as a cash tick?")]
		public bool UseCashTick = true;

		public override object Create(ActorInitializer init) { return new CollectScrapCrateAction(init.Self, this); }
	}

	class CollectScrapCrateAction : CrateAction
	{
		readonly CollectScrapCrateActionInfo info;

		readonly int bounty;

		public CollectScrapCrateAction(Actor self, CollectScrapCrateActionInfo info)
			: base(self, info)
		{
			this.info = info;

			bounty = self.Trait<ScrapValue>().Bounty;
		}

		public override void Activate(Actor collector)
		{
			collector.World.AddFrameEndTask(w =>
			{
				var amount = collector.Owner.PlayerActor.Trait<PlayerResources>().ChangeCash(bounty);

				if (info.UseCashTick)
					w.Add(new FloatingText(collector.CenterPosition, collector.Owner.Color, FloatingText.FormatCashTick(amount), 30));
			});

			base.Activate(collector);
		}

		public override int GetSelectionShares(Actor collector)
		{
			var pr = collector.Owner.PlayerActor.Trait<PlayerResources>();
			if (bounty < 0 && (pr.Cash + pr.Resources) == 0)
				return 0;

			return base.GetSelectionShares(collector);
		}
	}
}
