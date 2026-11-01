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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using OpenRA.Mods.Common.Lint;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Lint
{
	public class CheckUnusedNodes : ILintPass
	{
		void ILintPass.Run(Action<string> emitError, Action<string> emitWarning, ModData modData)
		{
			var rules = LoadNodes(modData, modData.Manifest.Rules);
			var sequences = LoadNodes(modData, modData.Manifest.Sequences);
			var weapons = LoadNodes(modData, modData.Manifest.Weapons);
			var usedWeapons = GetUsedWeapons(modData.DefaultRules);

			// Maps can use what the mod defines and define nodes of their own, some in files shared between maps.
			var mapFiles = new HashSet<string>();
			foreach (var package in modData.MapCache.EnumerateMapPackagesWithoutCaching())
			{
				using (var map = new Map(modData, package))
				{
					rules.AddRange(LoadMapNodes(map, map.RuleDefinitions, mapFiles));
					sequences.AddRange(LoadMapNodes(map, map.SequenceDefinitions, mapFiles));
					weapons.AddRange(LoadMapNodes(map, map.WeaponDefinitions, mapFiles));

					// Broken map rules are already reported by the map checks.
					if (!map.InvalidCustomRules)
						usedWeapons.UnionWith(GetUsedWeapons(map.Rules));
				}
			}

			CheckTemplates(emitWarning, rules);
			CheckTemplates(emitWarning, sequences);
			CheckTemplates(emitWarning, weapons);

			var inheritedWeapons = weapons.SelectMany(GetInherited).ToHashSet();
			foreach (var weapon in weapons.Where(w => !IsTemplate(w) && !w.Key.StartsWith('-')))
				if (!usedWeapons.Contains(weapon.Key.ToLowerInvariant()) && !inheritedWeapons.Contains(weapon.Key))
					emitWarning($"{weapon.Location} defines weapon `{weapon.Key}` that is not referenced by any actor or weapon in use.");
		}

		static List<MiniYamlNode> LoadNodes(ModData modData, IEnumerable<string> files)
		{
			return files.SelectMany(f => MiniYaml.FromStream(modData.DefaultFileSystem.Open(f), f)).ToList();
		}

		static IEnumerable<MiniYamlNode> LoadMapNodes(Map map, MiniYaml definitions, HashSet<string> mapFiles)
		{
			if (definitions == null)
				return [];

			return FieldLoader.GetValue<ImmutableArray<string>>("value", definitions.Value)
				.Select(f => (File: f, Location: f.Contains('|') ? f : $"{map.Package.Name}/{f}"))
				.Where(f => mapFiles.Add(f.Location))
				.SelectMany(f => MiniYaml.FromStream(map.Open(f.File), f.Location))
				.Concat(definitions.Nodes);
		}

		static void CheckTemplates(Action<string> emitWarning, List<MiniYamlNode> nodes)
		{
			var inherited = nodes.SelectMany(GetInherited).ToHashSet();
			foreach (var template in nodes.Where(IsTemplate))
				if (!inherited.Contains(template.Key))
					emitWarning($"{template.Location} defines template `{template.Key}` that is never inherited.");
		}

		static bool IsTemplate(MiniYamlNode node)
		{
			return node.Key.StartsWith(ActorInfo.AbstractActorPrefix);
		}

		static IEnumerable<string> GetInherited(MiniYamlNode node)
		{
			var inherited = node.Value.Nodes.SelectMany(GetInherited);
			if (node.Key == "Inherits" || node.Key.StartsWith("Inherits@", StringComparison.Ordinal))
				return inherited.Append(node.Value.Value);

			return inherited;
		}

		// Weapons that can only be fired by other unused weapons are unused as well.
		static HashSet<string> GetUsedWeapons(Ruleset rules)
		{
			var used = new HashSet<string>();
			var pending = new Stack<string>(rules.Actors.Values
				.SelectMany(a => a.TraitInfos<TraitInfo>())
				.SelectMany(GetWeaponReferences));

			while (pending.TryPop(out var name))
			{
				if (!used.Add(name) || !rules.Weapons.TryGetValue(name, out var weapon))
					continue;

				foreach (var reference in weapon.Warheads.Cast<object>().Append(weapon.Projectile).SelectMany(GetWeaponReferences))
					pending.Push(reference);
			}

			return used;
		}

		static IEnumerable<string> GetWeaponReferences(object info)
		{
			if (info == null)
				return [];

			return Utility.GetFields(info.GetType())
				.Where(Utility.HasAttribute<WeaponReferenceAttribute>)
				.SelectMany(f => LintExts.GetFieldValues(info, f))
				.Where(w => !string.IsNullOrEmpty(w))
				.Select(w => w.ToLowerInvariant());
		}
	}
}
