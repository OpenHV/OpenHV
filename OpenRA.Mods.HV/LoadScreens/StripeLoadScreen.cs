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
using OpenRA.Mods.Common.Widgets;
using OpenRA.Primitives;

namespace OpenRA.Mods.HV.LoadScreens
{
	public sealed class StripeLoadScreen : SheetLoadScreen
	{
		Rectangle stripeRect;
		Sprite stripe;

		Sheet lastSheet;
		int lastDensity;
		Size lastResolution;

		readonly string[] messages = { "Loading..." };

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
				stripe = CreateSprite(s, density, new(0, 0, 256, 256));
			}

			if (r.Resolution != lastResolution)
			{
				lastResolution = r.Resolution;
				stripeRect = new Rectangle(0, lastResolution.Height / 2 - 128, lastResolution.Width, 256);
			}

			if (stripe != null)
				WidgetUtils.FillRectWithSprite(stripeRect, stripe);

			if (r.Fonts != null)
			{
				var text = messages.Random(Game.CosmeticRandom);
				var textSize = r.Fonts["Bold"].Measure(text);
				r.Fonts["Bold"].DrawText(text, new float2(r.Resolution.Width - textSize.X - 20, r.Resolution.Height - textSize.Y - 20), Color.White);
			}
		}
	}
}
