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
using OpenRA.Activities;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	public class SeaMonsterInfo : TraitInfo, IPositionableInfo, IFacingInfo, IMoveInfo, IActorPreviewInitInfo
	{
		public readonly int Speed = 28;

		[Desc("Facing to use when actor spawns. Only 64 and 192 supported.")]
		public readonly int InitialFacing = 64;

		[Desc("Facing to use for actor previews (map editor, color picker, etc). Only 64 and 192 supported.")]
		public readonly int PreviewFacing = 64;

		public readonly string RestrictToTerrainType = "Water";

		public override object Create(ActorInitializer init) { return new SeaMonster(init, this); }

		public WAngle GetInitialFacing() { return WAngle.FromFacing(InitialFacing); }

		IEnumerable<ActorInit> IActorPreviewInitInfo.ActorPreviewInits(ActorInfo ai, ActorPreviewType type)
		{
			yield return new FacingInit(WAngle.FromFacing(PreviewFacing));
		}

		public IReadOnlyDictionary<CPos, SubCell> OccupiedCells(ActorInfo info, CPos location, SubCell subCell = SubCell.Any)
		{
			var occupied = new Dictionary<CPos, SubCell>() { { location, SubCell.FullCell } };
			return new ReadOnlyDictionary<CPos, SubCell>(occupied);
		}

		bool IOccupySpaceInfo.SharesCell { get { return false; } }

		// Used to determine if actor can spawn
		public bool CanEnterCell(World world, Actor self, CPos cell, SubCell subCell = SubCell.FullCell, Actor ignoreActor = null, BlockedByActor check = BlockedByActor.All)
		{
			return world.Map.Contains(cell);
		}
	}

	public class SeaMonster : ITick, ISync, IFacing, IPositionable, IMove, IDeathActorInitModifier,
		INotifyCreated, INotifyAddedToWorld, INotifyRemovedFromWorld, IActorPreviewInitModifier
	{
		public readonly SeaMonsterInfo Info;
		readonly Actor self;

		static readonly WAngle Left = new WAngle(256);
		static readonly WAngle Right = new WAngle(768);

		IEnumerable<int> speedModifiers;
		INotifyVisualPositionChanged[] notifyVisualPositionChanged;

		WRot orientation;

		[Sync]
		public WAngle Facing
		{
			get { return orientation.Yaw; }
			set { orientation = orientation.WithYaw(value); }
		}

		public WRot Orientation { get { return orientation; } }

		[Sync]
		public WPos CenterPosition { get; private set; }

		public CPos TopLeft { get { return self.World.Map.CellContaining(CenterPosition); } }

		// Isn't used anyway
		public WAngle TurnSpeed { get { return WAngle.Zero; } }

		CPos cachedLocation;
		CPos nextLocation;

		public SeaMonster(ActorInitializer init, SeaMonsterInfo info)
		{
			Info = info;
			self = init.Self;

			var locationInit = init.GetOrDefault<LocationInit>(info);
			if (locationInit != null)
				SetPosition(self, locationInit.Value);

			var centerPositionInit = init.GetOrDefault<CenterPositionInit>(info);
			if (centerPositionInit != null)
				SetPosition(self, centerPositionInit.Value);

			Facing = init.GetValue<FacingInit, WAngle>(Info.GetInitialFacing());

			// Prevent mappers from setting bogus facings
			if (Facing != Left && Facing != Right)
				Facing = Facing.Angle > 511 ? Right : Left;
		}

		void INotifyCreated.Created(Actor self)
		{
			speedModifiers = self.TraitsImplementing<ISpeedModifier>().ToArray().Select(sm => sm.GetSpeedModifier());
			cachedLocation = self.Location;
			notifyVisualPositionChanged = self.TraitsImplementing<INotifyVisualPositionChanged>().ToArray();
		}

		void INotifyAddedToWorld.AddedToWorld(Actor self)
		{
			self.World.AddToMaps(self, this);
		}

		void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
		{
			self.World.RemoveFromMaps(self, this);
		}

		void ITick.Tick(Actor self)
		{
			nextLocation = Facing == Left ? self.Location + new CVec(-1, 0) : self.Location + new CVec(1, 0);

			if (cachedLocation != self.Location)
			{
				// If the actor just left the map, switch facing
				if (!self.World.Map.Contains(self.Location))
					Turn();

				if (self.World.Map.GetTerrainInfo(nextLocation).Type != Info.RestrictToTerrainType)
					Turn();
			}

			cachedLocation = self.Location;

			SetVisualPosition(self, self.CenterPosition + MoveStep(Facing));
		}

		void Turn()
		{
			Facing = Facing == Left ? Right : Left;
		}

		int MovementSpeed
		{
			get { return Util.ApplyPercentageModifiers(Info.Speed, speedModifiers); }
		}

		public Pair<CPos, SubCell>[] OccupiedCells() { return new[] { Pair.New(TopLeft, SubCell.FullCell) }; }

		WVec MoveStep(WAngle facing)
		{
			return MoveStep(MovementSpeed, facing);
		}

		WVec MoveStep(int speed, WAngle facing)
		{
			var dir = new WVec(0, -1024, 0).Rotate(WRot.FromYaw(facing));
			return speed * dir / 1024;
		}

		void IDeathActorInitModifier.ModifyDeathActorInit(Actor self, TypeDictionary init)
		{
			init.Add(new FacingInit(Facing));
		}

		public bool CanExistInCell(CPos cell) { return true; }
		public bool IsLeavingCell(CPos location, SubCell subCell = SubCell.Any) { return false; }
		public bool CanEnterCell(CPos cell, Actor ignoreActor = null, BlockedByActor check = BlockedByActor.All) { return true; }
		public SubCell GetValidSubCell(SubCell preferred) { return SubCell.Invalid; }
		public SubCell GetAvailableSubCell(CPos a, SubCell preferredSubCell = SubCell.Any, Actor ignoreActor = null, BlockedByActor check = BlockedByActor.All)
		{
			// Does not use any subcell
			return SubCell.Invalid;
		}

		public void SetVisualPosition(Actor self, WPos pos) { SetPosition(self, pos); }

		public void SetPosition(Actor self, CPos cell, SubCell subCell = SubCell.Any)
		{
			SetPosition(self, self.World.Map.CenterOfCell(cell));
		}

		public void SetPosition(Actor self, WPos pos)
		{
			CenterPosition = pos;

			if (!self.IsInWorld)
				return;

			self.World.UpdateMaps(self, this);

			// This can be called from the constructor before notifyVisualPositionChanged is assigned.
			if (notifyVisualPositionChanged != null)
				foreach (var n in notifyVisualPositionChanged)
					n.VisualPositionChanged(self, 0, 0);
		}

		public Activity MoveTo(CPos cell, int nearEnough = 0, Actor ignoreActor = null,
			bool evaluateNearestMovableCell = false, Color? targetLineColor = null) { return null; }
		public Activity MoveWithinRange(Target target, WDist range,
			WPos? initialTargetPosition = null, Color? targetLineColor = null) { return null; }
		public Activity MoveWithinRange(Target target, WDist minRange, WDist maxRange,
			WPos? initialTargetPosition = null, Color? targetLineColor = null) { return null; }
		public Activity MoveFollow(Actor self, Target target, WDist minRange, WDist maxRange,
			WPos? initialTargetPosition = null, Color? targetLineColor = null) { return null; }
		public Activity ReturnToCell(Actor self) { return null; }
		public Activity MoveToTarget(Actor self, Target target,
			WPos? initialTargetPosition = null, Color? targetLineColor = null) { return null; }
		public Activity MoveIntoTarget(Actor self, Target target) { return null; }
		public Activity VisualMove(Actor self, WPos fromPos, WPos toPos) { return null; }

		public int EstimatedMoveDuration(Actor self, WPos fromPos, WPos toPos)
		{
			return (toPos - fromPos).Length / Info.Speed;
		}

		public CPos NearestMoveableCell(CPos cell) { return cell; }

		// Actors with SeaMonster always move
		public MovementType CurrentMovementTypes { get { return MovementType.Horizontal; } set { } }

		public bool CanEnterTargetNow(Actor self, Target target)
		{
			return false;
		}

		void IActorPreviewInitModifier.ModifyActorPreviewInit(Actor self, TypeDictionary inits)
		{
			if (!inits.Contains<DynamicFacingInit>() && !inits.Contains<FacingInit>())
				inits.Add(new DynamicFacingInit(() => Facing));
		}
	}
}
