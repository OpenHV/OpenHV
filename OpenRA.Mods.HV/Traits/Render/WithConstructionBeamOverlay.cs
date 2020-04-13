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
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits.Render
{
	[Desc("Rendered after the \"make\" animation.")]
	public class WithConstructionBeamOverlayInfo : ConditionalTraitInfo, Requires<RenderSpritesInfo>
	{
		[SequenceReference]
		[Desc("Sequence name to use")]
		public readonly string Sequence = "make-overlay";

		[PaletteReference("IsPlayerPalette")]
		[Desc("Custom palette name")]
		public readonly string Palette = null;

		[Desc("Custom palette is a player palette BaseName")]
		public readonly bool IsPlayerPalette = false;

		public override object Create(ActorInitializer init) { return new WithConstructionBeamOverlay(init, this); }
	}

	public class WithConstructionBeamOverlay : ConditionalTrait<WithConstructionBeamOverlayInfo>
	{
		readonly Animation overlay;
		readonly RenderSprites renderSprites;
		readonly WithConstructionBeamOverlayInfo info;
		readonly AnimationWithOffset anim;

		public WithConstructionBeamOverlay(ActorInitializer init, WithConstructionBeamOverlayInfo info)
			: base(info)
		{
			this.info = info;

			if (init.Contains<SkipMakeAnimsInit>())
				return;

			renderSprites = init.Self.Trait<RenderSprites>();

			overlay = new Animation(init.World, renderSprites.GetImage(init.Self));
			anim = new AnimationWithOffset(overlay, null, () => IsTraitDisabled);
			renderSprites.Add(anim, info.Palette, info.IsPlayerPalette);
		}

		protected override void TraitEnabled(Actor self)
		{
			// Remove the animation once it is complete
			if (overlay != null && renderSprites != null && anim != null)
				overlay.PlayThen(info.Sequence, () => self.World.AddFrameEndTask(w => renderSprites.Remove(anim)));
		}
	}
}
