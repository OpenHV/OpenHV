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

using System.Collections.Immutable;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Primitives;

namespace OpenRA.Mods.HV.Graphics
{
	public class EnergyBoltRenderable : IRenderable, IFinalizedRenderable
	{
		readonly ImmutableArray<WPos> offsets;
		readonly WDist width;
		readonly Color color;

		public EnergyBoltRenderable(ImmutableArray<WPos> offsets, int zOffset, WDist width, Color color)
		{
			this.offsets = offsets;
			ZOffset = zOffset;
			this.width = width;
			this.color = color;
		}

		public WPos Pos => new(offsets[0].X, offsets[0].Y, 0);
		public int ZOffset { get; }
		public bool IsDecoration => true;

		public IRenderable WithZOffset(int newOffset) { return new EnergyBoltRenderable(offsets, newOffset, width, color); }
		public IRenderable OffsetBy(in WVec vec)
		{
			// Lambdas can't use 'in' variables, so capture a copy for later
			var vector = vec;
			return new EnergyBoltRenderable(offsets.Select(offset => offset + vector).ToImmutableArray(), ZOffset, width, color);
		}

		public IRenderable AsDecoration() { return this; }

		public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }
		public void Render(WorldRenderer wr)
		{
			var screenWidth = wr.ScreenVector(new WVec(width, WDist.Zero, WDist.Zero))[0];

			Game.Renderer.WorldRgbaColorRenderer.DrawLine(offsets.Select(wr.Screen3DPosition), screenWidth, color, false);
		}

		public void RenderDebugGeometry(WorldRenderer wr) { }
		public Rectangle ScreenBounds(WorldRenderer wr) { return Rectangle.Empty; }
	}
}
