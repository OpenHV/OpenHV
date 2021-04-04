#region Copyright & License Information
/*
 * Copyright 2019-2021 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Primitives;
using OpenRA.Support;

namespace OpenRA.Mods.HV.Terrain
{
	public class TheaterTemplate
	{
		public readonly Sprite[] Sprites;
		public readonly int Stride;
		public readonly int Variants;

		public TheaterTemplate(Sprite[] sprites, int stride, int variants)
		{
			Sprites = sprites;
			Stride = stride;
			Variants = variants;
		}
	}

	public sealed class CustomTileCache : IDisposable
	{
		readonly Dictionary<ushort, TheaterTemplate> templates = new Dictionary<ushort, TheaterTemplate>();
		readonly Cache<SheetType, SheetBuilder> sheetBuilders;
		readonly Sprite missingTile;
		readonly MersenneTwister random;

		public CustomTileCache(CustomTerrain terrainInfo, Action<uint, string> onMissingImage = null)
		{
			sheetBuilders = new Cache<SheetType, SheetBuilder>(t => new SheetBuilder(t, terrainInfo.SheetSize));

			random = new MersenneTwister();

			var frameCache = new FrameCache(Game.ModData.DefaultFileSystem, Game.ModData.SpriteLoaders);
			foreach (var t in terrainInfo.Templates)
			{
				var variants = new List<Sprite[]>();
				var templateInfo = (CustomTerrainTemplateInfo)t.Value;

				for (var ii = 0; ii < templateInfo.Images.Length; ii++)
				{
					var i = templateInfo.Images[ii];

					ISpriteFrame[] allFrames;

					if (onMissingImage != null)
					{
						try
						{
							allFrames = frameCache[i];
						}
						catch (FileNotFoundException)
						{
							onMissingImage(t.Key, i);
							continue;
						}
					}
					else
						allFrames = frameCache[i];

					var frameCount = allFrames.Length;
					var indices = templateInfo.Frames != null ? templateInfo.Frames : Exts.MakeArray(t.Value.TilesCount, j => j);

					var start = indices.Min();
					var end = indices.Max();
					if (start < 0 || end >= frameCount)
						throw new YamlException("Template `{0}` uses frames [{1}..{2}] of {3}, but only [0..{4}] actually exist"
							.F(t.Key, start, end, i, frameCount - 1));

					variants.Add(indices.Select(j =>
					{
						var f = allFrames[j];
						var tile = t.Value.Contains(j) ? (CustomTerrainTileInfo)t.Value[j] : null;

						// The internal z axis is inverted from expectation (negative is closer)
						var zOffset = tile != null ? -tile.ZOffset : 0;
						var zRamp = tile != null ? tile.ZRamp : 1f;
						var offset = new float3(f.Offset, zOffset);
						var type = SheetBuilder.FrameTypeToSheetType(f.Type);

						var s = sheetBuilders[type].Allocate(f.Size, zRamp, offset);
						Util.FastCopyIntoChannel(s, f.Data, f.Type);

						return s;
					}).ToArray());
				}

				var allSprites = variants.SelectMany(s => s);

				if (onMissingImage != null && !variants.Any())
					continue;

				templates.Add(t.Value.Id, new TheaterTemplate(allSprites.ToArray(), variants.First().Length, templateInfo.Images.Length));
			}

			// 1x1px transparent tile
			var missingDataLength = 1;
			var missingFrameType = SpriteFrameType.Indexed8;
			var missingSheetType = SheetType.Indexed;

			missingTile = sheetBuilders[missingSheetType].Add(new byte[missingDataLength], missingFrameType, new Size(1, 1));
			foreach (var sb in sheetBuilders.Values)
				sb.Current.ReleaseBuffer();
		}

		public bool HasTileSprite(TerrainTile r, int? variant = null)
		{
			return TileSprite(r, variant) != missingTile;
		}

		public Sprite TileSprite(TerrainTile r, int? variant = null)
		{
			if (!templates.TryGetValue(r.Type, out var template))
				return missingTile;

			if (r.Index >= template.Stride)
				return missingTile;

			var start = template.Variants > 1 ? variant.HasValue ? variant.Value : random.Next(template.Variants) : 0;
			return template.Sprites[start * template.Stride + r.Index];
		}

		public Sprite MissingTile => missingTile;

		public void Dispose()
		{
			foreach (var sb in sheetBuilders.Values)
				sb.Dispose();
		}
	}
}
