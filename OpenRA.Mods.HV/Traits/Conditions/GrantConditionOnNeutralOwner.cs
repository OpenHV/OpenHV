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

using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Grants a condition if the owner is the Neutral player.")]
	public class GrantConditionOnNeutralOwnerInfo : TraitInfo
	{
		[GrantedConditionReference]
		[Desc("The condition to grant.")]
		public readonly string Condition = null;

		public override object Create(ActorInitializer init) { return new GrantConditionOnNeutralOwner(this); }
	}

	public class GrantConditionOnNeutralOwner : INotifyCreated, INotifyOwnerChanged
	{
		readonly GrantConditionOnNeutralOwnerInfo info;

		int token = Actor.InvalidConditionToken;

		public GrantConditionOnNeutralOwner(GrantConditionOnNeutralOwnerInfo info)
		{
			this.info = info;
		}

		void INotifyCreated.Created(Actor self)
		{
			if (self.Owner.PlayerName == "Neutral")
				GrantCondition(self, info.Condition);
		}

		void GrantCondition(Actor self, string condition)
		{
			token = self.GrantCondition(condition);
		}

		void RevokeCondition(Actor self)
		{
			token = self.RevokeCondition(token);
		}

		void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
		{
			if (newOwner.PlayerName == "Neutral" && token == Actor.InvalidConditionToken)
				GrantCondition(self, info.Condition);
			else if (newOwner.PlayerName != "Neutral" && token != Actor.InvalidConditionToken)
				RevokeCondition(self);
		}
	}
}
