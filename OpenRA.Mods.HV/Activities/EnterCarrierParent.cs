#region Copyright & License Information
/*
 * Copyright 2021 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.HV.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Activities
{
	sealed class EnterCarrierParent : Enter
	{
		readonly Actor parent;
		readonly CarrierParent spawnerParent;

		public EnterCarrierParent(Actor self, Actor parent, CarrierParent spawnerParent)
			: base(self, Target.FromActor(parent))
		{
			this.parent = parent;
			this.spawnerParent = spawnerParent;
		}

		protected override bool TryStartEnter(Actor self, Actor targetActor)
		{
			foreach (var notify in self.TraitsImplementing<INotifyEnterCarrier>())
				notify.Approaching(self, targetActor);

			return base.TryStartEnter(self, targetActor);
		}

		protected override void OnEnterComplete(Actor self, Actor targetActor)
		{
			if (parent.IsDead)
				return;

			foreach (var notify in self.TraitsImplementing<INotifyEnterCarrier>())
				notify.Landed(self, targetActor);

			self.World.AddFrameEndTask(w =>
			{
				if (self.IsDead || parent.IsDead)
					return;

				spawnerParent.PickupChild(parent, self);
				w.Remove(self);

				// Delayed launching is handled at spawner.
				var ammoPools = self.TraitsImplementing<AmmoPool>().ToArray();
				if (ammoPools != null)
					foreach (var pool in ammoPools)
						pool.GiveAmmo(self, pool.Info.Ammo);

				var aircraft = self.TraitOrDefault<Aircraft>();
				aircraft?.RemoveInfluence();
			});
		}
	}
}
