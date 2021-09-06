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
	[Desc("This actor displays a sprite sequence when spawning a missile.")]
	public class WithMissileLaunchAnimationInfo : TraitInfo, Requires<RenderSpritesInfo>, Requires<BodyOrientationInfo>
	{
		[SequenceReference]
		[Desc("Sequence to use for launch animation.")]
		public readonly string Sequence = "takeoff";

		[Desc("Which sprite body to play the animation on.")]
		public readonly string Body = "body";

		public override object Create(ActorInitializer init) { return new WithMissileLaunchAnimation(init.Self, this); }
	}

	public class WithMissileLaunchAnimation : INotifyMissileSpawn
	{
		readonly WithMissileLaunchAnimationInfo info;
		readonly WithSpriteBody body;

		public WithMissileLaunchAnimation(Actor self, WithMissileLaunchAnimationInfo info)
		{
			this.info = info;
			body = self.TraitsImplementing<WithSpriteBody>().Single(w => w.Info.Name == info.Body);
		}

		void INotifyMissileSpawn.Launching(Actor self, Target target)
		{
			body.PlayCustomAnimation(self, info.Sequence, () => body.CancelCustomAnimation(self));
		}
	}
}
