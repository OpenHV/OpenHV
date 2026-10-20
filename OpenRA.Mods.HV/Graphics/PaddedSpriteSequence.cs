#region Copyright & License Information
/*
 * Copyright 2026 The OpenHV Developers (see CREDITS)
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
using OpenRA.Mods.Common.Graphics;
using OpenRA.Primitives;

namespace OpenRA.Mods.HV.Graphics
{
	public class PaddedSpriteSequenceLoader : DefaultSpriteSequenceLoader
	{
		public override PaddedSpriteSequence CreateSequence(
			ModData modData, string tileset, SpriteCache cache, string image, string sequence, MiniYaml data, MiniYaml defaults)
		{
			return new PaddedSpriteSequence(cache, this, image, sequence, data, defaults);
		}
	}

	[Desc("Surrounds indexed frames with a copy of their outermost pixels inside the sprite sheet and renders only the original area. ",
		"This stops the pixel art scaling filter from sampling the sheet margin, which is filled with palette index 0 instead of our transparent index.")]
	public class PaddedSpriteSequence : DefaultSpriteSequence
	{
		sealed class PaddedFrame : ISpriteFrame
		{
			public SpriteFrameType Type => SpriteFrameType.Indexed8;
			public Size Size { get; }
			public Size FrameSize { get; }
			public float2 Offset { get; }
			public byte[] Data { get; }
			public bool DisableExportPadding { get; }

			public PaddedFrame(ISpriteFrame frame)
			{
				var width = frame.Size.Width;
				var height = frame.Size.Height;

				Size = new Size(width + 2, height + 2);
				FrameSize = frame.FrameSize;
				Offset = frame.Offset;
				DisableExportPadding = frame.DisableExportPadding;
				Data = new byte[Size.Width * Size.Height];

				for (var y = 0; y < Size.Height; y++)
				{
					var source = Math.Clamp(y - 1, 0, height - 1) * width;
					var target = y * Size.Width;
					Array.Copy(frame.Data, source, Data, target + 1, width);
					Data[target] = frame.Data[source];
					Data[target + Size.Width - 1] = frame.Data[source + width - 1];
				}
			}
		}

		public PaddedSpriteSequence(SpriteCache cache, ISpriteSequenceLoader loader, string image, string sequence, MiniYaml data, MiniYaml defaults)
			: base(cache, loader, image, sequence, data, defaults) { }

		static ISpriteFrame PadFrame(ISpriteFrame frame, int index, int total)
		{
			if (frame.Type != SpriteFrameType.Indexed8 || frame.Size.Width == 0 || frame.Size.Height == 0)
				return frame;

			return new PaddedFrame(frame);
		}

		static Sprite RemovePadding(Sprite sprite)
		{
			var bounds = sprite.Bounds;
			if (sprite.Sheet.Type != SheetType.Indexed || bounds.Width == 0 || bounds.Height == 0)
				return sprite;

			// Flipped sprites have negative dimensions.
			var dx = Math.Sign(bounds.Width);
			var dy = Math.Sign(bounds.Height);
			var inset = Rectangle.FromLTRB(bounds.Left + dx, bounds.Top + dy, bounds.Right - dx, bounds.Bottom - dy);

			return new Sprite(sprite.Sheet, inset, sprite.ZRamp, sprite.Offset, sprite.Channel, sprite.BlendMode);
		}

		public override void ReserveSprites(ModData modData, string tileset, SpriteCache cache, MiniYaml data, MiniYaml defaults)
		{
			var frames = LoadField(Frames, data, defaults);
			var flipX = LoadField(FlipX, data, defaults);
			var flipY = LoadField(FlipY, data, defaults);
			var zRamp = LoadField(ZRamp, data, defaults);
			var offset = LoadField(Offset, data, defaults);
			var blendMode = LoadField(BlendMode, data, defaults);

			// Depth sprites are sized from the frames and would need the same treatment.
			AdjustFrame adjustFrame = depthSpriteReservation == null ? PadFrame : null;

			var combineNode = data.NodeWithKeyOrDefault(Combine.Key);
			if (combineNode != null)
			{
				for (var i = 0; i < combineNode.Value.Nodes.Length; i++)
				{
					var subData = combineNode.Value.Nodes[i].Value;
					var subOffset = LoadField(Offset, subData, NoData);
					var subFlipX = LoadField(FlipX, subData, NoData);
					var subFlipY = LoadField(FlipY, subData, NoData);
					var subFrames = LoadField(Frames, subData);

					foreach (var f in ParseCombineFilenames(modData, tileset, subFrames, subData))
					{
						spritesToLoad.Add(new SpriteReservation
						{
							Token = cache.ReserveSprites(f.Filename, f.LoadFrames, f.Location, adjustFrame),
							Offset = subOffset + offset,
							FlipX = subFlipX ^ flipX,
							FlipY = subFlipY ^ flipY,
							BlendMode = blendMode,
							ZRamp = zRamp,
							Frames = f.Frames
						});
					}
				}
			}
			else
			{
				foreach (var f in ParseFilenames(modData, tileset, frames, data, defaults))
				{
					spritesToLoad.Add(new SpriteReservation
					{
						Token = cache.ReserveSprites(f.Filename, f.LoadFrames, f.Location, adjustFrame),
						Offset = offset,
						FlipX = flipX,
						FlipY = flipY,
						BlendMode = blendMode,
						ZRamp = zRamp,
						Frames = f.Frames,
					});
				}
			}
		}

		public override void ResolveSprites(SpriteCache cache)
		{
			if (bounds != null)
				return;

			base.ResolveSprites(cache);

			if (depthSpriteReservation != null)
				return;

			var unpadded = new Dictionary<Sprite, Sprite>();
			Sprite Unpad(Sprite s) => s == null ? null : unpadded.GetOrAdd(s, RemovePadding);

			sprites = sprites.Select(Unpad).ToArray();
			shadowSprites = shadowSprites?.Select(Unpad).ToArray();
			bounds = sprites.Concat(shadowSprites ?? Enumerable.Empty<Sprite>()).Select(OffsetSpriteBounds).Union();
		}
	}
}
