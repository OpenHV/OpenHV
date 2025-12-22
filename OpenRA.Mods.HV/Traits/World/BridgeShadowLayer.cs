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

using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[TraitLocation(SystemActors.World)]
	[Desc("Allows shadows below actors. Attach this to the world actor.")]
	public class BridgeShadowLayerInfo : TraitInfo, IMapPreviewSignatureInfo
	{
		[FieldLoader.Require]
		[Desc("Terrain type used to determine unit movement and minimap colors.")]
		public readonly string TerrainType = null;

		public class BridgeShadowTypeInfo
		{
			[FieldLoader.Require]
			[Desc("Resource index in the binary map data.")]
			public readonly byte ResourceIndex = 0;

			public BridgeShadowTypeInfo(MiniYaml yaml)
			{
				FieldLoader.Load(this, yaml);
			}
		}

		[FieldLoader.LoadUsing(nameof(LoadBridgeShadowTypes))]
		public readonly Dictionary<string, BridgeShadowTypeInfo> BridgeShadowTypes = null;

		// Copied to EditorResourceLayerInfo, ResourceRendererInfo
		protected static object LoadBridgeShadowTypes(MiniYaml yaml)
		{
			var dictionary = new Dictionary<string, BridgeShadowTypeInfo>();
			var resources = yaml.Nodes.FirstOrDefault(n => n.Key == "BridgeShadowTypes");
			if (resources != null)
				foreach (var r in resources.Value.Nodes)
					dictionary[r.Key] = new BridgeShadowTypeInfo(r.Value);

			return dictionary;
		}

		public void PopulateMapPreviewSignatureCells(Map map, Dictionary<string, BridgeShadowTypeInfo> resources,
			List<(MPos Uv, Color Color)> destinationBuffer)
		{
			var terrainInfo = map.Rules.TerrainInfo;
			var colors = resources.Values.ToDictionary(
				r => r.ResourceIndex,
				r => terrainInfo.TerrainTypes[terrainInfo.GetTerrainIndex(TerrainType)].Color);

			for (var i = 0; i < map.MapSize.Width; i++)
			{
				for (var j = 0; j < map.MapSize.Height; j++)
				{
					var cell = new MPos(i, j);
					if (colors.TryGetValue(map.Resources[cell].Type, out var color))
						destinationBuffer.Add((cell, color));
				}
			}
		}

		public bool TryGetTerrainType(string bridgeShadowType, out string terrainType)
		{
			if (bridgeShadowType == null || !BridgeShadowTypes.ContainsKey(bridgeShadowType))
			{
				terrainType = null;
				return false;
			}

			terrainType = TerrainType;
			return true;
		}

		void IMapPreviewSignatureInfo.PopulateMapPreviewSignatureCells(Map map, ActorInfo ai, ActorReference s, List<(MPos Uv, Color Color)> destinationBuffer)
		{
			PopulateMapPreviewSignatureCells(map, BridgeShadowTypes, destinationBuffer);
		}

		public bool TryGetResourceIndex(string bridgeShadowType, out byte index)
		{
			if (bridgeShadowType == null || !BridgeShadowTypes.TryGetValue(bridgeShadowType, out var bridgeShadowInfo))
			{
				index = 0;
				return false;
			}

			index = bridgeShadowInfo.ResourceIndex;
			return true;
		}

		public override object Create(ActorInitializer init) { return new BridgeShadowLayer(init.Self, this); }
	}

	public class BridgeShadowLayer : IWorldLoaded
	{
		public BridgeShadowLayerInfo Info;

		readonly World world;
		protected readonly Map Map;
		protected readonly CellLayer<ResourceLayerContents> Content;
		protected readonly Dictionary<byte, string> ResourceTypesByIndex;

		public BridgeShadowLayer(Actor self, BridgeShadowLayerInfo info)
		{
			Info = info;
			world = self.World;
			Map = world.Map;
			Content = new CellLayer<ResourceLayerContents>(Map);
			ResourceTypesByIndex = info.BridgeShadowTypes.ToDictionary(
				kv => kv.Value.ResourceIndex,
				kv => kv.Key);
		}

		protected virtual void WorldLoaded(World w, WorldRenderer wr)
		{
			foreach (var cell in w.Map.AllCells)
			{
				var resource = world.Map.Resources[cell];
				if (!ResourceTypesByIndex.TryGetValue(resource.Type, out var resourceType))
					continue;

				if (!AllowBridgeShadowAt(resourceType, cell))
					continue;

				Content[cell] = CreateResourceCell(resourceType, cell, resource.Index);
			}
		}

		void IWorldLoaded.WorldLoaded(World w, WorldRenderer wr) { WorldLoaded(w, wr); }

		protected virtual bool AllowBridgeShadowAt(string bridgeShadowType, CPos cell)
		{
			if (!Map.Contains(cell) || Map.Ramp[cell] != 0)
				return false;

			if (bridgeShadowType == null || !Info.BridgeShadowTypes.ContainsKey(bridgeShadowType))
				return false;

			return true;
		}

		ResourceLayerContents CreateResourceCell(string resourceType, CPos cell, int density)
		{
			if (!Info.BridgeShadowTypes.ContainsKey(resourceType))
			{
				world.Map.CustomTerrain[cell] = byte.MaxValue;
				return ResourceLayerContents.Empty;
			}

			return new ResourceLayerContents(resourceType, (byte)density.Clamp(1, 1));
		}

		public ResourceLayerContents GetResource(CPos cell) { return Content.Contains(cell) ? Content[cell] : default; }
		public byte GetMaxDensity(string resourceType)
		{
			if (!Info.BridgeShadowTypes.ContainsKey(resourceType))
				return 0;

			return 1;
		}

		public bool IsVisible(CPos cell) { return !world.FogObscures(cell); }
	}
}
