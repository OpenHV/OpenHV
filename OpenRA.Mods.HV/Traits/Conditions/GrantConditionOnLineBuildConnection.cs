#region Copyright & License Information
/*
 * Copyright 2021-2022 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Grants a condition when this actor is next to two walls.")]
	sealed class GrantConditionOnLineBuildConnectionInfo : TraitInfo, Requires<BuildingInfo>
	{
		[GrantedConditionReference]
		[FieldLoader.Require]
		[Desc("The condition to grant when left/top is connected.")]
		public readonly string FirstCondition = null;

		[GrantedConditionReference]
		[FieldLoader.Require]
		[Desc("The condition to grant when right/bottom is connected.")]
		public readonly string LastCondition = null;

		[Desc("Possible connections left/right or top/bottom from top left origin.")]
		public readonly CVec[] Edges = null;

		public override object Create(ActorInitializer init) { return new GrantConditionOnLineBuildConnection(init.Self, this); }
	}

	class GrantConditionOnLineBuildConnection : INotifyAddedToWorld, INotifyRemovedFromWorld
	{
		readonly GrantConditionOnLineBuildConnectionInfo info;
		readonly List<CPos> possibleConnections = new();

		int firstToken = Actor.InvalidConditionToken;
		int lastToken = Actor.InvalidConditionToken;

		int firstTriggerId;
		int lastTriggerId;

		public GrantConditionOnLineBuildConnection(Actor self, GrantConditionOnLineBuildConnectionInfo info)
		{
			this.info = info;

			var building = self.Trait<Building>();
			foreach (var edge in info.Edges)
				possibleConnections.Add(building.TopLeft + edge);
		}

		void INotifyAddedToWorld.AddedToWorld(Actor self)
		{
			void OnEntryFirst(Actor actor)
			{
				if (actor.TraitOrDefault<LineBuild>() != null)
					firstToken = self.GrantCondition(info.FirstCondition);
			}

			void OnExitFirst(Actor actor)
			{
				if (firstToken != Actor.InvalidConditionToken)
					firstToken = self.RevokeCondition(firstToken);
			}

			var first = new CPos[] { possibleConnections.First() };
			firstTriggerId = self.World.ActorMap.AddCellTrigger(first, OnEntryFirst, OnExitFirst);

			void OnEntryLast(Actor actor)
			{
				if (actor.TraitOrDefault<LineBuild>() != null)
					lastToken = self.GrantCondition(info.LastCondition);
			}

			void OnExitLast(Actor actor)
			{
				if (lastToken != Actor.InvalidConditionToken)
					lastToken = self.RevokeCondition(lastToken);
			}

			var last = new CPos[] { possibleConnections.Last() };
			lastTriggerId = self.World.ActorMap.AddCellTrigger(last, OnEntryLast, OnExitLast);
		}

		void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
		{
			self.World.ActorMap.RemoveCellTrigger(firstTriggerId);
			self.World.ActorMap.RemoveCellTrigger(lastTriggerId);
		}
	}
}
