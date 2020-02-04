#region Copyright & License Information
/*
 * Copyright 2007-2020 The OpenHV Developers (see AUTHORS)
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
	[Desc("Rendered together with the \"make\" animation.")]
	public class WithConstructionOverlayInfo : ConditionalTraitInfo, Requires<RenderSpritesInfo>, Requires<BodyOrientationInfo>
	{
		[SequenceReference]
		[Desc("Sequence name to use")]
		public readonly string Sequence = "make-overlay";

		[Desc("Position relative to body")]
		public readonly WVec Offset = WVec.Zero;

		[PaletteReference]
		[Desc("Custom palette name")]
		public readonly string Palette = null;

		public override object Create(ActorInitializer init) { return new WithConstructionOverlay(init, this); }
	}

	public class WithConstructionOverlay : ConditionalTrait<WithConstructionOverlayInfo>
	{
		public WithConstructionOverlay(ActorInitializer init, WithConstructionOverlayInfo info)
			: base(info)
		{
			if (init.Contains<SkipMakeAnimsInit>())
				return;

			var rs = init.Self.Trait<RenderSprites>();
			var body = init.Self.Trait<BodyOrientation>();

			var overlay = new Animation(init.World, rs.GetImage(init.Self));
			var anim = new AnimationWithOffset(overlay,
				() => body.LocalToWorld(info.Offset.Rotate(body.QuantizeOrientation(init.Self, init.Self.Orientation))),
				() => IsTraitDisabled);

			// Remove the animation once it is complete
			overlay.PlayThen(info.Sequence, () => init.World.AddFrameEndTask(w => rs.Remove(anim)));

			rs.Add(anim, info.Palette);
		}
	}
}
