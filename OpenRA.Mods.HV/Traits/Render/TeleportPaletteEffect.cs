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
using OpenRA.Graphics;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	[TraitLocation(SystemActors.World | SystemActors.EditorWorld)]
	[Desc("Apply palette full screen rotations during teleports. Add this to the world actor.")]
	public class TeleportPaletteEffectInfo : TraitInfo
	{
		[Desc("Measured in ticks.")]
		public readonly int EffectLength = 60;

		public override object Create(ActorInitializer init) { return new TeleportPaletteEffect(this); }
	}

	public class TeleportPaletteEffect : IPaletteModifier, ITick
	{
		readonly TeleportPaletteEffectInfo info;
		int remainingFrames;

		public TeleportPaletteEffect(TeleportPaletteEffectInfo info)
		{
			this.info = info;
		}

		public void Enable()
		{
			remainingFrames = info.EffectLength;
		}

		void ITick.Tick(Actor self)
		{
			if (remainingFrames > 0)
				remainingFrames--;
		}

		void IPaletteModifier.AdjustPalette(IReadOnlyDictionary<string, MutablePalette> palettes)
		{
			if (remainingFrames == 0)
				return;

			var frac = (float)remainingFrames / info.EffectLength;

			foreach (var pal in palettes)
			{
				for (var x = 0; x < Palette.Size; x++)
				{
					var orig = pal.Value.GetColor(x);
					var lum = (int)(255 * orig.GetBrightness());
					var desat = Color.FromArgb(orig.A, lum, lum, lum);
					pal.Value.SetColor(x, Exts.ColorLerp(frac, orig, desat));
				}
			}
		}
	}
}
