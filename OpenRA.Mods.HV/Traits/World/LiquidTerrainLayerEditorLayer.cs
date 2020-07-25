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

using System.Linq;
using OpenRA.Mods.Common.Traits;

namespace OpenRA.Mods.HV.Traits
{
	using ClearSides = LiquidTerrainRenderer.ClearSides;

	[Desc("Used to render spice with round borders.")]
	public class LiquidTerrainLayerEditorLayerInfo : EditorResourceLayerInfo
	{
		[FieldLoader.Require]
		[Desc("Only care for these ResourceType names.")]
		public readonly string[] Types = null; // TODO: This should exist on EditorResourceLayer as well to avoid conflicts.

		public override object Create(ActorInitializer init) { return new LiquidTerrainLayerEditorLayer(init.Self, this); }
	}

	public class LiquidTerrainLayerEditorLayer : EditorResourceLayer
	{
		readonly LiquidTerrainLayerEditorLayerInfo info;

		public LiquidTerrainLayerEditorLayer(Actor self, LiquidTerrainLayerEditorLayerInfo info)
			: base(self)
		{
			this.info = info;
		}

		public override EditorCellContents UpdateDirtyTile(CPos c)
		{
			var t = Tiles[c];

			// Empty tile
			if (t.Type == null)
			{
				t.Sequence = null;
				return t;
			}

			if (!info.Types.Contains(t.Type.Info.Type))
				return t;

			int index;
			var clear = FindClearSides(t.Type, c);
			if (LiquidTerrainRenderer.SpriteMap.TryGetValue(clear, out index))
			{
				t.Sequence = t.Type.Variants.First().Value;
				t.Frame = index;
			}
			else
				t.Sequence = null;

			return t;
		}

		bool CellContains(CPos c, ResourceType t)
		{
			return Tiles.Contains(c) && Tiles[c].Type == t;
		}

		ClearSides FindClearSides(ResourceType t, CPos p)
		{
			var ret = ClearSides.None;
			if (!CellContains(p + new CVec(0, -1), t))
				ret |= ClearSides.Top | ClearSides.TopLeft | ClearSides.TopRight;

			if (!CellContains(p + new CVec(-1, 0), t))
				ret |= ClearSides.Left | ClearSides.TopLeft | ClearSides.BottomLeft;

			if (!CellContains(p + new CVec(1, 0), t))
				ret |= ClearSides.Right | ClearSides.TopRight | ClearSides.BottomRight;

			if (!CellContains(p + new CVec(0, 1), t))
				ret |= ClearSides.Bottom | ClearSides.BottomLeft | ClearSides.BottomRight;

			if (!CellContains(p + new CVec(-1, -1), t))
				ret |= ClearSides.TopLeft;

			if (!CellContains(p + new CVec(1, -1), t))
				ret |= ClearSides.TopRight;

			if (!CellContains(p + new CVec(-1, 1), t))
				ret |= ClearSides.BottomLeft;

			if (!CellContains(p + new CVec(1, 1), t))
				ret |= ClearSides.BottomRight;

			return ret;
		}
	}
}
