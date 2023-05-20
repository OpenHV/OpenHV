#region Copyright & License Information
/*
 * Copyright 2019-2023 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("This must be attached to player in order for TeleportNetwork to work.")]
	public class TeleportNetworkManagerInfo : TraitInfo, IRulesetLoaded
	{
		[FieldLoader.Require]
		[Desc("Type of TeleportNetwork that pairs up, in order for it to work.")]
		public string Type;

		public void RulesetLoaded(Ruleset rules, ActorInfo ai)
		{
			var teleporters = rules.Actors.Values.Where(a => a.HasTraitInfo<TeleportNetworkInfo>());
			if (!teleporters.Any())
				throw new YamlException("TeleportNetworkManager without TeleportNetwork actors.");
			if (!teleporters.Any(a => a.TraitInfo<TeleportNetworkInfo>().Type == Type))
				throw new YamlException($"Can't find a TeleportNetwork with Type '{Type}'");
		}

		public override object Create(ActorInitializer init) { return new TeleportNetworkManager(this); }
	}

	public class TeleportNetworkManager
	{
		public readonly string Type;
		public int Count = 0;
		public Actor PrimaryActor = null;

		public TeleportNetworkManager(TeleportNetworkManagerInfo info)
		{
			Type = info.Type;
		}
	}
}
