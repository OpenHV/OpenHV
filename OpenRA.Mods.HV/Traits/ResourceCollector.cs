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
		public readonly int InitialDelay = 200;

		[Desc("Amount of resource to collect each time.")]
		public readonly int Amount = 100;

		[Desc("How much can be temporarily stored.")]
		public readonly int Capacity = 1000;

		[ActorReference(typeof(ResourceTransporterInfo))]
		public readonly string[] DeliveryVehicleType = null;

		[Desc("How much can be mined in total before depletion.")]
		public readonly Dictionary<string, int> Deposits = new Dictionary<string, int>();

		[Desc("Defines to which players the bar is to be shown.")]
		public readonly PlayerRelationship DisplayRelationships = PlayerRelationship.Ally;

		[Desc("Defines to which players the bar is to be shown.")]
		public readonly Dictionary<string, Color> Colors = new Dictionary<string, Color>();

		public override object Create(ActorInitializer init) { return new ResourceCollector(init.Self, this); }
	}

	public class ResourceCollector : PausableConditionalTrait<ResourceCollectorInfo>, ITick, ISync, ISelectionBar
	{
		readonly ResourceCollectorInfo info;
		readonly Actor self;
		readonly Lazy<RallyPoint> rallyPoint;
		readonly IResourceLayer resourceLayer;
		readonly Building building;

		LimitedResources limitedResources;
		string resourceType = null;
		int total;
		int deposit;
		int left;
		int ticks;
		bool depleted;

		[Sync]
		public int Resources { get; private set; }

		public ResourceCollector(Actor self, ResourceCollectorInfo info)
			: base(info)
		{
			this.info = info;
			this.self = self;

			ticks = info.InitialDelay;

			resourceLayer = self.World.WorldActor.Trait<IResourceLayer>();
			building = self.Trait<Building>();

			rallyPoint = Exts.Lazy(() => self.IsDead ? null : self.TraitOrDefault<RallyPoint>());
		}

		protected override void TraitEnabled(Actor self)
		{
			var cells = building.Info.Tiles(self.Location);
			foreach (var cell in cells)
			{
				var resource = resourceLayer.GetResource(cell);
				if (resource.Density > 0)
				{
					resourceType = resource.Type;
					deposit = info.Deposits[resource.Type];
					total = deposit * resource.Density;
					left = total;
				}
			}

			if (deposit > 0)
				foreach (var notify in self.TraitsImplementing<INotifyResourceCollection>())
					notify.Mining(self);

			limitedResources = self.Owner.World.WorldActor.TraitOrDefault<LimitedResources>();
		}

		void ITick.Tick(Actor self)
		{
			if (IsTraitDisabled)
				ticks = info.Interval;

			if (IsTraitPaused || IsTraitDisabled)
				return;

			if (depleted)
				return;

			if (--ticks < 0)
			{
				ticks = info.Interval;
				Resources += info.Amount;
				left -= info.Amount;
				if (left < info.Capacity)
				{
					depleted = true;

					foreach (var notify in self.TraitsImplementing<INotifyResourceCollection>())
						notify.Depletion(self);
				}

				if (limitedResources != null && limitedResources.IsTraitEnabled() && limitedResources.Enabled)
				{
					var density = (int)Math.Round(left / (float)deposit);
					var cells = building.Info.Tiles(self.Location);
					foreach (var cell in cells)
					{
						var resource = resourceLayer.GetResource(cell);
						if (resource.Density > density)
							resourceLayer.RemoveResource(resource.Type, cell);
					}
				}

				if (Resources >= info.Capacity || depleted)
				{
					if (depleted)
					{
						Resources += left;
						left = 0;
					}

					var exit = self.Exits().RandomOrDefault(self.World.SharedRandom);
					var vehicle = Info.DeliveryVehicleType.Random(self.World.SharedRandom).ToLowerInvariant();
					var actorInfo = self.World.Map.Rules.Actors[vehicle];

					if (resourceType != null)
						SpawnDeliveryVehicle(self, actorInfo, exit?.Info, resourceType);

					Resources = 0;
				}
			}
		}

		public void SpawnDeliveryVehicle(Actor self, ActorInfo actorInfo, ExitInfo exitInfo, string resourceType)
		{
			if (string.IsNullOrEmpty(resourceType))
				return;

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
						var facingInfo = actorInfo.TraitInfoOrDefault<IFacingInfo>();
						initialFacing = facingInfo != null ? facingInfo.GetInitialFacing() : WAngle.Zero;
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
				var deliveryVehicle = self.World.CreateActor(actorInfo.Name, typeDictionary);
				deliveryVehicle.Trait<ResourceTransporter>().ResourceType = resourceType;
				deliveryVehicle.Trait<ResourceTransporter>().LinkedCollector = self;

				var move = deliveryVehicle.TraitOrDefault<IMove>();
				if (exitInfo != null && move != null)
					foreach (var cell in exitLocations)
						deliveryVehicle.QueueActivity(new Move(deliveryVehicle, cell));
			});
		}

		bool CantViewSelectionBar()
		{
			var viewer = self.World.RenderPlayer ?? self.World.LocalPlayer;
			return viewer != null && !info.DisplayRelationships.HasRelationship(self.Owner.RelationshipWith(viewer));
		}

		float ISelectionBar.GetValue()
		{
			if (CantViewSelectionBar())
				return 0;

			if (resourceType == null)
				return 0;

			return left / (float)total;
		}

		Color ISelectionBar.GetColor()
		{
			if (resourceType != null)
				return info.Colors[resourceType];

			return Color.White;
		}

		bool ISelectionBar.DisplayWhenEmpty => !CantViewSelectionBar();
	}
}
