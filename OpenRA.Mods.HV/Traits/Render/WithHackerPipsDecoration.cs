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
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	public class WithHackerPipsDecorationInfo : WithDecorationBaseInfo, Requires<HackerInfo>
	{
		[Desc("Number of pips to display. Defaults to " + nameof(HackerInfo.Capacity))]
		public readonly int PipCount = 0;

		[Desc("If non-zero, override the spacing between adjacent pips.")]
		public readonly int2 PipStride = int2.Zero;

		[Desc("Image that defines the pip sequences.")]
		public readonly string Image = "pips";

		[SequenceReference(nameof(Image))]
		[Desc("Sequence used for indicating unused hack slots.")]
		public readonly string EmptySequence = "pip-empty";

		[SequenceReference(nameof(Image))]
		[Desc("Sequence used for indicating hackable units.")]
		public readonly string FullSequence = "pip-green";

		[PaletteReference]
		public readonly string Palette = "chrome";

		public override object Create(ActorInitializer init) { return new WithHackerPipsDecoration(init.Self, this); }
	}

	public class WithHackerPipsDecoration : WithDecorationBase<WithHackerPipsDecorationInfo>
	{
		readonly Hacker hacker;
		readonly Animation pips;
		readonly int pipCount;

		public WithHackerPipsDecoration(Actor self, WithHackerPipsDecorationInfo info)
			: base(self, info)
		{
			hacker = self.Trait<Hacker>();
			pips = new Animation(self.World, info.Image);
			pipCount = Info.PipCount > 0 ? Info.PipCount : hacker.Info.Capacity;
		}

		protected override IEnumerable<IRenderable> RenderDecoration(Actor self, WorldRenderer wr, int2 screenPos)
		{
			pips.PlayRepeating(Info.EmptySequence);

			var palette = wr.Palette(Info.Palette);
			var pipSize = pips.Image.Size.XY.ToInt2();
			var pipStride = Info.PipStride != int2.Zero ? Info.PipStride : new int2(pipSize.X, 0);

			screenPos -= pipSize / 2;

			var victimCount = hacker.Victims.Count();
			for (var i = 0; i < pipCount; i++)
			{
				var sequence = i < victimCount ? Info.FullSequence : Info.EmptySequence;
				pips.PlayRepeating(sequence);
				yield return new UISpriteRenderable(pips.Image, self.CenterPosition, screenPos, 0, palette, 1f);

				screenPos += pipStride;
			}
		}
	}
}
