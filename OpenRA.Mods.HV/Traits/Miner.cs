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

using System.Collections.Frozen;
using System.Collections.Generic;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.HV.Activities;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Allows the miner to deploy at a target location with a single order.")]
	public class MinerInfo : TraitInfo
	{
		[CursorReference]
		[Desc("Cursor to display when able to deploy a tower.")]
		public readonly string DeployCursor = "deploy";

		[FieldLoader.Require]
		[Desc("Terrain types that can be targeted for deployment.")]
		public readonly FrozenSet<string> TerrainTypes = FrozenSet<string>.Empty;

		[VoiceReference]
		[Desc("Voice to use when deploying into a tower.")]
		public readonly string Voice = "Action";

		[Desc("Defines to which players the target lines are shown.")]
		public readonly FrozenDictionary<string, Color> Colors = FrozenDictionary<string, Color>.Empty;

		public override object Create(ActorInitializer init) { return new Miner(this, init.Self); }
	}

	public class Miner : IIssueOrder, IResolveOrder, IOrderVoice
	{
		public readonly MinerInfo Info;
		readonly IResourceLayer resourceLayer;

		public Miner(MinerInfo info, Actor self)
		{
			Info = info;
			resourceLayer = self.World.WorldActor.Trait<IResourceLayer>();
		}

		IEnumerable<IOrderTargeter> IIssueOrder.Orders
		{
			get
			{
				yield return new DeployMinerOrderTargeter();
			}
		}

		Order IIssueOrder.IssueOrder(Actor self, IOrderTargeter order, in Target target, bool queued)
		{
			switch (order.OrderID)
			{
				case "DeployMiner":
					var targetPosition = self.World.Map.CellContaining(target.CenterPosition);
					return new Order("DeployMiner", self, Target.FromCell(self.World, targetPosition), queued);
				default:
					return null;
			}
		}

		void IResolveOrder.ResolveOrder(Actor self, Order order)
		{
			if (order.OrderString != "DeployMiner")
				return;

			var cell = self.World.Map.CellContaining(order.Target.CenterPosition);
			var targetLineColor = Info.Colors[resourceLayer.GetResource(cell).Type];
			if (IsCellAcceptable(self, cell))
				self.QueueActivity(order.Queued, new DeployMiner(self, cell, Info.TerrainTypes, targetLineColor));

			self.ShowTargetLines();
		}

		string IOrderVoice.VoicePhraseForOrder(Actor self, Order order)
		{
			if (order.OrderString == "DeployMiner")
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

		public class DeployMinerOrderTargeter : IOrderTargeter
		{
			public string OrderID { get { return "DeployMiner"; } }
			public int OrderPriority { get { return 10; } }
			public bool TargetOverridesSelection(Actor self, in Target target, List<Actor> actorsAt, CPos xy, TargetModifiers modifiers) { return true; }

			public bool CanTarget(Actor self, in Target target, ref TargetModifiers modifiers, ref string cursor)
			{
				if (target.Type != TargetType.Terrain)
					return false;

				var location = self.World.Map.CellContaining(target.CenterPosition);
				if (!self.World.Map.Contains(location))
					return false;

				var miner = self.Trait<Miner>();
				if (!miner.IsCellAcceptable(self, location))
					return false;

				cursor = miner.Info.DeployCursor;
				IsQueued = false; // can't undeploy

				return true;
			}

			public bool IsQueued { get; protected set; }
		}
	}
}
