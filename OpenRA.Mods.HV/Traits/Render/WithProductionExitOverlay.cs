﻿#region Copyright & License Information
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
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits.Render
{
	[Desc("Rendered when an actor finishes production on the used exit cell.")]
	public class WithProductionExitOverlayInfo : TraitInfo, Requires<RenderSpritesInfo>
	{
		[FieldLoader.Require]
		[Desc("Exit offset associated with the animation.")]
		public readonly CVec ExitCell = new CVec(0, 0);

		[SequenceReference]
		[FieldLoader.Require]
		[Desc("Sequence name to use.")]
		public readonly string Sequence = null;

		[PaletteReference("IsPlayerPalette")]
		[Desc("Custom palette name.")]
		public readonly string Palette = null;

		[Desc("Custom palette is a player palette BaseName.")]
		public readonly bool IsPlayerPalette = false;

		public override object Create(ActorInitializer init) { return new WithProductionExitOverlay(init, this); }
	}

	public class WithProductionExitOverlay : INotifyProduction
	{
		readonly WithProductionExitOverlayInfo info;
		readonly Animation overlay;

		bool active;

		public WithProductionExitOverlay(ActorInitializer init, WithProductionExitOverlayInfo info)
		{
			this.info = info;
			var renderSprites = init.Self.Trait<RenderSprites>();

			overlay = new Animation(init.World, renderSprites.GetImage(init.Self));
			renderSprites.Add(new AnimationWithOffset(overlay, null, () => !active), info.Palette, info.IsPlayerPalette);
		}

		void INotifyProduction.UnitProduced(Actor self, Actor other, CPos exit)
		{
			if (info.ExitCell == (exit - self.Location))
			{
				active = true;
				overlay.PlayThen(info.Sequence, () => active = false);
			}
		}
	}
}
