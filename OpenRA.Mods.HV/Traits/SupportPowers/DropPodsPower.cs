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

using System.Linq;
using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	public class DropPodsPowerInfo : SupportPowerInfo, IRulesetLoaded
	{
		[FieldLoader.Require]
		[Desc("Drop pod unit")]
		[ActorReference(new[] { typeof(AircraftInfo), typeof(FallsToEarthInfo) })]
		public readonly string[] UnitTypes = null;

		[Desc("Number of drop pods spawned.")]
		public readonly int2 Drops = new int2(3, 5);

		[Desc("Sets the approach direction.")]
		public readonly WAngle PodFacing = WAngle.Zero;

		[Desc("Maximum offset from targetLocation")]
		public readonly int PodScatter = 3;

		[ActorReference]
		[Desc("Actor to spawn when the attack starts")]
		public readonly string CameraActor = null;

		[Desc("Number of ticks to keep the camera alive")]
		public readonly int CameraRemoveDelay = 25;

		public override object Create(ActorInitializer init) { return new DropPodsPower(init.Self, this); }
	}

	public class DropPodsPower : SupportPower
	{
		readonly DropPodsPowerInfo info;

		public DropPodsPower(Actor self, DropPodsPowerInfo info)
			: base(self, info)
		{
			this.info = info;
		}

		public override void Activate(Actor self, Order order, SupportPowerManager manager)
		{
			base.Activate(self, order, manager);

			SendDropPods(self, order, info.PodFacing);
		}

		public void SendDropPods(Actor self, Order order, WAngle facing)
		{
			var actorInfo = self.World.Map.Rules.Actors[info.UnitTypes.First().ToLowerInvariant()];
			var aircraftInfo = actorInfo.TraitInfo<AircraftInfo>();
			var altitude = aircraftInfo.CruiseAltitude.Length;
			var approachRotation = WRot.FromYaw(facing);
			var fallsToEarthInfo = actorInfo.TraitInfo<FallsToEarthInfo>();
			var delta = new WVec(0, -altitude * aircraftInfo.Speed / fallsToEarthInfo.Velocity.Length, 0).Rotate(approachRotation);

			self.World.AddFrameEndTask(w =>
			{
				var target = order.Target.CenterPosition;
				var targetCell = self.World.Map.CellContaining(target);
				var podLocations = self.World.Map.FindTilesInCircle(targetCell, info.PodScatter)
					.Where(c => aircraftInfo.LandableTerrainTypes.Contains(w.Map.GetTerrainInfo(c).Type)
						&& !self.World.ActorMap.GetActorsAt(c).Any());

				if (!podLocations.Any())
					return;

				if (info.CameraActor != null)
				{
					var camera = w.CreateActor(info.CameraActor, new TypeDictionary
					{
						new LocationInit(targetCell),
						new OwnerInit(self.Owner),
					});

					camera.QueueActivity(new Wait(info.CameraRemoveDelay));
					camera.QueueActivity(new RemoveSelf());
				}

				PlayLaunchSounds();

				var drops = self.World.SharedRandom.Next(info.Drops.X, info.Drops.Y);
				for (var i = 0; i < drops; i++)
				{
					var unitType = info.UnitTypes.Random(self.World.SharedRandom);
					var podLocation = podLocations.Random(self.World.SharedRandom);
					var podTarget = Target.FromCell(w, podLocation);
					var location = self.World.Map.CenterOfCell(podLocation) - delta + new WVec(0, 0, altitude);

					var pod = w.CreateActor(false, unitType, new TypeDictionary
					{
						new CenterPositionInit(location),
						new OwnerInit(self.Owner),
						new FacingInit(facing)
					});

					var aircraft = pod.Trait<Aircraft>();
					if (!aircraft.CanLand(podLocation))
						pod.Dispose();
					else
						w.Add(pod);
				}
			});
		}
	}
}
