#region Copyright & License Information
/*
 * Copyright 2024-2025 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Frozen;
using System.Collections.Generic;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[TraitLocation(SystemActors.World | SystemActors.EditorWorld)]
	[Desc("Ensures a miner can always be placed around resource spots.")]
	public class NoBuildZoneInfo : TraitInfo, Requires<IResourceLayerInfo>
	{
		[FieldLoader.Require]
		[Desc("Terrain type used block surrounding cells.")]
		public readonly string BlockedTerrainType = null;

		[FieldLoader.Require]
		[ActorReference(typeof(BuildingInfo))]
		[Desc("Building reference for footprint calculation.")]
		public readonly string MinerActorType = null;

		[Desc("Apply only around these or all if not set.")]
		public readonly FrozenSet<string> ResourceTypes = default;

		public override object Create(ActorInitializer init) { return new NoBuildZone(init.Self, this); }
	}

	public class NoBuildZone : IWorldLoaded
	{
		readonly NoBuildZoneInfo info;
		readonly IResourceLayer resourceLayer;
		readonly BuildingInfo buildingInfo;
		readonly Map map;

		public NoBuildZone(Actor self, NoBuildZoneInfo info)
		{
			this.info = info;
			map = self.World.Map;
			resourceLayer = self.Trait<IResourceLayer>();
			resourceLayer.CellChanged += (cell, _) => CellChanged(cell);
			buildingInfo = map.Rules.Actors[info.MinerActorType].TraitInfo<BuildingInfo>();
		}

		public void WorldLoaded(World w, WorldRenderer wr)
		{
			foreach (var cell in w.Map.AllCells)
			{
				if (ResourceAt(cell))
					AddNoBuildZoneAround(cell);
			}
		}

		public IEnumerable<CPos> GetMinerFootprintAround(CPos cell)
		{
			var location = cell - new CVec(0, buildingInfo.Dimensions.Y - 1);
			return buildingInfo.Tiles(location);
		}

		void CellChanged(CPos cell)
		{
			if (ResourceAt(cell))
				AddNoBuildZoneAround(cell);
			else
				RemoveNoBuildZoneAround(cell);
		}

		void AddNoBuildZoneAround(CPos resourceLocation)
		{
			foreach (var blockedCell in GetMinerFootprintAround(resourceLocation))
			{
				// Don't overwrite invalid terrain with valid custom terrain.
				if (!buildingInfo.TerrainTypes.Contains(map.GetTerrainInfo(blockedCell).Type))
					continue;

				if (!ResourceAt(blockedCell))
					map.CustomTerrain[blockedCell] = map.Rules.TerrainInfo.GetTerrainIndex(info.BlockedTerrainType);
			}
		}

		void RemoveNoBuildZoneAround(CPos resourceLocation)
		{
			foreach (var blockedCell in GetMinerFootprintAround(resourceLocation))
			{
				if (!ResourceAt(blockedCell))
					map.CustomTerrain[blockedCell] = byte.MaxValue;
			}
		}

		bool ResourceAt(CPos cell)
		{
			var resource = resourceLayer.GetResource(cell);
			if (resource.Equals(ResourceLayerContents.Empty))
				return false;

			if (info.ResourceTypes.Count > 0)
				return info.ResourceTypes.Contains(resource.Type);

			return true;
		}
	}
}
