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
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Lets the actor generate resources in a set periodic time.")]
	public class ResourceCollectorInfo : PausableConditionalTraitInfo, Requires<BuildingInfo>
	{
		[Desc("Number of ticks to wait between gathering resources.")]
		public readonly int Interval = 50;

		[Desc("Number of ticks to wait before gathering first resources.")]
		public readonly int InitialDelay = 50;

		[Desc("Amount of resource to collect each time.")]
		public readonly int Amount = 100;

		[Desc("How much can be temporarily stored.")]
		public readonly int Capacity = 1000;

		[ActorReference(typeof(ResourceTransporterInfo))]
		public readonly string[] DeliveryVehicleType = null;

		public override object Create(ActorInitializer init) { return new ResourceCollector(init.Self, this); }
	}

	public class ResourceCollector : PausableConditionalTrait<ResourceCollectorInfo>, ITick, ISync
	{
		readonly ResourceCollectorInfo info;
		readonly Lazy<RallyPoint> rallyPoint;
		readonly ResourceType resourceType;

		public int Ticks { get; private set; }

		[Sync]
		public int Resources { get; private set; }

		public ResourceCollector(Actor self, ResourceCollectorInfo info)
			: base(info)
		{
			this.info = info;
			Ticks = info.InitialDelay;

			var resourceLayer = self.World.WorldActor.Trait<UndergroundResourceLayer>();

			var building = self.Trait<Building>();
			var cells = building.Info.Tiles(self.Location);
			foreach (var cell in cells)
			{
				if (resourceLayer.GetResourceDensity(cell) > 0)
					resourceType = resourceLayer.GetResourceType(cell);
			}

			rallyPoint = Exts.Lazy(() => self.IsDead ? null : self.TraitOrDefault<RallyPoint>());
		}

		void ITick.Tick(Actor self)
		{
			if (IsTraitDisabled)
				Ticks = info.Interval;

			if (IsTraitPaused || IsTraitDisabled)
				return;

			if (--Ticks < 0)
			{
				Ticks = info.Interval;
				Resources += info.Amount;

				if (Resources >= Info.Capacity)
				{
					var exit = self.Exits().RandomOrDefault(self.World.SharedRandom);
					var vehicle = Info.DeliveryVehicleType.Random(self.World.SharedRandom).ToLowerInvariant();
					var actorInfo = self.World.Map.Rules.Actors[vehicle];
					SpawnDeliveryVehicle(self, actorInfo, exit != null ? exit.Info : null);
					Resources = 0;
				}
			}
		}

		public void SpawnDeliveryVehicle(Actor self, ActorInfo actorInfo, ExitInfo exitInfo)
		{
			var exit = CPos.Zero;
			var exitLocations = new List<CPos>();

			var td = new TypeDictionary();

			if (exitInfo != null && self.OccupiesSpace != null && actorInfo.HasTraitInfo<IOccupySpaceInfo>())
			{
				exit = self.Location + exitInfo.ExitCell;
				var spawn = self.CenterPosition + exitInfo.SpawnOffset;
				var to = self.World.Map.CenterOfCell(exit);

				WAngle initialFacing;
				if (!exitInfo.Facing.HasValue)
				{
					var delta = to - spawn;
					if (delta.HorizontalLengthSquared == 0)
					{
						var fi = actorInfo.TraitInfoOrDefault<IFacingInfo>();
						initialFacing = fi != null ? fi.GetInitialFacing() : WAngle.Zero;
					}
					else
						initialFacing = delta.Yaw;
				}
				else
					initialFacing = exitInfo.Facing.Value;

				exitLocations = rallyPoint.Value != null && rallyPoint.Value.Path.Count > 0 ? rallyPoint.Value.Path : new List<CPos> { exit };

				td.Add(new LocationInit(exit));
				td.Add(new CenterPositionInit(spawn));
				td.Add(new FacingInit(initialFacing));
				td.Add(new OwnerInit(self.Owner));
				if (exitInfo != null)
					td.Add(new CreationActivityDelayInit(exitInfo.ExitDelay));
			}

			self.World.AddFrameEndTask(w =>
			{
				var deliveryVehicle = self.World.CreateActor(actorInfo.Name, td);
				deliveryVehicle.Trait<ResourceTransporter>().ResourceType = resourceType;

				var move = deliveryVehicle.TraitOrDefault<IMove>();
				if (exitInfo != null && move != null)
					foreach (var cell in exitLocations)
						deliveryVehicle.QueueActivity(new Move(deliveryVehicle, cell));
			});
		}
	}
}
