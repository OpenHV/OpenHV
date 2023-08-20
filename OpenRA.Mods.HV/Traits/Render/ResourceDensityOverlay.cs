#region Copyright & License Information
/*
 * Copyright 2019-2021 The OpenHV Developers (see CREDITS)
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
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[TraitLocation(SystemActors.World | SystemActors.EditorWorld)]
	[Desc("Displays resource density colored by type.")]
	sealed class ResourceDensityOverlayInfo : TraitInfo
	{
		public readonly string Font = "TinyBold";

		public override object Create(ActorInitializer init) { return new ResourceDensityOverlay(this); }
	}

	sealed class ResourceDensityOverlay : IWorldLoaded, IChatCommand, IRenderAnnotations
	{
		const string CommandName = "resource-density";
		const string CommandDesc = "Toggles the resource debug overlay.";

		public bool Enabled;

		readonly SpriteFont font;

		IResourceLayer resourceLayer;

		public ResourceDensityOverlay(ResourceDensityOverlayInfo info)
		{
			font = Game.Renderer.Fonts[info.Font];
		}

		void IWorldLoaded.WorldLoaded(World w, WorldRenderer wr)
		{
			var console = w.WorldActor.TraitOrDefault<ChatCommands>();
			var help = w.WorldActor.TraitOrDefault<HelpCommand>();
			resourceLayer = w.WorldActor.TraitOrDefault<IResourceLayer>();

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
				var density = resourceLayer.GetResource(cell).Density;
				if (density <= 0)
					continue;

				var info = wr.World.Map.GetTerrainInfo(cell);

				yield return new TextAnnotationRenderable(font, center, 0, info.Color, density.ToString());
			}
		}

		bool IRenderAnnotations.SpatiallyPartitionable { get { return false; } }
	}
}
