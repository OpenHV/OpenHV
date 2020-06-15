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

using System.Collections.Generic;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Commands;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Displays terrain types. Excludes custom terrain.")]
	class TerrainDebugOverlayInfo : TraitInfo
	{
		public readonly string Font = "TinyBold";

		public override object Create(ActorInitializer init) { return new TerrainDebugOverlay(this); }
	}

	class TerrainDebugOverlay : IWorldLoaded, IChatCommand, IRenderAnnotations
	{
		const string CommandName = "debugterrain";
		const string CommandDesc = "Toggles the terrain debug overlay.";

		public bool Enabled;

		readonly SpriteFont font;

		public TerrainDebugOverlay(TerrainDebugOverlayInfo info)
		{
			font = Game.Renderer.Fonts[info.Font];
		}

		void IWorldLoaded.WorldLoaded(World w, WorldRenderer wr)
		{
			var console = w.WorldActor.TraitOrDefault<ChatCommands>();
			var help = w.WorldActor.TraitOrDefault<HelpCommand>();

			if (console == null || help == null)
				return;

			console.RegisterCommand(CommandName, this);
			help.RegisterHelp(CommandName, CommandDesc);
		}

		void IChatCommand.InvokeCommand(string name, string arg)
		{
			if (name == CommandName)
				Enabled ^= true;
		}

		IEnumerable<IRenderable> IRenderAnnotations.RenderAnnotations(Actor self, WorldRenderer wr)
		{
			if (!Enabled)
				yield break;

			foreach (var uv in wr.Viewport.VisibleCellsInsideBounds.CandidateMapCoords)
			{
				var cell = uv.ToCPos(wr.World.Map);
				var center = wr.World.Map.CenterOfCell(cell);
				var terrainType = self.World.Map.CustomTerrain[cell];
				if (terrainType != byte.MaxValue)
					continue;

				var info = wr.World.Map.GetTerrainInfo(cell);

				yield return new TextAnnotationRenderable(font, center, 0, info.Color, info.Type);
			}
		}

		bool IRenderAnnotations.SpatiallyPartitionable { get { return false; } }
	}
}
