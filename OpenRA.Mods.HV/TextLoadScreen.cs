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
using System.Diagnostics;
using OpenRA.Mods.Common.LoadScreens;
using OpenRA.Primitives;

namespace OpenRA.Mods.HV
{
	public sealed class TextLoadScreen : BlankLoadScreen
	{
		readonly string text = "Loading... ";

		Stopwatch lastUpdate;

		public override void Init(ModData modData, Dictionary<string, string> info)
		{
			base.Init(modData, info);
		}

		public override void Display()
		{
			// Limit load screens to at most 5 FPS
			if (Game.Renderer == null || (lastUpdate != null && lastUpdate.Elapsed.TotalSeconds < 0.02))
				return;

			// Start the timer on the first render
			if (lastUpdate == null)
				lastUpdate = Stopwatch.StartNew();

			Game.Renderer.BeginUI();
			DisplayInner(Game.Renderer);
			Game.Renderer.EndFrame(new NullInputHandler());

			lastUpdate.Restart();
		}

		void DisplayInner(Renderer r)
		{
			if (r.Fonts != null)
			{
				var textSize = r.Fonts["Bold"].Measure(text);
				r.Fonts["Bold"].DrawText(text, new float2(r.Resolution.Width - textSize.X - 20, r.Resolution.Height - textSize.Y - 20), Color.White);
			}
		}
	}
}
