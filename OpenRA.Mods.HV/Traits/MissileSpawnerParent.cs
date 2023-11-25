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
using System.Linq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("This actor can spawn missile actors.")]
	public class MissileSpawnerParentInfo : BaseSpawnerParentInfo
	{
		[GrantedConditionReference]
		[Desc("The condition to grant to self right after launching a spawned unit.")]
		public readonly string LaunchingCondition = null;

		[Desc("After this many ticks, we remove the condition.")]
		public readonly int LaunchingTicks = 15;

		[GrantedConditionReference]
		[Desc("The condition to grant to self while spawned units are loaded.",
			"Condition can stack with multiple spawns.")]
		public readonly string LoadedCondition = null;

		[Desc("Conditions to grant when specified actors are contained inside the transport.",
			"A dictionary of [actor id]: [condition].")]
		public readonly Dictionary<string, string> SpawnContainConditions = new();

		[GrantedConditionReference]
		public IEnumerable<string> LinterSpawnContainConditions { get { return SpawnContainConditions.Values; } }

		public override object Create(ActorInitializer init) { return new MissileSpawnerParent(init, this); }
	}

	public class MissileSpawnerParent : BaseSpawnerParent, ITick, INotifyAttack
	{
		readonly Dictionary<string, Stack<int>> spawnContainTokens = new();
		readonly MissileSpawnerParentInfo info;

		readonly Stack<int> loadedTokens = new();

		int respawnTicks = 0;

		int launchCondition = Actor.InvalidConditionToken;
		int launchConditionTicks;

		public MissileSpawnerParent(ActorInitializer init, MissileSpawnerParentInfo info)
			: base(init, info)
		{
			this.info = info;
		}

		protected override void Created(Actor self)
		{
			base.Created(self);

			// Spawn initial load.
			var burst = Info.InitialActorCount == -1 ? Info.Actors.Length : Info.InitialActorCount;
			for (var i = 0; i < burst; i++)
				Replenish(self, ChildEntries);
		}

		public override void OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
		{
			// Do nothing, because missiles can't be captured.
		}

		void INotifyAttack.PreparingAttack(Actor self, in Target target, Armament a, Barrel barrel) { }

		// The rate of fire of the dummy weapon determines the launch cycle as each shot invokes Attacking()
		void INotifyAttack.Attacking(Actor self, in Target target, Armament a, Barrel barrel)
		{
			if (IsTraitDisabled)
				return;

			if (!Info.ArmamentNames.Contains(a.Info.Name))
				return;

			// Issue retarget order for already launched ones
			foreach (var child in ChildEntries)
				if (child.IsValid)
					child.SpawnerChild.Attack(child.Actor, target);

			var childEntry = GetLaunchable();
			if (childEntry == null)
				return;

			foreach (var notify in self.TraitsImplementing<INotifyMissileSpawn>())
				notify.Launching(self, target);

			if (info.LaunchingCondition != null)
			{
				if (launchCondition == Actor.InvalidConditionToken)
					launchCondition = self.GrantCondition(info.LaunchingCondition);

				launchConditionTicks = info.LaunchingTicks;
			}

			// Program the trajectory.
			var missile = childEntry.Actor.Trait<BallisticMissile>();
			missile.Target = Target.FromPos(target.CenterPosition);

			SpawnIntoWorld(self, childEntry.Actor, self.CenterPosition);

			if (spawnContainTokens.TryGetValue(a.Info.Name, out var spawnContainToken) && spawnContainToken.Count > 0)
				self.RevokeCondition(spawnContainToken.Pop());

			if (loadedTokens.Count > 0)
				self.RevokeCondition(loadedTokens.Pop());

			// Queue attack order, too.
			self.World.AddFrameEndTask(w => childEntry.Actor = null); // invalidate the slave entry so that slave will regen.

			// Set clock so that regen happens.
			if (respawnTicks <= 0) // Don't interrupt an already running timer!
				respawnTicks = Info.RespawnTicks;
		}

		BaseSpawnerChildEntry GetLaunchable()
		{
			foreach (var childEntry in ChildEntries)
				if (childEntry.IsValid)
					return childEntry;

			return null;
		}

		public int GetChildrenInsideCount()
		{
			return ChildEntries.Count(x => x.IsValid);
		}

		public override void Replenish(Actor self, BaseSpawnerChildEntry entry)
		{
			base.Replenish(self, entry);

			if (info.SpawnContainConditions.TryGetValue(entry.Actor.Info.Name, out var spawnContainCondition))
				spawnContainTokens.GetOrAdd(entry.Actor.Info.Name).Push(self.GrantCondition(spawnContainCondition));

			if (!string.IsNullOrEmpty(info.LoadedCondition))
				loadedTokens.Push(self.GrantCondition(info.LoadedCondition));
		}

		void ITick.Tick(Actor self)
		{
			if (launchCondition != Actor.InvalidConditionToken && --launchConditionTicks < 0)
				launchCondition = self.RevokeCondition(launchCondition);

			if (respawnTicks > 0)
			{
				respawnTicks--;

				// Time to respawn someting.
				if (respawnTicks <= 0)
				{
					Replenish(self, ChildEntries);

					// If there's something left to spawn, restart the timer.
					if (SelectEntryToSpawn(ChildEntries) != null)
						respawnTicks = Util.ApplyPercentageModifiers(Info.RespawnTicks, reloadModifiers.Select(rm => rm.GetReloadModifier()));
				}
			}
		}
	}
}
