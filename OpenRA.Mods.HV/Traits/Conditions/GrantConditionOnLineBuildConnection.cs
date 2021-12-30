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

using System.Collections.Generic;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Grants a condition when this actor is connected to a wall.")]
	class GrantConditionOnLineBuildConnectionInfo : TraitInfo, Requires<BuildingInfo>
	{
		[GrantedConditionReference]
		[FieldLoader.Require]
		[Desc("The condition to grant.")]
		public readonly string Condition = null;

		[Desc("Possible connections left/right or top/bottom from top left origin.")]
		public readonly CVec[] Edges = null;

		public override object Create(ActorInitializer init) { return new GrantConditionOnLineBuildConnection(init.Self, this); }
	}

	class GrantConditionOnLineBuildConnection : INotifyLineBuildSegmentsChanged
	{
		readonly GrantConditionOnLineBuildConnectionInfo info;
		readonly List<CPos> possibleConnections = new List<CPos>();

		int token = Actor.InvalidConditionToken;
		int segments;

		public GrantConditionOnLineBuildConnection(Actor self, GrantConditionOnLineBuildConnectionInfo info)
		{
			this.info = info;

			var building = self.Trait<Building>();
			foreach (var edge in info.Edges)
				possibleConnections.Add(building.TopLeft + edge);
		}

		void INotifyLineBuildSegmentsChanged.SegmentAdded(Actor self, Actor segment)
		{
			if (possibleConnections.Contains(segment.Location))
			{
				segments++;
				if (segments == 2)
					token = self.GrantCondition(info.Condition);
			}
		}

		void INotifyLineBuildSegmentsChanged.SegmentRemoved(Actor self, Actor segment)
		{
			if (token == Actor.InvalidConditionToken)
				return;

			if (possibleConnections.Contains(segment.Location))
			{
				segments--;
				if (segments == 0)
					token = self.RevokeCondition(token);
			}
		}
	}
}
