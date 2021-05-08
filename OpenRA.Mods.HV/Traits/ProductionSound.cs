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

using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits
{
	public class ProductionSoundInfo : ConditionalTraitInfo, Requires<RenderSpritesInfo>
	{
		[FieldLoader.Require]
		public readonly string[] Files = null;

		public override object Create(ActorInitializer init) { return new ProductionSound(this); }
	}

	public class ProductionSound : ConditionalTrait<ProductionSoundInfo>, INotifyProduction
	{
		readonly ProductionSoundInfo info;

		public ProductionSound(ProductionSoundInfo info)
			: base(info)
		{
			this.info = info;
		}

		void INotifyProduction.UnitProduced(Actor self, Actor other, CPos exit)
		{
			if (IsTraitDisabled)
				return;

			foreach (var file in info.Files)
				Game.Sound.PlayToPlayer(SoundType.World, self.Owner, file, other.CenterPosition);
		}
	}
}
