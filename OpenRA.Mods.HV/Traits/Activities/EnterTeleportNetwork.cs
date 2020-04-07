#region Copyright & License Information
/*
 * Copyright 2019-2020 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.HV.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Activities
{
	class EnterTeleportNetwork : Enter
	{
		readonly string type;

		public EnterTeleportNetwork(Actor self, Target target, string type)
			: base(self, target, Color.Yellow)
		{
			this.type = type;
		}

		protected override bool TryStartEnter(Actor self, Actor target)
		{
			return target.IsValidTeleportNetworkUser(self);
		}

		protected override void OnEnterComplete(Actor self, Actor target)
		{
			// entered the teleport network canal but the entrance is dead immediately.
			if (target.IsDead || self.IsDead)
				return;

			// Find the primary teleport network exit.
			var primary = target.Owner.PlayerActor.TraitsImplementing<TeleportNetworkManager>().First(x => x.Type == type).PrimaryActor;

			var exitInfo = primary.Info.TraitInfo<ExitInfo>();
			var rallyPoint = primary.TraitOrDefault<RallyPoint>();

			var exit = CPos.Zero; // spawn point
			var exitLocations = new List<CPos>(); // dest to move (cell pos)

			if (primary.OccupiesSpace != null)
			{
				exit = primary.Location + exitInfo.ExitCell;
				var spawn = primary.CenterPosition + exitInfo.SpawnOffset;
				var to = self.World.Map.CenterOfCell(exit);

				var initialFacing = exitInfo.Facing;
				if (exitInfo.Facing < 0)
				{
					var delta = to - spawn;
					if (delta.HorizontalLengthSquared == 0)
						initialFacing = 0;
					else
						initialFacing = delta.Yaw.Facing;

					var fi = self.TraitOrDefault<IFacing>();
					if (fi != null)
						fi.Facing = initialFacing;
				}

				exitLocations = rallyPoint != null ? rallyPoint.Path : new List<CPos>() { exit };
			}

			foreach (var notify in target.TraitsImplementing<INotifyEnterTeleporter>())
				notify.Charging(self, target);

			// Teleport myself to primary actor.
			self.Trait<IPositionable>().SetPosition(self, exit);

			// Cancel all activities (like PortableChrono does)
			self.CancelActivity();

			// Issue attack move to the rally point.
			self.World.AddFrameEndTask(w =>
			{
				var move = self.TraitOrDefault<IMove>();
				if (move != null)
				{
					foreach (var cell in exitLocations)
						self.QueueActivity(new AttackMoveActivity(self, () => move.MoveTo(cell, 1, evaluateNearestMovableCell: true, targetLineColor: Color.OrangeRed)));
				}
			});
		}
	}
}
