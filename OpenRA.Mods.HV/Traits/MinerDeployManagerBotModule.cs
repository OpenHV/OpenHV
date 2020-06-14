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

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Pathfinder;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Manages AI miner deployment logic.")]
	public class MinerDeployManagerBotModuleInfo : ConditionalTraitInfo
	{
		[FieldLoader.Require]
		[Desc("Actor types that can deploy onto resources.")]
		public readonly HashSet<string> DeployableActorTypes = new HashSet<string>();

		[FieldLoader.Require]
		[Desc("Terrain types that can be targeted for deployment.")]
		public readonly HashSet<string> DeployableTerrainTypes = new HashSet<string>();

		[FieldLoader.Require]
		[Desc("Actor types that have been deployed onto resources.")]
		public readonly HashSet<string> DeployedActorTypes = new HashSet<string>();

		[Desc("Prioritize this many resource towers before building other units.")]
		public readonly int MinimumDeployedActors = 1;

		[Desc("Minimum delay (in ticks) between trying to deploy with DeployableActorTypes.")]
		public readonly int MinimumScanDelay = 375;

		[Desc("Avoid enemy actors nearby when searching for a new resource patch. Should be somewhere near the max weapon range.")]
		public readonly WDist EnemyAvoidanceRadius = WDist.FromCells(8);

		public override object Create(ActorInitializer init) { return new MinerDeployManagerBotModule(init.Self, this); }
	}

	public class MinerDeployManagerBotModule : ConditionalTrait<MinerDeployManagerBotModuleInfo>, IBotTick
	{
		readonly World world;
		readonly Player player;

		readonly Func<Actor, bool> unitCannotBeOrdered;

		int scanForIdleMinersTicks;

		IPathFinder pathfinder;
		DomainIndex domainIndex;
		UndergroundResourceLayer resourceLayer;

		IBotRequestUnitProduction[] requestUnitProduction;

		class MinerTraitWrapper
		{
			public readonly Actor Actor;
			public readonly Locomotor Locomotor;
			public readonly Transforms Transforms;

			public MinerTraitWrapper(Actor actor)
			{
				Actor = actor;
				var mobile = actor.Trait<Mobile>();
				Locomotor = mobile.Locomotor;
				Transforms = actor.Trait<Transforms>();
			}
		}

		readonly Dictionary<Actor, MinerTraitWrapper> miners = new Dictionary<Actor, MinerTraitWrapper>();

		public MinerDeployManagerBotModule(Actor self, MinerDeployManagerBotModuleInfo info)
			: base(info)
		{
			world = self.World;
			player = self.Owner;

			unitCannotBeOrdered = a => a.Owner != self.Owner || a.IsDead || !a.IsInWorld;
		}

		protected override void TraitEnabled(Actor self)
		{
			// PERF: Avoid all AIs reevaluating assignments on the same tick, randomize their initial evaluation delay.
			scanForIdleMinersTicks = world.LocalRandom.Next(0, Info.MinimumScanDelay);

			pathfinder = world.WorldActor.Trait<IPathFinder>();
			domainIndex = world.WorldActor.Trait<DomainIndex>();
			resourceLayer = world.WorldActor.TraitOrDefault<UndergroundResourceLayer>();

			requestUnitProduction = player.PlayerActor.TraitsImplementing<IBotRequestUnitProduction>().ToArray();
		}

		void IBotTick.BotTick(IBot bot)
		{
			if (resourceLayer == null || resourceLayer.IsResourceLayerEmpty)
				return;

			if (--scanForIdleMinersTicks > 0)
				return;

			scanForIdleMinersTicks = Info.MinimumScanDelay;

			var toRemove = miners.Keys.Where(unitCannotBeOrdered).ToList();
			foreach (var a in toRemove)
				miners.Remove(a);

			// TODO: Look for a more performance friendly way to update this list
			var newMiners = world.Actors.Where(a => Info.DeployableActorTypes.Contains(a.Info.Name) && a.Owner == player && !miners.ContainsKey(a));
			foreach (var a in newMiners)
				miners[a] = new MinerTraitWrapper(a);

			foreach (var miner in miners)
			{
				if (!miner.Key.IsIdle)
					continue;

				// Tell the idle harvester to quit slacking:
				var newSafeResourcePatch = FindNextResource(miner.Key, miner.Value);
				AIUtils.BotDebug("AI: Miner {0} is idle. Ordering to {1} in search for new resources.".F(miner.Key, newSafeResourcePatch));
				bot.QueueOrder(new Order("Move", miner.Key, newSafeResourcePatch, true));
				bot.QueueOrder(new Order("DeployTransform", miner.Key, true));
			}

			// Keep the economy running before starving out.
			var unitBuilder = requestUnitProduction.FirstOrDefault(Exts.IsTraitEnabled);
			if (unitBuilder != null)
			{
				var minerInfo = AIUtils.GetInfoByCommonName(Info.DeployableActorTypes, player);
				var miningTowers = AIUtils.CountBuildingByCommonName(Info.DeployedActorTypes, player);
				if (miningTowers < Info.MinimumDeployedActors && unitBuilder.RequestedProductionCount(bot, minerInfo.Name) == 0)
					unitBuilder.RequestUnitProduction(bot, minerInfo.Name);
			}
		}

		Target FindNextResource(Actor actor, MinerTraitWrapper miner)
		{
			var towerInfo = AIUtils.GetInfoByCommonName(Info.DeployedActorTypes, player);
			var buildingInfo = towerInfo.TraitInfo<BuildingInfo>();
			Func<CPos, bool> isValidResource = cell =>
				domainIndex.IsPassable(actor.Location, cell, miner.Locomotor.Info)
					&& Info.DeployableTerrainTypes.Contains(world.Map.GetTerrainInfo(cell).Type)
					&& miner.Locomotor.CanStayInCell(cell)
					&& world.CanPlaceBuilding(cell + miner.Transforms.Info.Offset, towerInfo, buildingInfo, actor);

			var path = pathfinder.FindPath(
				PathSearch.Search(world, miner.Locomotor, actor, BlockedByActor.Stationary, isValidResource)
					.WithCustomCost(loc => world.FindActorsInCircle(world.Map.CenterOfCell(loc), Info.EnemyAvoidanceRadius)
						.Where(u => !u.IsDead && actor.Owner.Stances[u.Owner] == Stance.Enemy)
						.Sum(u => Math.Max(WDist.Zero.Length, Info.EnemyAvoidanceRadius.Length - (world.Map.CenterOfCell(loc) - u.CenterPosition).Length)))
					.FromPoint(actor.Location));

			if (path.Count == 0)
				return Target.Invalid;

			return Target.FromCell(world, path[0]);
		}
	}
}
