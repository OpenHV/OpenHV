#region Copyright & License Information
/*
 * Copyright 2024 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Mods.Common.Effects;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Renders the bubble effects.")]
	public class WithTeleportGroupOverlayInfo : TraitInfo
	{
		[Desc("Image used for the teleport effects. Defaults to the actor's type.")]
		public readonly string Image = null;

		[Desc("Sequence used for the effect played where the unit jumped from.")]
		[SequenceReference("Image")]
		public readonly string LaunchSequence = null;

		[Desc("Sequence used for the effect played where the unit jumped to.")]
		[SequenceReference("Image")]
		public readonly string AppearSequence = null;

		[Desc("Palette to render the teleport in/out sprites in.")]
		[PaletteReference]
		public readonly string Palette = "effect";

		public override object Create(ActorInitializer init) { return new WithTeleportGroupOverlay(init, this); }
	}

	public class WithTeleportGroupOverlay : INotifyTeleportation
	{
		readonly WithTeleportGroupOverlayInfo info;
		readonly Actor self;

		public WithTeleportGroupOverlay(ActorInitializer init, WithTeleportGroupOverlayInfo info)
		{
			self = init.Self;
			this.info = info;
		}

		void INotifyTeleportation.Teleporting(WPos from, WPos to)
		{
			var image = info.Image ?? self.Info.Name;

			self.World.AddFrameEndTask(w =>
			{
				if (info.LaunchSequence != null)
					w.Add(new SpriteEffect(from, w, image, info.LaunchSequence, info.Palette));

				if (info.AppearSequence != null)
					w.Add(new SpriteEffect(to, w, image, info.AppearSequence, info.Palette));
			});
		}
	}
}
