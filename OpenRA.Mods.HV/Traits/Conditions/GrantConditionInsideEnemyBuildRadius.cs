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

using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Grants a condition when the actor enters an enemy build radius.")]
	public class GrantConditionInsideEnemyBuildRadiusInfo : TraitInfo
	{
		[GrantedConditionReference]
		[Desc("The condition to grant.")]
		public readonly string Condition = null;

		public override object Create(ActorInitializer init) { return new GrantConditionInsideEnemyBuildRadius(this); }
	}

	public class GrantConditionInsideEnemyBuildRadius : ITick
	{
		readonly GrantConditionInsideEnemyBuildRadiusInfo info;

		int token = Actor.InvalidConditionToken;

		public GrantConditionInsideEnemyBuildRadius(GrantConditionInsideEnemyBuildRadiusInfo info)
		{
			this.info = info;
		}

		void ITick.Tick(Actor self)
		{
			if (IntersectsEnemyBuildRadius(self))
				GrantCondition(self, info.Condition);
			else
				RevokeCondition(self);
		}

		static bool IntersectsEnemyBuildRadius(Actor self)
		 {
			foreach (var baseProvider in self.World.ActorsWithTrait<BaseProvider>())
			{
				if (baseProvider.Actor.Owner.IsAlliedWith(self.Owner))
					continue;

				var target = Target.FromPos(baseProvider.Actor.CenterPosition);
				if (target.IsInRange(self.CenterPosition, baseProvider.Trait.Info.Range))
					return true;
			}

			return false;
		 }

		void GrantCondition(Actor self, string condition)
		{
			if (token == Actor.InvalidConditionToken)
				token = self.GrantCondition(condition);
		}

		void RevokeCondition(Actor self)
		{
			if (token != Actor.InvalidConditionToken)
				token = self.RevokeCondition(token);
		}
	}
}
