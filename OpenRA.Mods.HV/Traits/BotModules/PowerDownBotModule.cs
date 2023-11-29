#region Copyright & License Information
/*
 * Copyright 2019-2023 The OpenHV Developers (see CREDITS)
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
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Manages AI temporary power shutdowns.")]
	public class PowerDownBotModuleInfo : ConditionalTraitInfo
	{
		[Desc("Delay (in ticks) between toggling powerdown.")]
		public readonly int Interval = 150;

		public override object Create(ActorInitializer init) { return new PowerDownBotModule(init.Self, this); }
	}

	public class PowerDownBotModule : ConditionalTrait<PowerDownBotModuleInfo>, IBotTick, IGameSaveTraitData
	{
		readonly World world;
		readonly Player player;

		PowerManager playerPower;
		int toggleTick;

		readonly Func<Actor, bool> isToggledBuildingsValid;

		// We keep a list to track toggled buildings for performance.
		List<BuildingPowerWrapper> toggledBuildings = new();

		sealed class BuildingPowerWrapper
		{
			public int ExpectedPowerChanging;
			public Actor Actor;

			public BuildingPowerWrapper(Actor actor, int powerChanging)
			{
				Actor = actor;
				ExpectedPowerChanging = powerChanging;
			}
		}

		public PowerDownBotModule(Actor self, PowerDownBotModuleInfo info)
			: base(info)
		{
			world = self.World;
			player = self.Owner;

			isToggledBuildingsValid = a => a != null && a.Owner == self.Owner && !a.IsDead && a.IsInWorld;
		}

		protected override void Created(Actor self)
		{
			playerPower = self.Owner.PlayerActor.TraitOrDefault<PowerManager>();
		}

		protected override void TraitEnabled(Actor self)
		{
			toggleTick = world.LocalRandom.Next(Info.Interval);
		}

		static int GetTogglePowerChanging(Actor actor)
		{
			var powerChangingIfToggled = 0;
			var powerTraits = actor.TraitsImplementing<Power>().Where(t => !t.IsTraitDisabled).ToArray();
			if (powerTraits.Length > 0)
			{
				var powerMultipliers = actor.TraitsImplementing<PowerMultiplier>().ToArray();
				powerChangingIfToggled = powerTraits.Sum(p => p.Info.Amount) * (powerMultipliers.Sum(p => p.Info.Modifier) - 100) / 100;
				if (powerMultipliers.Any(t => !t.IsTraitDisabled))
					powerChangingIfToggled = -powerChangingIfToggled;
			}

			return powerChangingIfToggled;
		}

		IEnumerable<Actor> GetToggleableBuildings(IBot bot)
		{
			var toggleable = bot.Player.World.ActorsHavingTrait<ToggleConditionOnOrder>(t => !t.IsTraitDisabled && !t.IsTraitPaused)
				.Where(a => a != null && !a.IsDead && a.Owner == player && a.Info.HasTraitInfo<PowerInfo>() && a.Info.HasTraitInfo<PowerMultiplierInfo>() && a.Info.HasTraitInfo<BuildingInfo>());

			return toggleable;
		}

		IEnumerable<BuildingPowerWrapper> GetOnlineBuildings(IBot bot)
		{
			var toggleableBuildings = new List<BuildingPowerWrapper>();

			foreach (var actor in GetToggleableBuildings(bot))
			{
				var powerChanging = GetTogglePowerChanging(actor);
				if (powerChanging > 0)
					toggleableBuildings.Add(new BuildingPowerWrapper(actor, powerChanging));
			}

			return toggleableBuildings.OrderBy(bpw => bpw.ExpectedPowerChanging);
		}

		void IBotTick.BotTick(IBot bot)
		{
			if (toggleTick > 0 || playerPower == null)
			{
				toggleTick--;
				return;
			}

			var power = playerPower.ExcessPower;
			var togglingBuildings = new List<Actor>();

			// When there is extra power, check if AI can toggle on
			if (power > 0)
			{
				toggledBuildings = toggledBuildings.Where(bpw => isToggledBuildingsValid(bpw.Actor)).OrderByDescending(bpw => bpw.ExpectedPowerChanging).ToList();
				for (var i = 0; i < toggledBuildings.Count; i++)
				{
					var building = toggledBuildings[i];
					if (power + building.ExpectedPowerChanging < 0)
						continue;

					togglingBuildings.Add(building.Actor);
					power += building.ExpectedPowerChanging;
					toggledBuildings.RemoveAt(i);
				}
			}

			// When there is no power, check if AI can toggle off
			// and add those toggled to list for toggling on
			else if (power < 0)
			{
				var buildingsCanBeOff = GetOnlineBuildings(bot);
				foreach (var building in buildingsCanBeOff)
				{
					if (power > 0)
						break;

					togglingBuildings.Add(building.Actor);
					toggledBuildings.Add(new BuildingPowerWrapper(building.Actor, -building.ExpectedPowerChanging));
					power += building.ExpectedPowerChanging;
				}
			}

			if (togglingBuildings.Count > 0)
				bot.QueueOrder(new Order("PowerDown", null, false, groupedActors: togglingBuildings.ToArray()));

			toggleTick = Info.Interval;
		}

		List<MiniYamlNode> IGameSaveTraitData.IssueTraitData(Actor self)
		{
			if (IsTraitDisabled)
				return null;

			var data = new List<MiniYamlNode>();
			foreach (var building in toggledBuildings.Where(bpw => isToggledBuildingsValid(bpw.Actor)))
				data.Add(new MiniYamlNode(FieldSaver.FormatValue(building.Actor.ActorID), FieldSaver.FormatValue(building.ExpectedPowerChanging)));

			return new List<MiniYamlNode>()
			{
				new("ToggledBuildings", new MiniYaml("", data))
			};
		}

		void IGameSaveTraitData.ResolveTraitData(Actor self, MiniYaml data)
		{
			if (self.World.IsReplay)
				return;

			var toggledBuildingsNode = data.NodeWithKeyOrDefault("ToggledBuildings");
			if (toggledBuildingsNode != null)
			{
				foreach (var node in toggledBuildingsNode.Value.Nodes)
				{
					var actor = self.World.GetActorById(FieldLoader.GetValue<uint>(node.Key, node.Key));
					if (isToggledBuildingsValid(actor))
						toggledBuildings.Add(new BuildingPowerWrapper(actor, FieldLoader.GetValue<int>(node.Key, node.Value.Value)));
				}
			}
		}
	}
}
