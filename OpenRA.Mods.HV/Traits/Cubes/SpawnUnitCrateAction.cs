#region Copyright & License Information
/*
 * Copyright 2007-2021 The OpenRA Developers (see AUTHORS)
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
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Spawns units when collected and optionally plays an effect overlay.")]
	sealed class SpawnUnitCrateActionInfo : CrateActionInfo
	{
		[ActorReference]
		[FieldLoader.Require]
		[Desc("The list of units to spawn.")]
		public readonly string[] Units = Array.Empty<string>();

		[Desc("Factions that are allowed to trigger this action.")]
		public readonly HashSet<string> ValidFactions = new();

		[Desc("Override the owner of the newly spawned unit: e.g. Creeps or Neutral")]
		public readonly string Owner = null;

		[Desc("Image used to overlay the spawned actor.")]
		public readonly string AppearImage = "energyball";

		[SequenceReference(nameof(AppearImage), allowNullImage: true)]
		[Desc("Animation overlay played when spawning the actor.")]
		public readonly string AppearSequence = "appear";

		public override object Create(ActorInitializer init) { return new SpawnUnitCrateAction(init.Self, this); }
	}

	sealed class SpawnUnitCrateAction : CrateAction
	{
		readonly Actor self;
		readonly SpawnUnitCrateActionInfo info;
		readonly List<CPos> usedCells = new();

		public SpawnUnitCrateAction(Actor self, SpawnUnitCrateActionInfo info)
			: base(self, info)
		{
			this.self = self;
			this.info = info;
		}

		public bool CanGiveTo(Actor collector)
		{
			if (collector.Owner.NonCombatant)
				return false;

			if (info.ValidFactions.Any() && !info.ValidFactions.Contains(collector.Owner.Faction.InternalName))
				return false;

			foreach (var unit in info.Units)
			{
				// avoid dumping tanks in the sea, and ships on dry land.
				if (!GetSuitableCells(collector.Location, unit).Any())
					return false;
			}

			return true;
		}

		public override int GetSelectionShares(Actor collector)
		{
			if (!CanGiveTo(collector))
				return 0;

			return base.GetSelectionShares(collector);
		}

		public override void Activate(Actor collector)
		{
			foreach (var unit in info.Units)
			{
				var location = ChooseEmptyCellNear(collector, unit);
				if (location != null)
				{
					var cell = location.Value;
					usedCells.Add(cell);

					var world = collector.World;
					var position = world.Map.CenterOfCell(cell);
					if (info.AppearImage != null && info.AppearSequence != null)
						world.Add(new SpriteEffect(position, world, info.AppearImage, info.AppearSequence, Info.Palette));

					world.AddFrameEndTask(w =>
					{
						var spawnedActor = w.CreateActor(unit, new TypeDictionary
						{
							new LocationInit(cell),
							new OwnerInit(info.Owner ?? collector.Owner.InternalName)
						});
					});
				}
			}

			base.Activate(collector);
		}

		IEnumerable<CPos> GetSuitableCells(CPos near, string unitName)
		{
			var ip = self.World.Map.Rules.Actors[unitName].TraitInfo<IPositionableInfo>();

			for (var i = -1; i < 2; i++)
				for (var j = -1; j < 2; j++)
					if (ip.CanEnterCell(self.World, self, near + new CVec(i, j)))
						yield return near + new CVec(i, j);
		}

		CPos? ChooseEmptyCellNear(Actor a, string unit)
		{
			var possibleCells = GetSuitableCells(a.Location, unit).Where(c => !usedCells.Contains(c)).ToList();
			if (possibleCells.Count == 0)
				return null;

			return possibleCells.Random(self.World.SharedRandom);
		}
	}
}
