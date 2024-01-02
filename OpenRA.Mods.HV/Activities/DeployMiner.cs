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

using System.Collections.Generic;
using OpenRA.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Activities
{
	public class DeployMiner : Activity
	{
		readonly IMove movement;
		CPos? location;
		readonly HashSet<string> terrainTypes;
		readonly Color targetLineColor;

		public DeployMiner(Actor self, CPos? location, HashSet<string> terrainTypes, Color targetLineColor)
		{
			movement = self.Trait<IMove>();
			this.location = location;
			this.terrainTypes = terrainTypes;
			this.targetLineColor = targetLineColor;
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
				QueueChild(movement.MoveTo(location.Value));
				self.Trait<Transforms>().DeployTransform(true);
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
			return terrainTypes.Contains(self.World.Map.GetTerrainInfo(cell).Type) &&
				!self.World.WorldActor.Trait<BuildingInfluence>().AnyBuildingAt(cell);
		}
	}
}
