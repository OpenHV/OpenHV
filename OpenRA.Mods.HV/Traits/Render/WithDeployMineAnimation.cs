#region Copyright & License Information
/*
 * Copyright 2023 The OpenHV Developers (see CREDITS)
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
	public class WithDeployMineAnimationInfo : ConditionalTraitInfo, Requires<WithSpriteBodyInfo>, Requires<MinelayerInfo>
	{
		[SequenceReference]
		[Desc("Displayed while laying mine.")]
		public readonly string Sequence = string.Empty;

		[Desc("Which sprite body to modify.")]
		public readonly string Body = "body";

		public override object Create(ActorInitializer init)
		{
			return new WithDeployMineAnimation(init, this);
		}
	}

	public class WithDeployMineAnimation : ConditionalTrait<WithDeployMineAnimationInfo>, INotifyMineLaying
	{
		readonly WithSpriteBody body;

		public WithDeployMineAnimation(ActorInitializer init, WithDeployMineAnimationInfo info)
			: base(info)
		{
			body = init.Self.TraitsImplementing<WithSpriteBody>().Single(w => w.Info.Name == Info.Body);
		}

		void INotifyMineLaying.MineLaid(Actor self, Actor mine)
		{
			body.CancelCustomAnimation(self);
		}

		void INotifyMineLaying.MineLaying(Actor self, CPos location)
		{
			if (!IsTraitDisabled && !body.IsTraitDisabled && !string.IsNullOrEmpty(Info.Sequence))
				body.PlayCustomAnimationRepeating(self, Info.Sequence);
		}

		void INotifyMineLaying.MineLayingCanceled(Actor self, CPos location)
		{
			body.CancelCustomAnimation(self);
		}
	}
}
