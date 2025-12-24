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

using System.Collections.Immutable;
using OpenRA.Mods.Common.Traits.Render;

namespace OpenRA.Mods.HV.Traits.Render
{
	[Desc("Picks sprites from a random actor.")]
	public class WithRandomSpriteBodyInfo : WithSpriteBodyInfo
	{
		[FieldLoader.Require]
		[Desc("The sequence names that define the actor sprites.")]
		public readonly ImmutableArray<string> Images = default;

		public override object Create(ActorInitializer init) { return new WithRandomSpriteBody(init, this); }
	}

	public class WithRandomSpriteBody : WithSpriteBody
	{
		public WithRandomSpriteBody(ActorInitializer init, WithRandomSpriteBodyInfo info)
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
