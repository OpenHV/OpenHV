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

using OpenRA.Graphics;
using OpenRA.Primitives;

namespace OpenRA.Mods.HV.Graphics
{
	public class CustomSelectionBoxAnnotationRenderable : IRenderable, IFinalizedRenderable
	{
		readonly Rectangle decorationBounds;
		readonly Color color;

		public CustomSelectionBoxAnnotationRenderable(Actor actor, Rectangle decorationBounds, Color color)
			: this(actor.CenterPosition, decorationBounds, color) { }

		public CustomSelectionBoxAnnotationRenderable(WPos pos, Rectangle decorationBounds, Color color)
		{
			Pos = pos;
			this.decorationBounds = decorationBounds;
			this.color = Color.FromArgb(255, 168, 210, 244);
		}

		public WPos Pos { get; }

		public int ZOffset => 0;
		public bool IsDecoration => true;

		public IRenderable WithZOffset(int newOffset) { return this; }
		public IRenderable OffsetBy(in WVec vec) { return new CustomSelectionBoxAnnotationRenderable(Pos + vec, decorationBounds, color); }
		public IRenderable AsDecoration() { return this; }

		public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }
		public void Render(WorldRenderer wr)
		{
			var tl = wr.Viewport.WorldToViewPx(new float2(decorationBounds.Left, decorationBounds.Top)).ToFloat2();
			var br = wr.Viewport.WorldToViewPx(new float2(decorationBounds.Right, decorationBounds.Bottom)).ToFloat2();
			var tr = new float2(br.X, tl.Y);
			var bl = new float2(tl.X, br.Y);
			var u = new float2(3, 0);
			var v = new float2(0, 3);

			var cr = Game.Renderer.RgbaColorRenderer;
			cr.DrawLine(new float3[] { tl + u, tl, tl + v }, 1, color, true);
			cr.DrawLine(new float3[] { tr - u, tr, tr + v }, 1, color, true);
			cr.DrawLine(new float3[] { br - u, br, br - v }, 1, color, true);
			cr.DrawLine(new float3[] { bl + u, bl, bl - v }, 1, color, true);
		}

		public void RenderDebugGeometry(WorldRenderer wr) { }
		public Rectangle ScreenBounds(WorldRenderer wr) { return Rectangle.Empty; }
	}
}
