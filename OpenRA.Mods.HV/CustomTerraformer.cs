#region Copyright & License Information
/*
 * Copyright 2026 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using OpenRA.Mods.Common.MapGenerator;
using OpenRA.Mods.Common.Traits;
using OpenRA.Support;

namespace OpenRA.Mods.HV.MapGenerator
{
	/// <summary>Collection of high-level map generation utilities.</summary>
	public class CustomTerraformer : Terraformer
	{
		public CustomTerraformer(
			MapGenerationArgs mapGenerationArgs,
			Map map,
			ModData modData,
			List<ActorPlan> actorPlans,
			Symmetry.Mirror mirror,
			int rotations)
			: base(mapGenerationArgs, map, modData, actorPlans, mirror, rotations) { }

		/// <summary>
		/// Chooses a location for an actor within zoneable, and then projects, places, and dezones
		/// for it. (The zoneable CellLayer is modified.)
		/// </summary>
		/// <returns>True if a resource was placed, false if there was insufficient space.</returns>
		public bool AddResource(
			ResourceLayerInfo.ResourceTypeInfo resourceTypeInfo,
			MersenneTwister random,
			CellLayer<bool> zoneable,
			string actorType,
			WDist? actorDezoneRadius = null)
		{
			var actorPlan = new ActorPlan(Map, actorType);

			var requiredSpace = actorPlan.MaxSpan() * 1024 / 1448 + 2;
			var (chosenCell, chosenValue) = ChooseInZoneable(
				random, zoneable, requiredSpace);
			if (chosenValue < requiredSpace)
				return false;

			var chosenPosition = chosenCell.ToMPos(Map.Grid.Type);
			Map.Resources[chosenPosition] = new ResourceTile(
				resourceTypeInfo.ResourceIndex,
				resourceTypeInfo.MaxDensity);

			return true;
		}
	}
}
