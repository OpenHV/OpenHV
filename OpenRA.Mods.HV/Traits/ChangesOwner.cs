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

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Change the owner on condition and revert back when it isn't met anymore.")]
	class ChangesOwnerInfo : ConditionalTraitInfo
	{
		[FieldLoader.Require]
		[Desc("The new owner.")]
		public readonly string Owner = "";

		public override object Create(ActorInitializer init) { return new ChangesOwner(init.Self, this); }
	}

	class ChangesOwner : ConditionalTrait<ChangesOwnerInfo>
	{
		readonly Player oldOwner;
		readonly Player newOwner;

		public ChangesOwner(Actor self, ChangesOwnerInfo info)
			: base(info)
		{
			oldOwner = self.Owner;
			newOwner = self.World.Players.First(p => p.InternalName == info.Owner);
		}

		protected override void TraitEnabled(Actor self)
		{
			self.ChangeOwner(newOwner);
		}

		protected override void TraitDisabled(Actor self)
		{
			self.ChangeOwner(oldOwner);
		}
	}
}
