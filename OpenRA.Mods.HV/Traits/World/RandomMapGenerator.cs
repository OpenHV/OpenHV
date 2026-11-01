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
			public readonly string ShoreSegmentType = default;

			[FieldLoader.Require]
			public readonly int MinimumShoreLength = default;

			[FieldLoader.Require]
			public readonly string WaterCliffSegmentType = default;

			[FieldLoader.Require]
			public readonly int MinimumWaterCliffLength = default;

			[FieldLoader.Require]
			public readonly int WaterCliffRoughness = default;

			[FieldLoader.Require]
			public readonly int MinimumCoastStraight = default;

			[FieldLoader.Require]
			public readonly int Cliffs = default;

			[FieldLoader.Require]
			public readonly int CliffClearance = default;

			[FieldLoader.Require]
			public readonly int MinimumCliffThickness = default;

			[FieldLoader.Require]
			public readonly string CliffSegmentType = default;

			[FieldLoader.Require]
			public readonly int MinimumCliffLength = default;

			[FieldLoader.Require]
			public readonly string CliffGapSegmentType = default;

			[FieldLoader.Require]
			public readonly int MinimumCliffGapLength = default;

			[FieldLoader.Require]
			public readonly int CliffRoughness = default;

			[FieldLoader.Require]
			public readonly int MinimumCliffStraight = default;

			[FieldLoader.Require]
			public readonly int Ramps = default;

			[FieldLoader.Require]
			public readonly int RampFeatureSize = default;

			[FieldLoader.Require]
			public readonly int RampContourSpacing = default;

			[FieldLoader.Require]
			public readonly int MinimumRampThickness = default;

			[FieldLoader.Require]
			public readonly string RampSegmentType = default;

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
			public readonly int ForestFeatureSize = default;

			[FieldLoader.Require]
			public readonly int RockClustersPerPlayer = default;

			[FieldLoader.Require]
			public readonly int MaximumRocksPerCluster = default;

			[FieldLoader.Require]
			public readonly int MinimumRockClusterSize = default;

			[FieldLoader.Require]
			public readonly int MaximumRockClusterSize = default;

			[FieldLoader.LoadUsing(nameof(RockWeightsLoader))]
			public readonly IReadOnlyDictionary<string, int> RockWeights = default;

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

			static object RockWeightsLoader(MiniYaml my)
			{
				return my.NodeWithKey("RockWeights").Value.ToDictionary(subMy =>
					{
						if (Exts.TryParseInt32Invariant(subMy.Value, out var weight))
							return weight;
						else
							throw new YamlException($"Invalid rock weight `{subMy.Value}`");
					});
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

		/// <summary>Paints smooth spots of one terrain within or around another.</summary>
		sealed class SpotPainter
		{
			readonly Map map;
			readonly CustomTerraformer terraformer;
			readonly Parameters param;
			readonly Matrix<int> roughness;

			public readonly Matrix<int> Elevation;

			public SpotPainter(Map map, CustomTerraformer terraformer, Parameters param, Matrix<int> elevation)
			{
				this.map = map;
				this.terraformer = terraformer;
				this.param = param;
				Elevation = elevation;
				roughness = MatrixUtils.GridVariance(elevation, param.RoughnessRadius);
			}

			public static Terraformer.PathPartitionZone SmoothZone(string segmentType, int minimumLength)
			{
				return new Terraformer.PathPartitionZone()
				{
					SegmentType = segmentType,
					MinimumLength = minimumLength,
					MaximumDeviation = 10,
				};
			}

			/// <summary>Follows contours with one of the zones, depending on how rough the terrain is.</summary>
			List<TilingPath> PartitionContours(
				IEnumerable<int2[]> contours,
				Terraformer.PathPartitionZone smoothZone,
				Terraformer.PathPartitionZone roughZone,
				int spotRoughness,
				int minimumStraight)
			{
				var roughMask = MatrixUtils.CalibratedBooleanThreshold(
					roughness,
					spotRoughness, FractionMax);

				var partitionMask = roughMask.Map(masked => masked ? roughZone : smoothZone);
				var tilingPaths = terraformer.PartitionPaths(
					contours,
					[.. new[] { smoothZone, roughZone }.Distinct()],
					partitionMask,
					param.SegmentedBrushes,
					minimumStraight);

				foreach (var tilingPath in tilingPaths)
					tilingPath
						.OptimizeLoop()
						.ExtendEdge(4);

				return tilingPaths;
			}

			/// <summary>Plans spots of another terrain within a region, keeping away from its borders.</summary>
			Matrix<bool> PlanNestedSpots(
				Matrix<int> spotElevation,
				CellLayer<bool> region,
				int fraction,
				int contourSpacing,
				int minimumThickness)
			{
				var mask = CellLayerUtils.ToMatrix(
					terraformer.ImproveSymmetry(region, true, (a, b) => a && b),
					true);

				var plan = terraformer.SliceElevation(
					spotElevation ?? MatrixUtils.ChebyshevRoom(mask, false),
					mask,
					fraction,
					contourSpacing);

				return MatrixUtils.BooleanBlotch(
					plan,
					param.TerrainSmoothing,
					param.SmoothingThreshold, FractionMax,
					minimumThickness,
					false);
			}

			/// <summary>Smooth transitions from the base terrain to spots of another terrain.</summary>
			public CellLayer<Terraformer.Side> PaintSpots(
				MersenneTwister tilingRandom,
				int spotRoughness,
				int fraction,
				int minimumThickness,
				Terraformer.PathPartitionZone smoothZone,
				int minimumStraight,
				ushort tile,
				string description)
			{
				var plan = terraformer.SliceElevation(Elevation, null, fraction);
				plan = MatrixUtils.BooleanBlotch(
					plan,
					param.TerrainSmoothing,
					param.SmoothingThreshold, FractionMax,
					minimumThickness,
					true);

				var tilingPaths = PartitionContours(
					MatrixUtils.BordersToPoints(plan),
					smoothZone,
					smoothZone,
					spotRoughness,
					minimumStraight);

				return terraformer.PaintLoopsAndFill(
					tilingRandom,
					tilingPaths,
					plan[0] ? Terraformer.Side.In : Terraformer.Side.Out,
					null,
					[new MultiBrush().WithTemplate(map, tile, CVec.Zero)])
						?? throw new MapGenerationException($"Could not fit tiles for {description}.");
			}

			/// <summary>
			/// Spots of another terrain within a region outside of previously painted spots.
			/// Reversed segment types have the spot terrain on the outside of their loops.
			/// </summary>
			public CellLayer<Terraformer.Side> PaintNestedSpots(
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
				var plan = PlanNestedSpots(spotElevation, region, fraction, contourSpacing, minimumThickness);

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

			/// <summary>Lakes in the lowlands of a region, with beaches where the terrain is gentle.</summary>
			public CellLayer<Terraformer.Side> PaintLakes(
				MersenneTwister tilingRandom,
				CellLayer<bool> region,
				int fraction,
				int contourSpacing,
				int minimumThickness,
				Terraformer.PathPartitionZone shoreZone,
				Terraformer.PathPartitionZone waterCliffZone,
				int waterCliffRoughness,
				int minimumStraight,
				ushort tile)
			{
				var plan = PlanNestedSpots(Elevation.Map(v => -v), region, fraction, contourSpacing, minimumThickness);
				var tilingPaths = PartitionContours(
					MatrixUtils.BordersToPoints(plan),
					shoreZone,
					waterCliffZone,
					waterCliffRoughness,
					minimumStraight);

				return terraformer.PaintLoopsAndFill(
					tilingRandom,
					tilingPaths,
					plan[0] ? Terraformer.Side.In : Terraformer.Side.Out,
					null,
					[new MultiBrush().WithTemplate(map, tile, CVec.Zero)])
						?? throw new MapGenerationException("Could not fit tiles for lakes.");
			}

			/// <summary>
			/// Plateaus that enclose all craters and leave out all lakes, with gaps in their cliffs
			/// where the terrain is gentle. Segment types have the plateau on the left of their paths.
			/// A contour too rough for any gap is left out, so that no plateau is walled off.
			/// </summary>
			public void PaintPlateaus(
				MersenneTwister tilingRandom,
				CellLayer<Terraformer.Side> craters,
				CellLayer<Terraformer.Side> lakes,
				int fraction,
				int clearance,
				int minimumThickness,
				Terraformer.PathPartitionZone gapZone,
				Terraformer.PathPartitionZone cliffZone,
				int cliffRoughness,
				int minimumStraight)
			{
				const int SeamThickness = 3;

				static Matrix<int> Room(CellLayer<Terraformer.Side> sides)
				{
					return MatrixUtils.ChebyshevRoom(
						CellLayerUtils.ToMatrix(CellLayerUtils.Map(sides, s => s != Terraformer.Side.In), true),
						true);
				}

				var craterRoom = Room(craters);
				var lakeRoom = Room(lakes);

				var plan = MatrixUtils.BooleanBlotch(
					terraformer.SliceElevation(Elevation, null, fraction),
					param.TerrainSmoothing,
					param.SmoothingThreshold, FractionMax,
					minimumThickness,
					false);

				for (var n = 0; n < plan.Data.Length; n++)
					plan[n] = (plan[n] || craterRoom[n] <= clearance) && lakeRoom[n] > clearance;

				// Only clean up the seams: smoothing would erode what was just added, and
				// when thickness fixes fight each other the bias is painted over a wide area.
				plan = MatrixUtils.BooleanBlotch(
					plan,
					0,
					param.SmoothingThreshold, FractionMax,
					SeamThickness,
					true);

				var contours = MatrixUtils.BordersToPoints(plan)
					.Select(contour => contour.Reverse().ToArray());
				var tilingPaths = PartitionContours(contours, gapZone, cliffZone, cliffRoughness, minimumStraight);

				foreach (var tilingPath in tilingPaths)
				{
					var brush = tilingPath.Tile(tilingRandom)
						?? throw new MapGenerationException("Could not fit tiles for plateaus.");
					terraformer.PaintTiling(tilingRandom, brush);
				}
			}
		}

		public override Map Generate(ModData modData, MapGenerationArgs args)
		{
			var terrainInfo = modData.DefaultTerrainInfo[args.Tileset];
			var map = new Map(modData, terrainInfo, args.Size);
			var actorPlans = new List<ActorPlan>();
			var param = new Parameters(map, GenerateParameterYaml(modData, args));

			var terraformer = new CustomTerraformer(args, map, modData, actorPlans, param.Mirror, param.Rotations);

			// The order in which the random streams are drawn must stay stable to keep seeds reproducible
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
			var cliffTilingRandom = new MersenneTwister(random.Next());
			var forestRandom = new MersenneTwister(random.Next());
			var rockRandom = new MersenneTwister(random.Next());
			var rampRandom = new MersenneTwister(random.Next());
			var rampTilingRandom = new MersenneTwister(random.Next());

			terraformer.InitMap();

			PaintBaseTerrain(map, terraformer, param, pickAnyRandom);

			var elevation = terraformer.ElevationNoiseMatrix(
				elevationRandom,
				param.TerrainFeatureSize,
				param.TerrainSmoothing);

			var spotPainter = new SpotPainter(map, terraformer, param, elevation);

			switch (param.Landscape)
			{
				case Landscape.DirtyGrassland:
					PaintDirtyGrassland(
						terraformer,
						spotPainter,
						param,
						craterTilingRandom,
						waterTilingRandom,
						cliffTilingRandom,
						rampRandom,
						rampTilingRandom,
						forestRandom,
						treesTilingRandom);
					break;

				case Landscape.IcySnowland:
					PaintIcySnowland(spotPainter, param, iceTilingRandom, redSnowTilingRandom, waterTilingRandom);
					break;

				case Landscape.SwampyMeadows:
					PaintSwampyMeadows(spotPainter, param, darkGrassTilingRandom, swampTilingRandom, treesTilingRandom);
					break;
			}

			var playable = ChoosePlayableRegion(map, terraformer, param);
			var zoneable = terraformer.GetZoneable(param.ZoneableTerrain, playable);

			PlacePlayerSpawns(map, terraformer, param, playerRandom, zoneable);
			PlaceResources(terraformer, param, resourceRandom, zoneable);
			PlaceRocks(terraformer, param, rockRandom, zoneable);

			terraformer.BakeMap();

			return map;
		}

		static void PaintBaseTerrain(Map map, CustomTerraformer terraformer, Parameters param, MersenneTwister pickAnyRandom)
		{
			var baseTile = param.Landscape == Landscape.IcySnowland ? param.SnowTile : param.GrassTile;
			foreach (var mpos in map.AllCells.MapCoords)
				map.Tiles[mpos] = terraformer.PickTile(pickAnyRandom, baseTile);
		}

		static void PaintDirtyGrassland(
			CustomTerraformer terraformer,
			SpotPainter spotPainter,
			Parameters param,
			MersenneTwister craterTilingRandom,
			MersenneTwister waterTilingRandom,
			MersenneTwister cliffTilingRandom,
			MersenneTwister rampRandom,
			MersenneTwister rampTilingRandom,
			MersenneTwister forestRandom,
			MersenneTwister treesTilingRandom)
		{
			var craters = spotPainter.PaintSpots(
				craterTilingRandom,
				param.CraterRoughness,
				param.Crater,
				param.MinimumGrassCraterThickness,
				SpotPainter.SmoothZone(param.CraterSmoothSegmentType, param.MinimumCraterSmoothLength),
				param.MinimumCraterStraight,
				param.CraterTile,
				"crater spots");

			// Lakes in the lowlands, which are far from the craters on the high ground
			var lakes = CellLayerUtils.Map(craters, _ => Terraformer.Side.Out);
			if (param.Water > 0)
				lakes = spotPainter.PaintLakes(
					waterTilingRandom,
					terraformer.CheckSpace(param.GrassTile),
					param.Water,
					param.WaterContourSpacing,
					param.MinimumWaterThickness,
					new Terraformer.PathPartitionZone()
					{
						RequiredSomewhere = true,
						SegmentType = param.ShoreSegmentType,
						MinimumLength = param.MinimumShoreLength,
						MaximumDeviation = param.MinimumWaterThickness - 1,
					},
					new Terraformer.PathPartitionZone()
					{
						SegmentType = param.WaterCliffSegmentType,
						MinimumLength = param.MinimumWaterCliffLength,
						MaximumDeviation = param.MinimumWaterThickness - 1,
					},
					param.WaterCliffRoughness,
					param.MinimumCoastStraight,
					param.WaterTile);

			if (param.Cliffs > 0)
				spotPainter.PaintPlateaus(
					cliffTilingRandom,
					craters,
					lakes,
					param.Cliffs,
					param.CliffClearance,
					param.MinimumCliffThickness,
					new Terraformer.PathPartitionZone()
					{
						ShouldTile = false,
						RequiredSomewhere = true,
						SegmentType = param.CliffGapSegmentType,
						MinimumLength = param.MinimumCliffGapLength,
						MaximumDeviation = (param.MinimumCliffThickness - 1) / 2,
					},
					new Terraformer.PathPartitionZone()
					{
						SegmentType = param.CliffSegmentType,
						MinimumLength = param.MinimumCliffLength,
						MaximumDeviation = (param.MinimumCliffThickness - 1) / 2,
					},
					param.CliffRoughness,
					param.MinimumCliffStraight);

			// Forests wherever there is still plain grass
			if (param.Trees > 0)
				spotPainter.PaintNestedSpots(
					treesTilingRandom,
					terraformer.ElevationNoiseMatrix(forestRandom, param.ForestFeatureSize, param.TerrainSmoothing),
					terraformer.CheckSpace(param.GrassTile),
					param.Trees,
					param.TreesContourSpacing,
					param.MinimumTreesThickness,
					param.TreesSegmentType,
					true,
					param.TreesTile,
					"trees");

			// Gentle hills on their own, away from the cliffs and forests
			if (param.Ramps > 0)
				spotPainter.PaintNestedSpots(
					rampTilingRandom,
					terraformer.ElevationNoiseMatrix(rampRandom, param.RampFeatureSize, param.TerrainSmoothing),
					terraformer.CheckSpace(param.GrassTile),
					param.Ramps,
					param.RampContourSpacing,
					param.MinimumRampThickness,
					param.RampSegmentType,
					true,
					param.GrassTile,
					"ramps");
		}

		static void PaintIcySnowland(
			SpotPainter spotPainter,
			Parameters param,
			MersenneTwister iceTilingRandom,
			MersenneTwister redSnowTilingRandom,
			MersenneTwister waterTilingRandom)
		{
			var snowIce = spotPainter.PaintSpots(
				iceTilingRandom,
				param.IceRoughness,
				param.Ice,
				param.MinimumSnowIceThickness,
				SpotPainter.SmoothZone(param.IceSmoothSegmentType, param.MinimumIceSmoothLength),
				param.MinimumIceStraight,
				param.IceTile,
				"ice spots");

			// Red snow in the lowlands of the snow
			if (param.RedSnow > 0)
				spotPainter.PaintNestedSpots(
					redSnowTilingRandom,
					spotPainter.Elevation.Map(v => -v),
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
				spotPainter.PaintNestedSpots(
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
		}

		static void PaintSwampyMeadows(
			SpotPainter spotPainter,
			Parameters param,
			MersenneTwister darkGrassTilingRandom,
			MersenneTwister swampTilingRandom,
			MersenneTwister treesTilingRandom)
		{
			var grassDarkGrass = spotPainter.PaintSpots(
				darkGrassTilingRandom,
				param.DarkGrassRoughness,
				param.DarkGrass,
				param.MinimumGrassDarkGrassThickness,
				SpotPainter.SmoothZone(param.DarkGrassSmoothSegmentType, param.MinimumDarkGrassSmoothLength),
				param.MinimumDarkGrassStraight,
				param.DarkGrassTile,
				"dark grass spots");

			var meadow = CellLayerUtils.Map(grassDarkGrass, s => s == Terraformer.Side.Out);

			// Swamp in the lowlands of the grass
			if (param.Swamp > 0)
			{
				var grassSwamp = spotPainter.PaintNestedSpots(
					swampTilingRandom,
					spotPainter.Elevation.Map(v => -v),
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
				spotPainter.PaintNestedSpots(
					treesTilingRandom,
					spotPainter.Elevation,
					meadow,
					param.Trees,
					param.TreesContourSpacing,
					param.MinimumTreesThickness,
					param.TreesSegmentType,
					true,
					param.TreesTile,
					"trees");
		}

		static CellLayer<bool> ChoosePlayableRegion(Map map, CustomTerraformer terraformer, Parameters param)
		{
			// For circle-in-mountains, the outside is unplayable and should never count as
			// the largest/preferred region.
			CellLayer<bool> poison = null;
			if (param.ExternalCircularBias > 0)
				poison = terraformer.CenteredCircle(
					false, true, CellLayerUtils.Radius(map.Tiles) - new WDist(1024));

			var playable = terraformer.ChoosePlayableRegion(
				terraformer.CheckSpace(param.PlayableTerrain, true, false, true),
				poison)
					?? throw new MapGenerationException("could not find a playable region");

			var minimumPlayableSpace = (int)(param.Players * Math.PI * param.SpawnBuildSize * param.SpawnBuildSize);
			if (playable.Count(p => p) < minimumPlayableSpace)
				throw new MapGenerationException("playable space is too small");

			return playable;
		}

		static void PlacePlayerSpawns(
			Map map,
			CustomTerraformer terraformer,
			Parameters param,
			MersenneTwister playerRandom,
			CellLayer<bool> zoneable)
		{
			var symmetryCount = Symmetry.RotateAndMirrorProjectionCount(param.Rotations, param.Mirror);
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
		}

		static void PlaceResources(
			CustomTerraformer terraformer,
			Parameters param,
			MersenneTwister resourceRandom,
			CellLayer<bool> zoneable)
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

		static void PlaceRocks(
			CustomTerraformer terraformer,
			Parameters param,
			MersenneTwister rockRandom,
			CellLayer<bool> zoneable)
		{
			// Rocks are projected but resources are not, so keep clear of every projection of them
			var rockZoneable = terraformer.ImproveSymmetry(zoneable, false, (a, b) => a && b);

			var symmetryCount = Symmetry.RotateAndMirrorProjectionCount(param.Rotations, param.Mirror);
			var targetClusterCount = param.RockClustersPerPlayer * param.Players / symmetryCount;
			for (var i = 0; i < targetClusterCount; i++)
			{
				var added = terraformer.AddActorCluster(
					rockRandom,
					rockZoneable,
					param.RockWeights,
					rockRandom.Next(param.MaximumRocksPerCluster) + 1,
					1,
					param.MinimumRockClusterSize,
					param.MaximumRockClusterSize,
					1,
					true);
				if (added == 0)
					break;
			}
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
