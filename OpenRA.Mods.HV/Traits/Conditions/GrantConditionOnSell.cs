#region Copyright & License Information
/*
 * Copyright 2022 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Grants a condition when the actor is being sold.")]
	public class GrantConditionOnSellInfo : TraitInfo
	{
		[GrantedConditionReference]
		[Desc("The condition to grant.")]
		public readonly string Condition = null;

		public override object Create(ActorInitializer init) { return new GrantConditionOnSell(this); }
	}

	public class GrantConditionOnSell : INotifySold
	{
		readonly GrantConditionOnSellInfo info;

		int token = Actor.InvalidConditionToken;

		public GrantConditionOnSell(GrantConditionOnSellInfo info)
		{
			this.info = info;
		}

		void INotifySold.Selling(Actor self)
		{
			if (token == Actor.InvalidConditionToken)
				token = self.GrantCondition(info.Condition);
		}

		void INotifySold.Sold(Actor self)
		{
			if (token != Actor.InvalidConditionToken)
				token = self.RevokeCondition(token);
		}
	}
}
