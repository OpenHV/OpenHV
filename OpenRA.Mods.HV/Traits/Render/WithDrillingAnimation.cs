#region Copyright & License Information
/*
 * Copyright 2019-2023 The OpenHV Developers (see AUTHORS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Mods.HV.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.HV.Render
{
	[Desc("Replaces the building animation when resources are mined.")]
	public class WithDrillingAnimationInfo : TraitInfo, Requires<WithSpriteBodyInfo>, Requires<ResourceCollectorInfo>
	{
		[SequenceReference]
		[Desc("Sequence name to use when mining a full resource spot.")]
		public readonly string Sequence = "active";

		[SequenceReference]
		[Desc("Sequence name to use when resources are getting exhausted")]
		public readonly string DecreasedVolumeSequence = "depleted";

		[Desc("Which sprite body to play the animation on.")]
		public readonly string Body = "body";

		public override object Create(ActorInitializer init) { return new WithDrillingAnimation(init.Self, this); }
	}

	public class WithDrillingAnimation : INotifyResourceCollection
	{
		readonly WithDrillingAnimationInfo info;
		readonly WithSpriteBody body;

		public WithDrillingAnimation(Actor self, WithDrillingAnimationInfo info)
		{
			this.info = info;
			body = self.TraitsImplementing<WithSpriteBody>().Single(w => w.Info.Name == info.Body);
		}

		void INotifyResourceCollection.Mining(Actor self)
		{
			body.PlayCustomAnimationRepeating(self, info.Sequence);
		}

		void INotifyResourceCollection.Suspended(Actor self)
		{
			body.CancelCustomAnimation(self);
		}

		void INotifyResourceCollection.Depletion(Actor self)
		{
			body.PlayCustomAnimationRepeating(self, info.DecreasedVolumeSequence);
		}
	}
}
