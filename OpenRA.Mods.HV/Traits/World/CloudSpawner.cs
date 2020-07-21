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
		[SequenceReference("Image")]
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

			var facing = 256 * info.WindDirection / 32;
			var delta = new WVec(0, -1024, 0).Rotate(WRot.FromFacing(facing));

			var target = self.World.Map.CenterOfCell(position) + new WVec(0, 0, info.CruiseAltitude.Length);
			var startEdge = target - (self.World.Map.DistanceToEdge(target, -delta) + info.Cordon).Length * delta / 1024;
			var finishEdge = target + (self.World.Map.DistanceToEdge(target, delta) + info.Cordon).Length * delta / 1024;

			var animation = new Animation(self.World, info.Image, () => WAngle.FromFacing(facing));
			animation.PlayRepeating(info.Sequences.Random(self.World.SharedRandom));

			self.World.AddFrameEndTask(w => w.Add(new Cloud(self.World, animation, startEdge, finishEdge, facing, info)));
		}
	}
}
