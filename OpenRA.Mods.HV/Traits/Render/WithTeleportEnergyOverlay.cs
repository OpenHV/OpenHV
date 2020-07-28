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
	[Desc("This actor display an overlay upon arrival.")]
	public class WithTeleportEnergyOverlayInfo : TraitInfo, Requires<RenderSpritesInfo>, Requires<BodyOrientationInfo>
	{
		[Desc("Defaults to the actor name.")]
		public readonly string Image = null;

		[SequenceReference("Image")]
		[Desc("Sequence to use for charge animation.")]
		public readonly string Sequence = null;

		[Desc("Which sprite body to play the animation on.")]
		public readonly string Body = "body";

		[Desc("Position relative to body")]
		public readonly WVec Offset = WVec.Zero;

		[PaletteReference]
		[Desc("Custom palette name")]
		public readonly string Palette = null;

		public override object Create(ActorInitializer init) { return new WithTeleportEnergyOverlay(init, this); }
	}

	public class WithTeleportEnergyOverlay : INotifyExitTeleporter
	{
		readonly WithTeleportEnergyOverlayInfo info;
		readonly Animation overlay;
		readonly AnimationWithOffset animation;
		readonly RenderSprites renderSprites;

		public WithTeleportEnergyOverlay(ActorInitializer init, WithTeleportEnergyOverlayInfo info)
		{
			this.info = info;
			renderSprites = init.Self.Trait<RenderSprites>();
			overlay = new Animation(init.World, info.Image ?? renderSprites.GetImage(init.Self));
			var body = init.Self.Trait<BodyOrientation>();
			animation = new AnimationWithOffset(overlay,
				() => body.LocalToWorld(info.Offset.Rotate(body.QuantizeOrientation(init.Self, init.Self.Orientation))),
				() => false);
		}

		void INotifyExitTeleporter.Arrived(Actor self)
		{
			renderSprites.Add(animation, info.Palette);

			// Remove the animation once it is complete
			overlay.PlayBackwardsThen(info.Sequence, () => self.World.AddFrameEndTask(w => renderSprites.Remove(animation)));
		}
	}
}
