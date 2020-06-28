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
using OpenRA.Mods.Common.Orders;
using OpenRA.Mods.Common.Pathfinder;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.HV.Activities;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	public class ResourceTransporterInfo : TraitInfo, Requires<MobileInfo>
	{
		public readonly HashSet<string> DeliveryBuildings = new HashSet<string>();

		[Desc("How long (in ticks) to wait until (re-)checking for a nearby available DeliveryBuilding if not yet linked to one.")]
		public readonly int SearchForDeliveryBuildingDelay = 125;

		[Desc("How much resources it can carry.")]
		public readonly int Capacity = 1000;

		[Desc("Automatically scan for delivery buildings when created.")]
		public readonly bool SearchOnCreation = true;

		[VoiceReference]
		public readonly string DeliverVoice = "Action";

		[Desc("Cursor to display when able to unload at target actor.")]
		public readonly string EnterCursor = "enter";

		[Desc("Cursor to display when unable to unload at target actor.")]
		public readonly string EnterBlockedCursor = "enter-blocked";

		[Desc("Avoid enemy actors nearby when searching for a new delivery route. Should be somewhere near the max weapon range.")]
		public readonly WDist EnemyAvoidanceRadius = WDist.FromCells(8);

		public override object Create(ActorInitializer init) { return new ResourceTransporter(init.Self, this); }
	}

	public class ResourceTransporter : IIssueOrder, IResolveOrder, IOrderVoice, INotifyCreated
	{
		public readonly ResourceTransporterInfo Info;
		public readonly IReadOnlyDictionary<ResourceTypeInfo, int> Contents;

		readonly Mobile mobile;

		public ResourceTransporter(Actor self, ResourceTransporterInfo info)
		{
			Info = info;

			mobile = self.Trait<Mobile>();
		}

		void INotifyCreated.Created(Actor self)
		{
			// Note: This is queued in a FrameEndTask because otherwise the activity is dropped/overridden while moving out of a factory.
			if (Info.SearchOnCreation)
			{
				var target = Target.FromActor(ClosestDestination(self));
				self.World.AddFrameEndTask(w => self.QueueActivity(new TransportResources(self, target, Info.Capacity)));
			}
		}

		public Actor ClosestDestination(Actor self)
		{
			var refineries = self.World.ActorsWithTrait<AcceptsDeliveredResources>()
				.Where(r => r.Actor.Owner == self.Owner);

			List<CPos> path;

			using (var search = PathSearch.FromPoints(self.World, mobile.Locomotor, self, refineries.Select(r => r.Actor.Location), self.Location, BlockedByActor.None)
				.WithCustomCost(loc => self.World.FindActorsInCircle(self.World.Map.CenterOfCell(loc), Info.EnemyAvoidanceRadius)
					.Where(u => !u.IsDead && self.Owner.Stances[u.Owner] == Stance.Enemy)
					.Sum(u => Math.Max(WDist.Zero.Length, Info.EnemyAvoidanceRadius.Length - (self.World.Map.CenterOfCell(loc) - u.CenterPosition).Length))))
				path = self.World.WorldActor.Trait<IPathFinder>().FindPath(search);

			if (path.Count != 0)
				return refineries.First(r => r.Actor.Location == path.Last()).Actor;

			return null;
		}

		IEnumerable<IOrderTargeter> IIssueOrder.Orders
		{
			get
			{
				yield return new EnterAlliedActorTargeter<AcceptsDeliveredResourcesInfo>(
					"Deliver",
					5,
					Info.EnterCursor,
					Info.EnterBlockedCursor,
					(x, y) => true,
					_ => true);
			}
		}

		Order IIssueOrder.IssueOrder(Actor self, IOrderTargeter order, Target target, bool queued)
		{
			if (order.OrderID == "Deliver")
				return new Order(order.OrderID, self, target, queued);

			return null;
		}

		string IOrderVoice.VoicePhraseForOrder(Actor self, Order order)
		{
			if (order.OrderString == "Deliver")
				return Info.DeliverVoice;

			return null;
		}

		void IResolveOrder.ResolveOrder(Actor self, Order order)
		{
			if (order.OrderString == "Deliver")
			{
				// Deliver orders are only valid for own/allied actors,
				// which are guaranteed to never be frozen.
				if (order.Target.Type != TargetType.Actor)
					return;

				var targetActor = order.Target.Actor;
				var accepts = targetActor.TraitOrDefault<AcceptsDeliveredResources>();
				if (accepts == null)
					return;

				self.QueueActivity(order.Queued, new TransportResources(self,  order.Target, Info.Capacity));
				self.ShowTargetLines();
			}
		}
	}
}
