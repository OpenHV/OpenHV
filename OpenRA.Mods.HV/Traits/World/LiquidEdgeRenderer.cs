#region Copyright & License Information
/*
 * Copyright 2019-2025 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenRA.Graphics;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[TraitLocation(SystemActors.World)]
	[Desc("Used to render the border of liquids.", "Attach this to the world actor")]
	public class LiquidEdgeRendererInfo : TraitInfo, Requires<LiquidTerrainLayerInfo>
	{
		[FieldLoader.Require]
		[Desc("Sequence image that holds the different variants.")]
		public readonly string Image = null;

		[SequenceReference(nameof(Image))]
		public readonly string Sequence = "idle";

		[PaletteReference]
		[Desc("Palette used for rendering the resource sprites.")]
		public readonly string Palette = TileSet.TerrainPaletteInternalName;

		public readonly ImmutableArray<string> BlockTerrainTypes = [];
		public readonly ImmutableArray<string> CoveredTerrainTypes = [];

		public override object Create(ActorInitializer init) { return new LiquidEdgeRenderer(init.Self, this); }
	}

	public class LiquidEdgeRenderer : IWorldLoaded, IRenderOverlay, ITickRender
	{
		[Flags]
		public enum BitMask : byte
		{
			None = 0x00,

			Left = 0x1,
			Top = 0x2,
			Right = 0x4,
			Bottom = 0x8,

			TopLeft = 0x10,
			TopRight = 0x20,
			BottomLeft = 0x40,
			BottomRight = 0x80,

			All = Left | Top | Right | Bottom | TopLeft | TopRight | BottomLeft | BottomRight,
		}

		static readonly Dictionary<BitMask, int> SpriteMap = new()
		{
			{ BitMask.Bottom | BitMask.BottomLeft | BitMask.BottomRight, 0 },
			{ BitMask.BottomRight, 1 },
			{ BitMask.Right | BitMask.TopRight | BitMask.BottomRight, 2 },
			{ BitMask.TopRight, 3 },
			{ BitMask.Top | BitMask.TopLeft | BitMask.TopRight, 4 },
			{ BitMask.TopLeft, 5 },
			{ BitMask.Left | BitMask.TopLeft | BitMask.BottomLeft, 6 },
			{ BitMask.BottomLeft, 7 },
			{ BitMask.Right | BitMask.Bottom | BitMask.TopRight | BitMask.BottomLeft | BitMask.BottomRight, 8 },
			{ BitMask.Top | BitMask.Right | BitMask.TopLeft | BitMask.TopRight | BitMask.BottomRight, 9 },
			{ BitMask.Left | BitMask.Top | BitMask.TopLeft | BitMask.TopRight | BitMask.BottomLeft, 10 },
			{ BitMask.Left | BitMask.Bottom | BitMask.TopLeft | BitMask.BottomLeft | BitMask.BottomRight, 11 },
		};

		readonly LiquidTerrainLayer liquidTerrainLayer;
		readonly LiquidEdgeRendererInfo info;
		readonly ISpriteSequence spriteSequence;
		readonly Map map;

		readonly Queue<CPos> cleanDirty = [];
		readonly HashSet<CPos> dirty = [];

		TerrainSpriteLayer spriteLayer;
		PaletteReference paletteReference;

		public LiquidEdgeRenderer(Actor self, LiquidEdgeRendererInfo info)
		{
			liquidTerrainLayer = self.Trait<LiquidTerrainLayer>();
			liquidTerrainLayer.Covered.CellEntryChanged += AddDirtyCell;
			spriteSequence = self.World.Map.Sequences.GetSequence(info.Image, info.Sequence);
			map = self.World.Map;
			this.info = info;
		}

		void IWorldLoaded.WorldLoaded(World w, WorldRenderer wr)
		{
			paletteReference = wr.Palette(info.Palette);
			var first = spriteSequence.GetSprite(0);
			var emptySprite = new Sprite(first.Sheet, Rectangle.Empty, TextureChannel.Alpha);
			spriteLayer = new TerrainSpriteLayer(w, wr, emptySprite, first.BlendMode, wr.World.Type != WorldType.Editor);
		}

		void AddDirtyCell(CPos cell)
		{
			dirty.Add(cell);
		}

		BitMask FindClearSides(CPos cell)
		{
			var clearSides = BitMask.None;
			if (liquidTerrainLayer.ContainsTile(cell + new CVec(0, -1)))
				clearSides |= BitMask.Top | BitMask.TopLeft | BitMask.TopRight;

			if (liquidTerrainLayer.ContainsTile(cell + new CVec(-1, 0)))
				clearSides |= BitMask.Left | BitMask.TopLeft | BitMask.BottomLeft;

			if (liquidTerrainLayer.ContainsTile(cell + new CVec(1, 0)))
				clearSides |= BitMask.Right | BitMask.TopRight | BitMask.BottomRight;

			if (liquidTerrainLayer.ContainsTile(cell + new CVec(0, 1)))
				clearSides |= BitMask.Bottom | BitMask.BottomLeft | BitMask.BottomRight;

			if (liquidTerrainLayer.ContainsTile(cell + new CVec(-1, -1)))
				clearSides |= BitMask.TopLeft;

			if (liquidTerrainLayer.ContainsTile(cell + new CVec(1, -1)))
				clearSides |= BitMask.TopRight;

			if (liquidTerrainLayer.ContainsTile(cell + new CVec(-1, 1)))
				clearSides |= BitMask.BottomLeft;

			if (liquidTerrainLayer.ContainsTile(cell + new CVec(1, 1)))
				clearSides |= BitMask.BottomRight;

			return clearSides;
		}

		void IRenderOverlay.Render(WorldRenderer wr)
		{
			spriteLayer.Draw(wr.Viewport);
		}

		protected void UpdateSpriteLayers(CPos cell, ISpriteSequence sequence, int frame, PaletteReference palette)
		{
			if (sequence != null)
				spriteLayer.Update(cell, sequence, palette, frame);
			else
				spriteLayer.Clear(cell);
		}

		void ITickRender.TickRender(WorldRenderer wr, Actor self)
		{
			foreach (var cell in dirty)
			{
				if (!wr.World.FogObscures(cell))
				{
					UpdateRenderedSprite(cell);
					cleanDirty.Enqueue(cell);
				}
			}

			while (cleanDirty.Count > 0)
				dirty.Remove(cleanDirty.Dequeue());
		}

		void UpdateRenderedSprite(CPos cell)
		{
			if (info.BlockTerrainTypes.Contains(map.GetTerrainInfo(cell).Type))
				return;

			var directions = CVec.Directions;
			for (var i = 0; i < directions.Length; i++)
				UpdateRenderedSpriteInner(cell + directions[i]);
		}

		void UpdateRenderedSpriteInner(CPos cell)
		{
			if (info.BlockTerrainTypes.Contains(map.GetTerrainInfo(cell).Type) || info.CoveredTerrainTypes.Contains(map.GetTerrainInfo(cell).Type))
			{
				UpdateSpriteLayers(cell, null, 0, null);
				return;
			}

			var clear = FindClearSides(cell);
			if (clear == BitMask.All)
			{
				UpdateSpriteLayers(cell, null, 0, null);
				return;
			}

			if (SpriteMap.TryGetValue(clear, out var index))
				UpdateSpriteLayers(cell, spriteSequence, index, paletteReference);
			else
				UpdateSpriteLayers(cell, null, 0, null);
		}
	}
}
