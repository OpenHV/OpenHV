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
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	class ForestLayerInfo : ITraitInfo
	{
		[ActorReference]
		[FieldLoader.Require]
		public readonly string[] Trees = null;

		public object Create(ActorInitializer init) { return new ForestLayer(this); }
	}

	class ForestLayer : IWorldLoaded
	{
		readonly ForestLayerInfo info;
		readonly Dictionary<ushort, string> treeTypes = new Dictionary<ushort, string>();

		CellLayer<Tree> trees;

		public ForestLayer(ForestLayerInfo info)
		{
			this.info = info;
		}

		public void WorldLoaded(World w, WorldRenderer wr)
		{
			trees = new CellLayer<Tree>(w.Map);

			// Build a list of templates that should be overlayed with trees
			foreach (var tree in info.Trees)
			{
				var treeInfo = w.Map.Rules.Actors[tree].TraitInfo<TreeInfo>();
				treeTypes.Add(treeInfo.Template, tree);
			}

			// Take all templates to overlay from the map
			foreach (var cell in w.Map.AllCells.Where(cell => treeTypes.ContainsKey(w.Map.Tiles[cell].Type)))
				ConvertTreeToActor(w, cell);
		}

		void ConvertTreeToActor(World w, CPos cell)
		{
			// This cell already has a tree overlaying it from a previous iteration
			if (trees[cell] != null)
				return;

			// Correlate the tile "image" aka subtile with its position to find the template origin
			var tile = w.Map.Tiles[cell].Type;
			var index = w.Map.Tiles[cell].Index;
			var template = w.Map.Rules.TileSet.Templates[tile];
			var ni = cell.X - index % template.Size.X;
			var nj = cell.Y - index / template.Size.X;

			// Create a new actor for this bridge and keep track of which subtiles this bridge includes
			var tree = w.CreateActor(treeTypes[tile], new TypeDictionary
			{
				new LocationInit(new CPos(ni, nj)),
				new OwnerInit(w.WorldActor.Owner),
			});
		}
	}
}
