#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Orders;
using OpenRA.Mods.HV.Activities;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Provides access to the attack-move command, which will make the actor automatically engage viable targets while moving to the destination.")]
	sealed class AutoDeployInfo : TraitInfo, Requires<IMoveInfo>
	{
		[VoiceReference]
		public readonly string Voice = "Action";

		[FieldLoader.Require]
		[Desc("Terrain types that can be targeted for deployment.")]
		public readonly HashSet<string> TerrainTypes = new();

		[Desc("Color to use for the target line.")]
		public readonly Color TargetLineColor = Color.OrangeRed;

		[Desc("Avoid enemy actors nearby when searching for a new delivery route. Should be somewhere near the max weapon range.")]
		public readonly WDist EnemyAvoidanceRadius = WDist.FromCells(8);

		public override object Create(ActorInitializer init) { return new AutoDeploy(init.Self, this); }
	}

	sealed class AutoDeploy : IResolveOrder, IOrderVoice
	{
		public readonly AutoDeployInfo Info;
		readonly IMove move;

		public AutoDeploy(Actor self, AutoDeployInfo info)
		{
			move = self.Trait<IMove>();
			Info = info;
		}

		string IOrderVoice.VoicePhraseForOrder(Actor self, Order order)
		{
			if (order.OrderString == "AutoDeploy")
				return Info.Voice;

			return null;
		}

		public bool IsCellAcceptable(Actor self, CPos cell)
		{
			if (!self.World.Map.Contains(cell))
				return false;

			if (Info.TerrainTypes.Count == 0)
				return true;

			var terrainType = self.World.Map.GetTerrainInfo(cell).Type;
			return Info.TerrainTypes.Contains(terrainType);
		}

		public void ResolveOrder(Actor self, Order order)
		{
			if (order.OrderString == "AutoDeploy" || order.OrderString == "AttackMove" || order.OrderString == "AttackMoveActivity" || order.OrderString == "AssaultMove")
			{

				var cell = self.World.Map.CellContaining(order.Target.CenterPosition);
			    bool IsValidResource(CPos cell) => IsCellAcceptable(self, cell);

				var mobile =self.Trait<Mobile>();

			    var path = mobile.PathFinder.FindPathToTargetCellByPredicate(
				self, new[] { cell}, IsValidResource, BlockedByActor.Stationary,
				location => self.World.FindActorsInCircle(self.World.Map.CenterOfCell(cell), Info.EnemyAvoidanceRadius)
					.Where(u => !u.IsDead && self.Owner.RelationshipWith(u.Owner) == PlayerRelationship.Enemy)
					.Sum(u => Math.Max(WDist.Zero.Length, Info.EnemyAvoidanceRadius.Length - (self.World.Map.CenterOfCell(cell) - u.CenterPosition).Length)));

				var target = path.Count == 0 ? Target.Invalid : Target.FromCell(self.World, path[0]);
                var deployOrder = new Order("DeployMiner", self, target, false);
				var oreCell = self.World.Map.CellContaining(deployOrder.Target.CenterPosition);
				self.QueueActivity(order.Queued, new DeployMiner(self, oreCell, Info.TerrainTypes, Info.TargetLineColor));
			    self.ShowTargetLines();

			}
		}
	}
}
