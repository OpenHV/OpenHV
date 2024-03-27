#region Copyright & License Information
/*
 * Copyright 2019-2024 The OpenHV Developers (see CREDITS)
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
	[Desc("Dropoff point that spawns an unloaded tanker.")]
	public class AcceptsDeliveredResourcesInfo : TraitInfo
	{
		[ActorReference(typeof(ResourceTransporterInfo))]
		public readonly string[] DeliveryVehicleType = null;

		public override object Create(ActorInitializer init) { return new AcceptsDeliveredResources(init.Self, this); }
	}

	public class AcceptsDeliveredResources : INotifyResourceTransport
	{
		readonly Actor self;
		readonly AcceptsDeliveredResourcesInfo info;
		readonly Lazy<RallyPoint> rallyPoint;

		public AcceptsDeliveredResources(Actor self, AcceptsDeliveredResourcesInfo info)
		{
			this.self = self;
			this.info = info;
			rallyPoint = Exts.Lazy(() => self.IsDead ? null : self.TraitOrDefault<RallyPoint>());
		}

		void INotifyResourceTransport.Delivered(Actor sender, Actor receiver)
		{
			var vehicle = info.DeliveryVehicleType.Random(self.World.SharedRandom).ToLowerInvariant();
			var actorInfo = self.World.Map.Rules.Actors[vehicle];
			var exit = self.Exits().RandomOrDefault(self.World.SharedRandom);
			SpawnDeliveryVehicle(self, actorInfo, exit?.Info, sender);
		}

		public void SpawnDeliveryVehicle(Actor self, ActorInfo actorInfo, ExitInfo exitInfo, Actor returning)
		{
			var exit = CPos.Zero;
			var exitLocations = new List<CPos>();

			var typeDictionary = new TypeDictionary();

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
						var facing = actorInfo.TraitInfoOrDefault<IFacingInfo>();
						initialFacing = facing != null ? facing.GetInitialFacing() : WAngle.Zero;
					}
					else
						initialFacing = delta.Yaw;
				}
				else
					initialFacing = exitInfo.Facing.Value;

				exitLocations = rallyPoint.Value != null && rallyPoint.Value.Path.Count > 0 ? rallyPoint.Value.Path : new List<CPos> { exit };

				typeDictionary.Add(new LocationInit(exit));
				typeDictionary.Add(new CenterPositionInit(spawn));
				typeDictionary.Add(new FacingInit(initialFacing));
				typeDictionary.Add(new OwnerInit(self.Owner));
				if (exitInfo != null)
					typeDictionary.Add(new CreationActivityDelayInit(exitInfo.ExitDelay));
			}

			self.World.AddFrameEndTask(w =>
			{
				if (returning.Owner != self.Owner)
					return;

				var deliveryVehicle = self.World.CreateActor(actorInfo.Name, typeDictionary);
				deliveryVehicle.Trait<ResourceTransporter>().LinkedCollector = returning;

				var mobile = deliveryVehicle.TraitOrDefault<Mobile>();
				if (exitInfo != null && mobile != null)
				{
					foreach (var cell in exitLocations)
					{
						if (!mobile.CanEnterCell(cell))
							continue;

						deliveryVehicle.QueueActivity(new Move(deliveryVehicle, cell));
						return;
					}
				}
			});
		}
	}
}
