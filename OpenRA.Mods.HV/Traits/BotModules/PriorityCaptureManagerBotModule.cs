#region Copyright & License Information
/*
 * Copyright 2019-2022 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
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
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Manages AI capturing logic.")]
	public class PriorityCaptureManagerBotModuleInfo : ConditionalTraitInfo
	{
		[FieldLoader.Require]
		[Desc("Actor types that can capture other actors (via `Captures`).")]
		public readonly HashSet<string> CapturingActorTypes = new();

		[Desc("Percentage chance of trying a priority capture.")]
		public readonly int PriorityCaptureChance = 75;

		[Desc("Actor types that should be priorizited to be captured.",
			"Leave this empty to include all actors.")]
		public readonly HashSet<string> PriorityCapturableActorTypes = new();

		[Desc("Actor types that can be targeted for capturing.",
			"Leave this empty to include all actors.")]
		public readonly HashSet<string> CapturableActorTypes = new();

		[Desc("Avoid enemy actors nearby when searching for capture opportunities. Should be somewhere near the max weapon range.")]
		public readonly WDist EnemyAvoidanceRadius = WDist.FromCells(8);

		[Desc("Minimum delay (in ticks) between trying to capture with CapturingActorTypes.")]
		public readonly int MinimumCaptureDelay = 375;

		[Desc("Maximum number of options to consider for capturing.",
			"If a value less than 1 is given 1 will be used instead.")]
		public readonly int MaximumCaptureTargetOptions = 10;

		[Desc("Should visibility (Shroud, Fog, Cloak, etc) be considered when searching for capturable targets?")]
		public readonly bool CheckCaptureTargetsForVisibility = true;

		[Desc("Player stances that capturers should attempt to target.")]
		public readonly PlayerRelationship CapturableStances = PlayerRelationship.Enemy | PlayerRelationship.Neutral;

		public override object Create(ActorInitializer init) { return new PriorityCaptureManagerBotModule(init.Self, this); }
	}

	public class PriorityCaptureManagerBotModule : ConditionalTrait<PriorityCaptureManagerBotModuleInfo>, IBotTick, IBotPositionsUpdated, IGameSaveTraitData
	{
		readonly World world;
		readonly Player player;
		readonly int maximumCaptureTargetOptions;

		int minCaptureDelayTicks;
		CPos initialBaseCenter;

		public PriorityCaptureManagerBotModule(Actor self, PriorityCaptureManagerBotModuleInfo info)
			: base(info)
		{
			world = self.World;
			player = self.Owner;

			if (world.Type == WorldType.Editor)
				return;

			maximumCaptureTargetOptions = Math.Max(1, Info.MaximumCaptureTargetOptions);
		}

		void IBotPositionsUpdated.UpdatedBaseCenter(CPos newLocation)
		{
			initialBaseCenter = newLocation;
		}

		void IBotPositionsUpdated.UpdatedDefenseCenter(CPos newLocation) { }

		protected override void TraitEnabled(Actor self)
		{
			// Avoid all AIs reevaluating assignments on the same tick, randomize their initial evaluation delay.
			minCaptureDelayTicks = world.LocalRandom.Next(Info.MinimumCaptureDelay);
		}

		void IBotTick.BotTick(IBot bot)
		{
			if (--minCaptureDelayTicks <= 0)
			{
				minCaptureDelayTicks = Info.MinimumCaptureDelay;
				QueueCaptureOrders(bot);
			}
		}

		IEnumerable<Actor> GetVisibleActorsBelongingToPlayer(Player owner)
		{
			foreach (var actor in GetActorsThatCanBeOrderedByPlayer(owner))
				if (actor.CanBeViewedByPlayer(player))
					yield return actor;
		}

		IEnumerable<Actor> GetActorsThatCanBeOrderedByPlayer(Player owner)
		{
			foreach (var actor in world.Actors)
				if (actor.Owner == owner && !actor.IsDead && actor.IsInWorld)
					yield return actor;
		}

		void QueueCaptureOrders(IBot bot)
		{
			if (player.WinState != WinState.Undefined)
				return;

			var newUnits = world.ActorsHavingTrait<Captures>()
				.Where(a => a.Owner == player && !a.IsDead && a.IsInWorld);

			if (!newUnits.Any())
				return;

			var capturers = newUnits
				.Where(a => a.IsIdle && Info.CapturingActorTypes.Contains(a.Info.Name))
				.Select(a => new TraitPair<CaptureManager>(a, a.TraitOrDefault<CaptureManager>()))
				.Where(tp => tp.Trait != null);

			if (!capturers.Any())
				return;

			var baseCenter = world.Map.CenterOfCell(initialBaseCenter);

			if (world.LocalRandom.Next(100) < Info.PriorityCaptureChance)
			{
				var priorityTargets = world.Actors.Where(a =>
					!a.IsDead && a.IsInWorld && Info.CapturableStances.HasRelationship(player.RelationshipWith(a.Owner))
					&& Info.PriorityCapturableActorTypes.Contains(a.Info.Name.ToLowerInvariant()));

				if (Info.CheckCaptureTargetsForVisibility)
					priorityTargets = priorityTargets.Where(a => a.CanBeViewedByPlayer(player));

				if (priorityTargets.Any())
				{
					priorityTargets = priorityTargets.OrderBy(a => (a.CenterPosition - baseCenter).LengthSquared);

					var priorityCaptures = Math.Min(capturers.Count(), priorityTargets.Count());

					for (var i = 0; i < priorityCaptures; i++)
					{
						var capturer = capturers.First();

						var priorityTarget = priorityTargets.First();

						var captureManager = priorityTarget.TraitOrDefault<CaptureManager>();
						if (captureManager != null && capturer.Trait.CanTarget(captureManager))
						{
							var safeTarget = SafePath(capturer.Actor, priorityTarget);
							if (safeTarget.Type == TargetType.Invalid)
							{
								priorityTargets = priorityTargets.Skip(1);
								capturers = capturers.Skip(1);
								continue;
							}

							bot.QueueOrder(new Order("CaptureActor", capturer.Actor, safeTarget, true));
							AIUtils.BotDebug($"{player}: Ordered {capturer.Actor} {capturer.Actor.ActorID} to capture {priorityTarget} {priorityTarget.ActorID} in priority mode.");
						}

						priorityTargets = priorityTargets.Skip(1);
						capturers = capturers.Skip(1);
					}

					if (!capturers.Any())
						return;
				}
			}

			var randomPlayer = world.Players.Where(p => !p.Spectating
				&& Info.CapturableStances.HasRelationship(player.RelationshipWith(p))).Random(world.LocalRandom);

			var targetOptions = Info.CheckCaptureTargetsForVisibility
				? GetVisibleActorsBelongingToPlayer(randomPlayer)
				: GetActorsThatCanBeOrderedByPlayer(randomPlayer);

			var capturableTargetOptions = targetOptions
				.Where(target =>
				{
					var captureManager = target.TraitOrDefault<CaptureManager>();
					if (captureManager == null)
						return false;

					return capturers.Any(tp => tp.Trait.CanTarget(captureManager));
				})
				.OrderBy(target => (target.CenterPosition - baseCenter).LengthSquared)
				.Take(maximumCaptureTargetOptions);

			if (Info.CapturableActorTypes.Count > 0)
				capturableTargetOptions = capturableTargetOptions.Where(target => Info.CapturableActorTypes.Contains(target.Info.Name.ToLowerInvariant()));

			if (!capturableTargetOptions.Any())
				return;

			foreach (var capturer in capturers)
			{
				var nearestTargetActors = capturableTargetOptions.OrderBy(target => (target.CenterPosition - capturer.Actor.CenterPosition).LengthSquared);
				foreach (var nearestTargetActor in nearestTargetActors)
				{
					var attack = nearestTargetActor.TraitOrDefault<AttackBase>();
					if (attack != null && attack.HasAnyValidWeapons(Target.FromActor(capturer.Actor)))
						continue;

					var safeTarget = SafePath(capturer.Actor, nearestTargetActor);
					if (safeTarget.Type == TargetType.Invalid)
						continue;

					bot.QueueOrder(new Order("CaptureActor", capturer.Actor, safeTarget, true));
					AIUtils.BotDebug($"{capturer.Actor.Owner}: Ordered {capturer.Actor} to capture {nearestTargetActor}");
					break;
				}
			}
		}

		Target SafePath(Actor capturer, Actor target)
		{
			var mobile = capturer.Trait<Mobile>();
			var path = mobile.PathFinder.FindPathToTargetCell(capturer, new[] { mobile.ToCell }, target.Location, BlockedByActor.Stationary,
				location => world.FindActorsInCircle(world.Map.CenterOfCell(location), Info.EnemyAvoidanceRadius)
					.Where(u => !u.IsDead && capturer.Owner.RelationshipWith(u.Owner) == PlayerRelationship.Enemy && capturer.IsTargetableBy(u))
					.Sum(u => Math.Max(WDist.Zero.Length, Info.EnemyAvoidanceRadius.Length - (world.Map.CenterOfCell(location) - u.CenterPosition).Length)));

			if (path.Count == 0)
				return Target.Invalid;

			return Target.FromActor(target);
		}

		List<MiniYamlNode> IGameSaveTraitData.IssueTraitData(Actor self)
		{
			if (IsTraitDisabled)
				return null;

			return new List<MiniYamlNode>()
			{
				new("InitialBaseCenter", FieldSaver.FormatValue(initialBaseCenter))
			};
		}

		void IGameSaveTraitData.ResolveTraitData(Actor self, MiniYaml data)
		{
			if (self.World.IsReplay)
				return;

			var initialBaseCenterNode = data.NodeWithKeyOrDefault("InitialBaseCenter");
			if (initialBaseCenterNode != null)
				initialBaseCenter = FieldLoader.GetValue<CPos>("InitialBaseCenter", initialBaseCenterNode.Value.Value);
		}
	}
}
