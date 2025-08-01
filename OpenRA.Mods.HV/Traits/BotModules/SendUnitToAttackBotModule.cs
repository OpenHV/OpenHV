﻿#region Copyright & License Information
/*
 * Copyright 2007-2025 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Flags]
	public enum AttackDistance
	{
		Closest = 0,
		Furthest = 1,
		Random = 2
	}

	[TraitLocation(SystemActors.Player)]
	[Desc("Bot logic for units that should not be sent with a regular squad, like suicide or subterranean units.")]
	public class SendUnitToAttackBotModuleInfo : ConditionalTraitInfo
	{
		[Desc("Actors used for attack, and their base desire provided for attack desire.",
			"When desire reach 100, AI will send them to attack.")]
		public readonly Dictionary<string, int> ActorTypesAndAttackDesire = default;

		[Desc("Target types that can be targeted.")]
		public readonly BitSet<TargetableType> ValidTargets = new("Structure");

		[Desc("Target types that can't be targeted.")]
		public readonly BitSet<TargetableType> InvalidTargets;

		[Desc("Should attack the furthest or closest target. Possible values are Closest, Furthest, Random")]
		public readonly AttackDistance AttackDistance = AttackDistance.Closest;

		[Desc("Attack order name.")]
		public readonly string AttackOrderName = "Attack";

		[Desc("Find target and try attack target in this interval.")]
		public readonly int ScanTick = 463;

		[Desc("The total attack desire increases by this amount per scan",
			"Note: When there is no attack unit, the total attack desire will return to 0.")]
		public readonly int AttackDesireIncreasedPerScan = 10;

		public override object Create(ActorInitializer init) { return new SendUnitToAttackBotModule(init.Self, this); }
	}

	public class SendUnitToAttackBotModule : ConditionalTrait<SendUnitToAttackBotModuleInfo>, IBotTick
	{
		readonly World world;
		readonly Player player;
		readonly Predicate<Actor> unitCannotBeOrdered;
		readonly Predicate<Actor> unitCannotBeOrderedOrIsBusy;
		readonly Predicate<Actor> isInvalidActor;
		int minAssignRoleDelayTicks;
		Player targetPlayer;
		int desireIncreased;

		public SendUnitToAttackBotModule(Actor self, SendUnitToAttackBotModuleInfo info)
		: base(info)
		{
			world = self.World;
			player = self.Owner;
			isInvalidActor = a => a == null || a.IsDead || !a.IsInWorld || a.Owner != targetPlayer;
			unitCannotBeOrdered = a => a == null || a.IsDead || !a.IsInWorld || a.Owner != player;
			unitCannotBeOrderedOrIsBusy = a => unitCannotBeOrdered(a) || (!a.IsIdle && a.CurrentActivity is not FlyIdle);
			desireIncreased = 0;
		}

		protected override void TraitEnabled(Actor self)
		{
			// Avoid all AIs reevaluating assignments on the same tick, randomize their initial evaluation delay.
			minAssignRoleDelayTicks = world.LocalRandom.Next(0, Info.ScanTick);
		}

		void IBotTick.BotTick(IBot bot)
		{
			if (--minAssignRoleDelayTicks <= 0)
			{
				minAssignRoleDelayTicks = Info.ScanTick;

				var attackDesire = 0;
				var actors = world.ActorsWithTrait<IPositionable>().Select(at => at.Actor).Where(a =>
				{
					if (Info.ActorTypesAndAttackDesire.TryGetValue(a.Info.Name, out var desire) && !unitCannotBeOrderedOrIsBusy(a))
					{
						attackDesire += desire;
						return true;
					}

					return false;
				}).ToList();

				if (actors.Count == 0)
					desireIncreased = 0;
				else
					desireIncreased += Info.AttackDesireIncreasedPerScan;

				if (desireIncreased + attackDesire < 100)
					return;

				// Randomly choose enemy player to attack
				var enemyPlayers = world.Players.Where(p => p.RelationshipWith(player) == PlayerRelationship.Enemy && p.WinState != WinState.Lost).ToList();
				if (enemyPlayers.Count == 0)
					return;

				targetPlayer = enemyPlayers.Random(world.LocalRandom);

				var targets = world.Actors.Where(a =>
				{
					if (isInvalidActor(a))
						return false;

					var t = a.GetAllTargetTypes();

					if (!Info.ValidTargets.Overlaps(t) || Info.InvalidTargets.Overlaps(t))
						return false;

					var hasModifier = false;
					var visModifiers = a.TraitsImplementing<IVisibilityModifier>();
					foreach (var v in visModifiers)
					{
						if (v.IsVisible(a, player))
							return true;

						hasModifier = true;
					}

					return !hasModifier;
				});

				switch (Info.AttackDistance)
				{
					case AttackDistance.Closest:
						targets = targets.OrderBy(a => (a.CenterPosition - actors[0].CenterPosition).HorizontalLengthSquared);
						break;
					case AttackDistance.Furthest:
						targets = targets.OrderByDescending(a => (a.CenterPosition - actors[0].CenterPosition).HorizontalLengthSquared);
						break;
					case AttackDistance.Random:
						targets = targets.Shuffle(world.LocalRandom);
						break;
				}

				foreach (var t in targets)
				{
					var orderedActors = new List<Actor>();

					foreach (var a in actors)
					{
						if (!a.Info.HasTraitInfo<AircraftInfo>())
						{
							var mobile = a.TraitOrDefault<Mobile>();
							if (mobile == null || !mobile.PathFinder.PathExistsForLocomotor(mobile.Locomotor, a.Location, t.Location))
								continue;
						}

						orderedActors.Add(a);
					}

					actors.RemoveAll(orderedActors.Contains);

					if (orderedActors.Count > 0)
						bot.QueueOrder(new Order(Info.AttackOrderName, null, Target.FromActor(t), false, groupedActors: orderedActors.ToArray()));

					if (actors.Count == 0)
						break;
				}
			}
		}
	}
}
