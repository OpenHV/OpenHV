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

using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.HV.Traits.Render
{
	class WithSeaMonsterBodyInfo : WithSpriteBodyInfo, Requires<BodyOrientationInfo>, Requires<IFacingInfo>
	{
		[SequenceReference]
		public readonly string LeftSequence = "left";

		[SequenceReference]
		public readonly string RightSequence = "right";

		public override object Create(ActorInitializer init) { return new WithSeaMonsterBody(init, this); }
	}

	class WithSeaMonsterBody : WithSpriteBody, ITick
	{
		readonly WithSeaMonsterBodyInfo info;
		readonly RenderSprites rs;
		readonly IFacing facing;

		public WithSeaMonsterBody(ActorInitializer init, WithSeaMonsterBodyInfo info)
			: base(init, info)
		{
			this.info = info;
			rs = init.Self.Trait<RenderSprites>();
			facing = init.Self.Trait<IFacing>();
			var name = rs.GetImage(init.Self);
		}

		protected override void TraitEnabled(Actor self)
		{
			base.TraitEnabled(self);
		}

		void ITick.Tick(Actor self)
		{
			if (facing.Facing <= 128)
			{
				var left = NormalizeSequence(self, info.LeftSequence);
				if (DefaultAnimation.CurrentSequence.Name != left)
					DefaultAnimation.ReplaceAnim(left);
			}
			else
			{
				var right = NormalizeSequence(self, info.RightSequence);
				if (DefaultAnimation.CurrentSequence.Name != right)
					DefaultAnimation.ReplaceAnim(right);
			}
		}
	}
}
