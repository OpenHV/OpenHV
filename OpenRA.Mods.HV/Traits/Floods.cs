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

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Lets the actor spread resources around it in straight lines.")]
	class FloodsInfo : ConditionalTraitInfo
	{
		public readonly int Interval = 75;
		public readonly string ResourceType = "water";
		public readonly int MaxRange = 100;

		public override object Create(ActorInitializer init) { return new Floods(init.Self, this); }
	}

	class Floods : ConditionalTrait<FloodsInfo>, ITick
	{
		readonly FloodsInfo info;

		readonly ResourceType resourceType;
		readonly UndergroundResourceLayer resourceLayer;

		readonly List<CPos> cells = new List<CPos>();

		public Floods(Actor self, FloodsInfo info)
			: base(info)
		{
			this.info = info;

			resourceType = self.World.WorldActor.TraitsImplementing<ResourceType>()
				.FirstOrDefault(t => t.Info.Type == info.ResourceType);

			if (resourceType == null)
				throw new InvalidOperationException("No such resource type `{0}`".F(info.ResourceType));

			resourceLayer = self.World.WorldActor.Trait<UndergroundResourceLayer>(); // TODO: think of something else

			cells.Add(self.Location);
		}

		int ticks;

		void ITick.Tick(Actor self)
		{
			if (IsTraitDisabled)
				return;

			if (--ticks <= 0)
			{
				Flood(self);
				ticks = info.Interval;
			}
		}

		public void Flood(Actor self)
		{
			var waveFront = Util.ExpandFootprint(cells, true)
				.Take(info.MaxRange)
				.SkipWhile(p => !self.World.Map.Contains(p) ||
					(resourceLayer.GetResourceType(p) == resourceType && resourceLayer.IsFull(p)))
				.ToArray();

			foreach (var cell in waveFront)
			{
				if (resourceLayer.CanSpawnResourceAt(resourceType, cell))
				{
					cells.Add(cell);
					resourceLayer.AddResource(resourceType, cell, 1);
				}
			}
		}
	}
}
