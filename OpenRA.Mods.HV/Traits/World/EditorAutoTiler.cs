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
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using OpenRA.Mods.Common.Terrain;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.HV.Terrain;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[IncludeStaticFluentReferences(typeof(EditorAutoTiler))]
	public class EditorAutoTilerInfo : TraitInfo
	{
		public override object Create(ActorInitializer init)
		{
			return new EditorAutoTiler(this);
		}
	}

	[TraitLocation(SystemActors.EditorWorld)]
	public class EditorAutoTiler : IEditorTool
	{
		[FluentReference]
		const string Label = "label-tool-autotiler";

		[Desc("The widget tree to open when the tool is selected.")]
		const string PanelWidget = "AUTOTILER_TOOL_PANEL";

		string IEditorTool.Label => Label;
		string IEditorTool.PanelWidget => PanelWidget;

		public TraitInfo TraitInfo { get; }
		public bool IsEnabled => true;

		public EditorAutoTiler(EditorAutoTilerInfo info)
		{
			TraitInfo = info;
		}
	}

	[Flags]
	public enum BitMask : byte
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

		All = Left | Top | Right | Bottom | TopLeft | TopRight | BottomLeft | BottomRight
	}

	sealed class AutoConnectEditorAction : IEditorAction
	{
		public string Text { get; }

		readonly Map map;
		readonly CellRegion selection;
		readonly bool cliff;
		readonly ITemplatedTerrainInfo terrainInfo;
		readonly Queue<UndoTile> undoTiles = new();

		public AutoConnectEditorAction(Map map, CellRegion selection, bool cliff)
		{
			this.map = map;
			this.selection = selection;
			this.cliff = cliff;

			terrainInfo = (ITemplatedTerrainInfo)map.Rules.TerrainInfo;

			Text = "Auto connect tile transitions.";
		}

		public void Execute()
		{
			Do();
		}

		static readonly BitMask[] BorderTileMap =
		[
			BitMask.Top | BitMask.Left | BitMask.Right,
			BitMask.Top | BitMask.Right | BitMask.Bottom,
			BitMask.Left | BitMask.Bottom | BitMask.Right,
			BitMask.Top | BitMask.Bottom | BitMask.Left,
			BitMask.Left | BitMask.Bottom,
			BitMask.Top | BitMask.Left,
			BitMask.Top | BitMask.Right,
			BitMask.Bottom | BitMask.Right,
		];

		static readonly FrozenDictionary<BitMask, CVec[]> MatchingBorderCells = new Dictionary<BitMask, CVec[]>()
		{
			{ BitMask.Top | BitMask.Left | BitMask.Right, new[] { new CVec(0, 1) } },
			{ BitMask.Top | BitMask.Right | BitMask.Bottom, new[] { new CVec(-1, 0) } },
			{ BitMask.Left | BitMask.Bottom | BitMask.Right, new[] { new CVec(0, -1) } },
			{ BitMask.Top | BitMask.Bottom | BitMask.Left, new[] { new CVec(1, 0) } },
			{ BitMask.Left | BitMask.Bottom, new[] { new CVec(1, 0), new CVec(0, -1) } },
			{ BitMask.Top | BitMask.Left, new[] { new CVec(1, 0), new CVec(0, 1) } },
			{ BitMask.Top | BitMask.Right, new[] { new CVec(0, 1), new CVec(-1, 0) } },
			{ BitMask.Bottom | BitMask.Right, new[] { new CVec(-1, 0), new CVec(0, -1) } },
			{ BitMask.BottomRight | BitMask.TopRight | BitMask.TopLeft, new[] { new CVec(-1, 1) } },
			{ BitMask.TopRight | BitMask.BottomLeft | BitMask.BottomRight, new[] { new CVec(-1, -1) } },
			{ BitMask.TopLeft | BitMask.BottomLeft | BitMask.BottomRight, new[] { new CVec(1, -1) } },
			{ BitMask.TopLeft | BitMask.TopRight | BitMask.BottomLeft, new[] { new CVec(1, 1) } },
		}.ToFrozenDictionary();

		static readonly BitMask[] CornerTileMap =
		[
			BitMask.BottomRight | BitMask.TopRight | BitMask.TopLeft,
			BitMask.TopRight | BitMask.BottomLeft | BitMask.BottomRight,
			BitMask.TopLeft | BitMask.BottomLeft | BitMask.BottomRight,
			BitMask.TopLeft | BitMask.TopRight | BitMask.BottomLeft,
		];

		static bool CellContains(Map map, CPos cell, ImmutableArray<string> terrainTypes)
		{
			return map.Contains(cell) && terrainTypes.Contains(map.GetTerrainInfo(cell).Type);
		}

		static BitMask FindAdjacentTerrain(Map map, CPos cell, ImmutableArray<string> terrainTypes)
		{
			var adjacentTiles = BitMask.None;

			if (CellContains(map, cell + new CVec(0, -1), terrainTypes))
				adjacentTiles |= BitMask.Top;

			if (CellContains(map, cell + new CVec(-1, 0), terrainTypes))
				adjacentTiles |= BitMask.Left;

			if (CellContains(map, cell + new CVec(1, 0), terrainTypes))
				adjacentTiles |= BitMask.Right;

			if (CellContains(map, cell + new CVec(0, 1), terrainTypes))
				adjacentTiles |= BitMask.Bottom;

			return adjacentTiles;
		}

		static BitMask FindCorners(Map map, CPos cell, ImmutableArray<string> terrainTypes)
		{
			var cornerTile = BitMask.None;

			if (CellContains(map, cell + new CVec(-1, -1), terrainTypes))
				cornerTile |= BitMask.TopLeft;

			if (CellContains(map, cell + new CVec(1, -1), terrainTypes))
				cornerTile |= BitMask.TopRight;

			if (CellContains(map, cell + new CVec(-1, 1), terrainTypes))
				cornerTile |= BitMask.BottomLeft;

			if (CellContains(map, cell + new CVec(1, 1), terrainTypes))
				cornerTile |= BitMask.BottomRight;

			return cornerTile;
		}

		public void Do()
		{
			foreach (var cell in selection)
			{
				foreach (var autoConnect in FilteredAutoConnects(cell))
				{
					var clear = FindAdjacentTerrain(map, cell, autoConnect.BorderTypes);
					var index = BorderTileMap.IndexOf(clear);
					if (index != -1 && MatchingBorderCells.TryGetValue(clear, out var validNeighbors)
						&& validNeighbors.All(n => map.GetTerrainInfo(cell + n).Type == autoConnect.Type))
					{
						undoTiles.Enqueue(new UndoTile(cell, map.Tiles[cell]));
						var borderTransitions = autoConnect.BorderTransitions;
						map.Tiles[cell] = new TerrainTile(borderTransitions[index], 0x00);
					}
				}
			}

			foreach (var cell in selection)
			{
				foreach (var autoConnect in FilteredAutoConnects(cell))
				{
					var corner = FindCorners(map, cell, autoConnect.BorderTypes);
					var index = CornerTileMap.IndexOf(corner);
					if (index != -1 && MatchingBorderCells.TryGetValue(corner, out var validNeighbors)
						&& validNeighbors.All(n => map.GetTerrainInfo(cell + n).Type == autoConnect.Type))
					{
						undoTiles.Enqueue(new UndoTile(cell, map.Tiles[cell]));
						var corners = autoConnect.Corners;
						map.Tiles[cell] = new TerrainTile(corners[index], 0x00);
					}
				}
			}

			IEnumerable<AutoConnectInfo> FilteredAutoConnects(CPos cell)
			{
				var customTerrainTemplateInfo = (CustomTerrainTemplateInfo)terrainInfo.Templates[map.Tiles[cell].Type];
				return customTerrainTemplateInfo.AutoConnects
					.Where(autoConnect => cliff
						? autoConnect.BorderTypes.Any(b => b.Contains("Cliff"))
						: autoConnect.BorderTypes.All(b => !b.Contains("Cliff")));
			}
		}

		public void Undo()
		{
			while (undoTiles.Count > 0)
			{
				var undoTile = undoTiles.Dequeue();
				map.Tiles[undoTile.Cell] = undoTile.MapTile;
			}
		}
	}

	sealed class UndoTile
	{
		public CPos Cell { get; }
		public TerrainTile MapTile { get; }

		public UndoTile(CPos cell, TerrainTile mapTile)
		{
			Cell = cell;
			MapTile = mapTile;
		}
	}
}
