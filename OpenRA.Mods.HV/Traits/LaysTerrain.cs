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

using System.Collections.Generic;
using System.IO;
using OpenRA.Mods.Common.Terrain;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	public class LaysTerrainInfo : TraitInfo, Requires<BuildingInfo>
	{
		[Desc("The terrain template to place. If the template is PickAny, then " +
			"the actor footprint will be filled with this tile.")]
		public readonly ushort Template = 0;

		[Desc("The terrain types that this template will be placed on.")]
		public readonly HashSet<string> TerrainTypes = null;

		[Desc("Offset relative to the actor TopLeft. Not used if the template is PickAny.",
			"Tiles being offset out of the actor's footprint will not be placed.")]
		public readonly CVec Offset = CVec.Zero;

		public override object Create(ActorInitializer init) { return new LaysTerrain(init.Self, this); }
	}

	public class LaysTerrain : INotifyAddedToWorld
	{
		readonly LaysTerrainInfo info;
		readonly CustomTerrainLayer layer;
		readonly TerrainTemplateInfo template;
		readonly BuildingInfo buildingInfo;

		public LaysTerrain(Actor self, LaysTerrainInfo info)
		{
			this.info = info;
			layer = self.World.WorldActor.Trait<CustomTerrainLayer>();

			var terrainInfo = self.World.Map.Rules.TerrainInfo as ITemplatedTerrainInfo;
			if (terrainInfo == null)
				throw new InvalidDataException("LaysTerrain requires a template-based tileset.");

			template = terrainInfo.Templates[info.Template];

			buildingInfo = self.Info.TraitInfo<BuildingInfo>();
		}

		void INotifyAddedToWorld.AddedToWorld(Actor self)
		{
			var map = self.World.Map;

			if (template.PickAny)
			{
				// Fill the footprint with random variants
				foreach (var c in buildingInfo.Tiles(self.Location))
				{
					// Only place on allowed terrain types
					if (!map.Contains(c) || (info.TerrainTypes != null && !info.TerrainTypes.Contains(map.GetTerrainInfo(c).Type)))
						continue;

					var index = Game.CosmeticRandom.Next(template.TilesCount);
					layer.AddTile(c, new TerrainTile(template.Id, (byte)index));
				}

				return;
			}

			var origin = self.Location + info.Offset;
			for (var i = 0; i < template.TilesCount; i++)
			{
				var c = origin + new CVec(i % template.Size.X, i / template.Size.X);

				// Only place on allowed terrain types
				if (info.TerrainTypes != null && !info.TerrainTypes.Contains(map.GetTerrainInfo(c).Type))
					continue;

				layer.AddTile(c, new TerrainTile(template.Id, (byte)i));
			}
		}
	}
}
