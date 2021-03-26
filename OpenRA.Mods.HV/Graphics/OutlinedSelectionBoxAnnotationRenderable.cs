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
	public struct OutlinedSelectionBoxAnnotationRenderable : IRenderable, IFinalizedRenderable
	{
		readonly WPos pos;
		readonly Actor actor;
		readonly bool displayHealth;
		readonly bool displayExtra;
		readonly Rectangle decorationBounds;

		public OutlinedSelectionBoxAnnotationRenderable(Actor actor, Rectangle decorationBounds, bool displayHealth, bool displayExtra)
			: this(actor.CenterPosition, actor, decorationBounds)
		{
			this.displayHealth = displayHealth;
			this.displayExtra = displayExtra;
		}

		public OutlinedSelectionBoxAnnotationRenderable(WPos pos, Actor actor, Rectangle decorationBounds)
			: this()
		{
			this.pos = pos;
			this.actor = actor;
			this.decorationBounds = decorationBounds;
		}

		public WPos Pos { get { return pos; } }
		public bool DisplayHealth { get { return displayHealth; } }
		public bool DisplayExtra { get { return displayExtra; } }

		public PaletteReference Palette { get { return null; } }
		public int ZOffset { get { return 0; } }
		public bool IsDecoration { get { return true; } }

		public IRenderable WithPalette(PaletteReference newPalette) { return this; }
		public IRenderable WithZOffset(int newOffset) { return this; }
		public IRenderable OffsetBy(in WVec vec) { return new OutlinedSelectionBoxAnnotationRenderable(pos + vec, actor, decorationBounds); }
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

		void DrawSelectionBar(float2 start, float2 end, float value, Color barColor)
		{
			var c = Color.FromArgb(128, 30, 30, 30);
			var c2 = Color.FromArgb(128, 10, 10, 10);
			var p = new float2(0, -6);
			var q = new float2(0, -4);
			var r = new float2(0, -2);

			var barColor2 = Color.FromArgb(255, barColor.R / 2, barColor.G / 2, barColor.B / 2);

			var z = float3.Lerp(start, end, value);
			var cr = Game.Renderer.RgbaColorRenderer;
			cr.DrawLine(start + p, end + p, 2, c);
			cr.DrawLine(start + q, end + q, 2, c2);
			cr.DrawLine(start + r, end + r, 2, c);

			cr.DrawLine(start + p, z + p, 2, barColor2);
			cr.DrawLine(start + q, z + q, 2, barColor);
			cr.DrawLine(start + r, z + r, 2, barColor2);
		}

		Color GetHealthColor(IHealth health)
		{
			if (Game.Settings.Game.UsePlayerStanceColors)
				return actor.Owner.PlayerRelationshipColor(actor);

			return health.DamageState == DamageState.Critical ? Color.Crimson :
				health.DamageState == DamageState.Heavy ? Color.Gold : Color.Green;
		}

		void DrawHealthBar(IHealth health, float2 start, float2 end)
		{
			if (health == null || health.IsDead)
				return;

			var emptyOutline = Color.FromArgb(128, 30, 30, 30);
			var emptyBar = Color.FromArgb(128, 10, 10, 10);

			var p = new float2(0, -6);
			var q = new float2(0, -4); // unten nach mitte
			var r = new float2(0, -2);

			var cr = Game.Renderer.RgbaColorRenderer;

			cr.DrawLine(start + p, end + p, 2, emptyOutline);
			cr.DrawLine(start + q, end + q, 3, emptyBar);
			cr.DrawLine(start + r, end + r, 2, emptyOutline);

			var healthColor = GetHealthColor(health);
			var outline = Color.Black;

			var z = float3.Lerp(start, end, (float)health.HP / health.MaxHP);

			cr.DrawLine(start + p, z + p, 2, outline);
			cr.DrawLine(start + q, z + q, 3, healthColor);
			cr.DrawLine(start + r, z + r, 2, outline);

			if (health.DisplayHP != health.HP)
			{
				var deltaColor = Color.OrangeRed;
				var deltaColor2 = Color.FromArgb(
					255,
					deltaColor.R / 2,
					deltaColor.G / 2,
					deltaColor.B / 2);
				var zz = float3.Lerp(start, end, (float)health.DisplayHP / health.MaxHP);

				cr.DrawLine(z + p, zz + p, 2, deltaColor2);
				cr.DrawLine(z + q, zz + q, 2, deltaColor);
				cr.DrawLine(z + r, zz + r, 2, deltaColor2);
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
