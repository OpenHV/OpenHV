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

using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	// TODO: Create a proper check for Types of TeleportNetwork and TeleportNetworkManager or lint rule.
	[Desc("This actor can teleport actors like Nydus canels in SC1. Assuming static object.")]
	public class TeleportNetworkInfo : ITraitInfo
	{
		[FieldLoader.Require]
		[Desc("Type of TeleportNetwork that pairs up, in order for it to work.")]
		public string Type;

		[Desc("Stances requirement that targeted TeleportNetwork has to meet in order to teleport units.")]
		public Stance ValidStances = Stance.Ally;

		public object Create(ActorInitializer init) { return new TeleportNetwork(init, this); }
	}

	// The teleport network does nothing. The actor teleports itself, upon entering.
	public class TeleportNetwork : INotifyAddedToWorld, INotifyRemovedFromWorld, INotifyOwnerChanged
	{
		public TeleportNetworkInfo Info;
		TeleportNetworkManager teleportNetworkManager;

		public TeleportNetwork(ActorInitializer init, TeleportNetworkInfo info)
		{
			Info = info;
		}

		void IncreaseTeleportNetworkCount(Actor self, Player owner)
		{
			if (teleportNetworkManager.Count == 0)
			{
				var primary = self.TraitOrDefault<TeleportNetworkPrimaryExit>();

				if (primary == null)
					return;

				primary.SetPrimary(self);
			}

			teleportNetworkManager.Count++;
		}

		void DecreaseTeleportNetworkCount(Actor self, Player owner)
		{
			teleportNetworkManager.Count--;

			if (self.IsPrimaryTeleportNetworkExit())
			{
				var actors = self.World.ActorsWithTrait<TeleportNetworkPrimaryExit>()
				.Where(a => a.Actor.Owner == self.Owner && a.Actor != self);

				if (!actors.Any())
					teleportNetworkManager.PrimaryActor = null;
				else
				{
					var pri = actors.First().Actor;
					pri.Trait<TeleportNetworkPrimaryExit>().SetPrimary(pri);
				}
			}
		}

		void INotifyAddedToWorld.AddedToWorld(Actor self)
		{
			teleportNetworkManager = self.Owner.PlayerActor.TraitsImplementing<TeleportNetworkManager>().First(x => x.Type == Info.Type);
			IncreaseTeleportNetworkCount(self, self.Owner);
		}

		public void OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
		{
			DecreaseTeleportNetworkCount(self, oldOwner);
			IncreaseTeleportNetworkCount(self, newOwner);
		}

		void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
		{
			DecreaseTeleportNetworkCount(self, self.Owner);
		}
	}
}
