#region Copyright & License Information
/*
 * Copyright 2019-2022 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits.Render
{
	[Desc("Play an animation when a unit exits after production finished.")]
	class WithProductionDoorAnimationInfo : ConditionalTraitInfo, Requires<WithSpriteBodyInfo>
	{
		[FieldLoader.Require]
		[Desc("Exit offset associated with the animation.")]
		public readonly CVec[] ExitCells = Array.Empty<CVec>();

		[SequenceReference]
		public readonly string Sequence = "door";

		[Desc("Which sprite body to play the animation on.")]
		public readonly string Body = "body";

		[Desc("Replay the animation backwards after it was played once?")]
		public readonly bool ReplayBackwards = false;

		public override object Create(ActorInitializer init) { return new WithProductionDoorAnimation(init.Self, this); }
	}

	class WithProductionDoorAnimation : ConditionalTrait<WithProductionDoorAnimationInfo>, INotifyProduction
	{
		readonly WithSpriteBody body;

		public WithProductionDoorAnimation(Actor self, WithProductionDoorAnimationInfo info)
			: base(info)
		{
			body = self.TraitsImplementing<WithSpriteBody>().Single(w => w.Info.Name == info.Body);
		}

		void INotifyProduction.UnitProduced(Actor self, Actor other, CPos exit)
		{
			if (!IsTraitDisabled && Info.ExitCells.Contains(exit - self.Location))
				body.PlayCustomAnimation(self, Info.Sequence,
					() =>
					{
						if (Info.ReplayBackwards)
							body.PlayCustomAnimationBackwards(self, Info.Sequence);
					});
		}

		protected override void TraitDisabled(Actor self)
		{
			body.CancelCustomAnimation(self);
		}
	}
}
