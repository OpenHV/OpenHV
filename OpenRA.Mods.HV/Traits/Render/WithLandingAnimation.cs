#region Copyright & License Information
/*
 * Copyright 2019-2021 The OpenHV Developers (see CREDITS)
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
	public class WithLandingAnimationInfo : TraitInfo, Requires<IFacingInfo>, Requires<WithSpriteBodyInfo>
	{
		public readonly WAngle RequiredFacing = WAngle.Zero;

		[SequenceReference]
		public readonly string LandingSequence = "landing";

		[SequenceReference]
		public readonly string TouchdownSequence = "touchdown";

		[SequenceReference]
		public readonly string LiftoffSequence = "liftoff";

		[Desc("Which sprite body to play the animation on.")]
		public readonly string Body = "body";

		public override object Create(ActorInitializer init) { return new WithLandingAnimation(init, this); }
	}

	public class WithLandingAnimation : ITick
	{
		readonly WithLandingAnimationInfo info;
		readonly Actor self;
		readonly IFacing facing;
		readonly WithSpriteBody body;
		bool landed;

		public WithLandingAnimation(ActorInitializer init, WithLandingAnimationInfo info)
		{
			this.info = info;
			self = init.Self;
			facing = self.Trait<IFacing>();
			body = init.Self.TraitsImplementing<WithSpriteBody>().Single(w => w.Info.Name == info.Body);
		}

		public bool ShouldTouchdown()
		{
			if (self.World.Map.DistanceAboveTerrain(self.CenterPosition).Length > 0)
				return false;

			return facing.Facing == info.RequiredFacing;
		}

		void Touchdown()
		{
			if (landed || !body.DefaultAnimation.HasSequence(info.LandingSequence))
				return;

			landed = true;
			body.PlayCustomAnimation(self, info.LandingSequence, () =>
			{
				if (body.DefaultAnimation.HasSequence(info.TouchdownSequence))
					body.PlayCustomAnimationRepeating(self, info.TouchdownSequence);
			});
		}

		void Liftoff()
		{
			if (!landed || !body.DefaultAnimation.HasSequence(info.LiftoffSequence))
				return;

			landed = false;
			body.PlayCustomAnimation(self, info.LiftoffSequence);
		}

		void ITick.Tick(Actor self)
		{
			if (ShouldTouchdown())
				Touchdown();
			else
				Liftoff();
		}
	}
}
