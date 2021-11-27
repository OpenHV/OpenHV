#region Copyright & License Information
/*
 * Copyright 2019-2021 The OpenHV Developers (see CREDITS)
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
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Manages AI cloak detector deployment logic. For use with the regular `SquadManagerBotModule`.")]
	public class DeployDetectorBotModuleInfo : ConditionalTraitInfo
	{
		[FieldLoader.Require]
		[ActorReference]
		[Desc("Actor types that can deploy.")]
		public readonly HashSet<string> DeployableActorTypes = new HashSet<string>();

		[Desc("Minimum delay (in ticks) between trying to deploy with DeployableActorTypes.")]
		public readonly int MinimumScanDelay = 20;

		public override object Create(ActorInitializer init) { return new DeployDetectorBotModule(init.Self, this); }
	}

	public class DeployDetectorBotModule : ConditionalTrait<DeployDetectorBotModuleInfo>, IBotTick
	{
		readonly World world;
		readonly Player player;

		readonly Func<Actor, bool> unitCannotBeOrdered;

		int scanForIdleDetectorsTicks;

		class DetectorTraitWrapper
		{
			public readonly Actor Actor;
			public readonly GrantConditionOnDeploy GrantConditionOnDeploy;

			public DetectorTraitWrapper(Actor actor)
			{
				Actor = actor;
				GrantConditionOnDeploy = actor.Trait<GrantConditionOnDeploy>();
			}
		}

		readonly Dictionary<Actor, DetectorTraitWrapper> detectors = new Dictionary<Actor, DetectorTraitWrapper>();

		public DeployDetectorBotModule(Actor self, DeployDetectorBotModuleInfo info)
			: base(info)
		{
			world = self.World;
			player = self.Owner;

			unitCannotBeOrdered = a => a.Owner != self.Owner || a.IsDead || !a.IsInWorld;
		}

		protected override void TraitEnabled(Actor self)
		{
			// PERF: Avoid all AIs reevaluating assignments on the same tick, randomize their initial evaluation delay.
			scanForIdleDetectorsTicks = world.LocalRandom.Next(0, Info.MinimumScanDelay);
		}

		void IBotTick.BotTick(IBot bot)
		{
			if (--scanForIdleDetectorsTicks > 0)
				return;

			scanForIdleDetectorsTicks = Info.MinimumScanDelay;

			var toRemove = detectors.Keys.Where(unitCannotBeOrdered).ToList();
			foreach (var a in toRemove)
				detectors.Remove(a);

			// TODO: Look for a more performance friendly way to update this list
			var newDetectors = world.Actors.Where(a => Info.DeployableActorTypes.Contains(a.Info.Name) && a.Owner == player && !detectors.ContainsKey(a));
			foreach (var a in newDetectors)
				detectors[a] = new DetectorTraitWrapper(a);

			foreach (var detector in detectors)
			{
				if (detector.Value.GrantConditionOnDeploy.DeployState != DeployState.Undeployed)
					continue;

				bot.QueueOrder(new Order("GrantConditionOnDeploy", detector.Key, true));
			}
		}
	}
}
