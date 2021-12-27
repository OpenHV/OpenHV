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
	public class WithLandingUnloadAnimationInfo : TraitInfo, Requires<WithSpriteBodyInfo>, Requires<AircraftInfo>
	{
		public readonly WAngle RequiredFacing = WAngle.Zero;

		[SequenceReference]
		public readonly string LandingSequence = "landing";

		[SequenceReference]
		public readonly string UnloadedSequence = "unloaded";

		[Desc("Which sprite body to play the animation on.")]
		public readonly string Body = "body";

		public override object Create(ActorInitializer init) { return new WithLandingUnloadAnimation(init, this); }
	}

	public class WithLandingUnloadAnimation : ITick
	{
		readonly WithLandingUnloadAnimationInfo info;
		readonly Actor self;
		readonly Aircraft aircraft;
		readonly WithSpriteBody body;
		bool landed;

		public WithLandingUnloadAnimation(ActorInitializer init, WithLandingUnloadAnimationInfo info)
		{
			this.info = info;
			self = init.Self;
			aircraft = self.Trait<Aircraft>();
			body = init.Self.TraitsImplementing<WithSpriteBody>().Single(w => w.Info.Name == info.Body);
		}

		void Touchdown()
		{
			if (landed || !body.DefaultAnimation.HasSequence(info.LandingSequence))
				return;

			landed = true;
			body.PlayCustomAnimationRepeating(self, info.LandingSequence);
		}

		void Liftoff()
		{
			if (!landed || !body.DefaultAnimation.HasSequence(info.UnloadedSequence))
				return;

			landed = false;
			body.PlayCustomAnimationRepeating(self, info.UnloadedSequence);
		}

		void ITick.Tick(Actor self)
		{
			if (aircraft.AtLandAltitude)
				Touchdown();
			else
				Liftoff();
		}
	}
}
