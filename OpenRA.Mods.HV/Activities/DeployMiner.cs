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

using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Activities;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.HV.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Activities
{
	public class DeployMiner : Activity
	{
		readonly IMove movement;
		CPos? location;
		readonly FrozenSet<string> terrainTypes;
		readonly Color targetLineColor;
		readonly NoBuildZone noBuildZone;
		readonly BuildingInfluence buildingInfluence;

		public DeployMiner(Actor self, CPos? location, FrozenSet<string> terrainTypes, Color targetLineColor)
		{
			movement = self.Trait<IMove>();
			this.location = location;
			this.terrainTypes = terrainTypes;
			this.targetLineColor = targetLineColor;

			noBuildZone = self.World.WorldActor.Trait<NoBuildZone>();
			buildingInfluence = self.World.WorldActor.Trait<BuildingInfluence>();
		}

		protected override void OnFirstRun(Actor self)
		{
			if (location == null)
				location = self.Location;
		}

		public override bool Tick(Actor self)
		{
			if (IsCanceling)
				return true;

			if (CanDeploy(self, location.Value))
			{
				var footprint = noBuildZone.GetMinerFootprintAround(location.Value).ToList();
				foreach (var order in AIUtils.ClearBlockersOrders(footprint, self.Owner))
					self.World.IssueOrder(order);

				QueueChild(movement.MoveTo(location.Value));
				QueueChild(self.Trait<Transforms>().GetTransformActivity());
			}

			return true;
		}

		public override IEnumerable<TargetLineNode> TargetLineNodes(Actor self)
		{
			if (location != null)
				yield return new TargetLineNode(Target.FromCell(self.World, location.Value), targetLineColor);
		}

		bool CanDeploy(Actor self, CPos cell)
		{
			return terrainTypes.Contains(self.World.Map.GetTerrainInfo(cell).Type) && !buildingInfluence.AnyBuildingAt(cell);
		}
	}
}
