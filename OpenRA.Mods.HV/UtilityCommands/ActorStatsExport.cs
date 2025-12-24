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
using System.Data;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Warheads;

namespace OpenRA.Mods.HV.UtilityCommands
{
	public static class ActorStatsExport
	{
		public static DataTable GenerateTable(ModData modData)
		{
			var table = new DataTable();
			table.Columns.Add("Name", typeof(string));
			table.Columns.Add("Cost", typeof(int));
			table.Columns.Add("Speed", typeof(int));
			table.Columns.Add("HitPoints", typeof(int));
			table.Columns.Add("Armor", typeof(string));
			table.Columns.Add("Weapon", typeof(string));
			table.Columns.Add("Range", typeof(string));
			table.Columns.Add("Damage /s", typeof(int));

			var gameSpeeds = modData.GetOrCreate<GameSpeeds>();
			var defaultGameSpeed = gameSpeeds.Speeds[gameSpeeds.DefaultSpeed];

			var armorList = new List<string>();
			foreach (var actorInfo in modData.DefaultRules.Actors.Values)
			{
				var armor = actorInfo.TraitInfoOrDefault<ArmorInfo>();
				if (armor != null && !armorList.Contains(armor.Type))
					armorList.Add(armor.Type);
			}

			armorList.Sort();
			foreach (var armorType in armorList)
				table.Columns.Add("vs. " + armorType, typeof(int));

			foreach (var actorInfo in modData.DefaultRules.Actors.Values)
			{
				if (actorInfo.Name.StartsWith('^'))
					continue;

				var buildable = actorInfo.TraitInfoOrDefault<BuildableInfo>();
				if (buildable == null)
					continue;

				var row = table.NewRow();
				var tooltip = actorInfo.TraitInfos<TooltipInfo>().FirstOrDefault();
				row["Name"] = tooltip != null ? tooltip.Name : actorInfo.Name;

				var value = actorInfo.TraitInfoOrDefault<ValuedInfo>();
				row["Cost"] = value != null ? value.Cost : 0;

				var aircraft = actorInfo.TraitInfoOrDefault<AircraftInfo>();
				var mobile = actorInfo.TraitInfoOrDefault<MobileInfo>();
				row["Speed"] = aircraft != null ? aircraft.Speed : mobile != null ? mobile.Speed : 0;

				var health = actorInfo.TraitInfoOrDefault<HealthInfo>();
				row["HitPoints"] = health != null ? health.HP : 0;

				var armor = actorInfo.TraitInfoOrDefault<ArmorInfo>();
				row["Armor"] = armor != null ? armor.Type : "";

				var armaments = actorInfo.TraitInfos<ArmamentInfo>();
				if (armaments.Count > 0)
				{
					foreach (var armament in armaments)
					{
						row["Weapon"] = armament.Weapon;
						var weapon = modData.DefaultRules.Weapons[armament.Weapon.ToLowerInvariant()];

						row["Range"] = weapon.Range;

						foreach (var warhead in weapon.Warheads.Where(w => w is DamageWarhead))
						{
							var damageWarhead = warhead as DamageWarhead;
							var rateOfFire = weapon.ReloadDelay > 1 ? weapon.ReloadDelay : 1;
							var burst = weapon.Burst;
							var damage = damageWarhead.Damage;

							var damagePerSecond = 0f;

							foreach (var delay in weapon.BurstDelays)
								damagePerSecond += 1000f / defaultGameSpeed.Timestep * (damage * burst) / (delay + burst * rateOfFire);

							damagePerSecond /= weapon.BurstDelays.Length;
							row["Damage /s"] = Math.Round(damagePerSecond, 1, MidpointRounding.AwayFromZero);

							foreach (var armorType in armorList)
							{
								var vs = damageWarhead.Versus.TryGetValue(armorType, out var versus) ? versus : 100;
								row["vs. " + armorType] = Math.Round(damagePerSecond * vs / 100, 1, MidpointRounding.AwayFromZero);
							}
						}

						table.Rows.Add(row);
						row = table.NewRow();
					}
				}
			}

			return table;
		}
	}
}
