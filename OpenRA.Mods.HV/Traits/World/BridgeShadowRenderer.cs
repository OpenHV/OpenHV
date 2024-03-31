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

using System;
using System.Collections.Generic;
using OpenRA.Graphics;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[TraitLocation(SystemActors.World | SystemActors.EditorWorld)]
	[Desc("Used to render decorational shadows that can be manually placed without tooltips.")]
	public class BridgeShadowRendererInfo : TraitInfo, Requires<IResourceLayerInfo>, IMapPreviewSignatureInfo
	{
		public class BridgeShadowTypeInfo
		{
			[Desc("Sequence image that holds the different variants.")]
			public readonly string Image = "bridge-shadows";

			[FieldLoader.Require]
			[SequenceReference(nameof(Image))]
			[Desc("Sprite sequence to be rendered.")]
			public readonly string Sequence = null;

			[PaletteReference]
			[Desc("Palette used for rendering the sprites.")]
			public readonly string Palette = TileSet.TerrainPaletteInternalName;

			public BridgeShadowTypeInfo(MiniYaml yaml)
			{
				FieldLoader.Load(this, yaml);
			}
		}

		[FieldLoader.LoadUsing(nameof(LoadResourceTypes))]
		public readonly Dictionary<string, BridgeShadowTypeInfo> BridgeShadowTypes = null;

		// Copied from ResourceLayerInfo
		protected static object LoadResourceTypes(MiniYaml yaml)
		{
			var ret = new Dictionary<string, BridgeShadowTypeInfo>();
			var resources = yaml.NodeWithKeyOrDefault("BridgeShadowTypes");
			if (resources != null)
				foreach (var r in resources.Value.Nodes)
					ret[r.Key] = new BridgeShadowTypeInfo(r.Value);

			return ret;
		}

		void IMapPreviewSignatureInfo.PopulateMapPreviewSignatureCells(Map map, ActorInfo ai, ActorReference s, List<(MPos Uv, Color Color)> destinationBuffer)
		{
			var resourceLayer = ai.TraitInfoOrDefault<IResourceLayerInfo>();
			if (resourceLayer == null)
				return;

			var terrainInfo = map.Rules.TerrainInfo;
			var colors = new Dictionary<byte, Color>();
			foreach (var r in BridgeShadowTypes.Keys)
			{
				if (!resourceLayer.TryGetResourceIndex(r, out var resourceIndex) || !resourceLayer.TryGetTerrainType(r, out var terrainType))
					continue;

				var info = terrainInfo.TerrainTypes[terrainInfo.GetTerrainIndex(terrainType)];
				colors.Add(resourceIndex, info.Color);
			}

			for (var i = 0; i < map.MapSize.X; i++)
			{
				for (var j = 0; j < map.MapSize.Y; j++)
				{
					var cell = new MPos(i, j);
					if (colors.TryGetValue(map.Resources[cell].Type, out var color))
						destinationBuffer.Add((cell, color));
				}
			}
		}

		public override object Create(ActorInitializer init) { return new BridgeShadowRenderer(init.Self, this); }
	}

	public class BridgeShadowRenderer : IResourceRenderer, IWorldLoaded, IRenderOverlay, ITickRender, INotifyActorDisposing, IRadarTerrainLayer
	{
		protected readonly BridgeShadowRendererInfo Info;
		protected readonly IResourceLayer ResourceLayer;
		protected readonly CellLayer<RendererCellContents> RenderContents;
		protected readonly Dictionary<string, ISpriteSequence> Variants = new();
		protected readonly World World;

		readonly HashSet<CPos> dirty = new();
		readonly Queue<CPos> cleanDirty = new();
		TerrainSpriteLayer spriteLayer;
		bool disposed;

		public BridgeShadowRenderer(Actor self, BridgeShadowRendererInfo info)
		{
			Info = info;
			World = self.World;
			ResourceLayer = self.Trait<IResourceLayer>();
			ResourceLayer.CellChanged += AddDirtyCell;
			RenderContents = new CellLayer<RendererCellContents>(self.World.Map);
		}

		void AddDirtyCell(CPos cell, string resourceType)
		{
			if (resourceType == null || Info.BridgeShadowTypes.ContainsKey(resourceType))
				dirty.Add(cell);
		}

		protected virtual void WorldLoaded(World w, WorldRenderer wr)
		{
			var sequences = w.Map.Sequences;
			foreach (var kv in Info.BridgeShadowTypes)
			{
				var shadowInfo = kv.Value;
				var shadowVariants = sequences.GetSequence(shadowInfo.Image, shadowInfo.Sequence);
				Variants.Add(kv.Key, shadowVariants);

				if (spriteLayer == null)
				{
					var first = shadowVariants.GetSprite(0);
					var emptySprite = new Sprite(first.Sheet, Rectangle.Empty, TextureChannel.Alpha);
					spriteLayer = new TerrainSpriteLayer(w, wr, emptySprite, first.BlendMode, wr.World.Type != WorldType.Editor);
				}
			}

			// Initialize the RenderContent with the initial map state so it is visible
			// through the fog with the Explored Map option enabled
			foreach (var cell in w.Map.AllCells)
			{
				var resource = ResourceLayer.GetResource(cell);
				var rendererCellContents = CreateRenderCellContents(wr, resource, cell);
				if (rendererCellContents.Type != null)
				{
					RenderContents[cell] = rendererCellContents;
					UpdateRenderedSprite(cell, rendererCellContents);
				}
			}
		}

		void IWorldLoaded.WorldLoaded(World w, WorldRenderer wr) { WorldLoaded(w, wr); }

		protected RendererCellContents CreateRenderCellContents(WorldRenderer wr, ResourceLayerContents contents, CPos cell)
		{
			if (contents.Type != null && contents.Density > 0 && Info.BridgeShadowTypes.TryGetValue(contents.Type, out var resourceInfo))
				return new RendererCellContents(contents.Type, contents.Density, resourceInfo, ChooseVariant(contents.Type, cell), wr.Palette(resourceInfo.Palette));

			return RendererCellContents.Empty;
		}

		protected void UpdateSpriteLayers(CPos cell, ISpriteSequence sequence, int frame, PaletteReference palette)
		{
			// resource.Type is meaningless (and may be null) if resource.Sequence is null
			if (sequence != null)
				spriteLayer.Update(cell, sequence, palette, frame);
			else
				spriteLayer.Clear(cell);
		}

		void IRenderOverlay.Render(WorldRenderer wr)
		{
			spriteLayer.Draw(wr.Viewport);
		}

		void ITickRender.TickRender(WorldRenderer wr, Actor self)
		{
			foreach (var cell in dirty)
			{
				if (!ResourceLayer.IsVisible(cell))
					continue;

				var rendererCellContents = RendererCellContents.Empty;
				var contents = ResourceLayer.GetResource(cell);
				if (contents.Density > 0)
				{
					rendererCellContents = RenderContents[cell];

					// Contents are the same, so just update the density
					if (rendererCellContents.Type == contents.Type)
						rendererCellContents = new RendererCellContents(rendererCellContents, contents.Density);
					else
						rendererCellContents = CreateRenderCellContents(wr, contents, cell);
				}

				RenderContents[cell] = rendererCellContents;
				UpdateRenderedSprite(cell, rendererCellContents);
				cleanDirty.Enqueue(cell);
			}

			while (cleanDirty.Count > 0)
				dirty.Remove(cleanDirty.Dequeue());
		}

		protected virtual void UpdateRenderedSprite(CPos cell, RendererCellContents content)
		{
			if (content.Density > 0)
			{
				var maxDensity = ResourceLayer.GetMaxDensity(content.Type);
				var frame = int2.Lerp(0, content.Sequence.Length - 1, content.Density, maxDensity);
				UpdateSpriteLayers(cell, content.Sequence, frame, content.Palette);
			}
			else
				UpdateSpriteLayers(cell, null, 0, null);
		}

		protected virtual void Disposing(Actor self)
		{
			if (disposed)
				return;

			spriteLayer.Dispose();

			ResourceLayer.CellChanged -= AddDirtyCell;

			disposed = true;
		}

		void INotifyActorDisposing.Disposing(Actor self) { Disposing(self); }

		protected virtual ISpriteSequence ChooseVariant(string shadowType, CPos cell)
		{
			return Variants[shadowType];
		}

		protected virtual string GetRenderedResourceType(CPos cell) { return RenderContents[cell].Type; }

		protected virtual string GetRenderedResourceTooltip(CPos cell) { return null; }

		IEnumerable<string> IResourceRenderer.ResourceTypes => Info.BridgeShadowTypes.Keys;

		string IResourceRenderer.GetRenderedResourceType(CPos cell) { return GetRenderedResourceType(cell); }

		string IResourceRenderer.GetRenderedResourceTooltip(CPos cell) { return GetRenderedResourceTooltip(cell); }

		IEnumerable<IRenderable> IResourceRenderer.RenderUIPreview(WorldRenderer wr, string shadowType, int2 origin, float scale)
		{
			if (!Variants.TryGetValue(shadowType, out var variant))
				yield break;

			if (!Info.BridgeShadowTypes.TryGetValue(shadowType, out var shadowInfo))
				yield break;

			var sprite = variant.GetSprite(variant.Length - 1);
			var palette = wr.Palette(shadowInfo.Palette);

			yield return new UISpriteRenderable(sprite, WPos.Zero, origin, 0, palette, scale);
		}

		IEnumerable<IRenderable> IResourceRenderer.RenderPreview(WorldRenderer wr, string shadowType, WPos origin)
		{
			if (!Variants.TryGetValue(shadowType, out var variant))
				yield break;

			if (!Info.BridgeShadowTypes.TryGetValue(shadowType, out var shadowInfo))
				yield break;

			var sprite = variant.GetSprite(variant.Length - 1);
			var alpha = variant.GetAlpha(variant.Length - 1);
			var palette = wr.Palette(shadowInfo.Palette);
			var tintModifiers = variant.IgnoreWorldTint ? TintModifiers.IgnoreWorldTint : TintModifiers.None;

			yield return new SpriteRenderable(sprite, origin, WVec.Zero, 0, palette, variant.Scale, alpha, float3.Ones, tintModifiers, false);
		}

		event Action<CPos> IRadarTerrainLayer.CellEntryChanged
		{
			add => RenderContents.CellEntryChanged += value;
			remove => RenderContents.CellEntryChanged -= value;
		}

		bool IRadarTerrainLayer.TryGetTerrainColorPair(MPos uv, out (Color Left, Color Right) value)
		{
			value = default;

			var type = RenderContents[uv].Type;
			if (type == null)
				return false;

			if (!ResourceLayer.Info.TryGetTerrainType(type, out var terrainType))
				return false;

			var terrainInfo = World.Map.Rules.TerrainInfo;
			var info = terrainInfo.TerrainTypes[terrainInfo.GetTerrainIndex(terrainType)];

			value = (info.Color, info.Color);
			return true;
		}

		public readonly struct RendererCellContents
		{
			public readonly string Type;
			public readonly BridgeShadowRendererInfo.BridgeShadowTypeInfo Info;
			public readonly ISpriteSequence Sequence;
			public readonly PaletteReference Palette;
			public readonly int Density;

			public static readonly RendererCellContents Empty = default;

			public RendererCellContents(string resourceType, int density, BridgeShadowRendererInfo.BridgeShadowTypeInfo info, ISpriteSequence sequence, PaletteReference palette)
			{
				Type = resourceType;
				Density = density;
				Info = info;
				Sequence = sequence;
				Palette = palette;
			}

			public RendererCellContents(RendererCellContents contents, int density)
			{
				Type = contents.Type;
				Density = density;
				Info = contents.Info;
				Sequence = contents.Sequence;
				Palette = contents.Palette;
			}
		}
	}
}
