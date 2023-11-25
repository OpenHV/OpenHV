#region Copyright & License Information
/*
 * Copyright 2021-2022 The OpenHV Developers (see AUTHORS)
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
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[TraitLocation(SystemActors.Player)]
	[Desc("Put this on the Player actor. Manages cube collection.")]
	public class CubePickupBotModuleInfo : ConditionalTraitInfo
	{
		[Desc("Actor types that should not start hunting for cubes.")]
		public readonly HashSet<string> ExcludedUnitTypes = new();

		[Desc("Only these actor types should start hunting for cubes.")]
		public readonly HashSet<string> IncludedUnitTypes = new();

		[Desc("Interval (in ticks) between giving out orders to idle units.")]
		public readonly int ScanForCubesInterval = 50;

		[Desc("Only move this far away from base. Disabled if set to zero.")]
		public readonly int MaxProximityRadius = 0;

		[Desc("Avoid enemy actors nearby when searching for cubes. Should be somewhere near the max weapon range.")]
		public readonly WDist EnemyAvoidanceRadius = WDist.FromCells(8);

		[Desc("Should visibility (Shroud, Fog, Cloak, etc) be considered when searching for cubes?")]
		public readonly bool CheckTargetsForVisibility = true;

		public override object Create(ActorInitializer init) { return new CubePickupBotModule(init.Self, this); }
	}

	public class CubePickupBotModule : ConditionalTrait<CubePickupBotModuleInfo>, IBotTick
	{
		readonly World world;
		readonly Player player;
		readonly int maxProximity;

		CubeSpawner cubeSpawner;

		int scanForcubesTicks;

		readonly List<Actor> alreadyPursuitcubes = new();

		public CubePickupBotModule(Actor self, CubePickupBotModuleInfo info)
			: base(info)
		{
			world = self.World;
			player = self.Owner;

			maxProximity = Info.MaxProximityRadius > 0 ? info.MaxProximityRadius : world.Map.Grid.MaximumTileSearchRange;
		}

		protected override void Created(Actor self)
		{
			cubeSpawner = self.Owner.World.WorldActor.TraitOrDefault<CubeSpawner>();
		}

		protected override void TraitEnabled(Actor self)
		{
			scanForcubesTicks = Info.ScanForCubesInterval;
		}

		void IBotTick.BotTick(IBot bot)
		{
			if (cubeSpawner == null || !cubeSpawner.IsTraitEnabled() || !cubeSpawner.Enabled)
				return;

			if (--scanForcubesTicks > 0)
				return;

			scanForcubesTicks = Info.ScanForCubesInterval;

			var cubes = world.ActorsHavingTrait<Collectible>().ToList();
			if (cubes.Count < 1)
				return;

			if (Info.CheckTargetsForVisibility)
				cubes.RemoveAll(c => !c.CanBeViewedByPlayer(player));

			var idleUnits = world.ActorsHavingTrait<Mobile>().Where(a => a.Owner == player && a.IsIdle
				&& (Info.IncludedUnitTypes.Contains(a.Info.Name) || (Info.IncludedUnitTypes.Count < 1 && !Info.ExcludedUnitTypes.Contains(a.Info.Name)))).ToList();

			if (idleUnits.Count < 1)
				return;

			foreach (var cube in cubes)
			{
				if (alreadyPursuitcubes.Contains(cube))
					continue;

				if (!cube.IsAtGroundLevel())
					continue;

				var cubeCollector = idleUnits.ClosestToIgnoringPath(cube);
				if (cubeCollector == null)
					continue;

				if ((cube.Location - cubeCollector.Location).Length > maxProximity)
					continue;

				idleUnits.Remove(cubeCollector);

				var target = PathToNextcube(cubeCollector, cube);
				if (target.Type == TargetType.Invalid)
					continue;

				var cell = world.Map.CellContaining(target.CenterPosition);
				AIUtils.BotDebug($"{bot.Player}: Ordering {cubeCollector} to {cell} for cube pick up.");
				bot.QueueOrder(new Order("Move", cubeCollector, target, true));
				alreadyPursuitcubes.Add(cube);
			}
		}

		Target PathToNextcube(Actor collector, Actor cube)
		{
			var mobile = collector.Trait<Mobile>();
			var path = mobile.PathFinder.FindPathToTargetCell(
				collector, new[] { collector.Location }, cube.Location, BlockedByActor.Stationary,
				location => world.FindActorsInCircle(world.Map.CenterOfCell(location), Info.EnemyAvoidanceRadius)
					.Where(u => !u.IsDead && collector.Owner.RelationshipWith(u.Owner) == PlayerRelationship.Enemy)
					.Sum(u => Math.Max(WDist.Zero.Length, Info.EnemyAvoidanceRadius.Length - (world.Map.CenterOfCell(location) - u.CenterPosition).Length)));

			if (path.Count == 0)
				return Target.Invalid;

			// Don't use the actor to avoid invalid targets when the cube disappears midway.
			return Target.FromCell(world, cube.Location);
		}
	}
}
