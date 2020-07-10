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

using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Activities
{
	class TransportResources : Enter
	{
		readonly int payload;
		readonly ResourceTypeInfo typeInfo;

		public TransportResources(Actor self, Target target, int payload, ResourceTypeInfo typeInfo)
			: base(self, target, Color.Yellow)
		{
			this.payload = payload;
			this.typeInfo = typeInfo;
		}

		protected override void OnEnterComplete(Actor self, Actor targetActor)
		{
			var targetOwner = targetActor.Owner;
			var resources = targetOwner.PlayerActor.Trait<PlayerResources>();

			var initialAmount = resources.Resources;
			var value = typeInfo.ValuePerUnit * payload;
			resources.GiveResources(value);
			var amount = resources.Resources - initialAmount;

			if (self.Owner.IsAlliedWith(self.World.RenderPlayer))
				self.World.AddFrameEndTask(w => w.Add(new FloatingText(targetActor.CenterPosition, targetOwner.Color, FloatingText.FormatCashTick(amount), 30)));

			self.Dispose();
		}
	}
}
