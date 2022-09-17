#region Copyright & License Information
/*
 * Copyright 2022 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Linq;
using OpenRA.Mods.Common.Lint;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.HV.Traits;

namespace OpenRA.Mods.HV.Lint
{
	public class CheckMinerDeployment : ILintMapPass
	{
		public void Run(Action<string> emitError, Action<string> emitWarning, ModData modData, Map map)
		{
			var miners = map.Rules.Actors.Where(a => a.Value.TraitInfoOrDefault<MinerInfo>() != null);
			if (!miners.Any())
				return;

			foreach (var miner in miners)
			{
				var transforms = miner.Value.TraitInfoOrDefault<TransformsInfo>();
				if (transforms == null)
					continue;

				var miningTower = map.Rules.Actors[transforms.IntoActor];
				var building = miningTower.TraitInfoOrDefault<BuildingInfo>();
				if (building == null || building.AllowInvalidPlacement)
					continue;

				foreach (var cell in map.AllCells)
				{
					var resourceIndex = map.Resources[cell].Type;
					if (resourceIndex > 0 && resourceIndex < 3)
					{
						var footprintTiles = building.Tiles(cell + transforms.Offset);
						foreach (var footprintTile in footprintTiles)
						{
							if (!building.TerrainTypes.Contains(map.GetTerrainInfo(footprintTile).Type))
								emitError($"Can't construct mining tower at {footprintTile}!");
						}
					}
				}
			}
		}
	}
}
