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
	[Desc("Renders the fillness state.")]
	public class WithResourceCollectorOverlayInfo : PausableConditionalTraitInfo, Requires<RenderSpritesInfo>, Requires<ResourceCollectorInfo>
	{
		[SequenceReference]
		[Desc("Sequence name to use")]
		public readonly string Sequence = "silo";

		[Desc("Position relative to body")]
		public readonly WVec Offset = WVec.Zero;

		[PaletteReference(nameof(IsPlayerPalette))]
		[Desc("Custom palette name")]
		public readonly string Palette = null;

		[Desc("Custom palette is a player palette BaseName")]
		public readonly bool IsPlayerPalette = false;

		public override object Create(ActorInitializer init) { return new WithResourceCollectorOverlay(init.Self, this); }
	}

	public class WithResourceCollectorOverlay : PausableConditionalTrait<WithResourceCollectorOverlayInfo>
	{
		readonly Animation overlay;

		public WithResourceCollectorOverlay(Actor self, WithResourceCollectorOverlayInfo info)
			: base(info)
		{
			var rs = self.Trait<RenderSprites>();
			var resourceCollector = self.Trait<ResourceCollector>();

			overlay = new Animation(self.World, rs.GetImage(self), () => IsTraitPaused);
			overlay.PlayFetchIndex(info.Sequence, () =>
				(10 * overlay.CurrentSequence.Length - 1) * resourceCollector.Truckload / (10 * resourceCollector.Info.Capacity));

			var anim = new AnimationWithOffset(overlay, null,
				() => IsTraitDisabled || resourceCollector.Truckload == 0,
				p => RenderUtils.ZOffsetFromCenter(self, p, 1));

			rs.Add(anim, info.Palette, info.IsPlayerPalette);
		}
	}
}
