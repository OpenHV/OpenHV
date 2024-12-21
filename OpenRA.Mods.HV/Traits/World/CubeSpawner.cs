#region Copyright & License Information
/*
 * Copyright 2019-2024 The OpenHV Developers (see AUTHORS)
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
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[TraitLocation(SystemActors.World)]
	public class CubeSpawnerInfo : TraitInfo, ILobbyOptions
	{
		[FluentReference]
		[Desc("Descriptive label for the cubes checkbox in the lobby.")]
		public readonly string CheckboxLabel = "checkbox-crates.label";

		[FluentReference]
		[Desc("Tooltip description for the cubes checkbox in the lobby.")]
		public readonly string CheckboxDescription = "checkbox-crates.description";

		[Desc("Default value of the cubes checkbox in the lobby.")]
		public readonly bool CheckboxEnabled = false;

		[Desc("Prevent the cubes state from being changed in the lobby.")]
		public readonly bool CheckboxLocked = false;

		[Desc("Whether to display the cubes checkbox in the lobby.")]
		public readonly bool CheckboxVisible = true;

		[Desc("Display order for the cubes checkbox in the lobby.")]
		public readonly int CheckboxDisplayOrder = 0;

		[Desc("Minimum number of cubes.")]
		public readonly int Minimum = 1;

		[Desc("Maximum number of cubes.")]
		public readonly int Maximum = 255;

		[Desc("Average time (ticks) between cube spawn.")]
		public readonly int SpawnInterval = 180 * 25;

		[Desc("Delay (in ticks) before the first cube spawns.")]
		public readonly int InitialSpawnDelay = 0;

		[Desc("Which terrain types can we drop on?")]
		public readonly HashSet<string> ValidGround = new() { "Clear" };

		[ActorReference]
		[Desc("Cube actors to drop.")]
		public readonly string[] CubeActors = { "cube" };

		[Desc("Chance of each cube actor spawning.")]
		public readonly int[] CubeActorShares = { 10 };

		IEnumerable<LobbyOption> ILobbyOptions.LobbyOptions(MapPreview map)
		{
			yield return new LobbyBooleanOption(map, "cubes", CheckboxLabel, CheckboxDescription, CheckboxVisible, CheckboxDisplayOrder, CheckboxEnabled, CheckboxLocked);
		}

		public override object Create(ActorInitializer init) { return new CubeSpawner(init.Self, this); }
	}

	public class CubeSpawner : ITick, INotifyCreated
	{
		public bool Enabled { get; private set; }

		readonly Actor self;
		readonly CubeSpawnerInfo info;

		int cubes;
		int ticks;

		public CubeSpawner(Actor self, CubeSpawnerInfo info)
		{
			this.self = self;
			this.info = info;

			ticks = info.InitialSpawnDelay;
		}

		void INotifyCreated.Created(Actor self)
		{
			Enabled = self.World.LobbyInfo.GlobalSettings
				.OptionOrDefault("cubes", info.CheckboxEnabled);
		}

		void ITick.Tick(Actor self)
		{
			if (!Enabled)
				return;

			if (--ticks <= 0)
			{
				ticks = info.SpawnInterval;

				var toSpawn = Math.Max(0, info.Minimum - cubes)
					+ (cubes < info.Maximum && info.Maximum > info.Minimum ? 1 : 0);

				for (var n = 0; n < toSpawn; n++)
					SpawnCube(self);
			}
		}

		void SpawnCube(Actor self)
		{
			var dropCell = ChooseDropCell(self, 100);

			if (dropCell == null)
				return;

			var location = dropCell.Value;
			var cubeActor = ChooseCubeActor();

			self.World.AddFrameEndTask(w => w.CreateActor(cubeActor,
				new TypeDictionary { new OwnerInit(w.WorldActor.Owner), new LocationInit(location) }));
		}

		CPos? ChooseDropCell(Actor self, int maxTries)
		{
			for (var n = 0; n < maxTries; n++)
			{
				var p = self.World.Map.ChooseRandomCell(self.World.SharedRandom);

				// Is this valid terrain?
				var terrainType = self.World.Map.GetTerrainInfo(p).Type;
				if (!info.ValidGround.Contains(terrainType))
					continue;

				// Don't drop on any actors
				if (self.World.WorldActor.Trait<BuildingInfluence>().AnyBuildingAt(p)
					|| self.World.ActorMap.GetActorsAt(p).Any())
					continue;

				return p;
			}

			return null;
		}

		string ChooseCubeActor()
		{
			var cubeshares = info.CubeActorShares;
			var n = self.World.SharedRandom.Next(cubeshares.Sum());

			var cumulativeShares = 0;
			for (var i = 0; i < cubeshares.Length; i++)
			{
				cumulativeShares += cubeshares[i];
				if (n <= cumulativeShares)
					return info.CubeActors[i];
			}

			return null;
		}

		public void IncrementCubes()
		{
			cubes++;
		}

		public void DecrementCubes()
		{
			cubes--;
		}
	}
}
