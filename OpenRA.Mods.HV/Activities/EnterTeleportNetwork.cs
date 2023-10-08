#region Copyright & License Information
/*
 * Copyright 2019-2023 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Effects;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.HV.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Activities
{
	sealed class EnterTeleportNetwork : Enter
	{
		enum EnterState { Approaching, Entering, Exiting, Finished }

		readonly IMove move;
		readonly string type;

		Target target;
		Target lastVisibleTarget;
		bool useLastVisibleTarget;
		EnterState lastState = EnterState.Approaching;

		public EnterTeleportNetwork(Actor self, Target target, string type)
			: base(self, target, Color.Yellow)
		{
			move = self.Trait<IMove>();
			this.target = target;
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
			if (primary.IsDead)
				return;

			var exitInfo = primary.Info.TraitInfo<ExitInfo>();
			var rallyPoint = primary.TraitOrDefault<RallyPoint>();

			var exit = CPos.Zero; // spawn point
			var exitLocations = new List<CPos>(); // dest to move (cell pos)

			if (primary.OccupiesSpace != null)
			{
				exit = primary.Location + exitInfo.ExitCell;
				var spawn = primary.CenterPosition + exitInfo.SpawnOffset;
				var to = self.World.Map.CenterOfCell(exit);

				WAngle initialFacing;
				if (!exitInfo.Facing.HasValue)
				{
					var delta = to - spawn;
					if (delta.HorizontalLengthSquared == 0)
						initialFacing = WAngle.Zero;
					else
						initialFacing = delta.Yaw;

					var fi = self.TraitOrDefault<IFacing>();
					if (fi != null)
						fi.Facing = initialFacing;
				}
				else
					initialFacing = exitInfo.Facing.Value;

				exitLocations = rallyPoint != null ? rallyPoint.Path : new List<CPos>() { exit };
			}

			foreach (var notify in target.TraitsImplementing<INotifyEnterTeleporter>())
				notify.Charging(self, target);

			var teleporter = target.Trait<TeleportNetwork>();

			// TODO: avoid DelayedAction and use ITick instead.
			self.World.AddFrameEndTask(w => w.Add(new DelayedAction(teleporter.Info.Delay, () =>
			{
				// Teleport myself to primary actor.
				self.Trait<IPositionable>().SetPosition(self, exit);

				foreach (var notify in self.TraitsImplementing<INotifyExitTeleporter>())
					notify.Arrived(self);

				// Cancel all activities (like PortableChrono does)
				self.CancelActivity();

				// Issue attack move to the rally point.
				var move = self.TraitOrDefault<IMove>();
				if (move != null)
				{
					foreach (var cell in exitLocations)
						self.QueueActivity(new AttackMoveActivity(self, () => move.MoveTo(cell, 1, evaluateNearestMovableCell: true, targetLineColor: Color.OrangeRed)));
				}
			})));
		}

		public override bool Tick(Actor self)
		{
			// Update our view of the target
			target = target.Recalculate(self.Owner, out var targetIsHiddenActor);
			if (!targetIsHiddenActor && target.Type == TargetType.Actor)
				lastVisibleTarget = Target.FromTargetPositions(target);

			useLastVisibleTarget = targetIsHiddenActor || !target.IsValidFor(self);

			// Cancel immediately if the target died while we were entering it
			if (!IsCanceling && useLastVisibleTarget && lastState == EnterState.Entering)
				Cancel(self, true);

			TickInner(self, target, useLastVisibleTarget);

			// We need to wait for movement to finish before transitioning to
			// the next state or next activity
			if (!TickChild(self))
				return false;

			// Note that lastState refers to what we have just *finished* doing
			switch (lastState)
			{
				case EnterState.Approaching:
				{
					// NOTE: We can safely cancel in this case because we know the
					// actor has finished any in-progress move activities
					if (IsCanceling)
						return true;

					// Lost track of the target
					if (useLastVisibleTarget && lastVisibleTarget.Type == TargetType.Invalid)
						return true;

					// We are not next to the target - lets fix that
					if (target.Type != TargetType.Invalid && !move.CanEnterTargetNow(self, target))
					{
						// Target lines are managed by this trait, so we do not pass targetLineColor
						var initialTargetPosition = (useLastVisibleTarget ? lastVisibleTarget : target).CenterPosition;
						QueueChild(move.MoveToTarget(self, target, initialTargetPosition));
						return false;
					}

					// We are next to where we thought the target should be, but it isn't here
					// There's not much more we can do here
					if (useLastVisibleTarget || target.Type != TargetType.Actor)
						return true;

					// Are we ready to move into the target?
					if (TryStartEnter(self, target.Actor))
					{
						lastState = EnterState.Entering;
						QueueChild(move.MoveIntoTarget(self, target));
						return false;
					}

					// Subclasses can cancel the activity during TryStartEnter
					// Return immediately to avoid an extra tick's delay
					if (IsCanceling)
						return true;

					return false;
				}

				case EnterState.Entering:
				{
					// Check that we reached the requested position
					var targetPos = target.Positions.ClosestToIgnoringPath(self.CenterPosition);
					if (!IsCanceling && self.CenterPosition == targetPos && target.Type == TargetType.Actor)
						OnEnterComplete(self, target.Actor);

					lastState = EnterState.Finished; // never exit a departing teleporter
					return false;
				}
			}

			return true;
		}
	}
}
