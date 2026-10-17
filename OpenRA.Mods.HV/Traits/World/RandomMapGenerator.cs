#region Copyright & License Information
/*
 * Copyright 2025-2026 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.MapGenerator;
using OpenRA.Mods.Common.Terrain;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.HV.MapGenerator;
using OpenRA.Support;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[TraitLocation(SystemActors.EditorWorld)]
	public sealed class RandomMapGeneratorInfo : MapGeneratorBaseInfo
	{
		const int FractionMax = Terraformer.FractionMax;

		public enum Landscape { DirtyGrassland, IcySnowland, SwampyMeadows }

		sealed class Parameters
		{
			[FieldLoader.Require]
			public readonly int Seed = default;

			[FieldLoader.Require]
			public readonly int Rotations = default;

			[FieldLoader.LoadUsing(nameof(MirrorLoader))]
			public readonly Symmetry.Mirror Mirror = default;

			[FieldLoader.Require]
			public readonly int EnforceSymmetry = default;

			[FieldLoader.Require]
			public readonly int Players = default;

			[FieldLoader.Require]
			public readonly ushort GrassTile = default;

			[FieldLoader.Require]
			public readonly ushort CraterTile = default;

			[FieldLoader.Require]
			public readonly ushort SnowTile = default;

			[FieldLoader.Require]
			public readonly ushort IceTile = default;

			[FieldLoader.Require]
			public readonly ushort RedSnowTile = default;

			[FieldLoader.Require]
			public readonly ushort WaterTile = default;

			[FieldLoader.Require]
			public readonly ushort DarkGrassTile = default;

			[FieldLoader.Require]
			public readonly ushort SwampTile = default;

			[FieldLoader.Require]
			public readonly ushort TreesTile = default;

			[FieldLoader.Require]
			public readonly Landscape Landscape = default;

			[FieldLoader.Require]
			public readonly int TerrainFeatureSize = default;

			[FieldLoader.Require]
			public readonly int TerrainSmoothing = default;

			[FieldLoader.Require]
			public readonly int RoughnessRadius = default;

			[FieldLoader.Require]
			public readonly int CraterRoughness = default;

			[FieldLoader.Require]
			public readonly int Crater = default;

			[FieldLoader.Require]
			public readonly int SmoothingThreshold = default;

			[FieldLoader.Require]
			public readonly int MinimumGrassCraterThickness = default;

			[FieldLoader.Require]
			public readonly string CraterSmoothSegmentType = default;

			[FieldLoader.Require]
			public readonly int MinimumCraterSmoothLength = default;

			[FieldLoader.Ignore]
			public readonly IReadOnlyList<MultiBrush> SegmentedBrushes;

			[FieldLoader.Require]
			public readonly int MinimumCraterStraight = default;

			[FieldLoader.Require]
			public readonly int IceRoughness = default;

			[FieldLoader.Require]
			public readonly int Ice = default;

			[FieldLoader.Require]
			public readonly int MinimumSnowIceThickness = default;

			[FieldLoader.Require]
			public readonly string IceSmoothSegmentType = default;

			[FieldLoader.Require]
			public readonly int MinimumIceSmoothLength = default;

			[FieldLoader.Require]
			public readonly int MinimumIceStraight = default;

			[FieldLoader.Require]
			public readonly int RedSnow = default;

			[FieldLoader.Require]
			public readonly int RedSnowContourSpacing = default;

			[FieldLoader.Require]
			public readonly int MinimumRedSnowThickness = default;

			[FieldLoader.Require]
			public readonly string RedSnowSegmentType = default;

			[FieldLoader.Require]
			public readonly int Water = default;

			[FieldLoader.Require]
			public readonly int WaterContourSpacing = default;

			[FieldLoader.Require]
			public readonly int MinimumWaterThickness = default;

			[FieldLoader.Require]
			public readonly string WaterSegmentType = default;

			[FieldLoader.Require]
			public readonly int DarkGrassRoughness = default;

			[FieldLoader.Require]
			public readonly int DarkGrass = default;

			[FieldLoader.Require]
			public readonly int MinimumGrassDarkGrassThickness = default;

			[FieldLoader.Require]
			public readonly string DarkGrassSmoothSegmentType = default;

			[FieldLoader.Require]
			public readonly int MinimumDarkGrassSmoothLength = default;

			[FieldLoader.Require]
			public readonly int MinimumDarkGrassStraight = default;

			[FieldLoader.Require]
			public readonly int Swamp = default;

			[FieldLoader.Require]
			public readonly int SwampContourSpacing = default;

			[FieldLoader.Require]
			public readonly int MinimumSwampThickness = default;

			[FieldLoader.Require]
			public readonly string SwampSegmentType = default;

			[FieldLoader.Require]
			public readonly int Trees = default;

			[FieldLoader.Require]
			public readonly int TreesContourSpacing = default;

			[FieldLoader.Require]
			public readonly int MinimumTreesThickness = default;

			[FieldLoader.Require]
			public readonly string TreesSegmentType = default;

			[FieldLoader.Require]
			public readonly int SpawnBuildSize = default;

			[FieldLoader.Ignore]
			public readonly IReadOnlySet<byte> ZoneableTerrain;

			[FieldLoader.Ignore]
			public readonly IReadOnlySet<byte> PlayableTerrain;

			[FieldLoader.Require]
			public readonly int ExternalCircularBias = default;

			[FieldLoader.Require]
			public readonly int CentralSpawnReservationFraction = default;

			[FieldLoader.Require]
			public readonly int MinimumSpawnRadius = default;

			[FieldLoader.Require]
			public readonly int SpawnRegionSize = default;

			[FieldLoader.Require]
			public readonly int SpawnReservation = default;

			[FieldLoader.Require]
			public readonly int ResourceSpawnsPerPlayer = default;

			[FieldLoader.Ignore]
			public readonly ResourceLayerInfo.ResourceTypeInfo DefaultResource;

			public Parameters(Map map, MiniYaml my)
			{
				FieldLoader.Load(this, my);

				switch (Rotations)
				{
					case 1:
					case 2:
					case 4:
						break;
					default:
						EnforceSymmetry = 0;
						break;
				}

				SegmentedBrushes = MultiBrush.LoadCollection(map, "Segmented");

				var terrainInfo = (ITemplatedTerrainInfo)map.Rules.TerrainInfo;

				IReadOnlySet<byte> ParseTerrainIndexes(string key)
				{
					return my.NodeWithKey(key).Value.Value
						.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
						.Select(terrainInfo.GetTerrainIndex)
						.ToFrozenSet();
				}

				PlayableTerrain = ParseTerrainIndexes("PlayableTerrain");
				ZoneableTerrain = ParseTerrainIndexes("ZoneableTerrain");

				var resourceTypes = map.Rules.Actors[SystemActors.World].TraitInfo<UndergroundResourceLayerInfo>().ResourceTypes;
				if (!resourceTypes.TryGetValue(my.NodeWithKey("DefaultResource").Value.Value, out DefaultResource))
					throw new YamlException("DefaultResource is not valid");

				Validate();
			}

			static object MirrorLoader(MiniYaml my)
			{
				if (Symmetry.TryParseMirror(my.NodeWithKey("Mirror").Value.Value, out var mirror))
					return mirror;
				else
					throw new YamlException($"Invalid Mirror value `{my.NodeWithKey("Mirror").Value.Value}`");
			}

			public void Validate()
			{
				if (Rotations < 1)
					throw new MapGenerationException("Rotations must be >= 1.");

				if (Players > 32)
					throw new MapGenerationException("Total number of players must not exceed 32.");

				var symmetryCount = Symmetry.RotateAndMirrorProjectionCount(Rotations, Mirror);
				if (Players % symmetryCount != 0)
					throw new MapGenerationException($"Total number of players must be a multiple of {symmetryCount}.");
			}
		}

		public override Map Generate(ModData modData, MapGenerationArgs args)
		{
			var terrainInfo = modData.DefaultTerrainInfo[args.Tileset];
			var map = new Map(modData, terrainInfo, args.Size);
			var actorPlans = new List<ActorPlan>();
			var param = new Parameters(map, GenerateParameterYaml(modData, args));

			var terraformer = new CustomTerraformer(args, map, modData, actorPlans, param.Mirror, param.Rotations);

			var craterSmoothZone = new Terraformer.PathPartitionZone()
			{
				SegmentType = param.CraterSmoothSegmentType,
				MinimumLength = param.MinimumCraterSmoothLength,
				MaximumDeviation = 10,
			};

			var iceSmoothZone = new Terraformer.PathPartitionZone()
			{
				SegmentType = param.IceSmoothSegmentType,
				MinimumLength = param.MinimumIceSmoothLength,
				MaximumDeviation = 10,
			};

			var darkGrassSmoothZone = new Terraformer.PathPartitionZone()
			{
				SegmentType = param.DarkGrassSmoothSegmentType,
				MinimumLength = param.MinimumDarkGrassSmoothLength,
				MaximumDeviation = 10,
			};

			var random = new MersenneTwister(param.Seed);
			var elevationRandom = new MersenneTwister(random.Next());
			var pickAnyRandom = new MersenneTwister(random.Next());
			var craterTilingRandom = new MersenneTwister(random.Next());
			var playerRandom = new MersenneTwister(random.Next());
			var resourceRandom = new MersenneTwister(random.Next());
			var iceTilingRandom = new MersenneTwister(random.Next());
			var redSnowTilingRandom = new MersenneTwister(random.Next());
			var waterTilingRandom = new MersenneTwister(random.Next());
			var darkGrassTilingRandom = new MersenneTwister(random.Next());
			var swampTilingRandom = new MersenneTwister(random.Next());
			var treesTilingRandom = new MersenneTwister(random.Next());

			terraformer.InitMap();

			var baseTile = param.Landscape == Landscape.IcySnowland ? param.SnowTile : param.GrassTile;
			foreach (var mpos in map.AllCells.MapCoords)
				map.Tiles[mpos] = terraformer.PickTile(pickAnyRandom, baseTile);

			var elevation = terraformer.ElevationNoiseMatrix(
				elevationRandom,
				param.TerrainFeatureSize,
				param.TerrainSmoothing);

			var roughnessMatrix = MatrixUtils.GridVariance(
				elevation,
				param.RoughnessRadius);

			// Smooth transitions from the base terrain to spots of another terrain
			CellLayer<Terraformer.Side> PaintSpots(
				MersenneTwister tilingRandom,
				int roughness,
				int fraction,
				int minimumThickness,
				Terraformer.PathPartitionZone smoothZone,
				int minimumStraight,
				ushort tile,
				string description)
			{
				var cliffMask = MatrixUtils.CalibratedBooleanThreshold(
					roughnessMatrix,
					roughness, FractionMax);

				var plan = terraformer.SliceElevation(elevation, null, fraction);
				plan = MatrixUtils.BooleanBlotch(
					plan,
					param.TerrainSmoothing,
					param.SmoothingThreshold, FractionMax,
					minimumThickness,
					true);

				var contours = MatrixUtils.BordersToPoints(plan);
				var partitionMask = cliffMask.Map(masked => masked ? smoothZone : smoothZone);
				var tilingPaths = terraformer.PartitionPaths(
					contours,
					[smoothZone],
					partitionMask,
					param.SegmentedBrushes,
					minimumStraight);

				foreach (var tilingPath in tilingPaths)
					tilingPath
						.OptimizeLoop()
						.ExtendEdge(4);

				return terraformer.PaintLoopsAndFill(
					tilingRandom,
					tilingPaths,
					plan[0] ? Terraformer.Side.In : Terraformer.Side.Out,
					null,
					[new MultiBrush().WithTemplate(map, tile, CVec.Zero)])
						?? throw new MapGenerationException($"Could not fit tiles for {description}.");
			}

			// Spots of another terrain within a region outside of previously painted spots.
			// Reversed segment types have the spot terrain on the outside of their loops.
			CellLayer<Terraformer.Side> PaintNestedSpots(
				MersenneTwister tilingRandom,
				Matrix<int> spotElevation,
				CellLayer<bool> region,
				int fraction,
				int contourSpacing,
				int minimumThickness,
				string segmentType,
				bool reversedSegmentType,
				ushort tile,
				string description)
			{
				var mask = CellLayerUtils.ToMatrix(
					terraformer.ImproveSymmetry(region, true, (a, b) => a && b),
					true);

				var plan = terraformer.SliceElevation(
					spotElevation ?? MatrixUtils.ChebyshevRoom(mask, false),
					mask,
					fraction,
					contourSpacing);
				plan = MatrixUtils.BooleanBlotch(
					plan,
					param.TerrainSmoothing,
					param.SmoothingThreshold, FractionMax,
					minimumThickness,
					false);

				var contours = CellLayerUtils.FromMatrixPoints(
					MatrixUtils.BordersToPoints(plan),
					map.Tiles);
				if (reversedSegmentType)
					contours = contours.Select(contour => contour.Reverse().ToArray()).ToArray();

				var tilingPaths = contours
					.Select(contour =>
						TilingPath.QuickCreate(
								map,
								param.SegmentedBrushes,
								contour,
								(minimumThickness - 1) / 2,
								segmentType,
								segmentType)
							.ExtendEdge(4))
					.ToArray();

				MultiBrush[] brushes = [new MultiBrush().WithTemplate(map, tile, CVec.Zero)];
				var sides = terraformer.PaintLoopsAndFill(
					tilingRandom,
					tilingPaths,
					plan[0] != reversedSegmentType ? Terraformer.Side.In : Terraformer.Side.Out,
					reversedSegmentType ? brushes : null,
					reversedSegmentType ? null : brushes)
						?? throw new MapGenerationException($"Could not fit tiles for {description}.");

				// Always report the spots as being inside
				return reversedSegmentType ? CellLayerUtils.Map(sides, s => (Terraformer.Side)(-(sbyte)s)) : sides;
			}

			switch (param.Landscape)
			{
				case Landscape.DirtyGrassland:
					PaintSpots(
						craterTilingRandom,
						param.CraterRoughness,
						param.Crater,
						param.MinimumGrassCraterThickness,
						craterSmoothZone,
						param.MinimumCraterStraight,
						param.CraterTile,
						"crater spots");
					break;

				case Landscape.IcySnowland:
					var snowIce = PaintSpots(
						iceTilingRandom,
						param.IceRoughness,
						param.Ice,
						param.MinimumSnowIceThickness,
						iceSmoothZone,
						param.MinimumIceStraight,
						param.IceTile,
						"ice spots");

					// Red snow in the lowlands of the snow
					if (param.RedSnow > 0)
						PaintNestedSpots(
							redSnowTilingRandom,
							elevation.Map(v => -v),
							CellLayerUtils.Map(snowIce, s => s == Terraformer.Side.Out),
							param.RedSnow,
							param.RedSnowContourSpacing,
							param.MinimumRedSnowThickness,
							param.RedSnowSegmentType,
							false,
							param.RedSnowTile,
							"red snow spots");

					// Open water in the middle of the ice
					if (param.Water > 0)
						PaintNestedSpots(
							waterTilingRandom,
							null,
							CellLayerUtils.Map(snowIce, s => s == Terraformer.Side.In),
							param.Water,
							param.WaterContourSpacing,
							param.MinimumWaterThickness,
							param.WaterSegmentType,
							false,
							param.WaterTile,
							"water");

					break;

				case Landscape.SwampyMeadows:
					var grassDarkGrass = PaintSpots(
						darkGrassTilingRandom,
						param.DarkGrassRoughness,
						param.DarkGrass,
						param.MinimumGrassDarkGrassThickness,
						darkGrassSmoothZone,
						param.MinimumDarkGrassStraight,
						param.DarkGrassTile,
						"dark grass spots");

					var meadow = CellLayerUtils.Map(grassDarkGrass, s => s == Terraformer.Side.Out);

					// Swamp in the lowlands of the grass
					if (param.Swamp > 0)
					{
						var grassSwamp = PaintNestedSpots(
							swampTilingRandom,
							elevation.Map(v => -v),
							meadow,
							param.Swamp,
							param.SwampContourSpacing,
							param.MinimumSwampThickness,
							param.SwampSegmentType,
							false,
							param.SwampTile,
							"swamp");

						meadow = CellLayerUtils.Intersect(
							[meadow, CellLayerUtils.Map(grassSwamp, s => s == Terraformer.Side.Out)]);
					}

					// Trees on the slopes below the dark grass
					if (param.Trees > 0)
						PaintNestedSpots(
							treesTilingRandom,
							elevation,
							meadow,
							param.Trees,
							param.TreesContourSpacing,
							param.MinimumTreesThickness,
							param.TreesSegmentType,
							true,
							param.TreesTile,
							"trees");

					break;
			}

			CellLayer<bool> playable;
			{
				// For circle-in-mountains, the outside is unplayable and should never count as
				// the largest/preferred region.
				CellLayer<bool> poison = null;
				if (param.ExternalCircularBias > 0)
					poison = terraformer.CenteredCircle(
						false, true, CellLayerUtils.Radius(map.Tiles) - new WDist(1024));

				playable = terraformer.ChoosePlayableRegion(
					terraformer.CheckSpace(param.PlayableTerrain, true, false, true),
					poison)
						?? throw new MapGenerationException("could not find a playable region");

				var minimumPlayableSpace = (int)(param.Players * Math.PI * param.SpawnBuildSize * param.SpawnBuildSize);
				if (playable.Count(p => p) < minimumPlayableSpace)
					throw new MapGenerationException("playable space is too small");
			}

			var zoneable = terraformer.GetZoneable(param.ZoneableTerrain, playable);

			var zoneableArea = zoneable.Count(v => v);
			var symmetryCount = Symmetry.RotateAndMirrorProjectionCount(param.Rotations, param.Mirror);

			// Spawn generation
			var symmetryPlayers = param.Players / symmetryCount;
			for (var iteration = 0; iteration < symmetryPlayers; iteration++)
			{
				var chosenSpawnLocation = terraformer.ChooseSpawnInZoneable(
					playerRandom,
					zoneable,
					param.CentralSpawnReservationFraction,
					param.MinimumSpawnRadius,
					param.SpawnRegionSize,
					param.SpawnReservation)
						?? throw new MapGenerationException("Not enough room for player spawns");

				var playerSpawn = new ActorPlan(map, "mpspawn")
				{
					Location = chosenSpawnLocation,
				};

				terraformer.ProjectPlaceDezoneActor(playerSpawn, zoneable, new WDist(param.SpawnReservation * 1024));
			}

			{
				var targetResourceSpawnCount = param.ResourceSpawnsPerPlayer * param.Players;
				for (var i = 0; i < targetResourceSpawnCount; i++)
				{
					var added = terraformer.AddResource(
						param.DefaultResource,
						resourceRandom,
						zoneable,
						"miner", // account for space
						new WDist(param.SpawnReservation * 1024));
					if (!added)
						break;
				}

				terraformer.ZoneFromResources(zoneable, false);
			}

			terraformer.BakeMap();

			return map;
		}

		public override object Create(ActorInitializer init)
		{
			return new RandomMapGenerator(init, this);
		}
	}

	public class RandomMapGenerator : MapGeneratorBase
	{
		public RandomMapGenerator(ActorInitializer init, RandomMapGeneratorInfo info)
			: base(init, info) { }
	}
}
