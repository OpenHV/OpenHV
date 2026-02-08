#region Copyright & License Information
/*
 * Copyright 2019-2026 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[TraitLocation(SystemActors.World)]
	[Desc("Allows resources below actors. Attach this to the world actor.")]
	public class UndergroundResourceLayerInfo : TraitInfo, IResourceLayerInfo, IMapPreviewSignatureInfo
	{
		[FieldLoader.LoadUsing(nameof(LoadResourceTypes))]
		public readonly Dictionary<string, ResourceLayerInfo.ResourceTypeInfo> ResourceTypes = null;

		// Copied to EditorResourceLayerInfo, ResourceRendererInfo
		protected static object LoadResourceTypes(MiniYaml yaml)
		{
			var dictionary = new Dictionary<string, ResourceLayerInfo.ResourceTypeInfo>();
			var resources = yaml.Nodes.FirstOrDefault(n => n.Key == "ResourceTypes");
			if (resources != null)
				foreach (var r in resources.Value.Nodes)
					dictionary[r.Key] = new ResourceLayerInfo.ResourceTypeInfo(r.Value);

			return dictionary;
		}

		public static void PopulateMapPreviewSignatureCells(Map map,
			Dictionary<string, ResourceLayerInfo.ResourceTypeInfo> resources, List<(MPos Uv, Color Color)> destinationBuffer)
		{
			var terrainInfo = map.Rules.TerrainInfo;
			var colors = resources.Values.ToDictionary(
				r => r.ResourceIndex,
				r => terrainInfo.TerrainTypes[terrainInfo.GetTerrainIndex(r.TerrainType)].Color);

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

		void IMapPreviewSignatureInfo.PopulateMapPreviewSignatureCells(Map map, ActorInfo ai, ActorReference s, List<(MPos Uv, Color Color)> destinationBuffer)
		{
			PopulateMapPreviewSignatureCells(map, ResourceTypes, destinationBuffer);
		}

		bool IResourceLayerInfo.TryGetTerrainType(string resourceType, out string terrainType)
		{
			if (resourceType == null || !ResourceTypes.TryGetValue(resourceType, out var resourceInfo))
			{
				terrainType = null;
				return false;
			}

			terrainType = resourceInfo.TerrainType;
			return true;
		}

		bool IResourceLayerInfo.TryGetResourceIndex(string resourceType, out byte index)
		{
			if (resourceType == null || !ResourceTypes.TryGetValue(resourceType, out var resourceInfo))
			{
				index = 0;
				return false;
			}

			index = resourceInfo.ResourceIndex;
			return true;
		}

		public override object Create(ActorInitializer init) { return new UndergroundResourceLayer(init.Self, this); }
	}

	public class UndergroundResourceLayer : IResourceLayer, IWorldLoaded
	{
		readonly UndergroundResourceLayerInfo info;
		readonly World world;
		protected readonly Map Map;
		protected readonly CellLayer<ResourceLayerContents> Content;
		protected readonly Dictionary<byte, string> ResourceTypesByIndex;

		public int TotalResourceCells { get; set; }

		IResourceLayerInfo IResourceLayer.Info => info;

		public event Action<CPos, string> CellChanged;

		public UndergroundResourceLayer(Actor self, UndergroundResourceLayerInfo info)
		{
			this.info = info;
			world = self.World;
			Map = world.Map;
			Content = new CellLayer<ResourceLayerContents>(Map);
			ResourceTypesByIndex = info.ResourceTypes.ToDictionary(
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

				if (!AllowResourceAt(resourceType, cell))
					continue;

				Content[cell] = CreateResourceCell(resourceType, cell, resource.Index);
			}
		}

		void IWorldLoaded.WorldLoaded(World w, WorldRenderer wr) { WorldLoaded(w, wr); }

		protected virtual bool AllowResourceAt(string resourceType, CPos cell)
		{
			if (!Map.Contains(cell) || Map.Ramp[cell] != 0)
				return false;

			if (resourceType == null || !info.ResourceTypes.TryGetValue(resourceType, out var resourceInfo))
				return false;

			if (resourceInfo.AllowedTerrainTypes.First().Trim() == "*")
				return true;

			if (!resourceInfo.AllowedTerrainTypes.Contains(Map.GetTerrainInfo(cell).Type))
				return false;

			return true;
		}

		ResourceLayerContents CreateResourceCell(string resourceType, CPos cell, int density)
		{
			if (!info.ResourceTypes.TryGetValue(resourceType, out var resourceInfo))
			{
				world.Map.CustomTerrain[cell] = byte.MaxValue;
				return ResourceLayerContents.Empty;
			}

			world.Map.CustomTerrain[cell] = world.Map.Rules.TerrainInfo.GetTerrainIndex(resourceInfo.TerrainType);
			++TotalResourceCells;

			return new ResourceLayerContents(resourceType, (byte)density.Clamp(1, resourceInfo.MaxDensity));
		}

		bool CanAddResource(string resourceType, CPos cell, int amount = 1)
		{
			if (!world.Map.Contains(cell))
				return false;

			if (resourceType == null || !info.ResourceTypes.TryGetValue(resourceType, out var resourceInfo))
				return false;

			var content = Content[cell];
			if (content.Type == null)
				return amount <= resourceInfo.MaxDensity && AllowResourceAt(resourceType, cell);

			if (content.Type != resourceType)
				return false;

			return content.Density + amount <= resourceInfo.MaxDensity;
		}

		int AddResource(string resourceType, CPos cell, byte amount = 1)
		{
			if (!Content.Contains(cell))
				return 0;

			if (resourceType == null || !info.ResourceTypes.TryGetValue(resourceType, out var resourceInfo))
				return 0;

			var content = Content[cell];
			if (content.Type == null)
				content = CreateResourceCell(resourceType, cell, 0);

			if (content.Type != resourceType)
				return 0;

			var oldDensity = content.Density;
			var density = (byte)Math.Min(resourceInfo.MaxDensity, oldDensity + amount);
			Content[cell] = new ResourceLayerContents(content.Type, density);

			CellChanged?.Invoke(cell, content.Type);

			return density - oldDensity;
		}

		int RemoveResource(string resourceType, CPos cell, int amount = 1)
		{
			if (!Content.Contains(cell))
				return 0;

			var content = Content[cell];
			if (content.Type == null || content.Type != resourceType)
				return 0;

			var oldDensity = content.Density;
			var density = (byte)Math.Max(0, oldDensity - amount);

			if (density == 0)
			{
				Content[cell] = ResourceLayerContents.Empty;
				Map.CustomTerrain[cell] = byte.MaxValue;
				--TotalResourceCells;

				CellChanged?.Invoke(cell, null);
			}
			else
			{
				Content[cell] = new ResourceLayerContents(content.Type, density);
				CellChanged?.Invoke(cell, content.Type);
			}

			return oldDensity - density;
		}

		void ClearResources(CPos cell)
		{
			if (!Content.Contains(cell))
				return;

			// Don't break other users of CustomTerrain if there are no resources
			var content = Content[cell];
			if (content.Type == null)
				return;

			Content[cell] = ResourceLayerContents.Empty;
			Map.CustomTerrain[cell] = byte.MaxValue;
			--TotalResourceCells;

			CellChanged?.Invoke(cell, null);
		}

		ResourceLayerContents IResourceLayer.GetResource(CPos cell) { return Content.Contains(cell) ? Content[cell] : default; }
		byte IResourceLayer.GetMaxDensity(string resourceType)
		{
			if (!info.ResourceTypes.TryGetValue(resourceType, out var resourceInfo))
				return 0;

			return resourceInfo.MaxDensity;
		}

		bool IResourceLayer.CanAddResource(string resourceType, CPos cell, byte amount) { return CanAddResource(resourceType, cell, amount); }
		int IResourceLayer.AddResource(string resourceType, CPos cell, byte amount) { return AddResource(resourceType, cell, amount); }
		int IResourceLayer.RemoveResource(string resourceType, CPos cell, byte amount) { return RemoveResource(resourceType, cell, amount); }
		void IResourceLayer.ClearResources(CPos cell) { ClearResources(cell); }
		bool IResourceLayer.IsVisible(CPos cell) { return !world.FogObscures(cell); }
		bool IResourceLayer.IsEmpty => TotalResourceCells < 1;
	}
}
