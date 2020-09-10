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
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits.Render
{
	public class EmptySelectionDecorationsInfo : SelectionDecorationsBaseInfo, Requires<InteractableInfo>
	{
		public override object Create(ActorInitializer init) { return new EmptySelectionDecorations(init.Self, this); }
	}

	[Desc("Required to render and position WithDecoration traits.")]
	public class EmptySelectionDecorations : SelectionDecorationsBase
	{
		readonly Interactable interactable;

		public EmptySelectionDecorations(Actor self, EmptySelectionDecorationsInfo info)
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
				case "Bottom": return new int2(bounds.Left + bounds.Size.Width / 2, bounds.Bottom);
				case "Left": return new int2(bounds.Left, bounds.Top + bounds.Size.Height / 2);
				case "Right": return new int2(bounds.Right, bounds.Top + bounds.Size.Height / 2);
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
				case "Bottom": return new int2(0, -margin.Y);
				case "Left": return new int2(margin.X, 0);
				case "Right": return new int2(-margin.X, 0);
				default: return int2.Zero;
			}
		}

		protected override int2 GetDecorationOrigin(Actor self, WorldRenderer wr, string pos, int2 margin)
		{
			return wr.Viewport.WorldToViewPx(GetDecorationPosition(self, wr, pos)) + GetDecorationMargin(pos, margin);
		}

		protected override IEnumerable<IRenderable> RenderSelectionBox(Actor self, WorldRenderer wr, Color color)
		{
			return Enumerable.Empty<IRenderable>();
		}

		protected override IEnumerable<IRenderable> RenderSelectionBars(Actor self, WorldRenderer wr, bool displayHealth, bool displayExtra)
		{
			return Enumerable.Empty<IRenderable>();
		}
	}
}
