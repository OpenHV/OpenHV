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
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits.Render
{
	[Desc("Displays a custom UI overlay relative to the actor's mouseover bounds.")]
	public class WithSpriteSelectionDecorationsInfo : WithDecorationBaseInfo
	{
		[Desc("Image used for this decoration. Defaults to the actor's type.")]
		public readonly string Image = "outline";

		[SequenceReference("Image")]
		[Desc("Sequence used for this decoration (can be animated).")]
		public readonly string Sequence = "top-left";

		[PaletteReference("IsPlayerPalette")]
		[Desc("Palette to render the sprite in. Reference the world actor's PaletteFrom* traits.")]
		public readonly string Palette = "cursor";

		[Desc("Custom palette is a player palette BaseName")]
		public readonly bool IsPlayerPalette = false;

		public override object Create(ActorInitializer init) { return new WithSpriteSelectionDecorations(init.Self, this); }
	}

	public class WithSpriteSelectionDecorations : WithDecorationBase<WithSpriteSelectionDecorationsInfo>
	{
		protected Animation anim;
		readonly string image;
		readonly WithSpriteSelectionDecorationsInfo info;

		public WithSpriteSelectionDecorations(Actor self, WithSpriteSelectionDecorationsInfo info)
			: base(self, info)
		{
			this.info = info;
			image = info.Image ?? self.Info.Name;
			anim = new Animation(self.World, image, () => self.World.Paused);
		}

		protected virtual PaletteReference GetPalette(Actor self, WorldRenderer wr)
		{
			return wr.Palette(Info.Palette + (Info.IsPlayerPalette ? self.Owner.InternalName : ""));
		}

		protected override IEnumerable<IRenderable> RenderDecoration(Actor self, WorldRenderer wr, int2 screenPos)
		{
			anim.PlayRepeating(info.Sequence);

			return new IRenderable[]
			{
				new UISpriteRenderable(anim.Image, self.CenterPosition, screenPos, 0, GetPalette(self, wr), 1f)
			};
		}
	}
}
