#region Copyright & License Information
/*
 * Copyright 2026 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using OpenRA.Mods.Common.Lint;
using OpenRA.Mods.Common.MapGenerator;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Lint
{
	public class CheckTerrainTypes : ILintPass, ILintRulesPass
	{
		// The engine has no lint attribute for terrain types, so references are found by their field name.
		static readonly string[] TerrainFieldSuffixes = ["Terrain", "TerrainType", "TerrainTypes"];
		static readonly string[] TerrainFieldNames = ["TerrainSpeeds", "TerrainModifier", "ValidGround", "ValidWater"];

		// `*` is the wildcard of the resource layers. `None` matches nothing where an empty list would allow everything.
		static readonly string[] Placeholders = ["*", "None"];

		void ILintPass.Run(Action<string> emitError, Action<string> emitWarning, ModData modData)
		{
			var referenced = GetReferences(modData.DefaultRules).Select(r => r.TerrainType).ToHashSet();
			foreach (var terrainInfo in modData.DefaultTerrainInfo.Values)
				foreach (var terrainType in terrainInfo.TerrainTypes)
					if (!referenced.Contains(terrainType.Type))
						emitWarning($"Terrain type `{terrainType.Type}` of tileset `{terrainInfo.Id}` is not referenced by any rule.");
		}

		void ILintRulesPass.Run(Action<string> emitError, Action<string> emitWarning, ModData modData, Ruleset rules)
		{
			// The default rules are not bound to a tileset.
			var terrainInfos = rules.TerrainInfo != null ? [rules.TerrainInfo] : modData.DefaultTerrainInfo.Values;
			var defined = terrainInfos.SelectMany(t => t.TerrainTypes).Select(t => t.Type).ToHashSet();

			foreach (var (terrainType, source) in GetReferences(rules))
				if (!defined.Contains(terrainType) && !Placeholders.Contains(terrainType))
					emitError($"Undefined terrain type `{terrainType}` referenced by {source}.");
		}

		static IEnumerable<(string TerrainType, string Source)> GetReferences(Ruleset rules)
		{
			foreach (var actor in rules.Actors.Values)
			{
				foreach (var traitInfo in actor.TraitInfos<TraitInfo>())
				{
					var source = $"trait `{traitInfo.GetType().Name}` on actor `{actor.Name}`";
					foreach (var terrainType in GetTerrainTypes(traitInfo))
						yield return (terrainType, source);

					foreach (var resourceType in GetResourceTypes(traitInfo))
						foreach (var terrainType in GetTerrainTypes(resourceType))
							yield return (terrainType, source);

					if (traitInfo is not MapGeneratorBaseInfo mapGenerator)
						continue;

					foreach (var parameter in mapGenerator.Options.OfType<MapGeneratorMultiChoiceOption>()
						.SelectMany(o => o.Choices.Values)
						.SelectMany(c => c.Parameters)
						.Where(p => IsTerrainReference(p.Key)))
						foreach (var terrainType in FieldLoader.GetValue<string[]>(parameter.Key, parameter.Value.Value))
							yield return (terrainType, source);
				}
			}

			foreach (var weapon in rules.Weapons)
			{
				var source = $"weapon `{weapon.Key}`";
				foreach (var info in weapon.Value.Warheads.Cast<object>().Append(weapon.Value.Projectile))
					foreach (var terrainType in GetTerrainTypes(info))
						yield return (terrainType, source);
			}
		}

		static bool IsTerrainReference(string name)
		{
			return TerrainFieldNames.Contains(name) || TerrainFieldSuffixes.Any(s => name.EndsWith(s, StringComparison.Ordinal));
		}

		static IEnumerable<string> GetTerrainTypes(object info)
		{
			if (info == null)
				return [];

			return Utility.GetFields(info.GetType())
				.Where(f => IsTerrainReference(f.Name))
				.SelectMany(f => GetValues(f.GetValue(info)))
				.Where(t => !string.IsNullOrEmpty(t));
		}

		static IEnumerable<string> GetValues(object field)
		{
			return field switch
			{
				string terrainType => [terrainType],
				ImmutableArray<string> { IsDefault: true } => [],
				IEnumerable<string> terrainTypes => terrainTypes,
				IDictionary dictionary => dictionary.Keys.OfType<string>(),
				_ => []
			};
		}

		static IEnumerable<ResourceLayerInfo.ResourceTypeInfo> GetResourceTypes(TraitInfo traitInfo)
		{
			return Utility.GetFields(traitInfo.GetType())
				.Select(f => f.GetValue(traitInfo))
				.OfType<IEnumerable<KeyValuePair<string, ResourceLayerInfo.ResourceTypeInfo>>>()
				.SelectMany(r => r.Select(kv => kv.Value));
		}
	}
}
