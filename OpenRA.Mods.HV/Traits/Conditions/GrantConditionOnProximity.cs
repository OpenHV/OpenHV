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

using System.Collections.Generic;
using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Grants a condition by units in a specified proximity.")]
	public class GrantConditionOnProximityInfo : TraitInfo
	{
		[Desc("Maximum range at which a actor can initiate the condition.")]
		public readonly WDist Range = WDist.FromCells(5);

		[GrantedConditionReference]
		[Desc("The condition to grant.")]
		public readonly string Condition = null;

		public override object Create(ActorInitializer init) { return new GrantConditionOnProximity(init.Self, this); }
	}

	public class GrantConditionOnProximity : ITick, INotifyAddedToWorld, INotifyRemovedFromWorld
	{
		bool Taken => token != Actor.InvalidConditionToken;

		readonly GrantConditionOnProximityInfo info;
		readonly Actor self;
		readonly List<Actor> actorsInRange = new();

		int proximityTrigger;
		WPos previousPosition;
		int token = Actor.InvalidConditionToken;

		public GrantConditionOnProximity(Actor self, GrantConditionOnProximityInfo info)
		{
			this.info = info;
			this.self = self;
		}

		void INotifyAddedToWorld.AddedToWorld(Actor self)
		{
			proximityTrigger = self.World.ActorMap.AddProximityTrigger(self.CenterPosition, info.Range, WDist.Zero, ActorEntered, ActorLeft);
		}

		void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
		{
			self.World.ActorMap.RemoveProximityTrigger(proximityTrigger);
			actorsInRange.Clear();
		}

		void ITick.Tick(Actor self)
		{
			if (!self.IsInWorld || self.CenterPosition == previousPosition)
				return;

			self.World.ActorMap.UpdateProximityTrigger(proximityTrigger, self.CenterPosition, info.Range, WDist.Zero);
			previousPosition = self.CenterPosition;
		}

		void ActorEntered(Actor other)
		{
			if (other != self)
				actorsInRange.Add(other);

			UpdateCondition();
		}

		void ActorLeft(Actor other)
		{
			if (other != self)
				actorsInRange.Remove(other);

			UpdateCondition();
		}

		void UpdateCondition()
		{
			var actorInRangeTheLongest = actorsInRange.FirstOrDefault();
			if (actorInRangeTheLongest == null)
			{
				if (Taken)
					RevokeCondition(self);
			}
			else
			{
				if (!Taken)
					GrantCondition(self, info.Condition);
			}
		}

		void GrantCondition(Actor self, string condition)
		{
			token = self.GrantCondition(condition);
		}

		void RevokeCondition(Actor self)
		{
			token = self.RevokeCondition(token);
		}
	}
}
