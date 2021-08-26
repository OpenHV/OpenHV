#region Copyright & License Information
/*
 * Copyright 2021 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits.Render
{
	[Desc("This actor displays an open/close animation when child actors leave/enter.")]
	public class WithCarrierHatchAnimationInfo : TraitInfo, Requires<WithSpriteBodyInfo>, Requires<RenderSpritesInfo>, Requires<CarrierParentInfo>
	{
		[SequenceReference]
		[Desc("Sequence to use for charge animation.")]
		public readonly string DoorSequence = "open";

		[Desc("Which sprite body to play the animation on.")]
		public readonly string Body = "body";

		public override object Create(ActorInitializer init) { return new WithCarrierHatchAnimation(init, this); }
	}

	public class WithCarrierHatchAnimation : INotifyEnterCarrier, INotifyExitCarrier
	{
		readonly WithCarrierHatchAnimationInfo info;
		readonly WithSpriteBody body;

		public WithCarrierHatchAnimation(ActorInitializer init, WithCarrierHatchAnimationInfo info)
		{
			this.info = info;
			body = init.Self.TraitsImplementing<WithSpriteBody>().Single(w => w.Info.Name == info.Body);
		}

		void INotifyEnterCarrier.Approaching(Actor self, Actor child)
		{
			body.PlayCustomAnimation(self, info.DoorSequence);
		}

		void INotifyEnterCarrier.Landed(Actor self, Actor child)
		{
			body.PlayCustomAnimationBackwards(self, info.DoorSequence, () => body.CancelCustomAnimation(self));
		}

		void INotifyExitCarrier.Opening(Actor self, Actor child)
		{
			body.PlayCustomAnimation(self, info.DoorSequence);
		}

		void INotifyExitCarrier.Closing(Actor self, Actor child)
		{
			body.PlayCustomAnimationBackwards(self, info.DoorSequence, () => body.CancelCustomAnimation(self));
		}
	}
}
