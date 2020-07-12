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

using OpenRA.Mods.Common;
using OpenRA.Mods.Common.Activities;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	public class CloudSpawnerInfo : TraitInfo
	{
		[Desc("Average time (ticks) between cloud spawn.")]
		public readonly int SpawnInterval = 10 * 25;

		[Desc("Delay (in ticks) before the first cloud spawns.")]
		public readonly int InitialSpawnDelay = 0;

		[ActorReference]
		[FieldLoader.Require]
		[Desc("The actors to spawn.")]
		public readonly string[] ActorTypes = null;

		[Desc("Facing that the cloud may approach from.")]
		public readonly int WindDirection = 8;

		[Desc("Spawn and remove the cloud this far outside the map.")]
		public readonly WDist Cordon = new WDist(7680);

		public override object Create(ActorInitializer init) { return new CloudSpawner(this); }
	}

	public class CloudSpawner : ITick
	{
		readonly CloudSpawnerInfo info;
		int ticks;

		public CloudSpawner(CloudSpawnerInfo info)
		{
			this.info = info;

			ticks = info.InitialSpawnDelay;
		}

		void ITick.Tick(Actor self)
		{
			if (--ticks <= 0)
			{
				ticks = info.SpawnInterval;

				SpawnCloud(self);
			}
		}

		void SpawnCloud(Actor self)
		{
			var position = self.World.Map.ChooseRandomCell(self.World.SharedRandom);

			self.World.AddFrameEndTask(w =>
			{
				var actorType = info.ActorTypes.Random(self.World.SharedRandom);
				var actor = self.World.Map.Rules.Actors[actorType];
				var facing = 256 * info.WindDirection / 32;
				var delta = new WVec(0, -1024, 0).Rotate(WRot.FromFacing(facing));

				var altitude = actor.TraitInfo<AircraftInfo>().CruiseAltitude.Length;
				var target = self.World.Map.CenterOfCell(position) + new WVec(0, 0, altitude);
				var startEdge = target - (self.World.Map.DistanceToEdge(target, -delta) + info.Cordon).Length * delta / 1024;
				var finishEdge = target + (self.World.Map.DistanceToEdge(target, delta) + info.Cordon).Length * delta / 1024;

				var cloud = w.CreateActor(actorType, new TypeDictionary
				{
					new CenterPositionInit(startEdge),
					new OwnerInit(self.Owner),
					new FacingInit(WAngle.FromFacing(facing)),
				});

				cloud.QueueActivity(false, new Fly(cloud, Target.FromPos(finishEdge)));
				cloud.QueueActivity(new RemoveSelf());
			});
		}
	}
}
