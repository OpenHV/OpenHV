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
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits.Render
{
	[Desc("Play an animation when a unit exits after production finished.")]
	class WithProductionDoorAnimationInfo : ConditionalTraitInfo, Requires<WithSpriteBodyInfo>
	{
		[SequenceReference]
		public readonly string Sequence = "door";

		[Desc("Which sprite body to play the animation on.")]
		public readonly string Body = "body";

		public override object Create(ActorInitializer init) { return new WithProductionDoorAnimation(init.Self, this); }
	}

	class WithProductionDoorAnimation : ConditionalTrait<WithProductionDoorAnimationInfo>, INotifyProduction
	{
		readonly WithSpriteBody wsb;

		public WithProductionDoorAnimation(Actor self, WithProductionDoorAnimationInfo info)
			: base(info)
		{
			wsb = self.TraitsImplementing<WithSpriteBody>().Single(w => w.Info.Name == info.Body);
		}

		void INotifyProduction.UnitProduced(Actor self, Actor other, CPos exit)
		{
			if (!IsTraitDisabled)
				wsb.PlayCustomAnimation(self, Info.Sequence,
					() => wsb.PlayCustomAnimationBackwards(self, Info.Sequence));
		}

		protected override void TraitDisabled(Actor self)
		{
			wsb.CancelCustomAnimation(self);
		}
	}
}
