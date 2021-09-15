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
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits.Render
{
	[Desc("Displays an overlay when `SupportPower` is fully charged.")]
	public class WithSupportPowerChargedAnimationInfo : ConditionalTraitInfo, Requires<RenderSpritesInfo>, Requires<BodyOrientationInfo>
	{
		[SequenceReference]
		[Desc("Sequence name to use")]
		public readonly string ChargeSequence = "charged";

		[SequenceReference]
		[Desc("Sequence name to use")]
		public readonly string LoopSequence = "loop";

		[SequenceReference]
		[Desc("Sequence name to use")]
		public readonly string EndSequence = "";

		[Desc("Which sprite body to play the animation on.")]
		public readonly string Body = "body";

		public override object Create(ActorInitializer init) { return new WithSupportPowerChargedAnimation(init.Self, this); }
	}

	public class WithSupportPowerChargedAnimation : ConditionalTrait<WithSupportPowerChargedAnimationInfo>, INotifySupportPower
	{
		readonly WithSupportPowerChargedAnimationInfo info;
		readonly WithSpriteBody body;

		public WithSupportPowerChargedAnimation(Actor self, WithSupportPowerChargedAnimationInfo info)
			: base(info)
		{
			this.info = info;
			body = self.TraitsImplementing<WithSpriteBody>().Single(w => w.Info.Name == info.Body);
		}

		void INotifySupportPower.Charged(Actor self)
		{
			body.PlayCustomAnimation(self, info.ChargeSequence,
				() => body.PlayCustomAnimationRepeating(self, info.LoopSequence));
		}

		void INotifySupportPower.Activated(Actor self)
		{
			if (!string.IsNullOrEmpty(info.EndSequence))
				body.PlayCustomAnimation(self, info.EndSequence);
		}
	}
}
