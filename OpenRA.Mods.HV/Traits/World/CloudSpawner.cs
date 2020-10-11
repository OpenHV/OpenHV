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

using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.HV.Effects;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	public class CloudSpawnerInfo : TraitInfo, ILobbyCustomRulesIgnore
	{
		[Desc("Average time (ticks) between cloud spawn.")]
		public readonly int SpawnInterval = 10 * 25;

		[Desc("Delay (in ticks) before the first cloud spawns.")]
		public readonly int InitialSpawnDelay = 0;

		[FieldLoader.Require]
		[Desc("Which image to use.")]
		public readonly string Image;

		[FieldLoader.Require]
		[Desc("Which sequence to use.")]
		[SequenceReference(nameof(Image))]
		public readonly string[] Sequences;

		[FieldLoader.Require]
		[Desc("Which palette to use.")]
		[PaletteReference]
		public readonly string Palette;

		[Desc("Facing that the cloud may approach from.")]
		public readonly int WindDirection = 8;

		[Desc("Spawn and remove the cloud this far outside the map.")]
		public readonly WDist Cordon = new WDist(7680);

		[FieldLoader.Require]
		[Desc("Cloud forward movement. Two values mean the cloud speed randomizes between them.")]
		public readonly WDist[] Speed;

		[Desc("The altitude of the cloud.")]
		public readonly WDist CruiseAltitude = new WDist(2560);

		[Desc("Distance margin where the cloud can be removed.")]
		public readonly WDist CloseEnough = new WDist(128);

		[Desc("Should we pre-spawn clouds covers the map?")]
		public readonly bool ShouldPrespawn = true;
		public override object Create(ActorInitializer init) { return new CloudSpawner(this); }
	}

	public class CloudSpawner : ITick, IWorldLoaded
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

		void IWorldLoaded.WorldLoaded(World w, WorldRenderer wr)
		{
			if (info.ShouldPrespawn)
				PreSpawnClouds(w);
		}

		void SpawnCloud(Actor self)
		{
			var position = self.World.Map.ChooseRandomCell(self.World.SharedRandom);

			var facing = 256 * info.WindDirection / 32;
			var delta = new WVec(0, -1024, 0).Rotate(WRot.FromFacing(facing));

			var target = self.World.Map.CenterOfCell(position) + new WVec(0, 0, info.CruiseAltitude.Length);
			var startEdge = target - (self.World.Map.DistanceToEdge(target, -delta) + info.Cordon).Length * delta / 1024;
			var finishEdge = target + (self.World.Map.DistanceToEdge(target, delta) + info.Cordon).Length * delta / 1024;

			var animation = new Animation(self.World, info.Image, () => WAngle.FromFacing(facing));
			animation.PlayRepeating(info.Sequences.Random(self.World.SharedRandom));

			self.World.AddFrameEndTask(w => w.Add(new Cloud(self.World, animation, startEdge, finishEdge, facing, info)));
		}

		void PreSpawnClouds(World world)
		{
			var facing = 256 * info.WindDirection / 32;
			var delta = new WVec(0, -1024, 0).Rotate(WRot.FromFacing(facing));

			/*
			 * The following codes find the middle point in map. Because clouds must
			 * start from an edge to another edge, the paths of clouds
			 * which contain this point will always be the longest path no matter the direction,
			 * as long as the map is a symmetrical shape.
			 *
			 * For example, the middle point in map is "@", @ is (u = X/2, v = Y/2)
			 * You can imagine any wind direction with all edges clouds possibly spawn.
			 *
			 *              ____X____
			 *				|		|
			 *				|		|
			 *				|	@	Y
			 *				|		|
			 *				|_______|
			 *
			 * By using this longest path, we can figure out the number of clouds should
			 * spawn when a certain cloud completely goes over a longest path, which should be
			 * neither too little nor too big compared with the clouds per map-cell later
			 * spawned by `SpawnCloud`.
			 */
			var middlePoint = new MPos(world.Map.MapSize.X / 2, world.Map.MapSize.Y / 2);
			var middlePointTarget = world.Map.CenterOfCell(middlePoint.ToCPos(world.Map));

			// lDistance and averageSpeed used are for loop condition below.
			var longestCloudDistance = System.Math.Abs(
				(world.Map.DistanceToEdge(middlePointTarget, -delta) + info.Cordon).Length
				+ (world.Map.DistanceToEdge(middlePointTarget, delta) + info.Cordon).Length);
			var averageCloudSpeed = System.Math.Abs((int)info.Speed.Average(s => s.Length));

			var stepPerSpawn = averageCloudSpeed * info.SpawnInterval;
			var stepPerSpawnVector = stepPerSpawn * delta / 1024;

			/*
			 * Spawn clouds.
			 *
			 * Try to make clouds spawning cover the entire map, meanwhile
			 * with some randomization. Choose random spawning point and
			 * find startEdge, then add offset to let they go further to cover
			 * the map. The offset will be increased to simulate the movement
			 * the cloud would have made otherwise.
			 */
			var offset = WVec.Zero;
			while (longestCloudDistance > 0)
			{
				var position = world.Map.ChooseRandomCell(world.SharedRandom);
				var target = world.Map.CenterOfCell(position) + new WVec(0, 0, info.CruiseAltitude.Length);

				var startEdge = target - (world.Map.DistanceToEdge(target, -delta) + info.Cordon).Length * delta / 1024;
				startEdge += offset;
				var finishEdge = target + (world.Map.DistanceToEdge(target, delta) + info.Cordon).Length * delta / 1024;

				var animation = new Animation(world, info.Image, () => WAngle.FromFacing(facing));
				animation.PlayRepeating(info.Sequences.Random(world.SharedRandom));

				world.AddFrameEndTask(w => w.Add(new Cloud(world, animation, startEdge, finishEdge, facing, info)));

				offset += stepPerSpawnVector;
				longestCloudDistance -= stepPerSpawn;
			}
		}
	}
}
