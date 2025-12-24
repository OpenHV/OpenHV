#region Copyright & License Information
/*
 * Copyright 2020-2025 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Frozen;
using System.Collections.Immutable;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Modifies the production cost of this actor for a specific queue or when a prerequisite is granted. " +
		"Requires " + nameof(VariedCostManager) + " on the " + nameof(World) + " actor.")]
	public class VariedCostMultiplierInfo : TraitInfo<VariedCostMultiplier>, IProductionCostModifierInfo, IRulesetLoaded
	{
		[Desc("Only apply this cost change if the owner has these prerequisites.")]
		public readonly ImmutableArray<string> Prerequisites = [];

		[Desc("Production queues that this cost will apply to.")]
		public readonly FrozenSet<string> Queues = default;

		[Desc("Set this if items should get the same random pricing.")]
		public readonly string Group = null;

		int IProductionCostModifierInfo.GetProductionCostModifier(TechTree techTree, string queue)
		{
			if ((Queues.Count == 0 || Queues.Contains(queue)) && (Prerequisites.Length == 0 || techTree.HasPrerequisites(Prerequisites)))
				return techTree.Owner.World.WorldActor.Trait<VariedCostManager>().CachedCostPercentage[Group ?? actorInfo.Name];

			return 100;
		}

		ActorInfo actorInfo;

		public void RulesetLoaded(Ruleset rules, ActorInfo info)
		{
			actorInfo = info;

			var variedCostManagerInfo = rules.Actors["world"].TraitInfoOrDefault<VariedCostManagerInfo>();
			if (variedCostManagerInfo == null)
				throw new YamlException($"`{nameof(VariedCostMultiplier)}` requires the `{nameof(World)}` actor to have the `{nameof(VariedCostManager)}` trait.");
		}
	}

	public class VariedCostMultiplier { }
}
