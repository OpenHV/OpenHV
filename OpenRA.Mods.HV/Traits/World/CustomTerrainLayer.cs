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
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Attach this to the world actor. Required for LaysTerrain to work.")]
	public class CustomTerrainLayerInfo : TraitInfo
	{
		[Desc("Palette to render the layer sprites in.")]
		public readonly string Palette = TileSet.TerrainPaletteInternalName;

		public override object Create(ActorInitializer init) { return new CustomTerrainLayer(init.Self, this); }
	}

	public class CustomTerrainLayer : IRenderOverlay, IWorldLoaded, ITickRender, INotifyActorDisposing
	{
		readonly CustomTerrainLayerInfo info;
		readonly Dictionary<CPos, Sprite> dirty = new Dictionary<CPos, Sprite>();
		readonly Map map;

		TerrainSpriteLayer render;
		Theater theater;
		bool disposed;

		public CustomTerrainLayer(Actor self, CustomTerrainLayerInfo info)
		{
			this.info = info;
			map = self.World.Map;
		}

		public void WorldLoaded(World w, WorldRenderer wr)
		{
			theater = wr.Theater;
			render = new TerrainSpriteLayer(w, wr, theater.Sheet, BlendMode.Alpha, wr.Palette(info.Palette), wr.World.Type != WorldType.Editor);
		}

		public void AddTile(CPos cell, TerrainTile tile)
		{
			map.CustomTerrain[cell] = map.Rules.TileSet.GetTerrainIndex(tile);

			// Terrain tiles define their origin at the topleft
			var s = theater.TileSprite(tile);
			dirty[cell] = new Sprite(s.Sheet, s.Bounds, s.ZRamp, float2.Zero, s.Channel, s.BlendMode);
		}

		void ITickRender.TickRender(WorldRenderer wr, Actor self)
		{
			var remove = new List<CPos>();
			foreach (var kv in dirty)
			{
				if (!self.World.FogObscures(kv.Key))
				{
					render.Update(kv.Key, kv.Value);
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
