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

using System;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Activities;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.HV.Activities;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("This unit, when ordered to move, will fly in ballistic path then will detonate itself upon reaching target.")]
	public class BallisticMissileInfo : TraitInfo, IMoveInfo, IPositionableInfo, IFacingInfo
	{
		[Desc("Pitch angle at which the actor will be created.")]
		public readonly WAngle CreateAngle = WAngle.Zero;

		[Desc("The time it takes for the actor to be created to launch.")]
		public readonly int PrepareTick = 10;

		[Desc("The altitude at which the actor begins to cruise.")]
		public readonly WDist BeginCruiseAltitude = WDist.FromCells(7);

		[Desc("How fast it changes direction.")]
		public readonly WAngle TurnSpeed = new WAngle(25);

		[Desc("The actor starts hitting the target when the horizontal distance is less than this value.")]
		public readonly WDist HitRange = WDist.FromCells(4);

		[Desc("If the actor is closer to the target than this value, it will explode.")]
		public readonly WDist ExplosionRange = new WDist(1536);

		[Desc("The acceleration of the actor during the launch phase, the speed during the launch phase will not be more than the speed value.")]
		public readonly WDist LaunchAcceleration = WDist.Zero;

		[Desc("Unit acceleration during the strike, no upper limit for speed value.")]
		public readonly WDist HitAcceleration = new WDist(20);

		[Desc("Projectile speed in WDist / tick, two values indicate variable velocity.")]
		public readonly WDist Speed = new WDist(17);

		[Desc("In angle. Missile is launched at this pitch and the intial tangential line of the ballistic path will be this.")]
		public readonly WAngle LaunchAngle = WAngle.Zero;

		[Desc("Minimum altitude where this missile is considered airborne")]
		public readonly int MinAirborneAltitude = 5;

		[Desc("Types of damage missile explosion is triggered with. Leave empty for no damage types.")]
		public readonly BitSet<DamageType> DamageTypes = default;

		[GrantedConditionReference]
		[Desc("The condition to grant to self while airborne.")]
		public readonly string AirborneCondition = null;

		[Desc("Sounds to play when the actor is taking off.")]
		public readonly string[] LaunchSounds = Array.Empty<string>();

		public override object Create(ActorInitializer init) { return new BallisticMissile(init, this); }

		public IReadOnlyDictionary<CPos, SubCell> OccupiedCells(ActorInfo info, CPos location, SubCell subCell = SubCell.Any) { return new Dictionary<CPos, SubCell>(); }
		bool IOccupySpaceInfo.SharesCell { get { return false; } }
		public bool CanEnterCell(World world, Actor self, CPos cell, SubCell subCell = SubCell.FullCell, Actor ignoreActor = null, BlockedByActor check = BlockedByActor.All)
		{
			return false;
		}

		[Desc("Color to use for the target line .")]
		public readonly Color TargetLineColor = Color.Red;

		public Color GetTargetLineColor() { return TargetLineColor; }

		public WAngle GetInitialFacing() { return WAngle.Zero; }
	}

	public class BallisticMissile : ISync, IFacing, IMove, IPositionable, INotifyCreated, INotifyAddedToWorld, INotifyRemovedFromWorld, IOccupySpace
	{
		public readonly BallisticMissileInfo Info;

		readonly Actor self;

		IEnumerable<int> speedModifiers;
		WRot orientation;

		public Target Target;
		public WVec MoveDirection;

		[Sync]
		public WAngle Facing
		{
			get { return orientation.Yaw; }
			set { orientation = orientation.WithYaw(value); }
		}

		public WAngle Pitch
		{
			get { return orientation.Pitch; }
			set { orientation = orientation.WithPitch(value); }
		}

		public WAngle Roll
		{
			get { return orientation.Roll; }
			set { orientation = orientation.WithRoll(value); }
		}

		public WRot Orientation { get { return orientation; } }

		[Sync]
		public WPos CenterPosition { get; private set; }

		public CPos TopLeft { get { return self.World.Map.CellContaining(CenterPosition); } }

		bool airborne;
		int airborneToken = Actor.InvalidConditionToken;

		public BallisticMissile(ActorInitializer init, BallisticMissileInfo info)
		{
			Info = info;
			self = init.Self;

			var locationInit = init.GetOrDefault<LocationInit>(info);
			if (locationInit != null)
				SetPosition(self, locationInit.Value);

			var centerPositionInit = init.GetOrDefault<CenterPositionInit>(info);
			if (centerPositionInit != null)
				SetPosition(self, centerPositionInit.Value);

			// This will get overridden by the spawner's facing.
			Facing = init.GetValue<FacingInit, WAngle>(info, WAngle.Zero);
		}

		public WAngle TurnSpeed => Info.TurnSpeed;

		void INotifyCreated.Created(Actor self)
		{
			speedModifiers = self.TraitsImplementing<ISpeedModifier>().ToArray().Select(sm => sm.GetSpeedModifier());
		}

		void INotifyAddedToWorld.AddedToWorld(Actor self)
		{
			self.World.AddToMaps(self, this);
			Pitch = Info.CreateAngle;
			self.QueueActivity(new FlyGuidedIntoTarget(self, Target, this));

			var altitude = self.World.Map.DistanceAboveTerrain(CenterPosition);
			if (altitude.Length >= Info.MinAirborneAltitude)
				OnAirborneAltitudeReached();
		}

		(CPos Cell, SubCell SubCell)[] IOccupySpace.OccupiedCells()
		{
			return Array.Empty<(CPos, SubCell)>();
		}

		public int MovementSpeed
		{
			get { return Util.ApplyPercentageModifiers(Info.Speed.Length, speedModifiers); }
		}

		public WVec FlyStep(WAngle facing)
		{
			return FlyStep(MovementSpeed, facing);
		}

		public WVec FlyStep(int speed, WAngle facing)
		{
			var dir = new WVec(0, -1024, 0).Rotate(WRot.FromFacing(facing.Facing));
			return speed * dir / 1024;
		}

		#region Implement IPositionable

		public bool CanExistInCell(CPos cell) { return true; }
		public bool IsLeavingCell(CPos location, SubCell subCell = SubCell.Any) { return false; } // TODO: Handle landing
		public bool CanEnterCell(CPos location, Actor ignoreActor = null, BlockedByActor check = BlockedByActor.All) { return true; }
		public SubCell GetValidSubCell(SubCell preferred) { return SubCell.Invalid; }
		public SubCell GetAvailableSubCell(CPos location, SubCell preferredSubCell = SubCell.Any, Actor ignoreActor = null, BlockedByActor check = BlockedByActor.All)
		{
			return SubCell.Invalid;
		}

		public void SetCenterPosition(Actor self, WPos pos) { SetPosition(self, pos); }

		// Changes position, but not altitude
		public void SetPosition(Actor self, CPos cell, SubCell subCell = SubCell.Any)
		{
			SetPosition(self, self.World.Map.CenterOfCell(cell) + new WVec(0, 0, CenterPosition.Z));
		}

		public void SetPosition(Actor self, WPos pos)
		{
			CenterPosition = pos;

			if (!self.IsInWorld)
				return;

			self.World.UpdateMaps(self, this);

			var altitude = self.World.Map.DistanceAboveTerrain(CenterPosition);
			var isAirborne = altitude.Length >= Info.MinAirborneAltitude;
			if (isAirborne && !airborne)
				OnAirborneAltitudeReached();
			else if (!isAirborne && airborne)
				OnAirborneAltitudeLeft();
		}

		#endregion

		#region Implement IMove

		public Activity MoveTo(CPos cell, int nearEnough = 0, Actor ignoreActor = null,
			bool evaluateNearestMovableCell = false, Color? targetLineColor = null)
		{
			return null;
		}

		public Activity MoveWithinRange(in Target target, WDist range,
			WPos? initialTargetPosition = null, Color? targetLineColor = null)
		{
			return null;
		}

		public Activity MoveWithinRange(in Target target, WDist minRange, WDist maxRange,
			WPos? initialTargetPosition = null, Color? targetLineColor = null)
		{
			return null;
		}

		public Activity MoveFollow(Actor self, in Target target, WDist minRange, WDist maxRange,
			WPos? initialTargetPosition = null, Color? targetLineColor = null)
		{
			return null;
		}

		public Activity ReturnToCell(Actor self)
		{
			return null;
		}

		public Activity MoveToTarget(Actor self, in Target target,
			WPos? initialTargetPosition = null, Color? targetLineColor = null)
		{
			return null;
		}

		public Activity MoveIntoTarget(Actor self, in Target target)
		{
			return null;
		}

		public Activity LocalMove(Actor self, WPos fromPos, WPos toPos)
		{
			return null;
		}

		public int EstimatedMoveDuration(Actor self, WPos fromPos, WPos toPos)
		{
			return MovementSpeed > 0 ? (toPos - fromPos).Length / MovementSpeed : 0;
		}

		public CPos NearestMoveableCell(CPos cell) { return cell; }

		public MovementType CurrentMovementTypes { get { return MovementType.Horizontal | MovementType.Vertical; } set { } }

		public bool CanEnterTargetNow(Actor self, in Target target)
		{
			return false;
		}

		#endregion

		void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
		{
			self.World.RemoveFromMaps(self, this);
			OnAirborneAltitudeLeft();
		}

		#region Airborne conditions

		void OnAirborneAltitudeReached()
		{
			if (airborne)
				return;

			airborne = true;
			if (!string.IsNullOrEmpty(Info.AirborneCondition) && airborneToken == Actor.InvalidConditionToken)
				airborneToken = self.GrantCondition(Info.AirborneCondition);
		}

		void OnAirborneAltitudeLeft()
		{
			if (!airborne)
				return;

			airborne = false;
			if (airborneToken != Actor.InvalidConditionToken)
				airborneToken = self.RevokeCondition(airborneToken);
		}

		#endregion
	}
}
