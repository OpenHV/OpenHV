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

using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Helper trait to set up AI to sell heavily damaged newly controlled actors and repairing them otherwise.")]
	public class BotRepairOrSellCapturesInfo : TraitInfo
	{
		public override object Create(ActorInitializer init) { return new BotRepairOrSellCaptures(); }
	}

	public class BotRepairOrSellCaptures : INotifyOwnerChanged
	{
		public BotRepairOrSellCaptures() { }

		void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
		{
			if (!newOwner.IsBot)
				return;

			var health = self.TraitOrDefault<Health>();
			if (health == null)
				return;

			var sellable = self.TraitOrDefault<Sellable>();
			if (sellable != null && !sellable.IsTraitDisabled && (health.DamageState == DamageState.Heavy || health.DamageState == DamageState.Critical))
			{
				self.World.IssueOrder(new Order("Sell", self, Target.FromActor(self), false));
				return;
			}

			var rb = self.TraitOrDefault<RepairableBuilding>();
			if (rb != null && health.DamageState != DamageState.Undamaged)
				 self.World.IssueOrder(new Order("RepairBuilding", newOwner.PlayerActor, Target.FromActor(self), false));
		}
	}
}
