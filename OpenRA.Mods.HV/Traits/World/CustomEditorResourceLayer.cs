#region Copyright & License Information
/*
 * Copyright 2024 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[TraitLocation(SystemActors.EditorWorld)]
	[Desc("AllowedTerrainTypes is optional: use * as placeholder")]
	sealed class CustomEditorResourceLayerInfo : EditorResourceLayerInfo, Requires<EditorActorLayerInfo>
	{
		public override object Create(ActorInitializer init) { return new CustomEditorResourceLayer(init.Self, this); }
	}

	sealed class CustomEditorResourceLayer : EditorResourceLayer
	{
		readonly CustomEditorResourceLayerInfo info;

		public CustomEditorResourceLayer(Actor self, CustomEditorResourceLayerInfo info)
			: base(self, info)
		{
			this.info = info;
		}

		protected override bool AllowResourceAt(string resourceType, CPos cell)
		{
			var mapResources = Map.Resources;
			if (!mapResources.Contains(cell))
				return false;

			if (!info.ResourceTypes.TryGetValue(resourceType, out var resourceInfo))
				return false;

			if (resourceInfo.AllowedTerrainTypes.First().Trim() == "*")
				return true;

			// Ignore custom terrain types when spawning resources in the editor
			var terrainInfo = Map.Rules.TerrainInfo;
			var terrainType = terrainInfo.TerrainTypes[terrainInfo.GetTerrainInfo(Map.Tiles[cell]).TerrainType].Type;
			if (!resourceInfo.AllowedTerrainTypes.Contains(terrainType))
				return false;

			return true;
		}
	}
}
