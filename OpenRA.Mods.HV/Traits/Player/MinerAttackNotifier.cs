#region Copyright & License Information
/*
 * Copyright 2021 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Plays an audio notification and shows a radar ping when a miner is attacked.",
		"Attach this to the player actor.")]
	[TraitLocation(SystemActors.Player)]
	public class MinerAttackNotifierInfo : TraitInfo
	{
		[Desc("Minimum duration (in milliseconds) between notification events.")]
		public readonly int NotifyInterval = 30000;

		public readonly Color RadarPingColor = Color.Red;

		[Desc("Length of time (in ticks) to display a location ping in the minimap.")]
		public readonly int RadarPingDuration = 250;

		[NotificationReference("Speech")]
		[Desc("The audio notification type to play for undeployed miners.")]
		public string MinerNotification = "MinerUnderAttack";

		[NotificationReference("Speech")]
		[Desc("The audio notification type to play for deployed miners.")]
		public string ResourceCollectorNotification = "MiningTowerUnderAttack";

		public override object Create(ActorInitializer init) { return new MinerAttackNotifier(init.Self, this); }
	}

	public class MinerAttackNotifier : INotifyDamage
	{
		readonly RadarPings radarPings;
		readonly MinerAttackNotifierInfo info;

		long lastAttackTime;

		public MinerAttackNotifier(Actor self, MinerAttackNotifierInfo info)
		{
			radarPings = self.World.WorldActor.TraitOrDefault<RadarPings>();
			this.info = info;
			lastAttackTime = -info.NotifyInterval;
		}

		void INotifyDamage.Damaged(Actor self, AttackInfo e)
		{
			// Don't track self-damage
			if (e.Attacker != null && e.Attacker.Owner == self.Owner)
				return;

			var notification = string.Empty;
			if (self.Info.HasTraitInfo<MinerInfo>())
				notification = info.MinerNotification;

			if (self.Info.HasTraitInfo<ResourceCollectorInfo>())
				notification = info.ResourceCollectorNotification;

			if (string.IsNullOrEmpty(notification))
				return;

			if (Game.RunTime > lastAttackTime + info.NotifyInterval)
			{
				Game.Sound.PlayNotification(self.World.Map.Rules, self.Owner, "Speech", notification, self.Owner.Faction.InternalName);
				radarPings?.Add(() => self.Owner.IsAlliedWith(self.World.RenderPlayer), self.CenterPosition, info.RadarPingColor, info.RadarPingDuration);

				lastAttackTime = Game.RunTime;
			}
		}
	}
}
