#region Copyright & License Information
/*
 * Copyright 2007-2021 The OpenHV Developers (see AUTHORS)
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
using OpenRA.Mods.Common.Pathfinder;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Put this on the Player actor. Manages cube collection.")]
	public class CubePickupBotModuleInfo : ConditionalTraitInfo
	{
		[Desc("Actor types that should not start hunting for cubes.")]
		public readonly HashSet<string> ExcludedUnitTypes = new HashSet<string>();

		[Desc("Only these actor types should start hunting for cubes.")]
		public readonly HashSet<string> IncludedUnitTypes = new HashSet<string>();

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

		IPathFinder pathfinder;
		DomainIndex domainIndex;
		int scanForcubesTicks;

		readonly List<Actor> alreadyPursuitcubes = new List<Actor>();

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
			pathfinder = world.WorldActor.Trait<IPathFinder>();
			domainIndex = world.WorldActor.Trait<DomainIndex>();
			scanForcubesTicks = Info.ScanForCubesInterval;
		}

		void IBotTick.BotTick(IBot bot)
		{
			if (cubeSpawner == null || !cubeSpawner.IsTraitEnabled() || !cubeSpawner.Enabled)
				return;

			if (--scanForcubesTicks > 0)
				return;

			scanForcubesTicks = Info.ScanForCubesInterval;

			var cubes = world.ActorsHavingTrait<Crate>().ToList();
			if (!cubes.Any())
				return;

			if (Info.CheckTargetsForVisibility)
				cubes.RemoveAll(c => !c.CanBeViewedByPlayer(player));

			var idleUnits = world.ActorsHavingTrait<Mobile>().Where(a => a.Owner == player && a.IsIdle
				&& (Info.IncludedUnitTypes.Contains(a.Info.Name) || (!Info.IncludedUnitTypes.Any() && !Info.ExcludedUnitTypes.Contains(a.Info.Name)))).ToList();

			if (!idleUnits.Any())
				return;

			foreach (var cube in cubes)
			{
				if (alreadyPursuitcubes.Contains(cube))
					continue;

				if (!cube.IsAtGroundLevel())
					continue;

				var cubeCollector = idleUnits.ClosestTo(cube);
				if (cubeCollector == null)
					continue;

				if ((cube.Location - cubeCollector.Location).Length > maxProximity)
					continue;

				idleUnits.Remove(cubeCollector);

				var target = PathToNextcube(cubeCollector, cube);
				if (target.Type == TargetType.Invalid)
					continue;

				AIUtils.BotDebug("AI: Ordering unit {0} to {1} for cube pick up.".F(cubeCollector, target));
				bot.QueueOrder(new Order("Move", cubeCollector, target, true));
				alreadyPursuitcubes.Add(cube);
			}
		}

		Target PathToNextcube(Actor collector, Actor cube)
		{
			var locomotor = collector.Trait<Mobile>().Locomotor;

			if (!domainIndex.IsPassable(collector.Location, cube.Location, locomotor))
				return Target.Invalid;

			var path = pathfinder.FindPath(
				PathSearch.FromPoint(world, locomotor, collector, collector.Location, cube.Location, BlockedByActor.Stationary)
					.WithCustomCost(loc => world.FindActorsInCircle(world.Map.CenterOfCell(loc), Info.EnemyAvoidanceRadius)
						.Where(u => !u.IsDead && collector.Owner.RelationshipWith(u.Owner) == PlayerRelationship.Enemy)
						.Sum(u => Math.Max(WDist.Zero.Length, Info.EnemyAvoidanceRadius.Length - (world.Map.CenterOfCell(loc) - u.CenterPosition).Length)))
					.FromPoint(collector.Location));

			if (path.Count == 0)
				return Target.Invalid;

			// Don't use the actor to avoid invalid targets when the cube disappears midway.
			return Target.FromCell(world, cube.Location);
		}
	}
}
