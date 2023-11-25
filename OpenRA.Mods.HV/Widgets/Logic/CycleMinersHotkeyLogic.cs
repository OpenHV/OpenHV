#region Copyright & License Information
/*
 * Copyright 2022 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Lint;
using OpenRA.Mods.Common.Widgets.Logic;
using OpenRA.Mods.HV.Traits;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.HV.Widgets.Logic.Ingame
{
	[ChromeLogicArgsHotkeys("CycleMinersKey")]
	public class CycleMinersHotkeyLogic : SingleHotkeyBaseLogic
	{
		readonly Viewport viewport;
		readonly ISelection selection;
		readonly World world;

		[ObjectCreator.UseCtor]
		public CycleMinersHotkeyLogic(Widget widget, ModData modData, WorldRenderer worldRenderer, World world, Dictionary<string, MiniYaml> logicArgs)
			: base(widget, modData, "CycleMinersKey", "WORLD_KEYHANDLER", logicArgs)
		{
			viewport = worldRenderer.Viewport;
			selection = world.Selection;
			this.world = world;
		}

		protected override bool OnHotkeyActivated(KeyInput e)
		{
			var player = world.RenderPlayer ?? world.LocalPlayer;

			var miners = world.ActorsHavingTrait<Miner>()
				.Where(a => a.IsInWorld && a.Owner == player)
				.ToList();

			if (miners.Count < 1)
				return true;

			var next = miners
				.SkipWhile(b => !selection.Contains(b))
				.Skip(1)
				.FirstOrDefault();

			next ??= miners[0];

			selection.Combine(world, new Actor[] { next }, false, true);
			viewport.Center(selection.Actors);

			return true;
		}
	}
}
