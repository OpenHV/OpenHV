#region Copyright & License Information
/*
 * Copyright 2024 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[Desc("Grants a prerequisite for human only games.")]
	public class TechPrerequisiteWithBotsInfo : TraitInfo, ITechTreePrerequisiteInfo
	{
		[FieldLoader.Require]
		[Desc("The prerequisite type that this provides.")]
		public readonly string Prerequisite = null;

		IEnumerable<string> ITechTreePrerequisiteInfo.Prerequisites(ActorInfo info)
		{
			yield return Prerequisite;
		}

		public override object Create(ActorInitializer init) { return new TechPrerequisiteWithBots(init.Self, this); }
	}

	public class TechPrerequisiteWithBots : ITechTreePrerequisite, INotifyCreated
	{
		readonly string[] prerequisites;

		bool botGame;

		public TechPrerequisiteWithBots(Actor self, TechPrerequisiteWithBotsInfo info)
		{
			prerequisites = new[] { info.Prerequisite };
		}

		void INotifyCreated.Created(Actor self)
		{
			botGame = self.World.Players.Any(p => p.IsBot);
		}

		IEnumerable<string> ITechTreePrerequisite.ProvidesPrerequisites => botGame ? prerequisites : Enumerable.Empty<string>();
	}
}
