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
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Required for the map editor to work. Attach this to the world actor.")]
	public class UndergroundEditorResourceLayerInfo : TraitInfo, Requires<ResourceTypeInfo>
	{
		[Desc("Only care for these ResourceType names.")]
		public readonly string[] Types = null;

		public override object Create(ActorInitializer init) { return new UndergroundEditorResourceLayer(init.Self, this); }
	}

	public class UndergroundEditorResourceLayer : IWorldLoaded, IRenderOverlay, INotifyActorDisposing
	{
		protected readonly Map Map;
		protected readonly TileSet Tileset;
		protected readonly Dictionary<int, ResourceType> Resources;
		protected readonly CellLayer<EditorCellContents> Tiles;
		protected readonly HashSet<CPos> Dirty = new HashSet<CPos>();

		readonly Dictionary<PaletteReference, TerrainSpriteLayer> spriteLayers = new Dictionary<PaletteReference, TerrainSpriteLayer>();

		public int NetWorth { get; protected set; }

		public readonly UndergroundEditorResourceLayerInfo Info;

		bool disposed;

		public UndergroundEditorResourceLayer(Actor self, UndergroundEditorResourceLayerInfo info)
		{
			if (self.World.Type != WorldType.Editor)
				return;

			Info = info;

			Map = self.World.Map;
			Tileset = self.World.Map.Rules.TileSet;

			Tiles = new CellLayer<EditorCellContents>(Map);
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

			// Build the sprite layer dictionary for rendering resources
			// All resources that have the same palette must also share a sheet and blend mode
			foreach (var r in Resources)
			{
				var res = r;
				var layer = spriteLayers.GetOrAdd(r.Value.Palette, pal =>
				{
					var first = res.Value.Variants.First().Value.First();
					return new TerrainSpriteLayer(w, wr, first.Sheet, first.BlendMode, pal, false);
				});

				// Validate that sprites are compatible with this layer
				var sheet = layer.Sheet;
				if (res.Value.Variants.Any(kv => kv.Value.Any(s => s.Sheet != sheet)))
					throw new InvalidDataException("Resource sprites span multiple sheets. Try loading their sequences earlier.");

				var blendMode = layer.BlendMode;
				if (res.Value.Variants.Any(kv => kv.Value.Any(s => s.BlendMode != blendMode)))
					throw new InvalidDataException("Resource sprites specify different blend modes. "
						+ "Try using different palettes for resource types that use different blend modes.");
			}
		}

		public void UpdateCell(CPos cell)
		{
			var uv = cell.ToMPos(Map);
			var tile = Map.Resources[uv];

			var t = Tiles[cell];

			if (Info.Types != null && t.Type != null && !Info.Types.Contains(t.Type.Info.Type))
				return;

			if (t.Density > 0)
				NetWorth -= t.Density * t.Type.Info.ValuePerUnit;

			ResourceType type;
			if (Resources.TryGetValue(tile.Type, out type))
			{
				Tiles[uv] = new EditorCellContents
				{
					Type = type,
					Variant = ChooseRandomVariant(type),
				};

				Map.CustomTerrain[uv] = Tileset.GetTerrainIndex(type.Info.TerrainType);
			}
			else
			{
				Tiles[uv] = EditorCellContents.Empty;
				Map.CustomTerrain[uv] = byte.MaxValue;
			}

			Dirty.Add(cell);
		}

		protected virtual string ChooseRandomVariant(ResourceType t)
		{
			return t.Variants.Keys.Random(Game.CosmeticRandom);
		}

		public virtual EditorCellContents UpdateDirtyTile(CPos c)
		{
			var t = Tiles[c];
			var type = t.Type;

			// Empty tile
			if (type == null)
			{
				t.Sprite = null;
				return t;
			}

			if (Info.Types != null && !Info.Types.Contains(t.Type.Info.Type))
				return t;

			NetWorth -= t.Density * type.Info.ValuePerUnit;

			t.Density = Map.Resources[c].Index;

			NetWorth += t.Density * type.Info.ValuePerUnit;

			var sprites = type.Variants[t.Variant];
			var frame = int2.Lerp(0, sprites.Length - 1, t.Density, type.Info.MaxDensity);
			t.Sprite = sprites[frame];

			return t;
		}

		void IRenderOverlay.Render(WorldRenderer wr)
		{
			if (wr.World.Type != WorldType.Editor)
				return;

			foreach (var c in Dirty)
			{
				if (Tiles.Contains(c))
				{
					var resource = UpdateDirtyTile(c);
					Tiles[c] = resource;

					foreach (var kv in spriteLayers)
					{
						// resource.Type is meaningless (and may be null) if resource.Sprite is null
						if (resource.Sprite != null && resource.Type.Palette == kv.Key)
							kv.Value.Update(c, resource.Sprite);
						else
							kv.Value.Update(c, null);
					}
				}
			}

			Dirty.Clear();

			foreach (var l in spriteLayers.Values)
				l.Draw(wr.Viewport);
		}

		void INotifyActorDisposing.Disposing(Actor self)
		{
			if (disposed)
				return;

			foreach (var kv in spriteLayers.Values)
				kv.Dispose();

			Map.Resources.CellEntryChanged -= UpdateCell;

			disposed = true;
		}
	}
}
