#region Copyright & License Information
/*
 * Copyright 2024-2025 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Linq;
using OpenRA.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Mods.HV.Traits;

namespace OpenRA.Mods.HV.Activities
{
	public class Teleport : Activity
	{
		readonly Actor teleporter;
		readonly int? maximumDistance;
		CPos destination;
		readonly bool screenFlash;
		readonly string sound;

		public Teleport(Actor teleporter, CPos destination, int? maximumDistance,
			bool screenFlash, string sound, bool interruptable = true)
		{
			var max = teleporter.World.Map.Grid.MaximumTileSearchRange;
			if (maximumDistance > max)
				throw new InvalidOperationException($"Teleport distance cannot exceed the value of {nameof(MapGrid.MaximumTileSearchRange)} ({max}).");

			this.teleporter = teleporter;
			this.destination = destination;
			this.maximumDistance = maximumDistance;
			this.screenFlash = screenFlash;
			this.sound = sound;

			if (!interruptable)
				IsInterruptible = false;
		}

		public override bool Tick(Actor self)
		{
			var bestCell = ChooseBestDestinationCell(self, destination);
			if (bestCell == null)
				return true;

			destination = bestCell.Value;

			Game.Sound.Play(SoundType.World, sound, self.CenterPosition);
			Game.Sound.Play(SoundType.World, sound, self.World.Map.CenterOfCell(destination));

			self.Trait<IPositionable>().SetPosition(self, destination);
			self.Generation++;

			// Trigger screen desaturate effect
			if (screenFlash)
				foreach (var a in self.World.ActorsWithTrait<DesaturationPaletteEffect>())
					a.Trait.Enable();

			if (teleporter != null && self != teleporter && !teleporter.Disposed)
			{
				var building = teleporter.TraitOrDefault<WithSpriteBody>();
				if (building != null && building.DefaultAnimation.HasSequence("active"))
					building.PlayCustomAnimation(teleporter, "active");
			}

			return true;
		}

		CPos? ChooseBestDestinationCell(Actor self, CPos destination)
		{
			if (teleporter == null)
				return null;

			var restrictTo = maximumDistance == null ? null : self.World.Map.FindTilesInCircle(self.Location, maximumDistance.Value).ToHashSet();

			if (maximumDistance != null)
				destination = restrictTo.MinBy(x => (x - destination).LengthSquared);

			var pos = self.Trait<IPositionable>();
			if (pos.CanEnterCell(destination) && teleporter.Owner.Shroud.IsExplored(destination))
				return destination;

			var max = maximumDistance ?? teleporter.World.Map.Grid.MaximumTileSearchRange;
			foreach (var tile in self.World.Map.FindTilesInCircle(destination, max))
			{
				if (teleporter.Owner.Shroud.IsExplored(tile)
					&& (restrictTo == null || restrictTo.Contains(tile))
					&& pos.CanEnterCell(tile))
					return tile;
			}

			return null;
		}
	}
}
