#region Copyright & License Information
/*
 * Copyright 2021-2023 The OpenHV Developers (see CREDITS)
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
	[TraitLocation(SystemActors.Player)]
	[Desc("Plays an audio notification and shows a radar ping when a building (except resource towers) is attacked.",
		"Attach this to the player actor.")]
	public class BuildingAttackNotifierInfo : TraitInfo
	{
		[Desc("Minimum duration (in milliseconds) between notification events.")]
		public readonly int NotifyInterval = 30000;

		public readonly Color RadarPingColor = Color.Red;

		[Desc("Length of time (in ticks) to display a location ping in the minimap.")]
		public readonly int RadarPingDuration = 250;

		[NotificationReference("Speech")]
		[Desc("The audio notification type to play.")]
		public string Notification = "BaseAttack";

		[NotificationReference("Speech")]
		[Desc("The audio notification to play to allies when under attack.",
			"Won't play a notification to allies if this is null.")]
		public string AllyNotification = null;

		public override object Create(ActorInitializer init) { return new BuildingAttackNotifier(init.Self, this); }
	}

	public class BuildingAttackNotifier : INotifyDamage
	{
		readonly RadarPings radarPings;
		readonly BuildingAttackNotifierInfo info;

		long lastAttackTime;

		public BuildingAttackNotifier(Actor self, BuildingAttackNotifierInfo info)
		{
			radarPings = self.World.WorldActor.TraitOrDefault<RadarPings>();
			this.info = info;
			lastAttackTime = -info.NotifyInterval;
		}

		void INotifyDamage.Damaged(Actor self, AttackInfo e)
		{
			if (e.Attacker == null)
				return;

			if (e.Attacker.Owner == self.Owner)
				return;

			if (e.Attacker == self.World.WorldActor)
				return;

			if (!self.Info.HasTraitInfo<BuildingInfo>() || self.Info.HasTraitInfo<ResourceCollectorInfo>())
				return;

			if (e.Attacker.Owner.IsAlliedWith(self.Owner) && e.Damage.Value <= 0)
				return;

			if (Game.RunTime > lastAttackTime + info.NotifyInterval)
			{
				var rules = self.World.Map.Rules;
				Game.Sound.PlayNotification(rules, self.Owner, "Speech", info.Notification, self.Owner.Faction.InternalName);

				if (info.AllyNotification != null)
					foreach (var player in self.World.Players)
						if (player != self.Owner && player.IsAlliedWith(self.Owner) && player != e.Attacker.Owner)
							Game.Sound.PlayNotification(rules, player, "Speech", info.AllyNotification, player.Faction.InternalName);

				radarPings?.Add(() => self.Owner.IsAlliedWith(self.World.RenderPlayer), self.CenterPosition, info.RadarPingColor, info.RadarPingDuration);

				lastAttackTime = Game.RunTime;
			}
		}
	}
}
