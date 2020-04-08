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

using System.Linq;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits.Render
{
	[Desc("This actor displays a charge-up animation when it receives a teleported unit.")]
	public class WithTeleportChargeAnimationInfo : ITraitInfo, Requires<WithSpriteBodyInfo>, Requires<RenderSpritesInfo>
	{
		[SequenceReference]
		[Desc("Sequence to use for charge animation.")]
		public readonly string ChargeSequence = "active";

		[Desc("Which sprite body to play the animation on.")]
		public readonly string Body = "body";

		public object Create(ActorInitializer init) { return new WithTeleportChargeAnimation(init, this); }
	}

	public class WithTeleportChargeAnimation : INotifyEnterTeleporter
	{
		readonly WithTeleportChargeAnimationInfo info;
		readonly WithSpriteBody body;

		public WithTeleportChargeAnimation(ActorInitializer init, WithTeleportChargeAnimationInfo info)
		{
			this.info = info;
			body = init.Self.TraitsImplementing<WithSpriteBody>().Single(w => w.Info.Name == info.Body);
		}

		void INotifyEnterTeleporter.Charging(Actor self, Actor teleporter)
		{
			body.PlayCustomAnimation(teleporter, info.ChargeSequence, () => body.CancelCustomAnimation(teleporter));
		}
	}
}
