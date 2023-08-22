#region Copyright & License Information
/*
 * Copyright 2023 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;
using OpenRA.Graphics;
using OpenRA.Mods.Common.LoadScreens;
using OpenRA.Primitives;

namespace OpenRA.Mods.HV.LoadScreens
{
	public sealed class PanelLoadScreen : SheetLoadScreen
	{
		float2 panelPosition;
		Sprite panel;

		Sheet lastSheet;
		int lastDensity;
		Size lastResolution;

		public override void Init(ModData modData, Dictionary<string, string> info)
		{
			base.Init(modData, info);
		}

		public override void DisplayInner(Renderer r, Sheet s, int density)
		{
			if (s != lastSheet || density != lastDensity)
			{
				lastSheet = s;
				lastDensity = density;
				panel = CreateSprite(s, density, new Rectangle(0, 0, 512, 512));
			}

			if (r.Resolution != lastResolution)
			{
				lastResolution = r.Resolution;
				panelPosition = new float2(lastResolution.Width / 2 - 256, lastResolution.Height / 2 - 256);
			}

			if (panel != null)
				r.RgbaSpriteRenderer.DrawSprite(panel, panelPosition);
		}
	}
}
