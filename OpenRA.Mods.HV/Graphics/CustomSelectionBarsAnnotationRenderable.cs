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
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Graphics
{
	public class CustomSelectionBarsAnnotationRenderable : IRenderable, IFinalizedRenderable
	{
		readonly Actor actor;
		readonly Rectangle decorationBounds;

		public CustomSelectionBarsAnnotationRenderable(Actor actor, Rectangle decorationBounds, bool displayHealth, bool displayExtra)
			: this(actor.CenterPosition, actor, decorationBounds)
		{
			DisplayHealth = displayHealth;
			DisplayExtra = displayExtra;
		}

		public CustomSelectionBarsAnnotationRenderable(WPos pos, Actor actor, Rectangle decorationBounds)
		{
			Pos = pos;
			this.actor = actor;
			this.decorationBounds = decorationBounds;
		}

		public WPos Pos { get; }
		public bool DisplayHealth { get; }
		public bool DisplayExtra { get; }

		public int ZOffset => 0;
		public bool IsDecoration => true;

		public IRenderable WithZOffset(int newOffset) { return this; }
		public IRenderable OffsetBy(in WVec vec) { return new CustomSelectionBarsAnnotationRenderable(Pos + vec, actor, decorationBounds); }
		public IRenderable AsDecoration() { return this; }

		void DrawExtraBars(float2 start, float2 end)
		{
			foreach (var extraBar in actor.TraitsImplementing<ISelectionBar>())
			{
				var value = extraBar.GetValue();
				if (value != 0 || extraBar.DisplayWhenEmpty)
				{
					var offset = new float2(0, 4);
					start += offset;
					end += offset;
					DrawSelectionBar(start, end, extraBar.GetValue(), extraBar.GetColor());
				}
			}
		}

		static void DrawSelectionBar(float2 start, float2 end, float value, Color barColor)
		{
			var c = Color.FromArgb(128, 30, 30, 30);
			var c2 = Color.FromArgb(128, 10, 10, 10);
			var p = new float2(0, -4);
			var q = new float2(0, -3);
			var r = new float2(0, -2);

			var barColor2 = Color.FromArgb(255, barColor.R / 2, barColor.G / 2, barColor.B / 2);

			var z = float3.Lerp(start, end, value);
			var cr = Game.Renderer.RgbaColorRenderer;
			cr.DrawLine(start + p, end + p, 1, c);
			cr.DrawLine(start + q, end + q, 1, c2);
			cr.DrawLine(start + r, end + r, 1, c);

			cr.DrawLine(start + p, z + p, 1, barColor2);
			cr.DrawLine(start + q, z + q, 1, barColor);
			cr.DrawLine(start + r, z + r, 1, barColor2);
		}

		Color GetHealthColor(IHealth health)
		{
			return health.DamageState == DamageState.Critical ? Color.FromArgb(255, 128, 21, 21) :
				health.DamageState == DamageState.Heavy ? Color.FromArgb(255, 250, 227, 59) : Color.FromArgb(255, 57, 171, 47);
		}

		void DrawHealthBar(IHealth health, float2 start, float2 end)
		{
			if (health == null || health.IsDead)
				return;

			var blackoutline = Color.FromArgb(255, 4, 4, 4);
			var p = new float2(0, -5);
			var q = new float2(0, -4);
			var r = new float2(0, -3);
			var t = new float2(0, -2);
			var left = new float2(-1, 0);
			var right = new float2(1, 0);

			var healthColor = GetHealthColor(health);
			var healthColor2 = Color.FromArgb(
				255,
				healthColor.R - 20,
				healthColor.G - 20,
				healthColor.B - 20);

			var z = float3.Lerp(start, end, (float)health.HP / health.MaxHP);

			var cr = Game.Renderer.RgbaColorRenderer;
			cr.DrawLine(start + p, end + p, 1, blackoutline);
			cr.DrawLine(start + q, end + q, 1, blackoutline);
			cr.DrawLine(start + r, end + r, 1, blackoutline);
			cr.DrawLine(start + t, end + t, 1, blackoutline);

			cr.DrawLine(start + p, z + p, 1, blackoutline);
			cr.DrawLine(start + q, z + q, 1, healthColor);
			cr.DrawLine(start + r, z + r, 1, healthColor2);
			cr.DrawLine(start + t, z + t, 1, blackoutline);

			cr.DrawLine(start + p + left, start + p, 1, blackoutline);
			cr.DrawLine(start + q + left, start + q, 1, blackoutline);
			cr.DrawLine(start + r + left, start + r, 1, blackoutline);
			cr.DrawLine(start + t + left, start + t, 1, blackoutline);

			cr.DrawLine(end + p, end + p + right, 1, blackoutline);
			cr.DrawLine(end + q, end + q + right, 1, blackoutline);
			cr.DrawLine(end + r, end + r + right, 1, blackoutline);
			cr.DrawLine(end + t, end + t + right, 1, blackoutline);

			if (health.DisplayHP != health.HP)
			{
				var deltaColor = Color.FromArgb(255, 96, 0, 0);
				var deltaColor2 = Color.FromArgb(
					255,
					deltaColor.R / 2,
					deltaColor.G / 2,
					deltaColor.B / 2);
				var zz = float3.Lerp(start, end, (float)health.DisplayHP / health.MaxHP);

				cr.DrawLine(z + p, zz + p, 1, deltaColor2);
				cr.DrawLine(z + q, zz + q, 1, deltaColor);
				cr.DrawLine(z + r, zz + r, 1, deltaColor);
				cr.DrawLine(z + t, zz + t, 1, deltaColor2);
			}
		}

		public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }
		public void Render(WorldRenderer wr)
		{
			if (!actor.IsInWorld || actor.IsDead)
				return;

			var health = actor.TraitOrDefault<IHealth>();
			var start = wr.Viewport.WorldToViewPx(new float2(decorationBounds.Left + 1, decorationBounds.Top));
			var end = wr.Viewport.WorldToViewPx(new float2(decorationBounds.Right - 1, decorationBounds.Top));

			if (DisplayHealth)
				DrawHealthBar(health, start, end);

			if (DisplayExtra)
				DrawExtraBars(start, end);
		}

		public void RenderDebugGeometry(WorldRenderer wr) { }
		public Rectangle ScreenBounds(WorldRenderer wr) { return Rectangle.Empty; }
	}
}
