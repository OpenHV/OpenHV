#region Copyright & License Information
/*
 * Copyright 2021 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Attach this to the world actor.")]
	[TraitLocation(SystemActors.World)]
	public class LiquidTerrainLayerInfo : TraitInfo, Requires<ITiledTerrainRendererInfo>
	{
		[Desc("Palette to render the layer sprites in.")]
		public readonly string Palette = TileSet.TerrainPaletteInternalName;

		public override object Create(ActorInitializer init) { return new LiquidTerrainLayer(init.Self, this); }
	}

	public class LiquidTerrainLayer : IRenderOverlay, IWorldLoaded, ITickRender, IRadarTerrainLayer, INotifyActorDisposing
	{
		public readonly CellLayer<bool> Covered;

		readonly LiquidTerrainLayerInfo info;
		readonly Dictionary<CPos, TerrainTile?> dirty = new Dictionary<CPos, TerrainTile?>();
		readonly ITiledTerrainRenderer terrainRenderer;
		readonly World world;
		readonly CellLayer<(Color, Color)> radarColor;

		TerrainSpriteLayer render;
		PaletteReference paletteReference;
		bool disposed;

		public LiquidTerrainLayer(Actor self, LiquidTerrainLayerInfo info)
		{
			this.info = info;
			world = self.World;
			Covered = new CellLayer<bool>(world.Map);
			radarColor = new CellLayer<(Color, Color)>(world.Map);
			terrainRenderer = self.Trait<ITiledTerrainRenderer>();
		}

		void IWorldLoaded.WorldLoaded(World w, WorldRenderer wr)
		{
			render = new TerrainSpriteLayer(w, wr, terrainRenderer.MissingTile, BlendMode.Alpha, wr.World.Type != WorldType.Editor);
			paletteReference = wr.Palette(info.Palette);
		}

		public bool ContainsTile(CPos cell)
		{
			return Covered.Contains(cell) && Covered[cell];
		}

		public void AddTile(CPos cell, TerrainTile tile)
		{
			if (!Covered.Contains(cell))
				return;

			var uv = cell.ToMPos(world.Map);
			var tileInfo = world.Map.Rules.TerrainInfo.GetTerrainInfo(tile);
			world.Map.CustomTerrain[uv] = tileInfo.TerrainType;
			Covered[uv] = true;
			radarColor[uv] = (tileInfo.GetColor(world.LocalRandom), tileInfo.GetColor(world.LocalRandom));
			dirty[cell] = tile;
		}

		public void RemoveTile(CPos cell)
		{
			if (!Covered.Contains(cell))
				return;

			var uv = cell.ToMPos(world.Map);
			world.Map.CustomTerrain[uv] = byte.MaxValue;
			Covered[cell] = false;
			radarColor[uv] = (Color.Transparent, Color.Transparent);
			dirty[cell] = null;
		}

		void ITickRender.TickRender(WorldRenderer wr, Actor self)
		{
			var remove = new List<CPos>();
			foreach (var kv in dirty)
			{
				if (!self.World.FogObscures(kv.Key))
				{
					var tile = kv.Value;
					if (tile.HasValue)
					{
						// Terrain tiles define their origin at the topleft
						var s = terrainRenderer.TileSprite(tile.Value);
						var sprite = new Sprite(s.Sheet, s.Bounds, s.ZRamp, float2.Zero, s.Channel, s.BlendMode);
						render.Update(kv.Key, sprite, paletteReference);
					}
					else
						render.Clear(kv.Key);

					remove.Add(kv.Key);
				}
			}

			foreach (var r in remove)
				dirty.Remove(r);
		}

		void IRenderOverlay.Render(WorldRenderer wr)
		{
			render.Draw(wr.Viewport);
		}

		event Action<CPos> IRadarTerrainLayer.CellEntryChanged
		{
			add => radarColor.CellEntryChanged += value;
			remove => radarColor.CellEntryChanged -= value;
		}

		bool IRadarTerrainLayer.TryGetTerrainColorPair(MPos uv, out (Color Left, Color Right) value)
		{
			value = radarColor[uv];
			return Covered[uv];
		}

		void INotifyActorDisposing.Disposing(Actor self)
		{
			if (disposed)
				return;

			render.Dispose();
			disposed = true;
		}
	}
}
