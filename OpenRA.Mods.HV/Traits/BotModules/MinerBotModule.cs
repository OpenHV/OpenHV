#region Copyright & License Information
/*
 * Copyright 2019-2024 The OpenHV Developers (see CREDITS)
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
	[Desc("Manages AI miner deployment logic.")]
	public class MinerBotModuleInfo : ConditionalTraitInfo, NotBefore<IResourceLayerInfo>
	{
		[ActorReference]
		[FieldLoader.Require]
		[Desc("Actor types that can deploy onto resources.")]
		public readonly HashSet<string> DeployableActorTypes = new();

		[Desc("Where to request production of additional deployable actors.")]
		public readonly string VehiclesQueue = "Vehicle";

		[FieldLoader.Require]
		[Desc("Terrain types that can be targeted for deployment.")]
		public readonly HashSet<string> DeployableTerrainTypes = new();

		[ActorReference]
		[FieldLoader.Require]
		[Desc("Actor types that have been deployed onto resources.")]
		public readonly HashSet<string> DeployedActorTypes = new();

		[Desc("Prioritize this many resource towers before building other units.")]
		public readonly int MinimumDeployedActors = 1;

		[Desc("Minimum delay (in ticks) between trying to deploy with DeployableActorTypes.")]
		public readonly int MinimumScanDelay = 20;

		[Desc("Minimum delay (in ticks) after the last search for resources failed.")]
		public readonly int LastSearchFailedDelay = 500;

		[Desc("Avoid enemy actors nearby when searching for a new resource patch. Should be somewhere near the max weapon range.")]
		public readonly WDist EnemyAvoidanceRadius = WDist.FromCells(8);

		public override object Create(ActorInitializer init) { return new MinerBotModule(init.Self, this); }
	}

	public class MinerBotModule : ConditionalTrait<MinerBotModuleInfo>, IBotTick, INotifyActorDisposing
	{
		readonly World world;
		readonly Player player;

		readonly Func<Actor, bool> unitCannotBeOrdered;

		readonly ActorIndex.OwnerAndNamesAndTrait<MobileInfo> mobileMiners;
		readonly ActorIndex.OwnerAndNamesAndTrait<BuildingInfo> towerBuildings;

		int scanForIdleMinersTicks;

		IResourceLayer resourceLayer;

		IBotRequestUnitProduction[] requestUnitProduction;

		sealed class MinerTraitWrapper
		{
			public readonly Actor Actor;
			public readonly Mobile Mobile;
			public readonly Transforms Transforms;

			public MinerTraitWrapper(Actor actor)
			{
				Actor = actor;
				Mobile = actor.Trait<Mobile>();
				Transforms = actor.Trait<Transforms>();
			}
		}

		readonly Dictionary<Actor, MinerTraitWrapper> minerTraits = new();

		public MinerBotModule(Actor self, MinerBotModuleInfo info)
			: base(info)
		{
			world = self.World;
			player = self.Owner;

			unitCannotBeOrdered = a => a.Owner != self.Owner || a.IsDead || !a.IsInWorld;

			mobileMiners = new ActorIndex.OwnerAndNamesAndTrait<MobileInfo>(world, info.DeployableActorTypes, player);
			towerBuildings = new ActorIndex.OwnerAndNamesAndTrait<BuildingInfo>(world, info.DeployedActorTypes, player);
		}

		protected override void Created(Actor self)
		{
			requestUnitProduction = self.Owner.PlayerActor.TraitsImplementing<IBotRequestUnitProduction>().ToArray();
		}

		protected override void TraitEnabled(Actor self)
		{
			// PERF: Avoid all AIs reevaluating assignments on the same tick, randomize their initial evaluation delay.
			scanForIdleMinersTicks = world.LocalRandom.Next(0, Info.MinimumScanDelay);

			resourceLayer = world.WorldActor.TraitOrDefault<IResourceLayer>();
		}

		void IBotTick.BotTick(IBot bot)
		{
			if (resourceLayer == null || resourceLayer.IsEmpty)
				return;

			if (--scanForIdleMinersTicks > 0)
				return;

			scanForIdleMinersTicks = Info.MinimumScanDelay;

			foreach (var toRemove in minerTraits.Keys.Where(unitCannotBeOrdered).ToList())
				minerTraits.Remove(toRemove);

			foreach (var newMiner in world.Actors.Where(a => Info.DeployableActorTypes.Contains(a.Info.Name) && a.Owner == player && !minerTraits.ContainsKey(a)))
				minerTraits[newMiner] = new MinerTraitWrapper(newMiner);

			foreach (var miner in minerTraits)
			{
				if (!miner.Key.IsIdle)
					continue;

				// Tell the idle miner to quit slacking:
				var newSafeResourcePatch = FindNextResource(miner.Key, miner.Value);
				if (newSafeResourcePatch.Type == TargetType.Invalid)
				{
					scanForIdleMinersTicks = Info.LastSearchFailedDelay;
					return;
				}

				var cell = world.Map.CellContaining(newSafeResourcePatch.CenterPosition);
				AIUtils.BotDebug($"{miner.Key.Owner}: {miner.Key} is idle. Ordering to {cell} for deployment.");
				bot.QueueOrder(new Order("DeployMiner", miner.Key, newSafeResourcePatch, false));
			}

			var unitBuilder = requestUnitProduction.FirstOrDefault(Exts.IsTraitEnabled);
			if (unitBuilder == null)
				return;

			var miningTowers = AIUtils.CountActorByCommonName(towerBuildings);
			var minerType = Info.DeployableActorTypes.Random(world.LocalRandom);
			if (miningTowers < Info.MinimumDeployedActors && unitBuilder.RequestedProductionCount(bot, minerType) == 0)
				unitBuilder.RequestUnitProduction(bot, minerType);
		}

		Target FindNextResource(Actor actor, MinerTraitWrapper miner)
		{
			var towerInfo = actor.Owner.World.Map.Rules.Actors.Single(a => a.Key == miner.Transforms.Info.IntoActor).Value;
			var buildingInfo = towerInfo.TraitInfo<BuildingInfo>();
			bool IsValidResource(CPos cell) =>
				Info.DeployableTerrainTypes.Contains(world.Map.GetTerrainInfo(cell).Type)
					&& miner.Mobile.Locomotor.CanStayInCell(cell)
					&& world.CanPlaceBuilding(cell + miner.Transforms.Info.Offset, towerInfo, buildingInfo, actor);

			var path = miner.Mobile.PathFinder.FindPathToTargetCellByPredicate(
				actor, new[] { actor.Location }, IsValidResource, BlockedByActor.Stationary,
				location => world.FindActorsInCircle(world.Map.CenterOfCell(location), Info.EnemyAvoidanceRadius)
					.Where(u => !u.IsDead && actor.Owner.RelationshipWith(u.Owner) == PlayerRelationship.Enemy)
					.Sum(u => Math.Max(WDist.Zero.Length, Info.EnemyAvoidanceRadius.Length - (world.Map.CenterOfCell(location) - u.CenterPosition).Length)));

			if (path.Count == 0)
				return Target.Invalid;

			return Target.FromCell(world, path[0]);
		}

		void INotifyActorDisposing.Disposing(Actor self)
		{
			mobileMiners.Dispose();
		}
	}
}
