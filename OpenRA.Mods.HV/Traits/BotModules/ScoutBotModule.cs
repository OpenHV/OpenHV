#region Copyright & License Information
/*
 * Copyright 2019-2021 The OpenHV Developers (see CREDITS)
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
	[Desc("Manages AI random scouting. Exclude from `SquadManagerBotModule`.")]
	public class ScoutBotModuleInfo : ConditionalTraitInfo
	{
		[FieldLoader.Require]
		[Desc("Actor types that are sent around the map.")]
		public readonly HashSet<string> ScoutActorTypes = new();

		[Desc("Minimum delay (in ticks) between searching for ScoutActorTypes.")]
		public readonly int MinimumScanDelay = 200;

		[Desc("How far away to move from current location.")]
		public readonly int MoveRadius = 1;

		public override object Create(ActorInitializer init) { return new ScoutBotModule(init.Self, this); }
	}

	public class ScoutBotModule : ConditionalTrait<ScoutBotModuleInfo>, IBotTick
	{
		readonly List<Actor> scouts = new();

		readonly World world;
		readonly Player player;
		readonly ScoutBotModuleInfo info;

		readonly Func<Actor, bool> unitCannotBeOrdered;

		int scanForIdleScoutsTicks;

		public ScoutBotModule(Actor self, ScoutBotModuleInfo info)
			: base(info)
		{
			world = self.World;
			player = self.Owner;
			this.info = info;

			unitCannotBeOrdered = a => a.Owner != self.Owner || a.IsDead || !a.IsInWorld;
		}

		protected override void TraitEnabled(Actor self)
		{
			// PERF: Avoid all AIs reevaluating assignments on the same tick, randomize their initial evaluation delay.
			scanForIdleScoutsTicks = world.LocalRandom.Next(0, Info.MinimumScanDelay);
		}

		void IBotTick.BotTick(IBot bot)
		{
			if (--scanForIdleScoutsTicks > 0)
				return;

			scanForIdleScoutsTicks = Info.MinimumScanDelay;

			var toRemove = scouts.Where(unitCannotBeOrdered).ToList();
			foreach (var a in toRemove)
				scouts.Remove(a);

			// TODO: Look for a more performance friendly way to update this list
			var newScouts = world.Actors.Where(a => Info.ScoutActorTypes.Contains(a.Info.Name) && a.Owner == player && !scouts.Contains(a));
			scouts.AddRange(newScouts);

			foreach (var scout in scouts)
			{
				var target = PickTargetLocation(scout);
				if (target.Type == TargetType.Invalid)
					continue;

				bot.QueueOrder(new Order("Move", scout, target, false));
			}
		}

		Target PickTargetLocation(Actor scout)
		{
			var targetPosition = scout.CenterPosition + new WVec(0, -1024 * info.MoveRadius, 0).Rotate(WRot.FromFacing(world.LocalRandom.Next(255)));
			var targetCell = world.Map.CellContaining(targetPosition);

			if (!world.Map.Contains(targetCell))
				return Target.Invalid;

			var target = Target.FromCell(world, targetCell);
			return target;
		}
	}
}
