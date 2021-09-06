#region Copyright & License Information
/*
 * Copyright 2021 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using OpenRA.Activities;
using OpenRA.Mods.HV.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Activities
{
	public class FlyGuidedIntoTarget : Activity
	{
		enum Status { Prepare, Launch, NoCruiseLaunch, LazyCurve, Cruise, Hit, Unknown }

		readonly BallisticMissile missile;
		readonly BallisticMissileInfo info;
		readonly WPos initPosititon;
		readonly WPos targetPosition;

		int ticks = 0;
		Status status = Status.Prepare;

		int speed = 0;
		readonly int acceleration = 0;
		readonly WAngle preparePitchIncrement;

		public FlyGuidedIntoTarget(Actor self, Target target, BallisticMissile missile)
		{
			this.missile = missile;
			info = missile.Info;
			initPosititon = self.CenterPosition;
			targetPosition = target.CenterPosition;

			if (info.LaunchAcceleration == WDist.Zero)
			{
				speed = info.Speed.Length;
				acceleration = 0;
			}
			else
			{
				speed = 0;
				acceleration = info.LaunchAcceleration.Length;
			}

			preparePitchIncrement = info.PrepareTick == 0 ? WAngle.Zero : new WAngle((info.LaunchAngle - info.CreateAngle).Angle / info.PrepareTick);
		}

		protected override void OnFirstRun(Actor self)
		{
			missile.Pitch = info.CreateAngle;
		}

		void MoveForward(Actor self)
		{
			missile.MoveDirection = new WVec(0, -speed, 0)
				.Rotate(new WRot(missile.Pitch, WAngle.Zero, WAngle.Zero))
				.Rotate(new WRot(WAngle.Zero, WAngle.Zero, missile.Facing));

			missile.SetPosition(self, missile.CenterPosition + missile.MoveDirection);

			if (!self.IsInWorld)
				status = Status.Unknown;
		}

		void PrepareStatusHandle(Actor self)
		{
			if (ticks < info.PrepareTick)
				missile.Pitch += preparePitchIncrement;
			else
			{
				if (missile.Info.LaunchSounds.Length > 0)
					Game.Sound.Play(SoundType.World, missile.Info.LaunchSounds, self.World, missile.CenterPosition);

				status = Status.Launch;
			}
		}

		void LaunchStatusHandle(Actor self)
		{
			MoveForward(self);
			speed = speed + acceleration > info.Speed.Length ? info.Speed.Length : speed + acceleration;
			if (missile.CenterPosition.Z - initPosititon.Z > info.BeginCruiseAltitude.Length)
				status = Status.Cruise;
		}

		void CruiseStatusHandle(Actor self)
		{
			MoveForward(self);
			if (missile.Pitch != WAngle.Zero)
			{
				if ((missile.Pitch.Angle < missile.TurnSpeed.Angle) || ((1024 - missile.Pitch.Angle) < missile.TurnSpeed.Angle))
					missile.Pitch = WAngle.Zero;
				else
					missile.Pitch -= missile.TurnSpeed;
			}

			var targetYaw = (targetPosition - missile.CenterPosition).Yaw;
			var yawDiff = targetYaw - missile.Facing;
			if (yawDiff != WAngle.Zero)
			{
				if ((yawDiff.Angle < missile.TurnSpeed.Angle) || ((1024 - yawDiff.Angle) < missile.TurnSpeed.Angle))
					missile.Facing = targetYaw;
				else
				{
					if (yawDiff.Angle < 512)
						missile.Facing += missile.TurnSpeed;
					else
						missile.Facing -= missile.TurnSpeed;
				}
			}

			if ((targetPosition - missile.CenterPosition).HorizontalLength < info.HitRange.Length)
				status = Status.Hit;
		}

		static WAngle Pitch(WVec vector)
		{
			if (vector.LengthSquared == 0)
				return WAngle.Zero;

			// The engine defines north as -y
			return WAngle.ArcTan(vector.Z, vector.HorizontalLength);
		}

		void HitStatusHandle(Actor self)
		{
			MoveForward(self);
			speed += info.HitAcceleration.Length;
			var targetPitch = Pitch(targetPosition - missile.CenterPosition);
			var pitchDiff = targetPitch - missile.Pitch;
			if (pitchDiff != WAngle.Zero)
			{
				if ((pitchDiff.Angle < missile.TurnSpeed.Angle) || ((1024 - pitchDiff.Angle) < missile.TurnSpeed.Angle))
					missile.Pitch = targetPitch;
				else
				{
					if (pitchDiff.Angle < 512)
						missile.Pitch += missile.TurnSpeed;
					else
						missile.Pitch -= missile.TurnSpeed;
				}
			}

			var targetYaw = (targetPosition - missile.CenterPosition).Yaw;
			var yawDiff = targetYaw - missile.Facing;
			if (yawDiff != WAngle.Zero)
			{
				if ((yawDiff.Angle < missile.TurnSpeed.Angle) || ((1024 - yawDiff.Angle) < missile.TurnSpeed.Angle))
					missile.Facing = targetYaw;
				else
				{
					if (yawDiff.Angle < 512)
						missile.Facing += missile.TurnSpeed;
					else
						missile.Facing -= missile.TurnSpeed;
				}
			}

			if ((targetPosition - missile.CenterPosition).Length < info.ExplosionRange.Length)
				status = Status.Unknown;
		}

		public override bool Tick(Actor self)
		{
			switch (status)
			{
				case Status.Prepare:
					PrepareStatusHandle(self);
					break;
				case Status.Launch:
					LaunchStatusHandle(self);
					break;
				case Status.Cruise:
					CruiseStatusHandle(self);
					break;
				case Status.Hit:
					HitStatusHandle(self);
					break;
				default:
					missile.SetPosition(self, targetPosition);
					Queue(new CallFunc(() => self.Kill(self, missile.Info.DamageTypes)));
					return true;
			}

			ticks++;
			return false;
		}

		public override IEnumerable<Target> GetTargets(Actor self)
		{
			yield return Target.FromPos(targetPosition);
		}
	}
}
