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
using OpenRA.Mods.Common.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Used to render liquids.", "Attach this to the world actor")]
	public class LiquidTerrainRendererInfo : ResourceRendererInfo
	{
		public override object Create(ActorInitializer init) { return new LiquidTerrainRenderer(init.Self, this); }
	}

	public class LiquidTerrainRenderer : ResourceRenderer
	{
		[Flags]
		public enum ClearSides : byte
		{
			None = 0x0,
			Left = 0x1,
			Top = 0x2,
			Right = 0x4,
			Bottom = 0x8,

			TopLeft = 0x10,
			TopRight = 0x20,
			BottomLeft = 0x40,
			BottomRight = 0x80,

			All = 0xFF
		}

		// TODO: incomplete
		public static readonly Dictionary<ClearSides, int> SpriteMap = new Dictionary<ClearSides, int>()
		{
			{ ClearSides.Top, 0 },
			{ ClearSides.Top | ClearSides.TopLeft | ClearSides.TopRight, 0 },
			{ ClearSides.Left | ClearSides.Top | ClearSides.Bottom | ClearSides.TopLeft | ClearSides.TopRight | ClearSides.BottomLeft | ClearSides.BottomRight, 1 },
			{ ClearSides.Left | ClearSides.Top | ClearSides.TopLeft | ClearSides.BottomLeft, 1 },
			{ ClearSides.BottomLeft | ClearSides.Left | ClearSides.TopLeft | ClearSides.Top | ClearSides.TopRight, 1 },
			{ ClearSides.Left | ClearSides.TopLeft | ClearSides.BottomLeft, 2 },
			{ ClearSides.Left, 2 },
			{ ClearSides.Left | ClearSides.Bottom | ClearSides.TopLeft | ClearSides.BottomLeft | ClearSides.BottomRight, 3 },
			{ ClearSides.Bottom, 4 },
			{ ClearSides.Bottom | ClearSides.BottomLeft | ClearSides.BottomRight, 4 },
			{ ClearSides.Right | ClearSides.Bottom | ClearSides.TopRight | ClearSides.BottomLeft | ClearSides.BottomRight, 5 },
			{ ClearSides.Right, 6 },
			{ ClearSides.Right | ClearSides.TopRight | ClearSides.BottomRight, 6 },
			{ ClearSides.Top | ClearSides.Right | ClearSides.TopLeft | ClearSides.TopRight | ClearSides.BottomRight, 7 },
			{ ClearSides.Top | ClearSides.Right | ClearSides.TopLeft | ClearSides.TopRight | ClearSides.BottomLeft | ClearSides.BottomRight, 7 },
			{ ClearSides.None, 8 },
			{ ClearSides.TopLeft, 17 },
			{ ClearSides.BottomLeft, 19 },
			{ ClearSides.BottomRight, 20 },
			{ ClearSides.TopRight, 21 }
		};

		public LiquidTerrainRenderer(Actor self, LiquidTerrainRendererInfo info)
			: base(self, info) { }

		bool CellContains(CPos cell, string resourceType)
		{
			return RenderContents.Contains(cell) && RenderContents[cell].Type == resourceType;
		}

		ClearSides FindClearSides(CPos cell, string resourceType)
		{
			var clearSides = ClearSides.None;
			if (!CellContains(cell + new CVec(0, -1), resourceType))
				clearSides |= ClearSides.Top | ClearSides.TopLeft | ClearSides.TopRight;

			if (!CellContains(cell + new CVec(-1, 0), resourceType))
				clearSides |= ClearSides.Left | ClearSides.TopLeft | ClearSides.BottomLeft;

			if (!CellContains(cell + new CVec(1, 0), resourceType))
				clearSides |= ClearSides.Right | ClearSides.TopRight | ClearSides.BottomRight;

			if (!CellContains(cell + new CVec(0, 1), resourceType))
				clearSides |= ClearSides.Bottom | ClearSides.BottomLeft | ClearSides.BottomRight;

			if (!CellContains(cell + new CVec(-1, -1), resourceType))
				clearSides |= ClearSides.TopLeft;

			if (!CellContains(cell + new CVec(1, -1), resourceType))
				clearSides |= ClearSides.TopRight;

			if (!CellContains(cell + new CVec(-1, 1), resourceType))
				clearSides |= ClearSides.BottomLeft;

			if (!CellContains(cell + new CVec(1, 1), resourceType))
				clearSides |= ClearSides.BottomRight;

			return clearSides;
		}

		protected override void UpdateRenderedSprite(CPos cell, RendererCellContents content)
		{
			UpdateRenderedSpriteInner(cell, content);

			var directions = CVec.Directions;
			for (var i = 0; i < directions.Length; i++)
				UpdateRenderedSpriteInner(cell + directions[i]);
		}

		void UpdateRenderedSpriteInner(CPos cell)
		{
			UpdateRenderedSpriteInner(cell, RenderContents[cell]);
		}

		void UpdateRenderedSpriteInner(CPos cell, RendererCellContents content)
		{
			var density = content.Density;
			var renderType = content.Type;

			if (density > 0 && renderType != null)
			{
				var clear = FindClearSides(cell, content.Type);
				if (SpriteMap.TryGetValue(clear, out var index))
					UpdateSpriteLayers(cell, content.Sequence, index, content.Palette);
				else
					Log.Write("debug", "{1}: SpriteMap does not contain an index for ClearSides type '{0}'".F(clear, cell));
			}
			else
				UpdateSpriteLayers(cell, null, 0, null);
		}
	}
}
