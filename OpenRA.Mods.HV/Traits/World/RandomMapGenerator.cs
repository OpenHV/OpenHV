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
using System.Collections.Immutable;
using System.Linq;
using OpenRA.Mods.Common.MapGenerator;
using OpenRA.Mods.Common.Terrain;
using OpenRA.Mods.Common.Traits;
using OpenRA.Support;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[TraitLocation(SystemActors.EditorWorld)]
	public sealed class RandomMapGeneratorInfo : TraitInfo, IEditorMapGeneratorInfo
	{
		[FieldLoader.Require]
		public readonly string Type = null;

		[FieldLoader.Require]
		[FluentReference]
		public readonly string Name = null;

		[FieldLoader.Require]
		[Desc("Tilesets that are compatible with this map generator.")]
		public readonly ImmutableArray<string> Tilesets = default;

		[FluentReference]
		[Desc("The title to use for generated maps.")]
		public readonly string MapTitle = "label-random-map";

		[Desc("The widget tree to open when the tool is selected.")]
		public readonly string PanelWidget = "MAP_GENERATOR_TOOL_PANEL";

		// This is purely of interest to the linter.
		[FieldLoader.LoadUsing(nameof(FluentReferencesLoader))]
		[FluentReference]
		public readonly List<string> FluentReferences = null;

		[FieldLoader.LoadUsing(nameof(SettingsLoader))]
		public readonly MiniYaml Settings;

		string IMapGeneratorInfo.Type => Type;
		string IMapGeneratorInfo.Name => Name;
		string IMapGeneratorInfo.MapTitle => MapTitle;
		ImmutableArray<string> IEditorMapGeneratorInfo.Tilesets => Tilesets;

		static MiniYaml SettingsLoader(MiniYaml my)
		{
			return my.NodeWithKey("Settings").Value;
		}

		static List<string> FluentReferencesLoader(MiniYaml my)
		{
			return new MapGeneratorSettings(null, my.NodeWithKey("Settings").Value)
				.Options.SelectMany(o => o.GetFluentReferences()).ToList();
		}

		const int FractionMax = Terraformer.FractionMax;

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
			public readonly int SpawnBuildSize = default;

			[FieldLoader.Ignore]
			public readonly IReadOnlySet<byte> ZoneableTerrain;

			[FieldLoader.Ignore]
			public readonly IReadOnlySet<byte> PlayableTerrain;

			[FieldLoader.Require]
			public readonly int ExternalCircularBias = default;

			[FieldLoader.Require]
			public readonly int AreaEntityBonus = default;

			[FieldLoader.Require]
			public readonly int PlayerCountEntityBonus = default;

			[FieldLoader.Require]
			public readonly int CentralSpawnReservationFraction = default;

			[FieldLoader.Require]
			public readonly int MinimumSpawnRadius = default;

			[FieldLoader.Require]
			public readonly int SpawnRegionSize = default;

			[FieldLoader.Require]
			public readonly int SpawnReservation = default;

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

		public IMapGeneratorSettings GetSettings()
		{
			return new MapGeneratorSettings(this, Settings);
		}

		public Map Generate(ModData modData, MapGenerationArgs args)
		{
			var terrainInfo = modData.DefaultTerrainInfo[args.Tileset];
			var size = args.Size;

			var map = new Map(modData, terrainInfo, size);
			var actorPlans = new List<ActorPlan>();

			var param = new Parameters(map, args.Settings);

			var terraformer = new Terraformer(args, map, modData, actorPlans, param.Mirror, param.Rotations);

			var craterSmoothZone = new Terraformer.PathPartitionZone()
			{
				SegmentType = param.CraterSmoothSegmentType,
				MinimumLength = param.MinimumCraterSmoothLength,
				MaximumDeviation = 10,
			};

			var random = new MersenneTwister(param.Seed);
			var elevationRandom = new MersenneTwister(random.Next());
			var pickAnyRandom = new MersenneTwister(random.Next());
			var craterTilingRandom = new MersenneTwister(random.Next());
			var playerRandom = new MersenneTwister(random.Next());

			terraformer.InitMap();

			foreach (var mpos in map.AllCells.MapCoords)
				map.Tiles[mpos] = terraformer.PickTile(pickAnyRandom, param.GrassTile);

			var elevation = terraformer.ElevationNoiseMatrix(
				elevationRandom,
				param.TerrainFeatureSize,
				param.TerrainSmoothing);

			var roughnessMatrix = MatrixUtils.GridVariance(
				elevation,
				param.RoughnessRadius);

			// grassland to crater smooth transition generation
			CellLayer<Terraformer.Side> craterSmoothGrass;
			{
				var cliffMask = MatrixUtils.CalibratedBooleanThreshold(
					roughnessMatrix,
					param.CraterRoughness, FractionMax);

				var plan = terraformer.SliceElevation(elevation, null, param.Crater);
				plan = MatrixUtils.BooleanBlotch(
					plan,
					param.TerrainSmoothing,
					param.SmoothingThreshold, FractionMax,
					param.MinimumGrassCraterThickness,
					true);

				var contours = MatrixUtils.BordersToPoints(plan);
				var partitionMask = cliffMask.Map(masked => masked ? craterSmoothZone : craterSmoothZone);
				var tilingPaths = terraformer.PartitionPaths(
					contours,
					[craterSmoothZone],
					partitionMask,
					param.SegmentedBrushes,
					param.MinimumCraterStraight);

				foreach (var tilingPath in tilingPaths)
					tilingPath
						.OptimizeLoop()
						.ExtendEdge(4);

				craterSmoothGrass = terraformer.PaintLoopsAndFill(
					craterTilingRandom,
					tilingPaths,
					plan[0] ? Terraformer.Side.In : Terraformer.Side.Out,
					null,
					[new MultiBrush().WithTemplate(map, param.CraterTile, CVec.Zero)])
						?? throw new MapGenerationException("Could not fit tiles for crater spots.");
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
			var entityMultiplier =
				(long)zoneableArea * param.AreaEntityBonus +
				(long)param.Players * param.PlayerCountEntityBonus;
			var perSymmetryEntityMultiplier = entityMultiplier / symmetryCount;

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

			terraformer.BakeMap();

			return map;
		}

		public bool TryGenerateMetadata(ModData modData, MapGenerationArgs args, out MapPlayers players, out Dictionary<string, MiniYaml> ruleDefinitions)
		{
			try
			{
				var playerCount = FieldLoader.GetValue<int>("Players", args.Settings.NodeWithKey("Players").Value.Value);

				// Generated maps use the default ruleset
				ruleDefinitions = [];
				players = new MapPlayers(modData.DefaultRules, playerCount);

				return true;
			}
			catch
			{
				players = null;
				ruleDefinitions = null;
				return false;
			}
		}

		public override object Create(ActorInitializer init)
		{
			return new RandomMapGenerator(init, this);
		}
	}

	public class RandomMapGenerator : IEditorTool
	{
		public string Label { get; }
		public string PanelWidget { get; }
		public TraitInfo TraitInfo { get; }
		public bool IsEnabled { get; }

		public RandomMapGenerator(ActorInitializer init, RandomMapGeneratorInfo info)
		{
			Label = info.Name;
			PanelWidget = info.PanelWidget;
			TraitInfo = info;
			IsEnabled = info.Tilesets.Contains(init.Self.World.Map.Tileset);
		}
	}
}
