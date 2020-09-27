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
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	public enum RGBReorderMode
	{
		BGRA,
		RBGA,
		BRGA,
		GBRA,
		GRBA
	}

	[Desc("Create a palette by reordering the channels of another palette.")]
	class PaletteFromPaletteWithRGBReorderedInfo : TraitInfo
	{
		[Desc("Order of the channels in the new palette. The alpha channel can't be reordered. Available modes: BGRA, RBGA, BRGA, GBRA and GRBA.")]
		public readonly RGBReorderMode ReorderMode = RGBReorderMode.BGRA;

		[PaletteDefinition]
		[FieldLoader.Require]
		[Desc("Internal palette name")]
		public readonly string Name = null;

		[PaletteReference]
		[FieldLoader.Require]
		[Desc("The name of the palette to base off.")]
		public readonly string BasePalette = null;

		[Desc("Allow palette modifiers to change the palette.")]
		public readonly bool AllowModifiers = true;

		public override object Create(ActorInitializer init) { return new PaletteFromPaletteWithRGBReordered(this); }
	}

	class PaletteFromPaletteWithRGBReordered : ILoadsPalettes, IProvidesAssetBrowserPalettes
	{
		readonly PaletteFromPaletteWithRGBReorderedInfo info;

		public PaletteFromPaletteWithRGBReordered(PaletteFromPaletteWithRGBReorderedInfo info) { this.info = info; }

		public void LoadPalettes(WorldRenderer wr)
		{
			var remap = new RGBRemap(info.ReorderMode);
			wr.AddPalette(info.Name, new ImmutablePalette(wr.Palette(info.BasePalette).Palette, remap), info.AllowModifiers);
		}

		public IEnumerable<string> PaletteNames { get { yield return info.Name; } }
	}

	class RGBRemap : IPaletteRemap
	{
		readonly RGBReorderMode mode;

		public RGBRemap(RGBReorderMode mode)
		{
			this.mode = mode;
		}

		public Color GetRemappedColor(Color original, int index)
		{
			switch (mode)
			{
				case RGBReorderMode.BGRA:
					return Color.FromArgb(original.A, original.B, original.G, original.R);
				case RGBReorderMode.RBGA:
					return Color.FromArgb(original.A, original.R, original.B, original.G);
				case RGBReorderMode.BRGA:
					return Color.FromArgb(original.A, original.B, original.R, original.G);
				case RGBReorderMode.GBRA:
					return Color.FromArgb(original.A, original.G, original.B, original.R);
				case RGBReorderMode.GRBA:
				default:
					return Color.FromArgb(original.A, original.G, original.R, original.B);
			}
		}
	}
}
