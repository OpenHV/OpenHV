#region Copyright & License Information
/*
 * Copyright 2019-2021 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[TraitLocation(SystemActors.World)]
	[Desc("Attach this to the world actor. Required for LaysTerrain to work.")]
	public class CustomTerrainLayerInfo : TraitInfo, Requires<ITiledTerrainRendererInfo>
	{
		[Desc("Palette to render the layer sprites in.")]
		public readonly string Palette = TileSet.TerrainPaletteInternalName;

		public override object Create(ActorInitializer init) { return new CustomTerrainLayer(init.Self, this); }
	}

	public class CustomTerrainLayer : IRenderOverlay, IWorldLoaded, ITickRender, INotifyActorDisposing
	{
		readonly CustomTerrainLayerInfo info;
		readonly Dictionary<CPos, TerrainTile?> dirty = new();
		readonly ITiledTerrainRenderer terrainRenderer;
		readonly Map map;

		TerrainSpriteLayer render;
		PaletteReference paletteReference;
		bool disposed;

		public CustomTerrainLayer(Actor self, CustomTerrainLayerInfo info)
		{
			this.info = info;
			map = self.World.Map;
			terrainRenderer = self.Trait<ITiledTerrainRenderer>();
		}

		void IWorldLoaded.WorldLoaded(World w, WorldRenderer wr)
		{
			render = new TerrainSpriteLayer(w, wr, terrainRenderer.MissingTile, BlendMode.Alpha, w.Type != WorldType.Editor);
			paletteReference = wr.Palette(info.Palette);
		}

		public void AddTile(CPos cell, TerrainTile tile)
		{
			map.CustomTerrain[cell] = map.Rules.TerrainInfo.GetTerrainIndex(tile);
			dirty[cell] = tile;
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

		void INotifyActorDisposing.Disposing(Actor self)
		{
			if (disposed)
				return;

			render.Dispose();
			disposed = true;
		}
	}
}
