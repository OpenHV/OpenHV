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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[TraitLocation(SystemActors.World)]
	public class TerrainTileAnimationInfo : TraitInfo, ILobbyCustomRulesIgnore, Requires<BridgeShadowLayerInfo>
	{
		[FieldLoader.Require]
		public readonly ImmutableArray<ushort> Tiles = default;

		[Desc("Average time (ticks) between animations per 1000 tiles.")]
		public readonly ImmutableArray<int> Interval = [7 * 25, 13 * 25];

		[Desc("Delay (in ticks) before the first animation happens.")]
		public readonly int InitialDelay = 0;

		[FieldLoader.Require]
		[Desc("Which image to use.")]
		public readonly string Image;

		[FieldLoader.Require]
		[Desc("Which sequence to use.")]
		[SequenceReference(nameof(Image))]
		public readonly string Sequence;

		[FieldLoader.Require]
		[Desc("Which palette to use.")]
		[PaletteReference]
		public readonly string Palette;

		public override object Create(ActorInitializer init) { return new TerrainTileAnimation(init.Self, this); }
	}

	public class TerrainTileAnimation : IWorldLoaded, ITick, IRenderOverlay, INotifyActorDisposing
	{
		// Sequences define their frame time in milliseconds.
		const int TickLength = 40;

		// Keeps the animation rate of a tile independent of the amount of tiles.
		const int TilesPerInterval = 1000;

		sealed class AnimatedTile
		{
			public CPos Cell;
			public int Ticks;
			public int Frame;
		}

		readonly TerrainTileAnimationInfo info;
		readonly BridgeShadowLayer bridgeShadowLayer;
		readonly List<AnimatedTile> animations = [];
		readonly HashSet<CPos> animatedCells = [];

		ImmutableArray<CPos> cells = [];
		TerrainSpriteLayer spriteLayer;
		ISpriteSequence sequence;
		PaletteReference palette;
		int duration;
		int ticks;
		int pendingTiles;
		bool disposed;

		public TerrainTileAnimation(Actor self, TerrainTileAnimationInfo info)
		{
			this.info = info;

			ticks = info.InitialDelay;

			bridgeShadowLayer = self.Trait<BridgeShadowLayer>();
		}

		void IWorldLoaded.WorldLoaded(World world, WorldRenderer worldRenderer)
		{
			// Clashes with bridge shadows.
			cells = world.Map.AllCells
				.Where(cell => info.Tiles.Contains(world.Map.Tiles[cell].Type)
					&& bridgeShadowLayer.GetResource(cell).Equals(ResourceLayerContents.Empty))
				.ToImmutableArray();

			if (cells.Length < 1)
				return;

			sequence = world.Map.Sequences.GetSequence(info.Image, info.Sequence);
			palette = worldRenderer.Palette(info.Palette);

			var first = sequence.GetSprite(0);
			var emptySprite = new Sprite(first.Sheet, Rectangle.Empty, TextureChannel.Alpha);
			spriteLayer = new TerrainSpriteLayer(world, worldRenderer, emptySprite, first.BlendMode, world.Type != WorldType.Editor);

			duration = Exts.IntegerDivisionRoundingAwayFromZero(sequence.Length * sequence.Tick, TickLength);
		}

		void ITick.Tick(Actor self)
		{
			if (cells.Length < 1)
				return;

			var world = self.World;
			for (var i = animations.Count - 1; i >= 0; i--)
			{
				var animation = animations[i];
				if (--animation.Ticks <= 0)
				{
					spriteLayer.Clear(animation.Cell);
					animatedCells.Remove(animation.Cell);
					animations.RemoveAt(i);
					continue;
				}

				var frame = (duration - animation.Ticks) * TickLength / sequence.Tick;
				if (frame == animation.Frame)
					continue;

				animation.Frame = frame;
				DrawFrame(world, animation.Cell, frame);
			}

			if (--ticks <= 0)
			{
				ticks = Common.Util.RandomInRange(world.LocalRandom, info.Interval);

				pendingTiles += cells.Length;
				while (pendingTiles >= TilesPerInterval)
				{
					pendingTiles -= TilesPerInterval;

					var cell = cells.Random(world.LocalRandom);

					// The sprite layer only has room for a single animation per tile.
					if (!animatedCells.Add(cell))
						continue;

					animations.Add(new AnimatedTile { Cell = cell, Ticks = duration });
					DrawFrame(world, cell, 0);
				}
			}
		}

		void DrawFrame(World world, CPos cell, int frame)
		{
			if (world.FogObscures(cell))
				spriteLayer.Clear(cell);
			else
				spriteLayer.Update(cell, sequence, palette, frame);
		}

		void IRenderOverlay.Render(WorldRenderer wr)
		{
			spriteLayer?.Draw(wr.Viewport);
		}

		void INotifyActorDisposing.Disposing(Actor self)
		{
			if (disposed)
				return;

			spriteLayer?.Dispose();
			disposed = true;
		}
	}
}
