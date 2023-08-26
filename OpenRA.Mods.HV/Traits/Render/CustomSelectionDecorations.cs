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

using System.Collections.Generic;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Mods.HV.Graphics;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits.Render
{
	public class CustomSelectionDecorationsInfo : SelectionDecorationsBaseInfo, Requires<InteractableInfo>
	{
		public override object Create(ActorInitializer init) { return new CustomSelectionDecorations(init.Self, this); }
	}

	public class CustomSelectionDecorations : SelectionDecorationsBase, IRender
	{
		readonly Interactable interactable;

		public CustomSelectionDecorations(Actor self, CustomSelectionDecorationsInfo info)
			: base(info)
		{
			interactable = self.Trait<Interactable>();
		}

		int2 GetDecorationPosition(Actor self, WorldRenderer wr, string pos)
		{
			var bounds = interactable.DecorationBounds(self, wr);
			switch (pos)
			{
				case "TopLeft": return bounds.TopLeft;
				case "TopRight": return bounds.TopRight;
				case "BottomLeft": return bounds.BottomLeft;
				case "BottomRight": return bounds.BottomRight;
				case "Top": return new int2(bounds.Left + bounds.Size.Width / 2, bounds.Top);
				default: return bounds.TopLeft + new int2(bounds.Size.Width / 2, bounds.Size.Height / 2);
			}
		}

		static int2 GetDecorationMargin(string pos, int2 margin)
		{
			switch (pos)
			{
				case "TopLeft": return margin;
				case "TopRight": return new int2(-margin.X, margin.Y);
				case "BottomLeft": return new int2(margin.X, -margin.Y);
				case "BottomRight": return -margin;
				case "Top": return new int2(0, margin.Y);
				default: return int2.Zero;
			}
		}

		protected override int2 GetDecorationOrigin(Actor self, WorldRenderer wr, string pos, int2 margin)
		{
			return wr.Viewport.WorldToViewPx(GetDecorationPosition(self, wr, pos)) + GetDecorationMargin(pos, margin);
		}

		protected override IEnumerable<IRenderable> RenderSelectionBox(Actor self, WorldRenderer wr, Color color)
		{
			var bounds = interactable.DecorationBounds(self, wr);
			yield return new CustomSelectionBoxAnnotationRenderable(self, bounds, color);
		}

		protected override IEnumerable<IRenderable> RenderSelectionBars(Actor self, WorldRenderer wr, bool displayHealth, bool displayExtra)
		{
			// Don't render the selection bars for non-selectable actors
			if (interactable is not Selectable || (!displayHealth && !displayExtra))
				yield break;

			var bounds = interactable.DecorationBounds(self, wr);
			yield return new CustomSelectionBarsAnnotationRenderable(self, bounds, displayHealth, displayExtra);
		}

		IEnumerable<IRenderable> IRender.Render(Actor self, WorldRenderer wr)
		{
			yield break;
		}

		IEnumerable<Rectangle> IRender.ScreenBounds(Actor self, WorldRenderer wr)
		{
			yield return interactable.DecorationBounds(self, wr);
		}
	}
}
