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

using System.Linq;
using OpenRA.GameRules;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Warheads;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Warheads
{
	[Desc("Warhead extension class using `ValidTargets`/`InvalidTargets` to specify trigger conditions during an explosion,",
		"compared against the \"always trigger, filtering applies to actors in range\" logic followed by most OpenRA warheads.",
		"Model is abstracted from " + nameof(CreateEffectWarhead) + ".",
		"Checks against the `Air` TargetType when the explosion was outside of an actor's hitbox",
		"and happened above `Warhead.AirThreshold`.")]
	public abstract class ValidateTriggerWarhead : Warhead
	{
		static readonly BitSet<TargetableType> TargetTypeAir = new("Air");

		/// <summary>Checks if there are any actors at impact position and if the warhead is valid against any of them.</summary>
		ImpactActorType ActorTypeAtImpact(World world, WPos pos, Actor firedBy)
		{
			var anyInvalidActor = false;

			// Check whether the impact position overlaps with an actor's hitshape
			foreach (var victim in world.FindActorsOnCircle(pos, WDist.Zero))
			{
				if (!AffectsParent && victim == firedBy)
					continue;

				var activeShapes = victim.TraitsImplementing<HitShape>().Where(Exts.IsTraitEnabled);
				if (!activeShapes.Any(s => s.DistanceFromEdge(victim, pos).Length <= 0))
					continue;

				if (IsValidAgainst(victim, firedBy))
					return ImpactActorType.Valid;

				anyInvalidActor = true;
			}

			return anyInvalidActor ? ImpactActorType.Invalid : ImpactActorType.None;
		}

		/// <summary>Checks if the warhead is valid against the terrain at impact position.</summary>
		bool IsValidAgainstTerrain(World world, WPos pos)
		{
			var cell = world.Map.CellContaining(pos);
			if (!world.Map.Contains(cell))
				return false;

			var dat = world.Map.DistanceAboveTerrain(pos);
			return IsValidTarget(dat > AirThreshold ? TargetTypeAir : world.Map.GetTerrainInfo(cell).TargetTypes);
		}

		protected bool ValidateTrigger(in Target target, WarheadArgs args)
		{
			if (target.Type == TargetType.Invalid)
				return false;

			var firedBy = args.SourceActor;
			if (!target.IsValidFor(firedBy))
				return false;

			var world = firedBy.World;
			var pos = target.CenterPosition;
			var actorAtImpact = ActorTypeAtImpact(world, pos, firedBy);

			// If there's either a) an invalid actor, or b) no actor and invalid terrain, we don't trigger the effect(s).
			if (actorAtImpact == ImpactActorType.Invalid)
				return false;
			else if (actorAtImpact == ImpactActorType.None && !IsValidAgainstTerrain(world, pos))
				return false;

			return true;
		}
	}
}
