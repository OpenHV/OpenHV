#region Copyright & License Information
/*
 * Copyright 2021 The OpenHV Developers (see CREDITS)
 * This file is part of OpenHV, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits.Render
{
	public class WithMissileSpriteBodyInfo : WithSpriteBodyInfo, Requires<BodyOrientationInfo>, Requires<BallisticMissileInfo>
	{
		public override object Create(ActorInitializer init) { return new WithMissileSpriteBody(init, this); }

		public override IEnumerable<IActorPreview> RenderPreviewSprites(ActorPreviewInitializer init, string image, int facings, PaletteReference p)
		{
			if (!EnabledByDefault)
				yield break;

			var anim = new Animation(init.World, image, init.GetFacing());
			anim.PlayRepeating(RenderSprites.NormalizeSequence(anim, init.GetDamageState(), Sequence));

			yield return new SpriteActorPreview(anim, () => WVec.Zero, () => 0, p);
		}
	}

	public class WithMissileSpriteBody : WithSpriteBody
	{
		public static Func<WAngle> MakeFacingFunc(Actor self)
		{
			var missile = self.Trait<BallisticMissile>();
			return () => new WVec(missile.MoveDirection.X, missile.MoveDirection.Y - missile.MoveDirection.Z, 0).Yaw;
		}

		public WithMissileSpriteBody(ActorInitializer init, WithMissileSpriteBodyInfo info)
			: base(init, info, MakeFacingFunc(init.Self)) { }
	}
}
