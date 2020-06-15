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

using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Orders;
using OpenRA.Mods.HV.Activities;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Can move actors instantly to primary designated teleport network canal actor.")]
	class TeleportNetworkTransportableInfo : TraitInfo
	{
		[VoiceReference]
		public readonly string Voice = "Action";
		public readonly string EnterCursor = "enter";
		public readonly string EnterBlockedCursor = "enter-blocked";
		public override object Create(ActorInitializer init) { return new TeleportNetworkTransportable(this); }
	}

	class TeleportNetworkTransportable : IIssueOrder, IResolveOrder, IOrderVoice
	{
		readonly TeleportNetworkTransportableInfo info;

		public TeleportNetworkTransportable(TeleportNetworkTransportableInfo info)
		{
			this.info = info;
		}

		public IEnumerable<IOrderTargeter> Orders
		{
			get { yield return new TeleportNetworkTransportOrderTargeter(info); }
		}

		public Order IssueOrder(Actor self, IOrderTargeter order, Target target, bool queued)
		{
			if (order.OrderID != "TeleportNetworkTransport")
				return null;

			return new Order(order.OrderID, self, target, queued) { };
		}

		// Checks if targeted actor's owner has enough canals (more than 1) of provided type
		static bool HasEnoughCanals(Actor target, string type)
		{
			var managers = target.Owner.PlayerActor.TraitsImplementing<TeleportNetworkManager>().Where(x => x.Type == type).First();
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

		class TeleportNetworkTransportOrderTargeter : UnitOrderTargeter
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
				if (self.Owner.Stances[target.Owner] == Stance.Enemy && !modifiers.HasFlag(TargetModifiers.ForceMove))
					return false;

				var trait = target.TraitOrDefault<TeleportNetwork>();
				if (trait == null)
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
