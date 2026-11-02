#region Copyright & License Information
/*
 * Copyright 2019-2026 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Orders;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.HV.Activities;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Can move actors instantly to primary designated teleport network actor.")]
	public class TeleportNetworkTransportableInfo : TraitInfo
	{
		[VoiceReference]
		public readonly string Voice = "Action";

		[CursorReference]
		public readonly string EnterCursor = "enter";

		[CursorReference]
		public readonly string EnterBlockedCursor = "enter-blocked";

		[Desc("Automatically enter the teleport network when the rally point of the producing building is on an entrance.")]
		public readonly bool EnterOnRallyPoint = true;

		public override object Create(ActorInitializer init) { return new TeleportNetworkTransportable(this, init); }
	}

	public class TeleportNetworkTransportable : IIssueOrder, IResolveOrder, IOrderVoice, INotifyCreated
	{
		readonly TeleportNetworkTransportableInfo info;
		readonly CPos[] rallyPoint;

		public TeleportNetworkTransportable(TeleportNetworkTransportableInfo info, ActorInitializer init)
		{
			this.info = info;
			rallyPoint = init.GetOrDefault<RallyPointInit>()?.Value;
		}

		void INotifyCreated.Created(Actor self)
		{
			if (!info.EnterOnRallyPoint || rallyPoint == null || rallyPoint.Length == 0)
				return;

			var entrance = self.World.ActorMap.GetActorsAt(rallyPoint[^1])
				.FirstOrDefault(a => a.TraitOrDefault<TeleportNetwork>() != null);

			if (entrance == null)
				return;

			var teleportNetwork = entrance.Trait<TeleportNetwork>();
			if (!teleportNetwork.IsTraitEnabled() || !entrance.IsValidTeleportNetworkUser(self) || !HasEnoughCanals(entrance, teleportNetwork.Info.Type))
				return;

			// Queued after the creation activity that moves the actor to the rally point.
			self.QueueActivity(new EnterTeleportNetwork(self, Target.FromActor(entrance), teleportNetwork.Info.Type));
		}

		public IEnumerable<IOrderTargeter> Orders
		{
			get { yield return new TeleportNetworkTransportOrderTargeter(info); }
		}

		public Order IssueOrder(Actor self, IOrderTargeter order, in Target target, bool queued)
		{
			if (order.OrderID != "TeleportNetworkTransport")
				return null;

			return new Order(order.OrderID, self, target, queued);
		}

		// Checks if targeted actor's owner has enough canals (more than 1) of provided type
		static bool HasEnoughCanals(Actor target, string type)
		{
			var managers = target.Owner.PlayerActor.TraitsImplementing<TeleportNetworkManager>().FirstOrDefault(x => x.Type == type);
			if (managers == null)
				return false;

			return managers.Count > 1;
		}

		static bool IsValidOrder(Order order)
		{
			// Not targeting a frozen actor
			if (order.Target.Actor == null)
				return false;

			var teleportNetwork = order.Target.Actor.TraitOrDefault<TeleportNetwork>();
			if (teleportNetwork == null)
				return false;

			if (!HasEnoughCanals(order.Target.Actor, teleportNetwork.Info.Type))
				return false;

			return !order.Target.Actor.IsPrimaryTeleportNetworkExit();
		}

		public string VoicePhraseForOrder(Actor self, Order order)
		{
			return order.OrderString == "TeleportNetworkTransport" && IsValidOrder(order)
				? info.Voice : null;
		}

		public void ResolveOrder(Actor self, Order order)
		{
			if (order.OrderString != "TeleportNetworkTransport" || !IsValidOrder(order))
				return;

			if (order.Target.Type != TargetType.Actor)
				return;

			var teleportNetwork = order.Target.Actor.TraitOrDefault<TeleportNetwork>();
			if (teleportNetwork == null)
				return;

			if (!order.Queued)
				self.CancelActivity();

			self.QueueActivity(new EnterTeleportNetwork(self, order.Target, teleportNetwork.Info.Type));
		}

		public class TeleportNetworkTransportOrderTargeter : UnitOrderTargeter
		{
			readonly TeleportNetworkTransportableInfo info;

			public TeleportNetworkTransportOrderTargeter(TeleportNetworkTransportableInfo info)
				: base("TeleportNetworkTransport", 6, info.EnterCursor, true, true)
			{
				this.info = info;
			}

			public override bool CanTargetActor(Actor self, Actor target, TargetModifiers modifiers, ref string cursor)
			{
				if (modifiers.HasFlag(TargetModifiers.ForceAttack))
					return false;

				// Valid enemy TeleportNetwork entrances should still be offered to be destroyed first.
				if (self.Owner.RelationshipWith(target.Owner) == PlayerRelationship.Enemy && !modifiers.HasFlag(TargetModifiers.ForceMove))
					return false;

				var trait = target.TraitOrDefault<TeleportNetwork>();
				if (trait == null || !trait.IsTraitEnabled())
					return false;

				if (!target.IsValidTeleportNetworkUser(self)) // block, if primary exit.
					cursor = info.EnterBlockedCursor;

				return true;
			}

			public override bool CanTargetFrozenActor(Actor self, FrozenActor target, TargetModifiers modifiers, ref string cursor)
			{
				// You can't enter frozen actor.
				return false;
			}
		}
	}
}
