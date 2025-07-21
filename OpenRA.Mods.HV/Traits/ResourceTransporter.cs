#region Copyright & License Information
/*
 * Copyright 2019-2025 The OpenHV Developers (see CREDITS)
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
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.HV.Activities;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	public class ResourceTransporterInfo : TraitInfo, Requires<MobileInfo>
	{
		public readonly HashSet<string> DeliveryBuildings = [];

		[Desc("How much resources it can carry.")]
		public readonly int Capacity = 10;

		[VoiceReference]
		public readonly string DeliverVoice = "Action";

		[CursorReference]
		[Desc("Cursor to display when able to unload at target actor.")]
		public readonly string EnterCursor = "enter";

		[CursorReference]
		[Desc("Cursor to display when unable to unload at target actor.")]
		public readonly string EnterBlockedCursor = "enter-blocked";

		[Desc("Avoid enemy actors nearby when searching for a new delivery route. Should be somewhere near the max weapon range.")]
		public readonly WDist EnemyAvoidanceRadius = WDist.FromCells(8);

		[Desc("Minimum delay (in ticks) between searching for destinations.")]
		public readonly int IdleScanDelay = 20;

		public override object Create(ActorInitializer init) { return new ResourceTransporter(init.Self, this); }
	}

	public class ResourceTransporter : IIssueOrder, IResolveOrder, IOrderVoice, INotifyCreated, INotifyIdle
	{
		public readonly ResourceTransporterInfo Info;

		public string ResourceType;
		public Actor LinkedCollector;
		public int[] Multipliers;

		readonly Mobile mobile;

		int scanForDestinationTicks;

		public ResourceTransporter(Actor self, ResourceTransporterInfo info)
		{
			Info = info;

			mobile = self.Trait<Mobile>();
		}

		void ReturnHome(Actor self)
		{
			var linked = LinkedCollector != null && !LinkedCollector.IsDead && LinkedCollector.Owner == self.Owner;
			var safeReturn = string.IsNullOrEmpty(ResourceType) && linked;
			var destination = safeReturn ? LinkedCollector : ClosestDestination(self);
			if (destination == null)
				return;

			var target = Target.FromActor(destination);
			self.QueueActivity(new TransportResources(self, target, Info.Capacity, Multipliers, ResourceType, LinkedCollector));
		}

		void INotifyCreated.Created(Actor self)
		{
			// Wait for resource type to be set.
			self.World.AddFrameEndTask(w => ReturnHome(self));
		}

		public Actor ClosestDestination(Actor self)
		{
			var actors = string.IsNullOrEmpty(ResourceType)
			 ? self.World.ActorsHavingTrait<ResourceCollector>().Where(r => r.Owner == self.Owner).ToList()
			 : self.World.ActorsHavingTrait<AcceptsDeliveredResources>().Where(r => r.Owner == self.Owner).ToList();

			var path = mobile.PathFinder.FindPathToTargetCell(self, actors.Select(a => a.Location), mobile.ToCell, BlockedByActor.None,
				location => self.World.FindActorsInCircle(self.World.Map.CenterOfCell(location), Info.EnemyAvoidanceRadius)
					.Where(u => !u.IsDead && self.Owner.RelationshipWith(u.Owner) == PlayerRelationship.Enemy)
					.Sum(u => Math.Max(WDist.Zero.Length, Info.EnemyAvoidanceRadius.Length - (self.World.Map.CenterOfCell(location) - u.CenterPosition).Length)));

			if (path.Count > 0)
				return actors.First(r => r.Location == path[^1]);

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

		Order IIssueOrder.IssueOrder(Actor self, IOrderTargeter order, in Target target, bool queued)
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

				self.QueueActivity(order.Queued, new TransportResources(self, order.Target, Info.Capacity, Multipliers, ResourceType, LinkedCollector));
				self.ShowTargetLines();
			}
		}

		void INotifyIdle.TickIdle(Actor self)
		{
			if (--scanForDestinationTicks > 0)
				return;

			scanForDestinationTicks = Info.IdleScanDelay;

			ReturnHome(self);
		}
	}
}
