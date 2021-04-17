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
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	static class TeleportNetworkPrimaryExitExts
	{
		public static bool IsValidTeleportNetworkUser(this Actor network, Actor user)
		{
			var trait = network.TraitOrDefault<TeleportNetwork>();
			if (trait == null)
				return false;

			var exit = network.TraitOrDefault<TeleportNetworkPrimaryExit>();
			if (exit != null && exit.IsPrimary)
				return false;

			return network.Owner.RelationshipWith(user.Owner).HasFlag(trait.Info.ValidStances);
		}

		public static bool IsPrimaryTeleportNetworkExit(this Actor network)
		{
			var exit = network.TraitOrDefault<TeleportNetworkPrimaryExit>();
			if (exit == null)
				return false;

			return exit.IsPrimary;
		}
	}

	[Desc("Used with TeleportNetwork trait for primary exit designation.")]
	public class TeleportNetworkPrimaryExitInfo : TraitInfo, Requires<TeleportNetworkInfo>
	{
		[GrantedConditionReference]
		[Desc("The condition to grant to self while this is the primary building.")]
		public readonly string PrimaryCondition = "primary";

		[NotificationReference("Speech")]
		[Desc("The speech notification to play when selecting a primary exit.")]
		public readonly string SelectionNotification = null;

		[CursorReference]
		[Desc("Cursor to display when setting the primary building.")]
		public readonly string Cursor = "deploy";

		public override object Create(ActorInitializer init) { return new TeleportNetworkPrimaryExit(init.Self, this); }
	}

	public class TeleportNetworkPrimaryExit : IIssueOrder, IResolveOrder
	{
		readonly TeleportNetworkPrimaryExitInfo info;
		readonly TeleportNetworkManager manager;
		int primaryToken = Actor.InvalidConditionToken;

		public bool IsPrimary { get; private set; }

		public TeleportNetworkPrimaryExit(Actor self, TeleportNetworkPrimaryExitInfo info)
		{
			this.info = info;
			var trait = self.Info.TraitInfoOrDefault<TeleportNetworkInfo>();
			manager = self.Owner.PlayerActor.TraitsImplementing<TeleportNetworkManager>().Where(x => x.Type == trait.Type).First();
		}

		public IEnumerable<IOrderTargeter> Orders
		{
			get { yield return new DeployOrderTargeter("TeleportNetworkPrimaryExit", 1, () => info.Cursor); }
		}

		Order IIssueOrder.IssueOrder(Actor self, IOrderTargeter order, in Target target, bool queued)
		{
			if (order.OrderID == "TeleportNetworkPrimaryExit")
				return new Order(order.OrderID, self, false);

			return null;
		}

		public void ResolveOrder(Actor self, Order order)
		{
			// You can NEVER unselect a primary teleport network building.
			if (order.OrderString == "TeleportNetworkPrimaryExit")
				SetPrimary(self);
		}

		public void RevokePrimary(Actor self)
		{
			IsPrimary = false;

			if (primaryToken != Actor.InvalidConditionToken)
				primaryToken = self.RevokeCondition(primaryToken);
		}

		public void SetPrimary(Actor self)
		{
			IsPrimary = true;

			var primary = manager.PrimaryActor;
			if (primary != null && !primary.IsDead)
				primary.Trait<TeleportNetworkPrimaryExit>().RevokePrimary(primary);

			manager.PrimaryActor = self;

			primaryToken = self.GrantCondition(info.PrimaryCondition);
			Game.Sound.PlayNotification(self.World.Map.Rules, self.Owner, "Speech", info.SelectionNotification, self.Owner.Faction.InternalName);
		}
	}
}
