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

		protected override int2 GetDecorationPosition(Actor self, WorldRenderer wr, DecorationPosition pos)
		{
			var bounds = interactable.DecorationBounds(self, wr);
			switch (pos)
			{
				case DecorationPosition.TopLeft: return bounds.TopLeft;
				case DecorationPosition.TopRight: return bounds.TopRight;
				case DecorationPosition.BottomLeft: return bounds.BottomLeft;
				case DecorationPosition.BottomRight: return bounds.BottomRight;
				case DecorationPosition.Top: return new int2(bounds.Left + bounds.Size.Width / 2, bounds.Top);
				default: return bounds.TopLeft + new int2(bounds.Size.Width / 2, bounds.Size.Height / 2);
			}
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
