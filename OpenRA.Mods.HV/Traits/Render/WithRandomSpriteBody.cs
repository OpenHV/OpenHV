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

using System.Linq;
using OpenRA.Mods.Common.Traits.Render;

namespace OpenRA.Mods.HV.Traits.Render
{
	[Desc("Picks sprites from a random actor.")]
	class WithRandomFacingSpriteBodyInfo : WithFacingSpriteBodyInfo
	{
		[FieldLoader.Require]
		[Desc("The sequence names that define the actor sprites.")]
		public readonly string[] Images = null;

		public override object Create(ActorInitializer init) { return new WithRandomFacingSpriteBody(init, this); }
	}

	class WithRandomFacingSpriteBody : WithFacingSpriteBody
	{
		public WithRandomFacingSpriteBody(ActorInitializer init, WithRandomFacingSpriteBodyInfo info)
			: base(init, info)
		{
			var self = init.Self;
			var renderSprites = self.Trait<RenderSprites>();

			var image = info.Images.Random(self.World.SharedRandom);

			var withSpriteBody = self.Info.TraitInfoOrDefault<WithSpriteBodyInfo>();
			if (withSpriteBody != null)
			{
				DefaultAnimation.PlayRepeating(NormalizeSequence(self, withSpriteBody.Sequence));
				DefaultAnimation.ChangeImage(image, withSpriteBody.Sequence);
			}

			renderSprites.UpdatePalette();
		}
	}
}
