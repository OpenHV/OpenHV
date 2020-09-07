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

using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits.Render
{
	[Desc("Renders an overlay when the actor is taking heavy damage.")]
	public class WithDamagedOverlayInfo : TraitInfo, Requires<RenderSpritesInfo>
	{
		[Desc("Defaults to the actor name.")]
		public readonly string Image = null;

		[SequenceReference("Image")]
		public readonly string Sequence = null;

		[PaletteReference(nameof(IsPlayerPalette))]
		[Desc("Custom palette name.")]
		public readonly string Palette = null;

		[Desc("Custom palette is a player palette BaseName.")]
		public readonly bool IsPlayerPalette = false;

		[Desc("Damage types that this should be used for (defined on the warheads).",
			"Leave empty to disable all filtering.")]
		public readonly BitSet<DamageType> DamageTypes = default(BitSet<DamageType>);

		[Desc("Trigger when Undamaged, Light, Medium, Heavy, Critical or Dead.")]
		public readonly DamageState MinimumDamageState = DamageState.Heavy;
		public readonly DamageState MaximumDamageState = DamageState.Dead;

		public override object Create(ActorInitializer init) { return new WithDamagedOverlay(init.Self, this); }
	}

	public class WithDamagedOverlay : INotifyDamage
	{
		readonly WithDamagedOverlayInfo info;
		readonly Animation anim;

		bool isPlaying;

		public WithDamagedOverlay(Actor self, WithDamagedOverlayInfo info)
		{
			this.info = info;

			var renderSprites = self.Trait<RenderSprites>();

			anim = new Animation(self.World, info.Image ?? renderSprites.GetImage(self));
			anim.IsDecoration = true;
			renderSprites.Add(new AnimationWithOffset(anim, null, () => !isPlaying),
				info.Palette, info.IsPlayerPalette);
		}

		void INotifyDamage.Damaged(Actor self, AttackInfo e)
		{
			if (!info.DamageTypes.IsEmpty && !e.Damage.DamageTypes.Overlaps(info.DamageTypes))
				return;

			if (isPlaying)
				return;

			if (e.DamageState < info.MinimumDamageState)
				return;

			if (e.DamageState > info.MaximumDamageState)
				return;

			isPlaying = true;
			anim.PlayThen(info.Sequence, () => isPlaying = false);
		}
	}
}
