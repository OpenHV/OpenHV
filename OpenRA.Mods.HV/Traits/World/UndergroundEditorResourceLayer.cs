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
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Required for the map editor to work. Attach this to the world actor.",
		"Uses the resource density stored in the map.")]
	public class UndergroundEditorResourceLayerInfo : TraitInfo, IResourceLayerInfo, Requires<ResourceTypeInfo>
	{
		public override object Create(ActorInitializer init) { return new UndergroundEditorResourceLayer(init.Self); }
	}

	public class UndergroundEditorResourceLayer : IResourceLayer, IWorldLoaded, INotifyActorDisposing
	{
		protected readonly Map Map;
		protected readonly TileSet Tileset;
		protected readonly Dictionary<int, ResourceType> Resources;
		protected readonly CellLayer<ResourceLayerContents> Tiles;

		public int NetWorth { get; protected set; }

		bool disposed;

		public event Action<CPos, ResourceType> CellChanged;

		ResourceLayerContents IResourceLayer.GetResource(CPos cell) { return Tiles[cell]; }
		bool IResourceLayer.IsVisible(CPos cell) { return Map.Contains(cell); }

		public UndergroundEditorResourceLayer(Actor self)
		{
			if (self.World.Type != WorldType.Editor)
				return;

			Map = self.World.Map;
			Tileset = self.World.Map.Rules.TileSet;

			Tiles = new CellLayer<ResourceLayerContents>(Map);
			Resources = self.TraitsImplementing<ResourceType>()
				.ToDictionary(r => r.Info.ResourceType, r => r);

			Map.Resources.CellEntryChanged += UpdateCell;
		}

		public void WorldLoaded(World w, WorldRenderer wr)
		{
			if (w.Type != WorldType.Editor)
				return;

			foreach (var cell in Map.AllCells)
				UpdateCell(cell);
		}

		public void UpdateCell(CPos cell)
		{
			var uv = cell.ToMPos(Map);
			if (!Map.Resources.Contains(uv))
				return;

			var tile = Map.Resources[uv];
			var t = Tiles[uv];

			var newTile = ResourceLayerContents.Empty;
			var newTerrain = byte.MaxValue;
			if (Resources.TryGetValue(tile.Type, out ResourceType type))
			{
				newTile = new ResourceLayerContents
				{
					Type = type,
					Density = tile.Index
				};

				newTerrain = Tileset.GetTerrainIndex(type.Info.TerrainType);
			}

			// Nothing has changed
			if (newTile.Type == t.Type && newTile.Density == t.Density)
				return;

			UpdateNetWorth(t.Type, t.Density, newTile.Type, newTile.Density);
			Tiles[uv] = newTile;
			Map.CustomTerrain[uv] = newTerrain;
			CellChanged?.Invoke(cell, type);
		}

		void UpdateNetWorth(ResourceType oldType, int oldDensity, ResourceType newType, int newDensity)
		{
			if (oldType != null && oldDensity > 0)
				NetWorth -= oldDensity * oldType.Info.ValuePerUnit;

			if (newType != null && newDensity > 0)
				NetWorth += newDensity * newType.Info.ValuePerUnit;
		}

		void INotifyActorDisposing.Disposing(Actor self)
		{
			if (disposed)
				return;

			Map.Resources.CellEntryChanged -= UpdateCell;

			disposed = true;
		}
	}
}
