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
using OpenRA.Mods.Common.Commands;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	public class DebugOffsetOverlayManagerInfo : ITraitInfo
	{
		[Desc("The font used to draw cell vectors. Should match the value as-is in the Fonts section of the mod manifest (do not convert to lowercase).")]
		public readonly string Font = "TinyBold";

		object ITraitInfo.Create(ActorInitializer init) { return new DebugOffsetOverlayManager(init.Self, this); }
	}

	public class DebugOffsetOverlayManager : IWorldLoaded, IChatCommand
	{
		const string CommandName = "offset";
		const string CommandHelp = "Commands the DebugOffsetOverlay trait. See the trait documentation for controls.";

		readonly Actor self;

		public DebugOffsetOverlayManager(Actor self, DebugOffsetOverlayManagerInfo info)
		{
			this.self = self;
		}

		void IWorldLoaded.WorldLoaded(World w, WorldRenderer wr)
		{
			var console = self.TraitOrDefault<ChatCommands>();
			var help = self.TraitOrDefault<HelpCommand>();

			if (console == null || help == null)
				return;

			console.RegisterCommand(CommandName, this);
			help.RegisterHelp(CommandName, CommandHelp);
		}

		void IChatCommand.InvokeCommand(string command, string arg)
		{
			if (command != CommandName)
				return;

			foreach (var actor in self.World.Selection.Actors)
			{
				if (actor.IsDead)
					continue;

				var devOffset = actor.TraitOrDefault<DebugOffsetOverlay>();
				if (devOffset == null)
					continue;

				devOffset.ParseCommand(actor, arg);
			}
		}
	}
}
