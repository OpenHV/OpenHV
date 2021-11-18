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

using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.HV.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Activities
{
	class TransportResources : Enter
	{
		readonly int payload;
		readonly string resourceType;
		readonly PlayerResources playerResources;
		readonly Actor spawner;

		public TransportResources(Actor self, Target target, int payload, string resourceType, Actor spawner)
			: base(self, target, Color.Yellow)
		{
			this.payload = payload;
			this.resourceType = resourceType;
			this.spawner = spawner;

			playerResources = self.Owner.PlayerActor.Trait<PlayerResources>();
		}

		protected override void OnEnterComplete(Actor self, Actor targetActor)
		{
			if (!string.IsNullOrEmpty(resourceType))
			{
				var targetOwner = targetActor.Owner;
				var resources = targetOwner.PlayerActor.Trait<PlayerResources>();

				var initialAmount = resources.Resources;
				if (!playerResources.Info.ResourceValues.TryGetValue(resourceType, out var resourceValue))
					return;

				var value = resourceValue * payload;
				resources.GiveCash(value);

				if (self.Owner.IsAlliedWith(self.World.RenderPlayer) && value > 0)
					self.World.AddFrameEndTask(w => w.Add(new FloatingText(targetActor.CenterPosition, targetOwner.Color, FloatingText.FormatCashTick(value), 30)));

				foreach (var notify in targetActor.TraitsImplementing<INotifyResourceTransport>())
					notify.Delivered(spawner, targetActor);
			}

			self.Dispose();
		}
	}
}
